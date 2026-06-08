using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.EmailReply
{
    public interface IEmailRepliesServices
    {
        #region UpdateIssue
        void UpdateEmailReplies(EmailReplies entity);
        #endregion

        #region InsertIssue
        void InsertEmailReplies(EmailReplies entity);
        #endregion

        #region Get Twilio Email Replies
        Task<EmailReplies> GetTwilioEmailReplies(string TwilioEmailId);
        #endregion
    }
}
