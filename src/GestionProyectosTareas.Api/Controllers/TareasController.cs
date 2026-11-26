using GestionProyectosTareas.Application.Services;
using GestionProyectosTareas.Application.DTOs.Tareas;
using GestionProyectosTareas.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionProyectosTareas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class TareasController : ControllerBase
{
    private readonly ITareaService _tareaService;

    public TareasController(ITareaService tareaService)
    {
        _tareaService = tareaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TareaResponse>>> ObtenerTodas(
        [FromQuery] EstadoTarea? estado,
        [FromQuery] PrioridadTarea? prioridad,
        CancellationToken cancellationToken)
    {
        var tareas = await _tareaService.ObtenerTodasAsync(estado, prioridad, cancellationToken);
        return Ok(tareas);
    }

    [HttpGet("proyecto/{proyectoId:guid}")]
    public async Task<ActionResult<IEnumerable<TareaResponse>>> ObtenerPorProyecto(
        Guid proyectoId,
        CancellationToken cancellationToken)
    {
        var tareas = await _tareaService.ObtenerPorProyectoIdAsync(proyectoId, cancellationToken);
        return Ok(tareas);
    }

    [HttpGet("vencidas")]
    public async Task<ActionResult<IEnumerable<TareaResponse>>> ObtenerVencidas(CancellationToken cancellationToken)
    {
        var tareas = await _tareaService.ObtenerVencidasNoCompletadasAsync(cancellationToken);
        return Ok(tareas);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TareaResponse>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var tarea = await _tareaService.ObtenerPorIdAsync(id, cancellationToken);
        return Ok(tarea);
    }

    [HttpPost]
    public async Task<ActionResult<TareaResponse>> Crear(
        [FromBody] CrearTareaRequest request,
        CancellationToken cancellationToken)
    {
        var creada = await _tareaService.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TareaResponse>> Actualizar(
        Guid id,
        [FromBody] ActualizarTareaRequest request,
        CancellationToken cancellationToken)
    {
        var actualizada = await _tareaService.ActualizarAsync(id, request, cancellationToken);
        return Ok(actualizada);
    }

    [HttpPatch("{id:guid}/estado")]
    public async Task<ActionResult<TareaResponse>> CambiarEstado(
        Guid id,
        [FromBody] CambiarEstadoTareaRequest request,
        CancellationToken cancellationToken)
    {
        var actualizada = await _tareaService.CambiarEstadoAsync(id, request, cancellationToken);
        return Ok(actualizada);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        await _tareaService.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}