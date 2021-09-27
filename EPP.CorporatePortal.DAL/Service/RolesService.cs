using EPP.CorporatePortal.DAL.EDMX;
using System.Collections.Generic;
using System.Linq;

namespace EPP.CorporatePortal.DAL.Service
{
    public class RolesService : BaseService
    {
        public List<string> GetUserRoles(string userName)
        {
            var listRoleNames = new List<string>();
            var uid = dbEntities.Users.Where(u => u.UserName == userName).Select(u => u.Id).FirstOrDefault();
            var roles = dbEntities.UserRoles.Where(r => r.UserId == uid);
            foreach (var roleId in roles.Select(r => r.RoleId))
            {
                var roleName = dbEntities.Roles.Where(r => r.Id == roleId).Select(r => r.Name).FirstOrDefault();
                listRoleNames.Add(roleName);
            }
            return listRoleNames;
        }

        public List<Role> GetUserRolesObject(string userName)
        {
            var listRoles = new List<Role>();
            var uid = dbEntities.Users.Where(u => u.UserName == userName).Select(u => u.Id).FirstOrDefault();
            var roles = dbEntities.UserRoles.Where(r => r.UserId == uid);
            foreach (var roleId in roles.Select(r => r.RoleId))
            {
                var role = dbEntities.Roles.Where(r => r.Id == roleId).FirstOrDefault();
                listRoles.Add(role);
            }
            return listRoles;
        }

        public List<Role> GetAllRoles()
        {
            var listRoles = new List<Role>();

            var roles = dbEntities.Roles.Where(r => r.IsActive);
            if (roles.Count() > 0)
            {
                return roles.ToList();
            }
            return listRoles;
        }

        public bool IsUserInRole(string userName, string role)
        {
            var response = false;
            var user = GetUsersInRole(role).Where(u => u == userName);
            if (user.Any())
            {
                response = true;
            }
            else
            {
                response = false;
            }
            return response;
        }

        public string[] GetUsersInRole(string roleName)
        {
            var response = new List<string>();
            var role = dbEntities.Roles.Where(r => r.Name == roleName).FirstOrDefault();

            var userRoles = dbEntities.UserRoles.Where(ur => ur.RoleId == role.Id).ToList();
            foreach (var userRole in userRoles)
            {
                var user = dbEntities.Users.Where(r => r.Id == userRole.UserId).FirstOrDefault();
                response.Add(user.UserName);
            }
            return response.ToArray();
        }

        public bool AssignUserToRole(string userName, string roleName)
        {
            var response = false;
            if (!IsUserInRole(userName, roleName))
            {
                var role = dbEntities.Roles.Where(r => r.Name == roleName).FirstOrDefault();
                var user = dbEntities.Users.Where(u => u.UserName == userName).FirstOrDefault();
                var userRole = new UserRole() {
                    RoleId = role.Id,
                    UserId=user.Id
                };              
                
                dbEntities.UserRoles.Add(userRole);
                dbEntities.SaveChanges();

                response = true;
            }
            return response;

        }
        public bool DeleteUserToRole(string userName, string roleName)
        {

            var response = false;
            if (IsUserInRole(userName, roleName))
            {
                var role = dbEntities.Roles.Where(r => r.Name == roleName).FirstOrDefault();
                var user = dbEntities.Users.Where(u => u.UserName == userName).FirstOrDefault();
                var userRole = dbEntities.UserRoles.Where(r => r.RoleId == role.Id && r.UserId == user.Id).FirstOrDefault();
                userRole.RoleId = role.Id;
                userRole.UserId = user.Id;
                dbEntities.UserRoles.Remove(userRole);
                dbEntities.SaveChanges();
                response = true;
            }
            return response;
        }

        public void CreateRole(string roleName, string homePageUrl)
        {
            var role = dbEntities.Roles.Where(r => r.Name == roleName && r.IsActive);
            if (role.Count() == 0)
            {
                //add the role description
                var entity = new Role() {
                    Name = roleName,
                    Description = roleName,
                    HomePageUrl = homePageUrl,
                    IsActive = true
                };             
                dbEntities.Roles.Add(entity);
                dbEntities.SaveChanges();
            }
        }

        public void DeleteRole(string roleName)
        {
            var role = dbEntities.Roles.Where(r => r.Name == roleName && r.IsActive);
            if (role.Count() > 0)
            {
                role.FirstOrDefault().IsActive = false;
                dbEntities.SaveChanges();
            }
        }

    }
}
