using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.EmployeeOrgStructureDesignationMappings
{
    public class EmployeeOrgStructureDesignationMappingService : IEmployeeOrgStructureDesignationMappingService
    {
        #region Define Service
        private readonly IRepository<EmployeeOrgStructureDesignationMapping> _employeeOrgStructureDesignationMappingRepository;
        #endregion

        #region Service Initializations
        public EmployeeOrgStructureDesignationMappingService(IRepository<EmployeeOrgStructureDesignationMapping> employeeOrgStructureDesignationMapping)
        {
            _employeeOrgStructureDesignationMappingRepository = employeeOrgStructureDesignationMapping;
        }
        #endregion

        #region GetEmployeeOrgStructureByEmployeeOrgStructureId
        public List<EmployeeOrgStructureDesignationMapping> GetEmployeeOrgStructureByEmployeeOrgStructureId(string SiteId, string employeeOrgStructureId)
        {
            var query = _employeeOrgStructureDesignationMappingRepository.TableNoTracking.Where(x => x.EmployeeOrgStructureId == employeeOrgStructureId && x.EmployeeOrgStructure.SiteId == SiteId);
            var list = query.ToList();
            return list;
        }
        #endregion

        #region InsertEmployeeOrgStructureDesignations
        public void InsertEmployeeOrgStructureDesignations(EmployeeOrgStructureDesignationMapping entity)
        {
            _employeeOrgStructureDesignationMappingRepository.Insert(entity);
        }
        #endregion

        #region UpdateEmployeeOrgStructureDesignations
        // Title : UpdateEmployeeOrgStructureDesignations
        // Description: Updates an existing EmployeeOrgStructureDesignationMapping entity in the repository.
        public void UpdateEmployeeOrgStructureDesignations(EmployeeOrgStructureDesignationMapping entity)
        {
            _employeeOrgStructureDesignationMappingRepository.Update(entity);
        }
        #endregion

        #region DeleteEmployeeOrgStructureDesignations
        // Title : DeleteEmployeeOrgStructureDesignations
        // Description: Deletes a EmployeeOrgStructureDesignationMapping entity from the repository.
        public void DeleteEmployeeOrgStructureDesignations(EmployeeOrgStructureDesignationMapping entity)
        {
            _employeeOrgStructureDesignationMappingRepository.Delete(entity);
        }
        #endregion
    }
}
