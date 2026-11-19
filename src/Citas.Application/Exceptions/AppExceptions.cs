namespace Citas.Application.Exceptions;

public sealed class NotFoundException(string recurso, object id)
    : Exception($"{recurso} con id '{id}' no fue encontrado.");

public sealed class ConflictException(string message) : Exception(message);
