using GreenSpace.Application.Common.Settings;
using GreenSpace.Application.DTOs.Auth;
using GreenSpace.Application.Interfaces.Security;
using GreenSpace.Domain.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;


namespace GreenSpace.Infrastructure.ExternalServices
{
    public class EmailSender : IEmailSender
    {
        private readonly MailSettings _mailSettings;

        public EmailSender(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }


        public async Task SendEmailAsync(SendEmailDto request)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;
            var builder = new BodyBuilder();
            builder.HtmlBody = request.Body;

            // Xử lý đính kèm file
            if (request.Attachments != null)
            {
                byte[] fileBytes;
                foreach (var file in request.Attachments)
                {
                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            fileBytes = ms.ToArray();
                        }
                        builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                // Connect
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);

                // Authenticate (Login)
                await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);

                // Send
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi gửi mail: {ex.Message}");
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }

    }
}