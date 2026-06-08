using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Expences
{
    public class ExpenseAdvanceRequestFilesService : IExpenseAdvanceRequestFilesService
    {
        #region Define Services
        private readonly IRepository<ExpenseAdvanceRequestFiles> _expenseAdvanceRequestFilesRepository;
        #endregion

        #region Services Initializations

        public ExpenseAdvanceRequestFilesService(IRepository<ExpenseAdvanceRequestFiles> expenseAdvanceRequestFilesRepository)
        {
            _expenseAdvanceRequestFilesRepository = expenseAdvanceRequestFilesRepository;
        }

        #endregion

        #region GetExpenseAdvanceRequestFileById
        // Title: GetExpenseAdvanceRequestFileById
        // Description: This method retrieves a ExpenseAdvanceRequestFiles from the database by its unique identifier (`id`). 
        public async Task<ExpenseAdvanceRequestFiles> GetExpenseAdvanceRequestFileById(string id)
        {
            var query = _expenseAdvanceRequestFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetExpenseAdvanceRequestFileByFileId
        // Title: GetExpenseAdvanceRequestFileByFileId
        // Description: This method retrieves a ExpenseAdvanceRequestFiles from the database by fileId. 
        public async Task<ExpenseAdvanceRequestFiles> GetExpenseAdvanceRequestFileByFileId(string fileId)
        {
            var query = _expenseAdvanceRequestFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.FileId == fileId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllExpenseAdvanceRequestFileByExpenseAdvanceRequestId
        // Title: GetAllExpenseAdvanceRequestFileByExpenseAdvanceRequestId
        // Description:  This method retrieves all files linked to a specific expense advance request for a given site.
        public async Task<List<ExpenseAdvanceRequestFiles>> GetAllExpenseAdvanceRequestFileByExpenseAdvanceRequestId(string siteId, string expenseAdvanceRequestId)
        {
            var query = _expenseAdvanceRequestFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.ExpenseAdvanceRequestId == expenseAdvanceRequestId && x.Expense_Advance_Requests.SiteId == siteId);
            query = query.Select(x => new ExpenseAdvanceRequestFiles
            {
                Id = x.Id,
                FileId = x.FileId,
                ExpenseAdvanceRequestId = x.ExpenseAdvanceRequestId,
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

        #region InsertExpenseAdvanceRequestFile
        // Title: InsertExpenseAdvanceRequestFile
        // Description: This method inserts a new ExpenseAdvanceRequestFiles entity into the repository. It takes a ExpenseAdvanceRequestFiles object as input and uses the _expenseAdvanceRequestFilesRepository to handle the insertion operation.
        public void InsertExpenseAdvanceRequestFile(ExpenseAdvanceRequestFiles entity)
        {
            _expenseAdvanceRequestFilesRepository.Insert(entity);
        }
        #endregion

        #region UpdateExpenseAdvanceRequestFile
        // Title: UpdateExpenseAdvanceRequestFile
        // Description: This method updates the specified ExpenseAdvanceRequestFiles entity in the repository. It takes a ExpenseAdvanceRequestFiles object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateExpenseAdvanceRequestFile(ExpenseAdvanceRequestFiles entity)
        {
            _expenseAdvanceRequestFilesRepository.Update(entity);
        }
        #endregion

        #region DeleteExpenseAdvanceRequestFile
        // Title: DeleteExpenseAdvanceRequestFile
        // Description: Marks the specified ExpenseAdvanceRequestFiles entity as deleted by setting its `Deleted` property to true. 
        public void DeleteExpenseAdvanceRequestFile(ExpenseAdvanceRequestFiles entity)
        {
            entity.Deleted = true;
            _expenseAdvanceRequestFilesRepository.Update(entity);
        }
        #endregion
    }
}
