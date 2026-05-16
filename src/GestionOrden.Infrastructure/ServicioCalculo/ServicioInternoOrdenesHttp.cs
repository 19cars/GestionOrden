using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GestionOrden.ContratosInternos.Ordenes;
using System.Net.Http.Json;

namespace GestionOrden.Infrastructure.ServicioCalculoy;

public sealed class OpcionesServicioInternoOrdenes
{
    public const string Seccion = "ServicioInternoOrdenes";
    public string UrlBase { get; set; } = "http://localhost:5081";
    public string NombreCabeceraClave { get; set; } = "X-Clave-Interna";
    public string ValorClave { get; set; } = string.Empty;
    public string RutaValidacion { get; set; } = "interno/ordenes/validar-y-calcular";
    public int SegundosEspera { get; set; } = 30;
}

public interface IServicioInternoOrdenes
{
    Task<RespuestaValidacionOrdenInterna?> ValidarYCalcularAsync(
        SolicitudValidacionOrdenInterna solicitud,
        CancellationToken cancelacion);
}

public sealed class ServicioInternoOrdenesHttp : IServicioInternoOrdenes
{
    private readonly HttpClient _http;
    private readonly OpcionesServicioInternoOrdenes _opciones;
    private readonly ILogger<ServicioInternoOrdenesHttp> _logger;

    public ServicioInternoOrdenesHttp(
        HttpClient http,
        IOptions<OpcionesServicioInternoOrdenes> opciones,
        ILogger<ServicioInternoOrdenesHttp> logger)
    {
        _http = http;
        _opciones = opciones.Value;
        _logger = logger;
    }

    public async Task<RespuestaValidacionOrdenInterna?> ValidarYCalcularAsync(
        SolicitudValidacionOrdenInterna solicitud,
        CancellationToken cancelacion)
    {
        var uri = _opciones.RutaValidacion.TrimStart('/');
        using var respuesta = await _http.PostAsJsonAsync(uri, solicitud, cancelacion);

        var cuerpo = await respuesta.Content.ReadAsStringAsync(cancelacion);
        _logger.LogInformation(
            "Servicio interno órdenes respondió {Codigo} con cuerpo: {Cuerpo}",
            (int)respuesta.StatusCode,
            cuerpo.Length > 2000 ? cuerpo[..2000] + "…" : cuerpo);

        if (!respuesta.IsSuccessStatusCode && respuesta.StatusCode != System.Net.HttpStatusCode.BadRequest)
        {
            _logger.LogError("Fallo llamada al servicio interno: {Codigo}", respuesta.StatusCode);
            return null;
        }

        var resultado = System.Text.Json.JsonSerializer.Deserialize<RespuestaValidacionOrdenInterna>(
            cuerpo,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return resultado;
    }
}
