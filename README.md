# MCP Project Manager - Azure Functions

Sistema de gestión de proyectos usando **Model Context Protocol (MCP)** desplegado en Azure Functions (.NET 8).

## 🎯 Descripción

Servidor MCP **puro** que permite a Claude Desktop consultar y gestionar proyectos, tareas y equipos mediante lenguaje natural. 

**Arquitectura MCP Pura:** Un único endpoint HTTP (`/api/mcp`) maneja todo el protocolo JSON-RPC 2.0.

## ✨ Características

- **17 herramientas MCP** (3 worker + 14 project tools)
- **Azure Functions** con .NET 8 Isolated Worker
- **Node.js bridge** (stdio ↔ HTTP)
- **Datos demo** realistas: 5 proyectos, 15 tareas, 8 trabajadores ($1.4M presupuesto)
- **Desplegado en Azure** → `func-mcp-project-manager.azurewebsites.net`
- **Sin frontend** → Interacción 100% conversacional con Claude

## 🏗️ Arquitectura

```
Claude Desktop
    ↓ stdio (stdin/stdout)
mcp-bridge.js
    ↓ HTTPS POST /api/mcp (JSON-RPC 2.0)
McpServerFunction.cs (ÚNICO ENDPOINT)
    ↓ Switch case tools
WorkerTools.cs + ProjectTools.cs
    ↓ Repository Pattern
WorkerRepository.cs + ProjectRepository.cs (in-memory)
```

**Ver documentación completa:** [`ARQUITECTURA_MCP.md`](./ARQUITECTURA_MCP.md)

## 📁 Estructura del Proyecto

```
mcp-azfunction/
├── McpServer/
│   ├── McpServerFunction.cs     # ⭐ ÚNICO HTTP endpoint (/api/mcp)
│   └── Models/McpModels.cs      # JSON-RPC models
├── Functions/
│   ├── WorkerTools.cs           # 3 herramientas MCP
│   ├── ProjectTools.cs          # 14 herramientas MCP
│   └── HealthFunction.cs        # Health check
├── Data/
│   ├── WorkerRepository.cs      # CRUD workers (in-memory)
│   └── ProjectRepository.cs     # CRUD projects (in-memory)
├── Models/
│   ├── Worker.cs                # Entidad Trabajador
│   ├── Project.cs               # Entidad Proyecto
│   └── ProjectTask.cs           # Entidad Tarea
├── mcp-bridge.js                # Bridge Node.js (stdio → HTTP)
└── mcp-bridge-config.json       # Configuración endpoint
```

## 🛠️ Herramientas MCP Disponibles (17 total)

### Worker Tools (3)
- `get_worker_by_id` - Información detallada de un trabajador
- `get_all_workers` - Lista completa del equipo
- `search_workers_by_name` - Búsqueda por nombre

### Project Tools (11)
- `get_all_projects` - Todos los proyectos
- `get_project_by_id` - Detalle de un proyecto
- `get_projects_by_status` - Filtrar por estado (Planning, InProgress, OnHold, Completed)
- `get_projects_by_manager` - Proyectos de un PM específico
- `search_projects` - Búsqueda por nombre
- `get_tasks_by_project` - Tareas de un proyecto
- `get_tasks_by_worker` - Tareas asignadas a un trabajador
- `get_task_by_id` - Detalle de una tarea
- `get_tasks_by_status` - Filtrar tareas por estado
- `get_project_statistics` - Estadísticas globales del sistema
- `get_team_workload` - Carga de trabajo por miembro

### Team Management Tools (3)
- `assign_worker_to_project` - Asignar trabajador por IDs
- `remove_worker_from_project` - Quitar trabajador de proyecto
- `assign_worker_by_name` - **Natural language:** "Añade a Javier al proyecto de migración"
- `get_team_workload` - Carga de trabajo del equipo

## Quick Start

### Usar el servidor desplegado en Azure

```powershell
# 1. Instalar dependencias
npm install

# 2. Configurar Claude Desktop
# Editar: %APPDATA%\Claude\claude_desktop_config.json
# Copiar contenido de: claude-desktop-config.example.json

# 3. Reiniciar Claude Desktop
```

Ya puedes preguntar: *"¿Cuántos proyectos tenemos en progreso?"*

### Desarrollo local

```powershell
# 1. Iniciar servidor
func start

# 2. Probar herramientas
.\test-project-manager.ps1

# 3. Configurar bridge para local
# Editar mcp-bridge-config.json: "http://localhost:7073/api/mcp"
```

## Endpoints

### Local
- Health: `http://localhost:7073/api/health`
- MCP: `http://localhost:7073/api/mcp`

### Azure
- Health: `https://func-mcp-project-manager.azurewebsites.net/api/health`
- MCP: `https://func-mcp-project-manager.azurewebsites.net/api/mcp`

## Proyectos Demo

1. **Migración a Cloud Azure** - €250K (En Progreso)
2. **Renovación Portal Web** - €120K (En Progreso)
3. **Campaña Marketing Q2** - €85K (Planificación)
4. **Implementación ERP Cloud** - €450K (En Progreso)
5. **Expansión Mercado LATAM** - €500K (En Espera)

## Ejemplos de Uso

```
Usuario: ¿Cuántos proyectos tenemos en progreso?
Claude: Tenemos 3 proyectos en progreso con un presupuesto total de €820,000

Usuario: ¿Quién tiene más tareas asignadas?
Claude: Jorge Ramírez Castro tiene la mayor carga con 5 tareas (20 horas estimadas)

Usuario: ¿Hay tareas bloqueadas?
Claude: Sí, hay 1 tarea bloqueada en el proyecto ERP Cloud esperando migración de datos
```

## Despliegue en Azure

```powershell
# Ejecutar script de despliegue automático
.\deploy-azure-simple.ps1
```

Crea automáticamente:
- Resource Group: `rg-mcp-project-manager`
- Storage Account: `stmcpprojectmgr`
- Function App: `func-mcp-project-manager`

## Documentación Adicional

- **QUICK_START.md** - Guía de inicio en 5 minutos
- **DEMO_SCRIPT.md** - Script de presentación profesional
- **ARCHITECTURE.md** - Arquitectura técnica detallada
- **EXAMPLE_CONVERSATIONS.md** - Ejemplos de conversaciones
- **CLAUDE_SETUP.md** - Configuración de Claude Desktop
- **DEPLOY_AZURE.md** - Guía de despliegue completa

## Tecnologías

- **.NET 8** - Runtime de Azure Functions
- **Azure Functions v4** - Isolated Worker Model
- **Model Context Protocol** - MCP 2024-11-05
- **Node.js** - Bridge stdio ↔ HTTP
- **Claude Desktop** - Cliente MCP

## Requisitos

- .NET 8 SDK
- Azure Functions Core Tools v4
- Node.js (para el bridge)
- Claude Desktop
- Azure Subscription (para despliegue)

## Licencia

MIT
