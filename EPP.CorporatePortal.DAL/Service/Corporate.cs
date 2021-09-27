using EPP.CorporatePortal.DAL.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPP.CorporatePortal.DAL.Service
{
    public class CorporateService : BaseService
    {
        /// <summary>
        /// Gets the Corporate by Name
        /// </summary>
        /// <param name="corporateName"></param>
        /// <returns>Corporate Object</returns>
        public Corporate GetCorporateByName(string corporateName)
        {
            var corporate = dbEntities.Corporates.Where(u => u.Name == corporateName);
            return corporate.FirstOrDefault();
        }

        /// <summary>
        /// Gets the Corporate by Id
        /// </summary>
        /// <param name="corporateId"></param>
        /// <returns>Corporate Object</returns>
        public Corporate GetCorporateById(string corporateId)
        {
            var corporate = dbEntities.Corporates.Where(u => u.SourceId == corporateId);
            return corporate.FirstOrDefault();
        }

        /// <summary>
        /// Gets the Corporate by Biz Reg No
        /// </summary>
        /// <param name="bizRegNo"></param>
        /// <returns>Corporate Object</returns>
        public Corporate GetCorporateByBusinessRegistrationNo(string bizRegNo)
        {
            var corporate = dbEntities.Corporates.Where(u => u.SourceId == bizRegNo);
            return corporate.FirstOrDefault();
        }

        /// <summary>
        /// Gets the Biz Reg No for the corporate
        /// </summary>
        /// <param name="parentName"></param>
        /// <returns> Biz Reg No</returns>
        public string GetBusinessRegistrationNo(string corporateName)
        {
            var corporate = dbEntities.Corporates.Where(u => u.Name == corporateName);
            if (corporate.Count() > 0)
            {
                return corporate.FirstOrDefault().SourceId;
            }
            return "";
        }


        /// <summary>
        /// Gets the subsidaries for the corporate id
        /// </summary>
        /// <param name="corporateParentId"></param>
        /// <returns>List of subsidaries</returns>
        public List<Corporate> GetCorporateSubsidaries(string corporateParentId)
        {
            var subsidaries = dbEntities.Corporates.Where(u => u.ParentId == corporateParentId);
            return subsidaries.ToList();
        }

        /// <summary>
        /// Gets the subsidaries for the Biz Reg No
        /// </summary>
        /// <param name="bizRegNo"></param>
        /// <returns>List of subsidaries</returns>
        public List<Corporate> GetCorporateSubsidariesByBizRegNo(string bizRegNo)
        {
            var subsidaries = dbEntities.Corporates.Where(c => c.SourceId == bizRegNo);
            return subsidaries.ToList();
        }

        /// <summary>
        /// Get the parent corporate by Biz Reg No
        /// </summary>
        /// <param name="bizRegNo"></param>
        /// <returns>Parent Id</returns>
        public string GetParentId(string bizRegNo)
        {
            var serv = GetCorporateByBusinessRegistrationNo(bizRegNo);
            if (serv != null)
            {
                return serv.ParentId;
            }
            return "";
        }
        public List<Corporate> GetCorporateByUser(int userId)
        {
            var listCorporates = new List<Corporate>();

            var corpUsers = dbEntities.CorporateUsers.Where(cu => cu.UserId == userId);
            foreach (var corpUser in corpUsers)
            {
                var corporateObj = dbEntities.Corporates.Where(c => c.SourceId == corpUser.CorporateSourceId).FirstOrDefault();
                listCorporates.Add(corporateObj);
            }

            return listCorporates;
        }

        

    }
}
