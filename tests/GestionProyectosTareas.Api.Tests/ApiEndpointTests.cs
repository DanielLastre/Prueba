using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace GestionProyectosTareas.Api.Tests;

public class ApiEndpointTests : IClassFixture<ApiPostgreSqlFixture>
{
    private readonly HttpClient _client;

    public ApiEndpointTests(ApiPostgreSqlFixture fixture)
    {
        _client = fixture.Client;
    }

    private async Task<Guid> CrearProyectoAsync(string nombre = "Proyecto Test")
    {
        var response = await _client.PostAsJsonAsync("/api/v1/proyectos", new
        {
            nombre,
            descripcion = "Descripción de prueba",
            fechaInicio = "2026-01-01",
            fechaFinalizacion = "2026-12-31"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private async Task<Guid> CrearTareaAsync(Guid proyectoId, string fechaLimite = "2026-06-30")
    {
        var response = await _client.PostAsJsonAsync("/api/v1/tareas", new
        {
            titulo = "Tarea Test",
            descripcion = "Descripción",
            prioridad = "Alta",
            fechaLimite,
            proyectoId
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    #region Proyectos

    [Fact]
    public async Task CrearProyecto_ConDatosValidos_Retorna201ConRecurso()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/proyectos", new
        {
            nombre = "API Project",
            descripcion = "Creado vía API",
            fechaInicio = "2026-01-01",
            fechaFinalizacion = "2026-12-31"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("nombre").GetString().Should().Be("API Project");
        body.GetProperty("totalTareas").GetInt32().Should().Be(0);
        body.GetProperty("porcentajeAvance").GetDecimal().Should().Be(0m);
    }

    [Fact]
    public async Task CrearProyecto_ConNombreVacio_Retorna400()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/proyectos", new
        {
            nombre = "",
            descripcion = "Desc",
            fechaInicio = "2026-01-01",
            fechaFinalizacion = "2026-12-31"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("status").GetInt32().Should().Be(400);
        body.GetProperty("error").GetString().Should().Be("Error de validación de dominio");
    }

    [Fact]
    public async Task ObtenerProyectoPorId_Existente_Retorna200()
    {
        var id = await CrearProyectoAsync("Proyecto Existente");

        var response = await _client.GetAsync($"/api/v1/proyectos/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetGuid().Should().Be(id);
        body.GetProperty("nombre").GetString().Should().Be("Proyecto Existente");
    }

    [Fact]
    public async Task ObtenerProyectoPorId_Inexistente_Retorna404()
    {
        var response = await _client.GetAsync($"/api/v1/proyectos/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("error").GetString().Should().Be("Recurso no encontrado");
    }

    [Fact]
    public async Task ListarProyectos_Retorna200ConArray()
    {
        await CrearProyectoAsync("Listado A");
        await CrearProyectoAsync("Listado B");

        var response = await _client.GetAsync("/api/v1/proyectos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.ValueKind.Should().Be(JsonValueKind.Array);
        body.GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task ObtenerAvance_ProyectoConTareas_RetornaPorcentaje()
    {
        var proyectoId = await CrearProyectoAsync("Con Avance");
        var tareaId = await CrearTareaAsync(proyectoId);
        await _client.PatchAsJsonAsync($"/api/v1/tareas/{tareaId}/estado", new { estado = "Completada" });

        var response = await _client.GetAsync($"/api/v1/proyectos/{proyectoId}/avance");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("totalTareas").GetInt32().Should().Be(1);
        body.GetProperty("tareasCompletadas").GetInt32().Should().Be(1);
        body.GetProperty("porcentajeAvance").GetDecimal().Should().Be(100m);
    }

    [Fact]
    public async Task ActualizarProyecto_Existente_Retorna200ConDatosNuevos()
    {
        var id = await CrearProyectoAsync("Antes");

        var response = await _client.PutAsJsonAsync($"/api/v1/proyectos/{id}", new
        {
            nombre = "Después",
            descripcion = "Actualizado",
            fechaInicio = "2026-02-01",
            fechaFinalizacion = "2026-11-30"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("nombre").GetString().Should().Be("Después");
    }

    [Fact]
    public async Task EliminarProyecto_SinTareas_Retorna204()
    {
        var id = await CrearProyectoAsync("A eliminar");

        var response = await _client.DeleteAsync($"/api/v1/proyectos/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/v1/proyectos/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EliminarProyecto_ConTareaPendiente_Retorna409()
    {
        var proyectoId = await CrearProyectoAsync("Con pendiente");
        await CrearTareaAsync(proyectoId);

        var response = await _client.DeleteAsync($"/api/v1/proyectos/{proyectoId}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("error").GetString().Should().Be("Conflicto de regla de negocio");
    }

    #endregion

    #region Tareas

    [Fact]
    public async Task CrearTarea_ProyectoInexistente_Retorna404()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/tareas", new
        {
            titulo = "Tarea huérfana",
            descripcion = "Desc",
            prioridad = "Media",
            fechaLimite = "2026-06-30",
            proyectoId = Guid.NewGuid()
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CrearTarea_ProyectoExistente_Retorna201()
    {
        var proyectoId = await CrearProyectoAsync("Proyecto con tarea");

        var response = await _client.PostAsJsonAsync("/api/v1/tareas", new
        {
            titulo = "Implementar endpoint",
            descripcion = "Desc",
            prioridad = "Alta",
            fechaLimite = "2026-06-30",
            proyectoId
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("titulo").GetString().Should().Be("Implementar endpoint");
        body.GetProperty("estado").GetString().Should().Be("Pendiente");
        body.GetProperty("proyectoId").GetGuid().Should().Be(proyectoId);
    }

    [Fact]
    public async Task ObtenerTareasPorProyecto_DevuelveSoloSusTareas()
    {
        var p1 = await CrearProyectoAsync("P1");
        var p2 = await CrearProyectoAsync("P2");
        await CrearTareaAsync(p1);
        await CrearTareaAsync(p2);

        var response = await _client.GetAsync($"/api/v1/tareas/proyecto/{p1}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetArrayLength().Should().Be(1);
        body[0].GetProperty("proyectoId").GetGuid().Should().Be(p1);
    }

    [Fact]
    public async Task CambiarEstadoTarea_ACompletada_Retorna200()
    {
        var proyectoId = await CrearProyectoAsync("Cambio estado");
        var tareaId = await CrearTareaAsync(proyectoId);

        var response = await _client.PatchAsJsonAsync($"/api/v1/tareas/{tareaId}/estado",
            new { estado = "Completada" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("estado").GetString().Should().Be("Completada");
    }

    [Fact]
    public async Task CambiarEstadoTarea_EstadoInvalido_Retorna400()
    {
        var proyectoId = await CrearProyectoAsync("Estado inválido");
        var tareaId = await CrearTareaAsync(proyectoId);

        var response = await _client.PatchAsJsonAsync($"/api/v1/tareas/{tareaId}/estado",
            new { estado = "Inexistente" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ListarTareasVencidas_DevuelveSoloVencidasNoCompletadas()
    {
        var proyectoId = await CrearProyectoAsync("Vencidas");
        var vencidaId = await CrearTareaAsync(proyectoId, "2020-01-01");
        await CrearTareaAsync(proyectoId, "2099-01-01");

        var response = await _client.GetAsync("/api/v1/tareas/vencidas");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var ids = body.EnumerateArray().Select(t => t.GetProperty("id").GetGuid()).ToList();
        ids.Should().Contain(vencidaId);
        body.EnumerateArray().Should().OnlyContain(t =>
            t.GetProperty("estaVencida").GetBoolean());
    }

    [Fact]
    public async Task EliminarTarea_Existente_Retorna204()
    {
        var proyectoId = await CrearProyectoAsync("Borrar tarea");
        var tareaId = await CrearTareaAsync(proyectoId);

        var response = await _client.DeleteAsync($"/api/v1/tareas/{tareaId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ObtenerTareaPorId_Inexistente_Retorna404()
    {
        var response = await _client.GetAsync($"/api/v1/tareas/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
