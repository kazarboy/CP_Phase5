using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPP.CorporatePortal.DAL.Model
{
    [Serializable]
    public class UserIdentityModel
    {
        
        public string UserCorporates { get; set; }
        public string AccountStatus { get; set; }
        public string PrincipalName { get; set; }
        public string Name { get; set; }
        public string AccountCode { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string OfficePhone { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string AddressCity { get; set; }
        public string AddressState { get; set; }
        public string AddressCountry { get; set; }
        public string AddressPostcode { get; set; }
        public string BranchCode { get; set; }
        public string BusinessRegistrationNo { get; set; }
        public string AgentCodes { get; set; }
        public string ParentCorporate { get; set; }
        public string ParentBizRegNo { get; set; }
        public string IsOwner { get; set; }
        public string Role { get; set; }
        public string UCorpId { get; set; }
    }
}
