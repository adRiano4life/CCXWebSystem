using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebStudio.Enums;
using WebStudio.Models;
using WebStudio.ViewModels;
using X.PagedList;

namespace WebStudio.Controllers
{
    public class CardsController : Controller
    {
        private WebStudioContext _db;
        private UserManager<User> _userManager;

        public CardsController(WebStudioContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index(int? page)
        {
            List<Card> cards = _db.Cards.ToList();
            int pageSize = 20;
            int pageNumber = page ?? 1;

            return View(cards.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        public IActionResult DetailCard(string cardId)
        {
            if (cardId != null)
            {
                DetailCardViewModel model = new DetailCardViewModel
                {
                    Card = _db.Cards.FirstOrDefault(c => c.Id == cardId),
                    Users = _db.Users.ToList()
                };

                return View(model);
            }

            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCard(string cardId)
        {
            if (cardId != null)
            {
                Card card = _db.Cards.FirstOrDefault(c => c.Id == cardId);
                if (card != null)
                {
                    card.CardState = CardState.Удалена;
                    _db.Cards.Update(card);
                    await _db.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index", "Cards");
        }

        [HttpGet]
        public async Task<IActionResult> RestoreCard(string cardId)
        {
            if (cardId != null)
            {
                Card card = _db.Cards.FirstOrDefault(c => c.Id == cardId);
                if (card != null)
                {
                    card.CardState = CardState.Новая;
                    _db.Cards.Update(card);
                    await _db.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index", "Cards");
        }

        [HttpPost]
        public async Task<IActionResult> TakeCard(DetailCardViewModel model)
        {
            if (model.CardId != null)
            {
                Card card = _db.Cards.FirstOrDefault(c => c.Id == model.CardId);
                if (card != null)
                {
                    card.CardState = CardState.Проработка;
                    card.ExecutorId = model.UserId;
                    card.Executor = await _userManager.FindByIdAsync(model.UserId);

                    _db.Cards.Update(card);
                    await _db.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index", "Cards");
        }
        
        [HttpGet]
        public IActionResult CardInWork(int? page)
        {
            List<Card> cards = _db.Cards.Where(c => c.CardState == CardState.Проработка).ToList();
            int pageSize = 20;
            int pageNumber = page ?? 1;

            return View(cards.ToPagedList(pageNumber, pageSize));
        }
        
        [HttpGet]
        public IActionResult CardDeleted(int? page)
        {
            List<Card> cards = _db.Cards.Where(c => c.CardState == CardState.Удалена).ToList();
            int pageSize = 20;
            int pageNumber = page ?? 1;

            return View(cards.ToPagedList(pageNumber, pageSize));
        }
        
    }
}