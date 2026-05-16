using System.Security.Claims;

namespace GestionOrden.Api.Controllers.Presentacion;

public static class UsuarioAuditoria
{
    public static string Identificador(ClaimsPrincipal usuario)
    {
        return usuario.FindFirstValue(ClaimTypes.Email)
               ?? usuario.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? "sistema";
    }
}
