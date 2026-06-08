using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Website_Demo
{
    public class Website_DemosService : IWebsite_DemosService
    {
        #region Define Services
        private readonly IRepository<Website_Demos> _website_DemosRepository;
        #endregion

        #region Services Initializations
        public Website_DemosService(
            IRepository<Website_Demos> website_DemosRepository)
        {
            _website_DemosRepository = website_DemosRepository;
        }
        #endregion

        #region GetWebsiteDemoById
        // Title: GetWebsiteDemoById
        // Description: This method retrieves a Website_Demos from the database by its unique identifier (`id`). 
        public async Task<Website_Demos> GetWebsiteDemoById(string id)
        {
            return await _website_DemosRepository.TableNoTracking
                               .Include(x => x.BusinessSize)
                               .Include(x => x.Website_Demo_Modules)
                               .ThenInclude(m => m.Modules)
                               .Where(x => !x.Deleted && x.Id == id)
                               .FirstOrDefaultAsync();
        }
        #endregion

        #region GetWebsiteDemoByEmail
        // Title: GetWebsiteDemoByEmail
        // Description: This method retrieves a Website_Demos based on emailAddress.The method returns the first matching WebsiteDemo or null if no match is found.
        public async Task<Website_Demos> GetWebsiteDemoByEmail(string emailAddress)
        {
            return  await _website_DemosRepository.TableNoTracking.Where(x => !x.Deleted && x.EmailAddress == emailAddress).FirstOrDefaultAsync();
        }
        #endregion

        #region InsertWebsite_Demo
        // Title: InsertWebsite_Demo
        // Description: This method inserts a new Website_Demos entity into the repository. It takes a Website_Demos object as input and uses the  _website_DemosRepository to handle the insertion operation.
        public void InsertWebsite_Demo(Website_Demos entity)
        {
            _website_DemosRepository.Insert(entity);
        }
        #endregion
    }
}
