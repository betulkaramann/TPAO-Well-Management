using Microsoft.AspNetCore.Mvc;

namespace TpaoProject1.Controllers
{
    public enum NotificationType
    {
        Success,
        Error,
        Info,
        Warning,
        Question
    }
    public class BaseController : Controller
    {
        public void BasicNotification(string message, NotificationType type, string title="") {
            TempData["notification"] = $"Swal.fire('{title}','{message}', '{type.ToString().ToLower()}')";
        }
        public void DeleteNotification(string message, NotificationType type, string title="", bool showCancelButton=true, string confirmButtonColor= "#3E8914", string confirmButtonText="Evet, sil!", string cancelButtonColor ="#D00000" )
		{
            TempData["DeleteNotification"] = $"Swal.fire('{title}','{message}', '{type.ToString().ToLower()}'";
		}
    }
}
