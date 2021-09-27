using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPP.CorporatePortal.DAL.Model
{
    [Serializable]
    public class ClaimSubmissionModel
    {
        //Member Page
        public string MemberID { get; set; }
        public string MemberNameKeyIn { get; set; }
        public string MemberIDTypeKeyIn { get; set; }
        public string MemberIDNoKeyIn { get; set; }
        public string MemberCompanyName { get; set; }
        public DateTime MemberTerminationDate { get; set; }

        //Claims/Benefits Page
        public string BenefitCodeOri { get; set; }
        public string BenefitCode { get; set; }
        public string CauseOfEvent { get; set; }
        public string Accident { get; set; }
        public string EventDescription { get; set; }
        public string DayOfEvent { get; set; }
        public string MonthOfEvent { get; set; }
        public string YearOfEvent { get; set; }

        //Policy & Bank Details Page
        public List<ClaimSubmissionPolicy> PolicyList = new List<ClaimSubmissionPolicy>();

        //Document Page
        public bool DeathOverseas { get; set; }
        public List<ClaimSubmissionDocument> DocumentList = new List<ClaimSubmissionDocument>();

        //Etc
        public bool NonEmbeddedDeathFuneral { get; set; }
    }
}
