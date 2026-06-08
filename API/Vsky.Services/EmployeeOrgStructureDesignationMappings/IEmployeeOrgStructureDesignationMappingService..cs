using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.EmployeeOrgStructureDesignationMappings
{
    public interface IEmployeeOrgStructureDesignationMappingService
    {
        #region GetTrainingByTrainingId
        List<EmployeeOrgStructureDesignationMapping> GetEmployeeOrgStructureByEmployeeOrgStructureId(string SiteId, string employeeOrgStructureId);
        #endregion

        #region InsertEmployeeOrgStructureDesignations
        void InsertEmployeeOrgStructureDesignations(EmployeeOrgStructureDesignationMapping entity);
        #endregion

        #region UpdateEmployeeOrgStructureDesignations
        void UpdateEmployeeOrgStructureDesignations(EmployeeOrgStructureDesignationMapping entity);
        #endregion

        #region DeleteEmployeeOrgStructureDesignations
        void DeleteEmployeeOrgStructureDesignations(EmployeeOrgStructureDesignationMapping entity);
        #endregion
    }
}
