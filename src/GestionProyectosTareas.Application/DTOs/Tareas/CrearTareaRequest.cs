using GestionProyectosTareas.Domain.Enums;

namespace GestionProyectosTareas.Application.DTOs.Tareas;

public record CrearTareaRequest(
    string Titulo,
    string Descripcion,
    PrioridadTarea Prioridad,
    DateOnly FechaLimite,
    Guid ProyectoId
);