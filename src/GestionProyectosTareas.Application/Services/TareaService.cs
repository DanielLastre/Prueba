using GestionProyectosTareas.Application.DTOs.Tareas;
using GestionProyectosTareas.Application.Exceptions;
using GestionProyectosTareas.Application.Interfaces;
using GestionProyectosTareas.Domain.Entities;
using GestionProyectosTareas.Domain.Enums;

namespace GestionProyectosTareas.Application.Services;

public class TareaService : ITareaService
{
    private readonly ITareaRepository _tareaRepository;
    private readonly IProyectoRepository _proyectoRepository;

    public TareaService(ITareaRepository tareaRepository, IProyectoRepository proyectoRepository)
    {
        _tareaRepository = tareaRepository;
        _proyectoRepository = proyectoRepository;
    }

    public async Task<IEnumerable<TareaResponse>> ObtenerTodasAsync(EstadoTarea? estado = null, PrioridadTarea? prioridad = null, CancellationToken cancellationToken = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var tareas = await _tareaRepository.ObtenerTodasAsync(estado, prioridad, cancellationToken);
        return tareas.Select(t => MapToResponse(t, hoy));
    }

    public async Task<IEnumerable<TareaResponse>> ObtenerPorProyectoIdAsync(Guid proyectoId, CancellationToken cancellationToken = default)
    {
        if (!await _proyectoRepository.ExisteAsync(proyectoId, cancellationToken))
            throw new NotFoundException(nameof(Proyecto), proyectoId);

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var tareas = await _tareaRepository.ObtenerPorProyectoIdAsync(proyectoId, cancellationToken);
        return tareas.Select(t => MapToResponse(t, hoy));
    }

    public async Task<IEnumerable<TareaResponse>> ObtenerVencidasNoCompletadasAsync(CancellationToken cancellationToken = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var tareas = await _tareaRepository.ObtenerVencidasNoCompletadasAsync(hoy, cancellationToken);
        return tareas.Select(t => MapToResponse(t, hoy));
    }

    public async Task<TareaResponse> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tarea = await _tareaRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Tarea), id);

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        return MapToResponse(tarea, hoy);
    }

    public async Task<TareaResponse> CrearAsync(CrearTareaRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _proyectoRepository.ExisteAsync(request.ProyectoId, cancellationToken))
            throw new NotFoundException(nameof(Proyecto), request.ProyectoId);

        var tarea = new Tarea(
            Guid.NewGuid(),
            request.Titulo,
            request.Descripcion,
            request.Prioridad,
            request.FechaLimite,
            request.ProyectoId
        );

        await _tareaRepository.AgregarAsync(tarea, cancellationToken);
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        return MapToResponse(tarea, hoy);
    }

    public async Task<TareaResponse> ActualizarAsync(Guid id, ActualizarTareaRequest request, CancellationToken cancellationToken = default)
    {
        var tarea = await _tareaRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Tarea), id);

        tarea.Actualizar(
            request.Titulo,
            request.Descripcion,
            request.Prioridad,
            request.FechaLimite
        );

        await _tareaRepository.ActualizarAsync(tarea, cancellationToken);
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        return MapToResponse(tarea, hoy);
    }

    public async Task<TareaResponse> CambiarEstadoAsync(Guid id, CambiarEstadoTareaRequest request, CancellationToken cancellationToken = default)
    {
        var tarea = await _tareaRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Tarea), id);

        tarea.CambiarEstado(request.Estado);

        await _tareaRepository.ActualizarAsync(tarea, cancellationToken);
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        return MapToResponse(tarea, hoy);
    }

    public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tarea = await _tareaRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Tarea), id);

        await _tareaRepository.EliminarAsync(tarea, cancellationToken);
    }

    private static TareaResponse MapToResponse(Tarea tarea, DateOnly hoy) =>
        new(
            tarea.Id,
            tarea.Titulo,
            tarea.Descripcion,
            tarea.Estado,
            tarea.Prioridad,
            tarea.FechaLimite,
            tarea.EstaVencida(hoy),
            tarea.ProyectoId
        );
}