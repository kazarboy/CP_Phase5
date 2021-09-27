using System;
namespace EPP.CorporatePortal.DAL.Entity
{
    [Serializable]
    public class Policy : BaseEntity
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public string Description { get; set; }

        public PolicyDetail Details { get; set; }
    }

    public class PolicyDetail
    {
        public int DetailId { get; set; }         
        public DateTime? SrcCreated { get; set; }
        public string agentCode { get; set; }
        public string PrdComp { get; set; }
        public string PolNum { get; set; }
        public string PropNum { get; set; }
        public string Category { get; set; }
        public string ClsCode { get; set; }
        public string Cls { get; set; }
        public string PrdCode { get; set; }
        public string Prd { get; set; }
        public string Status { get; set; }
        public string SubStatus { get; set; }
        public decimal? TtlPrem { get; set; }
        public decimal? TtlSumIns { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }
        public DateTime? EffDate { get; set; }
        public DateTime? IssDate { get; set; }
        public DateTime? TermDate { get; set; }
        public DateTime? ExpDate { get; set; }
    }
}
