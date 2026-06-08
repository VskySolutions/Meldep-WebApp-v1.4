using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Website_Demo_Module
{
    public class Website_Demo_ModulesService : IWebsite_Demo_ModulesService
    {
        #region Define Services
        private readonly IRepository<Website_Demo_Modules> _website_Demo_ModulesRepository;
        #endregion

        #region Services Initializations
        public Website_Demo_ModulesService(
           IRepository<Website_Demo_Modules> website_Demo_ModulesRepository)
        {
            _website_Demo_ModulesRepository = website_Demo_ModulesRepository;
        }
        #endregion
        #region InsertWebsite_Demo_Modules
        // Title: InsertWebsite_Demo_Modules
        // Description: This method inserts a Website_Demo_Modules entity into the repository. It takes a Website_Demo_Modules object as input and uses the _website_Demo_ModulesRepository to handle the insertion operation.
        public void InsertWebsite_Demo_Modules(Website_Demo_Modules entity)
        {
            _website_Demo_ModulesRepository.Insert(entity);
        }
        #endregion
    }
}
