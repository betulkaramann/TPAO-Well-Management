using Microsoft.AspNetCore.Mvc;

namespace TpaoWebApp.Controllers
{
    public class UserController : Controller
    {
        [Route("ResetPassword")]
        public IActionResult ResetPassword()
        {
            return View();
        }   
    }
}
