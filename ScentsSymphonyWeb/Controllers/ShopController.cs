using Microsoft.AspNetCore.Mvc;
using ScentsSymphonyWeb.Data;
using ScentsSymphonyWeb.Models;
using System.Collections.Generic;
using System.Linq;

namespace ScentsSymphonyWeb.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ShopController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Parfumuri> objParfumuriList = _db.Perfume.ToList();
            return View(objParfumuriList);
        }

        public IActionResult Details(int id)
        {
            var parfum = _db.Perfume.FirstOrDefault(p => p.ProductID == id);
            if (parfum == null)
            {
                return NotFound();
            }

            return View(parfum);
        }
    }
}
