using Microsoft.AspNetCore.Mvc;
using GestionOrden.Application;
using GestionOrden.Domain;
using Microsoft.AspNetCore.Authorization;
using GestionOrden.Api.Controllers.Presentacion;
using GestionOrden.Infrastructure.Contratos;

namespace GestionOrden.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly ProductosServicio _servicio;

    public ProductosController(ProductosServicio servicio) => _servicio = servicio;

    [HttpGet]
    [Authorize(Roles = $"{RolesNombres.Administrador},{RolesNombres.Operador},{RolesNombres.Consulta}")]
    public async Task<ActionResult<ResultadoPaginado<ProductoRespuesta>>> Listar(
        [FromQuery] int? numeroPagina,
        [FromQuery] int? tamanoPagina,
        [FromQuery] int? categoriaId,
        [FromQuery] string? texto,
        [FromQuery] bool? soloActivos,
        CancellationToken cancelacion)
    {
        var (n, t) = Paginacion.Normalizar(numeroPagina, tamanoPagina);
        var resultado = await _servicio.ListarAsync(n, t, categoriaId, texto, soloActivos, cancelacion);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{RolesNombres.Administrador},{RolesNombres.Operador},{RolesNombres.Consulta}")]
    public async Task<ActionResult<ProductoRespuesta>> Obtener(int id, CancellationToken cancelacion)
    {
        var producto = await _servicio.ObtenerAsync(id, cancelacion);
        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpPost]
    [Authorize(Roles = RolesNombres.Administrador)]
    public async Task<ActionResult<ProductoRespuesta>> Crear(
        [FromBody] CrearProductoSolicitud solicitud,
        CancellationToken cancelacion)
    {
        var creado = await _servicio.CrearAsync(solicitud, UsuarioAuditoria.Identificador(User), cancelacion);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RolesNombres.Administrador)]
    public async Task<ActionResult<ProductoRespuesta>> Actualizar(
        int id,
        [FromBody] ActualizarProductoSolicitud solicitud,
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