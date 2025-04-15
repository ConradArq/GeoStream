using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GeoStream.Services;
using GeoStream.Dtos.Administration.Users;

namespace GeoStream.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IUserService _userService;

        [BindProperty]
        public LoginDto LoginDto { get; set; } = new LoginDto();

        public string? ErrorMessage { get; set; }

        public LoginModel(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _userService.LoginAsync(LoginDto.Email, LoginDto.Password, LoginDto.RememberMe);

            if (result.Succeeded)
            {
                var user = await _userService.GetUserByEmailAsync(LoginDto.Email);

                var options = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(30)
                };
                Response.Cookies.Append("userId", user.Id.ToString(), options);

                return Redirect("/");
            }
            else
            {
                ErrorMessage = "El email electrónico o la contraseña no son correctos. Inténtelo of new.";
                return Page();
            }
        }
    }
}