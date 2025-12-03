# Script para desplegar Azure Function con MCP Server
# Usa nombres fijos y verifica si los recursos existen
param(
    [string]$ResourceGroupName = "rg-mcp-project-manager",
    [string]$Location = "westeurope",
    [string]$StorageAccountName = "stmcpprojectmgr",
    [string]$FunctionAppName = "func-mcp-project-manager"
)

az account set --subscription "6364e341-b5f0-4259-91ed-bac0249f143e"

Write-Host "`n=== DESPLIEGUE DE AZURE FUNCTION MCP SERVER ===" -ForegroundColor Cyan
Write-Host ""

# Verificar Azure CLI
try {
    az --version | Out-Null
} catch {
    Write-Host "[ERROR] Azure CLI no esta instalado" -ForegroundColor Red
    Write-Host "Instala desde: https://docs.microsoft.com/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

# Verificar login
Write-Host "[1/7] Verificando login en Azure..." -ForegroundColor Yellow
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Host "Iniciando login..." -ForegroundColor Yellow
    az login
    $account = az account show | ConvertFrom-Json
}

Write-Host "[OK] Conectado a: $($account.name)" -ForegroundColor Green
Write-Host ""

# Crear o verificar Resource Group
Write-Host "[2/7] Verificando Resource Group: $ResourceGroupName" -ForegroundColor Yellow
$rgExists = az group exists --name $ResourceGroupName
if ($rgExists -eq "false") {
    Write-Host "    Creando Resource Group..." -ForegroundColor Gray
    az group create --name $ResourceGroupName --location $Location --output none
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Fallo al crear Resource Group" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK] Resource Group creado" -ForegroundColor Green
} else {
    Write-Host "[OK] Resource Group ya existe" -ForegroundColor Green
}

# Crear o verificar Storage Account
Write-Host "[3/7] Verificando Storage Account: $StorageAccountName" -ForegroundColor Yellow
$storageExists = az storage account check-name --name $StorageAccountName --query "nameAvailable" -o tsv 2>$null
if ($storageExists -eq "true") {
    Write-Host "    Creando Storage Account..." -ForegroundColor Gray
    az storage account create `
        --name $StorageAccountName `
        --location $Location `
        --resource-group $ResourceGroupName `
        --sku Standard_LRS `
        --output none
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Fallo al crear Storage Account" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK] Storage Account creado" -ForegroundColor Green
} else {
    Write-Host "[OK] Storage Account ya existe" -ForegroundColor Green
}

# Crear o verificar Function App
Write-Host "[4/7] Verificando Function App: $FunctionAppName" -ForegroundColor Yellow
$functionAppExists = az functionapp show --name $FunctionAppName --resource-group $ResourceGroupName 2>$null
if (-not $functionAppExists) {
    Write-Host "    Creando Function App..." -ForegroundColor Gray
    az functionapp create `
        --resource-group $ResourceGroupName `
        --name $FunctionAppName `
        --storage-account $StorageAccountName `
        --consumption-plan-location $Location `
        --runtime dotnet-isolated `
        --runtime-version 8 `
        --functions-version 4 `
        --output none
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Fallo al crear Function App" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK] Function App creada" -ForegroundColor Green
} else {
    Write-Host "[OK] Function App ya existe" -ForegroundColor Green
}

# Configurar CORS
Write-Host "[5/7] Configurando CORS..." -ForegroundColor Yellow
az functionapp cors add `
    --resource-group $ResourceGroupName `
    --name $FunctionAppName `
    --allowed-origins "*" `
    --output none 2>$null
Write-Host "[OK] CORS configurado" -ForegroundColor Green

# Compilar y desplegar usando Azure Functions Core Tools
Write-Host "[6/7] Compilando proyecto..." -ForegroundColor Yellow
dotnet build mcp-azfunction.csproj --configuration Release --nologo --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Fallo al compilar" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] Proyecto compilado" -ForegroundColor Green

# Desplegar usando func azure functionapp publish
Write-Host "[7/7] Desplegando Function App..." -ForegroundColor Yellow
func azure functionapp publish $FunctionAppName --dotnet-isolated

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Fallo al desplegar" -ForegroundColor Red
    Write-Host "Tip: Verifica que 'func' este instalado (Azure Functions Core Tools)" -ForegroundColor Yellow
    exit 1
}

Write-Host "[OK] Function App desplegada" -ForegroundColor Green
Write-Host "`nEsperando a que la funcion este lista..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Obtener URL
$functionUrl = az functionapp show `
    --resource-group $ResourceGroupName `
    --name $FunctionAppName `
    --query defaultHostName `
    -o tsv

# Resumen
Write-Host "`n=== DESPLIEGUE COMPLETADO ===" -ForegroundColor Green
Write-Host ""
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor White
Write-Host "Function App: $FunctionAppName" -ForegroundColor White
Write-Host "URL: https://$functionUrl" -ForegroundColor White
Write-Host ""
Write-Host "Endpoints:" -ForegroundColor Cyan
Write-Host "  Health: https://$functionUrl/api/health" -ForegroundColor White
Write-Host "  MCP: https://$functionUrl/api/mcp" -ForegroundColor White
Write-Host ""
Write-Host "Configuracion para mcp-bridge-config.json:" -ForegroundColor Cyan
Write-Host @"
{
  "hostname": "$functionUrl",
  "port": 443,
  "path": "/api/mcp",
  "protocol": "https"
}
"@ -ForegroundColor Yellow
Write-Host ""
Write-Host "Probar la funcion:" -ForegroundColor Cyan
Write-Host "  Invoke-RestMethod -Uri 'https://$functionUrl/api/health'" -ForegroundColor White
Write-Host ""
Write-Host "Para eliminar:" -ForegroundColor Cyan
Write-Host "  az group delete --name $ResourceGroupName --yes" -ForegroundColor White
Write-Host ""
