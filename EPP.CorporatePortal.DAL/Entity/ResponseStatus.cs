using System.Net;

namespace EPP.CorporatePortal.DAL.Entity
{
    public class ResponseStatusEntity : BaseEntity
    {
        public HttpStatusCode StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public bool isAuthenticated { get; set; }

        public UserData UserData { get; set; }
    }
}