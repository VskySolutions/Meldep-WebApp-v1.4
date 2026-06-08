using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Contacts
{
    public interface IContactService
    {
        #region Get All
        Task<IList<Contact>> GetAllContacts();
        IPagedList<Contact> GetAllContactUs(
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
         );
        #endregion

        #region Get By Id
        Task<Contact> GetContactById(string id);
        #endregion

        #region InsertContact
        void InsertContact(Contact entity);
        #endregion

        #region UpdateContact
        void UpdateContact(Contact entity);
        #endregion

        #region DeleteContact
        void DeleteContact(Contact entity);
        #endregion
    }
}