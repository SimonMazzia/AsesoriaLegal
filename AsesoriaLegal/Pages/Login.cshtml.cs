
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AsesoriaLegal.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string NombreUsuario { get; set; } = string.Empty;

        [BindProperty]
        public string Contrasena { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            string? usuarioGuardado =
                HttpContext.Session.GetString("UsuarioRegistrado");

            string? contrasenaGuardada =
                Request.Cookies["Contrasena"];

            if (usuarioGuardado == null || contrasenaGuardada == null)
            {
                Mensaje = "Primero debés registrarte en el sistema.";
                return Page();
            }

            if (NombreUsuario == usuarioGuardado &&
                Contrasena == contrasenaGuardada)
            {
                // Marcar al usuario como autenticado
                HttpContext.Session.SetString("NombreUsuario", NombreUsuario);

                return RedirectToPage("/Documentos");
            }

            Mensaje = "El usuario o la contraseña son incorrectos.";
            return Page();
        }
    }
}

