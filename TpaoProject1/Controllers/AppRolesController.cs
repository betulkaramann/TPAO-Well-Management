using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using TpaoWebApp.Data;
using Microsoft.EntityFrameworkCore;
using TpaoWebApp.Models;
using TpaoWebApp.Areas.Identity.Data;

namespace TpaoWebApp.Controllers
{
	public class AppRolesController : Controller
	{
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DatabaseContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AppRolesController(UserManager<ApplicationUser> userManager, DatabaseContext dbContext, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _roleManager = roleManager;
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> Create(IdentityRole model)
		{	//Role göre Index ekranına yönlendirme
			if(!_roleManager.RoleExistsAsync(model.Name).GetAwaiter().GetResult())
			{
				_roleManager.CreateAsync(new IdentityRole(model.Name)).GetAwaiter().GetResult();
			}
			return RedirectToAction("Index");
		}
		
		
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(string role,string userid)
        {	//Rol atama işlemleri
			if( userid != null) {

				var currentUser = await _userManager.GetUserAsync(HttpContext.User);
				var changeUser = _userManager.Users.First(x => x.Id == userid);

					var currentRole = await _userManager.GetRolesAsync(changeUser);

					if (!currentRole.Equals(role))
					{
						await _userManager.RemoveFromRolesAsync(changeUser, currentRole);
						await _userManager.AddToRoleAsync(changeUser, role);
					}
				return RedirectToAction("AssignRole");
			}
				var users = _dbContext.Users.ToList();
				var roles = _roleManager.Roles.ToList();

				var viewModel = new UserRolesViewModel
				{
					Kullanicilar = users,
					Roller = roles
				};

				return View(viewModel);
			
        }
    }
}
