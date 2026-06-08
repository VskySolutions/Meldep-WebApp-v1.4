using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.SOPAssignments
{
    public interface ISOPAssignmentService
    {
        IPagedList<SOPAssignment> GetAllSOPAssignments(
            string searchText, 
            string siteId,
            string LoggedUserId,
            string loggeduserEmployeeId, 
            List<string> templateIds, 
            List<string> AssignedToEmployeeIds, 
            List<string> ApproverEmployeeIds, 
            List<string> StatusIds, 
            List<string> PriorityIds, 
            string name, 
            bool IsApproved, 
            string sortBy, 
            bool descending, 
            int page = 1, 
            int pageSize = int.MaxValue
        );
        SOPAssignment GetSOPAssignmentById(string siteId, string Id);
        SOPAssignment GetSOPAssignmentByIdInDetail(string siteId, string Id);
        Task<SOPAssignment> GetSOPAssignmentByName(string SiteId, string name, string id = null);
        void InsertSOPAssignment(SOPAssignment entity);
        void UpdateSOPAssignment(SOPAssignment entity);
        void DeleteSOPAssignment(SOPAssignment entity);
    }
}
