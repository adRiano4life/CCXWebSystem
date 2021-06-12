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
using WebStudio.Models;
using WebStudio.Services;
using WebStudio.ViewModels;

namespace WebStudio.Controllers
{
    public class RequestsController : Controller
    {

        private WebStudioContext _db;
        private UserManager<User> _userManager;
        private readonly IHostEnvironment _environment;
        private readonly FileUploadService _uploadService;

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
        public IActionResult Index(RequestIndexViewModel model, string filterOrder)
        {
            model.Requests = _db.Requests.ToList();

            if (filterOrder == "DateOfCreate")
            {
                model.Requests = model.Requests
                    .Where(r => r.DateOfCreate >= model.DateFrom && r.DateOfCreate <= model.DateTo)
                    .OrderBy(r=>r.DateOfCreate).ToList();
            }

            if (filterOrder == "DateOfAcceptingEnd")
            {
                model.Requests = model.Requests
                    .Where(r => r.Card.DateOfAcceptingEnd >= model.DateFrom &&
                                r.Card.DateOfAcceptingEnd <= model.DateTo)
                    .OrderBy(r => r.Card.DateOfAcceptingEnd).ToList();
            }

            if (filterOrder == "DateOfAuctionStart")
            {
                model.Requests = model.Requests
                    .Where(r => r.Card.DateOfAuctionStart >= model.DateFrom &&
                                r.Card.DateOfAuctionStart <= model.DateTo)
                    .OrderBy(r => r.Card.DateOfAuctionStart).ToList();
            }

            if (model.ExecutorName != null)
            {
                model.ExecutorName = model.ExecutorName.ToLower();
                model.Requests = (IOrderedQueryable<Request>) model.Requests.Where(r =>
                    r.Card.Executor.Surname.Contains(model.ExecutorName));
            }
            
            return View(model);
        }

        [HttpGet]
        public IActionResult Create(string cardId)
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
            return View(model);
        }

        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> Create(CreateRequestViewModel model, string supplierHash, List<string> selectedLinkNames)
        {
            if (ModelState.IsValid)
            {
                List<string> filePaths = new List<string>(); 
                if (model.File != null)
                {
                    string path = Path.Combine(_environment.ContentRootPath, "wwwroot\\Files\\Requests");
                    string filePath = $"\\Files\\Requests\\{model.File.FileName}";
                    _uploadService.Upload(path, model.File.FileName, model.File);
                    model.FilePath = filePath;
                    filePaths.Add(filePath);
                }

                model.Card = _db.Cards.FirstOrDefault(c => c.Id == model.CardId);
                model.Suppliers = _db.Suppliers
                    .Where(s => s.Tags.Contains(supplierHash) || s.Name.Contains(supplierHash)).ToList();

                Request request = new Request
                {
                    Text = model.Text,
                    DateOfCreate = model.DateOfCreate,
                    ExecutorId = model.ExecutorId,
                    Executor = await _userManager.FindByIdAsync(model.ExecutorId),
                    CardId = model.CardId,
                    Card = _db.Cards.FirstOrDefault(c => c.Id == model.CardId),
                    Suppliers = model.Suppliers,
                    FilePath = model.FilePath
                };
                var result = _db.Requests.AddAsync(request);
                if (result.IsCompleted)
                {
                    await _db.SaveChangesAsync();
                    EmailService emailService = new EmailService();
                    string[] subDirectory = request.Card.Number.Split("/");
                    string attachPath = @$"{model.OverallPath}\{subDirectory[0]}";
                    foreach (var linkName in selectedLinkNames)
                    {
                        string filePath = $@"{attachPath}\{linkName}";
                        filePaths.Add(filePath);
                    }
                    await emailService.SendMessageAsync(model.Suppliers, "Запрос коммерческого предложения", $"{model.Text}",
                        filePaths, request.Executor, model.Card);
                    return RedirectToAction("DetailCard", "Cards", new {cardId = model.CardId});
                }
                
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AddPositionAjax(string cardId, string positionNumber, string codTNVED,
            string name, string measure, string amount, string deliveryTerms)
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
            return Json(cardPosition);
        }

        [HttpGet]
        public async Task<IActionResult> SearchSupplierAjax(CreateRequestViewModel model, string supplierSearchHash)
        {
            model.Suppliers = Search(supplierSearchHash);
            return Json(model.Suppliers);
        }

        [HttpGet]
        public async Task<IActionResult> RemoveSupplierAjax(CreateRequestViewModel model, string supplierId)
        {
            Supplier supplier = model.Suppliers.FirstOrDefault(s => s.Id == supplierId);
            if (supplier != null)
            {
                model.Suppliers.Remove(supplier);
            }

            return Json(model.Suppliers);
        }

        [NonAction]
        public List<Supplier> Search(string supplierSearchHash)
        {
            List<Supplier> suppliers = new List<Supplier>();
            if (String.IsNullOrEmpty(supplierSearchHash))
            {
                return suppliers;
            }
            
            suppliers = _db.Suppliers.Where(s => s.Tags.Contains(supplierSearchHash) || s.Name.Contains(supplierSearchHash)).ToList();
            return suppliers;
        }
    }
}