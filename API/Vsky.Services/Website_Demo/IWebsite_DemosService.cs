using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Website_Demo
{
    public interface IWebsite_DemosService
    {
        #region GetDemoByEmail
        Task<Website_Demos> GetWebsiteDemoByEmail(string emailAddress);
        #endregion

        #region GetDemoById
        Task<Website_Demos> GetWebsiteDemoById(string id);
        #endregion

        #region InsertWebsite_Demo
        void InsertWebsite_Demo(Website_Demos entity);
        #endregion

    }
}
