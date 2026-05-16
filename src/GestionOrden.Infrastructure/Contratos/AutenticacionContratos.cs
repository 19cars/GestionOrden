namespace GestionOrden.Infrastructure.Contratos;

public sealed record IniciarSesionSolicitud(string Usuario, string Contrasena);

public sealed record IniciarSesionRespuesta(string Token, DateTimeOffset Expira, IReadOnlyList<string> Roles);
