using EPP.CorporatePortal.DAL.EDMX;
using System.Collections.Generic;
using System.Linq;

namespace EPP.CorporatePortal.DAL.Service
{
    public class PolicyService:BaseService
    {
        public List<Policy> GetAllPolicy()
        {
            var policies = dbEntities.Policies;
            return policies.ToList();
        }

        public List<Policy> GetPolicyByContractNo(string contractNo)
        {
            var policies = dbEntities.Policies.Where(p=>p.ContractNo==contractNo);
            return policies.ToList();
        }

        public List<Policy> GetPolicyByContractHolder(string contractHolder)
        {            
            var policies = dbEntities.Policies.Where(po => po.ContractHolder == contractHolder).ToList();
            return policies;
        }
        //public List<Policy> GetPolicyByCoporateId(string corporateId)
        //{
        //    var listPolicies = new List<Policy>();
        //    var corporatePolicies = dbEntities.CorporatePolicies.Where(cp => cp.CorporateSourceId == corporateId).ToList();

        //    foreach (var cp in corporatePolicies)
        //    {
        //        var policy = dbEntities.Policies.Where(p => p.SourceId == cp.PolicySourceId).FirstOrDefault();
        //        listPolicies.Add(policy);
        //    }
        //    return listPolicies;
        //}

        //public List<Policy> GetPolicyByCoporateName(string corporateName)
        //{
        //    var listPolicies = new List<Policy>();
        //    var corporatePolicies = dbEntities.CorporatePolicies.Where(cp => cp.Corporate.Name == corporateName);

        //    foreach (var cp in corporatePolicies)
        //    {
        //        var policy = dbEntities.Policies.Where(p => p.SourceId == cp.PolicySourceId).FirstOrDefault();
        //        listPolicies.Add(policy);
        //    }
        //    return listPolicies;
        //}

        //public List<Policy> GetPolicyByCoporateBizRegNo(string corporateBizRegNo)
        //{
        //    var listPolicies = new List<Policy>();
        //    var corporatePolicies = dbEntities.CorporatePolicies.Where(cp => cp.Corporate.SourceId == corporateBizRegNo);

        //    foreach (var cp in corporatePolicies)
        //    {
        //        var policy = dbEntities.Policies.Where(p => p.SourceId == cp.PolicySourceId).FirstOrDefault();
        //        listPolicies.Add(policy);
        //    }
        //    return listPolicies;
        //}
    }
}
