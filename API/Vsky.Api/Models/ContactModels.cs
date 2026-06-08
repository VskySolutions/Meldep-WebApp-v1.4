using System;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record ContactModel : BaseEntityModel
    {
        public string FullName { get; set; }

        public string Email { get; set; }

        public string PhoneNo { get; set; }

        public string MobileNo { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public DateTime? ContactedDate { get; set; }

        public bool Deleted { get; set; }

    }

    public record ContactSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string Title { get; set; }
        public string Source { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public record ContactListModel : BasePagedListModel<ContactModel>
    {
    }
}