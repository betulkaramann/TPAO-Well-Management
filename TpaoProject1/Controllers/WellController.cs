using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TpaoWebApp.Data;
using TpaoWebApp.Model;
using TpaoWebApp.Models;
using Newtonsoft.Json.Linq;


namespace TpaoWebApp.Controllers
{
    public class WellController : Controller
    {
        public Dictionary<string, string> Color = new Dictionary<string, string>()
        {   //Formasyon renkleri
            {"A","#DEAB90" },{"B","#04471C" },{"C","#8A5A44" },{"D","#31572C" },{"E","#9C6644" },{"F","#99582A" },
            {"G","#5E503F" },{"H","#936639" },{"I","#7F4F24" },{"J","#582F0E" },{"K","#A39171" },{"L","#C38E70" },
            {"M","#C18C5D" },{"N","#6D4C3D" },{"O","#2C514C" },{"P","#8CB369" },{"Q","#386641" },{"R","#333D29" },
            {"S","#2D6A4F" },{"T","#004B23" },{"U","#BAA587" },{"V","#593D3B" },{"W","#49A078" },{"X","#90BE6D" },
            {"Y","#DDB892" },{"Z","#588157" }
        };

        private readonly DatabaseContext _context;
        public WellController(DatabaseContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var well = _context.WellTops.ToList();
            return View(well);
        }
        public IActionResult ViewWell(int id)
        {
            var well = _context.WellTops.Find(id);
            var formation = _context.Formation.Where(f => f.WellId == id).ToList();

            var wellInfos = new WellAndFormation()
            {
                Formation = formation,
                Well = well,
                Color = Color
            };
            return View(wellInfos);
        }
        [HttpGet]
        public IActionResult AddFormation(int id)
        {
            var well = _context.WellTops.Find(id);

            return View(well);
        }

        [HttpPost]
        [ActionName("AddFormation")]
        public async Task<IActionResult> AddFormation(WellTop well, string formationType, int formationMeter)
        {
            var id = well.Id;

            Formation formation = new(id)
            {
                FormationMeter = formationMeter,
                FormationType = formationType
            };

            List<Formation> formationList;
            int? biggestFormationMeter = -1;
            formationList = _context.Formation.Where(x => x.WellId == id).OrderByDescending(x => x.FormationMeter).ToList();

            if (formationList.Count != 0) biggestFormationMeter = formationList.First().FormationMeter;

            var Well = _context.WellTops.Find(id);
            var isExist = _context.Formation.Where(x => x.WellId == id).Where(x => x.FormationType == formationType).Count();

            if (formationMeter < 0 || formationMeter > 10000)
            {
                TempData["status"] = "OutofOrder";
                return View(Well);
            }
            else if (isExist == 0 && formationMeter <= biggestFormationMeter)
            {
                TempData["status"] = "LowerFormation";
                return View(Well);
            }
            else if (isExist == 0)
            {
                _context.Formation.Add(formation);
                _context.SaveChanges();
                var Formation = _context.Formation.Where(f => f.WellId == id).ToList();
                var all = new WellAndFormation()
                {
                    Formation = Formation,
                    Well = Well,
                    Color = Color
                };
                TempData["status"] = "true";
                return View("ViewWell", all);
            }
            else
            {
                TempData["status"] = "SameFormation";
                return View(Well);
            }
        }
        public IActionResult UpdateFormation(int id)
        {
            var formation = _context.Formation.Find(id);
            var well = _context.WellTops.Find(_context.Formation.Find(id).WellId);

            ViewData["name"] = well.Name;
            ViewData["latitude"] = well.Latitude;
            ViewData["longitude"] = well.Longitude;
            TempData["formation_well_id"] = well.Id;
            ViewData["well_type"] = well.WellTopType;

            return View(formation);
        }
        [HttpPost]
        public IActionResult UpdateFormation(Formation formation)
        {
            formation.WellId = Int32.Parse(TempData.Peek("formation_well_id").ToString());
            var well = _context.WellTops.Find(formation.WellId);
            ViewData["name"] = well.Name;
            ViewData["latitude"] = well.Latitude;
            ViewData["longitude"] = well.Longitude;
            ViewData["formation_well_id"] = well.Id;
            ViewData["well_type"] = well.WellTopType;

            var formationList = _context.Formation.Where(x => x.WellId == formation.WellId).ToList();
            var index = formationList.FindIndex(x => x.Id == formation.Id);
            var previousFormation = _context.Formation.Find(formation.Id);

            if (formation.FormationMeter < 0 || formation.FormationMeter > 10000)
            {
                TempData["Error"] = "OutofOrder";
                return View(previousFormation);
            }
            if ((formationList.Count() - 1 > index) && formation.FormationMeter > formationList[index + 1].FormationMeter)
            {
                TempData["Error"] = "Bigger";
                return View(previousFormation);
            }
            else if (index > 0 && formation.FormationMeter < formationList[index - 1].FormationMeter)
            {
                TempData["Error"] = "Smaller";
                return View(previousFormation);
            }
            else if ((index == 0 && formationList.Count() > 1 && formation.FormationMeter < formationList[index + 1].FormationMeter) || index == 0 && formationList.Count() == 1 || (index == formationList.Count() - 1 && formation.FormationMeter > formationList[index - 1].FormationMeter) || (formation.FormationMeter > formationList[index - 1].FormationMeter && formation.FormationMeter < formationList[index + 1].FormationMeter))
            {
                TempData["Error"] = "successful";
                _context.Formation.Update(formation);
                _context.SaveChanges();

                var wellInfos = new WellAndFormation()
                {
                    Formation = _context.Formation.Where(x => x.WellId == formation.WellId).ToList(),
                    Well = _context.WellTops.Find(formation.WellId),
                    Color = Color

                };
                return View("ViewWell", wellInfos);
            }
            else
            {
                return View(previousFormation);
            }
        }
        public IActionResult RemoveFormation(int id)
        {
            var formation = _context.Formation.Find(id);
            _context.Remove(formation);
            _context.SaveChanges();
            var wellInfos = new WellAndFormation()
            {
                Formation = _context.Formation.Where(x => x.WellId == formation.WellId).ToList(),
                Well = _context.WellTops.Find(formation.WellId),
                Color = Color
            };
            return View("ViewWell", wellInfos);
        }
    }
}
