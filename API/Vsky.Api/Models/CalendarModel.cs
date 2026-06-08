using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record CalendarModel : BaseEntityModel
    {
        public string Uid { get; set; }
        public string Subject { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string StartDateStr { get; set; }
        public string EndDateStr { get; set; }
        public string StartTime { get; set; } // e.g., "03:45 PM"
        public string EndTime { get; set; }   // e.g., "04:30 PM"
        public string Description { get; set; }
        public string Location { get; set; }
        public string TeamsMeetingUrl { get; set; }
        public string Timezone { get; set; }  // e.g. "Asia/Kolkata", "America/New_York"

        public string StartDateTimeCalendar { get; set; }
        public string EndDateTimeCalendar { get; set; }

        public string StartTimeCalendar { get; set; }
        public string EndTimeCalendar { get; set; }


    }
    public record CalendarSearchModel : BaseSearchModel
    {
        public int? Month { get; set; }
        public int? Year { get; set; }
        public string Timezone { get; set; }
        public string OutlookICSLink { get; set; }
    }

    public record CalendarListModel : BasePagedListModel<CalendarModel>
    {
        public bool editing { get; set; }
    }



}
