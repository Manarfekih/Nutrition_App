using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Recipe_Nutrition_App.Models;

namespace Recipe_Nutrition_App.Pages.Account;

[Authorize]
public class LogoutModel(SignInManager<ApplicationUser> signInManager) : PageModel
{
    
    public async Task<IActionResult> OnGetAsync()
    {
        await signInManager.SignOutAsync();
        return Redirect("/");
    }
}
