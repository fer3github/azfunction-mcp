# 🏗️ Arquitectura del Sistema - Project Manager MCP

## 📐 Diagrama de Arquitectura

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLAUDE DESKTOP                            │
│  (Usuario interactúa con lenguaje natural)                      │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         │ stdio (JSON-RPC)
                         ↓
┌─────────────────────────────────────────────────────────────────┐
│                     mcp-bridge.js (Node.js)                      │
│  Convierte stdio ↔ HTTP                                         │
│  Lee mcp-bridge-config.json                                     │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         │ HTTP POST
                         │ (JSON-RPC 2.0)
                         ↓
┌─────────────────────────────────────────────────────────────────┐
│              AZURE FUNCTIONS (localhost:7073)                    │
│                                                                  │
│  ┌────────────────────────────────────────────────────────┐    │
│  │  McpServerFunction (POST /api/mcp)                     │    │
│  │  - Maneja protocolo MCP                                 │    │
│  │  - Enruta llamadas a herramientas                       │    │
│  │  - Formatea respuestas                                  │    │
│  └──────────────┬──────────────────────┬───────────────────┘    │
│                 │                      │                         │
│                 ↓                      ↓                         │
│  ┌─────────────────────┐  ┌──────────────────────────┐         │
│  │   WorkerTools       │  │    ProjectTools           │         │
│  │  3 herramientas     │  │  11 herramientas          │         │
│  │  - get_worker_by_id │  │  - get_all_projects       │         │
│  │  - get_all_workers  │  │  - get_project_by_id      │         │
│  │  - search_workers   │  │  - get_tasks_by_worker    │         │
│  └─────────┬───────────┘  │  - get_project_statistics │         │
│            │              │  - etc...                  │         │
│            ↓              └──────────┬─────────────────┘         │
│  ┌─────────────────────┐            ↓                           │
│  │  WorkerRepository   │  ┌──────────────────────────┐         │
│  │  8 trabajadores     │  │   ProjectRepository       │         │
│  │  hardcoded          │  │  5 proyectos + 15 tareas  │         │
│  └─────────────────────┘  │  hardcoded                │         │
│                            └──────────────────────────┘         │
│                                                                  │
│  ┌────────────────────────────────────────────────────────┐    │
│  │  HealthFunction (GET /api/health)                       │    │
│  │  - Verifica que el servidor esté funcionando            │    │
│  └────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
```

## 🔄 Flujo de una Llamada MCP

### Ejemplo: "Muéstrame todos los proyectos"

```
1️⃣ Usuario en Claude Desktop
   "Muéstrame todos los proyectos"
   
2️⃣ Claude identifica herramienta
   Tool: get_all_projects
   
3️⃣ Claude Desktop → mcp-bridge.js (stdio)
   {
     "jsonrpc": "2.0",
     "id": 1,
     "method": "tools/call",
     "params": {
       "name": "get_all_projects",
       "arguments": {}
     }
   }
   
4️⃣ mcp-bridge.js → Azure Function (HTTP)
   POST http://localhost:7073/api/mcp
   Content-Type: application/json
   [mismo JSON]
   
5️⃣ McpServerFunction
   - Recibe request
   - Identifica method: "tools/call"
   - Extrae tool name: "get_all_projects"
   - Llama a _projectTools.GetAllProjects()
   
6️⃣ ProjectTools.GetAllProjects()
   - Llama a _projectRepository.GetAll()
   - Formatea resultados con emojis y formato
   - Retorna string con información
   
7️⃣ ProjectRepository.GetAll()
   - Retorna List<Project> (5 proyectos)
   
8️⃣ McpServerFunction
   - Empaqueta resultado en respuesta MCP
   {
     "jsonrpc": "2.0",
     "id": 1,
     "result": {
       "content": [{
         "type": "text",
         "text": "📊 **RESUMEN DE PROYECTOS**..."
       }]
     }
   }
   
9️⃣ Azure Function → mcp-bridge.js (HTTP Response)
   
🔟 mcp-bridge.js → Claude Desktop (stdio)
   
1️⃣1️⃣ Claude Desktop
   - Procesa respuesta
   - Muestra al usuario en formato legible
   - Puede hacer análisis adicional
```

## 📦 Componentes del Sistema

### 1. Frontend (Claude Desktop)
- **Rol:** Interfaz de usuario conversacional
- **Tecnología:** Aplicación Electron
- **Responsabilidades:**
  - Recibir instrucciones en lenguaje natural
  - Identificar herramientas MCP necesarias
  - Mostrar resultados al usuario
  - Hacer análisis y recomendaciones

### 2. Bridge (mcp-bridge.js)
- **Rol:** Adaptador de protocolos
- **Tecnología:** Node.js
- **Responsabilidades:**
  - Convertir stdio ↔ HTTP
  - Gestionar configuración (local vs Azure)
  - Mantener sesión persistente

### 3. Backend (Azure Functions)
- **Rol:** Servidor MCP y lógica de negocio
- **Tecnología:** .NET 8, Azure Functions v4
- **Componentes:**

#### a) McpServerFunction
- Implementa protocolo MCP 2024-11-05
- Maneja: initialize, tools/list, tools/call
- Enruta a las herramientas correctas

#### b) WorkerTools
- 3 herramientas para gestión de equipo
- Accede a WorkerRepository

#### c) ProjectTools
- 11 herramientas para gestión de proyectos
- Accede a ProjectRepository y WorkerRepository
- Formatea respuestas con emojis y Markdown

#### d) Repositories
- **WorkerRepository:** 8 trabajadores hardcoded
- **ProjectRepository:** 5 proyectos, 15 tareas hardcoded
- Encapsula lógica de acceso a datos

### 4. Datos (In-Memory)
- **Trabajadores:** Lista estática de 8 personas
- **Proyectos:** Lista estática de 5 proyectos
- **Tareas:** Embebidas en proyectos
- **Relaciones:** Por IDs (WorkerId, ProjectId, etc.)

## 🔌 Puntos de Integración

### Entrada (Claude Desktop)
```json
// claude_desktop_config.json
{
  "mcpServers": {
    "project-manager": {
      "command": "node",
      "args": ["ruta/a/mcp-bridge.js"]
    }
  }
}
```

### Configuración (Bridge)
```json
// mcp-bridge-config.json
{
  "mcpServer": {
    "protocol": "http",      // o "https" para Azure
    "hostname": "localhost", // o "*.azurewebsites.net"
    "port": 7073,           // o 443 para Azure
    "path": "/api/mcp"
  }
}
```

### API (Azure Functions)
```
GET  /api/health          → HealthCheck
POST /api/mcp             → MCP Server
```

## 🔐 Seguridad

### Actual (Desarrollo)
- **AuthorizationLevel.Anonymous** en funciones
- Sin autenticación en bridge
- Acceso local (localhost)

### Producción (Recomendado)
- Azure AD autenticación
- API Keys en Functions
- CORS configurado específicamente
- HTTPS obligatorio
- Rate limiting

## 📈 Escalabilidad

### Actual
- **Datos:** In-memory (no persiste)
- **Estado:** Stateless (cada llamada independiente)
- **Concurrencia:** Limitada por Azure Functions Consumption Plan

### Mejoras Futuras
- Migrar a Azure SQL Database / Cosmos DB
- Implementar caché (Redis)
- Usar Azure Functions Premium Plan
- Añadir Application Insights para monitoring

## 🎯 Patrones de Diseño Utilizados

1. **Repository Pattern** - Abstracción de acceso a datos
2. **Dependency Injection** - IoC container de .NET
3. **Adapter Pattern** - mcp-bridge.js adapta stdio a HTTP
4. **Strategy Pattern** - Diferentes herramientas MCP
5. **Builder Pattern** - Construcción de respuestas formateadas

## 🔄 Estados del Sistema

```
[Inicialización]
    ↓
[Azure Functions Start]
    ↓
[Registrar Servicios DI]
    ↓
[Cargar Repositorios]
    ↓
[Servidor Escuchando]
    ↓
┌─[Esperar Request]
│   ↓
│ [Recibir Request MCP]
│   ↓
│ [Procesar (initialize/tools/list/tools/call)]
│   ↓
│ [Ejecutar Herramienta]
│   ↓
│ [Formatear Respuesta]
│   ↓
│ [Enviar Response]
│   ↓
└─[Loop]
```

## 📊 Modelo de Datos

```
Worker (8 instancias)
├─ Id: int
├─ Nombre: string
├─ Departamento: string
├─ Puesto: string
├─ Email: string
├─ Telefono: string
└─ Ubicacion: string

Project (5 instancias)
├─ Id: int
├─ Name: string
├─ Description: string
├─ StartDate: DateTime
├─ EndDate: DateTime?
├─ Status: enum (Planning, InProgress, OnHold, Completed, Cancelled)
├─ ProjectManagerId: int → Worker
├─ Priority: string
├─ Budget: decimal
├─ TeamMemberIds: List<int> → Worker
└─ Tasks: List<ProjectTask>

ProjectTask (15 instancias)
├─ Id: int
├─ ProjectId: int → Project
├─ Title: string
├─ Description: string
├─ AssignedToId: int? → Worker
├─ Status: enum (ToDo, InProgress, InReview, Blocked, Completed, Cancelled)
├─ Priority: enum (Low, Medium, High, Critical)
├─ CreatedDate: DateTime
├─ DueDate: DateTime?
├─ CompletedDate: DateTime?
├─ EstimatedHours: int
├─ ActualHours: int
└─ Tags: List<string>
```

## 🚀 Deployment

### Local
```
Developer Machine
├─ dotnet build
├─ func start (puerto 7073)
├─ mcp-bridge.js (stdio ↔ http://localhost:7073)
└─ Claude Desktop
```

### Azure
```
Azure Cloud
├─ Azure Function App (*.azurewebsites.net)
│  ├─ Runtime: .NET 8 Isolated
│  ├─ Plan: Consumption (gratis)
│  └─ HTTPS habilitado
│
Developer Machine
├─ mcp-bridge.js (stdio ↔ https://*.azurewebsites.net)
└─ Claude Desktop
```

---

**Esta arquitectura permite:**
- ✅ Conversación natural con Claude
- ✅ Ejecución de herramientas en Azure
- ✅ Fácil despliegue y escalamiento
- ✅ Separación de responsabilidades
- ✅ Extensibilidad para nuevas features
