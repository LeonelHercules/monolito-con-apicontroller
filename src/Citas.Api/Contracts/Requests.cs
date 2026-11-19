namespace Citas.Api.Contracts;

/// <summary>Cuerpo para crear o actualizar un médico. En el PUT, el Id viene de la ruta.</summary>
public sealed record MedicoRequest(
    string Nombres, string Apellidos, string Especialidad,
    string NumeroColegiacion, string? Telefono, string? Correo);

public sealed record ReprogramarRequest(DateTime FechaHoraInicio, int DuracionMinutos);

public sealed record CancelarRequest(string Motivo);

public sealed record CompletarRequest(string? Notas);
