using FluentAssertions;
using GestionProyectosTareas.Application.DTOs.Proyectos;
using GestionProyectosTareas.Application.DTOs.Tareas;
using GestionProyectosTareas.Application.Exceptions;
using GestionProyectosTareas.Application.Interfaces;
using GestionProyectosTareas.Application.Services;
using GestionProyectosTareas.Domain.Entities;
using GestionProyectosTareas.Domain.Enums;
using GestionProyectosTareas.Domain.Exceptions;
using Moq;
using Xunit;

namespace GestionProyectosTareas.Application.UnitTest;

public class ServiceRequirementTests
{
    private static readonly DateOnly FechaInicio = new(2026, 1, 1);
    private static readonly DateOnly FechaFin = new(2026, 12, 31);

    private static Proyecto CrearProyecto(Guid? id = null)
    {
        return new Proyecto(id ?? Guid.NewGuid(), "Proyecto", "Desc", FechaInicio, FechaFin);
    }

    private static Proyecto CrearProyectoConTareas(params EstadoTarea[] estados)
    {
        var proyecto = CrearProyecto();
        foreach (var estado in estados)
            proyecto.AgregarTarea(CrearTarea(proyecto.Id, estado));
        return proyecto;
    }

    private static Tarea CrearTarea(Guid proyectoId, EstadoTarea estado = EstadoTarea.Pendiente) =>
        new(Guid.NewGuid(), "Tarea", "Desc", PrioridadTarea.Media,
            new DateOnly(2026, 6, 30), proyectoId, estado);

    #region ProyectoService

    [Fact]
    public async Task ObtenerTodosAsync_DevuelveProyectosMapeados()
    {
        var proyecto = CrearProyecto();
        var repo = new Mock<IProyectoRepository>();
        repo.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { proyecto });
        var service = new ProyectoService(repo.Object);

        var resultado = (await service.ObtenerTodosAsync()).ToList();

        resultado.Should().ContainSingle();
        resultado[0].Id.Should().Be(proyecto.Id);
        resultado[0].Nombre.Should().Be(proyecto.Nombre);
        resultado[0].TotalTareas.Should().Be(0);
        resultado[0].PorcentajeAvance.Should().Be(0m);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ProyectoExiste_DevuelveResponse()
    {
        var proyecto = CrearProyecto();
        var repo = new Mock<IProyectoRepository>();
        repo.Setup(r => r.ObtenerPorIdAsync(proyecto.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proyecto);
        var service = new ProyectoService(repo.Object);

        var resultado = await service.ObtenerPorIdAsync(proyecto.Id);

        resultado.Id.Should().Be(proyecto.Id);
        repo.Verify(r => r.ObtenerPorIdAsync(proyecto.Id, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ProyectoNoExiste_LanzaNotFoundException()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<IProyectoRepository>();
        repo.Setup(r => r.ObtenerPorIdAsync(id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Proyecto?)null);
        var service = new ProyectoService(repo.Object);

        var act = () => service.ObtenerPorIdAsync(id);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ObtenerAvanceAsync_ConTareas_DevuelveConteosYPorcentaje()
    {
        var proyecto = CrearProyectoConTareas(
            EstadoTarea.Completada, EstadoTarea.Completada,
            EstadoTarea.EnProgreso, EstadoTarea.Pendiente);
        var repo = new Mock<IProyectoRepository>();
        repo.Setup(r => r.ObtenerPorIdAsync(proyecto.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proyecto);
        var service = new ProyectoService(repo.Object);

        var avance = await service.ObtenerAvanceAsync(proyecto.Id);

        avance.TotalTareas.Should().Be(4);
        avance.TareasCompletadas.Should().Be(2);
        avance.TareasEnProgreso.Should().Be(1);
        avance.TareasPendientes.Should().Be(1);
        avance.PorcentajeAvance.Should().Be(50m);
    }

    [Fact]
    public async Task CrearAsync_ProyectoValido_AgregaEnRepositorio()
    {
        var repo = new Mock<IProyectoRepository>();
        var service = new ProyectoService(repo.Object);
        var request = new CrearProyectoRequest("Nuevo", "Desc", FechaInicio, FechaFin);

        var resultado = await service.CrearAsync(request);

        resultado.Nombre.Should().Be("Nuevo");
        resultado.Id.Should().NotBeEmpty();
        repo.Verify(r => r.AgregarAsync(It.Is<Proyecto>(p => p.Nombre == "Nuevo"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearAsync_ConDatosInvalidos_LanzaDomainExceptionSinGuardar()
    {
        var repo = new Mock<IProyectoRepository>();
        var service = new ProyectoService(repo.Object);
        var request = new CrearProyectoRequest("", "Desc", FechaInicio, FechaFin);

        var act = () => service.CrearAsync(request);

        await act.Should().ThrowAsync<DomainException>();
        repo.Verify(r => r.AgregarAsync(It.IsAny<Proyecto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ActualizarAsync_ProyectoExiste_ActualizaYGuarda()
    {
        var proyecto = CrearProyecto();
        var repo = new Mock<IProyectoRepository>();
        repo.Setup(r => r.ObtenerPorIdAsync(proyecto.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proyecto);
        var service = new ProyectoService(repo.Object);
        var request = new ActualizarProyectoRequest("Actualizado", "Desc2", FechaInicio, FechaFin);

        var resultado = await service.ActualizarAsync(proyecto.Id, request);

        resultado.Nombre.Should().Be("Actualizado");
        repo.Verify(r => r.ActualizarAsync(proyecto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarAsync_SinTareasPendientes_Elimina()
    {
        var proyecto = CrearProyectoConTareas(EstadoTarea.Completada);
        var repo = new Mock<IProyectoRepository>();
        repo.Setup(r => r.ObtenerPorIdAsync(proyecto.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proyecto);
        var service = new ProyectoService(repo.Object);

        await service.EliminarAsync(proyecto.Id);

        repo.Verify(r => r.EliminarAsync(proyecto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarAsync_ConTareasPendientes_LanzaExcepcionSinEliminar()
    {
        var proyecto = CrearProyectoConTareas(EstadoTarea.Pendiente);
        var repo = new Mock<IProyectoRepository>();
        repo.Setup(r => r.ObtenerPorIdAsync(proyecto.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proyecto);
        var service = new ProyectoService(repo.Object);

        var act = () => service.EliminarAsync(proyecto.Id);

        await act.Should().ThrowAsync<ProyectoConTareasPendientesException>();
        repo.Verify(r => r.EliminarAsync(It.IsAny<Proyecto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EliminarAsync_ProyectoNoExiste_LanzaNotFoundException()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<IProyectoRepository>();
        repo.Setup(r => r.ObtenerPorIdAsync(id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Proyecto?)null);
        var service = new ProyectoService(repo.Object);

        var act = () => service.EliminarAsync(id);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region TareaService

    private static (TareaService Service, Mock<ITareaRepository> TareaRepo, Mock<IProyectoRepository> ProyectoRepo) CrearTareaService()
    {
        var tareaRepo = new Mock<ITareaRepository>();
        var proyectoRepo = new Mock<IProyectoRepository>();
        return (new TareaService(tareaRepo.Object, proyectoRepo.Object), tareaRepo, proyectoRepo);
    }

    [Fact]
    public async Task CrearAsync_ProyectoExiste_CreaTarea()
    {
        var proyectoId = Guid.NewGuid();
        var (service, tareaRepo, proyectoRepo) = CrearTareaService();
        proyectoRepo.Setup(r => r.ExisteAsync(proyectoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var request = new CrearTareaRequest("Nueva tarea", "Desc", PrioridadTarea.Alta,
            new DateOnly(2026, 6, 30), proyectoId);

        var resultado = await service.CrearAsync(request);

        resultado.Titulo.Should().Be("Nueva tarea");
        resultado.ProyectoId.Should().Be(proyectoId);
        resultado.Estado.Should().Be(EstadoTarea.Pendiente);
        tareaRepo.Verify(r => r.AgregarAsync(It.IsAny<Tarea>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearAsync_ProyectoNoExiste_LanzaNotFoundExceptionSinCrear()
    {
        var proyectoId = Guid.NewGuid();
        var (service, tareaRepo, proyectoRepo) = CrearTareaService();
        proyectoRepo.Setup(r => r.ExisteAsync(proyectoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var request = new CrearTareaRequest("Nueva tarea", "Desc", PrioridadTarea.Alta,
            new DateOnly(2026, 6, 30), proyectoId);

        var act = () => service.CrearAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
        tareaRepo.Verify(r => r.AgregarAsync(It.IsAny<Tarea>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerPorProyectoIdAsync_ProyectoNoExiste_LanzaNotFoundException()
    {
        var proyectoId = Guid.NewGuid();
        var (service, _, proyectoRepo) = CrearTareaService();
        proyectoRepo.Setup(r => r.ExisteAsync(proyectoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = () => service.ObtenerPorProyectoIdAsync(proyectoId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ObtenerPorProyectoIdAsync_ProyectoExiste_DevuelveTareas()
    {
        var proyectoId = Guid.NewGuid();
        var tarea = CrearTarea(proyectoId);
        var (service, tareaRepo, proyectoRepo) = CrearTareaService();
        proyectoRepo.Setup(r => r.ExisteAsync(proyectoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        tareaRepo.Setup(r => r.ObtenerPorProyectoIdAsync(proyectoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { tarea });

        var resultado = (await service.ObtenerPorProyectoIdAsync(proyectoId)).ToList();

        resultado.Should().ContainSingle();
        resultado[0].ProyectoId.Should().Be(proyectoId);
    }

    [Fact]
    public async Task ObtenerTodasAsync_PasaFiltrosAlRepositorio()
    {
        var (service, tareaRepo, _) = CrearTareaService();
        tareaRepo.Setup(r => r.ObtenerTodasAsync(
                EstadoTarea.Pendiente, PrioridadTarea.Alta, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Tarea>());

        await service.ObtenerTodasAsync(EstadoTarea.Pendiente, PrioridadTarea.Alta);

        tareaRepo.Verify(r => r.ObtenerTodasAsync(
            EstadoTarea.Pendiente, PrioridadTarea.Alta, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerVencidasNoCompletadasAsync_DevuelveTareasVencidas()
    {
        var tareaVencida = CrearTarea(Guid.NewGuid(), EstadoTarea.Pendiente);
        var (service, tareaRepo, _) = CrearTareaService();
        tareaRepo.Setup(r => r.ObtenerVencidasNoCompletadasAsync(
                It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { tareaVencida });

        var resultado = (await service.ObtenerVencidasNoCompletadasAsync()).ToList();

        resultado.Should().ContainSingle();
    }

    [Fact]
    public async Task ObtenerPorIdAsync_TareaNoExiste_LanzaNotFoundException()
    {
        var id = Guid.NewGuid();
        var (service, tareaRepo, _) = CrearTareaService();
        tareaRepo.Setup(r => r.ObtenerPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tarea?)null);

        var act = () => service.ObtenerPorIdAsync(id);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CambiarEstadoAsync_EstadoValido_ActualizaYGuarda()
    {
        var tarea = CrearTarea(Guid.NewGuid());
        var (service, tareaRepo, _) = CrearTareaService();
        tareaRepo.Setup(r => r.ObtenerPorIdAsync(tarea.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tarea);
        var request = new CambiarEstadoTareaRequest(EstadoTarea.Completada);

        var resultado = await service.CambiarEstadoAsync(tarea.Id, request);

        resultado.Estado.Should().Be(EstadoTarea.Completada);
        tareaRepo.Verify(r => r.ActualizarAsync(tarea, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CambiarEstadoAsync_EstadoInvalido_LanzaDomainExceptionSinGuardar()
    {
        var tarea = CrearTarea(Guid.NewGuid());
        var (service, tareaRepo, _) = CrearTareaService();
        tareaRepo.Setup(r => r.ObtenerPorIdAsync(tarea.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tarea);
        var request = new CambiarEstadoTareaRequest((EstadoTarea)99);

        var act = () => service.CambiarEstadoAsync(tarea.Id, request);

        await act.Should().ThrowAsync<DomainException>();
        tareaRepo.Verify(r => r.ActualizarAsync(It.IsAny<Tarea>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EliminarAsync_TareaExiste_Elimina()
    {
        var tarea = CrearTarea(Guid.NewGuid());
        var (service, tareaRepo, _) = CrearTareaService();
        tareaRepo.Setup(r => r.ObtenerPorIdAsync(tarea.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tarea);

        await service.EliminarAsync(tarea.Id);

        tareaRepo.Verify(r => r.EliminarAsync(tarea, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarAsync_TareaNoExiste_LanzaNotFoundException()
    {
        var id = Guid.NewGuid();
        var (service, tareaRepo, _) = CrearTareaService();
        tareaRepo.Setup(r => r.ObtenerPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tarea?)null);

        var act = () => service.EliminarAsync(id);

        await act.Should().ThrowAsync<NotFoundException>();
        tareaRepo.Verify(r => r.EliminarAsync(It.IsAny<Tarea>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
