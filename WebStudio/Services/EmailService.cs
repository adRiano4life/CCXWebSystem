using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using WebStudio.Helpers;
using WebStudio.Models;

namespace WebStudio.Services
{
    public class EmailService
    {
        public string _emailOffice = AppCredentials.EmailName;
        public string _adminEmailOffice = AppCredentials.AdminEmailName;
        public string _passwordOffice = AppCredentials.EmailPassword;

        public async Task SendMessageAsync(List<SearchSupplier> suppliers, string title, string message, List<string> paths, User user, Card card)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress($"{user.Name} {user.Surname}", _emailOffice));
            foreach (var supplier in suppliers)
            {
                emailMessage.Bcc.Add(new MailboxAddress("", $"{supplier.Email}"));
                //emailMessage.Bcc.Add(MailboxAddress.Parse(supplier.Email));
            }

            emailMessage.Subject = title;

            var builder = new BodyBuilder();
            builder.HtmlBody = message;
            if (card != null)
            {
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
            }
            
            string signature = "<br>С уважением, " +
                               "<br>ТОО RD PROM " +
                               "<br>Республика Казахстан, г. Караганда. " +
                               "<br>Контактные данные: " +
                               "<br>Моб. тел.: +7 775 992 54 05" +
                               "<br>Email: office@rdprom.kz";

            builder.HtmlBody += signature;
            
            foreach (var path in paths) 
            {
                builder.Attachments.Add(path);
            }
            emailMessage.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync("smtp.mail.ru", 25, false);
            await client.AuthenticateAsync(_emailOffice, _passwordOffice);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }

        public async Task SendEmailAfterRegister(string email, string link)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Администрация сайта CCXWebSystem", _adminEmailOffice));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = "Подтвердите свой email";

            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = "Пройдите по <a href=\"" + link + "\"> данной ссылке </a> чтобы подтвердить свой email"
            };

            using var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync("smtp.mail.ru", 25, SecureSocketOptions.Auto);
            await client.AuthenticateAsync(_adminEmailOffice, _passwordOffice);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }


        public async Task SendEmailForResetPassword(string email, string title, string link)
        {
            var emailMessage = new MimeMessage();
            
            emailMessage.From.Add(new MailboxAddress("Администрация сайта CCXWebSystem", _adminEmailOffice));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = title;

            emailMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = $"Для сброса пароля пройдите по <a href= '{link}'>данной ссылке</a>.<br><br>" +
                       $"Если вы не отправляли запрос на изменение пароля в системе, проигнорируйте данное письмо"
            };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.mail.ru", 25, false);
            await client.AuthenticateAsync(_adminEmailOffice, _passwordOffice);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
     
    }
}