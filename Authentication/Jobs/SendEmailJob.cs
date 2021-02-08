using FluentEmail.Core;

using System.IO;
using System.Threading.Tasks;

namespace Authentication.Jobs
{
    public class SendEmailJob
    {
        #region Deps

        public IFluentEmail _emailSender;

        #endregion

        #region Constructors

        public SendEmailJob(IFluentEmail emailSender)
        {
            _emailSender = emailSender;
        }

        #endregion

        public async Task SendConfirmationLinkEmail(string address, string fullname, string emailConfirmationLink)
        {
            var emailConfirmationTemplatePath = Path.Combine(
                Directory.GetCurrentDirectory(), "_Templates/Email/EmailConfirmation.html"
            );

            var emailTemplate = File.ReadAllText(emailConfirmationTemplatePath);
            var email = emailTemplate
                .Replace("[%fullname%]", fullname)
                .Replace("[%url%]", emailConfirmationLink);

            await _emailSender
                .To(address)
                .Subject("Confirm your account email.")
                .Body(email, true)
                .SendAsync();
        }

        public async Task SendResetPasswordLinkEmail(string address, string resetPasswordLink)
        {
            var emailConfirmationTemplatePath = Path.Combine(
                Directory.GetCurrentDirectory(), "_Templates/Email/EmailPasswordReset.html"
            );

            var emailTemplate = File.ReadAllText(emailConfirmationTemplatePath);
            var email = emailTemplate
                .Replace("[%url%]", resetPasswordLink);

            await _emailSender
                .To(address)
                .Subject("Reset your account password.")
                .Body(email, true)
                .SendAsync();
        }
    }
}
