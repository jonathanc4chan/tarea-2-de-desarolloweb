using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string rutaCarpetaCompartida = @"\\localhost\share";
string nombreArchivo = "texto.txt";

app.UseStaticFiles();

app.MapGet("/", () =>
{
    return Results.Text(
      @"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Servicio de Texto SAMBA</title>
    <style>
        body {
            font-family: 'Segoe UI', sans-serif;
            background: #e6f0f8;
            margin: 0;
            padding: 30px;
            display: flex;
            justify-content: center;
        }

        .container {
            background: white;
            border-radius: 12px;
            padding: 30px;
            box-shadow: 0 10px 20px rgba(0,0,0,0.1);
            max-width: 600px;
            width: 100%;
        }

        h1 {
            text-align: center;
            color: #2163af;
            font-size: 24px;
            margin-bottom: 5px;
        }

        .subinfo {
            text-align: center;
            font-size: 14px;
            color: #555;
            margin-bottom: 25px;
        }

        h2 {
            font-size: 18px;
            margin-bottom: 10px;
            color: #333;
        }

        label {
            display: block;
            margin-top: 15px;
            margin-bottom: 5px;
            color: #333;
            font-weight: 600;
        }

        input[type='text'], textarea {
            width: 100%;
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 6px;
            box-sizing: border-box;
            font-size: 15px;
        }

        textarea {
            resize: vertical;
            height: 120px;
        }

        button {
            margin-top: 15px;
            padding: 12px;
            border: none;
            border-radius: 6px;
            font-size: 16px;
            font-weight: bold;
            color: white;
            cursor: pointer;
        }

        #guardar {
            background-color: #007bff;
            width: 100%;
        }

        #guardar:hover {
            background-color: #0056b3;
        }

        #cargar {
            background-color: #007bff;
            width: 100%;
        }

        #cargar:hover {
            background-color: #0056b3;
        }

        #resultado {
            margin-top: 20px;
            padding: 10px;
            border-radius: 5px;
            font-size: 14px;
            display: none;
        }

        .success {
            background-color: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
        }

        .error {
            background-color: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }

        .preview {
            margin-top: 15px;
            background: #f8f9fa;
            border-radius: 6px;
            padding: 10px;
            white-space: pre-wrap;
            font-family: monospace;
            font-size: 14px;
            color: #222;
        }
    </style>
</head>
<body>
    <div class='container'>
        <h1>📁 Servicio de Texto SAMBA</h1>
        <div class='subinfo'>
            Carpeta compartida: //192.168.1.13/Compartido
        </div>

        <h2>✏️ Guardar Texto (POST)</h2>
        <label for='nombre'>Nombre del archivo:</label>
        <input type='text' id='nombre' placeholder='Ej: archivo.txt'>

        <label for='texto'>Contenido:</label>
        <textarea id='texto' placeholder='Escribe aquí el contenido...'></textarea>

        <button id='guardar'>Guardar Texto</button>
        <div id='resultado'></div>

        <h2 style='margin-top: 35px;'>📄 Leer Texto (GET)</h2>
        <label for='nombreLeer'>Nombre del archivo:</label>
        <input type='text' id='nombreLeer' placeholder='Ej: archivo.txt' value='archivo.txt'>

        <button id='cargar'>Leer Texto</button>
        <div id='preview' class='preview'></div>
    </div>

    <script>
        const mostrarResultado = (mensaje, tipo) => {
            const resultado = document.getElementById('resultado');
            resultado.textContent = mensaje;
            resultado.className = tipo;
            resultado.style.display = 'block';
            setTimeout(() => resultado.style.display = 'none', 5000);
        };

        document.getElementById('guardar').addEventListener('click', async () => {
            const archivo = document.getElementById('nombre').value.trim();
            const texto = document.getElementById('texto').value;
            if (!archivo) {
                mostrarResultado('Debes escribir un nombre de archivo.', 'error');
                return;
            }
            try {
                const response = await fetch('/guardar-texto?archivo=' + encodeURIComponent(archivo), {
                    method: 'POST',
                    headers: { 'Content-Type': 'text/plain' },
                    body: texto
                });
                const msg = await response.text();
                if (response.ok) {
                    mostrarResultado(msg, 'success');
                } else {
                    mostrarResultado('Error: ' + msg, 'error');
                }
            } catch (err) {
                mostrarResultado('Error de conexión: ' + err.message, 'error');
            }
        });

        document.getElementById('cargar').addEventListener('click', async () => {
            const archivo = document.getElementById('nombreLeer').value.trim();
            if (!archivo) return;
            try {
                const response = await fetch('/leer-texto?archivo=' + encodeURIComponent(archivo));
                const contenido = await response.text();
                if (response.ok) {
                    document.getElementById('preview').textContent = contenido;
                } else {
                    document.getElementById('preview').textContent = 'Error: ' + contenido;
                }
            } catch (err) {
                document.getElementById('preview').textContent = 'Error de conexión: ' + err.message;
            }
        });
    </script>
</body>
</html>
", "text/html");
});

app.MapPost("/guardar-texto", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    string contenido = await reader.ReadToEndAsync();
    string rutaArchivo = Path.Combine(rutaCarpetaCompartida, nombreArchivo);

    try
    {
        await File.WriteAllTextAsync(rutaArchivo, contenido, Encoding.UTF8);
        return Results.Ok("Texto guardado correctamente.");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al guardar: {ex.Message}");
    }
});

app.MapGet("/leer-texto", () =>
{
    string rutaArchivo = Path.Combine(rutaCarpetaCompartida, nombreArchivo);

    try
    {
        if (!File.Exists(rutaArchivo))
            return Results.NotFound("Archivo no encontrado.");

        string contenido = File.ReadAllText(rutaArchivo);
        return Results.Ok(contenido);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al leer: {ex.Message}");
    }
});

app.Run();
