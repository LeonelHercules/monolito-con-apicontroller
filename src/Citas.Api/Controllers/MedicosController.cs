using Citas.Api.Contracts;
using Citas.Application.Common;
using Citas.Application.Medicos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Citas.Api.Controllers;

[ApiController]
[Route("api/medicos")]
public sealed class MedicosController(ISender sender) : ControllerBase
{
    /// <summary>Lista médicos con filtros y paginación.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<MedicoDto>>> Listar(
        [FromQuery] string? buscar,
        [FromQuery] string? especialidad,
        [FromQuery] bool? activo,
        CancellationToken ct,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20) =>
        Ok(await sender.Send(new ListarMedicosQuery(buscar, especialidad, activo, page, pageSize), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MedicoDto>> Obtener(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new ObtenerMedicoQuery(id), ct));

    [HttpPost]
    public async Task<ActionResult<MedicoDto>> Crear(MedicoRequest body, CancellationToken ct)
    {
        var dto = await sender.Send(new CrearMedicoCommand(
            body.Nombres, body.Apellidos, body.Especialidad,
            body.NumeroColegiacion, body.Telefono, body.Correo), ct);

        return CreatedAtAction(nameof(Obtener), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MedicoDto>> Actualizar(Guid id, MedicoRequest body, CancellationToken ct) =>
        Ok(await sender.Send(new ActualizarMedicoCommand(
            id, body.Nombres, body.Apellidos, body.Especialidad,
            body.NumeroColegiacion, body.Telefono, body.Correo), ct));

    [HttpPatch("{id:guid}/activar")]
    public async Task<ActionResult<MedicoDto>> Activar(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new CambiarEstadoMedicoCommand(id, true), ct));

    /// <summary>Baja lógica; falla si el médico tiene citas futuras pendientes.</summary>
    [HttpPatch("{id:guid}/desactivar")]
    public async Task<ActionResult<MedicoDto>> Desactivar(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new CambiarEstadoMedicoCommand(id, false), ct));

    /// <summary>Citas del médico en un día (?fecha=2030-03-04).</summary>
    [HttpGet("{id:guid}/agenda")]
    public async Task<ActionResult<AgendaMedicoDto>> Agenda(
        Guid id, [FromQuery, BindRequired] DateOnly fecha, CancellationToken ct) =>
        Ok(await sender.Send(new AgendaMedicoQuery(id, fecha), ct));
}
