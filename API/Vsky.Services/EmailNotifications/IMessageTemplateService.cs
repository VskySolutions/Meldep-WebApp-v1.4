using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.EmailNotifications
{
    public interface IMessageTemplateService
    {
        #region GetAllMessageTemplates
        IPagedList<MessageTemplate> GetAllMessageTemplates(
            string siteId,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );

        Task<List<MessageTemplate>> GetAllMasterMessageTemplates();
        #endregion
    }
}
