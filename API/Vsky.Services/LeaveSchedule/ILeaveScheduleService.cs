using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.LeaveSchedule
{
    public interface ILeaveScheduleService
    {
        Task<IList<LeaveSchedules>> GetAllLeaveEvents(string SiteId);
        Task<LeaveSchedules> GetLeaveEventById(string SiteId, string id);
        Task<LeaveSchedules> GetLeaveEventDetailsById(string id);
        Task<LeaveSchedules> GetLeaveScheduleByDate(string SiteId, DateTime? Date);
        Task<List<LeaveSchedules>> GetEmployeeLeaveListForDashboard(string SiteId, DateTime GetDateTime);

        void InsertLeaveEvent(LeaveSchedules entity);
        void UpdateLeaveEvent(LeaveSchedules entity);
        void DeleteLeaveEvent(LeaveSchedules entity);
    }
}
