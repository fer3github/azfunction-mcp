namespace McpAzFunction.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ProjectStatus Status { get; set; }
    public int ProjectManagerId { get; set; } // Referencias Worker
    public string Priority { get; set; } = string.Empty; // "Alta", "Media", "Baja"
    public decimal Budget { get; set; }
    public List<int> TeamMemberIds { get; set; } = new List<int>();
    public List<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
}

public enum ProjectStatus
{
    Planning,      // Planificación
    InProgress,    // En progreso
    OnHold,        // En pausa
    Completed,     // Completado
    Cancelled      // Cancelado
}
