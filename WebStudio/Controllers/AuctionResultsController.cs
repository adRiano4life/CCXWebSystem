using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebStudio.Models;

namespace WebStudio.Controllers
{
    public class AuctionResultsController : Controller
    {
        private WebStudioContext _db;

        public AuctionResultsController(WebStudioContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View(_db.AuctionResults.ToList());
        }
    }
}