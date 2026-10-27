
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AsesoriaLegal.Pages
{
    public class RegistroModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresá un correo electrónico válido.")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string NombreUsuario { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "La edad es obligatoria.")]
        [Range(16, 120, ErrorMessage = "Debés tener entre 16 y 120 años.")]
        public int? Edad { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Confirmá tu contraseña.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Contrasena),
            ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContrasena { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string Apellido { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Guardar el nombre de usuario en la sesión
            HttpContext.Session.SetString(
                "UsuarioRegistrado", NombreUsuario);

            // Configurar la cookie solicitada en la consigna
            CookieOptions opcionesCookie = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddMinutes(30),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            // Guardar la contraseña en una cookie
            Response.Cookies.Append(
                "Contrasena", Contrasena, opcionesCookie);

            Mensaje = "¡Registro realizado correctamente! "
                    + "Ahora podés iniciar sesión.";

            return Page();
        }
    }
}
