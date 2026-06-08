using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectWeeklyPlan
{
    public class ProjectWeeklyDatesLinesService : IProjectWeeklyDatesLinesService
    {
        #region Initialization
        private readonly IRepository<ProjectWeeklyPlanDatesLines> _projectWeeklyPlanDatesLinesRepository;
        public ProjectWeeklyDatesLinesService(IRepository<ProjectWeeklyPlanDatesLines> projectWeeklyPlanDatesLinesRepository)
        {
            _projectWeeklyPlanDatesLinesRepository = projectWeeklyPlanDatesLinesRepository;
        }
        #endregion

        #region GetById
        public async Task<ProjectWeeklyPlanDatesLines> GetById(string id)
        {
            var query = await _projectWeeklyPlanDatesLinesRepository.TableNoTracking.FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);
            return query;
        }
        public async Task<ProjectWeeklyPlanDatesLines> GetInDetailById(string id)
        {
            var query = _projectWeeklyPlanDatesLinesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.Select(n => new ProjectWeeklyPlanDatesLines
            {
                Id = n.Id,
                ProjectWeeklyPlanDatesId = n.ProjectWeeklyPlanDatesId,
                ExpectedDescription = n.ExpectedDescription,
                ExpectedHours = n.ExpectedHours,
                ExpectedDescriptionCreatedOnUtc = n.ExpectedDescriptionCreatedOnUtc,
                ExpectedDescriptionCreatedById = n.ExpectedDescriptionCreatedById,
                ExpectedDescriptionCreatedBy = new ApplicationUser
                {
                    Id = n.ExpectedDescriptionCreatedBy.Id,
                    PersonId = n.ExpectedDescriptionCreatedBy.PersonId,
                    Person = new Person
                    {
                        Id = n.ExpectedDescriptionCreatedBy.Person.Id,
                        FirstName = n.ExpectedDescriptionCreatedBy.Person.FirstName,
                        LastName = n.ExpectedDescriptionCreatedBy.Person.LastName
                    }
                },
                ExpectedDescriptionUpdatedOnUtc = n.ExpectedDescriptionUpdatedOnUtc,
                ExpectedDescriptionUpdatedById = n.ExpectedDescriptionUpdatedById,
                ExpectedDescriptionUpdatedBy = new ApplicationUser
                {
                    Id = n.ExpectedDescriptionUpdatedBy.Id,
                    PersonId = n.ExpectedDescriptionUpdatedBy.PersonId,
                    Person = new Person
                    {
                        Id = n.ExpectedDescriptionUpdatedBy.Person.Id,
                        FirstName = n.ExpectedDescriptionUpdatedBy.Person.FirstName,
                        LastName = n.ExpectedDescriptionUpdatedBy.Person.LastName
                    }
                },
                ActualDescription = n.ActualDescription,
                ActualDescriptionCreatedOnUtc = n.ActualDescriptionCreatedOnUtc,
                ActualDescriptionCreatedById = n.ActualDescriptionUpdatedById,
                ActualDescriptionCreatedBy = new ApplicationUser
                {
                    Id = n.ActualDescriptionCreatedBy.Id,
                    PersonId = n.ActualDescriptionCreatedBy.PersonId,
                    Person = new Person
                    {
                        Id = n.ActualDescriptionCreatedBy.Person.Id,
                        FirstName = n.ActualDescriptionCreatedBy.Person.FirstName,
                        LastName = n.ActualDescriptionCreatedBy.Person.LastName
                    }
                },
                ActualDescriptionUpdatedOnUtc = n.ActualDescriptionUpdatedOnUtc,
                ActualDescriptionUpdatedById = n.ActualDescriptionUpdatedById,
                ActualDescriptionUpdatedBy = new ApplicationUser
                {
                    Id = n.ActualDescriptionUpdatedBy.Id,
                    PersonId = n.ActualDescriptionUpdatedBy.PersonId,
                    Person = new Person
                    {
                        Id = n.ActualDescriptionUpdatedBy.Person.Id,
                        FirstName = n.ActualDescriptionUpdatedBy.Person.FirstName,
                        LastName = n.ActualDescriptionUpdatedBy.Person.LastName
                    }
                },
                ProjectWeeklyPlanDatesLinesAssignedTo = n.ProjectWeeklyPlanDatesLinesAssignedTo.Where(m => !m.Deleted).OrderByDescending(m => m.CreatedOnUtc).Select(x => new ProjectWeeklyPlanDatesLinesAssignedTo
                {
                    Id = x.Id,
                    ProjectWeeklyPlanDatesLineId = x.ProjectWeeklyPlanDatesLineId,
                    EmployeeId = x.EmployeeId,
                    Employee = new Employee
                    {
                        Id = x.Employee.Id,
                        Active = x.Employee.Active,
                        Person = new Person
                        {
                            Id = x.Employee.Person.Id,
                            FirstName = x.Employee.Person.FirstName,
                            LastName = x.Employee.Person.LastName,
                            PrimaryEmailAddress = x.Employee.Person.PrimaryEmailAddress,
                            PrimaryPhoneNumber = x.Employee.Person.PrimaryPhoneNumber,
                        }
                    },
                    EstimatedHours = x.EstimatedHours,
                    CreatedById = x.CreatedById,
                    CreatedOnUtc = x.CreatedOnUtc,
                    Deleted = x.Deleted
                }).ToList()
            }).FirstOrDefaultAsync();
            
            return item;
        }
        #endregion

        #region Insert & Update & Delete
        public void InsertProjectWeeklyPlanDatesLines(ProjectWeeklyPlanDatesLines entity)
        {
            _projectWeeklyPlanDatesLinesRepository.Insert(entity);
        }
        public void UpdateProjectWeeklyPlanDatesLines(ProjectWeeklyPlanDatesLines entity)
        {
            _projectWeeklyPlanDatesLinesRepository.Update(entity);
        }
        public void DeleteProjectWeeklyPlanDatesLines(ProjectWeeklyPlanDatesLines entity)
        {
            entity.Deleted = true;
            _projectWeeklyPlanDatesLinesRepository.Update(entity);
        }
        #endregion
    }
}
