using Backend_custom_test.API.Data;
using Backend_custom_test.API.Models;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

//var es la variable en c#
//builder es el nombre de la variable
//webaplication es una clase de asp.net
//create builder crea el constructor para preparar la configuracion de nuestra app
//args resive los argumentos que pueden enviarse al ejecutar el programa

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
        .WithOrigins("http://localhost:5173",
        "https://custom-system-2.netlify.app")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});


var app = builder.Build();
//basicamente es otra variable para que funcione la variable de arriba
//builder = planos y materiales 
//build = contruye el edificio 
//app = edificio construido

app.UseCors("ReactPolicy");


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



app.MapGet("/api/status", () =>
{
    return Results.Ok(new
    {
        name = "Ivan Osmar",
        estado = "Relacion",
        version = "Quien sabe (24.3)"
    });
});

app.MapGet("/api/usuarios", async (ApplicationDbContext db) =>
{
    var usuarios = await db.Usuarios.ToListAsync();

    return Results.Ok(usuarios);
});

app.MapPost("/api/creacion-usuario", async (Usuario usuario, ApplicationDbContext db) =>
{
    
db.Usuarios.Add(usuario);

await db.SaveChangesAsync();

return Results.Created($"/api/creacion-usuario/{usuario.Id}",usuario);


});

app.MapPost("/api/login", async (LoginRequest request, ApplicationDbContext db) =>
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
        usuario.Correo
    });
});

app.Run();
