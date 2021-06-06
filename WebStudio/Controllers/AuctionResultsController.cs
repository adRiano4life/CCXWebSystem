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
            if (_db.AuctionResults.Count() == 0)
            {
                AuctionResult auctionResult1 = new AuctionResult()
                {
                    Id = "10",
                    Name = "насосы",
                    Number = "T-0090331/1",
                    DateOfAuctionStart = Convert.ToDateTime("14.06.2021 10:00"),
                    DateOfSignContract = Convert.ToDateTime("04.06.2021 16:31"),
                    Winner = "ТОО Гермес Бурса (CGBA)",
                    Sum = 22000
                };
                
                AuctionResult auctionResult2 = new AuctionResult()
                {
                    Id = "11",
                    Name = "Трубы",
                    Number = "T-0090332/1",
                    DateOfAuctionStart = Convert.ToDateTime("14.06.2021 10:00"),
                    DateOfSignContract = null,
                    Winner = "ТОО Гермес Бурса (CGBA)",
                    Sum = null
                };
                
                _db.AuctionResults.Add(auctionResult1);
                _db.AuctionResults.Add(auctionResult2);
                _db.SaveChanges();
            }
            
            
            return View(_db.AuctionResults.ToList());
        }
    }
}