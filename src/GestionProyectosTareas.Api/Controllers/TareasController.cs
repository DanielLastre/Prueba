using GestionProyectosTareas.Application.Services;
using GestionProyectosTareas.Application.DTOs.Tareas;
using GestionProyectosTareas.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GestionProyectosTareas.Api.Controllers;

/// <summary>
/// Gestión de tareas: consulta, creación, actualización, cambio de estado y eliminación.
/// </summary>
[ApiController]
[Route("api/v1/tareas")]
public class TareasController : ControllerBase
{
    private readonly ITareaService _tareaService;

    public TareasController(ITareaService tareaService)
    {
        _tareaService = tareaService;
    }

    /// <summary>
    /// Obtiene todas las tareas, con filtros opcionales por estado y prioridad.
    /// </summary>
    /// <param name="estado">Estado de la tarea (opcional).</param>
    /// <param name="prioridad">Prioridad de la tarea (opcional).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de tareas.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TareaResponse>>> ObtenerTodas(
        [FromQuery] EstadoTarea? estado,
        [FromQuery] PrioridadTarea? prioridad,
        CancellationToken cancellationToken)
    {
        var tareas = await _tareaService.ObtenerTodasAsync(estado, prioridad, cancellationToken);
        return Ok(tareas);
    }

    /// <summary>
    /// Obtiene las tareas de un proyecto específico.
    /// </summary>
    /// <param name="proyectoId">Identificador del proyecto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de tareas del proyecto.</returns>
    [HttpGet("proyecto/{proyectoId:guid}")]
    public async Task<ActionResult<IEnumerable<TareaResponse>>> ObtenerPorProyecto(
        Guid proyectoId,
        CancellationToken cancellationToken)
    {
        var tareas = await _tareaService.ObtenerPorProyectoIdAsync(proyectoId, cancellationToken);
        return Ok(tareas);
    }

    /// <summary>
    /// Obtiene las tareas vencidas que aún no están completadas.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de tareas vencidas.</returns>
    [HttpGet("vencidas")]
    public async Task<ActionResult<IEnumerable<TareaResponse>>> ObtenerVencidas(CancellationToken cancellationToken)
    {
        var tareas = await _tareaService.ObtenerVencidasNoCompletadasAsync(cancellationToken);
        return Ok(tareas);
    }

    /// <summary>
    /// Obtiene una tarea por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la tarea.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>La tarea solicitada.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TareaResponse>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var tarea = await _tareaService.ObtenerPorIdAsync(id, cancellationToken);
        return Ok(tarea);
    }

    /// <summary>
    /// Crea una nueva tarea en un proyecto.
    /// </summary>
    /// <param name="request">Datos de la tarea a crear.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>La tarea creada.</returns>
    [HttpPost]
    public async Task<ActionResult<TareaResponse>> Crear(
        [FromBody] CrearTareaRequest request,
        CancellationToken cancellationToken)
    {
        var creada = await _tareaService.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    /// <summary>
    /// Actualiza una tarea existente.
    /// </summary>
    /// <param name="id">Identificador de la tarea.</param>
    /// <param name="request">Datos actualizados de la tarea.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>La tarea actualizada.</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TareaResponse>> Actualizar(
        Guid id,
        [FromBody] ActualizarTareaRequest request,
        CancellationToken cancellationToken)
    {
        var actualizada = await _tareaService.ActualizarAsync(id, request, cancellationToken);
        return Ok(actualizada);
    }

    /// <summary>
    /// Cambia el estado de una tarea.
    /// </summary>
    /// <param name="id">Identificador de la tarea.</param>
    /// <param name="request">Nuevo estado de la tarea.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>La tarea con el estado actualizado.</returns>
    [HttpPatch("{id:guid}/estado")]
    public async Task<ActionResult<TareaResponse>> CambiarEstado(
        Guid id,
        [FromBody] CambiarEstadoTareaRequest request,
        CancellationToken cancellationToken)
    {
        var actualizada = await _tareaService.CambiarEstadoAsync(id, request, cancellationToken);
        return Ok(actualizada);
    }

    /// <summary>
    /// Elimina una tarea.
    /// </summary>
    /// <param name="id">Identificador de la tarea.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Sin contenido.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        await _tareaService.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}