using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.TimeZone
{
    public interface ITimeZoneService
    {
        #region GetAllTimeZoneListForDropdown
        Task<List<TimeZones>> GetAllTimeZoneListForDropdown();
        #endregion
    }
}
