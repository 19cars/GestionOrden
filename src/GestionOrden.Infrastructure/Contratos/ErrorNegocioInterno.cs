namespace GestionOrden.Infrastructure.Contratos;

public sealed record ErrorNegocioInterno(string Codigo, string Mensaje, int? ProductoId);
