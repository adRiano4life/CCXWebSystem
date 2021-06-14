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
                 string cardNumber1 = "T-0090412/1", cardName1 ="насосы-fake"; //фэйковый лот который не состоялся
                 string cardNumber2 = "T-0090370/1", cardName2 ="Кабельная продукция-fake"; //фэйковый лот который состоялся
                 
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
                 }

                 if (!_db.Cards.Any(c => c.Number == cardNumber2))
                 {
                     Card fakeCard2 = new Card
                     {
                         Number = cardNumber2, Name = cardName2, StartSumm = 0,
                         DateOfAcceptingEnd = DateTime.Now, DateOfAuctionStart = DateTime.Now,
                         CardState = CardState.Новая,  ExecutorId = null,
                         Links = new List<string>(), LinkNames = new List<string>()
                     };
                     _db.Cards.Add(fakeCard2);
                     _db.SaveChanges();    
                 }
                 GetAuctionResults();
            }

        }
    }
}