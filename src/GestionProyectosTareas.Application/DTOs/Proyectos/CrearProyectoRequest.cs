namespace GestionProyectosTareas.Application.DTOs.Proyectos;

public record CrearProyectoRequest(
    string Nombre,
    string Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFinalizacion
);