using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerBI.Api.Models;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.Candidates
{
    public class CandidateService : ICandidateService
    {
        #region Define Services
        private readonly IRepository<Candidate> _CandidateRepository;
        private readonly IRepository<Person> _PersonRepository;
        private readonly IRepository<Picture> _pictureRepository;
        private readonly IRepository<Notes> _notesRepository;
        private readonly IRepository<CandidateActivities> _candidateActivityLogsRepository;
        private readonly IRepository<CandidateFeedback> _candidateFeedbackRepository;
        #endregion

        #region Services Initializations
        public CandidateService(
            IRepository<Candidate> CandidateRepository,
            IRepository<Person> PersonRepository,
            IRepository<Picture> Picturerepository,
            IRepository<Notes> notesRepository,
            IRepository<CandidateActivities> candidateActivityLogsRepository,
            IRepository<CandidateFeedback> candidateFeedbackRepository
            )
        {
            _CandidateRepository = CandidateRepository;
            _PersonRepository = PersonRepository;
            _pictureRepository = Picturerepository;
            _notesRepository = notesRepository;
            _candidateActivityLogsRepository = candidateActivityLogsRepository;
            _candidateFeedbackRepository = candidateFeedbackRepository;
        }
        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetAllCandidateList
        public IPagedList<Candidate> GetAllCandidateList(string SiteId, string SearchText, string fullName, string emailAddress, string mobileNumber, List<string> appliedWorkLocationId, List<string> jobId, DateTime? fromDate, DateTime? toDate, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _CandidateRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                fullName = fullName.Trim().ToLower();
                query = query.Where(x => (x.Person.FirstName.ToLower() + " " + x.Person.LastName.ToLower()).Contains(fullName));
            }

            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                emailAddress = emailAddress.Trim().ToLower();
                query = query.Where(x => x.Person.PrimaryEmailAddress.ToLower().Contains(emailAddress));
            }

            if (!string.IsNullOrWhiteSpace(mobileNumber))
            {
                mobileNumber = mobileNumber.Trim().ToLower();
                query = query.Where(x => x.Person.PrimaryPhoneNumber.ToLower().Contains(mobileNumber));
            }

            if (appliedWorkLocationId != null && appliedWorkLocationId.Any())
                query = query.Where(x => appliedWorkLocationId.Contains(x.AppliedWorkLocationId));

            if (jobId != null && jobId.Any())
                query = query.Where(x => jobId.Contains(x.JobId));

            //Search by FromDate and Todate
            if (fromDate != null)
                query = query.Where(x => x.JobApplyDate >= fromDate);

            if (toDate != null)
                query = query.Where(a => a.JobApplyDate <= toDate);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                string orderBy;
                if (sortBy == "person.fullName")
                {
                    orderBy = $"{GetOrderBy("Person.FirstName")} {(descending ? "desc" : "asc")}, {GetOrderBy("Person.LastName")} {(descending ? "desc" : "asc")}";
                }
                else
                {
                     orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                }
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                    m.SearchNumber.ToString().Contains(SearchText) ||
                   (m.Person.FirstName + " " + m.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    m.Person.PrimaryEmailAddress.ToLower().Contains(SearchText.ToLower()) ||
                    m.Person.PrimaryPhoneNumber.ToLower().Contains(SearchText.ToLower()) ||
                    m.Status.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                    m.AppliedWorkLocations.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                    m.Job.JobTitle.ToLower().Contains(SearchText.ToLower()) ||
                    m.Source.ToLower().Contains(SearchText.ToLower()) ||
                    m.UpdatedOnUtc.Value.Date == parsedDate ||
                    m.JobApplyDate.Value.Date == parsedDate

                );
            }
             query = query.Select(x => new Candidate
            {
                Id = x.Id,
                SearchNumber = x.SearchNumber,
                JobApplyDate = x.JobApplyDate,
                ExpectedSalaryFrom = x.ExpectedSalaryFrom,
                ExpectedSalaryTo = x.ExpectedSalaryTo,
                CandidateResumeFileId = x.CandidateResumeFileId,
                UpdatedOnUtc = x.UpdatedOnUtc,
                Source = x.Source,
                Job = new JobCreate
                {
                    Id = x.Job.Id,
                    JobTitle = x.Job.JobTitle
                },
                Person = new Person
                {
                    Id = x.Person.Id,
                    FirstName = x.Person.FirstName,
                    MiddleName = x.Person.MiddleName,
                    LastName = x.Person.LastName,
                    PrimaryPhoneNumber = x.Person.PrimaryPhoneNumber,
                    PrimaryEmailAddress = x.Person.PrimaryEmailAddress,
                    FullName = x.Person.FirstName + " " + x.Person.LastName
                },
               AppliedWorkLocations = new DropDown
               {
                   Id = x.AppliedWorkLocations.Id,
                   DropDownValue = x.AppliedWorkLocations.DropDownValue
               },
               AvailabilityWorks = new DropDown
               {
                   Id= x.AvailabilityWorks.Id,
                   DropDownValue= x.AvailabilityWorks.DropDownValue
               },
               Status = new DropDown
               {
                   Id = x.Status.Id,
                   DropDownValue = x.Status.DropDownValue
               },
                CandidateDepartments = x.CandidateDepartments.Select(p => new CandidateDepartments
                {
                    Id = p.Id,
                    DepartmentsId = p.DepartmentsId,
                    //CandidateId = p.CandidateId,
                    Departments = new Department
                    {
                        Id = p.Departments.Id,
                        Name = p.Departments.Name
                    }
                }).ToList(),
                 CandidateNotesCount = _notesRepository.TableNoTracking.Where(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "Candidate").Count(),
                 CandidateActivitiesCount = _candidateActivityLogsRepository.TableNoTracking.Where(a => !a.Deleted && a.CandidateId == x.Id).Count(),
                 CandidateFeedbackCount = _candidateFeedbackRepository.TableNoTracking.Where(a => !a.Deleted && a.CandidateId == x.Id).Count(),
             });

            var list = new PagedList<Candidate>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetById
        public async Task<Candidate> GetById(string id)
        {
            var query = _CandidateRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertCandidate
        public void InsertCandidate(Candidate entity)
        {
            _CandidateRepository.Insert(entity);
        }
        #endregion

        #region UpdateCandidate
        public void UpdateCandidate(Candidate entity)
        {
            _CandidateRepository.Update(entity);
        }
        #endregion

        #region DeleteCandidate
        public void DeleteCandidate(Candidate entity)
        {
            entity.Deleted = true;
            _CandidateRepository.Update(entity);
        }
        #endregion

        #region InsertPicture
        public void InsertPicture(Picture entity)
        {
            _pictureRepository.Insert(entity);
        }
        #endregion

        #region UpdatePicture
        public void UpdatePicture(Picture entity)
        {
            _pictureRepository.Update(entity);
        }
        #endregion

        #region Get Candidate details by id
        public async Task<Candidate> GetCandidateDetailsById(string id)
        {
            var query = _CandidateRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            query = query.Select(x => new Candidate
            {
                Id = x.Id,
                SearchNumber = x.SearchNumber,
                Source = x.Source,
                JobApplyDate = x.JobApplyDate,
                ReferenceName = x.ReferenceName,
                Qualification = x.Qualification,
                ExperienceYears = x.ExperienceYears,
                ExperienceMonths = x.ExperienceMonths,
                ExpectedSalaryFrom = x.ExpectedSalaryFrom,
                ExpectedSalaryTo = x.ExpectedSalaryTo,
                JobId = x.JobId,
                PersonId = x.PersonId,
                SiteId = x.SiteId,
                AppliedWorkLocationId = x.AppliedWorkLocationId,
                IsTransportration = x.IsTransportration,
                IsReadyToRelocate = x.IsReadyToRelocate,
                IsCandidateOwnSystem = x.IsCandidateOwnSystem,
                IsExperienced = x.IsExperienced,
                EnglishFluencyId = x.EnglishFluencyId,
                RecruiterId = x.RecruiterId,
                AvailabilityWorkId = x.AvailabilityWorkId,
                CandidateResumeFileId = x.CandidateResumeFileId,
                ExperienceDetails = x.ExperienceDetails,
                DepartmentId = x.DepartmentId,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                Job = new JobCreate
                {
                    Id = x.Job.Id,
                    JobTitle = x.Job.JobTitle
                },
                Person = new Person
                {
                    Id = x.Person.Id,
                    FirstName = x.Person.FirstName,
                    MiddleName = x.Person.MiddleName,
                    LastName = x.Person.LastName,
                    PrimaryPhoneNumber = x.Person.PrimaryPhoneNumber,
                    PrimaryEmailAddress = x.Person.PrimaryEmailAddress,
                    FullName = x.Person.FirstName + " " + x.Person.MiddleName + " " + x.Person.LastName,
                },
                Address = new Address
                {
                    Id = x.Address.Id,
                    AddressLine1 = x.Address.AddressLine1,
                    AddressLine2 = x.Address.AddressLine2,
                    CountryId = x.Address.CountryId,
                    StateProvinceId = x.Address.StateProvinceId,
                    City = x.Address.City,
                    ZipCode = x.Address.ZipCode,
                    AddressStateProvince = new StateProvince
                    {
                        Id = x.Address.AddressStateProvince.Id,
                        Name = x.Address.AddressStateProvince.Name
                    },
                    AddressCountry = new Country
                    {
                        Id = x.Address.AddressCountry.Id,
                        Name = x.Address.AddressCountry.Name
                    }
                },
                EnglishFluencies = new DropDown
                {
                    Id = x.EnglishFluencies.Id,
                    DropDownValue = x.EnglishFluencies.DropDownValue
                },
                Employee = new Employee
                {
                    Id = x.Employee.Id,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FirstName = x.Employee.Person.FirstName,
                        MiddleName = x.Employee.Person.MiddleName,
                        LastName = x.Employee.Person.LastName,
                        FullName = x.Employee.Person.FirstName + "" + x.Employee.Person.LastName
                    }
                },
                AppliedWorkLocations = new DropDown
                {
                    Id = x.AppliedWorkLocations.Id,
                    DropDownValue = x.AppliedWorkLocations.DropDownValue
                },
                AvailabilityWorks = new DropDown
                {
                    Id = x.AvailabilityWorks.Id,
                    DropDownValue = x.AvailabilityWorks.DropDownValue
                },
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                File = new Picture
                {
                    Id = x.File.Id,
                    VirtualPath = x.File.VirtualPath,
                    SeoFilename = x.File.SeoFilename
                },
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                    }
                },
                UpdatedBy = new ApplicationUser
                {
                    Id = x.UpdatedBy.Id,
                    Person = new Person
                    {
                        Id = x.UpdatedBy.PersonId,
                        FullName = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.LastName
                    }
                },
                CandidateDepartments = x.CandidateDepartments.Select(p => new CandidateDepartments
                {
                    Id = p.Id,
                    DepartmentsId = p.DepartmentsId,
                    //CandidateId = p.CandidateId,
                    Departments = new Department
                    {
                        Id = p.Departments.Id,
                        Name = p.Departments.Name
                    }
                }).ToList(),
                CandidateNotes = x.CandidateNotes.Select(n => new CandidateNotes
                {
                    Id = n.Id,
                    //CandidateId = n.CandidateId,
                    NoteId = n.NoteId,
                    Note = new Notes
                    {
                        Id = n.Note.Id,
                        Note = n.Note.Note
                    }

                }).ToList()
            });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion


        #region GetCandidateByPersonId
        public async Task<Candidate> GetCandidateByPersonId(string personId)
        {
            return await _CandidateRepository.TableNoTracking .FirstOrDefaultAsync(x => x.PersonId == personId);
        }
        #endregion
    }
}
