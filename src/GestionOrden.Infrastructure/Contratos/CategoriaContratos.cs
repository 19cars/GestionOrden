namespace GestionOrden.Infrastructure.Contratos;

public sealed record CategoriaRespuesta(
    int Id,
    string Nombre,
    bool Activo,
    DateTimeOffset FechaCreacion,
    string CreadoPor,
    DateTimeOffset? FechaModificacion,
    string? ModificadoPor);

public sealed record CrearCategoriaSolicitud(string Nombre);

public sealed record ActualizarCategoriaSolicitud(string Nombre, bool Activo);
