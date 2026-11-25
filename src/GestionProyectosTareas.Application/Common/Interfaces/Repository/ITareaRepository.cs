using GestionProyectosTareas.Domain.Entities;
using GestionProyectosTareas.Domain.Enums;

namespace GestionProyectosTareas.Application.Interfaces;

public interface ITareaRepository
{
    Task<IEnumerable<Tarea>> ObtenerTodasAsync(EstadoTarea? estado = null, PrioridadTarea? prioridad = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tarea>> ObtenerPorProyectoIdAsync(Guid proyectoId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tarea>> ObtenerVencidasNoCompletadasAsync(DateOnly fechaActual, CancellationToken cancellationToken = default);
    Task<Tarea?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AgregarAsync(Tarea tarea, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Tarea tarea, CancellationToken cancellationToken = default);
    Task EliminarAsync(Tarea tarea, CancellationToken cancellationToken = default);
}