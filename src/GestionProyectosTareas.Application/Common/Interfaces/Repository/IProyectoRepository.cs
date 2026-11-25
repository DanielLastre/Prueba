using GestionProyectosTareas.Domain.Entities;

namespace GestionProyectosTareas.Application.Interfaces;

public interface IProyectoRepository
{
    Task<IEnumerable<Proyecto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<Proyecto?> ObtenerPorIdAsync(Guid id, bool incluirTareas = false, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
    Task AgregarAsync(Proyecto proyecto, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Proyecto proyecto, CancellationToken cancellationToken = default);
    Task EliminarAsync(Proyecto proyecto, CancellationToken cancellationToken = default);
}