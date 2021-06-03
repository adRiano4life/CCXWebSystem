using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using WebStudio.Models;

namespace EZParser
{
    public class Program
    {
        static void Main(string[] args)
        {
            GetParse();
            ExcelReader.ExcelRead();
        }
        
        public static void GetParse()
        {
            // string connection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=123"; // бд Гульжан
            string connection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=123"; // бд Саня Ф.
            
            var optionsBuilder = new DbContextOptionsBuilder<WebStudioContext>();
            var options = optionsBuilder.UseNpgsql(connection).Options;

            using WebStudioContext _db = new WebStudioContext(options);
            
            var url = @"https://info.ccx.kz/ru/announcement?per-page=100";
            
            HtmlWeb web = new HtmlWeb();

            var docAllPosition = web.Load(url);

            var collectionPosition = docAllPosition.DocumentNode.SelectNodes("//td[contains(text(),'Казахмыс')]/..");

            var doc = new HtmlDocument();
            
            WebClient client = new WebClient();
            
            foreach (var position in collectionPosition)
            {
                doc.LoadHtml(position.InnerHtml);
                
                var tds = doc.DocumentNode.SelectNodes("//td");
                var links = doc.DocumentNode.SelectNodes("//a");
                List<string> stringLinks = new List<string>();
                List<string> linkNames = new List<string>();
                foreach (var link in links)
                {
                    string stringLink = $"https://info.ccx.kz{@link.Attributes[0].Value}";
                    string linkName = link.InnerText;
                    if (link.InnerText.Contains(".xlsx"))
                    {
                        // client.DownloadFile($"{stringLink}", @$"C:\Users\user\Desktop\files\{linkName}"); //Саня Т. 
                        client.DownloadFile($"{stringLink}", @$"E:\csharp\ESDP\Download Files\{linkName}"); //Саня Ф.
                        // client.DownloadFile($"{stringLink}", @$"D:\csharp\esdp\1itera\app\WebStudio\wwwroot\Files\Excel\{linkName}"); //Гульжан  
                    }
                    stringLinks.Add(stringLink);
                    linkNames.Add(linkName);
                }

                Card card = new Card
                {
                    Number = tds[0].InnerText,
                    Name = tds[1].InnerText,
                    StartSumm = Convert.ToDecimal(tds[2].InnerText),
                    DateOfAcceptingEnd = Convert.ToDateTime(tds[3].InnerText),
                    DateOfAuctionStart = Convert.ToDateTime(tds[4].InnerText),
                    Initiator = tds[5].InnerText,
                    Broker = tds[6].InnerText,
                    Auction = tds[7].InnerText,
                    State = tds[9].InnerText,
                    BestPrice = tds[10].InnerText,
                    Links = stringLinks,
                    LinkNames = linkNames,
                };

                if (!_db.Cards.Any(c=>c.Number == tds[0].InnerText))
                {
                    _db.Cards.Add(card);
                    _db.SaveChanges();
                }
            }
        }
    }
}