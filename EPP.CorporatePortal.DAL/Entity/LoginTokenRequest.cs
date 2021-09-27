namespace EPP.CorporatePortal.DAL.Entity

{
    public class LoginResponseMessage : BaseEntity
    {
        public string Message { get; set; }
    }
    public class LoginTokenRequest 
    {
        public string AccountCode { get; set; }
        public string Password { get; set; }
        public string AppCode { get; set; }
        public int BusinessEntityID { get; set; }
        public string UUiD { get; set; }
    }
    public class LoginTokenResponse
    {
        public ResponseStatusEntity ResponseStatusEntity { get; set; }
        public bool Valid { get; set; }
        public string TokenID { get; set; }
    }
    public class ChangePasswordTokenRequest
    {
        public string AccountCode { get; set; }
        public string Password { get; set; }
        public string AppCode { get; set; }
        public int BusinessEntityID { get; set; }
        public string NewPassword { get; set; }
    }
    public class ResetPasswordTokenRequest
    {
        public string AccountCode { get; set; }
        public string Password { get; set; }
        public string AppCode { get; set; }
        public int BusinessEntityID { get; set; }
        public string EmailAddress { get; set; }
    }
    public class ResetPasswordTokenResponse
    {
        public ResponseStatusEntity ResponseStatusEntity { get; set; }
        public bool Valid { get; set; }
    }
    public class CreateUserTokenRequest
    {
        public int BusinessEntityID { get; set; }
        public string Username { get; set; } // AccountCode
        public string AccountPIN { get; set; } = string.Empty;
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string MobilePhone { get; set; }
        public string Gender { get; set; }
        public string ICNo { get; set; }
        public string AppCode { get; set; }
        public string Url { get; set; }
    }
    public class CreateUserTokenResponse
    {
        public ResponseStatusEntity ResponseStatusEntity { get; set; }
        public bool Valid { get; set; }
    }
    public class ModifyUserTokenRequest
    {
        public string Username { get; set; } // AccountCode
        public string NewEmailAddress { get; set; }
        public string NewMobilePhone { get; set; }
        public string OldEmailAddress { get; set; }
        public string OldMobilePhone { get; set; }
    }
    public class ModifyUserTokenResponse
    {
        public ResponseStatusEntity ResponseStatusEntity { get; set; }
        public bool Valid { get; set; }
    }
    public class DeleteUserTokenRequest
    {
        public string Username { get; set; } // AccountCode
        public string EmailAddress { get; set; }
    }    
    public class DeleteUserTokenResponse
    {
        public ResponseStatusEntity ResponseStatusEntity { get; set; }
        public bool Valid { get; set; }
    }
    public class StatusChangeUserTokenRequest
    {
        public string Username { get; set; } // AccountCode
        public string EmailAddress { get; set; }
        public string AppCode { get; set; }
        public string Status { get; set; }
        public string UpdatedBy { get; set; }
    }
    public class StatusChangeUserTokenResponse
    {
        public ResponseStatusEntity ResponseStatusEntity { get; set; }
        public bool Valid { get; set; }
    }
}