
using Backend_custom_test.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend_custom_test.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Tablas existentes
    public DbSet<Usuario> Usuarios { get; set; }

    public DbSet<Empleado> Empleados { get; set; }

    // Nuevas tablas
    public DbSet<HorarioEmpleado> HorariosEmpleados { get; set; }

    public DbSet<SolicitudVacaciones> SolicitudesVacaciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // USUARIOS
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

        // EMPLEADOS
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

        // HORARIOS DE EMPLEADOS
        modelBuilder.Entity<HorarioEmpleado>(entity =>
        {
            entity.ToTable("horarios_empleados");

            entity.HasKey(h => h.Id);

            entity.Property(h => h.Id).HasColumnName("id");
            entity.Property(h => h.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(h => h.DiaDescanso).HasColumnName("dia_descanso");
            entity.Property(h => h.PrimeraGuardia)
                .HasColumnName("primera_guardia")
                .HasColumnType("date");

            entity.HasOne<Empleado>()
                .WithOne()
                .HasForeignKey<HorarioEmpleado>(h => h.EmpleadoId);
        });

        // SOLICITUDES DE VACACIONES
        modelBuilder.Entity<SolicitudVacaciones>(entity =>
        {
            entity.ToTable("solicitudes_vacaciones");

            entity.HasKey(v => v.Id);

            entity.Property(v => v.Id).HasColumnName("id");
            entity.Property(v => v.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(v => v.FechaInicio)
                .HasColumnName("fecha_inicio")
                .HasColumnType("date");
            entity.Property(v => v.FechaFin)
                .HasColumnName("fecha_fin")
                .HasColumnType("date");
            entity.Property(v => v.Estado).HasColumnName("estado");
            entity.Property(v => v.FechaSolicitud)
                .HasColumnName("fecha_solicitud")
                .HasColumnType("timestamp without time zone");
            entity.Property(v => v.FechaRespuesta)
                .HasColumnName("fecha_respuesta")
                .HasColumnType("timestamp without time zone");

            entity.HasOne<Empleado>()
                .WithMany()
                .HasForeignKey(v => v.EmpleadoId);
        });
    }
}
