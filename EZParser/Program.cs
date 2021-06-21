﻿using System;
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
        public static string DefaultConnection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=123"; // бд Гульжан, Саня Т.
        //public static string DefaultConnection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=QWEqwe123@"; // бд Саня Ф.

        public static string PathToFiles = @$"D:\csharp\esdp\app\WebStudio\wwwroot\Files"; // Гульжан
        //public static string PathToFiles = @$"C:\Users\user\Desktop\files"; // Саня Т.
        //public static string PathToFiles = @$"E:\csharp\ESDP\Download Files"; // Саня Ф.

        static void Main(string[] args)
        {
            GetParse();
            ExcelReader.ExcelRead();
            AuctionResultsParser.GetAuctionResults();
        }
        
        public static void GetParse()
        {
            var optionsBuilder = new DbContextOptionsBuilder<WebStudioContext>();
            var options = optionsBuilder.UseNpgsql(DefaultConnection).Options;

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
                    DirectoryInfo dirInfo = new DirectoryInfo(PathToFiles); // общий путь
                    
                    string[] subDirectory = tds[0].InnerText.Split("/");
                    dirInfo.CreateSubdirectory($"{subDirectory[0]}");
                    string stringLink = $"https://info.ccx.kz{@link.Attributes[0].Value}";
                    string linkName = link.InnerText;
                    if (link.InnerText.Contains(".xlsx") && link.InnerText.Contains("Приложение"))
                    {
                        foreach (var dir in dirInfo.GetDirectories())
                        {
                            if (!Directory.Exists("Excel"))
                                dirInfo.CreateSubdirectory("Excel");
                        }
                        client.DownloadFile($"{stringLink}", @$"{dirInfo}\Excel\{linkName}"); // общий путь
                    }
                    
                    client.DownloadFile($"{stringLink}", @$"{dirInfo}\{subDirectory[0]}\{linkName}");  // общий путь

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