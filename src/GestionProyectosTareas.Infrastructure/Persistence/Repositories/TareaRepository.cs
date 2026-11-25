using GestionProyectosTareas.Application.Interfaces;
using GestionProyectosTareas.Domain.Entities;
using GestionProyectosTareas.Domain.Enums;
using GestionProyectosTareas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionProyectosTareas.Infrastructure.Repositories;

public class TareaRepository : ITareaRepository
{
    private readonly AppDbContext _context;

    public TareaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Tarea>> ObtenerTodasAsync(
        EstadoTarea? estado = null,
        PrioridadTarea? prioridad = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Tarea> query = _context.Tareas.AsNoTracking();

        if (estado.HasValue)
            query = query.Where(t => t.Estado == estado.Value);

        if (prioridad.HasValue)
            query = query.Where(t => t.Prioridad == prioridad.Value);

        return await query
            .OrderBy(t => t.FechaLimite)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tarea>> ObtenerPorProyectoIdAsync(Guid proyectoId, CancellationToken cancellationToken = default)
    {
        return await _context.Tareas
            .AsNoTracking()
            .Where(t => t.ProyectoId == proyectoId)
            .OrderBy(t => t.FechaLimite)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tarea>> ObtenerVencidasNoCompletadasAsync(DateOnly fechaActual, CancellationToken cancellationToken = default)
    {
        return await _context.Tareas
            .AsNoTracking()
            .Where(t => t.FechaLimite < fechaActual && t.Estado != EstadoTarea.Completada)
            .OrderBy(t => t.FechaLimite)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tarea?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tareas.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AgregarAsync(Tarea tarea, CancellationToken cancellationToken = default)
    {
        await _context.Tareas.AddAsync(tarea, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(Tarea tarea, CancellationToken cancellationToken = default)
    {
        _context.Tareas.Update(tarea);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task EliminarAsync(Tarea tarea, CancellationToken cancellationToken = default)
    {
        _context.Tareas.Remove(tarea);
        await _context.SaveChangesAsync(cancellationToken);
    }
}