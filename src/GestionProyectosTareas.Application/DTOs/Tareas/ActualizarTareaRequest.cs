using GestionProyectosTareas.Domain.Enums;

namespace GestionProyectosTareas.Application.DTOs.Tareas;

public record ActualizarTareaRequest(
    string Titulo,
    string Descripcion,
    PrioridadTarea Prioridad,
    DateOnly FechaLimite
);