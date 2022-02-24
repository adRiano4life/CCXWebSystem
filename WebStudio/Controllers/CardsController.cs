using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebStudio.Enums;
using WebStudio.Models;
using WebStudio.ViewModels;
using X.PagedList;
using NLog;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace WebStudio.Controllers
{
    [Authorize]
    public class CardsController : Controller
    {
        private WebStudioContext _db;
        private UserManager<User> _userManager;
        IWebHostEnvironment _appEnvironment;
        private ILogger<CardsController> _iLogger;
        Logger _nLogger = LogManager.GetCurrentClassLogger();
        

        public CardsController(WebStudioContext db, UserManager<User> userManager, IWebHostEnvironment appEnvironment, ILogger<CardsController> iLogger)
        {
            _db = db;
            _userManager = userManager;
            _appEnvironment = appEnvironment;
            _iLogger = iLogger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                return View(_db.Cards.ToList());
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
            
        }

        /// <summary>
        /// Детальная информация на карточке
        /// </summary>
        /// <param name="cardId">для поиска в базе карт метод принимает её Id</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> DetailCard2(string cardId)
        {
            try
            {
                if (cardId != null)
                {
                    DetailCardViewModel model = new DetailCardViewModel
                    {
                        Card = await _db.Cards.FirstOrDefaultAsync(c => c.Id == cardId),
                        CardId = cardId,
                        Users =  _db.Users.ToList(),
                        FileModels = _db.Files.Where(f => f.CardId == cardId).ToList()
                    };

                    return View("DetailCard2", model);
                }

                return NotFound();
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
            
        }

        
        
        /// <summary>
        /// Универсальный экшн по изменению сатусов у карточек
        /// </summary>
        /// <param name="model">Если запрос пришел из детальной информации карточки, такж там передается Id карточки.</param>
        /// <param name="cardState">Данный параметр необходим для изменения статуса в нужный.</param>
        /// <param name="bid">Параметр для значения при пройгранном тендере.</param>
        /// <param name="cardId">Параметр приходит при удалении из общего списка карточек</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangeCardStatus(DetailCardViewModel model, string cardState, int bid, string cardId)
        {
            try
            {
                if (model.CardId != null || cardId != null)
                {
                    string id = model.CardId == null ? cardId : model.CardId;
                    
                    Card card = _db.Cards.FirstOrDefault(c => c.Id == id); 
                    if (card != null) 
                    { 
                        switch (cardState) 
                        { 
                            case "Проработка":
                                
                                _nLogger.Info($"Карточка {card.Number} взята в проработку.");
                                
                                card.CardState = CardState.Проработка;
                                card.ExecutorId = model.UserId;
                                card.Executor =  _userManager.FindByIdAsync(model.UserId).Result;
                                card.DateOfProcessingEnd = model.Card.DateOfProcessingEnd;
                                card.DateOfAuctionStartUpdated = model.Card.DateOfAuctionStartUpdated;
                                break;
                            
                            case "ПКО": 
                            
                                _nLogger.Info($"Карточка {card.Number} переведена в ПКО.");
                                
                                card.CardState = CardState.ПКО; 
                                card.DateOfProcessingEnd = model.Card.DateOfProcessingEnd;
                                card.DateOfAuctionStartUpdated = model.Card.DateOfAuctionStartUpdated; 
                                break;
                            
                            case "Торги": 
                                
                                _nLogger.Info($"Карточка {card.Number} переведена на Торги.");
                                
                                card.CardState = CardState.Торги; 
                                card.DateOfProcessingEnd = model.Card.DateOfProcessingEnd;
                                card.DateOfAuctionStartUpdated = model.Card.DateOfAuctionStartUpdated;
                                break;
                            
                            case "Удалена": 
                                
                                _nLogger.Info($"Карточка {card.Number} помечена на удаление.");
                                
                                card.CardState = CardState.Удалена;
                                _db.Cards.Update(card);
                                await _db.SaveChangesAsync();
                                return RedirectToAction("AllCardsList", "Cards", new {sort = CardState.Новая});
                                break;
                            
                            case "Восстановлена":
                                
                                _nLogger.Info($"Карточка {card.Number} восстановлена до состояния новая.");
                               
                                card.CardState = CardState.Новая;
                                break;
                            
                            case "Проиграна": 
                                
                                _nLogger.Info($"Карточка {card.Number} переведена в состояние проиграна.");
                                
                                card.CardState = CardState.Проиграна; 
                                card.Bidding = bid;
                                SaveCloneCard(card); 
                                break;
                            
                            case "Выиграна": 
                                
                                _nLogger.Info($"Карточка {card.Number} переведена в состояние выиграна.");
                                
                                card.CardState = CardState.Выиграна; 
                                SaveCloneCard(card); 
                                break;
                            
                            case "Активна": 
                                
                                _nLogger.Info($"Карточка {card.Number} переведена в состояние активна.");
                                
                                card.CardState = CardState.Активна; 
                                break;
                            
                            case "Закрыта": 
                                
                                _nLogger.Info($"Карточка {card.Number} переведена в состояние закрыта.");
                                
                                card.CardState = CardState.Закрыта; 
                                break;
                        }
                        _db.Cards.Update(card);
                        await _db.SaveChangesAsync();
                        return RedirectToAction("DetailCard2", "Cards", new {cardId = card.Id});
                    }
                }
                return NotFound("Не найдено");
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Добавление файлов на детальной информации карточки 
        /// </summary>
        /// <param name="uploads">Загружаемый файл</param>
        /// <param name="cardId">Айди карточки для прикрепления файла к искомой карточке</param>
        /// <returns></returns>
        [HttpPost] 
        public async Task<IActionResult> AddFile(IFormFileCollection uploads, string cardId)
        {
            try
            {
                if (!string.IsNullOrEmpty(cardId))
                {
                    Card card = _db.Cards.FirstOrDefault(c => c.Id == cardId);
                    if (card != null)
                    {
                        foreach(var uploadedFile in uploads)
                        {
                            if (!Directory.Exists( _appEnvironment.WebRootPath + $"/Files/{card.Number}"))
                            {
                                Directory.CreateDirectory(_appEnvironment.WebRootPath + $"/Files/{card.Number}");
                            }
                        
                            string path = $"/Files/{card.Number}" + uploadedFile.FileName;
                            using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                            {
                                await uploadedFile.CopyToAsync(fileStream);
                            }
                            FileModel file = new FileModel { Name = uploadedFile.FileName, Path = path, CardId = card.Id, Card = card};
                           
                            _nLogger.Info($"К карточке {card.Number} прикреплен файл: {uploadedFile.FileName}.");
                            
                            _db.Files.Add(file); 
                        } 
                        await _db.SaveChangesAsync(); 
                        return RedirectToAction("DetailCard2", "Cards", new {cardId = card.Id});
                    }
                    return Content("Такой карточки не обнаруженно");
                 
                }

                return NotFound();
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
            
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
        public IActionResult AllCardsList(int? page, CardState sort, string searchByCardNumber, 
            string searchByCardName, string searchByPositionName, string searchByExecutor, DateTime searchDateFrom, DateTime searchDateTo)
        {
            try
            {
                List<Card> cards = _db.Cards.OrderBy(c=>c.DateOfAcceptingEnd).ToList();

                switch (sort)
                {
                    case CardState.Новая:
                        cards = _db.Cards.Where(c => c.CardState == CardState.Новая).OrderBy(c => c.DateOfAcceptingEnd)
                            .ToList();
                        ViewBag.sort = CardState.Новая;
                        break;

                    case CardState.Удалена:
                        cards = _db.Cards.Where(c => c.CardState == CardState.Удалена).OrderBy(c => c.Number)
                            .ToList();
                        ViewBag.sort = CardState.Удалена;
                        break;

                    case CardState.Проработка:
                        cards = _db.Cards.Where(c => c.CardState == CardState.Проработка)
                            .OrderBy(c => c.DateOfAcceptingEnd).ToList();
                        ViewBag.sort = CardState.Проработка;
                        break;

                    case CardState.ПКО:
                        cards = _db.Cards.Where(c => c.CardState == CardState.ПКО).OrderBy(c => c.DateOfAcceptingEnd)
                            .ToList();
                        ViewBag.sort = CardState.ПКО;
                        break;

                    case CardState.Торги:
                        cards = _db.Cards.Where(c => c.CardState == CardState.Торги).OrderBy(c => c.DateOfAcceptingEnd)
                            .ToList();
                        ViewBag.sort = CardState.Торги;
                        break;

                    case CardState.Выиграна:
                        cards = _db.Cards.Where(c => c.CardState == CardState.Выиграна).OrderBy(c => c.Number)
                            .ToList();
                        ViewBag.sort = CardState.Выиграна;
                        break;

                    case CardState.Проиграна:
                        cards = _db.Cards.Where(c => c.CardState == CardState.Проиграна)
                            .OrderBy(c => c.Number).ToList();
                        ViewBag.sort = CardState.Проиграна;
                        break;

                    case CardState.Активна:
                        cards = _db.Cards.Where(c => c.CardState == CardState.Активна).OrderBy(c => c.Number)
                            .ToList();
                        ViewBag.sort = CardState.Активна;
                        break;

                    case CardState.Закрыта:
                        cards = _db.Cards.Where(c => c.CardState == CardState.Закрыта).OrderBy(c => c.Number)
                            .ToList();
                        ViewBag.sort = CardState.Закрыта;
                        break;
                }

                if (searchByCardNumber != null)
                {
                    cards = _db.Cards.Where(c => c.CardState == sort).OrderBy(c => c.DateOfAcceptingEnd).ToList();
                    cards = _db.Cards.Where(c => c.CardState == CardState.Новая).OrderBy(c => c.DateOfAcceptingEnd)
                        .ToList();
                    cards = cards.Where(c => c.Number.ToLower().Contains(searchByCardNumber.ToLower())).ToList();
                    ViewBag.searchByCardNumber = searchByCardNumber;
                }
                
                if (searchByCardName != null)
                {
                    cards = _db.Cards.Where(c => c.CardState == sort).OrderBy(c => c.DateOfAcceptingEnd).ToList();
                    cards = cards.Where(c => c.Name.ToLower().Contains(searchByCardName.ToLower())).ToList();
                    ViewBag.searchByCardName = searchByCardName;
                }
                
                if (searchByPositionName != null)
                {
                    cards = _db.Cards.Where(c => c.CardState == sort).OrderBy(c => c.DateOfAcceptingEnd).ToList();
                    cards = cards.Where(c =>
                        c.Positions.Any(p => p.Name.ToLower().Contains(searchByPositionName.ToLower()))).ToList();
                    ViewBag.searchByPositionName = searchByPositionName;
                }
                
                if (searchByExecutor != null)
                {
                    cards = _db.Cards.Where(c => c.CardState == sort && c.CardState != CardState.Новая && c.CardState != CardState.Удалена)
                        .OrderBy(c => c.DateOfAcceptingEnd).ToList();
                    cards = cards.Where(c => c.Executor.Name.ToLower().Contains(searchByExecutor.ToLower())
                                             || c.Executor.Surname.ToLower().Contains(searchByExecutor.ToLower()))
                        .ToList();
                    ViewBag.searchByExecutor = searchByExecutor;
                }
                
                if (searchDateFrom != null && searchDateTo != null && searchDateFrom != DateTime.MinValue && searchDateTo != DateTime.MinValue)
                {
                    cards = _db.Cards.Where(c => c.CardState == sort).OrderByDescending(c => c.DateOfAcceptingEnd).ToList();
                    cards = cards
                        .Where(c => c.DateOfAcceptingEnd >= searchDateFrom && c.DateOfAcceptingEnd <= searchDateTo)
                        .OrderBy(c => c.DateOfAcceptingEnd).ToList();
                    ViewBag.searchDateFrom = searchDateFrom;
                    ViewBag.searchDateTo = searchDateTo;
                }

                int pageSize = 20;
                int pageNumber = page ?? 1;
                
                _nLogger.Info($"Пользователь {_userManager.GetUserAsync(User).Result.Name} {_userManager.GetUserAsync(User).Result.Surname} сделал поиск карточек по статусу: {sort}");
                
                return View(cards.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание ошибка: {e.Message} => {e.StackTrace}");
                throw;
                
            }
            
        }
        
       [HttpGet]
       [Authorize]
       public IActionResult AuctionCards(AuctionCardsViewModel model, string cardStatus, string searchByCardNumber, string searchByCardName,
           string searchByExecutor, int? page)
       {
           try
           {
               model.Cards = _db.HistoryOfVictoryAndLosing.ToList();
               switch (cardStatus)
               {
                   case "All":
                       model.Cards = _db.HistoryOfVictoryAndLosing.ToList();
                       break;
                   case "winnerCard":
                       model.Cards = model.Cards.Where(c=>c.CardState == CardState.Выиграна).ToList();
                       ViewBag.cardStatus = cardStatus;
                       break;
                   case "looserCard":
                       model.Cards = model.Cards.Where(c => c.CardState == CardState.Проиграна).ToList();
                       ViewBag.cardStatus = cardStatus;
                       break;
               }

               if (searchByCardNumber != null)
               {
                   model.Cards = model.Cards.Where(c => c.Number.ToLower().Contains(searchByCardNumber.ToLower())).ToList();
                   ViewBag.searchByCardNumber = searchByCardNumber;
               }
               
               if (searchByCardName != null)
               {
                   model.Cards = model.Cards.Where(c => c.Name.ToLower().Contains(searchByCardName.ToLower())).ToList();
                   ViewBag.searchByCardName = searchByCardName;
               }
               
               if (searchByExecutor != null)
               {
                   model.Cards = model.Cards.Where(c => c.Executor.Name.ToLower().Contains(searchByExecutor.ToLower()) 
                                                        || c.Executor.Surname.ToLower().Contains(searchByExecutor.ToLower())).ToList();
                   ViewBag.searchByExecutor = searchByExecutor;
               }
               int pageSize = 20;
               int pageNumber = (page ?? 1);
               return View(model.Cards.ToPagedList(pageNumber, pageSize));
           }
           catch (Exception e)
           {
               _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
               _iLogger.Log(LogLevel.Error, $"Внимание ошибка: {e.Message} => {e.StackTrace}");
               throw;
           }
       }

       
       /// <summary>
       /// Данный метод позволяет делать копию тех карточек которые участвовали в торгах и сохранять их БД.
       /// </summary>
       /// <param name="card">Параметр карточки для клонирования.</param>
       [NonAction]
       private void SaveCloneCard(Card card)
       {
           try
           { 
               CardClone cardClone = new CardClone()
               {
                   Auction = card.Auction,
                   Bidding = card.Bidding,
                   Broker = card.Broker,
                   Executor = card.Executor,
                   Initiator = card.Initiator,
                   Links = card.Links,
                   Name = card.Name,
                   Number = card.Number,
                   Positions = card.Positions,
                   State = card.State,
                   BestPrice = card.BestPrice,
                   CardState = card.CardState,
                   ExecutorId = card.ExecutorId,
                   LinkNames = card.LinkNames,
                   StartSumm = card.StartSumm,
                   DateOfAcceptingEnd = card.DateOfAcceptingEnd,
                   DateOfAuctionStart = card.DateOfAuctionStart
               };
               
               _nLogger.Info($"Клон карточки {cardClone.Number} создан и сохранён в базу.");
               
               _db.HistoryOfVictoryAndLosing.Add(cardClone);
               _db.SaveChanges();
           }
           catch (Exception e)
           {
               _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
               _iLogger.Log(LogLevel.Error, $"Внимание ошибка: {e.Message} => {e.StackTrace}");
               throw;
           }
           

       }
       

       [HttpPost]
       [Authorize]
       public async Task<IActionResult> Comment(DetailCardViewModel model)
       {
           try
           {
               if (model.Comment.Message != null)
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
               }
               
               return RedirectToAction("DetailCard2", "Cards", new {cardId = model.Comment.CardId});
           }
           catch (Exception e)
           {
               _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
               _iLogger.Log(LogLevel.Error, $"Внимание ошибка: {e.Message} => {e.StackTrace}");
               throw;
           }
           
       }

       [HttpGet]
       [Authorize(Roles = "admin")]
       public IActionResult ChangeComment(string commentId)
       {
           try
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
                       return View("ChangeComment", model);
                   }

                   return NotFound();
               }

               return NotFound();
           }
           catch (Exception e)
           {
               _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
               _iLogger.Log(LogLevel.Error, $"Внимание ошибка: {e.Message} => {e.StackTrace}");
               throw;
           }
       }

       [HttpPost] 
       public async Task<IActionResult> ChangeComment(ChangeCommentViewModel model)
       {
           try
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
                       return RedirectToAction("DetailCard2", "Cards", new {cardId = comment.Card.Id});
                   }

                   return NotFound();
               }

               return View(model);
           }
           catch (Exception e)
           {
               _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
               _iLogger.Log(LogLevel.Error, $"Внимание ошибка: {e.Message} => {e.StackTrace}");
               throw;
           }
       }

       [HttpPost]
       public async Task<IActionResult> AddCommentToArchiveAjax(string cardId, string comment)
       {
           List<CardClone> cardClones = _db.HistoryOfVictoryAndLosing.ToList();
           CardClone card = _db.HistoryOfVictoryAndLosing.FirstOrDefault(c => c.Id == cardId);
           if (card != null)
           {
               if (comment != null)
               {
                   card.Comment = comment;
               
                   _db.HistoryOfVictoryAndLosing.Update(card);
                   await _db.SaveChangesAsync();
                   return PartialView("PartialViews/Comment/AddCommentToArchivePartialView", cardClones);
               }
           }

           return NotFound();
       }
    }
}