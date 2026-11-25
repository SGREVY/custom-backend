
using Backend_custom_test.API.Data;
using Backend_custom_test.API.Models;
using Microsoft.EntityFrameworkCore;

// ==========================================
// MÉTODO PARA CALCULAR LA GUARDIA MENSUAL
// ==========================================

static DateOnly CalcularGuardiaMensual(
    DateOnly primeraGuardia,
    int anio,
    int mes)
{
    // Identificar qué sábado del mes corresponde
    int numeroSabado = ((primeraGuardia.Day - 1) / 7) + 1;

    // Encontrar el último sábado del mes
    int ultimoDia = DateTime.DaysInMonth(anio, mes);

    var ultimoDiaMes = new DateOnly(anio, mes, ultimoDia);

    int ultimoSabado = ultimoDia -
        ((int)ultimoDiaMes.DayOfWeek + 1) % 7;

    // Encontrar el primer sábado del mes
    int primerSabado = 1;

    while (new DateOnly(anio, mes, primerSabado).DayOfWeek
           != DayOfWeek.Saturday)
    {
        primerSabado++;
    }

    // Calcular el sábado correspondiente
    int dia = primerSabado + (numeroSabado - 1) * 7;

    // Si no existe, utilizar el último sábado
    dia = Math.Min(dia, ultimoSabado);

    return new DateOnly(anio, mes, dia);
}

// ==========================================
// CONFIGURACIÓN DEL SERVIDOR
// ==========================================

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Configuración CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://custom-system-2.netlify.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("ReactPolicy");

// ==========================================
// PERFIL DEL EMPLEADO
// ==========================================

app.MapGet("/api/perfil/{usuarioId}", async (
    int usuarioId,
    ApplicationDbContext db) =>
{
    var empleado = await db.Empleados
        .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId);

    if (empleado == null)
    {
        return Results.NotFound(new
        {
            mensaje = "No se encontró el perfil del empleado"
        });
    }

    return Results.Ok(new
    {
        empleado.Id,
        empleado.UsuarioId,
        empleado.Nombre,
        empleado.Apellido,
        empleado.Telefono,
        empleado.Puesto,
        empleado.Departamento,
        empleado.FechaIngreso,
        empleado.Activo
    });
});

// ==========================================
// CALENDARIO INDIVIDUAL
// ==========================================

app.MapGet("/api/calendario/{empleadoId}", async (
    int empleadoId,
    int anio,
    int mes,
    ApplicationDbContext db) =>
{
    if (mes < 1 || mes > 12 || anio < 1 || anio > 9999)
    {
        return Results.BadRequest(new
        {
            mensaje = "El año o mes no son válidos"
        });
    }

    var horario = await db.HorariosEmpleados
        .AsNoTracking()
        .FirstOrDefaultAsync(h => h.EmpleadoId == empleadoId);

    if (horario == null)
    {
        return Results.NotFound(new
        {
            mensaje = "No se encontró el horario del empleado"
        });
    }

    var primerDia = new DateOnly(anio, mes, 1);

    var ultimoDia = new DateOnly(
        anio,
        mes,
        DateTime.DaysInMonth(anio, mes)
    );

    var vacaciones = await db.SolicitudesVacaciones
        .AsNoTracking()
        .Where(v =>
            v.EmpleadoId == empleadoId &&
            v.Estado == "APROBADA" &&
            v.FechaInicio <= ultimoDia &&
            v.FechaFin >= primerDia
        )
        .Select(v => new
        {
            v.FechaInicio,
            v.FechaFin
        })
        .ToListAsync();

    var guardia = CalcularGuardiaMensual(
        horario.PrimeraGuardia,
        anio,
        mes
    );

    return Results.Ok(new
    {
        empleadoId,
        diaDescanso = horario.DiaDescanso,
        guardia,
        vacaciones
    });
});

// ==========================================
// CALENDARIO GENERAL DE TODOS LOS EMPLEADOS
// ==========================================

app.MapGet("/api/calendario", async (
    int anio,
    int mes,
    ApplicationDbContext db) =>
{
    // Validar fechas
    if (mes < 1 || mes > 12 || anio < 1 || anio > 9999)
    {
        return Results.BadRequest(new
        {
            mensaje = "El año o mes no son válidos"
        });
    }

    // Obtener todos los empleados
    var empleados = await db.Empleados
        .AsNoTracking()
        .Where(e => e.Activo)
        .ToListAsync();

    var empleadoIds = empleados
        .Select(e => e.Id)
        .ToList();

    // Obtener horarios
    var horarios = await db.HorariosEmpleados
        .AsNoTracking()
        .Where(h => empleadoIds.Contains(h.EmpleadoId))
        .ToListAsync();

    // Fechas del mes
    var primerDia = new DateOnly(anio, mes, 1);

    var ultimoDia = new DateOnly(
        anio,
        mes,
        DateTime.DaysInMonth(anio, mes)
    );

    // Obtener vacaciones aprobadas
    var vacaciones = await db.SolicitudesVacaciones
        .AsNoTracking()
        .Where(v =>
            empleadoIds.Contains(v.EmpleadoId) &&
            v.Estado == "APROBADA" &&
            v.FechaInicio <= ultimoDia &&
            v.FechaFin >= primerDia
        )
        .ToListAsync();

    // Organizar horarios por empleado
    var horariosPorEmpleado = horarios
        .ToDictionary(h => h.EmpleadoId);

    // Organizar vacaciones por empleado
    var vacacionesPorEmpleado = vacaciones
        .GroupBy(v => v.EmpleadoId)
        .ToDictionary(g => g.Key, g => g.ToList());

    // Construir información del calendario
    var calendario = empleados.Select(empleado =>
    {
        horariosPorEmpleado.TryGetValue(
            empleado.Id,
            out var horario
        );

        vacacionesPorEmpleado.TryGetValue(
            empleado.Id,
            out var vacacionesEmpleado
        );

        DateOnly? guardia = horario != null
            ? CalcularGuardiaMensual(
                horario.PrimeraGuardia,
                anio,
                mes
            )
            : null;

        return new
        {
            empleadoId = empleado.Id,
            empleado.Nombre,
            empleado.Apellido,
            empleado.Puesto,

            diaDescanso = horario?.DiaDescanso,

            guardia,

            vacaciones = vacacionesEmpleado == null
                ? new List<object>()
                : vacacionesEmpleado
                    .Select(v => (object)new
                    {
                        v.FechaInicio,
                        v.FechaFin
                    })
                    .ToList()
        };
    }).ToList();

    return Results.Ok(new
    {
        anio,
        mes,
        empleados = calendario
    });
});

// ==========================================
// ESTADO DEL SERVIDOR
// ==========================================

app.MapGet("/api/status", () =>
{
    return Results.Ok(new
    {
        name = "Ivan Osmar",
        estado = "Relacion",
        version = "Quien sabe (24.3)"
    });
});

// ==========================================
// CONSULTAR USUARIOS
// ==========================================

app.MapGet("/api/usuarios", async (ApplicationDbContext db) =>
{
    var usuarios = await db.Usuarios.ToListAsync();

    return Results.Ok(usuarios);
});

// ==========================================
// CREAR USUARIO
// ==========================================

app.MapPost("/api/creacion-usuario", async (
    Usuario usuario,
    ApplicationDbContext db) =>
{
    db.Usuarios.Add(usuario);

    await db.SaveChangesAsync();

    return Results.Created(
        $"/api/creacion-usuario/{usuario.Id}",
        usuario
    );
});

// ==========================================
// INICIAR SESIÓN
// ==========================================


app.MapPost("/api/login", async (
    LoginRequest request,
    ApplicationDbContext db) =>
{
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Correo == request.Correo);

    if (usuario == null || !usuario.Activo)
    {
        return Results.Unauthorized();
    }

    if (usuario.ContrasenaHash != request.Contrasena)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new
    {
        mensaje = "Inicio de sesión correcto",
        usuario.Id,
        usuario.Nombre,
        usuario.Correo,
        usuario.Rol
    });
});


// ==========================================
// INICIAR APLICACIÓN
// ==========================================

app.Run();
