using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebStudio.Models;
using NLog;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace WebStudio.Controllers
{
    [Authorize]
    public class AuctionResultsController : Controller
    {
        private WebStudioContext _db;
        private UserManager<User> _userManager;
        private ILogger<CardsController> _iLogger;
        Logger _nLogger = LogManager.GetCurrentClassLogger();
        

        public AuctionResultsController(WebStudioContext db, UserManager<User> userManager,ILogger<CardsController> iLogger)
        {
            _db = db;
            _userManager = userManager;
            _iLogger = iLogger;
        }

        public IActionResult Index()
        {
            
            return View(_db.AuctionResults.ToList());
        }

        [HttpGet]
        public IActionResult GetResult(string finde)
        {
            try
            {
                _nLogger.Info($"Пользователь {_userManager.GetUserAsync(User).Result.Name} {_userManager.GetUserAsync(User).Result.Surname} сделал произвел поиск по: {finde}");

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
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка в: {e.TargetSite}: {e.Message} | {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание ошибка в: {e.TargetSite}: {e.Message} | {e.StackTrace}");
                throw;
            }
            
        }
    }
}