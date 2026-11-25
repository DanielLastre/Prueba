using GestionProyectosTareas.Application.DTOs.Proyectos;

namespace GestionProyectosTareas.Application.Services;

public interface IProyectoService
{
    Task<IEnumerable<ProyectoResponse>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<ProyectoResponse> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AvanceProyectoResponse> ObtenerAvanceAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProyectoResponse> CrearAsync(CrearProyectoRequest request, CancellationToken cancellationToken = default);
    Task<ProyectoResponse> ActualizarAsync(Guid id, ActualizarProyectoRequest request, CancellationToken cancellationToken = default);
    Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);
}