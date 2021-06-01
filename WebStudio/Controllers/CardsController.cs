using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public CardsController(WebStudioContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index(int? page)
        {
            List<Card> cards = _db.Cards.ToList();
            int pageSize = 20;
            int pageNumber = page ?? 1;

            return View(cards.ToPagedList(pageNumber, pageSize));
        }

        /// <summary>
        /// Детальная информация на карточке
        /// </summary>
        /// <param name="cardId">для поиска в базе карт метод принимает её Id</param>
        /// <returns></returns>
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

        /// <summary>
        /// Данный Action помечает карту удалённой
        /// </summary>
        /// <param name="cardId">для поиска в базе карт на удаление карты метод принимает её Id</param>
        /// <returns></returns>
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

        /// <summary>
        /// Отображение карт
        /// </summary>
        /// <param name="page">Номер страницы</param>
        /// <param name="from">С какого числа</param>
        /// <param name="to">По какое число</param>
        /// <param name="filter">Выбор фильтрации</param>
        /// <param name="sort">Сортировка статусов карт</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetCardInfo(int? page, DateTime? from, DateTime? to, string filter, CardState sort)
        {
            List<Card> cards = new List<Card>();
            
            switch (sort)
            {
                case CardState.Новая:
                    cards = _db.Cards.Where(c => c.CardState == CardState.Новая).ToList();
                    ViewBag.sort = CardState.Новая;
                    break;
                
                case CardState.Удалена: 
                    cards = _db.Cards.Where(c => c.CardState == CardState.Удалена).ToList();
                    ViewBag.sort = CardState.Удалена;
                    break;
                
                case CardState.Проработка: 
                    cards = _db.Cards.Where(c => c.CardState == CardState.Проработка).ToList();
                    ViewBag.sort = CardState.Проработка;
                    break;
            }
            
            switch (filter)
            {
                case "DateOfAcceptingEnd": 
                    if (from != null || to != null)
                    {
                        cards = cards.Where(c => c.DateOfAcceptingEnd >= from && c.DateOfAcceptingEnd <= to).ToList();
                    }
                    break;
                
                case "DateOfAuctionStart": 
                    if (from != null || to != null)
                    {
                        cards = cards.Where(c => c.DateOfAuctionStart >= from && c.DateOfAuctionStart <= to).ToList();
                    }
                    break;
                
            }
            
            
            
            int pageSize = 10;
            int pageNumber = page ?? 1;

            return View(cards.ToPagedList(pageNumber, pageSize));


        }
        
        
    }
}