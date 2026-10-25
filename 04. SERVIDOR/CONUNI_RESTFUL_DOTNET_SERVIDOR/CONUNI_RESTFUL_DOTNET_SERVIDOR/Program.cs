using ConversorUnidades.Servidor.Logica;
using ConversorUnidades.Servidor.Modelo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Patrón Singleton: el Autenticador se crea una sola vez con las credenciales de appsettings.json.
Autenticador.Configurar(builder.Configuration);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(opciones =>
    {
        // Si falta "valor" o no es un número, responder con el mismo formato de error que el resto del API.
        opciones.InvalidModelStateResponseFactory = contexto =>
            new BadRequestObjectResult(new ErrorServicio("VALOR_INVALIDO", "Debe enviar un número válido en el parámetro 'valor'."));
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CONUNI · Conversor de Unidades RESTful .NET",
        Version = "v1",
        Description = "API REST sin base de datos: login y 16 conversiones de temperatura, longitud y masa. Usuario y clave: monster / monster69. El login permite 3 intentos; al tercero el usuario queda bloqueado 2 minutos."
    });
    string xml = Path.Combine(AppContext.BaseDirectory, "CONUNI_RESTFUL_DOTNET_SERVIDOR.xml");
    if (File.Exists(xml))
    {
        opciones.IncludeXmlComments(xml);
    }
});

// Los clientes (web, móvil) pueden estar en otra máquina u origen.
builder.Services.AddCors(opciones =>
    opciones.AddDefaultPolicy(politica => politica.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Swagger queda activo siempre: es la página de pruebas del servidor en la presentación.
app.UseSwagger();
app.UseSwaggerUI(opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "CONUNI REST v1");
    opciones.DocumentTitle = "CONUNI · RESTful .NET";
});

app.UseCors();
app.MapControllers();

// La raíz del servidor abre la página de Swagger.
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
