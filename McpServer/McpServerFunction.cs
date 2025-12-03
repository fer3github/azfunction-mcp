using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;
using McpAzFunction.McpServer.Models;
using McpAzFunction.Functions;

namespace McpAzFunction.McpServer;

public class McpServerFunction
{
    private readonly ILogger<McpServerFunction> _logger;
    private readonly WorkerTools _workerTools;
    private readonly ProjectTools _projectTools;

    public McpServerFunction(ILogger<McpServerFunction> logger, WorkerTools workerTools, ProjectTools projectTools)
    {
        _logger = logger;
        _workerTools = workerTools;
        _projectTools = projectTools;
    }

    [Function("McpServer")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp")] HttpRequestData req)
    {
        _logger.LogInformation("MCP Server received request");

        try
        {
            // Read request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation($"Request body: {requestBody}");

            var mcpRequest = JsonSerializer.Deserialize<McpRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (mcpRequest == null)
            {
                return CreateErrorResponse(req, null, -32700, "Parse error");
            }

            _logger.LogInformation($"MCP Method: {mcpRequest.Method}");

            // Handle notifications (no response needed per MCP spec)
            if (mcpRequest.Method?.StartsWith("notifications/") == true)
            {
                _logger.LogInformation($"Received notification: {mcpRequest.Method}");
                var notificationResponse = req.CreateResponse(HttpStatusCode.NoContent);
                return notificationResponse;
            }

            object? result = mcpRequest.Method switch
            {
                "initialize" => HandleInitialize(),
                "tools/list" => HandleToolsList(),
                "tools/call" => await HandleToolsCall(mcpRequest),
                _ => throw new Exception($"Unknown method: {mcpRequest.Method}")
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            var mcpResponse = new McpResponse
            {
                JsonRpc = "2.0",
                Id = mcpRequest.Id ?? 1, // Asegurar que nunca sea null
                Result = result
            };

            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            await response.WriteStringAsync(JsonSerializer.Serialize(mcpResponse, jsonOptions), Encoding.UTF8);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MCP request");
            return CreateErrorResponse(req, null, -32603, $"Internal error: {ex.Message}");
        }
    }

    private McpInitializeResult HandleInitialize()
    {
        _logger.LogInformation("Handling initialize request");
        return new McpInitializeResult
        {
            ProtocolVersion = "2024-11-05",
            Capabilities = new McpCapabilities
            {
                Tools = new { }
            },
            ServerInfo = new McpServerInfo
            {
                Name = "Project Manager MCP Server",
                Version = "2.0.0"
            }
        };
    }

    private McpToolsListResult HandleToolsList()
    {
        _logger.LogInformation("Handling tools/list request");
        
        return new McpToolsListResult
        {
            Tools = new List<McpTool>
            {
                // Worker Tools
                new McpTool
                {
                    Name = "get_worker_by_id",
                    Description = "Obtiene información detallada de un trabajador específico por su ID",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            id = new
                            {
                                type = "number",
                                description = "ID único del trabajador"
                            }
                        },
                        required = new[] { "id" }
                    }
                },
                new McpTool
                {
                    Name = "get_all_workers",
                    Description = "Obtiene la lista completa de todos los trabajadores registrados en el sistema",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new { }
                    }
                },
                new McpTool
                {
                    Name = "search_workers_by_name",
                    Description = "Busca trabajadores por nombre (búsqueda parcial, no distingue mayúsculas/minúsculas)",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            name = new
                            {
                                type = "string",
                                description = "Nombre o parte del nombre del trabajador a buscar"
                            }
                        },
                        required = new[] { "name" }
                    }
                },
                // Project Tools
                new McpTool
                {
                    Name = "get_all_projects",
                    Description = "Obtiene la lista completa de todos los proyectos con información resumida",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new { }
                    }
                },
                new McpTool
                {
                    Name = "get_project_by_id",
                    Description = "Obtiene información detallada de un proyecto específico por su ID",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            id = new
                            {
                                type = "number",
                                description = "ID único del proyecto"
                            }
                        },
                        required = new[] { "id" }
                    }
                },
                new McpTool
                {
                    Name = "get_projects_by_status",
                    Description = "Obtiene proyectos filtrados por estado: Planning, InProgress, OnHold, Completed, Cancelled",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            status = new
                            {
                                type = "string",
                                description = "Estado del proyecto: Planning, InProgress, OnHold, Completed, Cancelled"
                            }
                        },
                        required = new[] { "status" }
                    }
                },
                new McpTool
                {
                    Name = "get_projects_by_manager",
                    Description = "Obtiene todos los proyectos gestionados por un Project Manager específico",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            managerId = new
                            {
                                type = "number",
                                description = "ID del trabajador que es Project Manager"
                            }
                        },
                        required = new[] { "managerId" }
                    }
                },
                new McpTool
                {
                    Name = "search_projects",
                    Description = "Busca proyectos por nombre o descripción",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            searchTerm = new
                            {
                                type = "string",
                                description = "Término de búsqueda para encontrar en nombre o descripción del proyecto"
                            }
                        },
                        required = new[] { "searchTerm" }
                    }
                },
                // Task Tools
                new McpTool
                {
                    Name = "get_tasks_by_project",
                    Description = "Obtiene todas las tareas de un proyecto específico",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            projectId = new
                            {
                                type = "number",
                                description = "ID del proyecto"
                            }
                        },
                        required = new[] { "projectId" }
                    }
                },
                new McpTool
                {
                    Name = "get_tasks_by_worker",
                    Description = "Obtiene todas las tareas asignadas a un trabajador específico",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            workerId = new
                            {
                                type = "number",
                                description = "ID del trabajador"
                            }
                        },
                        required = new[] { "workerId" }
                    }
                },
                new McpTool
                {
                    Name = "get_task_by_id",
                    Description = "Obtiene información detallada de una tarea específica por su ID",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            taskId = new
                            {
                                type = "number",
                                description = "ID único de la tarea"
                            }
                        },
                        required = new[] { "taskId" }
                    }
                },
                new McpTool
                {
                    Name = "get_tasks_by_status",
                    Description = "Obtiene tareas filtradas por estado: ToDo, InProgress, InReview, Blocked, Completed, Cancelled",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            status = new
                            {
                                type = "string",
                                description = "Estado de la tarea: ToDo, InProgress, InReview, Blocked, Completed, Cancelled"
                            }
                        },
                        required = new[] { "status" }
                    }
                },
                // Statistics and Reports
                new McpTool
                {
                    Name = "get_project_statistics",
                    Description = "Obtiene estadísticas generales del sistema de proyectos (totales, completados, en progreso, etc.)",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new { }
                    }
                },
                new McpTool
                {
                    Name = "get_team_workload",
                    Description = "Obtiene un resumen de la carga de trabajo de todo el equipo",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new { }
                    }
                },
                // Team Management
                new McpTool
                {
                    Name = "assign_worker_to_project",
                    Description = "Asigna un trabajador a un proyecto específico usando IDs",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            projectId = new
                            {
                                type = "number",
                                description = "ID del proyecto"
                            },
                            workerId = new
                            {
                                type = "number",
                                description = "ID del trabajador a asignar"
                            }
                        },
                        required = new[] { "projectId", "workerId" }
                    }
                },
                new McpTool
                {
                    Name = "remove_worker_from_project",
                    Description = "Remueve un trabajador de un proyecto específico usando IDs",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            projectId = new
                            {
                                type = "number",
                                description = "ID del proyecto"
                            },
                            workerId = new
                            {
                                type = "number",
                                description = "ID del trabajador a remover"
                            }
                        },
                        required = new[] { "projectId", "workerId" }
                    }
                },
                new McpTool
                {
                    Name = "assign_worker_by_name",
                    Description = "Asigna un trabajador a un proyecto usando nombres (busca automáticamente los IDs). Útil para asignaciones en lenguaje natural como 'añade a Javier Moreno al proyecto de migración'",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            workerName = new
                            {
                                type = "string",
                                description = "Nombre completo o parcial del trabajador"
                            },
                            projectName = new
                            {
                                type = "string",
                                description = "Nombre completo o parcial del proyecto"
                            }
                        },
                        required = new[] { "workerName", "projectName" }
                    }
                }
            }
        };
    }

    private Task<McpToolCallResult> HandleToolsCall(McpRequest request)
    {
        _logger.LogInformation("Handling tools/call request");

        var paramsJson = JsonSerializer.Serialize(request.Params);
        var toolCallParams = JsonSerializer.Deserialize<McpToolCallParams>(paramsJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (toolCallParams == null)
        {
            throw new Exception("Invalid tool call parameters");
        }

        _logger.LogInformation($"Calling tool: {toolCallParams.Name}");

        string resultText;

        try
        {
            resultText = toolCallParams.Name switch
            {
                // Worker tools
                "get_worker_by_id" => ExecuteGetWorkerById(toolCallParams.Arguments),
                "get_all_workers" => _workerTools.GetAllWorkers(),
                "search_workers_by_name" => ExecuteSearchWorkersByName(toolCallParams.Arguments),
                
                // Project tools
                "get_all_projects" => _projectTools.GetAllProjects(),
                "get_project_by_id" => ExecuteGetProjectById(toolCallParams.Arguments),
                "get_projects_by_status" => ExecuteGetProjectsByStatus(toolCallParams.Arguments),
                "get_projects_by_manager" => ExecuteGetProjectsByManager(toolCallParams.Arguments),
                "search_projects" => ExecuteSearchProjects(toolCallParams.Arguments),
                
                // Task tools
                "get_tasks_by_project" => ExecuteGetTasksByProject(toolCallParams.Arguments),
                "get_tasks_by_worker" => ExecuteGetTasksByWorker(toolCallParams.Arguments),
                "get_task_by_id" => ExecuteGetTaskById(toolCallParams.Arguments),
                "get_tasks_by_status" => ExecuteGetTasksByStatus(toolCallParams.Arguments),
                
                // Statistics and Reports
                "get_project_statistics" => _projectTools.GetProjectStatistics(),
                "get_team_workload" => _projectTools.GetTeamWorkload(),
                
                // Team Management
                "assign_worker_to_project" => ExecuteAssignWorkerToProject(toolCallParams.Arguments),
                "remove_worker_from_project" => ExecuteRemoveWorkerFromProject(toolCallParams.Arguments),
                "assign_worker_by_name" => ExecuteAssignWorkerByName(toolCallParams.Arguments),
                
                _ => $"Error: Herramienta desconocida '{toolCallParams.Name}'"
            };

            _logger.LogInformation($"Tool result length: {resultText.Length} characters");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error executing tool {toolCallParams.Name}");
            resultText = $"Error ejecutando la herramienta: {ex.Message}";
        }

        return Task.FromResult(new McpToolCallResult
        {
            Content = new List<McpContent>
            {
                new McpContent
                {
                    Type = "text",
                    Text = resultText
                }
            }
        });
    }

    private string ExecuteGetWorkerById(Dictionary<string, object>? arguments)
    {
        if (arguments == null || !arguments.ContainsKey("id"))
        {
            return "Error: Se requiere el parámetro 'id'";
        }

        var idValue = arguments["id"];
        int id;

        if (idValue is JsonElement jsonElement)
        {
            id = jsonElement.GetInt32();
        }
        else if (idValue is int intValue)
        {
            id = intValue;
        }
        else if (int.TryParse(idValue.ToString(), out int parsedId))
        {
            id = parsedId;
        }
        else
        {
            return "Error: El parámetro 'id' debe ser un número";
        }

        return _workerTools.GetWorkerById(id);
    }

    private string ExecuteSearchWorkersByName(Dictionary<string, object>? arguments)
    {
        if (arguments == null || !arguments.ContainsKey("name"))
        {
            return "Error: Se requiere el parámetro 'name'";
        }

        var nameValue = arguments["name"];
        string name;

        if (nameValue is JsonElement jsonElement)
        {
            name = jsonElement.GetString() ?? "";
        }
        else
        {
            name = nameValue.ToString() ?? "";
        }

        return _workerTools.SearchWorkersByName(name);
    }

    // Project Tool Helpers
    private string ExecuteGetProjectById(Dictionary<string, object>? arguments)
    {
        if (arguments == null || !arguments.ContainsKey("id"))
        {
            return "Error: Se requiere el parámetro 'id'";
        }

        var id = ExtractIntArgument(arguments, "id");
        return _projectTools.GetProjectById(id);
    }

    private string ExecuteGetProjectsByStatus(Dictionary<string, object>? arguments)
    {
        if (arguments == null || !arguments.ContainsKey("status"))
        {
            return "Error: Se requiere el parámetro 'status'";
        }

        var status = ExtractStringArgument(arguments, "status");
        return _projectTools.GetProjectsByStatus(status);
    }

    private string ExecuteGetProjectsByManager(Dictionary<string, object>? arguments)
    {
        if (arguments == null || !arguments.ContainsKey("managerId"))
        {
            return "Error: Se requiere el parámetro 'managerId'";
        }

        var managerId = ExtractIntArgument(arguments, "managerId");
        return _projectTools.GetProjectsByManager(managerId);
    }

    private string ExecuteSearchProjects(Dictionary<string, object>? arguments)
    {
        if (arguments == null || !arguments.ContainsKey("searchTerm"))
        {
            return "Error: Se requiere el parámetro 'searchTerm'";
        }

        var searchTerm = ExtractStringArgument(arguments, "searchTerm");
        return _projectTools.SearchProjects(searchTerm);
    }

    // Task Tool Helpers
    private string ExecuteGetTasksByProject(Dictionary<string, object>? arguments)
    {
        if (arguments == null || !arguments.ContainsKey("projectId"))
        {
            return "Error: Se requiere el parámetro 'projectId'";
        }

        var projectId = ExtractIntArgument(arguments, "projectId");
        return _projectTools.GetTasksByProject(projectId);
    }

    private string ExecuteGetTasksByWorker(Dictionary<string, object>? arguments)
    {
        if (arguments == null || !arguments.ContainsKey("workerId"))
        {
            return "Error: Se requiere el parámetro 'workerId'";
        }

        var workerId = ExtractIntArgument(arguments, "workerId");
        return _projectTools.GetTasksByWorker(workerId);
    }

    private string ExecuteGetTaskById(Dictionary<string, object>? arguments)
    {
        if (arguments == null || !arguments.ContainsKey("taskId"))
        {
            return "Error: Se requiere el parámetro 'taskId'";
        }

        var taskId = ExtractIntArgument(arguments, "taskId");
        return _projectTools.GetTaskById(taskId);
    }

    private string ExecuteGetTasksByStatus(Dictionary<string, object>? arguments)
    {
        if (arguments == null || !arguments.ContainsKey("status"))
        {
            return "Error: Se requiere el parámetro 'status'";
        }

        var status = ExtractStringArgument(arguments, "status");
        return _projectTools.GetTasksByStatus(status);
    }

    // Team Management Tool Helpers
    private string ExecuteAssignWorkerToProject(Dictionary<string, object>? arguments)
    {
        if (arguments == null || !arguments.ContainsKey("projectId") || !arguments.ContainsKey("workerId"))
        {
            return "Error: Se requieren los parámetros 'projectId' y 'workerId'";
        }

        var projectId = ExtractIntArgument(arguments, "projectId");
        var workerId = ExtractIntArgument(arguments, "workerId");
        return _projectTools.AssignWorkerToProject(projectId, workerId);
    }

    private string ExecuteRemoveWorkerFromProject(Dictionary<string, object>? arguments)
    {
        if (arguments == null || !arguments.ContainsKey("projectId") || !arguments.ContainsKey("workerId"))
        {
            return "Error: Se requieren los parámetros 'projectId' y 'workerId'";
        }

        var projectId = ExtractIntArgument(arguments, "projectId");
        var workerId = ExtractIntArgument(arguments, "workerId");
        return _projectTools.RemoveWorkerFromProject(projectId, workerId);
    }

    private string ExecuteAssignWorkerByName(Dictionary<string, object>? arguments)
    {
        if (arguments == null || !arguments.ContainsKey("workerName") || !arguments.ContainsKey("projectName"))
        {
            return "Error: Se requieren los parámetros 'workerName' y 'projectName'";
        }

        var workerName = ExtractStringArgument(arguments, "workerName");
        var projectName = ExtractStringArgument(arguments, "projectName");
        return _projectTools.AssignWorkerToProjectByName(workerName, projectName);
    }

    // Generic Argument Extractors
    private int ExtractIntArgument(Dictionary<string, object> arguments, string key)
    {
        var value = arguments[key];
        
        if (value is JsonElement jsonElement)
        {
            return jsonElement.GetInt32();
        }
        else if (value is int intValue)
        {
            return intValue;
        }
        else if (int.TryParse(value.ToString(), out int parsedValue))
        {
            return parsedValue;
        }
        
        throw new ArgumentException($"El parámetro '{key}' debe ser un número");
    }

    private string ExtractStringArgument(Dictionary<string, object> arguments, string key)
    {
        var value = arguments[key];
        
        if (value is JsonElement jsonElement)
        {
            return jsonElement.GetString() ?? "";
        }
        
        return value.ToString() ?? "";
    }

    private HttpResponseData CreateErrorResponse(HttpRequestData req, object? id, int code, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");

        var errorResponse = new McpResponse
        {
            JsonRpc = "2.0",
            Id = id ?? 1, // Asegurar que nunca sea null
            Error = new McpError
            {
                Code = code,
                Message = message
            }
        };

        var jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        response.WriteString(JsonSerializer.Serialize(errorResponse, jsonOptions), Encoding.UTF8);
        return response;
    }
}
