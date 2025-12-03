using McpAzFunction.Data;
using McpAzFunction.Models;
using System.Text;

namespace McpAzFunction.Functions;

public class WorkerTools
{
    private readonly WorkerRepository _repository;

    public WorkerTools(WorkerRepository repository)
    {
        _repository = repository;
    }

    public string GetWorkerById(int id)
    {
        var worker = _repository.GetById(id);
        
        if (worker == null)
        {
            return $"No se encontró ningún trabajador con ID {id}.";
        }

        return FormatWorker(worker);
    }

    public string GetAllWorkers()
    {
        var workers = _repository.GetAll();
        
        if (workers.Count == 0)
        {
            return "No hay trabajadores registrados en el sistema.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Lista completa de trabajadores ({workers.Count} total):");
        sb.AppendLine();

        foreach (var worker in workers)
        {
            sb.AppendLine(FormatWorkerShort(worker));
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    public string SearchWorkersByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Por favor, proporciona un nombre para buscar.";
        }

        var workers = _repository.SearchByName(name);
        
        if (workers.Count == 0)
        {
            return $"No se encontraron trabajadores con el nombre '{name}'.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Resultados de búsqueda para '{name}' ({workers.Count} encontrado{(workers.Count > 1 ? "s" : "")}):");
        sb.AppendLine();

        foreach (var worker in workers)
        {
            sb.AppendLine(FormatWorker(worker));
            if (workers.Count > 1 && worker != workers.Last())
            {
                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine();
            }
        }

        return sb.ToString().TrimEnd();
    }

    private string FormatWorker(Worker worker)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"👤 {worker.Nombre}");
        sb.AppendLine($"📋 Puesto: {worker.Puesto}");
        sb.AppendLine($"🏢 Departamento: {worker.Departamento}");
        sb.AppendLine($"📧 Email: {worker.Email}");
        sb.AppendLine($"📱 Teléfono: {worker.Telefono}");
        sb.AppendLine($"📍 Ubicación: {worker.Ubicacion}");
        sb.AppendLine($"🆔 ID: {worker.Id}");
        return sb.ToString().TrimEnd();
    }

    private string FormatWorkerShort(Worker worker)
    {
        return $"• [ID:{worker.Id}] {worker.Nombre} - {worker.Puesto} ({worker.Departamento})";
    }
}
