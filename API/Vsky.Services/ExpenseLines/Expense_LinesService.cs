using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vsky.Services.ExpenseExpensExpense_Lines;
using System.Xml.Linq;


namespace Vsky.Services.ExpenseLines
{

    public class Expense_LinesService : IExpense_LinesService
    {
        #region Fields

        private readonly IRepository<Expense_Lines> _expenseLinesServiceRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly IRepository<Picture> _pictureRepository;

        #endregion

        #region Ctor

        public Expense_LinesService(IRepository<Expense_Lines> Expense_LinesService, ApplicationDbContext dbContext, IRepository<Picture> pictureRepository)
        {
            _expenseLinesServiceRepository = Expense_LinesService;   // dependancy injection
            _dbContext = dbContext;
            _pictureRepository = pictureRepository;
        }

        #endregion

        #region Private Methods

        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }

        #endregion

        #region Public Methods

        public IPagedList<Expense_Lines> GetAllExpensExpense_Lines(string SiteId, string ExpenseCategoryId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _expenseLinesServiceRepository.TableNoTracking.Where(x => !x.Deleted && x.Expense.SiteId == SiteId);

            // Custom filter
            if (!string.IsNullOrWhiteSpace(ExpenseCategoryId))
            {
                query = query.Where(x => x.ExpenseCategoryId.Contains(ExpenseCategoryId));
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderBy(x => x.ExpenseCategoryId);
            }

            if (lookup)
            {
                query = query.Select(x => new Expense_Lines
                {
                    Id = x.Id,
                    Description = x.Description,
                    Amount = x.Amount
                });
            }
            else
            {
                query = query.Select(x => new Expense_Lines
                {
                    Id = x.Id,
                    Description = x.Description,
                    Amount = x.Amount,
                    Attachment = x.Attachment,
                    ExpenseCategoryId = x.ExpenseCategoryId,
                    ExpenseSubcategoryId = x.ExpenseSubcategoryId,
                    CreatedById = x.CreatedById,
                    CreatedOnUtc = x.CreatedOnUtc,
                    UpdatedById = x.UpdatedById,
                    UpdatedOnUtc = x.UpdatedOnUtc,


                }).OrderByDescending(x => x.UpdatedOnUtc);

            }

            var list = new PagedList<Expense_Lines>(query, page, pageSize);

            return list;
        }

        public async Task<Expense_Lines> GetByExpenseLinesId(string id)
        {
            var query = _expenseLinesServiceRepository.TableNoTracking.Where(x => x.Id == id);

            query = query.Where(x => !x.Deleted && x.Id == id);

            var item = await query.FirstOrDefaultAsync();

            return item;
        }

        public async Task<List<Expense_Lines>> GetByExpenseId(string SiteId, string ExpenseId)  //added for delete
        {

            var query = _expenseLinesServiceRepository.TableNoTracking.Where(x => x.ExpenseId == ExpenseId && !x.Deleted && x.Expense.SiteId == SiteId);
            var item = await query.ToListAsync();
            return item;

        }


        public void InsertExpensExpense_Lines(List<Expense_Lines> addList)
        {
            foreach (var entity in addList)
            {
                _expenseLinesServiceRepository.Insert(entity);
            }
        }


        public void UpdateExpensExpense_Lines(List<Expense_Lines> updateList)
        {
            foreach (var entity in updateList)
            {
                _expenseLinesServiceRepository.Update(entity);
            }
        }

        public void DeleteExpensExpense_Lines(List<Expense_Lines> deleteList)
        {
            //foreach (var entity in deleteList)
            //{
            //    _expenseLinesServiceRepository.Update(entity);
            //}
            _expenseLinesServiceRepository.Update(deleteList);
        }      

        #endregion
    }
}
