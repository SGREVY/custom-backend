
using Backend_custom_test.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend_custom_test.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }

    public DbSet<Empleado> Empleados { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id).HasColumnName("id");
            entity.Property(u => u.Nombre).HasColumnName("nombre");
            entity.Property(u => u.Correo).HasColumnName("correo");
            entity.Property(u => u.ContrasenaHash).HasColumnName("contrasena_hash");
            entity.Property(u => u.Activo).HasColumnName("activo");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.ToTable("empleados");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.Nombre).HasColumnName("nombre");
            entity.Property(e => e.Apellido).HasColumnName("apellido");
            entity.Property(e => e.Telefono).HasColumnName("telefono");
            entity.Property(e => e.Puesto).HasColumnName("puesto");
            entity.Property(e => e.Departamento).HasColumnName("departamento");
            entity.Property(e => e.FechaIngreso).HasColumnName("fecha_ingreso");
            entity.Property(e => e.Activo).HasColumnName("activo");
        });
    }
}
