
using GestionProyectosTareas.Application.DTOs.Proyectos;
using GestionProyectosTareas.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionProyectosTareas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class ProyectosController : ControllerBase
{
    private readonly IProyectoService _proyectoService;

    public ProyectosController(IProyectoService proyectoService)
    {
        _proyectoService = proyectoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProyectoResponse>>> ObtenerTodos(CancellationToken cancellationToken)
    {
        var proyectos = await _proyectoService.ObtenerTodosAsync(cancellationToken);
        return Ok(proyectos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProyectoResponse>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var proyecto = await _proyectoService.ObtenerPorIdAsync(id, cancellationToken);
        return Ok(proyecto);
    }

    [HttpGet("{id:guid}/avance")]
    public async Task<ActionResult<AvanceProyectoResponse>> ObtenerAvance(Guid id, CancellationToken cancellationToken)
    {
        var avance = await _proyectoService.ObtenerAvanceAsync(id, cancellationToken);
        return Ok(avance);
    }

    [HttpPost]
    public async Task<ActionResult<ProyectoResponse>> Crear(
        [FromBody] CrearProyectoRequest request,
        CancellationToken cancellationToken)
    {
        var creado = await _proyectoService.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProyectoResponse>> Actualizar(
        Guid id,
        [FromBody] ActualizarProyectoRequest request,
        CancellationToken cancellationToken)
    {
        var actualizado = await _proyectoService.ActualizarAsync(id, request, cancellationToken);
        return Ok(actualizado);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        await _proyectoService.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}