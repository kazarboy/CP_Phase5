using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using static EPP.CorporatePortal.Common.Enums;

namespace EPP.CorporatePortal.DAL.Service
{
    public class AuditTrail
    {
        public int LogAuditTrail(DateTime date,AuditType type, string userName, string description, string menu)
        {
            var value= new StoredProcService(userName).InsAuditTrail(date, type.ToString(),  userName,   description,   menu);
            return value;
        }
    }
}
