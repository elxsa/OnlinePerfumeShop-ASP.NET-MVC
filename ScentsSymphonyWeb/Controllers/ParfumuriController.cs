using Microsoft.AspNetCore.Mvc;
using ScentsSymphonyWeb.Data;
using ScentsSymphonyWeb.Models;

namespace ScentsSymphonyWeb.Controllers
{
    public class ParfumuriController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ParfumuriController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Parfumuri> objParfumuriList = _db.Perfume.ToList();
            return View(objParfumuriList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Parfumuri obj)
        {
            
            if (ModelState.IsValid) 
            { 
            _db.Perfume.Add(obj);
            _db.SaveChanges();
            return RedirectToAction("Index");
            }
            return View(obj);
        }
        public IActionResult Edit(int? ProductID)
        {
            if( ProductID == null || ProductID == 0)
            {
                return NotFound();
            }
            var parfumuriFromDb = _db.Perfume.Find(ProductID);
            if(parfumuriFromDb == null)
            {
                return NotFound();
            }
            return View(parfumuriFromDb);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Parfumuri obj)
        {
            if (ModelState.IsValid)
            {
                
                var existingEntity = _db.Perfume.FirstOrDefault(p => p.ProductID == obj.ProductID);
                if (existingEntity == null)
                {
                    return NotFound();
                }

                _db.Entry(existingEntity).CurrentValues.SetValues(obj);
                _db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        public IActionResult Delete(int? ProductID)
        {
            if (ProductID == null || ProductID == 0)
            {
                return NotFound();
            }

            var parfumuriFromDb = _db.Perfume.Find(ProductID);
            if (parfumuriFromDb == null)
            {
                return NotFound();
            }

            return View(parfumuriFromDb);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int ProductID)
        {
            var parfumuriToDelete = _db.Perfume.FirstOrDefault(p => p.ProductID == ProductID);
            if (parfumuriToDelete == null)
            {
                return NotFound();
            }

            _db.Perfume.Remove(parfumuriToDelete);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Search(string searchTerm)
        {
            var results = _db.Perfume.Where(p => p.Name.Contains(searchTerm)).ToList();
            return View("SearchResults", results); // Redirecționează la view-ul SearchResults
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
