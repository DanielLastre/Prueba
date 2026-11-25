namespace GestionProyectosTareas.Application.DTOs.Proyectos;

public record ProyectoResponse(
    Guid Id,
    string Nombre,
    string Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFinalizacion,
    int TotalTareas,
    decimal PorcentajeAvance
);