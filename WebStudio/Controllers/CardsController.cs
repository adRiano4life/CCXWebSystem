using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Детальная информация на карточке
        /// </summary>
        /// <param name="cardId">для поиска в базе карт метод принимает её Id</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
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
        [HttpPost]
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
                    return RedirectToAction("GetCardInfo", "Cards", new {sort = CardState.Новая});
                }
                return NotFound();
            }

            return NotFound();
        }

        /// <summary>
        /// Данный Action позволяет восстановить карту в новое состояние.
        /// </summary>
        /// <param name="cardId">Id карты по которому ищется карта БД.</param>
        /// <returns></returns>
        [HttpPost]
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

            return RedirectToAction("GetCardInfo", "Cards", new {sort = CardState.Новая});
        }
        
        /// <summary>
        /// Данный Action передает карту ответственному исполнителю и меняет её статус.
        /// </summary>
        /// <param name="model">Данный параметр является передаваемым контейнером для сущности Card.</param>
        /// <returns></returns>
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
                    card.DateOfProcessingEnd = model.Card.DateOfProcessingEnd;
                    card.DateOfAuctionStartUpdated = model.Card.DateOfAuctionStartUpdated;

                    _db.Cards.Update(card);
                    await _db.SaveChangesAsync();
                }
            }
            return RedirectToAction("GetCardInfo", "Cards", new {sort = CardState.Проработка});
        }

        
        // [HttpPost]
        // [Authorize(Roles = "admin")]
        // public async Task<IActionResult> AuctionCard(string cardId)
        // {
        //     if (cardId != null)
        //     {
        //         Card card = _db.Cards.FirstOrDefault(c => c.Id == cardId);
        //         if (card != null)
        //         {
        //             card.CardState = CardState.Торги;
        //             _db.Cards.Update(card);
        //             await _db.SaveChangesAsync();
        //         }
        //     }
        //     return RedirectToAction("Index", "Cards");
        // }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangeCardStatus(string cardId, string cardState)
        {
            if (cardId != null)
            {
                Card card = _db.Cards.FirstOrDefault(c => c.Id == cardId);
                if (card != null)
                {
                    switch (cardState)
                    {
                        case "ПКО":
                            card.CardState = CardState.ПКО;
                            break;
                        case "Торги":
                            card.CardState = CardState.Торги;
                            break;
                        case "Удалена":
                            card.CardState = CardState.Удалена;
                            break;
                    }

                    _db.Cards.Update(card);
                    await _db.SaveChangesAsync();
                }
            }

            return RedirectToAction("DetailCard", "Cards", new {cardId = cardId});
        }
        
       /// <summary>
        /// Данный Action позволяет делать отображение, фильтрацию карт и пагинацию страниц по статусам, датам и исполнителям.
        /// </summary>
        /// <param name="page">Номер страницы</param>
        /// <param name="from">С какого числа</param>
        /// <param name="to">По какое число</param>
        /// <param name="filter">Выбор фильтрации</param>
        /// <param name="sort">Сортировка статусов карт</param>
        /// <returns></returns>
       [HttpGet]
       [Authorize]
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
                
                case CardState.Торги: 
                    cards = _db.Cards.Where(c => c.CardState == CardState.Торги).ToList();
                    ViewBag.sort = CardState.Торги;
                    break;
                
                case CardState.Выигранная: 
                    cards = _db.Cards.Where(c => c.CardState == CardState.Выигранная).ToList();
                    ViewBag.sort = CardState.Выигранная;
                    break;
                
                case CardState.ПКО: 
                    cards = _db.Cards.Where(c => c.CardState == CardState.ПКО).ToList();
                    ViewBag.sort = CardState.ПКО;
                    break;
            }

            if (filter != null)
            {
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
                    
                    default:
                        cards = _db.Cards.Where(c => c.ExecutorId == filter).ToList();
                        ViewBag.filter = filter;
                        break;
                }
            }

            int pageSize = 20;
            int pageNumber = page ?? 1;

            return View(cards.ToPagedList(pageNumber, pageSize));
        }

       [HttpPost]
       [Authorize]
       public async Task<IActionResult> Comment(DetailCardViewModel model)
       {
           Comment comment = new Comment
           {
               Message = model.Comment.Message,
               DateOfSend = DateTime.Now,
               CardId = model.Comment.CardId,
               Card = _db.Cards.FirstOrDefault(c => c.Id == model.Comment.CardId),
               UserId = model.Comment.UserId,
               User = await _userManager.FindByIdAsync(model.Comment.UserId)
           };

           await _db.Comments.AddAsync(comment);
           await _db.SaveChangesAsync();
           return RedirectToAction("DetailCard", "Cards", new {cardId = model.Comment.CardId});
       }

       [HttpGet]
       [Authorize(Roles = "admin")]
       public async Task<IActionResult> ChangeComment(string commentId)
       {
           if (commentId != null)
           {
               Comment comment = _db.Comments.FirstOrDefault(c => c.Id == commentId);
               if (comment != null)
               {
                   ChangeCommentViewModel model = new ChangeCommentViewModel
                   {
                       Id = comment.Id,
                       Message = comment.Message,
                   };
                   return View(model);
               }

               return NotFound();
           }

           return NotFound();
       }

       [HttpPost]
       public async Task<IActionResult> ChangeComment(ChangeCommentViewModel model)
       {
           if (ModelState.IsValid)
           {
               Comment comment = _db.Comments.FirstOrDefault(c => c.Id == model.Id);
               if (comment != null)
               {
                   comment.Message = model.Message;
                   comment.DateOfChange = DateTime.Now;
                   
                   _db.Comments.Update(comment);
                   await _db.SaveChangesAsync();
                   return RedirectToAction("DetailCard", "Cards", new {cardId = comment.Card.Id});
               }

               return NotFound();
           }

           return View(model);
       }
    }
}