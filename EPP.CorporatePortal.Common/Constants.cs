using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPP.CorporatePortal.Common
{
    public static class Constants
    {
        public class Application
        {
            //Application Status
            public struct PortalStatus
            {
                public const string NewSubmission = "New Submission";
                public const string InProgress = "In Progress";
                public const string PendingRequirement = "Pending Requirement";
                public const string PendingApproval = "Pending Approval";
                public const string Approved = "Approved";
                public const string Paid = "Paid";
                public const string Rejected = "Rejected";
                public const string Closed = "Closed";
            }
            public struct CGLSStatus
            {
                public const string InProgress = "In Progress";
                public const string PendingApproval = "Pending Approval";
                public const string Approved = "Approved";
                public const string FullIntimationSentToFin = "Full Intimation sent to Fin";
                public const string IntimationSentToFin = "Intimation sent to Fin";
                public const string FullyPaid = "Fully Paid";
                public const string Paid = "Paid";
                public const string PaidHoldPayment = "Paid- Hold Payment";
                public const string PaidNoPayment = "Paid- No Payment";
                public const string PaidStopPayment = "Paid- Stop Payment";
                public const string Rejected = "Rejected";
                public const string Closed = "Closed";
                public const string Deleted = "Deleted";
                public const string Voided = "Voided";
            }
        }
    }
}
