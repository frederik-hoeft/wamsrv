using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using System;
using System.Diagnostics;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using washared;

namespace wamsrv.Email
{
    public class EmailManager
    {
        public Subject Subject { get; private set; } = Subject.Undefined;
        public MimeMessage Message { get; private set; } = null;
        private EmailManager() { }

        public bool Send()
        {
            if (UnitTestDetector.IsInUnitTest)
            {
                Debug.WriteLine("Simulating EmailManager.Send()");
                return true;
            }
            using SmtpClient client = new SmtpClient
            {
                ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate)
            };
            try
            {
                client.Connect(MainServer.Config.WamsrvEmailConfig.SmptServer, MainServer.Config.WamsrvEmailConfig.SmptServerPort, true);
                client.Authenticate(MainServer.Config.WamsrvEmailConfig.EmailAddress, MainServer.Config.WamsrvEmailConfig.EmailPassword);
                client.Send(Message);
            }
            catch (Exception ex)
            {
                client.Disconnect(false);
                if (ex is MailKit.Security.AuthenticationException)
                {
                    throw;
                }
                return false;
            }
            client.Disconnect(true);
            return true;
        }

        public static EmailManager Create(Subject subject, string recipiant, string name, string securityCode)
        {
            EmailManager emailManager = new EmailManager
            {
                Subject = subject,
                Message = new MimeMessage()
            };
            emailManager.Message.From.Add(new MailboxAddress(MainServer.Config.WamsrvEmailConfig.EmailDisplayName, MainServer.Config.WamsrvEmailConfig.EmailAddress));
            emailManager.Message.To.Add(new MailboxAddress(name, recipiant));
            switch (subject)
            {
                case Subject.CreateAccount:
                    emailManager.Message.Subject = "Confirm your email address.";
                    emailManager.Message.Body = new TextPart(TextFormat.Plain)
                    {
                        Text = "OMG! this is your code:\n" + securityCode
                    };
                    return emailManager;
                default:
                    return emailManager;
            }
        }
        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            Debug.WriteLine("Certificate error: {0}", sslPolicyErrors);
            return false;
        }
    }
    public enum Subject
    {
        Undefined,
        CreateAccount,
        DeleteAccount,
        ChangePassword
    }
}
