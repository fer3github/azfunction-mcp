using McpAzFunction.Data;
using McpAzFunction.Models;
using System.Text;

namespace McpAzFunction.Functions;

public class ProjectTools
{
    private readonly ProjectRepository _projectRepository;
    private readonly WorkerRepository _workerRepository;

    public ProjectTools(ProjectRepository projectRepository, WorkerRepository workerRepository)
    {
        _projectRepository = projectRepository;
        _workerRepository = workerRepository;
    }

    public string GetAllProjects()
    {
        var projects = _projectRepository.GetAll();
        
        if (projects.Count == 0)
        {
            return "No hay proyectos registrados en el sistema.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"📊 **RESUMEN DE PROYECTOS** ({projects.Count} total)\n");

        foreach (var project in projects)
        {
            var pm = _workerRepository.GetById(project.ProjectManagerId);
            var statusEmoji = GetStatusEmoji(project.Status);
            var priorityEmoji = GetPriorityEmoji(project.Priority);
            
            sb.AppendLine($"{statusEmoji} **{project.Name}** (ID: {project.Id})");
            sb.AppendLine($"   └─ Estado: {TranslateStatus(project.Status)} | Prioridad: {priorityEmoji} {project.Priority}");
            sb.AppendLine($"   └─ Project Manager: {pm?.Nombre ?? "No asignado"}");
            sb.AppendLine($"   └─ Fechas: {project.StartDate:dd/MM/yyyy} - {project.EndDate?.ToString("dd/MM/yyyy") ?? "Sin fecha"}");
            sb.AppendLine($"   └─ Equipo: [{string.Join(",", project.TeamMemberIds)}] | Tareas: {project.Tasks.Count}");
            sb.AppendLine($"   └─ Presupuesto: {project.Budget:C0}\n");
        }

        return sb.ToString().TrimEnd();
    }

    public string GetProjectById(int id)
    {
        var project = _projectRepository.GetById(id);
        
        if (project == null)
        {
            return $"❌ No se encontró ningún proyecto con ID {id}.";
        }

        return FormatProjectDetail(project);
    }

    public string GetProjectsByStatus(string status)
    {
        if (!Enum.TryParse<ProjectStatus>(status, true, out var projectStatus))
        {
            return $"❌ Estado no válido. Estados disponibles: Planning, InProgress, OnHold, Completed, Cancelled";
        }

        var projects = _projectRepository.GetByStatus(projectStatus);
        
        if (projects.Count == 0)
        {
            return $"No se encontraron proyectos con estado '{TranslateStatus(projectStatus)}'.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"📋 **PROYECTOS EN ESTADO: {TranslateStatus(projectStatus).ToUpper()}** ({projects.Count} encontrado{(projects.Count > 1 ? "s" : "")})\n");

        foreach (var project in projects)
        {
            sb.AppendLine(FormatProjectSummary(project));
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    public string GetProjectsByManager(int managerId)
    {
        var manager = _workerRepository.GetById(managerId);
        if (manager == null)
        {
            return $"❌ No se encontró ningún trabajador con ID {managerId}.";
        }

        var projects = _projectRepository.GetByProjectManager(managerId);
        
        if (projects.Count == 0)
        {
            return $"El trabajador {manager.Nombre} no está gestionando ningún proyecto actualmente.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"👤 **PROYECTOS DE {manager.Nombre.ToUpper()}** ({projects.Count} proyecto{(projects.Count > 1 ? "s" : "")})\n");

        foreach (var project in projects)
        {
            sb.AppendLine(FormatProjectSummary(project));
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    public string SearchProjects(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return "❌ Por favor, proporciona un término de búsqueda.";
        }

        var projects = _projectRepository.SearchByName(searchTerm);
        
        if (projects.Count == 0)
        {
            return $"No se encontraron proyectos que coincidan con '{searchTerm}'.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"🔍 **RESULTADOS DE BÚSQUEDA: '{searchTerm}'** ({projects.Count} encontrado{(projects.Count > 1 ? "s" : "")})\n");

        foreach (var project in projects)
        {
            sb.AppendLine(FormatProjectSummary(project));
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    public string GetTasksByProject(int projectId)
    {
        var project = _projectRepository.GetById(projectId);
        if (project == null)
        {
            return $"❌ No se encontró ningún proyecto con ID {projectId}.";
        }

        var tasks = _projectRepository.GetTasksByProject(projectId);
        
        if (tasks.Count == 0)
        {
            return $"El proyecto '{project.Name}' no tiene tareas registradas.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"📋 **TAREAS DEL PROYECTO: {project.Name.ToUpper()}** ({tasks.Count} tarea{(tasks.Count > 1 ? "s" : "")})\n");

        var tasksByStatus = tasks.GroupBy(t => t.Status);
        foreach (var group in tasksByStatus.OrderBy(g => g.Key))
        {
            sb.AppendLine($"\n**{TranslateTaskStatus(group.Key)}** ({group.Count()}):");
            foreach (var task in group)
            {
                sb.AppendLine(FormatTaskSummary(task));
            }
        }

        return sb.ToString().TrimEnd();
    }

    public string GetTasksByWorker(int workerId)
    {
        var worker = _workerRepository.GetById(workerId);
        if (worker == null)
        {
            return $"❌ No se encontró ningún trabajador con ID {workerId}.";
        }

        var tasks = _projectRepository.GetTasksByWorker(workerId);
        
        if (tasks.Count == 0)
        {
            return $"{worker.Nombre} no tiene tareas asignadas actualmente.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"👤 **TAREAS DE {worker.Nombre.ToUpper()}** ({tasks.Count} tarea{(tasks.Count > 1 ? "s" : "")})\n");

        var tasksByStatus = tasks.GroupBy(t => t.Status);
        foreach (var group in tasksByStatus.OrderBy(g => g.Key))
        {
            sb.AppendLine($"\n**{TranslateTaskStatus(group.Key)}** ({group.Count()}):");
            foreach (var task in group)
            {
                var project = _projectRepository.GetById(task.ProjectId);
                sb.AppendLine($"   {GetTaskStatusEmoji(task.Status)} {task.Title}");
                sb.AppendLine($"      └─ Proyecto: {project?.Name ?? "N/A"} | Prioridad: {GetTaskPriorityEmoji(task.Priority)} {TranslateTaskPriority(task.Priority)}");
                sb.AppendLine($"      └─ Vencimiento: {task.DueDate?.ToString("dd/MM/yyyy") ?? "Sin fecha"} | Estimado: {task.EstimatedHours}h | Real: {task.ActualHours}h");
            }
        }

        return sb.ToString().TrimEnd();
    }

    public string GetTaskById(int taskId)
    {
        var task = _projectRepository.GetTaskById(taskId);
        
        if (task == null)
        {
            return $"❌ No se encontró ninguna tarea con ID {taskId}.";
        }

        return FormatTaskDetail(task);
    }

    public string GetTasksByStatus(string status)
    {
        if (!Enum.TryParse<ProjectTaskStatus>(status, true, out var ProjectTaskStatus))
        {
            return $"❌ Estado no válido. Estados disponibles: ToDo, InProgress, InReview, Blocked, Completed, Cancelled";
        }

        var tasks = _projectRepository.GetTasksByStatus(ProjectTaskStatus);
        
        if (tasks.Count == 0)
        {
            return $"No se encontraron tareas con estado '{TranslateTaskStatus(ProjectTaskStatus)}'.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"📋 **TAREAS EN ESTADO: {TranslateTaskStatus(ProjectTaskStatus).ToUpper()}** ({tasks.Count} encontrada{(tasks.Count > 1 ? "s" : "")})\n");

        foreach (var task in tasks)
        {
            var project = _projectRepository.GetById(task.ProjectId);
            var assignedTo = task.AssignedToId.HasValue ? _workerRepository.GetById(task.AssignedToId.Value) : null;
            
            sb.AppendLine($"{GetTaskStatusEmoji(task.Status)} **{task.Title}** (ID: {task.Id})");
            sb.AppendLine($"   └─ Proyecto: {project?.Name ?? "N/A"}");
            sb.AppendLine($"   └─ Asignado a: {assignedTo?.Nombre ?? "Sin asignar"}");
            sb.AppendLine($"   └─ Prioridad: {GetTaskPriorityEmoji(task.Priority)} {TranslateTaskPriority(task.Priority)}");
            sb.AppendLine($"   └─ Vencimiento: {task.DueDate?.ToString("dd/MM/yyyy") ?? "Sin fecha"}\n");
        }

        return sb.ToString().TrimEnd();
    }

    public string GetProjectStatistics()
    {
        var stats = _projectRepository.GetProjectStatistics();
        var projects = _projectRepository.GetAll();
        
        var sb = new StringBuilder();
        sb.AppendLine("📊 **ESTADÍSTICAS DEL SISTEMA DE PROYECTOS**\n");
        
        sb.AppendLine("**📁 PROYECTOS:**");
        sb.AppendLine($"   • Total de proyectos: {stats["TotalProjects"]}");
        sb.AppendLine($"   • En planificación: {stats["PlanningProjects"]}");
        sb.AppendLine($"   • En progreso: {stats["ActiveProjects"]}");
        sb.AppendLine($"   • Completados: {stats["CompletedProjects"]}\n");
        
        sb.AppendLine("**✅ TAREAS:**");
        sb.AppendLine($"   • Total de tareas: {stats["TotalTasks"]}");
        sb.AppendLine($"   • Completadas: {stats["CompletedTasks"]}");
        sb.AppendLine($"   • En progreso: {stats["InProgressTasks"]}");
        sb.AppendLine($"   • Bloqueadas: {stats["BlockedTasks"]}\n");
        
        var totalBudget = projects.Sum(p => p.Budget);
        sb.AppendLine("**💰 PRESUPUESTO:**");
        sb.AppendLine($"   • Presupuesto total: {totalBudget:C0}\n");
        
        var completionRate = stats["TotalTasks"] > 0 
            ? (double)stats["CompletedTasks"] / stats["TotalTasks"] * 100 
            : 0;
        sb.AppendLine($"**📈 PROGRESO GENERAL:** {completionRate:F1}% de tareas completadas");
        
        return sb.ToString().TrimEnd();
    }

    public string GetTeamWorkload()
    {
        var workers = _workerRepository.GetAll();
        var sb = new StringBuilder();
        
        sb.AppendLine("👥 **CARGA DE TRABAJO DEL EQUIPO**\n");
        
        foreach (var worker in workers)
        {
            var tasks = _projectRepository.GetTasksByWorker(worker.Id);
            var activeTasks = tasks.Count(t => t.Status == ProjectTaskStatus.InProgress || t.Status == ProjectTaskStatus.ToDo);
            var totalHours = tasks.Where(t => t.Status != ProjectTaskStatus.Completed).Sum(t => t.EstimatedHours - t.ActualHours);
            
            if (tasks.Count > 0)
            {
                sb.AppendLine($"**{worker.Nombre}** ({worker.Puesto})");
                sb.AppendLine($"   └─ Tareas activas: {activeTasks} | Total tareas: {tasks.Count}");
                sb.AppendLine($"   └─ Horas pendientes: {totalHours}h");
                
                var projects = _projectRepository.GetByProjectManager(worker.Id);
                if (projects.Count > 0)
                {
                    sb.AppendLine($"   └─ **Project Manager de {projects.Count} proyecto(s)**");
                }
                sb.AppendLine();
            }
        }
        
        return sb.ToString().TrimEnd();
    }

    // Helper methods
    private string FormatProjectDetail(Project project)
    {
        var sb = new StringBuilder();
        var pm = _workerRepository.GetById(project.ProjectManagerId);
        var statusEmoji = GetStatusEmoji(project.Status);
        
        sb.AppendLine($"{statusEmoji} **{project.Name}** (ID: {project.Id})");
        sb.AppendLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
        
        sb.AppendLine($"📝 **Descripción:**");
        sb.AppendLine($"   {project.Description}\n");
        
        sb.AppendLine($"📊 **Información General:**");
        sb.AppendLine($"   • Estado: {TranslateStatus(project.Status)}");
        sb.AppendLine($"   • Prioridad: {GetPriorityEmoji(project.Priority)} {project.Priority}");
        sb.AppendLine($"   • Presupuesto: {project.Budget:C0}\n");
        
        sb.AppendLine($"📅 **Fechas:**");
        sb.AppendLine($"   • Inicio: {project.StartDate:dd/MM/yyyy}");
        sb.AppendLine($"   • Fin estimado: {project.EndDate?.ToString("dd/MM/yyyy") ?? "Sin fecha"}\n");
        
        sb.AppendLine($"👥 **Equipo:**");
        sb.AppendLine($"   • Project Manager: {pm?.Nombre ?? "No asignado"}");
        sb.AppendLine($"   • Miembros del equipo ({project.TeamMemberIds.Count}):");
        foreach (var memberId in project.TeamMemberIds)
        {
            var member = _workerRepository.GetById(memberId);
            if (member != null)
            {
                sb.AppendLine($"     - {member.Nombre} ({member.Puesto})");
            }
        }
        
        sb.AppendLine($"\n✅ **Tareas:** ({project.Tasks.Count} total)");
        var tasksByStatus = project.Tasks.GroupBy(t => t.Status);
        foreach (var group in tasksByStatus.OrderBy(g => g.Key))
        {
            sb.AppendLine($"   • {TranslateTaskStatus(group.Key)}: {group.Count()}");
        }
        
        return sb.ToString().TrimEnd();
    }

    private string FormatProjectSummary(Project project)
    {
        var pm = _workerRepository.GetById(project.ProjectManagerId);
        var statusEmoji = GetStatusEmoji(project.Status);
        var priorityEmoji = GetPriorityEmoji(project.Priority);
        
        var sb = new StringBuilder();
        sb.AppendLine($"{statusEmoji} **{project.Name}** (ID: {project.Id})");
        sb.AppendLine($"   └─ Estado: {TranslateStatus(project.Status)} | Prioridad: {priorityEmoji} {project.Priority}");
        sb.AppendLine($"   └─ PM: {pm?.Nombre ?? "No asignado"} | Equipo: {project.TeamMemberIds.Count} | Tareas: {project.Tasks.Count}");
        sb.AppendLine($"   └─ {project.StartDate:dd/MM/yyyy} → {project.EndDate?.ToString("dd/MM/yyyy") ?? "Sin fecha"}");
        
        return sb.ToString().TrimEnd();
    }

    private string FormatTaskDetail(ProjectTask task)
    {
        var sb = new StringBuilder();
        var project = _projectRepository.GetById(task.ProjectId);
        var assignedTo = task.AssignedToId.HasValue ? _workerRepository.GetById(task.AssignedToId.Value) : null;
        
        sb.AppendLine($"{GetTaskStatusEmoji(task.Status)} **{task.Title}** (ID: {task.Id})");
        sb.AppendLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
        
        sb.AppendLine($"📝 **Descripción:**");
        sb.AppendLine($"   {task.Description}\n");
        
        sb.AppendLine($"📊 **Información:**");
        sb.AppendLine($"   • Proyecto: {project?.Name ?? "N/A"}");
        sb.AppendLine($"   • Estado: {TranslateTaskStatus(task.Status)}");
        sb.AppendLine($"   • Prioridad: {GetTaskPriorityEmoji(task.Priority)} {TranslateTaskPriority(task.Priority)}");
        sb.AppendLine($"   • Asignado a: {assignedTo?.Nombre ?? "Sin asignar"}\n");
        
        sb.AppendLine($"📅 **Fechas:**");
        sb.AppendLine($"   • Creación: {task.CreatedDate:dd/MM/yyyy}");
        sb.AppendLine($"   • Vencimiento: {task.DueDate?.ToString("dd/MM/yyyy") ?? "Sin fecha"}");
        if (task.CompletedDate.HasValue)
        {
            sb.AppendLine($"   • Completada: {task.CompletedDate.Value:dd/MM/yyyy}");
        }
        
        sb.AppendLine($"\n⏱️ **Tiempo:**");
        sb.AppendLine($"   • Estimado: {task.EstimatedHours}h");
        sb.AppendLine($"   • Real: {task.ActualHours}h");
        sb.AppendLine($"   • Restante: {Math.Max(0, task.EstimatedHours - task.ActualHours)}h");
        
        if (task.Tags.Count > 0)
        {
            sb.AppendLine($"\n🏷️ **Tags:** {string.Join(", ", task.Tags)}");
        }
        
        return sb.ToString().TrimEnd();
    }

    private string FormatTaskSummary(ProjectTask task)
    {
        var assignedTo = task.AssignedToId.HasValue ? _workerRepository.GetById(task.AssignedToId.Value) : null;
        return $"   {GetTaskStatusEmoji(task.Status)} {task.Title} | Asignado a: {assignedTo?.Nombre ?? "Sin asignar"} | Vence: {task.DueDate?.ToString("dd/MM/yyyy") ?? "Sin fecha"}";
    }

    private string GetStatusEmoji(ProjectStatus status)
    {
        return status switch
        {
            ProjectStatus.Planning => "📋",
            ProjectStatus.InProgress => "🚀",
            ProjectStatus.OnHold => "⏸️",
            ProjectStatus.Completed => "✅",
            ProjectStatus.Cancelled => "❌",
            _ => "📁"
        };
    }

    private string GetPriorityEmoji(string priority)
    {
        return priority.ToLower() switch
        {
            "alta" or "high" => "🔴",
            "media" or "medium" => "🟡",
            "baja" or "low" => "🟢",
            _ => "⚪"
        };
    }

    private string GetTaskStatusEmoji(ProjectTaskStatus status)
    {
        return status switch
        {
            ProjectTaskStatus.ToDo => "⬜",
            ProjectTaskStatus.InProgress => "🔵",
            ProjectTaskStatus.InReview => "🟣",
            ProjectTaskStatus.Blocked => "🔴",
            ProjectTaskStatus.Completed => "✅",
            ProjectTaskStatus.Cancelled => "❌",
            _ => "⚪"
        };
    }

    private string GetTaskPriorityEmoji(TaskPriority priority)
    {
        return priority switch
        {
            TaskPriority.Critical => "🔴",
            TaskPriority.High => "🟠",
            TaskPriority.Medium => "🟡",
            TaskPriority.Low => "🟢",
            _ => "⚪"
        };
    }

    private string TranslateStatus(ProjectStatus status)
    {
        return status switch
        {
            ProjectStatus.Planning => "En Planificación",
            ProjectStatus.InProgress => "En Progreso",
            ProjectStatus.OnHold => "En Pausa",
            ProjectStatus.Completed => "Completado",
            ProjectStatus.Cancelled => "Cancelado",
            _ => status.ToString()
        };
    }

    private string TranslateTaskStatus(ProjectTaskStatus status)
    {
        return status switch
        {
            ProjectTaskStatus.ToDo => "Por Hacer",
            ProjectTaskStatus.InProgress => "En Progreso",
            ProjectTaskStatus.InReview => "En Revisión",
            ProjectTaskStatus.Blocked => "Bloqueada",
            ProjectTaskStatus.Completed => "Completada",
            ProjectTaskStatus.Cancelled => "Cancelada",
            _ => status.ToString()
        };
    }

    private string TranslateTaskPriority(TaskPriority priority)
    {
        return priority switch
        {
            TaskPriority.Critical => "Crítica",
            TaskPriority.High => "Alta",
            TaskPriority.Medium => "Media",
            TaskPriority.Low => "Baja",
            _ => priority.ToString()
        };
    }

    public string AssignWorkerToProject(int projectId, int workerId)
    {
        var project = _projectRepository.GetById(projectId);
        if (project == null)
        {
            return $"❌ No se encontró ningún proyecto con ID {projectId}.";
        }

        var worker = _workerRepository.GetById(workerId);
        if (worker == null)
        {
            return $"❌ No se encontró ningún trabajador con ID {workerId}.";
        }

        if (project.TeamMemberIds.Contains(workerId))
        {
            return $"⚠️ {worker.Nombre} ya está asignado al proyecto '{project.Name}'.";
        }

        project.TeamMemberIds.Add(workerId);

        return $"✅ **{worker.Nombre}** ha sido asignado exitosamente al proyecto **'{project.Name}'**.\n" +
               $"   └─ Equipo actual: {project.TeamMemberIds.Count} miembros\n" +
               $"   └─ Proyecto: {project.Name} ({TranslateStatus(project.Status)})";
    }

    public string RemoveWorkerFromProject(int projectId, int workerId)
    {
        var project = _projectRepository.GetById(projectId);
        if (project == null)
        {
            return $"❌ No se encontró ningún proyecto con ID {projectId}.";
        }

        var worker = _workerRepository.GetById(workerId);
        if (worker == null)
        {
            return $"❌ No se encontró ningún trabajador con ID {workerId}.";
        }

        if (!project.TeamMemberIds.Contains(workerId))
        {
            return $"⚠️ {worker.Nombre} no está asignado al proyecto '{project.Name}'.";
        }

        project.TeamMemberIds.Remove(workerId);

        return $"✅ **{worker.Nombre}** ha sido removido del proyecto **'{project.Name}'**.\n" +
               $"   └─ Equipo actual: {project.TeamMemberIds.Count} miembros\n" +
               $"   └─ Proyecto: {project.Name} ({TranslateStatus(project.Status)})";
    }

    public string AssignWorkerToProjectByName(string workerName, string projectName)
    {
        // Buscar trabajador por nombre
        var workers = _workerRepository.SearchByName(workerName);
        if (workers.Count == 0)
        {
            return $"❌ No se encontró ningún trabajador con el nombre '{workerName}'.";
        }
        if (workers.Count > 1)
        {
            var names = string.Join(", ", workers.Select(w => $"{w.Nombre} (ID: {w.Id})"));
            return $"⚠️ Se encontraron {workers.Count} trabajadores con ese nombre: {names}. Por favor especifica el ID.";
        }

        var worker = workers[0];

        // Buscar proyecto por nombre
        var projects = _projectRepository.SearchByName(projectName);
        if (projects.Count == 0)
        {
            return $"❌ No se encontró ningún proyecto con el nombre '{projectName}'.";
        }
        if (projects.Count > 1)
        {
            var names = string.Join(", ", projects.Select(p => $"{p.Name} (ID: {p.Id})"));
            return $"⚠️ Se encontraron {projects.Count} proyectos con ese nombre: {names}. Por favor especifica el ID.";
        }

        var project = projects[0];

        // Asignar
        if (project.TeamMemberIds.Contains(worker.Id))
        {
            return $"⚠️ {worker.Nombre} ya está asignado al proyecto '{project.Name}'.";
        }

        project.TeamMemberIds.Add(worker.Id);

        return $"✅ **{worker.Nombre}** ha sido asignado exitosamente al proyecto **'{project.Name}'**.\n" +
               $"   └─ Equipo actual: {project.TeamMemberIds.Count} miembros\n" +
               $"   └─ Proyecto: {project.Name} ({TranslateStatus(project.Status)})";
    }
}
