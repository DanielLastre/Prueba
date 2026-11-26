using GestionProyectosTareas.Application.Interfaces;
using GestionProyectosTareas.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GestionProyectosTareas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProyectoService, ProyectoService>();
        services.AddScoped<ITareaService, TareaService>();

        return services;
    }
}
