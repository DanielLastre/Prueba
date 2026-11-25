using GestionProyectosTareas.Application.Interfaces;
using GestionProyectosTareas.Domain.Entities;
using GestionProyectosTareas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionProyectosTareas.Infrastructure.Repositories;

public class ProyectoRepository : IProyectoRepository
{
    private readonly AppDbContext _context;

    public ProyectoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Proyecto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Proyectos
            .AsNoTracking()
            .Include(p => p.Tareas)
            .OrderBy(p => p.FechaInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<Proyecto?> ObtenerPorIdAsync(Guid id, bool incluirTareas = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Proyecto> query = _context.Proyectos;

        if (incluirTareas)
            query = query.Include(p => p.Tareas);

        return await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Proyectos
            .AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AgregarAsync(Proyecto proyecto, CancellationToken cancellationToken = default)
    {
        await _context.Proyectos.AddAsync(proyecto, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(Proyecto proyecto, CancellationToken cancellationToken = default)
    {
        _context.Proyectos.Update(proyecto);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task EliminarAsync(Proyecto proyecto, CancellationToken cancellationToken = default)
    {
        _context.Proyectos.Remove(proyecto);
        await _context.SaveChangesAsync(cancellationToken);
    }
}