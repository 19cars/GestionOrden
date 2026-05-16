using GestionOrden.Infrastructure.Contratos;

namespace GestionOrden.Infrastructure;

public sealed class ExcepcionReglaNegocio : Exception
{
    public int CodigoEstado { get; }
    public string Codigo { get; }
    public IReadOnlyList<ErrorNegocioInterno>? ErroresInternos { get; }

    public ExcepcionReglaNegocio(
        int codigoEstado,
        string codigo,
        string mensaje,
        IReadOnlyList<ErrorNegocioInterno>? erroresInternos = null) : base(mensaje)
    {
        CodigoEstado = codigoEstado;
        Codigo = codigo;
        ErroresInternos = erroresInternos;
    }
}
