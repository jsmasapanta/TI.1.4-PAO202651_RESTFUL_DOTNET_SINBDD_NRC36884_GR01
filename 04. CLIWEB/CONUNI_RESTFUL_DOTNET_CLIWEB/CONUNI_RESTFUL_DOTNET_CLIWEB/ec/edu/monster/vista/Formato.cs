using System.Globalization;
using ec.edu.monster.modelo;

namespace ec.edu.monster.vista;

/// <summary>Cómo se muestran los datos en pantalla. No tiene reglas: los rangos los valida el servidor.</summary>
public static class Formato
{
    /// <summary>Hasta 6 decimales, sin ceros sobrantes.</summary>
    public static string Numero(double numero) =>
        Math.Round(numero, 6).ToString("0.######", CultureInfo.InvariantCulture);

    /// <summary>Texto de la unidad en las listas: "Celsius (°C)".</summary>
    public static string Texto(Unidad unidad) => $"{unidad.Nombre} ({unidad.Simbolo})";
}
