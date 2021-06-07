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
            
            WebClient client = new WebClient();   
            
            foreach (var row in rows)
            {
                doc.LoadHtml(row.InnerHtml);
                
                var tds = doc.DocumentNode.SelectNodes("//td");
                var links = doc.DocumentNode.SelectNodes("//a");
                List<string> stringLinks = new List<string>();
                List<string> linkNames = new List<string>();
                foreach (var link in links)
                {
                    DirectoryInfo dirFilesInfo = new DirectoryInfo(Program.PathToFiles); // общий путь
                    
                    foreach (var dir in dirFilesInfo.GetDirectories()) 
                    {
                        if (!Directory.Exists("Results"))
                            dirFilesInfo.CreateSubdirectory("Results");
                    }
                    
                    DirectoryInfo dirResultsInfo = new DirectoryInfo(@$"{Program.PathToFiles}\Results"); // общий путь
                    
                    foreach (var dir in dirResultsInfo.GetDirectories())
                    {
                        if (!Directory.Exists("Excel"))
                            dirResultsInfo.CreateSubdirectory("Excel");
                    }
                    
                    
                    if (!tds[5].InnerText.ToLower().Contains("отмен") && !tds[5].InnerText.ToLower().Contains("состоялся"))
                    {
                        string[] subDirectory = tds[0].InnerText.Split("/");
                        dirResultsInfo.CreateSubdirectory($"{subDirectory[0]}");
                        string stringLink = $"https://info.ccx.kz{@link.Attributes[0].Value}";
                        string linkName = link.InnerText;
                        linkName = linkName.Trim();
                        
                        if (link.InnerText.Contains(".xls") && link.InnerText.Contains("Приложение"))
                        {
                            client.DownloadFile($"{stringLink}", @$"{dirResultsInfo}\Excel\{linkName}"); // общий путь
                        }
                        
                        client.DownloadFile($"{stringLink}", @$"{dirResultsInfo}\{subDirectory[0]}\{linkName}"); // общий путь
                        
                        stringLinks.Add(stringLink);
                        linkNames.Add(linkName);
                    }
                }
                
                if (_db.Cards.Any(c=>c.Number == tds[0].InnerText) && !_db.AuctionResults.Any(r=>r.Number == tds[0].InnerText))
                {
                    AuctionResult result = new AuctionResult
                    {
                        Number = tds[0].InnerText,
                        Name = tds[1].InnerText,
                        DateOfAuctionStart = Convert.ToDateTime(tds[2].InnerText),
                        Links = stringLinks,
                        LinkNames = linkNames,
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
                Card fakeCard = new Card
                {
                 Number = "T-0089444/1",
                 Name = "вентиляционное оборудование",
                 StartSumm = 39931200,
                 DateOfAcceptingEnd = DateTime.Now,
                 DateOfAuctionStart = DateTime.Now,
                 Initiator = "ТОО Корпорация Казахмыс 050140000656г.Караганда, ул.Ленина, д.12АО «Bank RBK» БИК KINCKZKA, ИИК KZ 778210139812144560",
                 Broker = "ТОО «Steppe Nomad» (CSND)+7 (708) 439-87-76",
                 Auction = "На понижение",
                 State = "Прием заявок",
                 BestPrice = "Ожидание начала торгов",
                 CardState = CardState.Новая,
                 Links =
                 {
                     "https://info.ccx.kz/ru/site/download?uid=991AB39A-7844-4D78-86EF-04ABD22AB369",
                     "https://info.ccx.kz/ru/site/download?uid=7300FEDA-BE8E-45F1-81B7-5091F61D02E4",
                     "https://info.ccx.kz/ru/site/download?uid=42A769A3-B6F5-43FB-9081-28085FA640B0",
                     "https://info.ccx.kz/ru/site/download?uid=09789A77-F2C7-493B-9935-45C2006AAA73",
                     "https://info.ccx.kz/ru/site/download?uid=B8E3F610-41C7-46FD-B933-BD4643E1185C",
                     "https://info.ccx.kz/ru/site/download?uid=F067A871-8B51-49E3-B7AD-80B0E0779B2E"
                 },
                 LinkNames =
                 {
                     " Приложение 1.docx",
                     " Приложение к торгам №T-0090413 DDP.xlsx",
                     " Квалификационные требования 13 п..docx",
                     " Приложение к торгам №T-0090413 DAP.xlsx"," ПКО 33994.pdf",
                     " ТБ Типовая форма договора поставки ТМЦ, по заказу, Резидент.docx"
                 },
                 ExecutorId = null
                };
                _db.Cards.Add(fakeCard);
                _db.SaveChanges();
                GetAuctionResults();
            }
        }
    }
}