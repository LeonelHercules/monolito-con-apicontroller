using Citas.Domain.Entities;
using Citas.Domain.Enums;
using Citas.Domain.Exceptions;

namespace Citas.Domain.Tests;

public class CitaTests
{
    // "Ahora" fijo para todas las pruebas: 1 de diciembre de 2026, 8:00 a. m.
    private static readonly DateTime Ahora = new(2026, 12, 1, 8, 0, 0);
    private static readonly DateTime AhoraUtc = Ahora.AddHours(6);

    private static Cita CrearCita(DateTime inicio, int duracion = 30) =>
        Cita.Programar(
            medicoId: Guid.NewGuid(),
            pacienteNombre: "Carlos Mejía",
            pacienteIdentidad: "0801-1990-12345",
            pacienteTelefono: null,
            pacienteCorreo: null,
            inicio: inicio,
            duracionMinutos: duracion,
            motivo: "Control general",
            notas: null,
            ahoraLocal: Ahora,
            ahoraUtc: AhoraUtc);

    [Fact]
    public void Programar_crea_la_cita_en_estado_programada_y_calcula_el_fin()
    {
        var cita = CrearCita(Ahora.AddHours(1), duracion: 45);

        Assert.Equal(EstadoCita.Programada, cita.Estado);
        Assert.Equal(Ahora.AddHours(1).AddMinutes(45), cita.FechaHoraFin);
        Assert.Equal(45, cita.DuracionMinutos);
    }

    [Fact]
    public void Programar_normaliza_la_identidad_quitando_guiones()
    {
        var cita = CrearCita(Ahora.AddHours(1));

        Assert.Equal("0801199012345", cita.PacienteIdentidad);
    }

    [Fact]
    public void No_se_puede_programar_en_el_pasado()
    {
        var ex = Assert.Throws<DomainException>(() => CrearCita(Ahora.AddMinutes(-1)));

        Assert.Contains("pasada", ex.Message);
    }

    [Theory]
    [InlineData(9)]
    [InlineData(241)]
    public void La_duracion_tiene_que_estar_entre_10_y_240_minutos(int duracion)
    {
        Assert.Throws<DomainException>(() => CrearCita(Ahora.AddHours(1), duracion));
    }

    [Fact]
    public void No_se_puede_confirmar_una_cita_cancelada()
    {
        var cita = CrearCita(Ahora.AddHours(1));
        cita.Cancelar("El paciente avisó que no viene", AhoraUtc);

        Assert.Throws<DomainException>(() => cita.Confirmar(AhoraUtc));
    }

    [Fact]
    public void Completar_solo_funciona_despues_de_la_hora_de_inicio()
    {
        var inicio = Ahora.AddHours(1);
        var cita = CrearCita(inicio);

        // A las 8:00, la cita de las 9:00 todavía no empezó
        Assert.Throws<DomainException>(() => cita.Completar(null, Ahora, AhoraUtc));

        // A las 9:30 sí se puede
        var despues = inicio.AddMinutes(30);
        cita.Completar("Paciente estable", despues, despues.AddHours(6));

        Assert.Equal(EstadoCita.Completada, cita.Estado);
        Assert.Equal("Paciente estable", cita.Notas);
    }

    [Fact]
    public void Reprogramar_una_cita_confirmada_la_devuelve_a_programada()
    {
        var cita = CrearCita(Ahora.AddHours(1));
        cita.Confirmar(AhoraUtc);

        cita.Reprogramar(Ahora.AddDays(1), 30, Ahora, AhoraUtc);

        Assert.Equal(EstadoCita.Programada, cita.Estado);
    }
}
