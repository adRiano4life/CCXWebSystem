using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog;
using WebStudio.Models;
using WebStudio.ViewModels;

namespace WebStudio.Controllers
{
    public class CalculationsController : Controller
    {
        
        private WebStudioContext _db;
        private ILogger<CardsController> _iLogger;
        Logger _nLogger = LogManager.GetCurrentClassLogger();
        

        public CalculationsController(WebStudioContext db, UserManager<User> userManager, IWebHostEnvironment appEnvironment, ILogger<CardsController> iLogger)
        {
            _db = db;
            _iLogger = iLogger;
        }



        [HttpGet]
        public async Task<IActionResult> GetCardAndOffer(string cardNumber, string offerId)
        {
            if (!string.IsNullOrEmpty(cardNumber) && !string.IsNullOrEmpty(offerId))
            {
                Card card = await _db.Cards.FirstOrDefaultAsync(c => c.Number == cardNumber);
                Offer offer = await _db.Offers.FirstOrDefaultAsync(o => o.Id == offerId);
                
                if (card != null && offer != null)
                {
                    CardAndOffersViewModel model = new CardAndOffersViewModel()
                    {
                        Card = card,
                        Offer = offer
                    };
                    
                    return View(model);
                }
            }
            return Content("Такого лота нет в базе.");
        }

        [HttpGet]
        public async Task<IActionResult> GetPositionOffer(string positionId)
        {
            if (!string.IsNullOrEmpty(positionId))
            {
                OfferPosition position = await _db.OfferPositions.FirstOrDefaultAsync(o => o.Id == positionId);
                if (position != null)
                {
                    return PartialView("OfferPositionViewModel", position);
                }
                return Content("В такой КП позий нет.");
            }

            return Content("Такого Id нет.");
        }
        
        [HttpGet]
        public async Task<IActionResult> GetPositionCard(string positionId)
        {
            if (!string.IsNullOrEmpty(positionId))
            {
                CardPosition position = await _db.Positions.FirstOrDefaultAsync(o => o.Id == positionId);
                if (position != null)
                {
                    return PartialView("CardPositionViewModel", position);
                }
                return Content("В этом лоте позиций нет.");
            }

            return Content("Такого Id нет.");
        }
    }
}