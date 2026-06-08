using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.SOPTemplates
{
    public class SOPTemplateService : ISOPTemplateService
    {
        #region Define services

        private readonly IRepository<SOPTemplate> _sOPTemplateRepository;
        public SOPTemplateService(IRepository<SOPTemplate> sOPTemplateRepository)
        {
            _sOPTemplateRepository = sOPTemplateRepository;
        }
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }

        #endregion

        #region List

        public IPagedList<SOPTemplate> GetAllSOPTemplates(string searchText, string siteId, string name, bool isActive, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue) 
        { 
            var query = _sOPTemplateRepository.TableNoTracking.Where(x => !x.Deleted && x.IsActive == isActive && x.SiteId == siteId);

            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(m => m.Name.ToLower().Contains(searchText.ToLower()) || m.Description.ToLower().Contains(searchText.ToLower()));

            if (!string.IsNullOrEmpty(name))
                query = query.Where(x => x.Name.ToLower().Contains(name));

            //if(isActive)
            //    query = query.Where(x => x.IsActive == isActive);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
                query = query.OrderByDescending(x => x.CreatedOnUtc);

            query = query.Select(x => new SOPTemplate
            {
                Id = x.Id,
                SiteId = x.SiteId,
                Name = x.Name,
                Description = x.Description,
                Version = x.Version,
                IsActive = x.IsActive,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                    }
                },
                UpdatedById = x.UpdatedById,
                UpdatedOnUtc = x.UpdatedOnUtc,
                UpdatedBy = new ApplicationUser
                {
                    Id = x.UpdatedBy.Id,
                    Person = new Person
                    {
                        Id = x.UpdatedBy.PersonId,
                        FullName = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.LastName
                    }
                },
                Deleted = x.Deleted
            });

            var list = new PagedList<SOPTemplate>(query, page, pageSize);
            return list;
        }

        #endregion
        public async Task<List<SOPTemplate>> GetSOPTemplatesListForDropdown(string SiteId)
        {
            var query = _sOPTemplateRepository.TableNoTracking.Where(x => !x.Deleted && x.IsActive && x.SiteId == SiteId);

            query = query.Select(x => new SOPTemplate
            {
                Id = x.Id,
                Name = x.Name
            });

            var list = await query.OrderBy(m => m.Name).ToListAsync();
            return list;
        }

        #region Get By Id

        public SOPTemplate GetSOPTemplateById(string siteId, string Id)
        {
            var query = _sOPTemplateRepository.TableNoTracking.FirstOrDefault(x => !x.Deleted && x.SiteId == siteId && x.Id == Id);
            return query ?? null;
        }

        public SOPTemplate GetSOPTemplateByIdInDetail(string siteId, string Id)
        {
            var query = _sOPTemplateRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.Id == Id);

            query = query.Select(x => new SOPTemplate
            {
                Id = x.Id,
                SiteId = x.SiteId,
                Name = x.Name,
                SortOrder = x.SortOrder,
                Description = x.Description,
                Version = x.Version,
                IsActive = x.IsActive,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                    }
                },
                UpdatedById = x.UpdatedById,
                UpdatedOnUtc = x.UpdatedOnUtc,
                UpdatedBy = new ApplicationUser
                {
                    Id = x.UpdatedBy.Id,
                    Person = new Person
                    {
                        Id = x.UpdatedBy.PersonId,
                        FullName = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.LastName
                    }
                },
                Deleted = x.Deleted,
                SOPTemplateSections = x.SOPTemplateSections.Where(x => !x.Deleted).OrderBy(o => o.SortOrder).Select(y => new SOPTemplateSection { 
                    Id = y.Id,
                    TemplateId = y.TemplateId,
                    Name = y.Name,
                    Description = y.Description,
                    SortOrder = y.SortOrder,
                    CreatedById = y.CreatedById,
                    CreatedOnUtc = y.CreatedOnUtc,
                    CreatedBy = new ApplicationUser
                    {
                        Id = y.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = y.CreatedBy.PersonId,
                            FullName = y.CreatedBy.Person.FirstName + " " + y.CreatedBy.Person.LastName
                        }
                    },
                    UpdatedById = y.UpdatedById,
                    UpdatedOnUtc = y.UpdatedOnUtc,
                    UpdatedBy = new ApplicationUser
                    {
                        Id = y.UpdatedBy.Id,
                        Person = new Person
                        {
                            Id = y.UpdatedBy.PersonId,
                            FullName = y.UpdatedBy.Person.FirstName + " " + y.UpdatedBy.Person.LastName
                        }
                    },
                    Deleted = y.Deleted,
                    SOPTemplateSectionItems = y.SOPTemplateSectionItems.Where(x => !x.Deleted).OrderBy(o => o.SortOrder).Select(z => new SOPTemplateSectionItems { 
                        Id = z.Id,
                        TemplateId = z.TemplateId,
                        SectionId = z.SectionId,
                        Name = z.Name,
                        Description = z.Description,
                        InputTypeId = z.InputTypeId,
                        InputType = new DropDown
                        {
                            Id = z.InputTypeId,
                            DropDownText = z.InputType.DropDownText,
                            DropDownValue = z.InputType.DropDownValue,
                        },
                        IsMandatory = z.IsMandatory,
                        IsRequiredEvidence = z.IsRequiredEvidence,
                        ValidationJson = z.ValidationJson,
                        SortOrder = z.SortOrder,
                        CreatedById = z.CreatedById,
                        CreatedOnUtc = z.CreatedOnUtc,
                        CreatedBy = new ApplicationUser
                        {
                            Id = z.CreatedBy.Id,
                            Person = new Person
                            {
                                Id = z.CreatedBy.PersonId,
                                FullName = z.CreatedBy.Person.FirstName + " " + z.CreatedBy.Person.LastName
                            }
                        },
                        UpdatedById = z.UpdatedById,
                        UpdatedOnUtc = z.UpdatedOnUtc,
                        UpdatedBy = new ApplicationUser
                        {
                            Id = z.UpdatedBy.Id,
                            Person = new Person
                            {
                                Id = z.UpdatedBy.PersonId,
                                FullName = z.UpdatedBy.Person.FirstName + " " + z.UpdatedBy.Person.LastName
                            }
                        },
                    }).ToList(),
                }).ToList()
            });

            return query.FirstOrDefault();
        }

        #endregion

        #region Insert Update Delete
        public void InsertSOPTemplate(SOPTemplate entity)
        {
            _sOPTemplateRepository.Insert(entity);
        }

        public void UpdateSOPTemplate(SOPTemplate entity)
        {
            _sOPTemplateRepository.Update(entity);
        }

        public void DeleteSOPTemplate(SOPTemplate entity)
        {
            entity.Deleted = true;
            _sOPTemplateRepository.Update(entity);
        }
        #endregion
    }
}