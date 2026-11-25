namespace GestionProyectosTareas.Application.DTOs.Proyectos;

public record AvanceProyectoResponse(
    Guid ProyectoId,
    string NombreProyecto,
    int TotalTareas,
    int TareasCompletadas,
    int TareasEnProgreso,
    int TareasPendientes,
    decimal PorcentajeAvance
);