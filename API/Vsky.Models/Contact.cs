using System;
using System.Collections;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models
{
    public class ContactUsList
    {
        public virtual ICollection<Contact> ContactUsLists { get; set; } = new List<Contact>();
        public int Total { get; set; }
    }

    public class Contact : BaseEntity
    {
        public string FullName { get; set; }

        public string Email { get; set; }

        public string PhoneNo { get; set; }

        public string MobileNo { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }
        public string Source { get; set; }
        public DateTime? ContactedDate { get; set; }
        public bool Deleted { get; set; }
    }
    public class SaveContactUs
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string MobileNo { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string RecaptchaToken { get; set; }
        public bool Deleted { get; set; }
    }
}