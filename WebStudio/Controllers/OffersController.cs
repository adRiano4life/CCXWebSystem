﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NLog;
using WebStudio.Helpers;
using WebStudio.Models;
using WebStudio.Services;
using X.PagedList;

namespace WebStudio.Controllers
{
    [Authorize]
    public class OffersController : Controller
    {
        private WebStudioContext _db;
        private IWebHostEnvironment _environment;
        Logger _nLogger = LogManager.GetCurrentClassLogger();

        
        public OffersController(WebStudioContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }
        
        
        public IActionResult Index(string searchByCardNumber, string searchBySupplierName, string searchByPositionName, 
            DateTime searchByOfferDate, string resetSearch, int? page)
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

                if (searchByPositionName != null)
                {
                    foreach (var offer in offers)
                    {
                        if (offer.Card.Positions.Count() > 0)
                        {
                            foreach (var position in offer.Card.Positions)
                            {
                                if (position.Name.ToLower().Contains(searchByPositionName.ToLower()))
                                {
                                    offers = offers.Where(o => o.CardId == position.CardId).ToList();
                                    ViewBag.searchByPositionName = searchByPositionName;
                                }
                            }
                        }
                    }   
                }

                if (searchByOfferDate != null && searchByOfferDate != DateTime.MinValue)
                {
                    offers = offers.Where(o => o.DateOfIssue == searchByOfferDate).ToList();
                    ViewBag.searchByOfferDate = searchByOfferDate;
                }

                if (resetSearch != null)
                {
                    searchByOfferDate = Convert.ToDateTime("");
                    offers = _db.Offers.ToList();
                }
                int pageSize = 5;
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
        public async Task<IActionResult> Create(Offer offer, IFormFileCollection uploads)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string cardNumber = offer.CardNumber.Substring(0, offer.CardNumber.IndexOf('/'));
                    
                    //string dirFiles = AppCredentials.PathToFiles;
                    string dirFiles = _environment.WebRootPath + "/Files";
                    DirectoryInfo dirFilesInfo = new DirectoryInfo(dirFiles);
                    foreach (var dir in dirFilesInfo.GetDirectories())
                    {
                        if (!Directory.Exists($"Offers"))
                            dirFilesInfo.CreateSubdirectory("Offers");
                    }

                    DirectoryInfo dirOffersInfo = new DirectoryInfo(dirFiles + "/Offers");
                    if (dirOffersInfo.GetDirectories().Length > 0)
                    {
                        foreach (var dir in dirOffersInfo.GetDirectories())
                        {
                            if (!Directory.Exists($"{cardNumber}"))
                                dirOffersInfo.CreateSubdirectory($"{cardNumber}");
                        }    
                    }
                    else
                    {
                        dirOffersInfo.CreateSubdirectory($"{cardNumber}");
                    }
                    
                    foreach (var upFile in uploads)
                    {
                        string offerPath = $"/Files/Offers/{cardNumber}/" + upFile.FileName;
                        using (var fileStream = new FileStream(_environment.WebRootPath + offerPath, FileMode.Create))
                        {
                            await upFile.CopyToAsync(fileStream);
                        }

                        FileModel file = new FileModel 
                        {
                            Name = upFile.FileName, 
                            Path = offerPath, 
                            CardId = offer.CardId, 
                            Card = offer.Card
                        };
                        
                        _db.Files.Add(file);
                        offer.Path = file.Path;
                        offer.FileName = file.Name;
                    }

                    await _db.SaveChangesAsync();
                    _nLogger.Info($"Загружен файл комм.предложения");
                    
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
            
                var filePath = Path.Combine("", path);
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