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
    public class ProjectWeeklyPlanDatesLinesAssignedToService : IProjectWeeklyPlanDatesLinesAssignedToService
    {
        #region Initialization
        private readonly IRepository<ProjectWeeklyPlanDatesLinesAssignedTo> _projectWeeklyPlanDatesLinesAssignedToRepository;
        public ProjectWeeklyPlanDatesLinesAssignedToService(
            IRepository<ProjectWeeklyPlanDatesLinesAssignedTo> projectWeeklyPlanDatesLinesAssignedToRepository
        )
        {
            _projectWeeklyPlanDatesLinesAssignedToRepository = projectWeeklyPlanDatesLinesAssignedToRepository;
        }
        #endregion

        #region GetById & GetAllByProjectWeeklyPlanDatesLineId
        public async Task<ProjectWeeklyPlanDatesLinesAssignedTo> GetById(string Id)
        {
            return await _projectWeeklyPlanDatesLinesAssignedToRepository.TableNoTracking.FirstOrDefaultAsync(x => !x.Deleted && x.Id == Id);
        }
        
        public async Task<ProjectWeeklyPlanDatesLinesAssignedTo> GetByIdInDetail(string Id)
        {
            return await _projectWeeklyPlanDatesLinesAssignedToRepository.TableNoTracking
            .Where(x => !x.Deleted && x.Id == Id)
            .OrderByDescending(m => m.CreatedOnUtc)
            .Select(x => new ProjectWeeklyPlanDatesLinesAssignedTo
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
            }).FirstOrDefaultAsync();
        }

        public async Task<List<ProjectWeeklyPlanDatesLinesAssignedTo>> GetAllAssignToAsList(string ProjectWeeklyPlanDatesLineId)
        {
            return await _projectWeeklyPlanDatesLinesAssignedToRepository.TableNoTracking
                .Where(x => !x.Deleted && x.ProjectWeeklyPlanDatesLineId == ProjectWeeklyPlanDatesLineId)
                .OrderByDescending(m => m.CreatedOnUtc)
                .ToListAsync();
        }

        public async Task<List<ProjectWeeklyPlanDatesLinesAssignedTo>> GetAllByProjectWeeklyPlanDatesLineId(string ProjectWeeklyPlanDatesLineId)
        {
            return await _projectWeeklyPlanDatesLinesAssignedToRepository.TableNoTracking
            .Where(x => !x.Deleted && x.ProjectWeeklyPlanDatesLineId == ProjectWeeklyPlanDatesLineId)
            .OrderByDescending(m => m.CreatedOnUtc)
            .Select(x => new ProjectWeeklyPlanDatesLinesAssignedTo
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
            }).ToListAsync();
        }

        public async Task<bool> CheckIfEmployeeIdExistsInWeeklyPlanLine(string LineId, string EmployeeId)
        {
            var query = await _projectWeeklyPlanDatesLinesAssignedToRepository.TableNoTracking.FirstOrDefaultAsync(x => !x.Deleted && x.ProjectWeeklyPlanDatesLineId == LineId && x.EmployeeId == EmployeeId);
            return query != null ? true : false;
        }
        #endregion

        #region Insert & Update & Delete
        public void InsertProjectWeeklyPlanDatesLinesAssignedTo(ProjectWeeklyPlanDatesLinesAssignedTo entity)
        {
            _projectWeeklyPlanDatesLinesAssignedToRepository.Insert(entity);
        }
        public void UpdateProjectWeeklyPlanDatesLinesAssignedTo(ProjectWeeklyPlanDatesLinesAssignedTo entity)
        {
            _projectWeeklyPlanDatesLinesAssignedToRepository.Update(entity);
        }
        public void DeleteProjectWeeklyPlanDatesLinesAssignedTo(ProjectWeeklyPlanDatesLinesAssignedTo entity)
        {
            entity.Deleted = true;
            _projectWeeklyPlanDatesLinesAssignedToRepository.Update(entity);
        }
        #endregion
    }
}
