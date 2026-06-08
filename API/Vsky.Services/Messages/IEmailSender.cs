using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.Messages
{
    public interface IEmailSender
    {
        Task SendEmailAsync(EmailAccount emailAccount, string subject, string body,
            string fromAddress, string fromName, string toAddress, string toName,
             string replyToAddress = null, string replyToName = null,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null,
            string attachmentFilePath = null, string attachmentFileName = null,
            IDictionary<string, string> headers = null);

        #region  Twilio Send EmailsAsync
        /// <summary>
        /// Twilio Send EmailsAsync
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="fromName"></param>
        /// <param name="toAddress"></param>
        /// <param name="toName"></param>
        /// <param name="replyTo"></param>
        /// <param name="replyToName"></param>
        /// <param name="bcc"></param>
        /// <param name="cc"></param>
        /// <param name="attachmentFilePath"></param>
        /// <param name="attachmentFileName"></param>
        /// <param name="headers"></param>
        /// <param name="token"></param>
        /// <param name="trackingId"></param>
        /// <returns></returns>
        Task<string> TwilioSendEmailsAsync(string subject,
                                            string body,
                                            string toAddress,
                                            string toName,
                                            string redirectionUrl,
                                            string replyTo = null,
                                            string replyToName = null,
                                            IEnumerable<string> bcc = null,
                                            IEnumerable<string> cc = null,
                                            string attachmentFilePath = null,
                                            string attachmentFileName = null,
                                            IDictionary<string, string> headers = null,
                                            string token = null,
                                            string siteId = null);
        #endregion
    }
}