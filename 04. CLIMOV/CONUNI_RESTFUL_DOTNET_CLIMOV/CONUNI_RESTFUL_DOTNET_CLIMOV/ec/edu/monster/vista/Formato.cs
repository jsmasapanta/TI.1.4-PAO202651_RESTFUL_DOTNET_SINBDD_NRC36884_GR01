using System.Globalization;
using ec.edu.monster.modelo;

namespace ec.edu.monster.vista;

/// <summary>Cómo se muestran los datos en pantalla. No tiene reglas: los rangos los valida el servidor.</summary>
public static class Formato
{
    /// <summary>
    /// Convierte lo escrito ("12.5" o "12,5") al número que recibe el servicio; null si no es un número.
    /// Si está fuera de rango (por ejemplo bajo el cero absoluto) lo rechaza el servidor.
    /// </summary>
    public static double? LeerNumero(string? texto)
    {
        string limpio = (texto ?? "").Trim().Replace(',', '.');
        return double.TryParse(limpio, NumberStyles.Float, CultureInfo.InvariantCulture, out double numero)
               && double.IsFinite(numero)
            ? numero
            : null;
    }

    /// <summary>Hasta 6 decimales, sin ceros sobrantes.</summary>
    public static string Numero(double numero)
    {
        string texto = Math.Round(numero, 6).ToString("0.######", CultureInfo.CurrentCulture);
        return texto == "-0" ? "0" : texto;
    }

    public static string Simbolo(Categoria categoria) => categoria switch
    {
        Categoria.Temperatura => "°C",
        Categoria.Longitud => "m",
        _ => "kg"
    };
}
