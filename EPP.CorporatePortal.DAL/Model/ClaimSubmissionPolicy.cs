using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPP.CorporatePortal.DAL.Model
{
    [Serializable]
    public class ClaimSubmissionPolicy
    {
        public string PolicyID { get; set; }
        public string AccountHolderName { get; set; }
        public string IDType { get; set; }
        public string IDNo { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string BankAccountNo { get; set; }
        public string ContactNo { get; set; }
        public string EmailAddress { get; set; }
        public string BankROC { get; set; }
        public int MemberClaimID { get; set; }
    }
}
