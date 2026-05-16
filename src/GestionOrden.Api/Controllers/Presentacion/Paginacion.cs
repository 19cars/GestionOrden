namespace GestionOrden.Api.Controllers.Presentacion;

public static class Paginacion
{
    public static (int NumeroPagina, int TamanoPagina) Normalizar(int? numeroPagina, int? tamanoPagina)
    {
        var numero = numeroPagina is null or < 1 ? 1 : numeroPagina.Value;
        var tamanoBase = tamanoPagina is null or < 1 ? 20 : tamanoPagina.Value;
        var tamano = Math.Clamp(tamanoBase, 1, 100);
        return (numero, tamano);
    }
}
