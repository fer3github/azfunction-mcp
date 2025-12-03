using McpAzFunction.Models;

namespace McpAzFunction.Data;

public class WorkerRepository
{
    private readonly List<Worker> _workers;

    public WorkerRepository()
    {
        _workers = new List<Worker>
        {
            new Worker
            {
                Id = 1,
                Nombre = "Carlos Martínez López",
                Departamento = "DAR",
                Puesto = "Chief Arquitect",
                Email = "carlos.martinez@empresa.com",
                Telefono = "+34 912 345 001",
                Ubicacion = "Valencia, España"
            },
            new Worker
            {
                Id = 2,
                Nombre = "Luis Fernando García",
                Departamento = "DAR",
                Puesto = "Chief Arquitect",
                Email = "luis.garcia@empresa.com",
                Telefono = "+34 912 345 002",
                Ubicacion = "Valencia, España"
            },
            new Worker
            {
                Id = 3,
                Nombre = "Ricardo Sánchez Torres",
                Departamento = "DAR",
                Puesto = "Project Manager",
                Email = "ricardo.sanchez@empresa.com",
                Telefono = "+34 912 345 003",
                Ubicacion = "Valencia, España"
            },
            new Worker
            {
                Id = 4,
                Nombre = "Miguel Ángel Ruiz",
                Departamento = "DAR",
                Puesto = "Manager",
                Email = "miguel.ruiz@empresa.com",
                Telefono = "+34 912 345 004",
                Ubicacion = "Valencia, España"
            },
            new Worker
            {
                Id = 5,
                Nombre = "Antonio Fernández Vega",
                Departamento = "DAR",
                Puesto = "Director",
                Email = "antonio.fernandez@empresa.com",
                Telefono = "+34 912 345 005",
                Ubicacion = "Valencia, España"
            },
            new Worker
            {
                Id = 6,
                Nombre = "Alberto Ramírez Cruz",
                Departamento = "DAR",
                Puesto = "Junior Engineer",
                Email = "alberto.ramirez@empresa.com",
                Telefono = "+34 912 345 006",
                Ubicacion = "Valencia, España"
            },
            new Worker
            {
                Id = 7,
                Nombre = "Patricia Moreno Díaz",
                Departamento = "Operaciones",
                Puesto = "Coordinadora de Operaciones",
                Email = "patricia.moreno@empresa.com",
                Telefono = "+34 912 345 007",
                Ubicacion = "Bilbao, España"
            },
            new Worker
            {
                Id = 8,
                Nombre = "David Silva Romero",
                Departamento = "IT - Desarrollo",
                Puesto = "Tech Lead Frontend",
                Email = "david.silva@empresa.com",
                Telefono = "+34 912 345 008",
                Ubicacion = "Madrid, España"
            }
        };
    }

    public Worker? GetById(int id)
    {
        return _workers.FirstOrDefault(w => w.Id == id);
    }

    public List<Worker> GetAll()
    {
        return _workers.ToList();
    }

    public List<Worker> SearchByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new List<Worker>();

        var searchTerm = name.ToLower();
        return _workers
            .Where(w => w.Nombre.ToLower().Contains(searchTerm))
            .ToList();
    }

    public List<Worker> GetByDepartment(string department)
    {
        if (string.IsNullOrWhiteSpace(department))
            return new List<Worker>();

        var searchTerm = department.ToLower();
        return _workers
            .Where(w => w.Departamento.ToLower().Contains(searchTerm))
            .ToList();
    }
}
