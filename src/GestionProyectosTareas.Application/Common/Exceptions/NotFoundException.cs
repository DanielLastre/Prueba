namespace GestionProyectosTareas.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entidad, Guid id)
        : base($"No se encontró '{entidad}' con el ID '{id}'.")
    {
    }
}