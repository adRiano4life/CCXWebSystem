﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
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
            emailMessage.From.Add(new MailboxAddress($"{user.Name} {user.Surname}", "test@rdprom.kz"));
            foreach (var supplier in suppliers)
            {
                emailMessage.Bcc.Add(new MailboxAddress("", $"{supplier.Email}"));
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
                                 $"</ul><br><hr>";
                builder.HtmlBody += positionTable;
            }
            foreach (var path in paths) 
            {
                builder.Attachments.Add(path);
            }
            emailMessage.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.mail.ru", 25, false);
            await client.AuthenticateAsync("test@rdprom.kz", "QWEqwe123");
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }

        public bool SendEmailAfterRegister(string email, string link)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("админ CCXWebSystem", "test@rdprom.kz"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = "Подтвердите свой email";
            
            emailMessage.Body = new TextPart {Text = $@"Пройдите по ссылке {link} чтобы подтвердить свой email"};

            try
            {
                // using var client = new SmtpClient();
                // client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                // client.ConnectAsync("smtp.mail.ru", 25, SecureSocketOptions.Auto);
                // client.AuthenticateAsync("test@rdprom.kz", "QWEqwe123");
                // client.SendAsync(emailMessage);
                // client.DisconnectAsync(true);
                TryingSendMessage(emailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }


        private async Task TryingSendMessage(MimeMessage emailMessage)
        {
            using var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync("smtp.mail.ru", 25, SecureSocketOptions.Auto);
            await client.AuthenticateAsync("test@rdprom.kz", "QWEqwe123");
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);   
        }
        
    }
}