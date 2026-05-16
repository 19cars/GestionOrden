using GestionOrden.Domain.Core;

namespace GestionOrden.Infrastructure;

public static class AuditoriaExtensiones
{
    public static void AplicarCreacion(this IEntidadAuditable entidad, string usuario)
    {
        var ahora = DateTimeOffset.UtcNow;
        entidad.FechaCreacion = ahora;
        entidad.CreadoPor = usuario;
        entidad.FechaModificacion = null;
        entidad.ModificadoPor = null;
    }

    public static void AplicarModificacion(this IEntidadAuditable entidad, string usuario)
    {
        entidad.FechaModificacion = DateTimeOffset.UtcNow;
        entidad.ModificadoPor = usuario;
    }
}
