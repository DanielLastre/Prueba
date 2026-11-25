using GestionProyectosTareas.Domain.Enums;
using GestionProyectosTareas.Domain.Exceptions;

namespace GestionProyectosTareas.Domain.Entities;

public class Proyecto
{
    private readonly List<Tarea> _tareas = [];

    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public DateOnly FechaInicio { get; private set; }
    public DateOnly FechaFinalizacion { get; private set; }
    public IReadOnlyCollection<Tarea> Tareas => _tareas.AsReadOnly();

    private Proyecto() { }

    public Proyecto(
        Guid id,
        string nombre,
        string descripcion,
        DateOnly fechaInicio,
        DateOnly fechaFinalizacion)
    {
        if (id == Guid.Empty)
            throw new DomainException("El ID del proyecto no puede estar vacío.");

        ValidarDatos(nombre, fechaInicio, fechaFinalizacion);

        Id = id;
        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim() ?? string.Empty;
        FechaInicio = fechaInicio;
        FechaFinalizacion = fechaFinalizacion;
    }

    public void Actualizar(
        string nombre,
        string descripcion,
        DateOnly fechaInicio,
        DateOnly fechaFinalizacion)
    {
        ValidarDatos(nombre, fechaInicio, fechaFinalizacion);

        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim() ?? string.Empty;
        FechaInicio = fechaInicio;
        FechaFinalizacion = fechaFinalizacion;
    }

    public void AgregarTarea(Tarea tarea)
    {
        ArgumentNullException.ThrowIfNull(tarea);
        _tareas.Add(tarea);
    }

    public decimal CalcularPorcentajeAvance()
    {
        if (_tareas.Count == 0)
            return 0m;

        int completadas = _tareas.Count(t => t.Estado == EstadoTarea.Completada);
        decimal porcentaje = (decimal)completadas / _tareas.Count * 100m;
        return Math.Round(porcentaje, 2);
    }

    public void ValidarEliminacion()
    {
        int pendientes = _tareas.Count(t => t.Estado == EstadoTarea.Pendiente);
        if (pendientes > 0)
            throw new ProyectoConTareasPendientesException(Id, pendientes);
    }

    private static void ValidarDatos(string nombre, DateOnly fechaInicio, DateOnly fechaFinalizacion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del proyecto es obligatorio.");

        if (nombre.Trim().Length > 150)
            throw new DomainException("El nombre del proyecto no puede superar los 150 caracteres.");

        if (fechaFinalizacion < fechaInicio)
            throw new DomainException("La fecha de finalización no puede ser anterior a la fecha de inicio.");
    }
}