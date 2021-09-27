using EPP.CorporatePortal.DAL.EDMX;
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPP.CorporatePortal.DAL.Service
{
    public class BaseService
    {
        public AuditTrail auditTrailService = new AuditTrail();

        public EPPCorporatePortalEntities dbEntities = new EPPCorporatePortalEntities();


    }
}
