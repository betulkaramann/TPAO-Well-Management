﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using TpaoProject1.Areas.Identity.Data;
using TpaoProject1.Data;
using TpaoProject1.Model;
using TpaoProject1.Models;

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


            // WellTop well = new WellTop();
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
                    //_dbContext.SaveChanges();
                    BasicNotification("Anasayfaya yönlendiriliyorsunuz...", NotificationType.Success, "Kuyu Başarıyla Eklendi!");
                    await _dbContext.SaveChangesAsync();

                }
                catch (Exception ex) {

                    BasicNotification("Koordinatlarınızı kontrol ediniz..", NotificationType.Question, "Türk deniz sahasını aştınız!");
                }

                return RedirectToAction("MainPage", "ViewWelltops");
            }


            return View();
        }
        public bool IsNameExists(string name)
        {
            return _dbContext.WellTops.Any(u => u.Name == name);
        }

        public bool IsLocationExists(string longitude, string latitude)
        {
            return _dbContext.WellTops.Any(u => u.Latitude == latitude) && _dbContext.WellTops.Any(u => u.Longitude == longitude);
        }

        public async Task<IActionResult> MainPage()
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

       
        public async Task<IActionResult> Delete(int id)
        {
            DeleteNotification("Silinen Kuyu Geri Getirilmez!", NotificationType.Warning, "Emin Misiniz?");

            ViewBag.ActionName = "Delete";
            ViewBag.ButtonText = "Sil";

            var kuyu = _dbContext.WellTops.Find(id);
            _dbContext.Remove(kuyu);
            _dbContext.SaveChanges();
            return RedirectToAction("MainPage", "ViewWelltops");
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
			catch (Exception ex) {
				BasicNotification("Koordinatlarınızı kontrol ediniz..", NotificationType.Question, "Türk deniz sahasını aştınız!");

			}

            return RedirectToAction("MainPage", "ViewWelltops");

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
