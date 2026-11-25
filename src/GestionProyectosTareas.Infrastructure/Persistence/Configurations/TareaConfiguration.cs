using GestionProyectosTareas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionProyectosTareas.Infrastructure.Persistence.Configurations;

public class TareaConfiguration : IEntityTypeConfiguration<Tarea>
{
    public void Configure(EntityTypeBuilder<Tarea> builder)
    {
        builder.ToTable("Tareas");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Titulo)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(t => t.Descripcion)
            .HasMaxLength(1000);

        builder.Property(t => t.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(t => t.Prioridad)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(t => t.FechaLimite)
            .IsRequired();

        builder.Property(t => t.ProyectoId)
            .IsRequired();
    }
}