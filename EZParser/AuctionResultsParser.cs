using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog;
using WebStudio.Enums;
using WebStudio.Models;

namespace EZParser
{
    public class AuctionResultsParser
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public static void GetAuctionResults()
        {
            try
            {
                Console.WriteLine($"{DateTime.Now} - Парсинг результатов аукциона начат");
                _logger.Info("Парсинг результатов аукциона начат");
                
                string connection = Program.DefaultConnection;
                
                var optionsBuilder = new DbContextOptionsBuilder<WebStudioContext>();
                var options = optionsBuilder.UseNpgsql(connection).Options;

                using WebStudioContext _db = new WebStudioContext(options);
                var url = @"https://info.ccx.kz/ru/result?per-page=100";
                HtmlWeb web = new HtmlWeb();

                var docAllPosition = web.Load(url);
                var rows = docAllPosition.DocumentNode.SelectNodes("//tbody//tr");
                var doc = new HtmlDocument();

                foreach (var row in rows)
                {
                    doc.LoadHtml(row.InnerHtml);
                    var tds = doc.DocumentNode.SelectNodes("//td");
                    string[] auctionDateStrings = tds[2].InnerText.Split(".");
                    string date = $"{auctionDateStrings[1]}/{auctionDateStrings[0]}/{auctionDateStrings[2]}";
                    DateTime auctionDate = Convert.ToDateTime(date);

                    DateTime signDate = new DateTime();
                    if (!tds[3].InnerText.Contains("-"))
                    {
                        string[] signDateStrings = tds[3].InnerText.Split(".");
                        string signStringDate = $"{signDateStrings[1]}/{signDateStrings[0]}/{signDateStrings[2]}";
                        signDate = Convert.ToDateTime(signStringDate);
                    }
                    else
                    {
                        signDate = DateTime.MinValue;
                    }

                    string contractStringSum = tds[6].InnerText.Trim();
                    decimal contractSum = 0;
                    if (!tds[6].InnerText.Contains("-"))
                    {
                        contractStringSum = contractStringSum.Replace(" ", "");
                        contractStringSum = contractStringSum.Replace(",", ".");
                        contractSum = Convert.ToDecimal(contractStringSum);
                    }
                    else
                    {
                        contractSum = Decimal.MinValue;
                    }
                    
                    if (_db.Cards.Any(c=>c.Number == tds[0].InnerText) && !_db.AuctionResults.Any(r=>r.Number == tds[0].InnerText))
                    {
                        AuctionResult result = new AuctionResult
                        {
                            Number = tds[0].InnerText,
                            Name = tds[1].InnerText,
                            DateOfAuctionStart = auctionDate,
                            Winner = tds[5].InnerText,
                            DateOfSignContract = signDate,
                            Sum = contractSum,
                        };

                        _db.AuctionResults.Add(result);
                        _db.SaveChanges();
                        Console.WriteLine($"{DateTime.Now} - Результат аукциона по лоту {result.Number} создан");
                        _logger.Info($"Результат аукциона по лоту {result.Number} создан");
                    }
                }
                Console.WriteLine($"{DateTime.Now} - Парсинг результатов аукциона закончен");
                _logger.Info("Парсинг результатов аукциона закончен");
            }
            
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }

        }
    }
}