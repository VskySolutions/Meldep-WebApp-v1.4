using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Expences
{
    public class ExpenseFileService : IExpenseFileService
    {
        #region Define Services
        private readonly IRepository<Expense_Files> _expenseFilesRepository;
        public ExpenseFileService(IRepository<Expense_Files> expenseFilesRepository)
        {
            _expenseFilesRepository = expenseFilesRepository;
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

        public async Task<Expense_Files> GetExpenseFileById(string id)
        {
            var query = _expenseFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<List<Expense_Files>> GetAllExpenseFilesByExpenseId(string siteId, string expenseId)
        {
            var query = _expenseFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.ExpenseId == expenseId && x.Expenses.SiteId == siteId);

            query = query.Select(x => new Expense_Files
            {
                Id = x.Id,
                FileId = x.FileId,
                ExpenseId = x.ExpenseId,
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

        #region InsertExpenseFile
        // Title: InsertExpenseFile
        // Description: This method inserts a new Expense_Files entity into the repository. It takes a Expense_Files object as input and uses the _expenseFilesRepository to handle the insertion operation.
        public void InsertExpenseFile(Expense_Files entity)
        {
            _expenseFilesRepository.Insert(entity);
        }
        #endregion

        #region UpdateExpenseFile
        // Title: UpdateExpenseFile
        // Description: This method updates the specified Expense_Files entity in the repository. It takes a Expense_Files object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateExpenseFile(Expense_Files entity)
        {
            _expenseFilesRepository.Update(entity);
        }
        #endregion

        #region DeleteExpenseFiles
        // Title: DeleteExpenseFiles
        // Description: Marks the specified Expense_Files entity as deleted by setting its `Deleted` property to true.
        public void DeleteExpenseFiles(Expense_Files entity)
        {
            entity.Deleted = true;
            _expenseFilesRepository.Update(entity);
        }
        #endregion

    }
}
 