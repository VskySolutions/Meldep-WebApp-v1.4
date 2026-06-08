using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Vsky.Services.Sites;
namespace Vsky.Services.SalesPersons
{
    public class SalesPersonService : ISalesPersonService
    {
        #region Service Initialization
        private readonly IRepository<SalesPerson> _salespersonRepository;
        public SalesPersonService(IRepository<SalesPerson> salespersonRepository)
        {
            _salespersonRepository = salespersonRepository;
        }
        #endregion

        #region Private Methods

        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }

        #endregion

        #region GetAllCompanies
        public IPagedList<SalesPerson> GetAllSalesPersons(string SiteId, string name, string sortBy,
            bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _salespersonRepository.TableNoTracking.Where(x => !x.Deleted && x.Employee.SiteId == SiteId);

            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }

            // projection
            query = query.Select(x => new SalesPerson
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                Employee = new Employee
                {
                    Id = x.Id,
                    Person = new Person
                    {
                        Id= x.Id,
                       FirstName = x.Employee.Person.FirstName,
                       LastName = x.Employee.Person.LastName,
                       PrimaryEmailAddress = x.Employee.Person.PrimaryEmailAddress,
                       PrimaryPhoneNumber = x.Employee.Person.PrimaryPhoneNumber,
                    }
                },
            });
            var list = new PagedList<SalesPerson>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetById
        public async Task<SalesPerson> GetById(string id)
        {
            var query = _salespersonRepository.TableNoTracking.Where(x => x.Id == id);
            query = query.Where(x => !x.Deleted);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetSalesPersonDetailsById
        public async Task<SalesPerson> GetSalesPersonDetailsById(string id)
        {
            var query = _salespersonRepository.TableNoTracking.Where(x => x.Id == id && !x.Deleted);
            query = query.Select(x => new SalesPerson
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                Employee = new Employee
                {
                    Id = x.Id,
                    Person = new Person
                    {
                        Id = x.Id,
                        FirstName = x.Employee.Person.FirstName,
                        LastName = x.Employee.Person.LastName,
                        PrimaryEmailAddress = x.Employee.Person.PrimaryEmailAddress,
                        PrimaryPhoneNumber = x.Employee.Person.PrimaryPhoneNumber,
                    }
                },
            });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertSalesPerson
        public void InsertSalesPerson(SalesPerson entity)
        {
            _salespersonRepository.Insert(entity);
        }
        #endregion
    }
}
