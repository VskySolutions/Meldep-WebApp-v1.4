using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.EmailReply
{
    public interface IHelpDeskEmailRepliesMappingService
    {
        #region GetAllHelpDeskEmailRepliesMappingList
        Task<List<HelpDeskEmailRepliesMapping>> GetAllHelpDeskEmailRepliesMappingList(string siteId, string helpDeskId, int skipIndex = 0, int takeCount = 10, bool isSystemEmail = false);
        #endregion

        #region InsertHelpDeskEmailRepliesMapping
        void InsertHelpDeskEmailRepliesMapping(HelpDeskEmailRepliesMapping entity);
        #endregion

        #region UpdateHelpDeskEmailRepliesMapping
        void UpdateHelpDeskEmailRepliesMapping(HelpDeskEmailRepliesMapping entity);
        #endregion
    }
}
