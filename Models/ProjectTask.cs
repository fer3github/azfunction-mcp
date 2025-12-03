namespace McpAzFunction.Models;

public class ProjectTask
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? AssignedToId { get; set; } // Referencias Worker
    public ProjectTaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int EstimatedHours { get; set; }
    public int ActualHours { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
}

public enum ProjectTaskStatus
{
    ToDo,          // Por hacer
    InProgress,    // En progreso
    InReview,      // En revisión
    Blocked,       // Bloqueada
    Completed,     // Completada
    Cancelled      // Cancelada
}

public enum TaskPriority
{
    Low,           // Baja
    Medium,        // Media
    High,          // Alta
    Critical       // Crítica
}
