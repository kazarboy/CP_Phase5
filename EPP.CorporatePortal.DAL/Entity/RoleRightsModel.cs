using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPP.CorporatePortal.DAL.Entity
{
    public class RoleRightsModel : BaseEntity
    {
        public int RoleRightsID { get; set; }
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public int RightID { get; set; }
        public string RightsName { get; set; }
        public bool IsChecked { get; set; }
    }
}
