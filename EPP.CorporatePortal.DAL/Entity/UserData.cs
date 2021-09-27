using System;
using System.Collections.Generic;

namespace EPP.CorporatePortal.DAL.Entity
{
    public class UserData : BaseEntity
    {
        public string AccountStatus { get; set; }
        public string FullName { get; set; }
        public string AccountCode { get; set; }
        public string EmailAddress { get; set; }
        public string MobilePhone { get; set; }
        public string HomePhone { get; set; }
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
        public List<Products> Products { get; set; }
        public List<Managers> Managers { get; set; }
        public string[] Roles { get; set; }
        public string[] Rights { get; set; }
    }
    public class Managers
    {
        public string Ranking { get; set; }
        public string PFNumber { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
    }
    public class Products
    {
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string Abbreviation { get; set; }
        public string Result { get; set; }
        public DateTime TakenDate { get; set; }

    }
}