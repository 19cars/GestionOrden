namespace GestionOrden.Infrastructure.Contratos;

public sealed record SolicitudValidacionOrdenInterna(IReadOnlyList<ItemOrdenInterno> Items);
