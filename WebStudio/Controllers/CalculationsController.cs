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
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

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
        public async Task<IActionResult> AddCalculation(string offerId, string positionId, string value)
        {
            try
            {
                if (!string.IsNullOrEmpty(positionId) && !string.IsNullOrEmpty(offerId))
                {
                    var position = await _db.Positions.FirstOrDefaultAsync(p => p.Id == positionId);
                    Offer offer = await _db.Offers.FirstOrDefaultAsync(of => of.Id == offerId);
                    if (!_db.InputDataUsers.Any())
                    {
                        InputDataUser inputDataUser = new InputDataUser();
                        await _db.InputDataUsers.AddAsync(inputDataUser);
                    }
                    
                    if (position != null && offer != null)
                    {
                        GetInfoInputDataViewModel model = new GetInfoInputDataViewModel()
                        {
                            ListInputData = _db.InputDataUsers.ToList()[0],
                            Currencies = _db.Currencies.ToList(),
                            Position = position,
                            Offer = offer,
                            Value = value
                        };
                        return View(model);
                    }
                    return Content("Такой позиции не существует");
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
        public async Task<IActionResult> SaveDataCalculations(InputDataViewModel inputData, ResultsInputDataViewModel resultsInputData, string value)
        {
            try
            {
                if (inputData != null && resultsInputData != null && inputData.OfferId != null && inputData.PositionId != null)
                {
                    Offer offer = await _db.Offers.FirstOrDefaultAsync(o => o.Id == inputData.OfferId);
                    CardPosition position = await _db.Positions.FirstOrDefaultAsync(p => p.Id == inputData.PositionId);

                    if (offer != null && position != null)
                    {
                        if (value == "calculation")
                        {
                            Create(inputData, resultsInputData, offer, position);
                            return Content("Вводимые расчеты и результат удачно сохранены.");
                        }
                        
                        else if(value == "recalculation")
                        {
                            Update(inputData, resultsInputData, offer, position);
                            return Content("Перерасчет произведён успешно.");
                        }
                    }
                    return Content("Не найдены КП и нужная позиция, попробуйте снова.");
                }
                return Content("Нет данных, попробуйте снова.");
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInfoResultOfferPosition(string offerId, string positionId)
        {
            try
            {
                if (!string.IsNullOrEmpty(positionId) && !string.IsNullOrEmpty(offerId))
                {
                    InfoResultsOfferPosition infoResultsOfferPosition =
                        await _db.InfoResultsOfferPositions.FirstOrDefaultAsync(i => i.OfferId == offerId && i.PositionId == positionId);
                    
                    if (infoResultsOfferPosition != null)
                    {
                        
                        return View(infoResultsOfferPosition);
                    }

                    return Content("Такой позиции результаты не найдены");
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

        [HttpGet]
        public IActionResult GetInfoInputData()
        {
            try
            {
                if (!_db.InputDataUsers.Any())
                {
                    InputDataUser inputDataUser = new InputDataUser();
                    _db.InputDataUsers.Add(inputDataUser);
                }
                GetInfoInputDataViewModel model = new GetInfoInputDataViewModel()
                {
                    ListInputData = _db.InputDataUsers.ToList()[0],
                    Currencies = _db.Currencies.ToList()
                };
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
        public async Task<IActionResult> AddInputData(int? collectionNum, string value, string key, string sum)
        { 
            try 
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
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeleteInputData(int? collectionNum, string value, string currencyId)
        {
            try
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
            
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        [NonAction]
        private void Update(InputDataViewModel inputData, ResultsInputDataViewModel resultsInputData, Offer offer, CardPosition position)
        {
            try
            {
                InfoResultsOfferPosition infoRes =_db.InfoResultsOfferPositions.FirstOrDefault(x => x.OfferId == offer.Id && x.PositionId == position.Id);
                if (infoRes != null)
                {
                    var inData = _db.InputDatas.FirstOrDefault(i => i.Id == infoRes.InputDataId);
                    var resInputData = _db.ResultsInputDatas.FirstOrDefault(r => r.Id == infoRes.ResultsInputDataId);

                    if (inData != null && resInputData != null)
                    {
                        inData.Currency = inputData.Currency;
                        inData.Prepay = inputData.Prepay;
                        inData.NDS = inputData.NDS;
                        inData.KPN = inputData.KPN;
                        inData.PayMethod = inputData.PayMethod;
                        inData.DelivTerm = inputData.DelivTerm;
                        inData.Meas = inputData.Meas;
                        inData.Purchase = inputData.Purchase;
                        inData.Amount = inputData.Amount;
                        inData.Bet = inputData.Bet;
                        inData.Duty = inputData.Duty;
                        inData.Transport = inputData.Transport;
                        inData.Administrative = inputData.Administrative;
                        inData.TermPayment = inputData.TermPayment;
                        inData.City = inputData.City;
                        inData.DeliveryTime = Convert.ToDateTime(inputData.DeliveryTime);
                        inData.Description = inputData.Description;

                        resInputData.Summ = resultsInputData.Summ;
                        resInputData.summTenge = resultsInputData.summTenge;
                        resInputData.Broker = resultsInputData.Broker;
                        resInputData.NDSImport = resultsInputData.NDSImport;
                        resInputData.Investments = resultsInputData.Investments;
                        resInputData.tPay = resultsInputData.tPay;
                        resInputData.Bet = resultsInputData.Bet;
                        resInputData.Payouts = resultsInputData.Payouts;
                        resInputData.Total = resultsInputData.Total;
                        resInputData.NDS = resultsInputData.NDS;
                        resInputData.NDSTenge = resultsInputData.NDSTenge;
                        resInputData.KPN = resultsInputData.KPN;
                        resInputData.KPNTenge = resultsInputData.KPNTenge;
                        resInputData.EconomyNDS = resultsInputData.EconomyNDS;
                        resInputData.Profit = resultsInputData.Profit;

                        _db.InputDatas.Update(inData);
                        _db.ResultsInputDatas.Update(resInputData);
                        
                        _db.SaveChanges();
                        
                        _nLogger.Info($"Обнавлен расчет для КП: {infoRes.Id} => Номер КП: {infoRes.Offer.Number}");
                    }
                }
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        [NonAction]
        private void Create(InputDataViewModel inputData, ResultsInputDataViewModel resultsInputData, Offer offer, CardPosition position)
        {
            try
            {
                InputData inData = new InputData()
                {
                    Currency = inputData.Currency,
                    Prepay = inputData.Prepay,
                    NDS = inputData.NDS,
                    KPN = inputData.KPN,
                    PayMethod = inputData.PayMethod,
                    DelivTerm = inputData.DelivTerm,
                    Meas = inputData.Meas,
                    Purchase = inputData.Purchase,
                    Amount = inputData.Amount,
                    Bet = inputData.Bet,
                    Duty = inputData.Duty,
                    Transport = inputData.Transport,
                    Administrative = inputData.Administrative,
                    TermPayment = inputData.TermPayment,
                    City = inputData.City,
                    DeliveryTime = Convert.ToDateTime(inputData.DeliveryTime),
                    Description = inputData.Description,
                };
                
                ResultsInputData resInputData = new ResultsInputData()
                {
                    Summ = resultsInputData.Summ,
                    summTenge = resultsInputData.summTenge,
                    Broker = resultsInputData.Broker,
                    NDSImport = resultsInputData.NDSImport,
                    Investments = resultsInputData.Investments,
                    tPay = resultsInputData.tPay,
                    Bet = resultsInputData.Bet,
                    Payouts = resultsInputData.Payouts,
                    Total = resultsInputData.Total,
                    NDS = resultsInputData.NDS,
                    NDSTenge = resultsInputData.NDSTenge,
                    KPN = resultsInputData.KPN,
                    KPNTenge = resultsInputData.KPNTenge,
                    EconomyNDS = resultsInputData.EconomyNDS,
                    Profit = resultsInputData.Profit,
                    InputDataId = inData.Id,
                    
                };


                InfoResultsOfferPosition infoResults = new InfoResultsOfferPosition()
                {
                    OfferId = offer.Id,
                    Offer = offer,
                            
                    PositionId = position.Id,
                    Position = position,
                            
                    InputDataId = inData.Id,
                    InputData = inData,
                            
                    ResultsInputDataId = resInputData.Id,
                    ResultsInputData = resInputData
                            
                };

                _db.InputDatas.Add(inData);
                _db.ResultsInputDatas.Add(resInputData);
                _db.InfoResultsOfferPositions.Add(infoResults);

                _db.SaveChanges();
                
                _nLogger.Info($"Внимание добавлен расчет для КП: {infoResults.Id} => Номер КП: {infoResults.Offer.Number}");
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }




    }
}