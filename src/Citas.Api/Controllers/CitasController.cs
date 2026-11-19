using Citas.Api.Contracts;
using Citas.Application.Citas;
using Citas.Application.Common;
using Citas.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Api.Controllers;

[ApiController]
[Route("api/citas")]
public sealed class CitasController(ISender sender) : ControllerBase
{
    /// <summary>Lista citas con filtros y paginación.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<CitaDto>>> Listar(
        [FromQuery] Guid? medicoId,
        [FromQuery] EstadoCita? estado,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? pacienteIdentidad,
        CancellationToken ct,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20) =>
        Ok(await sender.Send(
            new ListarCitasQuery(medicoId, estado, desde, hasta, pacienteIdentidad, page, pageSize), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CitaDto>> Obtener(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new ObtenerCitaQuery(id), ct));

    /// <summary>Programa una cita validando traslapes de médico y paciente.</summary>
    [HttpPost]
    public async Task<ActionResult<CitaDto>> Programar(ProgramarCitaCommand body, CancellationToken ct)
    {
        var dto = await sender.Send(body, ct);
        return CreatedAtAction(nameof(Obtener), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}/reprogramar")]
    public async Task<ActionResult<CitaDto>> Reprogramar(Guid id, ReprogramarRequest body, CancellationToken ct) =>
        Ok(await sender.Send(new ReprogramarCitaCommand(id, body.FechaHoraInicio, body.DuracionMinutos), ct));

    [HttpPost("{id:guid}/confirmar")]
    public async Task<ActionResult<CitaDto>> Confirmar(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new ConfirmarCitaCommand(id), ct));

    [HttpPost("{id:guid}/cancelar")]
    public async Task<ActionResult<CitaDto>> Cancelar(Guid id, CancelarRequest body, CancellationToken ct) =>
        Ok(await sender.Send(new CancelarCitaCommand(id, body.Motivo), ct));

    [HttpPost("{id:guid}/completar")]
    public async Task<ActionResult<CitaDto>> Completar(Guid id, CompletarRequest body, CancellationToken ct) =>
        Ok(await sender.Send(new CompletarCitaCommand(id, body.Notas), ct));

    [HttpPost("{id:guid}/no-asistio")]
    public async Task<ActionResult<CitaDto>> NoAsistio(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new MarcarNoAsistioCommand(id), ct));
}
