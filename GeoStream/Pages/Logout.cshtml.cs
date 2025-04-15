using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GeoStream.Services;

namespace GeoStream.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly IUserService _userService;

        public LogoutModel(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
        {
            await _userService.Logout();

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage("/login");
            }
        }
    }
}