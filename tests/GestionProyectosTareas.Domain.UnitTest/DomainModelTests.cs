using FluentAssertions;
using GestionProyectosTareas.Domain.Entities;
using GestionProyectosTareas.Domain.Enums;
using GestionProyectosTareas.Domain.Exceptions;
using Xunit;

namespace GestionProyectosTareas.Domain.UnitTest;

public class DomainModelTests
{
    private static readonly DateOnly FechaInicio = new(2026, 1, 1);
    private static readonly DateOnly FechaFin = new(2026, 12, 31);

    private static Proyecto CrearProyectoValido() =>
        new(Guid.NewGuid(), "Proyecto Alpha", "Descripción", FechaInicio, FechaFin);

    private static Tarea CrearTareaValida(Guid? proyectoId = null, EstadoTarea estado = EstadoTarea.Pendiente) =>
        new(Guid.NewGuid(), "Tarea 1", "Descripción", PrioridadTarea.Media,
            new DateOnly(2026, 6, 30), proyectoId ?? Guid.NewGuid(), estado);

    #region Proyecto - Constructor

    [Fact]
    public void Constructor_ConIdVacio_LanzaDomainException()
    {
        var act = () => new Proyecto(Guid.Empty, "Nombre", "Desc", FechaInicio, FechaFin);

        act.Should().Throw<DomainException>()
            .WithMessage("*ID del proyecto*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ConNombreVacioOLlanco_LanzaDomainException(string nombre)
    {
        var act = () => new Proyecto(Guid.NewGuid(), nombre, "Desc", FechaInicio, FechaFin);

        act.Should().Throw<DomainException>()
            .WithMessage("*nombre del proyecto es obligatorio*");
    }

    [Fact]
    public void Constructor_ConNombreMayorA150_LanzaDomainException()
    {
        var nombre = new string('x', 151);

        var act = () => new Proyecto(Guid.NewGuid(), nombre, "Desc", FechaInicio, FechaFin);

        act.Should().Throw<DomainException>()
            .WithMessage("*150 caracteres*");
    }

    [Fact]
    public void Constructor_ConFechaFinAnteriorAInicio_LanzaDomainException()
    {
        var act = () => new Proyecto(Guid.NewGuid(), "Nombre", "Desc", FechaFin, FechaInicio);

        act.Should().Throw<DomainException>()
            .WithMessage("*fecha de finalización*");
    }

    [Fact]
    public void Constructor_RecortaNombreYDescripcion()
    {
        var proyecto = new Proyecto(Guid.NewGuid(), "  Nombre  ", "  Desc  ", FechaInicio, FechaFin);

        proyecto.Nombre.Should().Be("Nombre");
        proyecto.Descripcion.Should().Be("Desc");
    }

    [Fact]
    public void Constructor_ConDescripcionNull_UsaCadenaVacia()
    {
        var proyecto = new Proyecto(Guid.NewGuid(), "Nombre", null!, FechaInicio, FechaFin);

        proyecto.Descripcion.Should().BeEmpty();
    }

    #endregion

    #region Proyecto - Actualizar

    [Fact]
    public void Actualizar_ConDatosValidos_ActualizaPropiedades()
    {
        var proyecto = CrearProyectoValido();
        var nuevaInicio = new DateOnly(2026, 2, 1);
        var nuevaFin = new DateOnly(2026, 11, 30);

        proyecto.Actualizar("Nuevo nombre", "Nueva desc", nuevaInicio, nuevaFin);

        proyecto.Nombre.Should().Be("Nuevo nombre");
        proyecto.Descripcion.Should().Be("Nueva desc");
        proyecto.FechaInicio.Should().Be(nuevaInicio);
        proyecto.FechaFinalizacion.Should().Be(nuevaFin);
    }

    [Fact]
    public void Actualizar_ConNombreInvalido_LanzaDomainException()
    {
        var proyecto = CrearProyectoValido();

        var act = () => proyecto.Actualizar("", "Desc", FechaInicio, FechaFin);

        act.Should().Throw<DomainException>();
    }

    #endregion

    #region Proyecto - Tareas y avance

    [Fact]
    public void AgregarTarea_AgregaTareaALista()
    {
        var proyecto = CrearProyectoValido();
        var tarea = CrearTareaValida(proyecto.Id);

        proyecto.AgregarTarea(tarea);

        proyecto.Tareas.Should().ContainSingle().Which.Should().Be(tarea);
    }

    [Fact]
    public void AgregarTarea_Nula_LanzaExcepcion()
    {
        var proyecto = CrearProyectoValido();

        var act = () => proyecto.AgregarTarea(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CalcularPorcentajeAvance_SinTareas_RetornaCero()
    {
        CrearProyectoValido().CalcularPorcentajeAvance().Should().Be(0m);
    }

    [Fact]
    public void CalcularPorcentajeAvance_ConTareas_CalculaPorcentajeCorrecto()
    {
        var proyecto = CrearProyectoValido();
        proyecto.AgregarTarea(CrearTareaValida(proyecto.Id, EstadoTarea.Completada));
        proyecto.AgregarTarea(CrearTareaValida(proyecto.Id, EstadoTarea.Completada));
        proyecto.AgregarTarea(CrearTareaValida(proyecto.Id, EstadoTarea.EnProgreso));
        proyecto.AgregarTarea(CrearTareaValida(proyecto.Id, EstadoTarea.Pendiente));

        proyecto.CalcularPorcentajeAvance().Should().Be(50m);
    }

    #endregion

    #region Proyecto - ValidarEliminacion

    [Fact]
    public void ValidarEliminacion_SinTareasPendientes_NoLanzaExcepcion()
    {
        var proyecto = CrearProyectoValido();
        proyecto.AgregarTarea(CrearTareaValida(proyecto.Id, EstadoTarea.Completada));
        proyecto.AgregarTarea(CrearTareaValida(proyecto.Id, EstadoTarea.EnProgreso));

        var act = () => proyecto.ValidarEliminacion();

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidarEliminacion_ConTareasPendientes_LanzaProyectoConTareasPendientesException()
    {
        var proyecto = CrearProyectoValido();
        proyecto.AgregarTarea(CrearTareaValida(proyecto.Id, EstadoTarea.Pendiente));
        proyecto.AgregarTarea(CrearTareaValida(proyecto.Id, EstadoTarea.Pendiente));
        proyecto.AgregarTarea(CrearTareaValida(proyecto.Id, EstadoTarea.Completada));

        var act = () => proyecto.ValidarEliminacion();

        act.Should().Throw<ProyectoConTareasPendientesException>()
            .WithMessage($"*{proyecto.Id}*2*");
    }

    [Fact]
    public void ValidarEliminacion_SinTareas_NoLanzaExcepcion()
    {
        var act = () => CrearProyectoValido().ValidarEliminacion();

        act.Should().NotThrow();
    }

    #endregion

    #region Tarea - Constructor

    [Fact]
    public void Tarea_ConIdVacio_LanzaDomainException()
    {
        var act = () => new Tarea(Guid.Empty, "Título", "Desc", PrioridadTarea.Baja,
            FechaFin, Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .WithMessage("*ID de la tarea*");
    }

    [Fact]
    public void Tarea_ConProyectoIdVacio_LanzaDomainException()
    {
        var act = () => new Tarea(Guid.NewGuid(), "Título", "Desc", PrioridadTarea.Baja,
            FechaFin, Guid.Empty);

        act.Should().Throw<DomainException>()
            .WithMessage("*ID del proyecto*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Tarea_ConTituloVacioOLlanco_LanzaDomainException(string titulo)
    {
        var act = () => new Tarea(Guid.NewGuid(), titulo, "Desc", PrioridadTarea.Baja,
            FechaFin, Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .WithMessage("*título de la tarea es obligatorio*");
    }

    [Fact]
    public void Tarea_ConTituloMayorA150_LanzaDomainException()
    {
        var titulo = new string('x', 151);

        var act = () => new Tarea(Guid.NewGuid(), titulo, "Desc", PrioridadTarea.Baja,
            FechaFin, Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .WithMessage("*150 caracteres*");
    }

    [Fact]
    public void Tarea_PorDefecto_EstadoPendienteYRecortaCampos()
    {
        var tarea = new Tarea(Guid.NewGuid(), "  Título  ", "  Desc  ", PrioridadTarea.Alta,
            FechaFin, Guid.NewGuid());

        tarea.Estado.Should().Be(EstadoTarea.Pendiente);
        tarea.Titulo.Should().Be("Título");
        tarea.Descripcion.Should().Be("Desc");
    }

    #endregion

    #region Tarea - Actualizar y CambiarEstado

    [Fact]
    public void Tarea_Actualizar_ConDatosValidos_ActualizaPropiedades()
    {
        var tarea = CrearTareaValida();
        var nuevaFecha = new DateOnly(2026, 9, 15);

        tarea.Actualizar("Nuevo título", "Nueva desc", PrioridadTarea.Alta, nuevaFecha);

        tarea.Titulo.Should().Be("Nuevo título");
        tarea.Descripcion.Should().Be("Nueva desc");
        tarea.Prioridad.Should().Be(PrioridadTarea.Alta);
        tarea.FechaLimite.Should().Be(nuevaFecha);
    }

    [Fact]
    public void CambiarEstado_ConEstadoValido_ActualizaEstado()
    {
        var tarea = CrearTareaValida();

        tarea.CambiarEstado(EstadoTarea.Completada);

        tarea.Estado.Should().Be(EstadoTarea.Completada);
    }

    [Fact]
    public void CambiarEstado_ConEstadoInvalido_LanzaDomainException()
    {
        var tarea = CrearTareaValida();
        var estadoInvalido = (EstadoTarea)99;

        var act = () => tarea.CambiarEstado(estadoInvalido);

        act.Should().Throw<DomainException>()
            .WithMessage("*estado de la tarea no es válido*");
    }

    #endregion

    #region Tarea - EstaVencida

    [Fact]
    public void EstaVencida_FechaLimitePasadaYNoCompletada_RetornaTrue()
    {
        var tarea = CrearTareaValida(estado: EstadoTarea.EnProgreso);
        var hoy = new DateOnly(2026, 7, 1);

        tarea.EstaVencida(hoy).Should().BeTrue();
    }

    [Fact]
    public void EstaVencida_TareaCompletada_RetornaFalse()
    {
        var tarea = CrearTareaValida(estado: EstadoTarea.Completada);
        var hoy = new DateOnly(2026, 7, 1);

        tarea.EstaVencida(hoy).Should().BeFalse();
    }

    [Fact]
    public void EstaVencida_FechaLimiteFutura_RetornaFalse()
    {
        var tarea = CrearTareaValida();
        var hoy = new DateOnly(2026, 1, 1);

        tarea.EstaVencida(hoy).Should().BeFalse();
    }

    [Fact]
    public void EstaVencida_FechaLimiteIgualAHoy_RetornaFalse()
    {
        var tarea = CrearTareaValida();

        tarea.EstaVencida(new DateOnly(2026, 6, 30)).Should().BeFalse();
    }

    #endregion
}
