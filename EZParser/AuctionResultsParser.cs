using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using WebStudio.Models;

namespace EZParser
{
    public class AuctionResultsParser
    {


        public static void GetAuctionResults()
        {
            //string connection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=123"; // бд Гульжан
            string connection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=QWEqwe123@"; // бд Саня Ф.
            // string connection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=123"; // бд Саня Т.
            
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
                    DirectoryInfo dirFilesInfo = new DirectoryInfo(@"E:\csharp\ESDP\Download Files"); //Саня Ф.
                    //DirectoryInfo dirFilesInfo = new DirectoryInfo(@$"C:\Users\user\Desktop\files"); //Саня Т.
                    
                    // Гульжан - общая папка со всеми файлами
                    //DirectoryInfo dirFilesInfo = new DirectoryInfo(@$"D:\csharp\esdp\app\WebStudio\wwwroot\Files\"); 
                    
                    // Гульжан - в общей папке создается папка Results (если такой не существует), где будут файлы результатов
                    foreach (var dir in dirFilesInfo.GetDirectories()) 
                    {
                        if (!Directory.Exists("Results"))
                            dirFilesInfo.CreateSubdirectory("Results");
                    }
                    
                    // Гульжан - сохраняется путь к папке Results
                    //DirectoryInfo dirResultsInfo = new DirectoryInfo(@$"D:\csharp\esdp\app\WebStudio\wwwroot\Files\Results\");
                    
                    // Саня Ф.
                    DirectoryInfo dirResultsInfo = new DirectoryInfo(@$"E:\csharp\ESDP\Download Files\Results");
                    
                    // Гульжан - в папке Results создается папка для файлов Excel (если такой не существует)
                    foreach (var dir in dirResultsInfo.GetDirectories())
                    {
                        if (!Directory.Exists("Excel"))
                            dirResultsInfo.CreateSubdirectory("Excel");
                    }
                    
                    // скачиваются документы только тех лотов, которые состоялись
                    // те которые не состоялись - пустые болванки, их скачивать не надо
                    if (!tds[5].InnerText.ToLower().Contains("отмен") && !tds[5].InnerText.ToLower().Contains("состоялся"))
                    {
                        string[] subDirectory = tds[0].InnerText.Split("/");
                        dirResultsInfo.CreateSubdirectory($"{subDirectory[0]}");
                        string stringLink = $"https://info.ccx.kz{@link.Attributes[0].Value}";
                        string linkName = link.InnerText;
                        
                        if (link.InnerText.Contains(".xls") && link.InnerText.Contains("Приложение"))
                        {
                            client.DownloadFile($"{stringLink}", @$"C:\Users\user\Desktop\files\{linkName}"); //Саня Т. 
                            // client.DownloadFile($"{stringLink}", @$"E:\csharp\ESDP\Download Files\Excel\{linkName}"); //Саня Ф.
                            // client.DownloadFile($"{stringLink}", @$"{dirResultsInfo}\Excel\{linkName}"); //Гульжан
                        }
                        client.DownloadFile($"{stringLink}", $@"E:\csharp\ESDP\Download Files\{subDirectory[0]}\{linkName}"); //Саня Ф.
                        //client.DownloadFile($"{stringLink}", @$"C:\Users\user\Desktop\files\{subDirectory[0]}\{linkName}"); //Саня Т.
                        //client.DownloadFile($"{stringLink}", @$"{dirResultsInfo}{subDirectory[0]}\{linkName}"); //Гульжан
                        stringLinks.Add(stringLink);
                        linkNames.Add(linkName);
                    }
                }
                
                // занесение объекта в таблицу AuctionResults, если номера лотов имеются в бд и результат ранее не заносился
                if (_db.Cards.Any(c=>c.Number == tds[0].InnerText) && !_db.AuctionResults.Any(r=>r.Number == tds[0].InnerText))
                {
                    AuctionResult result = new AuctionResult()
                    {
                        Number = tds[0].InnerText,
                        Name = tds[1].InnerText,
                        DateOfAuctionStart = Convert.ToDateTime(tds[2].InnerText),
                        DateOfSignContract = Convert.ToDateTime(tds[3]?.InnerText),
                        Links = stringLinks,
                        LinkNames = linkNames,
                        Winner = tds[5].InnerText,
                        Sum = Convert.ToDecimal(tds[6]?.InnerText)
                    };
                    
                    _db.AuctionResults.Add(result);
                    _db.SaveChanges();
                }
            }
            
        }
    }
}