using Citas.Application.Exceptions;
using Citas.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Api.Errores;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception ex, CancellationToken ct)
    {
        var (status, titulo) = ex switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Error de validación"),
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflicto"),
            DomainException => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio no cumplida"),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Error no controlado");

        var problem = new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = ex switch
            {
                ValidationException => "Uno o más campos no son válidos.",
                _ when status == StatusCodes.Status500InternalServerError => "Ocurrió un error inesperado.",
                _ => ex.Message
            }
        };

        if (ex is ValidationException ve)
        {
            problem.Extensions["errors"] = ve.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problem,
            Exception = ex
        });
    }
}
