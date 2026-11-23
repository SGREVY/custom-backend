namespace Backend_custom_test.API.Models;

public class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string ContrasenaHash { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}