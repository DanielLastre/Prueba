using GestionProyectosTareas.Domain.Enums;
using GestionProyectosTareas.Domain.Exceptions;

namespace GestionProyectosTareas.Domain.Entities;

public class Tarea
{
    public Guid Id { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public EstadoTarea Estado { get; private set; }
    public PrioridadTarea Prioridad { get; private set; }
    public DateOnly FechaLimite { get; private set; }
    public Guid ProyectoId { get; private set; }
    public Proyecto? Proyecto { get; private set; }

    private Tarea() { }

    public Tarea(
        Guid id,
        string titulo,
        string descripcion,
        PrioridadTarea prioridad,
        DateOnly fechaLimite,
        Guid proyectoId,
        EstadoTarea estado = EstadoTarea.Pendiente)
    {
        if (id == Guid.Empty)
            throw new DomainException("El ID de la tarea no puede estar vacío.");

        if (proyectoId == Guid.Empty)
            throw new DomainException("El ID del proyecto es obligatorio para crear una tarea.");

        ValidarTitulo(titulo);

        Id = id;
        Titulo = titulo.Trim();
        Descripcion = descripcion?.Trim() ?? string.Empty;
        Prioridad = prioridad;
        FechaLimite = fechaLimite;
        ProyectoId = proyectoId;
        Estado = estado;
    }

    public void Actualizar(
        string titulo,
        string descripcion,
        PrioridadTarea prioridad,
        DateOnly fechaLimite)
    {
        ValidarTitulo(titulo);

        Titulo = titulo.Trim();
        Descripcion = descripcion?.Trim() ?? string.Empty;
        Prioridad = prioridad;
        FechaLimite = fechaLimite;
    }

    public void CambiarEstado(EstadoTarea nuevoEstado)
    {
        if (!Enum.IsDefined(nuevoEstado))
            throw new DomainException("El estado de la tarea no es válido.");

        Estado = nuevoEstado;
    }

    public bool EstaVencida(DateOnly fechaActual)
    {
        return FechaLimite < fechaActual && Estado != EstadoTarea.Completada;
    }

    private static void ValidarTitulo(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new DomainException("El título de la tarea es obligatorio.");

        if (titulo.Trim().Length > 150)
            throw new DomainException("El título de la tarea no puede superar los 150 caracteres.");
    }
}