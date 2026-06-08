using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Contacts
{
    public class ContactService : IContactService
    {
        #region Fields

        private readonly IRepository<Contact> _contactRepository;

        #endregion

        #region Ctor

        public ContactService(IRepository<Contact> contactRepository)
        {
            _contactRepository = contactRepository;
        }

        #endregion

        #region Private Methods

        private static string GetOrderBy(string orderBy)
        {
            if (orderBy == "stateProvinceId")
            {
                orderBy = "StateProvince.Name";
            }

            return orderBy;
        }

        #endregion

        #region Public Methods

        public async Task<IList<Contact>> GetAllContacts()
        {
            var query = _contactRepository.Table;
            query = query.Where(x => !x.Deleted);
            query = query.OrderBy(x => x.FullName);
            query = query.Select(x => new Contact
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                MobileNo = x.MobileNo
            });

            var list = await query.ToListAsync();
            return list;
        }
        public IPagedList<Contact> GetAllContactUs(
            string SiteId,
            string SearchText,
            string fullName,
            string email,
            string phoneNo,
            string title,
            string source,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue
        )
        {
            var query = _contactRepository.TableNoTracking.Where(x => !x.Deleted);

            // custom filter
            if (!string.IsNullOrWhiteSpace(fullName)) query = query.Where(x => x.FullName.ToLower().Contains(fullName.ToLower()));

            if (!string.IsNullOrWhiteSpace(email)) query = query.Where(x => x.Email.ToLower().Contains(email.ToLower()));

            if (!string.IsNullOrWhiteSpace(phoneNo)) query = query.Where(x => x.PhoneNo.Contains(phoneNo));

            if (!string.IsNullOrWhiteSpace(title)) query = query.Where(x => x.Title.Contains(title));

            if (!string.IsNullOrWhiteSpace(source)) query = query.Where(x => x.Source.Contains(source));

            // static search
            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                SearchText = SearchText.ToLower();

                query = query.Where(m =>
                    m.FullName.ToLower().Contains(SearchText) ||
                    m.Email.ToLower().Contains(SearchText) ||
                    m.PhoneNo.ToLower().Contains(SearchText) ||
                    m.MobileNo.ToLower().Contains(SearchText) ||
                    m.Title.ToLower().Contains(SearchText) ||
                    m.Message.ToLower().Contains(SearchText) ||
                    m.Source.ToLower().Contains(SearchText) ||
                    m.ContactedDate.HasValue && m.ContactedDate.Value.Date == parsedDate.Date
                );
            }

            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderBy(x => x.ContactedDate);
            }

            query = query.Select(x => new Contact
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                PhoneNo = x.PhoneNo,
                MobileNo = x.MobileNo,
                Title = x.Title,
                Message = x.Message,
                Source = x.Source,
                ContactedDate = x.ContactedDate
            });

            var list = new PagedList<Contact>(query, page, pageSize);
            return list;
        }
        #endregion

        #region Get By Id
        public async Task<Contact> GetContactById(string id)
        {
            var query = _contactRepository.Table;
            query = query.Where(x => x.Id == id);
            query = query.Where(x => !x.Deleted);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertContact
        public void InsertContact(Contact entity)
        {
            _contactRepository.Insert(entity);
        }
        #endregion

        #region UpdateContact
        public void UpdateContact(Contact entity)
        {
            _contactRepository.Update(entity);
        }
        #endregion

        #region DeleteContact
        public void DeleteContact(Contact entity)
        {
            entity.Deleted = true;

            _contactRepository.Update(entity);
        }
        #endregion
    }
}