
namespace Backend_custom_test.API.Models;

public class HorarioEmpleado
{
    public int Id { get; set; }

    public int EmpleadoId { get; set; }

    public short DiaDescanso { get; set; }

    public DateOnly PrimeraGuardia { get; set; }
}
