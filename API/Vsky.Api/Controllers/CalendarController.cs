using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Core.Configuration;
using Vsky.Data;
using Vsky.Models;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Net;
using System.Data.SqlTypes;

namespace Vsky.Api.Controllers
{
    [Route("calendar")]
    public class CalendarController : BaseController
    {
        #region Define Services
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        #endregion

        #region Services Initializations
        public CalendarController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        #endregion

        #region GetAllCalendarEvents
        [HttpPost("list")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCalendarEvents([FromBody] CalendarSearchModel searchModel)
        {
            try
            {
                var icsUrl = searchModel.OutlookICSLink;

                if (string.IsNullOrWhiteSpace(icsUrl))
                    return BadRequest("ICS URL is required");

                using var webClient = new WebClient();
                webClient.Headers.Add("User-Agent", "Mozilla/5.0");

                string icsData = webClient.DownloadString(icsUrl);
                var calendar = Ical.Net.Calendar.Load(icsData);

                //var calendar = Ical.Net.Calendar.Load(icsData);

                //searchModel.Year = 2025;
                //searchModel.Month = 7;
                var startDate = new CalDateTime(new DateTime(searchModel.Year.Value, searchModel.Month.Value, 1));
                var endDay = DateTime.DaysInMonth(searchModel.Year.Value, searchModel.Month.Value);
                var endDate = new DateTime(searchModel.Year.Value, searchModel.Month.Value, endDay, 23, 59, 59);

                // Get all occurrences starting from the first day of the month
                var occurrences = calendar.GetOccurrences(startDate);

                var events = new List<CalendarModel>();

                foreach (var occ in occurrences)
                {
                    var periodStart = occ.Period.StartTime?.Value;
                    var periodEnd = occ.Period.EffectiveEndTime?.Value;

                    if (periodStart == null || periodEnd == null)
                        continue; // skip if either is null

                    if (periodStart > endDate)
                        continue;

                    var calendarEvent = occ.Source as CalendarEvent;
                    if (calendarEvent != null)
                    {
                        events.Add(new CalendarModel
                        {
                            Uid = calendarEvent.Uid,
                            Subject = calendarEvent.Summary,
                            Start = periodStart.Value,
                            End = periodEnd.Value,
                            StartDateTimeCalendar = periodStart?.ToString("yyyy-MM-ddTHH:mm:ss"),
                            EndDateTimeCalendar = periodEnd?.ToString("yyyy-MM-ddTHH:mm:ss"),
                            StartTimeCalendar = periodStart?.ToString("HH:mm:ss"),
                            EndTimeCalendar = periodEnd?.ToString("HH:mm:ss"),
                            StartDateStr = periodStart?.ToString("yyyy-MM-dd"),
                            EndDateStr = periodEnd?.ToString("yyyy-MM-dd"),
                            StartTime = periodStart?.ToString("hh:mm tt"),
                            EndTime = periodEnd?.ToString("hh:mm tt"),
                            Description = ExtractTitleFromDescription(calendarEvent.Description ?? ""),
                            Location = calendarEvent.Location
                        });
                    }
                }

                // Sort events descending by Start, then End
                var sortedEvents = events
                    .OrderByDescending(e => e.Start)
                    .ThenByDescending(e => e.End)
                    .ToList();

                var model = new CalendarListModel
                {
                    Data = sortedEvents,
                    Total = sortedEvents.Count
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        #endregion

        private string ExtractTeamsLink(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return null;

            var match = Regex.Match(description, @"https:\/\/teams\.microsoft\.com\/[^\s]+");
            return match.Success ? match.Value : null;
        }

        private string ExtractTitleFromDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return string.Empty;

            var lines = description
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            foreach (var line in lines)
            {
                // Skip unwanted Teams-related footer or formatting lines
                if (line.StartsWith("Microsoft Teams", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("Join the meeting", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("Meeting ID", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("Passcode", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("Meeting options", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("For organisers", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("For organizers", StringComparison.OrdinalIgnoreCase) ||
                    line.All(c => c == '_' || c == '–' || c == '-')) // separator line
                {
                    continue;
                }

                return line; // Return the first valid line
            }

            return string.Empty;
        }




    }
}