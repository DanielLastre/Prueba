using GestionProyectosTareas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionProyectosTareas.Infrastructure.Persistence.Configurations;

public class ProyectoConfiguration : IEntityTypeConfiguration<Proyecto>
{
    public void Configure(EntityTypeBuilder<Proyecto> builder)
    {
        builder.ToTable("Proyectos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Descripcion)
            .HasMaxLength(1000);

        builder.Property(p => p.FechaInicio)
            .IsRequired();

        builder.Property(p => p.FechaFinalizacion)
            .IsRequired();

        builder.HasMany(p => p.Tareas)
            .WithOne(t => t.Proyecto)
            .HasForeignKey(t => t.ProyectoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(p => p.Tareas)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}