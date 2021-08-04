using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using NLog;
using WebStudio.Models;
using WebStudio.Services;
using WebStudio.ViewModels;
using X.PagedList;

namespace WebStudio.Controllers
{
    [Authorize]
    public class RequestsController : Controller
    {

        private WebStudioContext _db;
        private UserManager<User> _userManager;
        private readonly IHostEnvironment _environment;
        private readonly FileUploadService _uploadService;
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public RequestsController(WebStudioContext db, 
            UserManager<User> userManager,
            IHostEnvironment environment,
            FileUploadService uploadService)
        {
            _db = db;
            _userManager = userManager;
            _environment = environment;
            _uploadService = uploadService;
        }

        [HttpGet]
        public IActionResult Index(RequestIndexViewModel model, string searchByCardNumber, string searchByCardName, 
            string searchByExecutor, DateTime searchDateFrom, DateTime searchDateTo, int? page)
        {
            try
            {
                model.Requests = _db.Requests.ToList();

                if (searchByCardNumber != null)
                {
                    model.Requests = model.Requests
                        .Where(r => r.Card.Number.ToLower().Contains(searchByCardNumber.ToLower())).ToList();
                    ViewBag.searchByCardNumber = searchByCardNumber;
                }

                if (searchByCardName != null)
                {
                    model.Requests = model.Requests
                        .Where(r => r.Card.Name.ToLower().Contains(searchByCardName.ToLower())).ToList();
                    ViewBag.searchByCardName = searchByCardName;
                }

                if (searchByExecutor != null)
                {
                    model.Requests = model.Requests.Where(r =>
                        r.Executor.Name.ToLower().Contains(searchByExecutor.ToLower())
                        || r.Executor.Surname.ToLower().Contains(searchByExecutor.ToLower())).ToList();
                    ViewBag.searchByExecutor = searchByExecutor;
                }

                if (searchDateFrom != null && searchDateTo != null && searchDateFrom != DateTime.MinValue && searchDateTo != DateTime.MinValue)
                {
                    model.Requests = model.Requests
                        .Where(r => r.DateOfCreate >= searchDateFrom && r.DateOfCreate <= searchDateTo)
                        .OrderBy(r => r.DateOfCreate).ToList();
                    ViewBag.searchDateFrom = searchDateFrom;
                    ViewBag.searchDateTo = searchDateTo;
                }

                _logger.Info("Открыта таблица поданных запросов поставщикам");
                
                int pageSize = 20;
                int pageNumber = (page ?? 1);
                return View(model.Requests.ToPagedList(pageNumber, pageSize));
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
        public async Task<IActionResult> Create(CreateRequestViewModel model, List<string> selectedLinkNames)
        {
            try
            { 
                if (ModelState.IsValid)
                {
                    List<string> filePaths = new List<string>();
                    model.Card = _db.Cards.FirstOrDefault(c => c.Id == model.CardId);
                    string[] subDirectory = model.Card.Number.Split("/");
                    string attachPath = $"{model.OverallPath}/{subDirectory[0]}";
                    if (model.Files != null)
                    {
                        foreach (var file in model.Files)
                        {
                            string path = Path.Combine(_environment.ContentRootPath, $"{attachPath}");
                            string filePath = @$"{attachPath}/{file.FileName}";
                            _uploadService.Upload(path, file.FileName, file);
                            filePaths.Add(filePath);
                            _logger.Info($"Файл {file.FileName} загружен для прикрепления к запросу");
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
        public IActionResult CreateWithoutLot()
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
                 };

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
        public async Task<IActionResult> CreateWithoutLot(CreateRequestViewModel model, List<string> selectedLinkNames)
        {
            try
            { 
                if (ModelState.IsValid)
                {
                    List<string> filePaths = new List<string>();
                    //model.Card = _db.Cards.FirstOrDefault(c => c.Id == model.CardId);
                    //string[] subDirectory = model.Card.Number.Split("/");
                    if (!Directory.Exists(_environment.ContentRootPath + "/Other Files"))
                    {
                        Directory.CreateDirectory(_environment.ContentRootPath + "/Other Files");
                    }
                    string attachPath = _environment.ContentRootPath + "/Other Files";
                    if (model.Files != null)
                    {
                        foreach (var file in model.Files)
                        {
                            string path = Path.Combine(_environment.ContentRootPath, $"{attachPath}");
                            string filePath = @$"{attachPath}/{file.FileName}";
                            _uploadService.Upload(path, file.FileName, file);
                            filePaths.Add(filePath);
                            _logger.Info($"Файл {file.FileName} загружен для прикрепления к запросу");
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
                        
                        await emailService.SendMessageAsync(suppliers, "Запрос коммерческого предложения", $"{model.Text}",
                            filePaths, request.Executor, model.Card);
                        _logger.Info("Запрос поставщикам отправлен");
                        _db.SearchSuppliers.RemoveRange(_db.SearchSuppliers);
                        await _db.SaveChangesAsync();
                        return RedirectToAction("Index", "Cards");
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

        [HttpPost]
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
                    
                    _logger.Info($"Сформирована временная база данных для рассылки запроса поставщикам. {searchSupplier.Name} добавлен во временную базу");
                }

                
                
                return PartialView("SuppliersTablePartialView", supplierCard);
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> SearchSupplierWithoutLotAjax(CreateRequestViewModel model, string supplierSearchHash, string supplierCardId)
        {
            try
            {
                model.Suppliers = Search(supplierSearchHash);
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
                    
                    _logger.Info($"Сформирована временная база данных для рассылки запроса поставщикам. {searchSupplier.Name} добавлен во временную базу");
                }
                
                return PartialView("SuppliersTableWithoutLotPartialView");
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSupplierAjax(string supplierId, string supplierRemoveCardId)
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

                if (supplier.Card.Id == null)
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

        [HttpPost]
        public async Task<IActionResult> RemoveSupplierWithoutLotAjax(string supplierId)
        {
            try
            {
                SearchSupplier? supplier = _db.SearchSuppliers.FirstOrDefault(s => s.Id == supplierId);
                if (supplier != null)
                {
                    _db.SearchSuppliers.Remove(supplier);
                    await _db.SaveChangesAsync();
                }

                _logger.Info($"Поставщик {supplier?.Name} удален с временной базы поставщиков для массовой рассылки запроса");
            
                return PartialView("SuppliersTablePartialView");
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
            
                suppliers = _db.Suppliers.Where(s => s.Tags.Contains(supplierSearchHash) || s.Name.ToLower().Contains(supplierSearchHash.ToLower())).ToList();
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