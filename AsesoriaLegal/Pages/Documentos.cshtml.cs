
using System; // Funciones generales de C#, como Guid y Convert.
using System.Collections.Generic; // Permite utilizar listas.
using System.IO; // Permite trabajar con archivos y carpetas.
using System.Linq; // Permite filtrar y transformar colecciones.
using System.Security.Cryptography; // Proporciona funciones criptográficas.
using System.Text; // Permite convertir texto a bytes.
using Microsoft.AspNetCore.Http; // Incluye Session, cookies e IFormFile.
using Microsoft.AspNetCore.Mvc; // Permite redireccionar entre páginas.
using Microsoft.AspNetCore.Mvc.RazorPages; // Permite crear páginas Razor.

namespace AsesoriaLegal.Pages
{
    // Modelo que controla la lógica de la página Documentos.cshtml.
    public class DocumentosModel : PageModel
    {
        // Define el tamaño máximo permitido: 5 MB en bytes.
        private const long TamanoMaximo = 5 * 1024 * 1024;

        // Lista de extensiones de archivo que aceptamos.
        private readonly string[] extensionesPermitidas =
        {
            ".pdf", ".doc", ".docx"
        };

        // Guarda el nombre del usuario que inició sesión.
        // La vista utiliza esta propiedad para mostrar su nombre.
        public string NombreUsuario { get; set; } = string.Empty;

        // Lista de nombres de los documentos del usuario.
        public List<string> Archivos { get; set; } = new();

        // Permite mostrar mensajes entre una petición y otra.
        // Por ejemplo, después de subir un archivo.
        [TempData]
        public string? Mensaje { get; set; }

        // Permite mostrar errores al usuario.
        [TempData]
        public string? Error { get; set; }

        // Recibe el archivo seleccionado en el formulario.
        [BindProperty]
        public IFormFile? Archivo { get; set; }

        // Construye la ruta de la carpeta del usuario.
        private string ObtenerCarpetaUsuario(string usuario)
        {
            // Convierte el nombre del usuario en una secuencia de bytes.
            byte[] datos = Encoding.UTF8.GetBytes(usuario);

            // Calcula un hash SHA-256 para obtener un identificador.
            // Esto evita usar directamente el nombre en la ruta.
            string identificador = Convert.ToHexString(
                SHA256.HashData(datos));

            // Construye la ruta de almacenamiento de sus documentos.
            // Los archivos quedan fuera de la carpeta pública wwwroot.
            return Path.Combine(
                Directory.GetCurrentDirectory(),
                "App_Data",
                "Documentos",
                identificador);
        }

        // Se ejecuta cuando el usuario abre la página mediante GET.
        public IActionResult OnGet()
        {
            // Consulta la sesión para saber quién inició sesión.
            string? usuario = HttpContext.Session.GetString(
                "NombreUsuario");

            // Si no hay usuario autenticado, lo envía al Login.
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return RedirectToPage("/Login");
            }

            // Guarda el nombre para mostrarlo en la vista.
            NombreUsuario = usuario;

            // Obtiene la carpeta correspondiente a ese usuario.
            string carpeta = ObtenerCarpetaUsuario(usuario);

            // Comprueba si la carpeta existe antes de leerla.
            if (Directory.Exists(carpeta))
            {
                // Obtiene los archivos de la carpeta.
                Archivos = Directory
                    .GetFiles(carpeta)

                    // Extrae solamente el nombre de cada archivo.
                    .Select(Path.GetFileName)

                    // Descarta posibles nombres nulos.
                    .Where(nombre => nombre != null)

                    // Convierte los nombres restantes en una lista.
                    .Select(nombre => nombre!)
                    .ToList();
            }

            // Indica que debe mostrarse la página.
            return Page();
        }

        // Se ejecuta al enviar el formulario con el botón Subir.
        // El nombre del método corresponde al handler "Subir".
        public IActionResult OnPostSubir()
        {
            // Comprueba nuevamente que exista una sesión iniciada.
            string? usuario = HttpContext.Session.GetString(
                "NombreUsuario");

            // Si no hay sesión, impide la carga y redirige al Login.
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return RedirectToPage("/Login");
            }

            // Verifica que se haya seleccionado un archivo no vacío.
            if (Archivo == null || Archivo.Length == 0)
            {
                Error = "Seleccioná un archivo antes de subirlo.";
                return RedirectToPage();
            }

            // Comprueba que el archivo no supere los 5 MB.
            if (Archivo.Length > TamanoMaximo)
            {
                Error = "El archivo no puede superar los 5 MB.";
                return RedirectToPage();
            }

            // Obtiene la extensión del archivo seleccionado.
            string extension = Path.GetExtension(Archivo.FileName);

            // Compara la extensión con la lista permitida.
            // Ignora diferencias entre mayúsculas y minúsculas.
            if (!extensionesPermitidas.Contains(
                extension, StringComparer.OrdinalIgnoreCase))
            {
                Error = "Formato no permitido. Solo se aceptan PDF, DOC y DOCX.";
                return RedirectToPage();
            }

            // Obtiene la carpeta personal del usuario actual.
            string carpeta = ObtenerCarpetaUsuario(usuario);

            try
            {
                // Crea la carpeta si todavía no existe.
                Directory.CreateDirectory(carpeta);

                // Genera un nombre aleatorio y único para el archivo.
                // No utiliza el nombre original enviado por el usuario.
                string nombreSeguro =
                    Guid.NewGuid().ToString("N")
                    + extension.ToLowerInvariant();

                // Combina la carpeta y el nombre para formar la ruta.
                string ruta = Path.Combine(carpeta, nombreSeguro);

                // Crea el archivo y lo abre para escribir.
                // CreateNew evita sobrescribir uno que ya exista.
                using (var flujo = new FileStream(
                    ruta,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    // Copia el contenido recibido al archivo del servidor.
                    Archivo.CopyTo(flujo);
                } // El archivo se cierra automáticamente al salir del using.

                // Informa que la carga terminó correctamente.
                Mensaje = "El documento se subió correctamente.";
            }
            // Captura errores relacionados con operaciones de archivos.
            catch (IOException)
            {
                Error = "No se pudo guardar el documento. Intentá nuevamente.";
            }
            // Captura errores por falta de permisos del sistema.
            catch (UnauthorizedAccessException)
            {
                Error = "El servidor no tiene permiso para guardar archivos.";
            }

            // Redirige a la página para mostrar el resultado actualizado.
            return RedirectToPage();
        }
    }
}
