using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStudio.Models;

namespace WebStudio.Controllers
{
    [Authorize]
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

        [HttpGet]
        public IActionResult GetResult(string finde)
        {
            if (!string.IsNullOrEmpty(finde))
            {
                var result = _db.AuctionResults.Where(a => a.Number.ToLower().Contains(finde.ToLower())).ToList();
                if (result.Count != 0)
                {
                    return PartialView("GetResultsPartialView", result);
                }
                else
                {
                    result = _db.AuctionResults.Where(a => a.Name.ToLower().Contains(finde.ToLower())).ToList();
                    var cards = _db.Cards.Where(c => c.Positions.Any(n => n.Name.ToLower().Contains(finde.ToLower()))).ToList();
                    
                    foreach (var card in cards)
                    {
                       var auctionResult = _db.AuctionResults.FirstOrDefault(a => a.Number == card.Number);
                       if (auctionResult != null)
                       {
                           if (result.All(r => r.Number != auctionResult.Number))
                           {
                              result.Add(auctionResult); 
                           }
                       }
                           
                    }

                    if (result.Count != 0)
                    {
                        return PartialView("GetResultsPartialView", result);  
                    }
                    

                    return PartialView("ErrorFindePartialView");
                }
                
            }
            return PartialView("GetAllResultsPartialView", _db.AuctionResults.ToList());
        }
    }
}