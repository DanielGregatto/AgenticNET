using Domain.Configs;
using Identity.Model;
using Microsoft.Extensions.Options;
using Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Services.Infrastructure
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfig _emailConfig;
        private SmtpClient _smtpClient;
        private List<string> _destinatarios;

        public EmailService(IOptions<EmailConfig> emailConfig)
        {
            _emailConfig = emailConfig.Value;
            _destinatarios = new List<string>();
            this.BuildSmtpClient();
        }

        public void CleanToList()
        {
            _destinatarios = new List<string>();
        }

        public void AddTo(string email)
        {
            _destinatarios.Add(email);
        }

        public void SendHtml(string Assunto, string HTML)
        {
            MailMessage msg = BuildMailMesage();

            foreach (var dest in _destinatarios)
                msg.To.Add(new MailAddress(dest));

            msg.Subject = Assunto;
            msg.Body = HTML;
            msg.IsBodyHtml = true;
            msg.Priority = MailPriority.High;

            _smtpClient.Send(msg);

            msg.Dispose();
        }

        public async Task SendHtmlAsync(string Assunto, string HTML)
        {
            MailMessage msg = BuildMailMesage();

            foreach (var dest in _destinatarios)
                msg.To.Add(new MailAddress(dest));

            msg.Subject = Assunto;
            msg.Body = HTML;
            msg.IsBodyHtml = true;
            msg.Priority = MailPriority.High;

            await _smtpClient.SendMailAsync(msg);

            msg.Dispose();
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            CleanToList();

            AddTo(email);

            await SendHtmlAsync(subject, htmlMessage);
        }

        public async Task SendCredentialsThirdPartyRegister(string email, string password)
        {
            string subject = "Credenciais de acesso";

            var values = new Dictionary<string, string>
            {
                { "Email", email },
                { "Password", password },
                { "MainUrl", _emailConfig.MainUrl },
                { "MainUrlHelp", _emailConfig.MainUrlHelp },
            };

            string htmlContent = await LoadEmailTemplateAsync("AuthCredentialsThirdPartyRegister", values);

            CleanToList();

            AddTo(email);

            await SendHtmlAsync(subject, htmlContent);
        }

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            string subject = "Confirme seu e-mail";
            string htmlMessage = $"Por favor, confirme sua conta clicando <a href='{confirmationLink}'>aqui</a>.";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            string subject = "Redefinir senha";
            string htmlMessage = $"Para redefinir sua senha, clique <a href='{resetLink}'>aqui</a>.";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            string subject = "Código de redefinição de senha";
            string htmlMessage = $"Seu código de redefinição de senha é: {resetCode}";

            await SendEmailAsync(email, subject, htmlMessage);
        }


        #region HELPERS
        private void BuildSmtpClient()
        {
            var smtpClient = new SmtpClient();

            smtpClient.Host = _emailConfig.Host;
            smtpClient.Port = _emailConfig.SmtpPort;
            smtpClient.EnableSsl = _emailConfig.UseSSL;
            smtpClient.Credentials = new NetworkCredential(_emailConfig.Email, _emailConfig.Password);

            _smtpClient = smtpClient;
        }

        private MailMessage BuildMailMesage()
        {
            MailMessage msg = new MailMessage();

            msg.Sender = new MailAddress(_emailConfig.Email, _emailConfig.Name);
            msg.From = new MailAddress(_emailConfig.Email, _emailConfig.Name);

            return msg;
        }

        private async Task<string> LoadEmailTemplateAsync(string templateName, Dictionary<string, string> values)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates");
            var templatePath = Path.Combine(basePath, $"{templateName}.html");

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Email template not found: {templatePath}");

            string templateContent = await File.ReadAllTextAsync(templatePath);

            // Replace placeholders with actual values
            foreach (var key in values)
            {
                templateContent = templateContent.Replace($"{{{{{key.Key}}}}}", key.Value);
            }

            return templateContent;
        }
        #endregion
    }
}