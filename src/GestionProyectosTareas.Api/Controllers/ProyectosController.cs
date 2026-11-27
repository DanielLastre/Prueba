
using GestionProyectosTareas.Application.DTOs.Proyectos;
using GestionProyectosTareas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionProyectosTareas.Api.Controllers;

/// <summary>
/// Gestión de proyectos: consulta, creación, actualización, eliminación y avance.
/// </summary>
[ApiController]
[Route("api/v1/proyectos")]
public class ProyectosController : ControllerBase
{
    private readonly IProyectoService _proyectoService;

    public ProyectosController(IProyectoService proyectoService)
    {
        _proyectoService = proyectoService;
    }

    /// <summary>
    /// Obtiene la lista de todos los proyectos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de proyectos.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProyectoResponse>>> ObtenerTodos(CancellationToken cancellationToken)
    {
        var proyectos = await _proyectoService.ObtenerTodosAsync(cancellationToken);
        return Ok(proyectos);
    }

    /// <summary>
    /// Obtiene un proyecto por su identificador.
    /// </summary>
    /// <param name="id">Identificador del proyecto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El proyecto solicitado.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProyectoResponse>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var proyecto = await _proyectoService.ObtenerPorIdAsync(id, cancellationToken);
        return Ok(proyecto);
    }

    /// <summary>
    /// Obtiene el porcentaje de avance de un proyecto según sus tareas completadas.
    /// </summary>
    /// <param name="id">Identificador del proyecto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Porcentaje de avance del proyecto.</returns>
    [HttpGet("{id:guid}/avance")]
    public async Task<ActionResult<AvanceProyectoResponse>> ObtenerAvance(Guid id, CancellationToken cancellationToken)
    {
        var avance = await _proyectoService.ObtenerAvanceAsync(id, cancellationToken);
        return Ok(avance);
    }

    /// <summary>
    /// Crea un nuevo proyecto.
    /// </summary>
    /// <param name="request">Datos del proyecto a crear.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El proyecto creado.</returns>
    [HttpPost]
    public async Task<ActionResult<ProyectoResponse>> Crear(
        [FromBody] CrearProyectoRequest request,
        CancellationToken cancellationToken)
    {
        var creado = await _proyectoService.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    /// <summary>
    /// Actualiza un proyecto existente.
    /// </summary>
    /// <param name="id">Identificador del proyecto.</param>
    /// <param name="request">Datos actualizados del proyecto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El proyecto actualizado.</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProyectoResponse>> Actualizar(
        Guid id,
        [FromBody] ActualizarProyectoRequest request,
        CancellationToken cancellationToken)
    {
        var actualizado = await _proyectoService.ActualizarAsync(id, request, cancellationToken);
        return Ok(actualizado);
    }

    /// <summary>
    /// Elimina un proyecto. Falla si tiene tareas pendientes.
    /// </summary>
    /// <param name="id">Identificador del proyecto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Sin contenido.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        await _proyectoService.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}