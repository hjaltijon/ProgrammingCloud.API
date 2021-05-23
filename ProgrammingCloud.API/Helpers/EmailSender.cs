using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using ProgrammingCloud.API.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Helpers
{
    public class EmailSender : IDisposable
    {
        private readonly SmtpClient _smtpClient;
        private readonly MailAddress _fromAddress;
        private readonly string _fromPassword;

        private readonly IHostingEnvironment _env;

        private readonly AppSettings _settings;

        public EmailSender(IHostingEnvironment env, IOptions<AppSettings> settings)
        {
            _env = env;
            _settings = settings.Value;

            //TODO: use appsettings.json for config values.
            _fromAddress = new MailAddress("noreply.dev.pcloud@gmail.com", "pcloud");
            _fromPassword = _settings.EmailPassword;
            _smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_fromAddress.Address, _fromPassword)
            };
        }

        public void Dispose()
        {
            _smtpClient.Dispose();
        }

        public async Task SendVerifyEmailEmail(string email, int userId, string verifyEmailToken)
        {
            string toEmailAddress = email;
            string toName = "unknown";
            var toAddress = new MailAddress(toEmailAddress, toName);
            using (var message = new MailMessage(_fromAddress, toAddress))
            {
                string activationLink = "";
                if (_env.IsDevelopment())
                {
                    activationLink = $"http://localhost:4200/login/activate-account?email={email}&verifyEmailToken={verifyEmailToken}";
                }
                else
                {
                    activationLink = $"https://programmingcloudapi.azurewebsites.net/login/activate-account?email={email}&verifyEmailToken={verifyEmailToken}";
                }
                message.Subject = "pcloud - activate your account";
                message.Body =
                $@"
                A new user account was requested for your email address. <br>
                If you don't think this email was meant for you then please delete it. <br>
                Otherwise please click on the following link to activate your account: <br>
                <a href='{activationLink}'>{activationLink}</a> <br><br>
                Thanks, <br>
                -- The pcloud team";
                message.IsBodyHtml = true;
                await _smtpClient.SendMailAsync(message);
            }
        }

    }
}
