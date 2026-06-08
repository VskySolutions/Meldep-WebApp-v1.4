using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Models.Expens;

namespace Vsky.Services.Expences
{
    public class ExpensePurchaseRequestFilesService : IExpensePurchaseRequestFilesService
    {
        #region Define Services
        private readonly IRepository<ExpensePurchaseRequestFiles> _expensePurchaseRequestFilesRepository;
        #endregion

        #region Services Initializations

        public ExpensePurchaseRequestFilesService(IRepository<ExpensePurchaseRequestFiles> expensePurchaseRequestFilesRepository)
        {
            _expensePurchaseRequestFilesRepository = expensePurchaseRequestFilesRepository;
        }

        #endregion
       
        #region GetExpensePurchaseRequestFileById
        // Title: GetExpensePurchaseRequestFileById
        // Description: This method retrieves a ExpensePurchaseRequestFiles from the database by its unique identifier (`id`). 
        public async Task<ExpensePurchaseRequestFiles> GetExpensePurchaseRequestFileById(string id)
        {
            var query = _expensePurchaseRequestFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetExpensePurchaseRequestFileByFileId
        // Title: GetExpensePurchaseRequestFileByFileId
        // Description: This method retrieves a ExpensePurchaseRequestFiles from the database by fileId. 
        public async Task<ExpensePurchaseRequestFiles> GetExpensePurchaseRequestFileByFileId(string fileId)
        {
            var query = _expensePurchaseRequestFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.FileId == fileId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllExpensePurchaseRequestFileByExpensePurchaseRequestId
        // Title: GetAllExpensePurchaseRequestFileByExpensePurchaseRequestId
        // Description:  This method retrieves all files linked to a specific expense purchase request for a given site.
        public async Task<List<ExpensePurchaseRequestFiles>> GetAllExpensePurchaseRequestFileByExpensePurchaseRequestId(string siteId, string expensePurchaseRequestId)
        {
            var query = _expensePurchaseRequestFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.ExpensePurchaseRequestId == expensePurchaseRequestId && x.Expense_Purchase_Requests.SiteId == siteId);
            query = query.Select(x => new ExpensePurchaseRequestFiles
            {
                Id = x.Id,
                FileId = x.FileId,
                ExpensePurchaseRequestId = x.ExpensePurchaseRequestId,
                File = new Picture
                {
                    Id = x.File.Id,
                    VirtualPath = x.File.VirtualPath,
                    MimeType = x.File.MimeType,
                    SeoFilename = x.File.SeoFilename
                }
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region InsertExpensePurchaseRequestFile
        // Title: InsertExpensePurchaseRequestFile
        // Description: This method inserts a new ExpensePurchaseRequestFiles entity into the repository. It takes a ExpensePurchaseRequestFiles object as input and uses the _expensePurchaseRequestFilesRepository to handle the insertion operation.
        public void InsertExpensePurchaseRequestFile(ExpensePurchaseRequestFiles entity)
        {
            _expensePurchaseRequestFilesRepository.Insert(entity);
        }
        #endregion

        #region UpdateExpensePurchaseRequestFile
        // Title: UpdateExpensePurchaseRequestFile
        // Description: This method updates the specified ExpensePurchaseRequestFiles entity in the repository. It takes a ExpensePurchaseRequestFiles object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateExpensePurchaseRequestFile(ExpensePurchaseRequestFiles entity)
        {
            _expensePurchaseRequestFilesRepository.Update(entity);
        }
        #endregion

        #region DeleteExpensePurchaseRequestFile
        // Title: DeleteExpensePurchaseRequestFile
        // Description: Marks the specified ExpensePurchaseRequestFiles entity as deleted by setting its `Deleted` property to true. 
        public void DeleteExpensePurchaseRequestFile(ExpensePurchaseRequestFiles entity)
        {
            entity.Deleted = true;
            _expensePurchaseRequestFilesRepository.Update(entity);
        }
        #endregion
    }
}
