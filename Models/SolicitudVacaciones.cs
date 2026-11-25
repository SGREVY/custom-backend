
namespace Backend_custom_test.API.Models;

public class SolicitudVacaciones
{
    public int Id { get; set; }

    public int EmpleadoId { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public string Estado { get; set; } = "PENDIENTE";

    public DateTime FechaSolicitud { get; set; }

    public DateTime? FechaRespuesta { get; set; }
}
