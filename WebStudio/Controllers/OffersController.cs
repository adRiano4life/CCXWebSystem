using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using NLog;
using Org.BouncyCastle.Asn1.X509;
using WebStudio.Helpers;
using WebStudio.Models;
using WebStudio.Services;
using X.PagedList;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace WebStudio.Controllers
{
    [Authorize]
    public class OffersController : Controller
    {
        private WebStudioContext _db;
        private FileUploadService _uploadService;
        private IHostEnvironment _environment;
        Logger _nLogger = LogManager.GetCurrentClassLogger();

        
        public OffersController(WebStudioContext db, FileUploadService uploadService, IHostEnvironment environment)
        {
            _db = db;
            _uploadService = uploadService;
            _environment = environment;
        }
        
        
        public IActionResult Index(string searchByCardNumber, string searchBySupplierName, int? page)
        {
            try
            {
                List<Offer> offers = _db.Offers.ToList();
            
                if (searchByCardNumber != null)
                {
                    offers = offers.Where(s => s.CardNumber.ToLower().Contains(searchByCardNumber.ToLower())).ToList();
                    ViewBag.searchByCardNumber = searchByCardNumber;
                }
            
                if (searchBySupplierName != null)
                {
                    offers = offers.Where(s => s.SupplierName.ToLower().Contains(searchBySupplierName.ToLower())).ToList();
                    ViewBag.searchBySupplierName = searchBySupplierName;
                }
                
                int pageSize = 20;
                int pageNumber = (page ?? 1);
                return View($"Index",offers.OrderByDescending(s=>s.CardNumber).ToPagedList(pageNumber, pageSize));
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        
        [HttpGet]
        public IActionResult Create(string userId)
        {
            try
            {
                if (userId == null)
                    return NotFound();
                Offer offer = new Offer(){UserId = userId};
                return View("Create", offer);
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.TargetSite}: {e.Message} | {e.StackTrace}");
                throw;
            }
        }
        
        
        [HttpPost]
        public async Task<IActionResult> Create(Offer offer)
        {
            try
            {
                if (ModelState.IsValid && offer.File != null)
                {
                    string rootDirName = AppCredentials.PathToFiles;
                    DirectoryInfo dirInfo = new DirectoryInfo(rootDirName);
                    foreach (var dir in dirInfo.GetDirectories())
                    {
                        if (!Directory.Exists("Offers"))
                            dirInfo.CreateSubdirectory("Offers");
                    }

                    string rootDirPath = Path.Combine(_environment.ContentRootPath, $"wwwroot\\Files\\Offers");
                    string fileType = offer.File.FileName.Substring(offer.File.FileName.IndexOf('.'));
                    var supplierName = new String(offer.SupplierName.Where(x => char.IsLetterOrDigit(x)
                        || char.IsWhiteSpace(x)).ToArray());
                
                    string fileName =
                        $"{offer.CardNumber.Substring(0, offer.CardNumber.IndexOf('/'))} - {supplierName}{fileType}";

                    _uploadService.Upload(rootDirPath, fileName, offer.File);
                    _nLogger.Info($"Загружен файл комм.предложения");
                    offer.Path = $"/Offers/{fileName}";
                    offer.FileName = fileName;

                    Card card = await _db.Cards.FirstOrDefaultAsync(c => c.Number == offer.CardNumber);
                    if (card == null)
                        return NotFound();

                    offer.CardId = card.Id;
                    _db.Offers.Add(offer);
                    await _db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                return View(offer);
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.TargetSite}: {e.Message} | {e.StackTrace}");
                throw;
            }
        }
        
        
        public IActionResult DownloadFile(string path, string fileName)
        {
            try
            {
                if (path == null && fileName == null)
                {
                    _nLogger.Info($"Ошибка при скачивании комм.предложения: передано пустое значение в путь/имя " +
                                  $"файла в DownloadFile");
                    return NotFound();
                }
        
                Offer offer = _db.Offers.FirstOrDefault(o => o.Path == path && o.FileName == fileName);
                if (offer == null)
                    return NotFound();
                string contentType = GetContentType(fileName);
            
                var filePath = Path.Combine("", "/Files" + path);
                try
                {
                    return File(filePath, contentType, fileName);
                }
                catch (Exception e)
                {
                    _nLogger.Info("Ошибка при скачивании файла");
                    _nLogger.Error($"Внимание ошибка: {e.TargetSite}: {e.Message} | {e.StackTrace}");
                    throw;
                }
                finally
                { 
                    RedirectToAction("Index");
                }
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.TargetSite}: {e.Message} | {e.StackTrace}");
                throw;
            }
        }
        
        
        [NonAction]
        private string GetContentType(string filename) {
            try
            {
                var dictionary = new Dictionary<string, string> {
                    { ".doc", "application/ms-word" },
                    { ".docx", "application/ms-word" },
                    { ".xls", "application/ms-excel" },
                    { ".xlsx", "application/ms-excel" },
                    { ".pdf", "application/pdf" },
                    { ".png", "image/png" },
                    { ".jpg", "image/jpg" }
                };
                string contentType = "";
                string fileExtension = Path.GetExtension(filename);
                dictionary.TryGetValue(fileExtension, out contentType);
                return contentType;
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.TargetSite}: {e.Message} | {e.StackTrace}");
                throw;
            }
        }

    }
}