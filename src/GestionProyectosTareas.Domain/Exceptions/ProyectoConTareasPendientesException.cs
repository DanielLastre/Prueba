namespace GestionProyectosTareas.Domain.Exceptions;

public sealed class ProyectoConTareasPendientesException : DomainException
{
    public ProyectoConTareasPendientesException(Guid proyectoId, int cantidadTareasPendientes)
        : base($"No se puede eliminar el proyecto '{proyectoId}' porque tiene {cantidadTareasPendientes} tarea(s) pendiente(s).")
    {
    }
}