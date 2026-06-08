using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Candidates
{
    public class CandidateDepartmentService : ICandidateDepartmentService
    {
        #region Define Services
        private readonly IRepository<CandidateDepartments> _candidateDepartmentRepository;
        #endregion

        #region Services Initializations
        public CandidateDepartmentService(IRepository<CandidateDepartments> departmentRepository)
        {
            _candidateDepartmentRepository = departmentRepository;
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

        #region GetById
        // Title: GetById
        // Description: This method retrieves a department from the database by its unique identifier (`id`). 
        public async Task<CandidateDepartments> GetById(string id)
        {
            var query = _candidateDepartmentRepository.TableNoTracking.Where(x => x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public List<CandidateDepartments> GetCandidateDepartmentById(string id)
        {
            var query = _candidateDepartmentRepository.TableNoTracking.Where(x => x.CandidateId == id);

            var list =  query.ToList();
            return list;
        }
        #endregion

        #region InsertDepartment
        // Title: InsertDepartment
        // Description: This method inserts a new department entity into the repository. It takes a department object as input and uses the _departmentRepository to handle the insertion operation.
        public void InsertCandidateDepartment(CandidateDepartments entity)
        {
            _candidateDepartmentRepository.Insert(entity);
        }
        #endregion

        #region UpdateDepartment
        // Title: UpdateDepartment
        // Description: This method updates the specified department entity in the repository. It takes a department object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateCandidateDepartment(CandidateDepartments entity)
        {
            _candidateDepartmentRepository.Update(entity);
        }
        #endregion

        #region DeleteCandidateDepartment
        public void DeleteCandidateDepartment(CandidateDepartments entity)
        {
            _candidateDepartmentRepository.Delete(entity);
        }
        #endregion
    }
}
