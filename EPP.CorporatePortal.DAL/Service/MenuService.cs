using EPP.CorporatePortal.DAL.EDMX;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EPP.CorporatePortal.DAL.Service
{
    public class MenuService : BaseService
    {
        public List<DataRow> GetMenus(string userName)
        {
            var serv = new StoredProcService(userName);             
            var rights = serv.GetUserRights(userName);
            var menuList = new List<DataRow>();
            if (rights != null)
            {
                try
                {
                    foreach (DataRow right in rights.Rows)
                    {
                        var menus = serv.GetMenuRights(Convert.ToInt32(right["Id"]));
                        foreach (DataRow menu in menus.Rows)
                        {

                            menuList.Add(menu);
                        }
                    }
                }
                catch (Exception ex)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in GetMenuItems(username). Error : "+ ex.Message, "Menu"); 
                }
            }
            return menuList;
        }
    }
}
