namespace GestionProyectosTareas.Application.DTOs.Proyectos;

public record ActualizarProyectoRequest(
    string Nombre,
    string Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFinalizacion
);