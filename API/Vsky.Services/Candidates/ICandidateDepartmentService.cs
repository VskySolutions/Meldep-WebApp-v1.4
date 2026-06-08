using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Candidates
{
    public interface ICandidateDepartmentService
    {
        #region GetDepartmentById
        Task<CandidateDepartments> GetById(string id);
        List<CandidateDepartments> GetCandidateDepartmentById(string id);

        #endregion

        #region InsertCandidateDepartment
        void InsertCandidateDepartment(CandidateDepartments entity);
        #endregion

        #region UpdateCandidateDepartment
        void UpdateCandidateDepartment(CandidateDepartments entity);
        #endregion

        #region DeleteCandidateDepartment
        void DeleteCandidateDepartment(CandidateDepartments entity);
        #endregion
    }
}
