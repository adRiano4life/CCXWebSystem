using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using NLog;
using WebStudio.Models;
using WebStudio.Services;
using WebStudio.ViewModels;

namespace WebStudio.Controllers
{
    [Authorize]
    public class RequestsController : Controller
    {

        private WebStudioContext _db;
        private UserManager<User> _userManager;
        //private readonly IHostEnvironment _environment;
        private readonly IWebHostEnvironment _environment;
        private readonly FileUploadService _uploadService;
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public RequestsController(WebStudioContext db, 
            UserManager<User> userManager,
            IWebHostEnvironment environment,
            FileUploadService uploadService)
        {
            _db = db;
            _userManager = userManager;
            _environment = environment;
            _uploadService = uploadService;
        }

        [HttpGet]
        public IActionResult Index(RequestIndexViewModel model, string filterOrder)
        {
            try
            {
                model.Requests = _db.Requests.ToList();
                switch (filterOrder)
                {
                    case "DateOfCreate":
                        model.Requests = model.Requests
                            .Where(r => r.DateOfCreate >= model.DateFrom && r.DateOfCreate <= model.DateTo)
                            .OrderBy(r=>r.DateOfCreate).ToList();
                        break;
                    case "DateOfAcceptingEnd":
                        model.Requests = model.Requests
                            .Where(r => r.Card.DateOfAcceptingEnd >= model.DateFrom &&
                                        r.Card.DateOfAcceptingEnd <= model.DateTo)
                            .OrderBy(r => r.Card.DateOfAcceptingEnd).ToList();
                        break;
                    case "DateOfAuctionStart":
                        model.Requests = model.Requests
                            .Where(r => r.Card.DateOfAuctionStart >= model.DateFrom &&
                                        r.Card.DateOfAuctionStart <= model.DateTo)
                            .OrderBy(r => r.Card.DateOfAuctionStart).ToList();
                        break;
                }

                if (model.ExecutorName != null)
                {
                    model.Requests = model.Requests.Where(r =>
                        r.Card.Executor.Surname.Contains(model.ExecutorName)).ToList();
                }
                
                _logger.Info("Открыта таблица поданных запросов поставщикам");
                    
                return View(model);
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }

        }

        [HttpGet]
        public IActionResult Create(string cardId)
        {
            try
            {
                CreateRequestViewModel model = new CreateRequestViewModel
                {
                    Text = "<p>В связи с производственной необходимостью, просим Вас рассмотреть возможность предоставления коммерческого предложения в максимально короткие сроки. " +
                           "При составлении коммерческого предложения просим Вас учесть следующее:</p>" +
                           "<ol>" +
                           "<li>Соответствие запрашиваемого ТМЦ с Вашим предложением: марка, модель, модификации, ГОСТ , ТУ и т.д. В случае не “критичного” расхождения, просим указать данный факт в КП.</li>" +
                           "<li>Количество: в случае несоответствия запрашиваемого кол-ва нормам отгрузки ТМЦ, просим указать данный факт.</li>" +
                           "<li>Качество: предлагать только новый товар, не бывшим в использовании, в ином случае предупреждать в КП.</li>" +
                           "<li>Отгрузка: просим указывать варианты отгрузки, которыми располагаете (автотранспорт, жд/транспорт, авиатранспорт).</li>" +
                           "<li>Условия оплаты.</li>" +
                           "<li>В случае габаритного груза, просим указывать полные габариты в упакованном виде.</li>",
                    TextView = "В связи с производственной необходимостью, просим Вас рассмотреть возможность предоставления коммерческого предложения в максимально короткие сроки.\n " +
                               "При составлении коммерческого предложения просим Вас учесть следующее:\n" +
                               "1.    Соответствие запрашиваемого ТМЦ с Вашим предложением: марка, модель, модификации, ГОСТ , ТУ и т.д. В случае не “критичного” расхождения, просим указать данный факт в КП.\n" +
                               "2.    Количество: в случае несоответствия запрашиваемого кол-ва нормам отгрузки ТМЦ, просим указать данный факт.\n" +
                               "3.    Качество: предлагать только новый товар, не бывшим в использовании, в ином случае предупреждать в КП.\n" +
                               "4.    Отгрузка: просим указывать варианты отгрузки, которыми располагаете (автотранспорт, жд/транспорт, авиатранспорт)\n" +
                               "5.    Условия оплаты.\n" +
                               "6.    В случае габаритного груза, просим указывать полные габариты в упакованном виде.",
                    Card = _db.Cards.FirstOrDefault(c=>c.Id == cardId)
                };
                ViewBag.SearchSuppliersCard = _db.SearchSuppliers.FirstOrDefault(c => c.Id == cardId);
                _logger.Info($"Сформирована форма запроса");
                return View(model);
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }

        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRequestViewModel model, string supplierHash, List<string> selectedLinkNames)
        {
            try
            { 
                if (ModelState.IsValid)
                {
                    List<string> filePaths = new List<string>();
                    model.Card = _db.Cards.FirstOrDefault(c => c.Id == model.CardId);
                    string[] subDirectory = model.Card.Number.Split("/");
                    string attachPath = $"{Program.PathToFiles}/{subDirectory[0]}";
                    if (model.Files != null)
                    {
                        foreach (var uploadFile in model.Files)
                        {
                            string path = $"{subDirectory[0]}" + $"/{uploadFile.Name}";
                            string filePath = @$"{attachPath}/{uploadFile.FileName}";
                            //_uploadService.Upload(Program.PathToFiles + path, file);
                            using (var fileStream = new FileStream(Program.PathToFiles + path, FileMode.Create))
                            {
                                await uploadFile.CopyToAsync(fileStream);
                            }

                            FileModel file = new FileModel
                            {
                                Name = uploadFile.Name,
                                Path = "/Files" + path,
                                CardId = model.CardId,
                                Card = model.Card
                            };

                            _db.Files.Add(file);
                            await _db.SaveChangesAsync();
                            
                            filePaths.Add(filePath);
                            _logger.Info($"Файл {uploadFile.FileName} загружен для прикрепления к запросу");
                        }
                    }

                    List<SearchSupplier> suppliers = _db.SearchSuppliers.ToList();
                    
                    Request request = new Request
                    {
                        Text = model.Text,
                        DateOfCreate = model.DateOfCreate,
                        ExecutorId = model.ExecutorId,
                        Executor = await _userManager.FindByIdAsync(model.ExecutorId),
                        CardId = model.CardId,
                        Card = model.Card,
                        Suppliers = model.Suppliers,
                    };
                    var result = _db.Requests.AddAsync(request);
                    if (result.IsCompleted)
                    {
                        await _db.SaveChangesAsync();
                        EmailService emailService = new EmailService();

                        foreach (var linkName in selectedLinkNames)
                        {
                            string filePath = $@"{attachPath}/{linkName}";
                            filePaths.Add(filePath);
                            _logger.Info($"Файл {linkName} добавлен к запросу");
                        }
                        await emailService.SendMessageAsync(suppliers, "Запрос коммерческого предложения", $"{model.Text}",
                            filePaths, request.Executor, model.Card);
                        _logger.Info("Запрос поставщикам отправлен");
                        _db.SearchSuppliers.RemoveRange(_db.SearchSuppliers);
                        await _db.SaveChangesAsync();
                        return RedirectToAction("DetailCard2", "Cards", new {cardId = model.CardId});
                    }
                    
                }

                return View(model);
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> AddPositionAjax(int number, string cardId, string codTNVED,
            string name, string measure, string amount, string deliveryTerms)
        {
            try
            {
                CardPosition cardPosition = new CardPosition
                {
                    CodTNVED = codTNVED,
                    Name = name.ToUpper(),
                    Measure = measure.ToUpper(),
                    Amount = Convert.ToInt32(amount),
                    DeliveryTerms = deliveryTerms,
                    CardId = cardId,
                    Card = _db.Cards.FirstOrDefault(c=>c.Id == cardId),
                };
                await _db.Positions.AddAsync(cardPosition);
                await _db.SaveChangesAsync();
                _logger.Info($"Добавлена позиция {cardPosition.Name} к лоту {cardPosition.Card.Number}");
            
                RequestAddPositionViewModel model = new RequestAddPositionViewModel
                {
                    Card = _db.Cards.FirstOrDefault(c => c.Id == cardId),
                    CardPosition = cardPosition,
                    Number = number
                };
            
                return PartialView("PositionAddPartialView", model);
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> SearchSupplierAjax(CreateRequestViewModel model, string supplierSearchHash, string supplierCardId)
        {
            try
            {
                model.Suppliers = Search(supplierSearchHash);
                Card supplierCard = _db.Cards.FirstOrDefault(c => c.Id == supplierCardId);
                foreach (var supplier in model.Suppliers)
                {
                    SearchSupplier searchSupplier = new SearchSupplier
                    {
                        Name = supplier.Name,
                        Email = supplier.Email,
                        Website = supplier.Website,
                        PhoneNumber = supplier.PhoneNumber,
                        Address = supplier.Address,
                        Tags = supplier.Tags,
                        CardId = supplierCardId,
                        Card = _db.Cards.FirstOrDefault(c=>c.Id == supplierCardId)
                    };
                    await _db.SearchSuppliers.AddAsync(searchSupplier);
                    await _db.SaveChangesAsync();
                }
                
                _logger.Info("Сформирована временная база данных для рассылки запроса поставщикам");
                
                return PartialView("SuppliersTablePartialView", supplierCard);
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> RemoveSupplierAjax(CreateRequestViewModel model, string supplierId, string supplierRemoveCardId)
        {
            try
            {
                SearchSupplier? supplier = _db.SearchSuppliers.FirstOrDefault(s => s.Id == supplierId);
                Card supplierCard = _db.Cards.FirstOrDefault(c => c.Id == supplier.Card.Id);
                if (supplier != null && supplier.Card.Id == supplierRemoveCardId)
                {
                    _db.SearchSuppliers.Remove(supplier);
                    await _db.SaveChangesAsync();
                }

                _logger.Info($"Поставщик {supplier?.Name} удален с временной базы поставщиков для массовой рассылки запроса");
            
                return PartialView("SuppliersTablePartialView", supplierCard);
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }

        }

        [NonAction]
        public List<Supplier> Search(string supplierSearchHash)
        {
            try
            {
                List<Supplier> suppliers = new List<Supplier>();
                if (String.IsNullOrEmpty(supplierSearchHash))
                {
                    return suppliers;
                }
            
                suppliers = _db.Suppliers.Where(s => s.Tags.Contains(supplierSearchHash) || s.Name.Contains(supplierSearchHash)).ToList();
                return suppliers;
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }
    }
}