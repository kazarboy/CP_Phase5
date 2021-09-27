using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPP.CorporatePortal.DAL.Model
{
    [Serializable]
    public class ClaimSubmissionDocument
    {
        public int DocumentId { get; set; }
        public string FileName { get; set; }
        public byte[] FileBytes { get; set; }
    }
}
