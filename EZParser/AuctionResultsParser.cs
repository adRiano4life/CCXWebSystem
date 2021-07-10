using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using WebStudio.Enums;
using WebStudio.Models;

namespace EZParser
{
    public class AuctionResultsParser
    {
        public static void GetAuctionResults()
        {
            try
            {
                string connection = Program.DefaultConnection; // общая строка
                
                var optionsBuilder = new DbContextOptionsBuilder<WebStudioContext>();
                var options = optionsBuilder.UseNpgsql(connection).Options;

                using WebStudioContext _db = new WebStudioContext(options);
                var url = @"https://info.ccx.kz/ru/result?per-page=100";
                HtmlWeb web = new HtmlWeb();

                var docAllPosition = web.Load(url);
                var rows = docAllPosition.DocumentNode.SelectNodes("//tbody//tr");
                var doc = new HtmlDocument();
                //WebClient client = new WebClient();   
                
                foreach (var row in rows)
                {
                    doc.LoadHtml(row.InnerHtml);
                    var tds = doc.DocumentNode.SelectNodes("//td");
                    
                    // вывод номеров лотов по которым есть результаты на сайте
                    // if (tds[0].InnerText != null) 
                    //     Console.WriteLine(tds[0].InnerText);
                    
                    if (_db.Cards.Any(c=>c.Number == tds[0].InnerText) && !_db.AuctionResults.Any(r=>r.Number == tds[0].InnerText))
                    {
                        AuctionResult result = new AuctionResult
                        {
                            Number = tds[0].InnerText,
                            Name = tds[1].InnerText,
                            DateOfAuctionStart = Convert.ToDateTime(tds[2].InnerText),
                            Winner = tds[5].InnerText
                        };
                        result.DateOfSignContract = tds[3].InnerText.Contains("-") ? DateTime.MinValue : Convert.ToDateTime(tds[3]?.InnerText);
                        result.Sum = tds[6].InnerText.Contains("-") ? Decimal.MinValue : Convert.ToDecimal(tds[6]?.InnerText);

                        _db.AuctionResults.Add(result);
                        _db.SaveChanges();
                    }
                }
                
                 // убрать когда появятся реальные результаты аукциона
                 if (_db.AuctionResults.Count() == 0) 
                 {
                     string cardNumber1 = "T-0090668/1", cardName1 ="Запасные части МТ436-fake"; //фэйковый лот который отменен
                     string cardNumber2 = "T-0090781/1", cardName2 ="электромонтажные изделия-fake"; //фэйковый лот который состоялся
                     string cardNumber3 = "T-0090538/1", cardName3 ="Котельное оборудование-fake"; //фэйковый лот который не состоялся
                     
                     if (!_db.Cards.Any(c => c.Number == cardNumber1))
                     {
                         Card fakeCard1 = new Card
                         {
                             Number = cardNumber1, Name = cardName1, StartSumm = 0,
                             DateOfAcceptingEnd = DateTime.Now, DateOfAuctionStart = DateTime.Now,
                             CardState = CardState.Новая, ExecutorId = null,
                             Links = new List<string>(), LinkNames = new List<string>() 
                         };
                         _db.Cards.Add(fakeCard1);
                         _db.SaveChanges();

                         CardPosition position1 = new CardPosition {Name = "Трубы - T-0090668/1", Measure = "шт",
                             Amount = 10, CardId = fakeCard1.Id};
                         _db.Positions.Add(position1);
                         _db.SaveChanges();
                         
                         CardPosition position2 = new CardPosition {Name = "Молотки - T-0090668/1", Measure = "шт",
                             Amount = 5, CardId = fakeCard1.Id};
                         _db.Positions.Add(position2);
                         _db.SaveChanges();
                         
                         CardPosition position3 = new CardPosition {Name = "Лопаты совковые - T-0090668/1", Measure = "шт",
                             Amount = 8, CardId = fakeCard1.Id};
                         _db.Positions.Add(position3);
                         _db.SaveChanges();
                         
                         CardPosition position4 = new CardPosition {Name = "Лопаты штыковые - T-0090668/1", Measure = "шт",
                             Amount = 2, CardId = fakeCard1.Id};
                         _db.Positions.Add(position4);
                         _db.SaveChanges();
                         
                         CardPosition position5 = new CardPosition {Name = "Шурупы Merax - T-0090668/1", Measure = "шт",
                             Amount = 14, CardId = fakeCard1.Id};
                         _db.Positions.Add(position5);
                         _db.SaveChanges();
                     }

                     if (!_db.Cards.Any(c => c.Number == cardNumber2))
                     {
                         Card fakeCard2 = new Card
                         {
                             Number = cardNumber2, Name = cardName2, StartSumm = 0,
                             DateOfAcceptingEnd = DateTime.Now, DateOfAuctionStart = DateTime.Now,
                             CardState = CardState.Проиграна,  ExecutorId = null,
                             Links = new List<string>(), LinkNames = new List<string>(),
                             Bidding = 5000000
                         };
                         _db.Cards.Add(fakeCard2);
                         _db.SaveChanges();    
                         
                         CardPosition position6 = new CardPosition {Name = "Болты крепежные MTX - T-0090485/1", Measure = "тонна",
                             Amount = 200, CardId = fakeCard2.Id};
                         _db.Positions.Add(position6);
                         _db.SaveChanges();
                         
                         CardPosition position7 = new CardPosition {Name = "Шины к БЕЛАЗ T-120 - T-0090485/1", Measure = "комплект",
                             Amount = 500, CardId = fakeCard2.Id};
                         _db.Positions.Add(position7);
                         _db.SaveChanges();
                     }
                     
                     if (!_db.Cards.Any(c => c.Number == cardNumber3))
                     {
                         Card fakeCard3 = new Card
                         {
                             Number = cardNumber3, Name = cardName3, StartSumm = 0,
                             DateOfAcceptingEnd = DateTime.Now, DateOfAuctionStart = DateTime.Now,
                             CardState = CardState.Новая,  ExecutorId = null,
                             Links = new List<string>(), LinkNames = new List<string>()
                         };
                         _db.Cards.Add(fakeCard3);
                         _db.SaveChanges();
                         
                         CardPosition position8 = new CardPosition {Name = "Рельсы Heiner NCV-320 - T-0090538/1", Measure = "пг.м.",
                             Amount = 100, CardId = fakeCard3.Id};
                         _db.Positions.Add(position8);
                         _db.SaveChanges();
                         
                         CardPosition position9 = new CardPosition {Name = "Алюминивые перегородки АЛ-200 - T-0090538/1", Measure = "кв.м.",
                             Amount = 999, CardId = fakeCard3.Id};
                         _db.Positions.Add(position9);
                         _db.SaveChanges();
                         
                         CardPosition position10 = new CardPosition {Name = "Шурупы Merax - T-0090538/1", Measure = "упаковка",
                             Amount = 32, CardId = fakeCard3.Id};
                         _db.Positions.Add(position10);
                         _db.SaveChanges();
                     }
                     
                     GetAuctionResults();
                }
                Console.WriteLine($"{DateTime.Now} - Парсинг результатов аукциона закончен");
            }
            
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}