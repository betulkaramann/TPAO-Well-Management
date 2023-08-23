using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using TpaoWebApp.Areas.Identity.Data;
using TpaoWebApp.Data;
using TpaoWebApp.Model;
using TpaoWebApp.Models;

namespace TpaoWebApp.Controllers
{
    public class HomeController : BaseController
    {
        private readonly DatabaseContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MapsGeocodingService _geocodingService;


        public HomeController(DatabaseContext dbContext, UserManager<ApplicationUser> userManager, MapsGeocodingService geocodingService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _geocodingService = geocodingService;

        }
        
        [HttpGet]
		public async Task<IActionResult> Index()
		{   //Kuyu sayısının tiplere göre dağılımı
			var tespit = _dbContext.WellTops.Where(p => p.WellTopType == "tespit").Count();
			var uretim = _dbContext.WellTops.Where(p => p.WellTopType == "üretim").Count();
			var arama = _dbContext.WellTops.Where(p => p.WellTopType == "arama").Count();
			var toplam = tespit + uretim + arama;
			ViewBag.uretim = uretim;
			ViewBag.arama = arama;
			ViewBag.tespit = tespit;
			ViewBag.toplam = toplam;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Index(WellTop model)
		{
            return View();
		}
      
        public async Task<IActionResult> LineSimplification()
        {
			return View();
        }
                
        public IActionResult History() 
        {
            return View();
        }

        public async Task<IActionResult> AddedWelltops()
        {       //Role göre kuyu gösterme işlemleri
                var user = await _userManager.GetUserAsync(User);
                var WellTopList = _dbContext.WellTops.ToList();


                if (User.IsInRole("Admin"))
                {
                    WellTopList = _dbContext.WellTops.ToList();
                }
                else
                {
                    WellTopList = _dbContext.WellTops.Where(w => w.UserId == user.Id).ToList();
                }

                var users = _dbContext.Users.ToList();

                var viewModel = new UserRolesViewModel
                {
                    Kullanicilar = users,
                    Kuyular = WellTopList
                };


                return View(viewModel);            
        }

		[HttpPost]
        public ActionResult UploadJson(IFormFile jsonFile)
        {   //Proje klasöründeki json dosyasını okuma işlemi
            if (jsonFile != null && jsonFile.Length > 0)
            {
                using (var reader = new StreamReader(jsonFile.OpenReadStream()))
                {
                    string jsonContent = reader.ReadToEnd();

                    var jsonData = JsonConvert.DeserializeObject<JsonDataModel>(jsonContent);

                    List<HatKoordinat> coordinates = jsonData.HatKoordinatlari;
                    List<SadelestirilmisHatKoordinat> simplifiedCoordinates = jsonData.SadelestirilmisHat;

                    List<(double x, double y)> jsonList = new();
                    List<(double x, double y)> jsonPureList = new();

                    foreach (var coordinate in coordinates)
                    {
                        jsonList.Add((coordinate.X, coordinate.Y));
                    }
                    foreach (var simplifiedCoordinate in simplifiedCoordinates)
                    {
                        jsonPureList.Add((simplifiedCoordinate.X, simplifiedCoordinate.Y));
                    }
                    List<(double x, double y)> pureList = GeoSimplifier.LineSimplificator.SimplifyPoints(jsonList, 0, jsonList.Count - 1, 100);
                    pureList.Add((jsonList[jsonList.Count() - 1].x, jsonList[jsonList.Count() - 1].y));
                    ViewBag.listCount = pureList.Count;

                    ViewBag.pureListJson = JsonConvert.SerializeObject(pureList);
                    
                    ViewBag.pureList = JsonConvert.SerializeObject(pureList);

                    TempData["pureList"] = JsonConvert.SerializeObject(pureList);

                    return RedirectToAction("LineSimplification");

                }
            }         
            return View();
        }
        public IActionResult Line3D()
        {
            return View();        
        }

        public ActionResult ViewMap()
        {
            var pureListData = TempData["pureList"] as string;
            ViewBag.PureListJson = pureListData;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
