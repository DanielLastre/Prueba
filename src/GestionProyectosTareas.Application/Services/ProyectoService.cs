using GestionProyectosTareas.Application.DTOs.Proyectos;
using GestionProyectosTareas.Application.Exceptions;
using GestionProyectosTareas.Application.Interfaces;
using GestionProyectosTareas.Domain.Entities;
using GestionProyectosTareas.Domain.Enums;

namespace GestionProyectosTareas.Application.Services;

public class ProyectoService : IProyectoService
{
    private readonly IProyectoRepository _proyectoRepository;

    public ProyectoService(IProyectoRepository proyectoRepository)
    {
        _proyectoRepository = proyectoRepository;
    }

    public async Task<IEnumerable<ProyectoResponse>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var proyectos = await _proyectoRepository.ObtenerTodosAsync(cancellationToken);
        return proyectos.Select(MapToResponse);
    }

    public async Task<ProyectoResponse> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var proyecto = await _proyectoRepository.ObtenerPorIdAsync(id, incluirTareas: true, cancellationToken)
            ?? throw new NotFoundException(nameof(Proyecto), id);

        return MapToResponse(proyecto);
    }

    public async Task<AvanceProyectoResponse> ObtenerAvanceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var proyecto = await _proyectoRepository.ObtenerPorIdAsync(id, incluirTareas: true, cancellationToken)
            ?? throw new NotFoundException(nameof(Proyecto), id);

        var total = proyecto.Tareas.Count;
        var completadas = proyecto.Tareas.Count(t => t.Estado == EstadoTarea.Completada);
        var enProgreso = proyecto.Tareas.Count(t => t.Estado == EstadoTarea.EnProgreso);
        var pendientes = proyecto.Tareas.Count(t => t.Estado == EstadoTarea.Pendiente);

        return new AvanceProyectoResponse(
            proyecto.Id,
            proyecto.Nombre,
            total,
            completadas,
            enProgreso,
            pendientes,
            proyecto.CalcularPorcentajeAvance()
        );
    }

    public async Task<ProyectoResponse> CrearAsync(CrearProyectoRequest request, CancellationToken cancellationToken = default)
    {
        var proyecto = new Proyecto(
            Guid.NewGuid(),
            request.Nombre,
            request.Descripcion,
            request.FechaInicio,
            request.FechaFinalizacion
        );

        await _proyectoRepository.AgregarAsync(proyecto, cancellationToken);
        return MapToResponse(proyecto);
    }

    public async Task<ProyectoResponse> ActualizarAsync(Guid id, ActualizarProyectoRequest request, CancellationToken cancellationToken = default)
    {
        var proyecto = await _proyectoRepository.ObtenerPorIdAsync(id, incluirTareas: true, cancellationToken)
            ?? throw new NotFoundException(nameof(Proyecto), id);

        proyecto.Actualizar(
            request.Nombre,
            request.Descripcion,
            request.FechaInicio,
            request.FechaFinalizacion
        );

        await _proyectoRepository.ActualizarAsync(proyecto, cancellationToken);
        return MapToResponse(proyecto);
    }

    public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Importante incluir las tareas para validar si tiene tareas pendientes
        var proyecto = await _proyectoRepository.ObtenerPorIdAsync(id, incluirTareas: true, cancellationToken)
            ?? throw new NotFoundException(nameof(Proyecto), id);

        proyecto.ValidarEliminacion();
        await _proyectoRepository.EliminarAsync(proyecto, cancellationToken);
    }

    private static ProyectoResponse MapToResponse(Proyecto proyecto) =>
        new(
            proyecto.Id,
            proyecto.Nombre,
            proyecto.Descripcion,
            proyecto.FechaInicio,
            proyecto.FechaFinalizacion,
            proyecto.Tareas.Count,
            proyecto.CalcularPorcentajeAvance()
        );
}