using GestionProyectosTareas.Domain.Enums;

namespace GestionProyectosTareas.Application.DTOs.Tareas;

public record TareaResponse(
    Guid Id,
    string Titulo,
    string Descripcion,
    EstadoTarea Estado,
    PrioridadTarea Prioridad,
    DateOnly FechaLimite,
    bool EstaVencida,
    Guid ProyectoId
);