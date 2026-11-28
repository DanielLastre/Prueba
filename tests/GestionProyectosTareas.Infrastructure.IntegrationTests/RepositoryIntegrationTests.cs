using FluentAssertions;
using GestionProyectosTareas.Domain.Entities;
using GestionProyectosTareas.Domain.Enums;
using GestionProyectosTareas.Infrastructure.Persistence;
using GestionProyectosTareas.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestionProyectosTareas.Infrastructure.IntegrationTests;

public class RepositoryIntegrationTests : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;

    public RepositoryIntegrationTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync() => await _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private static Proyecto CrearProyecto(string nombre = "Proyecto", DateOnly? fechaInicio = null)
    {
        var inicio = fechaInicio ?? new DateOnly(2026, 1, 1);
        return new Proyecto(Guid.NewGuid(), nombre, "Descripción", inicio, inicio.AddMonths(6));
    }

    private static Tarea CrearTarea(Guid proyectoId, EstadoTarea estado = EstadoTarea.Pendiente,
        PrioridadTarea prioridad = PrioridadTarea.Media, DateOnly? fechaLimite = null)
    {
        return new Tarea(Guid.NewGuid(), "Tarea", "Descripción", prioridad,
            fechaLimite ?? new DateOnly(2026, 6, 30), proyectoId, estado);
    }

    #region ProyectoRepository

    [Fact]
    public async Task AgregarAsync_YObtenerPorIdAsync_ConTareas_DevuelveProyectoConTareas()
    {
        await using var context = _fixture.CreateContext();
        var repo = new ProyectoRepository(context);
        var proyecto = CrearProyecto();
        var tarea = CrearTarea(proyecto.Id);

        await context.Tareas.AddAsync(tarea);
        await repo.AgregarAsync(proyecto);

        var obtenido = await repo.ObtenerPorIdAsync(proyecto.Id, incluirTareas: true);

        obtenido.Should().NotBeNull();
        obtenido!.Nombre.Should().Be(proyecto.Nombre);
        obtenido.Tareas.Should().ContainSingle(t => t.Id == tarea.Id);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_SinIncluirTareas_NoCargaTareas()
    {
        await using var context = _fixture.CreateContext();
        var repo = new ProyectoRepository(context);
        var proyecto = CrearProyecto();
        await context.Tareas.AddAsync(CrearTarea(proyecto.Id));
        await repo.AgregarAsync(proyecto);
        context.ChangeTracker.Clear();

        var obtenido = await repo.ObtenerPorIdAsync(proyecto.Id, incluirTareas: false);

        obtenido.Should().NotBeNull();
        obtenido!.Tareas.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerTodosAsync_OrdenaPorFechaInicio()
    {
        await using var context = _fixture.CreateContext();
        var repo = new ProyectoRepository(context);
        var tardio = CrearProyecto("Tardío", new DateOnly(2026, 6, 1));
        var temprano = CrearProyecto("Temprano", new DateOnly(2026, 1, 1));
        await repo.AgregarAsync(tardio);
        await repo.AgregarAsync(temprano);

        var proyectos = (await repo.ObtenerTodosAsync()).ToList();

        proyectos.Should().HaveCount(2);
        proyectos[0].Nombre.Should().Be("Temprano");
        proyectos[1].Nombre.Should().Be("Tardío");
    }

    [Fact]
    public async Task ExisteAsync_ReflejaPersistencia()
    {
        await using var context = _fixture.CreateContext();
        var repo = new ProyectoRepository(context);
        var proyecto = CrearProyecto();

        (await repo.ExisteAsync(proyecto.Id)).Should().BeFalse();

        await repo.AgregarAsync(proyecto);

        (await repo.ExisteAsync(proyecto.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task ActualizarAsync_PersisteCambios()
    {
        await using var context = _fixture.CreateContext();
        var repo = new ProyectoRepository(context);
        var proyecto = CrearProyecto();
        await repo.AgregarAsync(proyecto);

        proyecto.Actualizar("Renombrado", "Nueva desc", new DateOnly(2026, 2, 1), new DateOnly(2026, 10, 1));
        await repo.ActualizarAsync(proyecto);
        context.ChangeTracker.Clear();

        var obtenido = await repo.ObtenerPorIdAsync(proyecto.Id);
        obtenido!.Nombre.Should().Be("Renombrado");
        obtenido.FechaInicio.Should().Be(new DateOnly(2026, 2, 1));
    }

    [Fact]
    public async Task EliminarAsync_EliminaProyecto()
    {
        await using var context = _fixture.CreateContext();
        var proyectoRepo = new ProyectoRepository(context);
        var proyecto = CrearProyecto();
        await proyectoRepo.AgregarAsync(proyecto);

        var cargado = await proyectoRepo.ObtenerPorIdAsync(proyecto.Id);
        await proyectoRepo.EliminarAsync(cargado!);

        (await proyectoRepo.ExisteAsync(proyecto.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task EliminarAsync_ProyectoConTareas_RelacionRestrictImpedirEliminar()
    {
        await using var context = _fixture.CreateContext();
        var proyectoRepo = new ProyectoRepository(context);
        var tareaRepo = new TareaRepository(context);
        var proyecto = CrearProyecto();
        await context.Proyectos.AddAsync(proyecto);
        await tareaRepo.AgregarAsync(CrearTarea(proyecto.Id, EstadoTarea.Completada));

        var cargado = await proyectoRepo.ObtenerPorIdAsync(proyecto.Id, incluirTareas: true);

        var act = () => proyectoRepo.EliminarAsync(cargado!);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region TareaRepository

    [Fact]
    public async Task AgregarYObtenerPorIdAsync_DevuelveTareaPersistida()
    {
        await using var context = _fixture.CreateContext();
        var proyectoRepo = new ProyectoRepository(context);
        var tareaRepo = new TareaRepository(context);
        var proyecto = CrearProyecto();
        await proyectoRepo.AgregarAsync(proyecto);
        var tarea = CrearTarea(proyecto.Id, EstadoTarea.EnProgreso, PrioridadTarea.Alta);

        await tareaRepo.AgregarAsync(tarea);

        var obtenida = await tareaRepo.ObtenerPorIdAsync(tarea.Id);
        obtenida.Should().NotBeNull();
        obtenida!.Titulo.Should().Be("Tarea");
        obtenida.Estado.Should().Be(EstadoTarea.EnProgreso);
        obtenida.Prioridad.Should().Be(PrioridadTarea.Alta);
        obtenida.ProyectoId.Should().Be(proyecto.Id);
    }

    [Fact]
    public async Task ObtenerTodasAsync_FiltraPorEstadoYPrioridad()
    {
        await using var context = _fixture.CreateContext();
        var proyectoRepo = new ProyectoRepository(context);
        var tareaRepo = new TareaRepository(context);
        var proyecto = CrearProyecto();
        await proyectoRepo.AgregarAsync(proyecto);
        await tareaRepo.AgregarAsync(CrearTarea(proyecto.Id, EstadoTarea.Pendiente, PrioridadTarea.Baja));
        await tareaRepo.AgregarAsync(CrearTarea(proyecto.Id, EstadoTarea.Completada, PrioridadTarea.Alta));

        var porEstado = (await tareaRepo.ObtenerTodasAsync(estado: EstadoTarea.Completada)).ToList();
        var porPrioridad = (await tareaRepo.ObtenerTodasAsync(prioridad: PrioridadTarea.Baja)).ToList();
        var todos = (await tareaRepo.ObtenerTodasAsync()).ToList();

        porEstado.Should().ContainSingle(t => t.Estado == EstadoTarea.Completada);
        porPrioridad.Should().ContainSingle(t => t.Prioridad == PrioridadTarea.Baja);
        todos.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObtenerTodasAsync_OrdenaPorFechaLimite()
    {
        await using var context = _fixture.CreateContext();
        var proyectoRepo = new ProyectoRepository(context);
        var tareaRepo = new TareaRepository(context);
        var proyecto = CrearProyecto();
        await proyectoRepo.AgregarAsync(proyecto);
        await tareaRepo.AgregarAsync(CrearTarea(proyecto.Id, fechaLimite: new DateOnly(2026, 9, 1)));
        await tareaRepo.AgregarAsync(CrearTarea(proyecto.Id, fechaLimite: new DateOnly(2026, 3, 1)));

        var tareas = (await tareaRepo.ObtenerTodasAsync()).ToList();

        tareas.Should().HaveCount(2);
        tareas[0].FechaLimite.Should().Be(new DateOnly(2026, 3, 1));
        tareas[1].FechaLimite.Should().Be(new DateOnly(2026, 9, 1));
    }

    [Fact]
    public async Task ObtenerPorProyectoIdAsync_SoloDevuelveTareasDelProyecto()
    {
        await using var context = _fixture.CreateContext();
        var proyectoRepo = new ProyectoRepository(context);
        var tareaRepo = new TareaRepository(context);
        var p1 = CrearProyecto("P1");
        var p2 = CrearProyecto("P2");
        await proyectoRepo.AgregarAsync(p1);
        await proyectoRepo.AgregarAsync(p2);
        await tareaRepo.AgregarAsync(CrearTarea(p1.Id));
        await tareaRepo.AgregarAsync(CrearTarea(p1.Id));
        await tareaRepo.AgregarAsync(CrearTarea(p2.Id));

        var tareas = (await tareaRepo.ObtenerPorProyectoIdAsync(p1.Id)).ToList();

        tareas.Should().HaveCount(2);
        tareas.Should().OnlyContain(t => t.ProyectoId == p1.Id);
    }

    [Fact]
    public async Task ObtenerVencidasNoCompletadasAsync_FiltraCorrectamente()
    {
        await using var context = _fixture.CreateContext();
        var proyectoRepo = new ProyectoRepository(context);
        var tareaRepo = new TareaRepository(context);
        var proyecto = CrearProyecto();
        await proyectoRepo.AgregarAsync(proyecto);
        var hoy = new DateOnly(2026, 7, 1);

        await tareaRepo.AgregarAsync(CrearTarea(proyecto.Id, EstadoTarea.Pendiente, fechaLimite: new DateOnly(2026, 6, 1)));
        await tareaRepo.AgregarAsync(CrearTarea(proyecto.Id, EstadoTarea.EnProgreso, fechaLimite: new DateOnly(2026, 6, 15)));
        await tareaRepo.AgregarAsync(CrearTarea(proyecto.Id, EstadoTarea.Completada, fechaLimite: new DateOnly(2026, 6, 1)));
        await tareaRepo.AgregarAsync(CrearTarea(proyecto.Id, EstadoTarea.Pendiente, fechaLimite: new DateOnly(2026, 8, 1)));

        var vencidas = (await tareaRepo.ObtenerVencidasNoCompletadasAsync(hoy)).ToList();

        vencidas.Should().HaveCount(2);
        vencidas.Should().OnlyContain(t => t.FechaLimite < hoy && t.Estado != EstadoTarea.Completada);
    }

    [Fact]
    public async Task ActualizarAsync_PersisteCambiosDeEstado()
    {
        await using var context = _fixture.CreateContext();
        var proyectoRepo = new ProyectoRepository(context);
        var tareaRepo = new TareaRepository(context);
        var proyecto = CrearProyecto();
        await proyectoRepo.AgregarAsync(proyecto);
        var tarea = CrearTarea(proyecto.Id);
        await tareaRepo.AgregarAsync(tarea);

        tarea.CambiarEstado(EstadoTarea.Completada);
        await tareaRepo.ActualizarAsync(tarea);
        context.ChangeTracker.Clear();

        var obtenida = await tareaRepo.ObtenerPorIdAsync(tarea.Id);
        obtenida!.Estado.Should().Be(EstadoTarea.Completada);
    }

    [Fact]
    public async Task EliminarAsync_EliminaTarea()
    {
        await using var context = _fixture.CreateContext();
        var proyectoRepo = new ProyectoRepository(context);
        var tareaRepo = new TareaRepository(context);
        var proyecto = CrearProyecto();
        await proyectoRepo.AgregarAsync(proyecto);
        var tarea = CrearTarea(proyecto.Id);
        await tareaRepo.AgregarAsync(tarea);

        await tareaRepo.EliminarAsync(tarea);

        (await tareaRepo.ObtenerPorIdAsync(tarea.Id)).Should().BeNull();
    }

    #endregion
}
