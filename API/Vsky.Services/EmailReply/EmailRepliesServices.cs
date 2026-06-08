using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.EmailReply
{
    public class EmailRepliesServices: IEmailRepliesServices
    {
        #region Define Services
        private readonly IRepository<EmailReplies> _emailReplies;
        #endregion

        #region Services Initializations
        public EmailRepliesServices(IRepository<EmailReplies> emailReplies)
        {
            _emailReplies = emailReplies;
        }
        #endregion

        #region Insert EmailReplies
        // Title: EmailReplies
        // Description: This method inserts a new EmailReplies entity into the repository. It takes a Issue object as input and uses the _emailReplies to handle the insertion operation.
        public void InsertEmailReplies(EmailReplies entity)
        {
            _emailReplies.Insert(entity);
        }
        #endregion

        #region Update EmailReplies
        // Title: EmailReplies
        // Description: This method updates the specified Issue entity in the repository. It takes a Issue object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateEmailReplies(EmailReplies entity)
        {
            _emailReplies.Update(entity);
        }
        #endregion

        #region Get Twilio Email Replies Data
        /// <summary>
        /// Get twilio reply data from TwilioEmailId
        /// </summary>
        /// <param name="TwilioEmailId"></param>
        /// <returns></returns>
        public async Task<EmailReplies> GetTwilioEmailReplies(string TwilioEmailId)
        {
            var query = _emailReplies.Table.Where(x => x.TwilioEmailId == TwilioEmailId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion
    }
}
