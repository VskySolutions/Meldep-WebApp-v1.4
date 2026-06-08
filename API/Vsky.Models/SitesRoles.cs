using System;
using System.Collections.Generic;
using Vsky.Core;


namespace Vsky.Models;

   public class SitesRoles : BaseEntity
   {
        public string SiteId { get; set; }
        public string RoleId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }
       public virtual Site Site { get; set; }
       public virtual ApplicationRole ApplicationRole { get; set; }
   }
