using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PFDETBM.Pages;

// Public landing page served at "/". Signed-in users see an "Open Dashboard"
// call to action instead of the Sign In / Get Started buttons.
public class IndexModel : PageModel
{
    public bool IsSignedIn => User.Identity?.IsAuthenticated == true;

    public void OnGet()
    {
    }
}
