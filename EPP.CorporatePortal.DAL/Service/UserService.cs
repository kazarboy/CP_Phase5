using EPP.CorporatePortal.DAL.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using EPP.CorporatePortal.Common;
namespace EPP.CorporatePortal.DAL.Service
{
    public class UserService : BaseService
    {
      
        public List<Rights_Enum> GetRoleRightsEnumList(string roleName)
        {
            var ReturnRights = new List<Rights_Enum>();
            var dbRoles = dbEntities.Roles.Where(r => r.Name == roleName).FirstOrDefault();
            var listRights = dbEntities.RoleRights.Where(r => r.RoleId == dbRoles.Id).Select(r => r.RightId).ToList();

            foreach (var rights in listRights)
            {
                var right = dbEntities.Rights.Where(r => r.Id == rights.Value).FirstOrDefault();
                var rightEnum = (Rights_Enum)Enum.Parse(typeof(Rights_Enum), right.Id.ToString());
                ReturnRights.Add(rightEnum);
            }
            return ReturnRights;
        }

        public List<User> GetAllUsers()
        {
            var listUsers = new List<User>();

            var users = dbEntities.Users;
            if (users.Count() > 0)
            {
                return users.ToList();
            }
            return listUsers;
        }

       

        public List<Right> GetUserRights(string userName)
        {
            var listRightNames = new List<Right>();
            var uid = dbEntities.Users.Where(u => u.UserName == userName).Select(u => u.Id).FirstOrDefault();
            var roles = dbEntities.UserRoles.Where(r => r.UserId == uid).ToList();
            foreach (var role in roles)
            {
                var rights = dbEntities.RoleRights.Where(r => r.RoleId == role.RoleId).ToList();
                foreach (var right in rights)
                {
                    var rightEntity = dbEntities.Rights.Where(r => r.Id == right.Id);
                    listRightNames.Add(rightEntity.FirstOrDefault());

                }

            }
            return listRightNames;

        }

     
        public List<Right> GetAllRights()
        {

            var rights = dbEntities.Rights.ToList();
            return rights;
        }

        public bool IsRightInRole(int rightsID, int roleID)
        {
            var returnVal = false;
            var rolerights = dbEntities.RoleRights.Where(rr => rr.RightId == rightsID && rr.RoleId == roleID).FirstOrDefault();
            if (rolerights != null)
            {
                returnVal = true;
            }
            return returnVal;
        }

        public bool AssignRightsToRole(int roleId, int rightsId)
        {
            var response = IsValidRoleRights(roleId, rightsId);
            if (!response)
            {
                RoleRight rolerights = new RoleRight() {
                    RightId = rightsId,
                    RoleId = roleId
                };                 

                dbEntities.RoleRights.Add(rolerights);
                dbEntities.SaveChanges();
                return true;
            }
            return false;
        }
        public bool IsValidRoleRights(int roleId, int rightsId)
        {

            var rightsInfo = dbEntities.RoleRights.Where(r => r.RoleId == roleId && r.RightId == rightsId).FirstOrDefault();
            if (rightsInfo == null)
            { return false; }
            else
            { return true; }
        }

        public bool DeleteRightsToRole(int roleId, int rightsId)
        {
            var response = IsValidRoleRights(roleId, rightsId);
            if (response)
            {
                var rolerights = dbEntities.RoleRights.Where(r => r.RoleId == roleId && r.RightId == rightsId).FirstOrDefault();

                dbEntities.RoleRights.Remove(rolerights);
                dbEntities.SaveChanges();
            }
            return response;

        }
        public bool isValidUser(string userName)
        {
            if (userName != "")
            {
                var userInfo = dbEntities.Users.Where(u=>u.UserName== userName);
                if (userInfo.Count() ==0)
                { return false; }
                else
                { return true; }
            }
            else
            {
                return false;
            }
        }
        public bool DeleteUser(string userName)
        { 
            var user = dbEntities.Users.Where(u => u.UserName == userName);
            if (user.Count()>0)
            {
                dbEntities.Users.Remove(user.FirstOrDefault());
                dbEntities.SaveChanges();
                return true;
            }
            return false;
        }

        public List<User> GetUsersByCorporate(string corporateId)
        {
            var listUsers = new List<User>();
            var users = dbEntities.CorporateUsers.Where(cu => cu.CorporateSourceId == corporateId);

            foreach (var user in users)
            {
                var userObj = dbEntities.Users.Where(u => u.Id == user.UserId).FirstOrDefault();
                listUsers.Add(userObj);

            }
            return listUsers;

        }

        public void SetUserOTP(string username, string GenOTP)
        {
            var GetUserInfo = dbEntities.Users.Where(x => x.UserName == username).FirstOrDefault();
            GetUserInfo.OTPCode = GenOTP;
            GetUserInfo.GenOTPCode = DateTime.Now;
            dbEntities.SaveChanges();
        }

        public string GetOTP(string username)
        {
            var code = dbEntities.Users.Where(x => x.UserName == username).Select(y => y.OTPCode).FirstOrDefault();
            return code;
        }

        public DateTime GetGenOTP(string username)
        {
            var GenCode = dbEntities.Users.Where(x => x.UserName == username).Select(y => y.GenOTPCode).FirstOrDefault();
            DateTime RetCode = GenCode.Value;
            return RetCode;
        }

        public Tuple<string,string> GetEmailOTP(string username)
        {
            var RetEmail = dbEntities.Users.Where(x => x.UserName == username).Select(y => new { y.EmailAddress ,y.FullName}).FirstOrDefault();
            var data = Tuple.Create(RetEmail.EmailAddress,RetEmail.FullName);

            return data;
        }
    }
}
