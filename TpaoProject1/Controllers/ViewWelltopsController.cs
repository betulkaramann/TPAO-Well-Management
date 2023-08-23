using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TpaoWebApp.Areas.Identity.Data;
using TpaoWebApp.Data;
using TpaoWebApp.Model;
using TpaoWebApp.Models;
using X.PagedList;

namespace TpaoWebApp.Controllers
{
    public class ViewWelltopsController : BaseController
    {
        private readonly DatabaseContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MapsGeocodingService _geocodingService;

        public ViewWelltopsController(DatabaseContext dbContext, UserManager<ApplicationUser> userManager, MapsGeocodingService geocodingService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _geocodingService = geocodingService;
        }

        [HttpGet]
        public async Task<IActionResult> AddWellTop()
        {
            ViewBag.ActionName = "AddWellTop";
            ViewBag.ButtonText = "Kaydet";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddWellTop(WellTop model)
        {   //Yeni kuyu ekleme işlemleri
            ViewBag.ActionName = "AddWellTop";
            ViewBag.ButtonText = "Kaydet";
            var WellTopList = _dbContext.WellTops.ToList();

            if (ModelState.IsValid)
            {
                int counter = 0;
                string city = null;
                double lati = double.Parse(model.Latitude);
                double longi = double.Parse(model.Longitude);
                string apiKey = "AIzaSyDU_pWP66-BTzvW7AnEcQRSaBPutMzWxU4";

                string geocodingData = await _geocodingService.GetGeocodingData(lati, longi, apiKey);
                try
                {
                    if (!string.IsNullOrEmpty(geocodingData))
                    {   //Koordinatlara göre şehir ataması
                        while (city == null)
                        {
                            JObject jsonObject = JObject.Parse(geocodingData);
                            city = jsonObject["results"][counter]["address_components"]
                                                 .FirstOrDefault(c => c["types"].Any(t => t.ToString() == "locality" || t.ToString() == "administrative_area_level_1"))?["long_name"]?.ToString();
                            counter++;
                        }


                        model.City = city;
                    }
                    var user = await _userManager.GetUserAsync(User);

                    model.UserId = user.Id;
                    var numUserId = model.UserId.ToString();

                    var Name = model.Name;
                    var Latitude = model.Latitude;
                    var Longitude = model.Longitude;
                    var WellTopType = model.WellTopType;
                    var City = model.City;
                    var InsertionDate = DateTime.Now;
                    var UpdateDate = DateTime.Now;

                    WellTop welltop = new WellTop { UserId = numUserId, Name = Name, Latitude = Latitude, Longitude = Longitude, WellTopType = WellTopType, City = City, InsertionDate = InsertionDate, UpdateDate = UpdateDate };
                    var name = welltop.Name;
                    var longitude = welltop.Longitude;
                    var latitude = welltop.Latitude;

                    if (!IsLocationExists(longitude, latitude))
                    {
                        _dbContext.WellTops.Add(welltop);
                    }
                    else 
                    {
                        BasicNotification("Koordinatlarınızı tekrar giriniz...", NotificationType.Error, "Seçtiğiniz koordinatlarda kuyu bulunmaktadır");
                        return View();
                    }
                    
                    BasicNotification("Anasayfaya yönlendiriliyorsunuz...", NotificationType.Success, "Kuyu Başarıyla Eklendi!");
                    await _dbContext.SaveChangesAsync();

                }
                catch (Exception ex) {

                    BasicNotification("Koordinatlarınızı kontrol ediniz..", NotificationType.Question, "Türk deniz sahasını aştınız!");
                }

                return RedirectToAction("AddedWelltops", "Home");
            }
            return View();
        }
        
        public bool IsLocationExists(string longitude, string latitude)
        {   //Verilen koordinatlarla eşleşen kuyu varlığını kontrol etme
            return _dbContext.WellTops.Any(u => u.Latitude == latitude) && _dbContext.WellTops.Any(u => u.Longitude == longitude);
        }

        public async Task<IActionResult> MainPage(int page = 1, int pageSize = 50)
        {
            IPagedList<WellTop> data = null;
            
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
            
            List<WellTop> welltop = new();
            //Proje klasöründe var olan çoklu random kuyuları haritada optimize şekilde gösterme 
            using (var reader = new StreamReader(@"randomKuyuVerisi.csv"))
            {
                bool flag = false;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    if (flag)
                    {
                        var name = values[0];
                        var lngi = values[1];
                        var lati = values[2];
                        string type = "";
                        var typeNumber = Int32.Parse(values[3]);
                        if (typeNumber == 0)
                            type = "arama";
                        else if (typeNumber == 1)
                            type = "tespit";
                        else if (typeNumber == 2)
                            type = "üretim";
                        

                        WellTop new_well = new()
                        {
                            UserId = user.Id,
                            Latitude = lati,
                            Longitude = lngi,
                            Name = name,
                            WellTopType = type
                        };

                        welltop.Add(new_well);
                        data = welltop.ToList().ToPagedList(page, pageSize);
                    }
                    flag = true;
                }
            }

            var viewModel = new PageUserModel()
            {
                Kullanicilar = users,
                Kuyular = data,
                MapKuyular = welltop,
                PageSize = pageSize
            };
            return View(viewModel);
        }
       
        public async Task<IActionResult> Delete(int id)
        {
            DeleteNotification("Silinen Kuyu Geri Getirilmez!", NotificationType.Warning, "Emin Misiniz?");

            ViewBag.ActionName = "Delete";
            ViewBag.ButtonText = "Sil";

            var kuyu = _dbContext.WellTops.Find(id);
            _dbContext.Remove(kuyu);
            _dbContext.SaveChanges();
            return RedirectToAction("AddedWelltops", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            ViewBag.ActionName = "Update";
            ViewBag.ButtonText = "Güncelle";
            var kuyu = _dbContext.WellTops.Find(id);
            var updateUserId = kuyu.UserId;
            TempData["userid"] = updateUserId;
            TempData["type"] = kuyu.WellTopType;
            return View(kuyu);
        }
        [HttpPost]
        public async Task<IActionResult> Update(WellTop welltop)
        {
            int counter = 0;
            string city = null;

            double lati = double.Parse(welltop.Latitude);
            double longi = double.Parse(welltop.Longitude);
            string welltopType = welltop.WellTopType;
            if(welltopType == null)
            {
                welltop.WellTopType = TempData["type"].ToString();
            }
            string apiKey = "AIzaSyDU_pWP66-BTzvW7AnEcQRSaBPutMzWxU4";

            string geocodingData = await _geocodingService.GetGeocodingData(lati, longi, apiKey);
            try
            {
				if (!string.IsNullOrEmpty(geocodingData))
				{
					while (city == null)
					{   //Koordinatlara göre şehir ataması
						JObject jsonObject = JObject.Parse(geocodingData);
						city = jsonObject["results"][counter]["address_components"]
															 .FirstOrDefault(c => c["types"].Any(t => t.ToString() == "locality" || t.ToString() == "administrative_area_level_1"))?["long_name"]?.ToString();
						counter++;
					}
					welltop.City = city;
				}
                var user = await _userManager.GetUserAsync(User);
                welltop.UserId = user.Id.ToString();

                _dbContext.WellTops.Update(welltop);
				_dbContext.SaveChanges();
			}
			catch (Exception ex) 
            {
				BasicNotification("Koordinatlarınızı kontrol ediniz..", NotificationType.Question, "Türk deniz sahasını aştınız!");
			}
            return RedirectToAction("AddedWelltops", "Home");
        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult HasWellName(string name)
        {   //Dinamik isim kontrolü
            var anyName = _dbContext.WellTops.Any(x => x.Name == name);
            if (anyName)
            {
                return Json("Bu isimde bir kuyu zaten mevcut");
            }
            else
            {
                return Json(true);
            }
        }
    }
}
