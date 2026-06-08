using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class Candidate : BaseEntity
    {
        [NotMapped]
        public int SearchNumber { get; set; }
        [NotMapped]
        public int CandidateNotesCount { get; set; }
        [NotMapped]
        public int CandidateFeedbackCount { get; set; }
        [NotMapped]
        public int CandidateActivitiesCount { get; set; }

        public string SiteId { get; set; }
        public string JobId { get; set; }
        public string PersonId { get; set; }
        public string AddressId { get; set; }
        public string EnglishFluencyId { get; set; }
        public string LanguageId { get; set; }
        public string SourceId { get; set; }
        public string DepartmentId { get; set; }
        public string AppliedJobPositionId { get; set; }
        public string RecruiterId { get; set; }
        public string AppliedWorkLocationId { get; set; }
        public string StatusId { get; set; }
        public string AvailabilityWorkId { get; set; }
        public string Source { get; set; }
        public DateTime? JobApplyDate { get; set; }
        public string ReferenceName { get; set; }
        public string Qualification { get; set; }
        public int ExperienceYears { get; set; }
        public int ExperienceMonths { get; set; }        
        public decimal ExpectedSalaryFrom { get; set; }        
        public decimal ExpectedSalaryTo { get; set; }
        public string ExperienceDetails { get; set; }
        public string IsTransportration { get; set; }
        public string IsReadyToRelocate { get; set; }
        public string IsCandidateOwnSystem { get; set; }
        public string IsExperienced { get; set; }
        public string CandidateResumeFileId { get; set; }
        public string LinkedInProfile { get; set; }
        public string CreatedById { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        //FK relations
        public virtual Site Sites { get; set; }
        public virtual JobCreate Job { get; set; }
        public virtual Person Person { get; set; }
        public virtual Address Address { get; set; }
        public virtual DropDown EnglishFluencies { get; set; }
        public virtual DropDown Language { get; set; }
        public virtual DropDown Sources { get; set; }
        public virtual Department Departments { get; set; }
        public virtual DropDown AppliedJobPositions { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual DropDown AppliedWorkLocations { get; set; }
        public virtual DropDown AvailabilityWorks { get; set; }
        public virtual DropDown Status { get; set; }
        public virtual Picture File { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }

        //Mapping tables
        public virtual ICollection<CandidateActivities> CandidateActivities { get; set; } = new List<CandidateActivities>();
        public virtual ICollection<CandidateFeedback> CandidateFeedback { get; set; } = new List<CandidateFeedback>();
        public virtual ICollection<CandidateDepartments> CandidateDepartments { get; set; } = new List<CandidateDepartments>();
        public virtual ICollection<CandidateNotes> CandidateNotes { get; set; } = new List<CandidateNotes>();

    }
}
