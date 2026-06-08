using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.HelpDesks
{
    public class HelpDeskFilesService : IHelpDeskFilesService
    {
        #region Define Services
        private readonly IRepository<HelpDeskFiles> _helpDeskFilesRepository;
        #endregion

        #region Services Initializations

        public HelpDeskFilesService(IRepository<HelpDeskFiles> helpDeskFilesRepository)
        {
            _helpDeskFilesRepository = helpDeskFilesRepository;
        }

        #endregion

        #region Private Methods
        // Title: GetOrderBy
        // Description: This method returns the input string as it is, which can be used as the `ORDER BY` clause in a SQL query.
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetHelpDeskFileByFileId
        // Title: GetHelpDeskFileByFileId
        // Description: This method retrieves a HelpDeskFiles from the database by fileId. 
        public async Task<HelpDeskFiles> GetHelpDeskFileByFileId(string fileId)
        {
            var query = _helpDeskFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.FileId == fileId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertHelpDeskFile
        // Title: InsertHelpDeskFile
        // Description: This method inserts a new HelpDeskFiles entity into the repository. It takes a HelpDeskFiles object as input and uses the _helpDeskFilesRepository to handle the insertion operation.
        public void InsertHelpDeskFile(HelpDeskFiles entity)
        {
            _helpDeskFilesRepository.Insert(entity);
        }
        #endregion

        #region UpdateHelpDeskFile
        // Title: UpdateHelpDeskFile
        // Description: This method updates the specified HelpDeskFiles entity in the repository. It takes a HelpDeskFiles object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateHelpDeskFile(HelpDeskFiles entity)
        {
            _helpDeskFilesRepository.Update(entity);
        }
        #endregion

        #region DeleteHelpDeskFile
        // Title: DeleteHelpDeskFile
        // Description: Marks the specified HelpDeskFiles entity as deleted by setting its `Deleted` property to true. 
        public void DeleteHelpDeskFile(HelpDeskFiles entity)
        {
            entity.Deleted = true;
            _helpDeskFilesRepository.Update(entity);
        }
        #endregion
    }
}



