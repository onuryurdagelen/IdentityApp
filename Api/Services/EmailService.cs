using Api.DTOs.Account;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Api.Services
{
    public class EmailService
    {
        private readonly IConfiguration config;

        public EmailService(IConfiguration config)
        {
            this.config = config;
        }

        public async Task<bool> SendEmailAsync(EmailSendDto model)
        {
            MailjetClient client = new MailjetClient(config["MailJet:ApiKey"], config["MailJet:SecretKey"]);

            var email = new TransactionalEmailBuilder()
                .WithFrom(new SendContact(config["Email:From"], config["Email:ApplicationName"]))
                .WithSubject(model.Subject)
                .WithHtmlPart(model.Body)
                .WithTo(new SendContact(model.To))
                .Build();

            var response = await client.SendTransactionalEmailAsync(email);
            if(response.Messages != null)
            {
                if (response.Messages[0].Status =="success")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
