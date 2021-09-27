using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPP.CorporatePortal.Common
{
    public class Enums
    {
        public enum ComparisonType
        {
             
            All, 
            Any,
        }
        public enum AuditType
        {
            Debug,  
            Error,
            Info
        }
        public enum FileUploadType
        {
            MemberListing,
            ClaimSubmission,
            Others
        }
        public enum FileDownloadType
        {
            Exception,
            Invoice,
            UploadFile,
            GeneratedFile,
            RPAException,
            CGLSException,
            CreateUser,
            CreateUserException,
            ClaimsEForm,
            ClaimsDocuments,
            Others
        }
        public enum AgentHubUserStatus
        {
            Active = 0,
            Dormant = 2,
            Disable = 10
        }
        public enum UserRole
        {
            Admin = 1,
            HR = 2,
            UIDAdmin = 4,
            Agent = 3
        }
        public enum ClaimStepsStatus
        {
            complete = 1,
            active = 2,
            disabled = 4
        }
    }
}
