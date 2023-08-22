using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TpaoProject1.Areas.Identity.Data;
using TpaoProject1.Data;
using TpaoProject1.Model;
using TpaoProject1.Models;
using X.PagedList;

namespace TpaoProject1.Controllers
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
        {
            ViewBag.ActionName = "AddWellTop";
            ViewBag.ButtonText = "Kaydet";
            var WellTopList = _dbContext.WellTops.ToList();
            if (ModelState.IsValid)
            {
                int counter = 0;
                string city = null;
                double lati = double.Parse(model.Latitude);//enlem- paralel 36-42
                double longi = double.Parse(model.Longitude);//boylam - meridyen 26-45
                string apiKey = "AIzaSyDU_pWP66-BTzvW7AnEcQRSaBPutMzWxU4";
                string geocodingData = await _geocodingService.GetGeocodingData(lati, longi, apiKey);
                try
                {
                    if (!string.IsNullOrEmpty(geocodingData))
                    {
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
                catch (Exception ex)
                {
                    BasicNotification("Koordinatlarınızı kontrol ediniz..", NotificationType.Question, "Türk deniz sahasını aştınız!");
                }
                return RedirectToAction("AddedWell", "ViewWelltops");
            }


            return View();
        }
        public async Task<IActionResult> AddedWell()
        {
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
        public bool IsLocationExists(string longitude, string latitude)
        {
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
                        string real_type = "";
                        var type_number = Int32.Parse(values[3]);
                        if (type_number == 0)
                            real_type = "arama";
                        else if (type_number == 1)
                            real_type = "tespit";
                        else if (type_number == 2)
                            real_type = "üretim";

                        WellTop new_well = new()
                        {
                            UserId = user.Id,
                            Latitude = lati,
                            Longitude = lngi,
                            Name = name,
                            WellTopType = real_type
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
                PageSize = pageSize,
            };
            return View(viewModel);
        }
        public async Task<IActionResult> Delete(int id)
        {
            ViewBag.ActionName = "Delete";
            ViewBag.ButtonText = "Sil";
            var kuyu = _dbContext.WellTops.Find(id);
            _dbContext.Remove(kuyu);
            _dbContext.SaveChanges();
            return RedirectToAction("AddedWell", "ViewWelltops");
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            ViewBag.ActionName = "Update";
            ViewBag.ButtonText = "Güncelle";
            var kuyu = _dbContext.WellTops.Find(id);
            var updateUserId = kuyu.UserId;
            TempData["userid"] = updateUserId;
            return View(kuyu);
        }
        [HttpPost]
        public async Task<IActionResult> Update(WellTop welltop)
        {
            int counter = 0;
            string city = null;

            double lati = double.Parse(welltop.Latitude);
            double longi = double.Parse(welltop.Longitude);
            string apiKey = "AIzaSyDU_pWP66-BTzvW7AnEcQRSaBPutMzWxU4";

            string geocodingData = await _geocodingService.GetGeocodingData(lati, longi, apiKey);
            try
            {
                if (!string.IsNullOrEmpty(geocodingData))
                {

                    while (city == null)
                    {
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
            return RedirectToAction("AddedWell", "ViewWelltops");

        }
        [AcceptVerbs("GET", "POST")]
        public IActionResult HasWellName(string name)
        {
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
