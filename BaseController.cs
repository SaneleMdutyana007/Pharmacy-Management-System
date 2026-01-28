using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PharmacyManager.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            bool isAjax = HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (!isAjax)
            {
                if (HttpContext.Session.GetString("UserSession") == null && User.Identity != null && User.Identity.IsAuthenticated)
                {
                    HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme).Wait();

                    context.Result = new RedirectToActionResult("Login", "Account", null);
                }
            }
            base.OnActionExecuted(context);
        }

    }
}
