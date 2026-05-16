using GestionOrden.Domain;
using GestionOrden.Infrastructure.Contratos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GestionOrden.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutenticacionController : ControllerBase
{
    private readonly UserManager<UsuarioAplicacion> _usuarios;

    public AutenticacionController(
        UserManager<UsuarioAplicacion> usuarios)
    {
        _usuarios = usuarios;
    }

    private string CrearToken(LoginRequest req, IEnumerable<string> roles)
    {
        // In a real app validate user credentials against a user store.
        if (string.IsNullOrEmpty(req.Usuario) || string.IsNullOrEmpty(req.Contrasena))
            return "Error de credenciales";

        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "GestionOrden";
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "GestionOrdenAudience";

        if (string.IsNullOrEmpty(jwtSecret))
            return "JWT no configurado";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, req.Usuario),
            new Claim("role", "User")
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var expires = DateTime.UtcNow.AddHours(2);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenString;
    }

    [HttpPost("iniciar-sesion")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IniciarSesionRespuesta), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IniciarSesionRespuesta>> IniciarSesion(
        [FromBody] LoginRequest solicitud,
        CancellationToken cancelacion)
    {
        var usuario = await _usuarios.FindByEmailAsync(solicitud.Usuario.Trim())
                      ?? await _usuarios.FindByNameAsync(solicitud.Usuario.Trim());

        if (usuario is null)
        {
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });
        }

        var passwordValida = await _usuarios.CheckPasswordAsync(usuario, solicitud.Contrasena);
        if (!passwordValida)
        {
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });
        }

        var roles = await _usuarios.GetRolesAsync(usuario);
        var token = CrearToken(solicitud, roles);
        var expira = DateTime.UtcNow.AddHours(2);

        return Ok(new IniciarSesionRespuesta(token, expira, roles.ToList()));
    }

    public class LoginRequest
    {
        public string Usuario { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
    }
}
