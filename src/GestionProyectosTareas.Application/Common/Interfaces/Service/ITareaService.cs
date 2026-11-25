using GestionProyectosTareas.Application.DTOs.Tareas;
using GestionProyectosTareas.Domain.Enums;

namespace GestionProyectosTareas.Application.Services;

public interface ITareaService
{
    Task<IEnumerable<TareaResponse>> ObtenerTodasAsync(EstadoTarea? estado = null, PrioridadTarea? prioridad = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<TareaResponse>> ObtenerPorProyectoIdAsync(Guid proyectoId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TareaResponse>> ObtenerVencidasNoCompletadasAsync(CancellationToken cancellationToken = default);
    Task<TareaResponse> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TareaResponse> CrearAsync(CrearTareaRequest request, CancellationToken cancellationToken = default);
    Task<TareaResponse> ActualizarAsync(Guid id, ActualizarTareaRequest request, CancellationToken cancellationToken = default);
    Task<TareaResponse> CambiarEstadoAsync(Guid id, CambiarEstadoTareaRequest request, CancellationToken cancellationToken = default);
    Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);
}