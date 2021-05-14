using System;
using HtmlAgilityPack;

namespace EZParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var html = @"https://info.ccx.kz/ru/announcement?per-page=100";
            
            HtmlWeb web = new HtmlWeb();

            var htmlDoc = web.Load(html);

            var nodes = htmlDoc.DocumentNode.SelectNodes("//td[text(), 'Казахмыс']");
            

            foreach (var node in nodes)
            {
                Console.WriteLine(node.OuterHtml);
            }
        }
    }
}