using McpAzFunction.Models;

namespace McpAzFunction.Data;

public class ProjectRepository
{
    private readonly List<Project> _projects;
    private readonly WorkerRepository _workerRepository;

    public ProjectRepository(WorkerRepository workerRepository)
    {
        _workerRepository = workerRepository;
        _projects = InitializeProjects();
    }

    private List<Project> InitializeProjects()
    {
        return new List<Project>
        {
            new Project
            {
                Id = 1,
                Name = "Migración a Cloud Azure",
                Description = "Migración completa de infraestructura on-premise a Azure Cloud, incluyendo bases de datos, aplicaciones y servicios.",
                StartDate = new DateTime(2025, 1, 15),
                EndDate = new DateTime(2025, 6, 30),
                Status = ProjectStatus.InProgress,
                ProjectManagerId = 6, // Jorge Ramírez Castro - Arquitecto Cloud
                Priority = "Alta",
                Budget = 250000m,
                TeamMemberIds = new List<int> { 1, 6, 8 }, // Javier, Jorge, David
                Tasks = new List<ProjectTask>
                {
                    new ProjectTask
                    {
                        Id = 101,
                        ProjectId = 1,
                        Title = "Análisis de infraestructura actual",
                        Description = "Documentar toda la infraestructura on-premise existente",
                        AssignedToId = 6,
                        Status = ProjectTaskStatus.Completed,
                        Priority = TaskPriority.High,
                        CreatedDate = new DateTime(2025, 1, 15),
                        DueDate = new DateTime(2025, 2, 1),
                        CompletedDate = new DateTime(2025, 1, 28),
                        EstimatedHours = 40,
                        ActualHours = 38,
                        Tags = new List<string> { "infrastructure", "analysis" }
                    },
                    new ProjectTask
                    {
                        Id = 102,
                        ProjectId = 1,
                        Title = "Configurar Azure Landing Zone",
                        Description = "Establecer la arquitectura base en Azure con redes, seguridad y governance",
                        AssignedToId = 6,
                        Status = ProjectTaskStatus.InProgress,
                        Priority = TaskPriority.Critical,
                        CreatedDate = new DateTime(2025, 2, 1),
                        DueDate = new DateTime(2025, 3, 15),
                        EstimatedHours = 80,
                        ActualHours = 45,
                        Tags = new List<string> { "azure", "setup", "security" }
                    },
                    new ProjectTask
                    {
                        Id = 103,
                        ProjectId = 1,
                        Title = "Migrar bases de datos a Azure SQL",
                        Description = "Migración de bases de datos SQL Server a Azure SQL Database",
                        AssignedToId = 1,
                        Status = ProjectTaskStatus.ToDo,
                        Priority = TaskPriority.High,
                        CreatedDate = new DateTime(2025, 2, 15),
                        DueDate = new DateTime(2025, 4, 30),
                        EstimatedHours = 60,
                        ActualHours = 0,
                        Tags = new List<string> { "database", "migration" }
                    },
                    new ProjectTask
                    {
                        Id = 104,
                        ProjectId = 1,
                        Title = "Desarrollo de pipeline CI/CD",
                        Description = "Implementar Azure DevOps pipelines para despliegue automático",
                        AssignedToId = 8,
                        Status = ProjectTaskStatus.InProgress,
                        Priority = TaskPriority.Medium,
                        CreatedDate = new DateTime(2025, 2, 20),
                        DueDate = new DateTime(2025, 4, 1),
                        EstimatedHours = 50,
                        ActualHours = 25,
                        Tags = new List<string> { "devops", "ci/cd", "automation" }
                    }
                }
            },
            new Project
            {
                Id = 2,
                Name = "Renovación Portal Web Corporativo",
                Description = "Rediseño completo del portal web corporativo con nuevas funcionalidades y diseño responsive moderno.",
                StartDate = new DateTime(2025, 2, 1),
                EndDate = new DateTime(2025, 5, 31),
                Status = ProjectStatus.InProgress,
                ProjectManagerId = 8, // David Silva Romero - Tech Lead Frontend
                Priority = "Media",
                Budget = 120000m,
                TeamMemberIds = new List<int> { 8, 4 }, // David, Luis
                Tasks = new List<ProjectTask>
                {
                    new ProjectTask
                    {
                        Id = 201,
                        ProjectId = 2,
                        Title = "Diseño UX/UI del nuevo portal",
                        Description = "Crear mockups y prototipos interactivos del nuevo diseño",
                        AssignedToId = 4,
                        Status = ProjectTaskStatus.Completed,
                        Priority = TaskPriority.High,
                        CreatedDate = new DateTime(2025, 2, 1),
                        DueDate = new DateTime(2025, 2, 28),
                        CompletedDate = new DateTime(2025, 2, 25),
                        EstimatedHours = 60,
                        ActualHours = 55,
                        Tags = new List<string> { "design", "ux", "ui" }
                    },
                    new ProjectTask
                    {
                        Id = 202,
                        ProjectId = 2,
                        Title = "Desarrollo frontend con React",
                        Description = "Implementar el nuevo frontend usando React y TypeScript",
                        AssignedToId = 8,
                        Status = ProjectTaskStatus.InProgress,
                        Priority = TaskPriority.High,
                        CreatedDate = new DateTime(2025, 3, 1),
                        DueDate = new DateTime(2025, 4, 30),
                        EstimatedHours = 120,
                        ActualHours = 65,
                        Tags = new List<string> { "react", "frontend", "typescript" }
                    },
                    new ProjectTask
                    {
                        Id = 203,
                        ProjectId = 2,
                        Title = "Integración con CMS",
                        Description = "Integrar el portal con el sistema de gestión de contenidos",
                        AssignedToId = 8,
                        Status = ProjectTaskStatus.ToDo,
                        Priority = TaskPriority.Medium,
                        CreatedDate = new DateTime(2025, 3, 15),
                        DueDate = new DateTime(2025, 5, 15),
                        EstimatedHours = 40,
                        ActualHours = 0,
                        Tags = new List<string> { "cms", "integration" }
                    }
                }
            },
            new Project
            {
                Id = 3,
                Name = "Campaña Marketing Digital Q2",
                Description = "Campaña de marketing digital integral para el segundo trimestre, enfocada en redes sociales y contenido SEO.",
                StartDate = new DateTime(2025, 4, 1),
                EndDate = new DateTime(2025, 6, 30),
                Status = ProjectStatus.Planning,
                ProjectManagerId = 4, // Luis Martínez Fernández - Marketing Digital
                Priority = "Alta",
                Budget = 85000m,
                TeamMemberIds = new List<int> { 4, 3 }, // Luis, María
                Tasks = new List<ProjectTask>
                {
                    new ProjectTask
                    {
                        Id = 301,
                        ProjectId = 3,
                        Title = "Estrategia de contenido Q2",
                        Description = "Definir calendario editorial y temas para el trimestre",
                        AssignedToId = 4,
                        Status = ProjectTaskStatus.InProgress,
                        Priority = TaskPriority.High,
                        CreatedDate = new DateTime(2025, 3, 15),
                        DueDate = new DateTime(2025, 3, 31),
                        EstimatedHours = 30,
                        ActualHours = 15,
                        Tags = new List<string> { "strategy", "content", "planning" }
                    },
                    new ProjectTask
                    {
                        Id = 302,
                        ProjectId = 3,
                        Title = "Configurar campañas Google Ads",
                        Description = "Crear y configurar campañas de Google Ads para productos principales",
                        AssignedToId = 4,
                        Status = ProjectTaskStatus.ToDo,
                        Priority = TaskPriority.High,
                        CreatedDate = new DateTime(2025, 3, 20),
                        DueDate = new DateTime(2025, 4, 10),
                        EstimatedHours = 25,
                        ActualHours = 0,
                        Tags = new List<string> { "google-ads", "sem", "advertising" }
                    },
                    new ProjectTask
                    {
                        Id = 303,
                        ProjectId = 3,
                        Title = "Producción de videos promocionales",
                        Description = "Crear 10 videos cortos para redes sociales",
                        AssignedToId = null,
                        Status = ProjectTaskStatus.ToDo,
                        Priority = TaskPriority.Medium,
                        CreatedDate = new DateTime(2025, 3, 25),
                        DueDate = new DateTime(2025, 5, 1),
                        EstimatedHours = 50,
                        ActualHours = 0,
                        Tags = new List<string> { "video", "social-media", "content" }
                    }
                }
            },
            new Project
            {
                Id = 4,
                Name = "Implementación ERP Cloud",
                Description = "Implementación de sistema ERP en la nube para centralizar operaciones financieras y de recursos humanos.",
                StartDate = new DateTime(2025, 3, 1),
                EndDate = new DateTime(2025, 12, 31),
                Status = ProjectStatus.InProgress,
                ProjectManagerId = 7, // Patricia Moreno Díaz - Coordinadora Operaciones
                Priority = "Alta",
                Budget = 450000m,
                TeamMemberIds = new List<int> { 7, 2, 5 }, // Patricia, Carlos, Elena
                Tasks = new List<ProjectTask>
                {
                    new ProjectTask
                    {
                        Id = 401,
                        ProjectId = 4,
                        Title = "Análisis de requisitos ERP",
                        Description = "Levantar requisitos de todas las áreas involucradas",
                        AssignedToId = 7,
                        Status = ProjectTaskStatus.Completed,
                        Priority = TaskPriority.Critical,
                        CreatedDate = new DateTime(2025, 3, 1),
                        DueDate = new DateTime(2025, 3, 31),
                        CompletedDate = new DateTime(2025, 3, 28),
                        EstimatedHours = 80,
                        ActualHours = 85,
                        Tags = new List<string> { "requirements", "analysis" }
                    },
                    new ProjectTask
                    {
                        Id = 402,
                        ProjectId = 4,
                        Title = "Configuración módulo RRHH",
                        Description = "Configurar el módulo de recursos humanos del ERP",
                        AssignedToId = 2,
                        Status = ProjectTaskStatus.InProgress,
                        Priority = TaskPriority.High,
                        CreatedDate = new DateTime(2025, 4, 1),
                        DueDate = new DateTime(2025, 6, 30),
                        EstimatedHours = 100,
                        ActualHours = 30,
                        Tags = new List<string> { "hr", "configuration", "erp" }
                    },
                    new ProjectTask
                    {
                        Id = 403,
                        ProjectId = 4,
                        Title = "Configuración módulo Finanzas",
                        Description = "Configurar el módulo financiero y contable del ERP",
                        AssignedToId = 5,
                        Status = ProjectTaskStatus.InProgress,
                        Priority = TaskPriority.High,
                        CreatedDate = new DateTime(2025, 4, 1),
                        DueDate = new DateTime(2025, 7, 31),
                        EstimatedHours = 120,
                        ActualHours = 25,
                        Tags = new List<string> { "finance", "accounting", "erp" }
                    },
                    new ProjectTask
                    {
                        Id = 404,
                        ProjectId = 4,
                        Title = "Migración de datos históricos",
                        Description = "Migrar datos históricos del sistema legacy al nuevo ERP",
                        AssignedToId = null,
                        Status = ProjectTaskStatus.Blocked,
                        Priority = TaskPriority.High,
                        CreatedDate = new DateTime(2025, 4, 15),
                        DueDate = new DateTime(2025, 8, 31),
                        EstimatedHours = 150,
                        ActualHours = 0,
                        Tags = new List<string> { "migration", "data", "blocked" }
                    }
                }
            },
            new Project
            {
                Id = 5,
                Name = "Expansión Mercado LATAM",
                Description = "Estrategia de expansión comercial en mercados de Latinoamérica, comenzando por México y Colombia.",
                StartDate = new DateTime(2025, 5, 1),
                EndDate = new DateTime(2025, 12, 31),
                Status = ProjectStatus.Planning,
                ProjectManagerId = 3, // María López Sánchez - Directora Ventas
                Priority = "Alta",
                Budget = 500000m,
                TeamMemberIds = new List<int> { 3, 4 }, // María, Luis
                Tasks = new List<ProjectTask>
                {
                    new ProjectTask
                    {
                        Id = 501,
                        ProjectId = 5,
                        Title = "Estudio de mercado LATAM",
                        Description = "Realizar análisis de mercado en México, Colombia y Argentina",
                        AssignedToId = 3,
                        Status = ProjectTaskStatus.ToDo,
                        Priority = TaskPriority.Critical,
                        CreatedDate = new DateTime(2025, 4, 15),
                        DueDate = new DateTime(2025, 5, 31),
                        EstimatedHours = 60,
                        ActualHours = 0,
                        Tags = new List<string> { "market-research", "latam", "analysis" }
                    },
                    new ProjectTask
                    {
                        Id = 502,
                        ProjectId = 5,
                        Title = "Identificar partners locales",
                        Description = "Contactar y evaluar posibles partners comerciales en cada país",
                        AssignedToId = 3,
                        Status = ProjectTaskStatus.ToDo,
                        Priority = TaskPriority.High,
                        CreatedDate = new DateTime(2025, 5, 1),
                        DueDate = new DateTime(2025, 7, 31),
                        EstimatedHours = 80,
                        ActualHours = 0,
                        Tags = new List<string> { "partnerships", "networking" }
                    }
                }
            }
        };
    }

    public List<Project> GetAll()
    {
        return _projects;
    }

    public Project? GetById(int id)
    {
        return _projects.FirstOrDefault(p => p.Id == id);
    }

    public List<Project> GetByStatus(ProjectStatus status)
    {
        return _projects.Where(p => p.Status == status).ToList();
    }

    public List<Project> GetByProjectManager(int projectManagerId)
    {
        return _projects.Where(p => p.ProjectManagerId == projectManagerId).ToList();
    }

    public List<Project> SearchByName(string name)
    {
        return _projects
            .Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                       p.Description.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<ProjectTask> GetTasksByProject(int projectId)
    {
        var project = GetById(projectId);
        return project?.Tasks ?? new List<ProjectTask>();
    }

    public List<ProjectTask> GetTasksByWorker(int workerId)
    {
        var tasks = new List<ProjectTask>();
        foreach (var project in _projects)
        {
            tasks.AddRange(project.Tasks.Where(t => t.AssignedToId == workerId));
        }
        return tasks;
    }

    public List<ProjectTask> GetTasksByStatus(ProjectTaskStatus status)
    {
        var tasks = new List<ProjectTask>();
        foreach (var project in _projects)
        {
            tasks.AddRange(project.Tasks.Where(t => t.Status == status));
        }
        return tasks;
    }

    public ProjectTask? GetTaskById(int taskId)
    {
        foreach (var project in _projects)
        {
            var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null) return task;
        }
        return null;
    }

    public Dictionary<string, int> GetProjectStatistics()
    {
        return new Dictionary<string, int>
        {
            { "TotalProjects", _projects.Count },
            { "ActiveProjects", _projects.Count(p => p.Status == ProjectStatus.InProgress) },
            { "PlanningProjects", _projects.Count(p => p.Status == ProjectStatus.Planning) },
            { "CompletedProjects", _projects.Count(p => p.Status == ProjectStatus.Completed) },
            { "TotalTasks", _projects.Sum(p => p.Tasks.Count) },
            { "CompletedTasks", _projects.Sum(p => p.Tasks.Count(t => t.Status == ProjectTaskStatus.Completed)) },
            { "InProgressTasks", _projects.Sum(p => p.Tasks.Count(t => t.Status == ProjectTaskStatus.InProgress)) },
            { "BlockedTasks", _projects.Sum(p => p.Tasks.Count(t => t.Status == ProjectTaskStatus.Blocked)) }
        };
    }
}
