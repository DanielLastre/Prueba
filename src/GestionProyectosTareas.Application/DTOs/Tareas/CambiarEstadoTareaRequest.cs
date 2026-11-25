using GestionProyectosTareas.Domain.Enums;

namespace GestionProyectosTareas.Application.DTOs.Tareas;

public record CambiarEstadoTareaRequest(
    EstadoTarea Estado
);