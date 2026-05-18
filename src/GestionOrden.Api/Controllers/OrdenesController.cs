using GestionOrden.Api.Controllers.Presentacion;
using GestionOrden.Application;
using GestionOrden.Domain;
using GestionOrden.Infrastructure.Contratos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOrden.Api.Controllers;

[ApiController]
[Route("api/ordenes")]
[Authorize]
public sealed class OrdenesController : ControllerBase
{
    private readonly OrdenesServicio _servicio;

    public OrdenesController(OrdenesServicio servicio) => _servicio = servicio;

    [HttpGet]
    [Authorize(Roles = $"{RolesNombres.Administrador},{RolesNombres.Operador},{RolesNombres.Consulta}")]
    public async Task<ActionResult<ResultadoPaginado<OrdenResumenRespuesta>>> Listar(
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
    public async Task<ActionResult<OrdenDetalleRespuesta>> Obtener(int id, CancellationToken cancelacion)
    {
        var orden = await _servicio.ObtenerDetalleAsync(id, cancelacion);
        return orden is null ? NotFound() : Ok(orden);
    }

    [HttpPost]
    [Authorize(Roles = $"{RolesNombres.Administrador},{RolesNombres.Operador}")]
    public async Task<ActionResult<OrdenDetalleRespuesta>> Crear(
        [FromBody] CrearOrdenSolicitud solicitud,
        CancellationToken cancelacion)
    {
        var creado = await _servicio.CrearAsync(solicitud, UsuarioAuditoria.Identificador(User), cancelacion);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }
}
