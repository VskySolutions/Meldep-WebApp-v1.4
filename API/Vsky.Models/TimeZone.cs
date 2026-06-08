using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models
{
    public class TimeZones : BaseEntity
    {
        public string Continent { get; set; }
        public string Name { get; set; }

        [NotMapped]
        public string DisplayText { get; set; }
    }
}

