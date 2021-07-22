using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public async Task<IActionResult> AddCalculation(string positionId)
        {
            if (!string.IsNullOrEmpty(positionId))
            {
                var position = await _db.Positions.FirstOrDefaultAsync(p => p.Id == positionId);
                if (position != null)
                {
                    return View(position);
                }

                return Content("Такой позиции не существует");
            }

            return NotFound();
        }

        [HttpGet]
        public IActionResult GetInfoInputData()
        {
            var a = _db.InputDataUsers.ToList()[0];
            
            GetInfoInputDataViewModel model = new GetInfoInputDataViewModel()
            {
                ListInputData = _db.InputDataUsers.ToList()[0],
                Currencies = _db.Currencies.ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddInputData(int? collectionNum, string value, string key, string sum)
        {
           if (collectionNum != null)
           {
                InputDataUser inputDataUser = _db.InputDataUsers.ToList()[0];
                switch (collectionNum)
                {
                    case 1:
                        sum = sum.Replace(".", ",");
                        Currency currency = new Currency() {Name = key, Сourse = Convert.ToDouble(sum)};
                        
                        _db.Currencies.Add(currency);
                        await _db.SaveChangesAsync();
                        
                        return RedirectToAction("GetInfoInputData");
                        
                    case 2:
                        inputDataUser.DelivTerm.Add(value);
                        break;
                    
                    case 3:
                        inputDataUser.PayTerm.Add(value);
                        break;
                    
                    case 4:
                        inputDataUser.Meas.Add(value);
                        break
                            ;
                    case 5:
                        inputDataUser.NDS.Add(value);
                        break;
                    
                }

                _db.InputDataUsers.Update(inputDataUser);
                await _db.SaveChangesAsync();

                return RedirectToAction("GetInfoInputData");


            }

            return Content("Значение не добавлено");

        }

        [HttpPost]
        public async Task<IActionResult> DeleteInputData(int? collectionNum, string value, string currencyId)
        {
            if (collectionNum != null)
            {
                InputDataUser inputDataUser = _db.InputDataUsers.ToList()[0];
                switch (collectionNum)
                {
                    case 1:
                        Currency currency = await _db.Currencies.FirstOrDefaultAsync(c => c.Id == currencyId);
                        
                        _db.Currencies.Remove(currency);
                        await _db.SaveChangesAsync();
                        
                        return RedirectToAction("GetInfoInputData");
                        
                    case 2:
                        inputDataUser.DelivTerm.Remove(value);
                        break;
                    
                    case 3:
                        inputDataUser.PayTerm.Remove(value);
                        break;
                    
                    case 4:
                        inputDataUser.Meas.Remove(value);
                        break;
                    
                    case 5:
                        inputDataUser.NDS.Remove(value);
                        break;
                    
                }

                _db.InputDataUsers.Update(inputDataUser);
                await _db.SaveChangesAsync();

                return RedirectToAction("GetInfoInputData");
            }

            return Content("Значение не удалено");
        }




    }
}