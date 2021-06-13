using System.Collections.Generic;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MimeKit;
using WebStudio.Models;

namespace WebStudio.Services
{
    public class EmailService
    {
        public async Task SendMessageAsync(List<SearchSupplier> suppliers, string title, string message, List<string> paths, User user, Card card)
        {
            var emailMessage = new MimeMessage();
            //emailMessage.From.Add(new MailboxAddress($"{user.Name} {user.Surname}", $"{user.Email}"));
            emailMessage.From.Add(new MailboxAddress($"{user.Name} {user.Surname}", "folomeshkin_instagram@mail.ru"));
            foreach (var supplier in suppliers)
            {
                emailMessage.To.Add(new MailboxAddress("", $"{supplier.Email}"));
            }

            emailMessage.Subject = title;

            var builder = new BodyBuilder();
            builder.HtmlBody = message;
            foreach (var position in card.Positions)
            {
                string positionTable = $"<br><ul>" +
                                 $"<li><b>Код ТНВЕД:</b> {@position.CodTNVED}</li>" +
                                 $"<li><b>Наименование:</b> {@position.Name}</li>" +
                                 $"<li><b>Единица измерения:</b> {@position.Measure}</li>" +
                                 $"<li><b>Количество:</b> {@position.Amount}</li>" +
                                 $"<li><b>Условия поставки:</b> {@position.DeliveryTerms}</li>" +
                                 $"</ul>";
                builder.HtmlBody += positionTable;
            }
            foreach (var path in paths) 
            {
                builder.Attachments.Add(path);
            }
            emailMessage.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.mail.ru", 25, false);
            await client.AuthenticateAsync("folomeshkin_instagram@mail.ru", "InstagramProject");
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}