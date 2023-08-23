using Microsoft.AspNetCore.Mvc;

namespace TpaoProject1.Controllers
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
