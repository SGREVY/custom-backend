
namespace Backend_custom_test.API.Models;

public class Empleado
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public string? Puesto { get; set; }

    public string? Departamento { get; set; }

    public DateTime FechaIngreso { get; set; }

    public bool Activo { get; set; }
}
