using EPP.CorporatePortal.DAL.Entity;
using EPP.CorporatePortal.DAL.Model;
using RestSharp;
using System;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;
using System.Runtime.Serialization;
using System.IO;
//using System.Security.Claims;
using System.Threading;
using Microsoft.AspNet.Identity;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using EPP.CorporatePortal.DAL.EDMX;
using System.Data;
using Newtonsoft.Json;


namespace EPP.CorporatePortal.DAL.Service
{
    public class LoginService : BaseService
    {


        /// <summary>
        /// Authenticate against AgentHub Service
        /// </summary>
        /// <param name="body"></param>
        /// <param name="accountCode"></param>
        /// <returns>LoginTokenResponse</returns>
        public LoginTokenResponse Authenticate(string body, string accountCode)
        {

            var returnObj = new LoginTokenResponse();

            try
            {
                Uri baseUrl = new Uri(CommonService.GetSystemConfigValue("AgentDataHubBaseUrl") + CommonService.GetSystemConfigValue("LoginWebService"));

                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("Request", "{\n\"T\":\"" + body + "\"\n}", ParameterType.RequestBody);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Requesting Login.", "Login");
                IRestResponse response = client.Execute(request);                
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response details: StatusCode: " + response.StatusCode + "| StatusDescription: " + response.StatusDescription + "| Content: " + response.Content, "Login");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Login Successful", "Login");
                    var token = GetToken(response);
                    //to return JWT TokenID
                    returnObj.TokenID = token.ToString();
                    var account = GetAccount(token.ToString(), accountCode);
                    if (account.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var strJson = account.Content.ToString();
                        var userData = GetDeserializeObject<UserData>(strJson);

                        returnObj.Valid = true;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = account.StatusCode, StatusDescription = account.StatusDescription, UserData = userData };
                    }
                    else
                    {
                        returnObj.Valid = true;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = account.StatusCode, StatusDescription = account.StatusDescription, UserData = null };
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    LoginResponseMessage responseMessage = new LoginResponseMessage();

                    try
                    {
                        var strJson = response.Content.ToString();
                        responseMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResponseMessage>(strJson);
                    }
                    catch (Exception ex)
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error getting Response Message JSON: " + ex.Message, "Login");
                        responseMessage.Message = "";
                    }

                    //To cater for first time logins, AgentPortalHub will request to Change Password
                    if (responseMessage.Message == "Unauthorized Access - Please change password for first time login")
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed.Requesting Login due to Change Password needed.", "Login");
                        returnObj.Valid = false;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = "Change Password Needed.", UserData = null };
                    }
                    else// Normal scenario
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed.Requesting Login due to No Authorization.", "Login");
                        returnObj.Valid = false;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = !string.IsNullOrEmpty(responseMessage.Message) ? responseMessage.Message : "Not authorized.", UserData = null };
                    }
                }
                else
                {
                    returnObj.Valid = false;
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed.Requesting Login due to No response from Authentication Service.", "Login");
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = "No response from Authentication Service. Probably the service is down", UserData = null };
                }
                //Save to log error message content as well
                try
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response Contents: " + response.Content, "Login");
                    }
                }
                catch (Exception ex)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in UpdatePassword: " + ex.Message, "ResetPassword");
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in Authenticate: " + ex.Message, "Login");
            }

            return returnObj;
        }

        /// <summary>
        /// Authenticate against AgentHub Service by requesting for the Account Info
        /// </summary>
        /// <param name="tokenID"></param>
        /// <param name="accountCode"></param>
        /// <returns>LoginTokenResponse</returns>
        public LoginTokenResponse IsAuthenticated(string tokenID, string accountCode)
        {
            var returnObj = new LoginTokenResponse();

            try
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Re-authenticating tokenID", "Login");
                var account = GetAccount(tokenID, accountCode);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response details: StatusCode: " + account.StatusCode + "| StatusDescription: " + account.StatusDescription + "| Content: " + account.Content, "GetAccount");
                if (account.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var strJson = account.Content.ToString();
                    var userData = GetDeserializeObject<UserData>(strJson);

                    returnObj.Valid = true;
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = account.StatusCode, StatusDescription = account.StatusDescription, UserData = userData };
                }
                else
                {
                    returnObj.Valid = false;
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed retrieving login account: " + account.StatusCode + "|" + account.StatusDescription, "Get Account");
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = account.StatusCode, StatusDescription = account.StatusDescription, UserData = null };
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in Authenticate: " + ex.Message, "Login");
            }

            return returnObj;
        }

        /// <summary>
        /// Validate Token against AgentHub Service
        /// </summary>
        /// <param name="tokenID"></param>
        /// <param name="accountCode"></param>
        /// <returns>LoginTokenResponse</returns>
        public LoginTokenResponse ValidateToken(string tokenID, string accountCode)
        {
            var returnObj = new LoginTokenResponse();

            try
            {
                Uri baseUrl = new Uri(CommonService.GetSystemConfigValue("AgentDataHubBaseUrl") + CommonService.GetSystemConfigValue("ValidateWebService"));
                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.GET);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + tokenID);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Validating tokenID", "ValidateToken");
                IRestResponse response = client.Execute(request);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response details: StatusCode: " + response.StatusCode + "| StatusDescription: " + response.StatusDescription + "| Content: " + response.Content, "ValidateToken");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    LoginResponseMessage responseMessage = new LoginResponseMessage();

                    try
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response Contents: " + response.Content, "ValidateToken");
                        var strJson = response.Content.ToString();
                        responseMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResponseMessage>(strJson);
                    }
                    catch (Exception ex)
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error getting Response Message JSON: " + ex.Message, "ValidateToken");
                        responseMessage.Message = "";
                    }
                    
                    if (responseMessage.Message == "Valid Token")//Token valid
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Token is valid", "ValidateToken");
                        returnObj.Valid = true;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = response.StatusCode, StatusDescription = responseMessage.Message, UserData = null };
                    }
                    else//Other error messages
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Token is not valid", "ValidateToken");
                        returnObj.Valid = false;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = !string.IsNullOrEmpty(responseMessage.Message) ? responseMessage.Message : response.StatusDescription, UserData = null };
                    }
                }
                else
                {
                    try
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response Contents: " + response.Content, "ValidateToken");
                    }
                    catch (Exception ex)
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error getting Response content: " + ex.Message, "ResetPassword");
                    }

                    returnObj.Valid = false;
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed retrieving login account: " + response.StatusCode + "|" + response.StatusDescription, "Get Account");
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = response.StatusCode, StatusDescription = response.StatusDescription, UserData = null };
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in ValidateToken: " + ex.Message, "ValidateToken");
            }

            return returnObj;
        }

        /// <summary>
        /// Logout Token from AgentHub Service
        /// </summary>
        /// <param name="tokenID"></param>
        /// <param name="accountCode"></param>
        /// <returns>LoginTokenResponse</returns>
        public LoginTokenResponse LogoutToken(string tokenID, string accountCode)
        {
            var returnObj = new LoginTokenResponse();

            try
            {
                Uri baseUrl = new Uri(CommonService.GetSystemConfigValue("AgentDataHubBaseUrl") + CommonService.GetSystemConfigValue("LogoutWebService"));
                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.GET);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + tokenID);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Logging out tokenID", "LogoutToken");
                IRestResponse response = client.Execute(request);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response details: StatusCode: " + response.StatusCode + "| StatusDescription: " + response.StatusDescription + "| Content: " + response.Content, "Logout");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    LoginResponseMessage responseMessage = new LoginResponseMessage();

                    try
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response Contents: " + response.Content, "LogoutToken");
                        var strJson = response.Content.ToString();
                        responseMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResponseMessage>(strJson);
                    }
                    catch (Exception ex)
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error getting Response Message JSON: " + ex.Message, "LogoutToken");
                        responseMessage.Message = "";
                    }

                    if (responseMessage.Message == "Logout process success.")//Token valid
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Logged out successfully", "LogoutToken");
                        returnObj.Valid = true;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = response.StatusCode, StatusDescription = responseMessage.Message, UserData = null };
                    }
                    else//Other error messages
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Error logging out", "LogoutToken");
                        returnObj.Valid = false;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = !string.IsNullOrEmpty(responseMessage.Message) ? responseMessage.Message : response.StatusDescription, UserData = null };
                    }
                }
                else
                {
                    try
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response Contents: " + response.Content, "LogoutToken");
                    }
                    catch (Exception ex)
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error getting Response content: " + ex.Message, "LogoutToken");
                    }

                    returnObj.Valid = false;
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Error logging out: " + response.StatusCode + "|" + response.StatusDescription, "LogoutToken");
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = response.StatusCode, StatusDescription = response.StatusDescription, UserData = null };
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in LogoutToken: " + ex.Message, "LogoutToken");
            }

            return returnObj;
        }

        /// <summary>
        /// Update password to Agent Portal Hub
        /// </summary>
        /// <param name="tokenID"></param>
        /// <param name="accountCode"></param>
        /// <returns>LoginTokenResponse</returns>
        public ResetPasswordTokenResponse UpdatePassword(string body, string accountCode)
        {
            var returnObj = new ResetPasswordTokenResponse();

            try
            {
                Uri baseUrl = new Uri(CommonService.GetSystemConfigValue("AgentDataHubBaseUrl") + CommonService.GetSystemConfigValue("ChangePasswordWebService"));

                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("Request", "{\n\"T\":\"" + body + "\"\n}", ParameterType.RequestBody);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Sending Change Password request to Agent Portal Hub", "UpdatePassword");
                IRestResponse response = client.Execute(request);                
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response details: StatusCode: " + response.StatusCode + "| StatusDescription: " + response.StatusDescription + "| Content: " + response.Content, "UpdatePassword");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    LoginResponseMessage responseMessage = new LoginResponseMessage();

                    try
                    {
                        var strJson = response.Content.ToString();
                        responseMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResponseMessage>(strJson);
                    }
                    catch (Exception ex)
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error getting Response Message JSON: " + ex.Message, "Login");
                        responseMessage.Message = "";
                    }

                    //Checking Response message
                    if (responseMessage.Message == "Change password successful") //Successful
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Change Password Successful", "UpdatePassword");

                        returnObj.Valid = true;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = response.StatusCode, StatusDescription = responseMessage.Message, UserData = null };
                    }
                    else //In case failed or other uncatered messages
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Change Password Not Successful", "UpdatePassword");

                        returnObj.Valid = false;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = response.StatusCode, StatusDescription = responseMessage.Message, UserData = null };
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed.Password Change due to No Authorization.", "UpdatePassword");
                    returnObj.Valid = false;
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = "Not authorized.", UserData = null };
                }
                else
                {
                    returnObj.Valid = false;
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed.Password Change due to No response from Authentication Service.", "UpdatePassword");
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = "No response from Authentication Service. Probably the service is down", UserData = null };
                }
                //Save to log error message content as well
                try
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response Contents: " + response.Content, "UpdatePassword");
                }
                catch (Exception ex)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in UpdatePassword: " + ex.Message, "UpdatePassword");
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in UpdatePassword: " + ex.Message, "UpdatePassword");
            }

            return returnObj;
        }

        /// <summary>
        /// Reset password request to Agent Portal Hub
        /// </summary>
        /// <param name="tokenID"></param>
        /// <param name="accountCode"></param>
        /// <returns>LoginTokenResponse</returns>
        public ResetPasswordTokenResponse ResetPassword(string body, string accountCode)
        {
            var returnObj = new ResetPasswordTokenResponse();

            try
            {
                //TODO: Change parameter naming for Change Password., Also logging and cs coding
                Uri baseUrl = new Uri(CommonService.GetSystemConfigValue("AgentDataHubBaseUrl") + CommonService.GetSystemConfigValue("ResetPasswordWebService"));

                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("Request", "{\n\"T\":\"" + body + "\"\n}", ParameterType.RequestBody);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Sending Reset Password request to Agent Portal Hub", "ResetPassword");
                IRestResponse response = client.Execute(request);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response details: StatusCode: " + response.StatusCode + "| StatusDescription: " + response.StatusDescription + "| Content: " + response.Content, "ResetPassword");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    LoginResponseMessage responseMessage = new LoginResponseMessage();

                    try
                    {
                        var strJson = response.Content.ToString();
                        responseMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResponseMessage>(strJson);
                    }
                    catch (Exception ex)
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error getting Response Message JSON: " + ex.Message, "Login");
                        responseMessage.Message = "";
                    }

                    //Checking Response message
                    if (responseMessage.Message == "Process successful") //Successful
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Reset Password Successful", "ResetPassword");

                        returnObj.Valid = true;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = response.StatusCode, StatusDescription = responseMessage.Message, UserData = null };
                    }
                    else //In case failed or other uncatered messages
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Reset Password Not Successful", "ResetPassword");

                        returnObj.Valid = false;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = response.StatusCode, StatusDescription = responseMessage.Message, UserData = null };
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed.Password Change due to No Authorization.", "ResetPassword");
                    returnObj.Valid = false;
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = "Not authorized.", UserData = null };
                }
                else
                {
                    returnObj.Valid = false;
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed.Password Change due to No response from Authentication Service.", "ResetPassword");
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = "No response from Authentication Service. Probably the service is down", UserData = null };
                }
                //Save to log error message content as well
                try
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response Contents: " + response.Content, "ResetPassword");
                }
                catch (Exception ex)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in UpdatePassword: " + ex.Message, "ResetPassword");
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in UpdatePassword: " + ex.Message, "ResetPassword");
            }

            return returnObj;
        }

        /// <summary>
        /// Create user at Agent Portal Hub
        /// </summary>
        /// <param name="body"></param>
        /// <param name="accountCode"></param>
        /// <returns>CreateUserTokenResponse</returns>
        public CreateUserTokenResponse CreateUser(string body, string accountCode, string bearerToken)
        {
            var returnObj = new CreateUserTokenResponse();

            try
            {
                Uri baseUrl = new Uri(CommonService.GetSystemConfigValue("AgentDataHubBaseUrl") + CommonService.GetSystemConfigValue("CreateUserWebService"));

                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + bearerToken);
                request.AddParameter("Request", "{\n\"T\":\"" + body + "\"\n}", ParameterType.RequestBody);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Sending Create User request to Agent Portal Hub", "CreateUser");
                IRestResponse response = client.Execute(request);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response details: StatusCode: " + response.StatusCode + "| StatusDescription: " + response.StatusDescription + "| Content: " + response.Content, "CreateUser");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    LoginResponseMessage responseMessage = new LoginResponseMessage();

                    try
                    {
                        var strJson = response.Content.ToString();
                        responseMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResponseMessage>(strJson);
                    }
                    catch (Exception ex)
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error getting Response Message JSON: " + ex.Message, "CreateUser");
                        responseMessage.Message = "";
                    }

                    //Checking Response message
                    if (responseMessage.Message == "Process successful") //Successful
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Create User Successful", "CreateUser");

                        returnObj.Valid = true;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = response.StatusCode, StatusDescription = responseMessage.Message, UserData = null };
                    }
                    else //In case failed or other uncatered messages
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Create User Not Successful", "CreateUser");

                        returnObj.Valid = false;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = response.StatusCode, StatusDescription = responseMessage.Message, UserData = null };
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed.Create User due to No Authorization.", "CreateUser");
                    returnObj.Valid = false;
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = "Not authorized.", UserData = null };
                }
                else
                {
                    returnObj.Valid = false;
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed.Create User due to No response from Authentication Service.", "CreateUser");
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = "No response from Authentication Service. Probably the service is down", UserData = null };
                }
                //Save to log error message content as well
                try
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response Contents: " + response.Content, "CreateUser");
                }
                catch (Exception ex)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in CreateUser: " + ex.Message, "CreateUser");
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in CreateUser: " + ex.Message, "CreateUser");
            }

            return returnObj;
        }

        /// <summary>
        /// Create user at Agent Portal Hub
        /// </summary>
        /// <param name="body"></param>
        /// <param name="accountCode"></param>
        /// <returns>CreateUserTokenResponse</returns>
        public ModifyUserTokenResponse ModifyUser(string body, string accountCode, string bearerToken)
        {
            var returnObj = new ModifyUserTokenResponse();

            try
            {
                Uri baseUrl = new Uri(CommonService.GetSystemConfigValue("AgentDataHubBaseUrl") + CommonService.GetSystemConfigValue("ModifyUserWebService"));

                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + bearerToken);
                request.AddParameter("Request", "{\n\"T\":\"" + body + "\"\n}", ParameterType.RequestBody);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Sending Modify User request to Agent Portal Hub", "ModifyUser");
                IRestResponse response = client.Execute(request);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response details: StatusCode: " + response.StatusCode + "| StatusDescription: " + response.StatusDescription + "| Content: " + response.Content, "ModifyUser");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    LoginResponseMessage responseMessage = new LoginResponseMessage();

                    try
                    {
                        var strJson = response.Content.ToString();
                        responseMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResponseMessage>(strJson);
                    }
                    catch (Exception ex)
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error getting Response Message JSON: " + ex.Message, "ModifyUser");
                        responseMessage.Message = "";
                    }

                    //Checking Response message
                    if (responseMessage.Message == "Process successful") //Successful
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Modify User Successful", "ModifyUser");

                        returnObj.Valid = true;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = response.StatusCode, StatusDescription = responseMessage.Message, UserData = null };
                    }
                    else //In case failed or other uncatered messages
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Modify User Not Successful", "ModifyUser");

                        returnObj.Valid = false;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = response.StatusCode, StatusDescription = responseMessage.Message, UserData = null };
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed.Modify User due to No Authorization.", "ModifyUser");
                    returnObj.Valid = false;
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = "Not authorized.", UserData = null };
                }
                else
                {
                    returnObj.Valid = false;
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed.Modify User due to No response from Authentication Service.", "ModifyUser");
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = "No response from Authentication Service. Probably the service is down", UserData = null };
                }
                //Save to log error message content as well
                try
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response Contents: " + response.Content, "ModifyUser");
                }
                catch (Exception ex)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in ModifyUser: " + ex.Message, "ModifyUser");
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in ModifyUser: " + ex.Message, "ModifyUser");
            }

            return returnObj;
        }
        /// <summary>
        /// Create user at Agent Portal Hub
        /// </summary>
        /// <param name="body"></param>
        /// <param name="accountCode"></param>
        /// <returns>CreateUserTokenResponse</returns>
        public StatusChangeUserTokenResponse StatusChangeUser(string body, string accountCode, string bearerToken)
        {
            var returnObj = new StatusChangeUserTokenResponse();

            try
            {
                Uri baseUrl = new Uri(CommonService.GetSystemConfigValue("AgentDataHubBaseUrl") + CommonService.GetSystemConfigValue("StatusChangeUserWebService"));

                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + bearerToken);
                request.AddParameter("Request", "{\n\"T\":\"" + body + "\"\n}", ParameterType.RequestBody);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Sending Status Change User request to Agent Portal Hub", "StatusChangeUser");
                IRestResponse response = client.Execute(request);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response details: StatusCode: " + response.StatusCode + "| StatusDescription: " + response.StatusDescription + "| Content: " + response.Content, "StatusChangeUser");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    LoginResponseMessage responseMessage = new LoginResponseMessage();

                    try
                    {
                        var strJson = response.Content.ToString();
                        responseMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResponseMessage>(strJson);
                    }
                    catch (Exception ex)
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error getting Response Message JSON: " + ex.Message, "StatusChangeUser");
                        responseMessage.Message = "";
                    }

                    //Checking Response message
                    if (responseMessage.Message == "Process successful") //Successful
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Status Change User Successful", "StatusChangeUser");

                        returnObj.Valid = true;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = response.StatusCode, StatusDescription = responseMessage.Message, UserData = null };
                    }
                    else //In case failed or other uncatered messages
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Status Change User Not Successful", "StatusChangeUser");

                        returnObj.Valid = false;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = response.StatusCode, StatusDescription = responseMessage.Message, UserData = null };
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed.Status Change User due to No Authorization.", "StatusChangeUser");
                    returnObj.Valid = false;
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = "Not authorized.", UserData = null };
                }
                else
                {
                    returnObj.Valid = false;
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed.Status Change User due to No response from Authentication Service.", "StatusChangeUser");
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = "No response from Authentication Service. Probably the service is down", UserData = null };
                }
                //Save to log error message content as well
                try
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response Contents: " + response.Content, "StatusChangeUser");
                }
                catch (Exception ex)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in StatusChangeUser: " + ex.Message, "StatusChangeUser");
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in StatusChangeUser: " + ex.Message, "StatusChangeUser");
            }

            return returnObj;
        }
        /// <summary>
        /// Create user at Agent Portal Hub
        /// </summary>
        /// <param name="body"></param>
        /// <param name="accountCode"></param>
        /// <returns>CreateUserTokenResponse</returns>
        public DeleteUserTokenResponse DeleteUser(string body, string accountCode, string bearerToken)
        {
            var returnObj = new DeleteUserTokenResponse();

            try
            {
                Uri baseUrl = new Uri(CommonService.GetSystemConfigValue("AgentDataHubBaseUrl") + CommonService.GetSystemConfigValue("DeleteUserWebService"));

                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + bearerToken);
                request.AddParameter("Request", "{\n\"T\":\"" + body + "\"\n}", ParameterType.RequestBody);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Sending Delete User request to Agent Portal Hub", "DeleteUser");
                IRestResponse response = client.Execute(request);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response details: StatusCode: " + response.StatusCode + "| StatusDescription: " + response.StatusDescription + "| Content: " + response.Content, "DeleteUser");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    LoginResponseMessage responseMessage = new LoginResponseMessage();

                    try
                    {
                        var strJson = response.Content.ToString();
                        responseMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResponseMessage>(strJson);
                    }
                    catch (Exception ex)
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error getting Response Message JSON: " + ex.Message, "DeleteUser");
                        responseMessage.Message = "";
                    }

                    //Checking Response message
                    if (responseMessage.Message == "Process successful") //Successful
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Delete User Successful", "DeleteUser");

                        returnObj.Valid = true;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = response.StatusCode, StatusDescription = responseMessage.Message, UserData = null };
                    }
                    else //In case failed or other uncatered messages
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Delete User Not Successful", "DeleteUser");

                        returnObj.Valid = false;
                        returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = response.StatusCode, StatusDescription = responseMessage.Message, UserData = null };
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed.Delete User due to No Authorization.", "DeleteUser");
                    returnObj.Valid = false;
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = "Not authorized.", UserData = null };
                }
                else
                {
                    returnObj.Valid = false;
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Failed.Delete User due to No response from Authentication Service.", "DeleteUser");
                    returnObj.ResponseStatusEntity = new ResponseStatusEntity() { StatusCode = 0, StatusDescription = "No response from Authentication Service. Probably the service is down", UserData = null };
                }
                //Save to log error message content as well
                try
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Response Contents: " + response.Content, "DeleteUser");
                }
                catch (Exception ex)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in DeleteUser: " + ex.Message, "DeleteUser");
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in DeleteUser: " + ex.Message, "DeleteUser");
            }

            return returnObj;
        }

        /// <summary>
        /// Deserialize JSON string into object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonStr"></param>
        /// <returns>Type Object</returns>
        private T GetDeserializeObject<T>(string jsonStr)
        {
            DataContractJsonSerializerSettings dateFormatSettings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true,
                DateTimeFormat = new DateTimeFormat(CommonService.GetAppSettingValue("jsonDateFormat"))
            };
            var deserializedUserD = new UserData();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonStr));
            var ser = new DataContractJsonSerializer(deserializedUserD.GetType(), dateFormatSettings);
            deserializedUserD = ser.ReadObject(ms) as UserData;
            ms.Close();
            var userData = deserializedUserD;
            return (T)Convert.ChangeType(userData, typeof(T));
        }

        /// <summary>
        /// Get account info from AgentHub service
        /// </summary>
        /// <param name="body"></param>
        /// <param name="accountCode"></param>
        /// <returns>IRestResponse</returns>
        public IRestResponse GetAccount(string body, string accountCode)
        {
            IRestResponse response = null;

            try
            {
                Uri baseUrl = new Uri(CommonService.GetSystemConfigValue("AgentDataHubBaseUrl") + CommonService.GetSystemConfigValue("AccountWebService"));
                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.GET);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + body);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, accountCode, "Retrieving Account.", "Login");
                response = client.Execute(request);
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, accountCode, "Error in Authenticate: " + ex.Message, "Login");
            }

            return response;
        }
        /// <summary>
        /// Builds token string for AgentHub service
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Token String</returns>
        private object GetToken(IRestResponse response)
        {
            var content = response.Content;
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            //dynamic dobj = jsonSerializer.Deserialize<dynamic>(content);
            TokenClass obj = jsonSerializer.Deserialize<TokenClass>(content);
            //string tokenStr = dobj["Token"].ToString();
            string tokenStr = obj.Token;
            return tokenStr;


        }

        /// <summary>
        /// Creates Identity object
        /// </summary>
        /// <param name="label"></param>
        /// <param name="userName"></param>
        /// <param name="userData"></param>
        /// <returns>ClaimsIdentity</returns>
        //public ClaimsIdentity CreateIdentity(string userName, UserData userData)
        //{
        //    var userIdentity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie)
        //    {
        //        Label = "CorporatePortal_Identity"
        //    };

        //    var loggedInUser = userName;
        //    var userCorporates = GetUserCorporates(loggedInUser);
        //    var jsonStr = JsonConvert.SerializeObject(userCorporates);

        //    userIdentity.AddClaim(new Claim("UserCorporates", jsonStr));

        //    userIdentity.AddClaim(new Claim("AccountStatus", userData.AccountStatus ?? ""));
        //    userIdentity.AddClaim(new Claim(ClaimTypes.Upn, loggedInUser ?? ""));
        //    userIdentity.AddClaim(new Claim(ClaimTypes.Name, userData.FullName ?? ""));
        //    userIdentity.AddClaim(new Claim("AccountCode", userData.AccountCode ?? "", "string"));
        //    userIdentity.AddClaim(new Claim(ClaimTypes.Email, userData.EmailAddress ?? "", "string"));
        //    userIdentity.AddClaim(new Claim(ClaimTypes.MobilePhone, userData.MobilePhone ?? "", "string"));
        //    userIdentity.AddClaim(new Claim("OfficePhone", userData.OfficePhone ?? "", "string"));
        //    userIdentity.AddClaim(new Claim("Address1", userData.Address1 ?? "", "string"));
        //    userIdentity.AddClaim(new Claim("Address2", userData.Address2 ?? "", "string"));
        //    userIdentity.AddClaim(new Claim("Address3", userData.Address3 ?? "", "string"));
        //    userIdentity.AddClaim(new Claim("AddressCity", userData.AddressCity ?? "", "string"));
        //    userIdentity.AddClaim(new Claim(ClaimTypes.StateOrProvince, userData.AddressState ?? "", "string"));
        //    userIdentity.AddClaim(new Claim(ClaimTypes.Country, userData.AddressCountry ?? "", "string"));
        //    userIdentity.AddClaim(new Claim(ClaimTypes.PostalCode, userData.AddressPostcode ?? "", "string"));
        //    userIdentity.AddClaim(new Claim("BranchCode", userData.BranchCode ?? "", "string"));
        //    userIdentity.AddClaim(new Claim("BusinessRegistrationNo", userCorporates[0].SourceId ?? "", "string"));
        //    userIdentity.AddClaim(new Claim("AgentCodes", userData.AgentCodes ?? "", "string"));
        //    userIdentity.AddClaim(new Claim("ParentCorporate", userCorporates[0].Name ?? "", "string"));
        //    userIdentity.AddClaim(new Claim("ParentBizRegNo", userCorporates[0].SourceId ?? "", "string"));
        //    userIdentity.AddClaim(new Claim("IsOwner", userCorporates[0].IsOwner ? "true" : "false"));


        //    var roles = new RolesService().GetUserRoles(userName);
        //    foreach (var role in roles)
        //    {
        //        userIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
        //    }
        //    var identity = new ClaimsPrincipal(userIdentity);
        //    Thread.CurrentPrincipal = identity;           

        //    return userIdentity;
        //}

        public UserIdentityModel CreateSession(string userName, UserData userData)
        {
            //var userIdentity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie)
            //{
            //    Label = "CorporatePortal_Identity"
            //};

            var loggedInUser = userName;
            var userCorporates = GetUserCorporates(loggedInUser);
            var jsonStr = JsonConvert.SerializeObject(userCorporates);

            UserIdentityModel identityModel = new Model.UserIdentityModel();

            identityModel.UserCorporates = jsonStr;
            identityModel.AccountStatus = userData.AccountStatus ?? "";
            identityModel.PrincipalName = loggedInUser ?? "";
            identityModel.Name = userData.FullName ?? "";
            identityModel.AccountCode = userData.AccountCode ?? "";
            identityModel.Email = userData.EmailAddress ?? "";
            identityModel.MobileNumber = userData.MobilePhone ?? "";
            identityModel.OfficePhone = userData.OfficePhone ?? "";
            identityModel.Address1 = userData.Address1 ?? "";
            identityModel.Address2 = userData.Address2 ?? "";
            identityModel.Address3 = userData.Address3 ?? "";
            identityModel.AddressCity = userData.AddressCity ?? "";
            identityModel.AddressState = userData.AddressState ?? "";
            identityModel.AddressCountry = userData.AddressCountry ?? "";
            identityModel.AddressPostcode = userData.AddressPostcode ?? "";
            identityModel.BranchCode = userData.BranchCode ?? "";
            identityModel.BusinessRegistrationNo = userCorporates[0].SourceId ?? "";
            identityModel.AgentCodes = userData.AgentCodes ?? "";
            identityModel.ParentCorporate = userCorporates[0].Name ?? "";
            identityModel.ParentBizRegNo = userCorporates[0].SourceId ?? "";
            identityModel.IsOwner = userCorporates[0].IsOwner ? "true" : "false";
            identityModel.UCorpId = "";

            var roles = new RolesService().GetUserRoles(userName);
            foreach (var role in roles)
            {
                identityModel.Role = role.ToString();
            }
            
            return identityModel;
        }

        private List<Corporate> GetUserCorporates(string loggedInUser)
        {
            var list = new List<Corporate>();
            var dt = new StoredProcService(loggedInUser).GetCorporatesByUser(loggedInUser);
            foreach (DataRow drow in dt.Rows)
            {
                var corp = new Corporate
                {
                    SourceId = drow["SourceId"].ToString(),
                    IsActive = Convert.ToBoolean(drow["IsActive"]),
                    Name = Convert.ToString(drow["Name"]),
                    ParentId = drow["ParentId"].ToString(),
                    Description = Convert.ToString(drow["Description"]),
                    IsOwner = Convert.ToBoolean(drow["IsOwner"])
                };
                list.Add(corp);
            }
            return list;
        }


        /// <summary>
        /// Updates key Identity claims
        /// </summary>
        /// <param name="newName"></param>
        //public void UpdateClaim(string newName)
        //{
        //    var user = HttpContext.Current.User as ClaimsPrincipal;
        //    var identity = user.Identity as ClaimsIdentity;
        //    var nameClaim = (from c in user.Claims
        //                     where c.Type == ClaimTypes.Name
        //                     select c).Single();
        //    identity.RemoveClaim(nameClaim);



        //    var businessRegistrationNoClaim = (from c in user.Claims
        //                                       where c.Type == "BusinessRegistrationNo"
        //                                       select c).Single();
        //    identity.RemoveClaim(businessRegistrationNoClaim);




        //    identity.AddClaim(new Claim(ClaimTypes.Name, newName));

        //    //var newBusinessRegistrationNo = GetBusinessRegistrationNo(newName);
        //    //identity.AddClaim(new Claim("BusinessRegistrationNo", newBusinessRegistrationNo));

        //}

        public UserIdentityModel UpdateSession(string userName, string newName, UserIdentityModel currsession)
        {
            
            //currsession.Name = newName;
            var newBusinessRegistrationNo = GetBusinessRegistrationNo(userName, newName);
            currsession.BusinessRegistrationNo = newBusinessRegistrationNo;

            return currsession;

        }



        /// <summary>
        /// Gets Business Registration No for thr corporate
        /// </summary>
        /// <param name="parentName"></param>
        /// <returns>Business Registration No</returns>
        private string GetBusinessRegistrationNo(string userName, string parentName)
        {
            //var user = HttpContext.Current.User as ClaimsPrincipal;
            //var identity = user.Identity as ClaimsIdentity;
            //var userName = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).SingleOrDefault();
            var bizRegNo = new StoredProcService(userName).GetBusinessRegistrationNo(parentName);
            return bizRegNo;
        }
    }

    public class TokenClass
    {
        public string Token { get; set; }
    }
}
