using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectWeeklyPlan
{
    public class ProjectWeeklyDatesService : IProjectWeeklyDatesService
    {
        #region Initialization
        private readonly IRepository<Models.ProjectWeeklyPlanDates> _projectWeeklyPlanDatesRepository;
        public ProjectWeeklyDatesService(IRepository<Models.ProjectWeeklyPlanDates> projectWeeklyPlanDatesRepository)
        {
            _projectWeeklyPlanDatesRepository = projectWeeklyPlanDatesRepository;
        }
        #endregion

        #region GetById
        public async Task<ProjectWeeklyPlanDates> GetById(string id)
        {
            return await _projectWeeklyPlanDatesRepository.TableNoTracking.FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);
        }

        public async Task<ProjectWeeklyPlanDates> GetByIdInDetail(string id)
        {
            return await _projectWeeklyPlanDatesRepository.TableNoTracking
                .Where(x => !x.Deleted && x.Id == id)
                .Include(m => m.ProjectWeeklyPlanDatesLines.Where(m => !m.Deleted))
                .FirstOrDefaultAsync();
        }

        public async Task<List<Models.ProjectWeeklyPlanDates>> GetProjectWeeklyPlanDatesByProjectId(string projectId, string planTypeId, int skipIndex = 0, int takeCount = 4, DateTime? weekEndDate = null)
        {
            var query = _projectWeeklyPlanDatesRepository.TableNoTracking
            .Where(x => !x.Deleted && x.ProjectWeeklyPlan != null && x.ProjectWeeklyPlan.ProjectId == projectId && x.PlanTypeId == planTypeId && (weekEndDate == null || x.WeekDate == weekEndDate.Value))
            .OrderByDescending(m => m.WeekDate)
            .Skip(skipIndex * takeCount)
            .Take(takeCount)
            .Select(m => new ProjectWeeklyPlanDates
            {
                Id = m.Id,
                ProjectWeeklyPlanId = m.ProjectWeeklyPlanId,
                PlanTypeId = m.PlanTypeId,
                WeekDate = m.WeekDate,
                IsApproved = m.IsApproved,
                ApprovedById = m.ApprovedById,
                ApprovedBy = m.ApprovedBy == null ? null : new ApplicationUser
                {
                    Id = m.ApprovedBy.Id,
                    PersonId = m.ApprovedBy.PersonId,
                    Person = m.ApprovedBy.Person == null ? null : new Person
                    {
                        Id = m.ApprovedBy.Person.Id,
                        FirstName = m.ApprovedBy.Person.FirstName,
                        LastName = m.ApprovedBy.Person.LastName
                    }
                },
                ApprovedOnUtc = m.ApprovedOnUtc,
                IsCompleted = m.IsCompleted,
                CompletionPercentage = m.CompletionPercentage,
                CompletedById = m.CompletedById,
                CompletedBy = m.CompletedBy == null ? null : new ApplicationUser
                {
                    Id = m.CompletedBy.Id,
                    PersonId = m.CompletedBy.PersonId,
                    Person = m.CompletedBy.Person == null ? null : new Person
                    {
                        Id = m.CompletedBy.Person.Id,
                        FirstName = m.CompletedBy.Person.FirstName,
                        LastName = m.CompletedBy.Person.LastName
                    }
                },
                CompletedOnUtc = m.CompletedOnUtc,
                ProjectWeeklyPlanDatesLines = m.ProjectWeeklyPlanDatesLines.Where(n => !n.Deleted && (weekEndDate == null || n.ProjectWeeklyPlanDates.WeekDate == weekEndDate.Value))
                .OrderByDescending(n => n.ExpectedDescriptionCreatedOnUtc)
                .Select(n => new ProjectWeeklyPlanDatesLines
                {
                    Id = n.Id,
                    ProjectWeeklyPlanDatesId = n.ProjectWeeklyPlanDatesId,
                    ExpectedDescription = n.ExpectedDescription,
                    ExpectedHours = n.ExpectedHours,
                    ExpectedDescriptionCreatedById = n.ExpectedDescriptionCreatedById,
                    ExpectedDescriptionCreatedBy = n.ExpectedDescriptionCreatedBy == null ? null : new ApplicationUser
                    {
                        Id = n.ExpectedDescriptionCreatedBy.Id,
                        PersonId = n.ExpectedDescriptionCreatedBy.PersonId,
                        Person = n.ExpectedDescriptionCreatedBy.Person == null ? null : new Person
                        {
                            Id = n.ExpectedDescriptionCreatedBy.Person.Id,
                            FirstName = n.ExpectedDescriptionCreatedBy.Person.FirstName,
                            LastName = n.ExpectedDescriptionCreatedBy.Person.LastName
                        }
                    },
                    ExpectedDescriptionCreatedOnUtc = n.ExpectedDescriptionCreatedOnUtc, // fixed
                    ExpectedDescriptionUpdatedById = n.ExpectedDescriptionUpdatedById,
                    ExpectedDescriptionUpdatedBy = n.ExpectedDescriptionUpdatedBy == null ? null : new ApplicationUser
                    {
                        Id = n.ExpectedDescriptionUpdatedBy.Id,
                        PersonId = n.ExpectedDescriptionUpdatedBy.PersonId,
                        Person = n.ExpectedDescriptionUpdatedBy.Person == null ? null : new Person
                        {
                            Id = n.ExpectedDescriptionUpdatedBy.Person.Id,
                            FirstName = n.ExpectedDescriptionUpdatedBy.Person.FirstName,
                            LastName = n.ExpectedDescriptionUpdatedBy.Person.LastName
                        }
                    },
                    ExpectedDescriptionUpdatedOnUtc = n.ExpectedDescriptionUpdatedOnUtc,
                    ActualDescription = n.ActualDescription,
                    ActualDescriptionCreatedById = n.ActualDescriptionCreatedById,
                    ActualDescriptionCreatedBy = n.ActualDescriptionCreatedBy == null ? null : new ApplicationUser
                    {
                        Id = n.ActualDescriptionCreatedBy.Id,
                        PersonId = n.ActualDescriptionCreatedBy.PersonId,
                        Person = n.ActualDescriptionCreatedBy.Person == null ? null : new Person
                        {
                            Id = n.ActualDescriptionCreatedBy.Person.Id,
                            FirstName = n.ActualDescriptionCreatedBy.Person.FirstName,
                            LastName = n.ActualDescriptionCreatedBy.Person.LastName
                        }
                    },
                    ActualDescriptionCreatedOnUtc = n.ActualDescriptionCreatedOnUtc,
                    ActualDescriptionUpdatedById = n.ActualDescriptionUpdatedById,
                    ActualDescriptionUpdatedBy = n.ActualDescriptionUpdatedBy == null ? null : new ApplicationUser
                    {
                        Id = n.ActualDescriptionUpdatedBy.Id,
                        PersonId = n.ActualDescriptionUpdatedBy.PersonId,
                        Person = n.ActualDescriptionUpdatedBy.Person == null ? null : new Person
                        {
                            Id = n.ActualDescriptionUpdatedBy.Person.Id,
                            FirstName = n.ActualDescriptionUpdatedBy.Person.FirstName,
                            LastName = n.ActualDescriptionUpdatedBy.Person.LastName
                        }
                    },
                    ActualDescriptionUpdatedOnUtc = n.ActualDescriptionUpdatedOnUtc,
                    ProjectWeeklyPlanDatesLinesAssignedTo = n.ProjectWeeklyPlanDatesLinesAssignedTo.Where(a => !a.Deleted)
                    .OrderByDescending(a => a.CreatedOnUtc)
                    .Select(x => new ProjectWeeklyPlanDatesLinesAssignedTo
                    {
                        Id = x.Id,
                        ProjectWeeklyPlanDatesLineId = x.ProjectWeeklyPlanDatesLineId,
                        EmployeeId = x.EmployeeId,
                        Employee = x.Employee == null ? null : new Employee
                        {
                            Id = x.Employee.Id,
                            Active = x.Employee.Active,
                            Person = x.Employee.Person == null ? null : new Person
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
                }
            ).ToList(),
            ProjectWeeklyPlanDatesReqTaskIssueMapping = m.ProjectWeeklyPlanDatesReqTaskIssueMapping.Where(n => !n.Deleted)
            .Select(n => new ProjectWeeklyPlanDatesReqTaskIssueMapping
            {
                Id = n.Id,
                ProjectWeeklyPlanDatesId = n.ProjectWeeklyPlanDatesId,
                RequirementId = n.RequirementId,
                Requirement = n.Requirement == null ? null : new Requirement
                {
                    Id = n.Requirement.Id,
                    RequirementNumber = n.Requirement.RequirementNumber,
                    Title = n.Requirement.Title,
                },
                TaskId = n.TaskId,
                Task = n.Task == null ? null : new ProjectTask
                {
                    Id = n.Task.Id,
                    ProjectTaskNumber = n.Task.ProjectTaskNumber,
                    Name = n.Task.Name,
                },
                IssueId = n.IssueId,
                Issue = n.Issue == null ? null : new Issue
                {
                    Id = n.Issue.Id,
                    IssueNumber = n.Issue.IssueNumber,
                    Name = n.Issue.Name,
                }
            }).ToList(),
            EmployeeEstimateHoursForWeekSummaryList = m.ProjectWeeklyPlanDatesLines.Where(dl => !dl.Deleted)
            .SelectMany(n => n.ProjectWeeklyPlanDatesLinesAssignedTo.Where(a => !a.Deleted))
            .GroupBy(o => o.EmployeeId)
            .Select(p => new EmployeeEstimateHoursForWeekSummary
            {
                EmployeeId = p.Key,
                Employee = new Employee
                {
                    Id = p.First().Employee.Id,
                    Person = new Person
                    {
                        Id = p.First().Employee.Person.Id,
                        FirstName = p.First().Employee.Person.FirstName,
                        LastName = p.First().Employee.Person.LastName,
                        PrimaryEmailAddress = p.First().Employee.Person.PrimaryEmailAddress,
                        PrimaryPhoneNumber = p.First().Employee.Person.PrimaryPhoneNumber,
                    }
                },
                TotalEstimatedHours = p.Sum(q => q.EstimatedHours)
            })
            .OrderByDescending(x => x.TotalEstimatedHours)
            .ToList()
            });

            var list = await query.ToListAsync();
            return list;
        }

        public async Task<List<EmployeeEstimateHoursForWeekSummary>> GetEmployeeHourSummaryByWeekPlanId(string planTypeId, string planWeekId)
        {
            var summary = await _projectWeeklyPlanDatesRepository.TableNoTracking
            .Where(x => !x.Deleted && x.PlanTypeId == planTypeId && x.Id == planWeekId)
            .SelectMany(m => m.ProjectWeeklyPlanDatesLines.Where(dl => !dl.Deleted)
            .SelectMany(n => n.ProjectWeeklyPlanDatesLinesAssignedTo.Where(a => !a.Deleted)))
            .GroupBy(o => o.EmployeeId)
            .Select(p => new EmployeeEstimateHoursForWeekSummary
            {
                EmployeeId = p.Key,
                Employee = new Employee
                {
                    Id = p.First().Employee.Id,
                    Person = new Person
                    {
                        Id = p.First().Employee.Person.Id,
                        FirstName = p.First().Employee.Person.FirstName,
                        LastName = p.First().Employee.Person.LastName,
                        PrimaryEmailAddress = p.First().Employee.Person.PrimaryEmailAddress,
                        PrimaryPhoneNumber = p.First().Employee.Person.PrimaryPhoneNumber,
                    }
                },
                TotalEstimatedHours = p.Sum(q => q.EstimatedHours)
            })
            .OrderByDescending(x => x.TotalEstimatedHours)
            .ToListAsync();

            return summary;
        }

        public async Task<string> CheckIfProjectWeeklyPlanIsCreated(string projectId, string planTypeId, DateTime weekDate)
        {
           var query = await _projectWeeklyPlanDatesRepository.TableNoTracking.Where(m => !m.Deleted && m.ProjectWeeklyPlan.ProjectId == projectId && m.PlanTypeId == planTypeId && m.WeekDate == weekDate).FirstOrDefaultAsync();

            return query != null ? query.Id : "";
        }
        #endregion

        #region Insert & Update & Delete
        public void InsertProjectWeeklyPlanDates(Models.ProjectWeeklyPlanDates entity)
        {
            _projectWeeklyPlanDatesRepository.Insert(entity);
        }
        public void UpdateProjectWeeklyPlanDates(Models.ProjectWeeklyPlanDates entity)
        {
            _projectWeeklyPlanDatesRepository.Update(entity);
        }
        public void DeleteProjectWeeklyPlanDates(Models.ProjectWeeklyPlanDates entity)
        {
            entity.Deleted = true;
            _projectWeeklyPlanDatesRepository.Update(entity);
        }
        #endregion
    }
}
