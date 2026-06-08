using System.Collections.Generic;

namespace Vsky.Api.Models
{
    public record EmployeeFilterModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> CompanyIds { get; set; }
        public List<string> DepartmentIds { get; set; }
        public List<string> DesignationIds { get; set; }
    }
}