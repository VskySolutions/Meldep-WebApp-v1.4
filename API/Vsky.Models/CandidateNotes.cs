using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class CandidateNotes : BaseEntity
    {
        public string CandidateId { get; set; }
        public string NoteId { get; set; }

        public virtual Candidate Candidates { get; set; }
        public virtual Notes Note { get; set; }
    }
}
