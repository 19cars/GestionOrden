using GestionOrden.Api.Controllers.Presentacion;
using GestionOrden.Application;
using GestionOrden.Domain;
using GestionOrden.Infrastructure.Contratos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOrden.Api.Controllers;

[ApiController]
[Route("api/categorias")]
[Authorize]
public sealed class CategoriasController : ControllerBase
{
    private readonly CategoriasServicio _servicio;

    public CategoriasController(CategoriasServicio servicio) => _servicio = servicio;

    [HttpGet]
    [Authorize(Roles = $"{RolesNombres.Administrador},{RolesNombres.Operador},{RolesNombres.Consulta}")]
    public async Task<ActionResult<ResultadoPaginado<CategoriaRespuesta>>> Listar(
        [FromQuery] int? numeroPagina,
        [FromQuery] int? tamanoPagina,
        CancellationToken cancelacion)
    {
        var (n, t) = Paginacion.Normalizar(numeroPagina, tamanoPagina);
        var resultado = await _servicio.ListarAsync(n, t, cancelacion);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{RolesNombres.Administrador},{RolesNombres.Operador},{RolesNombres.Consulta}")]
    public async Task<ActionResult<CategoriaRespuesta>> Obtener(int id, CancellationToken cancelacion)
    {
        var categoria = await _servicio.ObtenerAsync(id, cancelacion);
        return categoria is null ? NotFound() : Ok(categoria);
    }

    [HttpPost]
    [Authorize(Roles = RolesNombres.Administrador)]
    public async Task<ActionResult<CategoriaRespuesta>> Crear(
        [FromBody] CrearCategoriaSolicitud solicitud,
        CancellationToken cancelacion)
    {
        var creado = await _servicio.CrearAsync(solicitud, UsuarioAuditoria.Identificador(User), cancelacion);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RolesNombres.Administrador)]
    public async Task<ActionResult<CategoriaRespuesta>> Actualizar(
        int id,
        [FromBody] ActualizarCategoriaSolicitud solicitud,
        CancellationToken cancelacion)
    {
        var actualizado = await _servicio.ActualizarAsync(id, solicitud, UsuarioAuditoria.Identificador(User), cancelacion);
        return Ok(actualizado);
    }

    [HttpPost("{id:int}/desactivar")]
    [Authorize(Roles = RolesNombres.Administrador)]
    public async Task<IActionResult> Desactivar(int id, CancellationToken cancelacion)
    {
        await _servicio.DesactivarAsync(id, UsuarioAuditoria.Identificador(User), cancelacion);
        return NoContent();
    }
}
