//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EPP.CorporatePortal.DAL.EDMX
{
    using System;
    using System.Collections.Generic;
    
    public partial class CorporatePolicy
    {
        public int Id { get; set; }
        public string CorporateSourceId { get; set; }
        public string PolicySourceId { get; set; }
        public string PersonInCharge { get; set; }
        public string Status { get; set; }
        public Nullable<int> InsuredNo { get; set; }
        public string AccountNo { get; set; }
        public string InsurableInterest { get; set; }
        public Nullable<int> CorpId { get; set; }
    }
}
