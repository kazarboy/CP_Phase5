using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPP.CorporatePortal.DAL.Model
{
    [Serializable]
    public class ClaimSubmissionDocEFTB
    {
        public string ContractNo { get; set; }
        public string ContractHolderName { get; set; }
        public string SubsidiaryName { get; set; }
        public string LifeAssuredName { get; set; }
        public string LifeAssuredID { get; set; }
        public string SubmitterName { get; set; }
        public string SubmitterContactNo { get; set; }
        public string SubmitterEmail { get; set; }
        public string BankName { get; set; }
        public string BankAccNo { get; set; }
        public string BankBranchName { get; set; }
        public string BankROC { get; set; }
        public string ClaimType { get; set; }
        public string DateOfEvent { get; set; }
        public string CauseOfEvent { get; set; }
        public string UploadedDocList { get; set; }
        public string SigClaimantName { get; set; }
        public string SigSubmissionDate { get; set; }
        public string PortalClaimNo { get; set; }
        public string EtiqaLogo { get; set; }
        public string EtiqaFooter { get; set; }
        public string AccountHolderName { get; set; }
        public string AccountHolderNRIC { get; set; }
    }
}
