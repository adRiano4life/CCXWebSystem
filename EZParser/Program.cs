using System;
using System.Collections.Generic;
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
        }
        public static void GetParse()
        {
            string connection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=QWEqwe123@";
            var optionsBuilder = new DbContextOptionsBuilder<WebStudioContext>();
            var options = optionsBuilder.UseNpgsql(connection).Options;

            using WebStudioContext _db = new WebStudioContext(options);
            
            var url = @"https://info.ccx.kz/ru/announcement?per-page=100";
            
            HtmlWeb web = new HtmlWeb();

            var docAllPosition = web.Load(url);

            var collectionPosition = docAllPosition.DocumentNode.SelectNodes("//td[contains(text(),'Казахмыс')]/..");

            var doc = new HtmlDocument();
            
            // WebClient client = new WebClient();
            // client.DownloadFile("https://info.ccx.kz/ru/site/download?uid=45D20B99-17F3-4162-859E-752BCB6A21E6", "Приложение 1.docx");

            foreach (var position in collectionPosition)
            {
                doc.LoadHtml(position.InnerHtml);
                
                var tds = doc.DocumentNode.SelectNodes("//td");
                var links = doc.DocumentNode.SelectNodes("//a/..");

                Card card = new Card
                {
                    Number = tds[0].InnerText,
                    Name = tds[1].InnerText,
                    StartSumm = Convert.ToDouble(tds[2].InnerText),
                    DateOfAcceptingEnd = Convert.ToDateTime(tds[3].InnerText),
                    DateOfAuctionStart = Convert.ToDateTime(tds[4].InnerText),
                    Initiator = tds[5].InnerText,
                    Broker = tds[6].InnerText,
                    Auction = tds[7].InnerText,
                    State = tds[9].InnerText,
                    BestPrice = tds[10].InnerText,
                };
               
                _db.Cards.Add(card);
                _db.SaveChanges();
            }
        }
    }
}