using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.HelpDesks
{
    public interface IHelpDeskFilesService
    {
        #region GetHelpDeskFileByFileId
        Task<HelpDeskFiles> GetHelpDeskFileByFileId(string fileId);
        #endregion

        #region InsertHelpDeskFile
        void InsertHelpDeskFile(HelpDeskFiles entity);
        #endregion

        #region DeleteHelpDeskFile
        void DeleteHelpDeskFile(HelpDeskFiles entity);
        #endregion
    }
}


