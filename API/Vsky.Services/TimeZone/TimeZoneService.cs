using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Common;

namespace Vsky.Services.TimeZone
{
    public class TimeZoneService : ITimeZoneService
    {
        #region Define Services
        private readonly IRepository<TimeZones> _timeZoneRepository;
        #endregion

        #region Services Initializations
        public TimeZoneService(
            IRepository<TimeZones> timeZoneRepository)
        {

            _timeZoneRepository = timeZoneRepository;
        }
        #endregion

        #region GetAllTimeZoneListForDropdown
        public async Task<List<TimeZones>> GetAllTimeZoneListForDropdown()
        {
            var list = await _timeZoneRepository.TableNoTracking
                .OrderBy(x => x.Continent)
                .ThenBy(x => x.Name)
                .Select(x => new TimeZones
                {
                    Continent = x.Continent,
                    Name = x.Name,
                    DisplayText = x.Continent + " - " + x.Name
                })
                .ToListAsync();

            return list;
        }
        #endregion
    }
}
