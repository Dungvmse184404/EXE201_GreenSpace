using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.Common.Mail
{
    public interface IEmailTemplateHelper
    {
        Task<string> GetResetPasswordContentAsync(string recipientName, string otp, int expiry);
        Task<string> GetAccountActivationContentAsync(string recipientName, string otp, int expiry);
    }
}
