# 🏗️ ARQUITECTURA MCP PURA

## 📐 Estructura del Proyecto

```
mcp-azfunction/
├── Program.cs                          # DI Container + Entry Point
├── host.json                           # Azure Functions runtime config
├── local.settings.json                 # Variables de entorno locales
├── mcp-bridge.js                       # Bridge stdio → HTTP
├── mcp-bridge-config.json              # Configuración del bridge
│
├── McpServer/                          # 🎯 CAPA MCP PROTOCOL
│   ├── McpServerFunction.cs            # HTTP endpoint /api/mcp (ÚNICO PUNTO DE ENTRADA)
│   └── Models/
│       └── McpModels.cs                # McpRequest, McpResponse, McpTool, etc.
│
├── Functions/                          # 🔧 CAPA DE LÓGICA DE NEGOCIO
│   ├── WorkerTools.cs                  # 3 herramientas MCP para trabajadores
│   ├── ProjectTools.cs                 # 14 herramientas MCP para proyectos
│   └── HealthFunction.cs               # Health check endpoint
│
├── Data/                               # 💾 CAPA DE DATOS
│   ├── WorkerRepository.cs             # CRUD de trabajadores (in-memory)
│   └── ProjectRepository.cs            # CRUD de proyectos (in-memory)
│
└── Models/                             # 📦 CAPA DE DOMINIO
    ├── Worker.cs                       # Entidad Trabajador
    ├── Project.cs                      # Entidad Proyecto
    └── ProjectTask.cs                  # Entidad Tarea
```

---

## 🔄 FLUJO DE COMUNICACIÓN

### **1. Claude Desktop → Azure Functions**

```
Usuario: "Muéstrame todos los proyectos"
    ↓
[Claude Desktop]
    ↓ stdio (stdin/stdout)
[mcp-bridge.js]                 # Convierte stdio → HTTP
    ↓ POST https://tu-funcion.azurewebsites.net/api/mcp
    {
      "jsonrpc": "2.0",
      "id": 1,
      "method": "tools/call",
      "params": {
        "name": "get_all_projects",
        "arguments": {}
      }
    }
    ↓
[McpServerFunction.cs]          # ÚNICO HTTP endpoint
    ↓ HandleToolsCall()
    ↓ Switch case: "get_all_projects"
    ↓ Llama a _projectTools.GetAllProjects()
    ↓
[ProjectTools.cs]               # Lógica de negocio
    ↓ Llama a _projectRepository.GetAll()
    ↓
[ProjectRepository.cs]          # Acceso a datos
    ↓ Devuelve List<Project> (in-memory)
    ↓
[ProjectTools.cs]               # Formatea respuesta bonita
    ↓ Devuelve string formateado con emojis
    ↓
[McpServerFunction.cs]          # Envuelve en JSON-RPC
    ↓ HTTP 200 OK
    {
      "jsonrpc": "2.0",
      "id": 1,
      "result": {
        "content": [
          {
            "type": "text",
            "text": "📁 PROYECTOS ACTIVOS (5 total)..."
          }
        ]
      }
    }
    ↓
[mcp-bridge.js]                 # Convierte HTTP → stdio
    ↓ stdout
[Claude Desktop]                # Procesa respuesta
    ↓
Usuario ve: "Tienes 5 proyectos activos: Migración Cloud, Sistema de facturación..."
```

---

## 🎯 PRINCIPIOS DE DISEÑO

### ✅ **1. Separación de Responsabilidades**

```csharp
McpServerFunction.cs     # Solo protocolo JSON-RPC (routing)
    ↓
ProjectTools.cs          # Solo lógica de negocio (formateo, validaciones)
    ↓
ProjectRepository.cs     # Solo acceso a datos (CRUD)
```

**Ventaja:** Cada capa es testeable independientemente.

---

### ✅ **2. Dependency Injection**

**`Program.cs`:**
```csharp
builder.Services.AddSingleton<WorkerRepository>();
builder.Services.AddSingleton<ProjectRepository>();
builder.Services.AddSingleton<WorkerTools>();
builder.Services.AddSingleton<ProjectTools>();
```

**`McpServerFunction.cs`:**
```csharp
public McpServerFunction(
    ILogger<McpServerFunction> logger,
    WorkerTools workerTools,      // ← Inyectado
    ProjectTools projectTools)     // ← Inyectado
{
    _workerTools = workerTools;
    _projectTools = projectTools;
}
```

**Ventaja:** Fácil cambiar implementaciones (ej: de in-memory a SQL).

---

### ✅ **3. Repository Pattern**

```csharp
public class ProjectRepository
{
    private readonly List<Project> _projects;
    
    public List<Project> GetAll() => _projects;
    public Project? GetById(int id) => _projects.FirstOrDefault(p => p.Id == id);
    public List<Project> SearchByName(string name) => 
        _projects.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
}
```

**Ventaja:** Cambiar de in-memory a database solo requiere cambiar `ProjectRepository`.

---

### ✅ **4. Referencias por ID (no objetos)**

```csharp
public class Project
{
    public List<int> TeamMemberIds { get; set; }  // ← Solo IDs
    // NO: public List<Worker> TeamMembers { get; set; }  ❌
}
```

**Ventajas:**
- ✅ Sin referencias circulares
- ✅ Serialización JSON simple
- ✅ Lazy loading manual

---

## 🛠️ HERRAMIENTAS MCP DISPONIBLES

### **Worker Tools (3)**
1. `get_all_workers` - Lista todos los trabajadores
2. `get_worker_by_id` - Obtiene un trabajador por ID
3. `search_workers_by_name` - Busca trabajadores por nombre

### **Project Tools (14)**
1. `get_all_projects` - Lista todos los proyectos
2. `get_project_by_id` - Obtiene un proyecto por ID
3. `get_projects_by_status` - Filtra proyectos por estado
4. `get_projects_by_manager` - Proyectos de un PM específico
5. `search_projects` - Busca proyectos por nombre
6. `get_tasks_by_project` - Tareas de un proyecto
7. `get_tasks_by_worker` - Tareas asignadas a un trabajador
8. `get_task_by_id` - Obtiene una tarea por ID
9. `get_tasks_by_status` - Tareas por estado
10. `get_project_statistics` - Estadísticas del sistema
11. `get_team_workload` - Carga de trabajo del equipo
12. `assign_worker_to_project` - Asignar trabajador a proyecto (por IDs)
13. `remove_worker_from_project` - Quitar trabajador de proyecto (por IDs)
14. `assign_worker_by_name` - Asignar trabajador por nombres (natural language)

---

## 🎨 PATRONES DE CÓDIGO

### **Pattern 1: Tool Routing**

```csharp
string resultText = toolCallParams.Name switch
{
    "get_all_workers" => _workerTools.GetAllWorkers(),
    "get_worker_by_id" => ExecuteGetWorkerById(toolCallParams.Arguments),
    "get_all_projects" => _projectTools.GetAllProjects(),
    _ => $"Error: Herramienta desconocida '{toolCallParams.Name}'"
};
```

---

### **Pattern 2: Argument Extraction**

```csharp
private int ExtractIntArgument(Dictionary<string, object> arguments, string key)
{
    var value = arguments[key];
    
    if (value is JsonElement jsonElement)      // Desde HTTP
        return jsonElement.GetInt32();
    else if (value is int intValue)            // Desde tests
        return intValue;
    else if (int.TryParse(value.ToString(), out int parsed))  // Fallback
        return parsed;
    
    throw new ArgumentException($"'{key}' debe ser un número");
}
```

**Problema que resuelve:** `System.Text.Json` devuelve `JsonElement`, no tipos nativos.

---

### **Pattern 3: Response Formatting**

```csharp
private string FormatProjectSummary(Project project)
{
    var pm = _workerRepository.GetById(project.ProjectManagerId);
    var statusEmoji = GetStatusEmoji(project.Status);
    
    return $@"{statusEmoji} **{project.Name}** (ID: {project.Id})
   └─ Estado: {TranslateStatus(project.Status)} | Prioridad: {project.Priority}
   └─ PM: {pm?.Nombre ?? "No asignado"} | Equipo: {project.TeamMemberIds.Count}
   └─ {project.StartDate:dd/MM/yyyy} → {project.EndDate?.ToString("dd/MM/yyyy") ?? "Sin fecha"}";
}
```

**Características:**
- ✅ Emojis para mejor UX
- ✅ Formato consistente
- ✅ Información relevante condensada

---

## 🔐 PERSISTENCIA DE DATOS

### **Actual: In-Memory con Singleton**

```csharp
// Program.cs
builder.Services.AddSingleton<ProjectRepository>();

// Ventajas:
✅ Simple
✅ Rápido
✅ Cambios persisten entre requests

// Desventajas:
❌ Datos se pierden en restart
❌ No thread-safe sin locks
❌ No funciona en múltiples instancias
```

### **Para Producción: Migrar a Database**

**Opción 1: Azure SQL Database**
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<IProjectRepository, SqlProjectRepository>();
```

**Opción 2: Azure Table Storage**
```csharp
builder.Services.AddSingleton<TableServiceClient>(sp => 
    new TableServiceClient(connectionString));
builder.Services.AddScoped<IProjectRepository, TableStorageProjectRepository>();
```

**Opción 3: Cosmos DB**
```csharp
builder.Services.AddSingleton<CosmosClient>(sp => 
    new CosmosClient(endpoint, authKey));
builder.Services.AddScoped<IProjectRepository, CosmosProjectRepository>();
```

---

## 🚀 DEPLOYMENT

### **Local Development**
```powershell
func start
# Runs at http://localhost:7073
```

### **Azure Deployment**
```powershell
func azure functionapp publish func-mcp-project-manager
```

### **Bridge Configuration**
```json
// mcp-bridge-config.json (local)
{
  "mcpServer": {
    "protocol": "http",
    "hostname": "localhost",
    "port": 7073,
    "path": "/api/mcp"
  }
}

// mcp-bridge-config.json (production)
{
  "mcpServer": {
    "protocol": "https",
    "hostname": "func-mcp-project-manager.azurewebsites.net",
    "port": 443,
    "path": "/api/mcp"
  }
}
```

---

## 🎓 LECCIONES APRENDIDAS

1. **MCP = Un solo endpoint HTTP** → `/api/mcp` maneja todo JSON-RPC
2. **No mezclar REST APIs** → Confunde el propósito del proyecto
3. **Singleton para demo** → Simple pero no production-ready
4. **IDs mejor que objetos** → Evita referencias circulares
5. **JsonElement requires helpers** → Deserialización no es directa
6. **Bridge es crítico** → Claude Desktop solo habla stdio
7. **Separación de capas** → Protocol → Business Logic → Data Access

---

## 📚 PRÓXIMOS PASOS

Para mejorar este proyecto:

1. **Agregar persistencia real** (Azure SQL/Cosmos DB)
2. **Implementar autenticación** (Azure AD, API Keys)
3. **Thread-safety** (ConcurrentDictionary si seguimos in-memory)
4. **Tests unitarios** (xUnit + Moq)
5. **Logging estructurado** (Application Insights)
6. **Rate limiting** (protección contra abuso)
7. **Versionado de herramientas** (backward compatibility)
