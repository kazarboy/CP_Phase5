using EPP.CorporatePortal.DAL.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using static EPP.CorporatePortal.Common.Enums;

namespace EPP.CorporatePortal.DAL.Service
{

   
    
    public class StoredProcService
    {
        public static AuditTrail auditTrailService { get; set;  }
        
        public string UserName { get; set; }

        public   StoredProcService(string userName)
        {
            auditTrailService = new AuditTrail();

            UserName = userName;

        }

 
         public static readonly SqlConnection conn = new SqlConnection()
        {
            ConnectionString= ConfigurationManager.ConnectionStrings["EPPCorporatePortalConnection"].ConnectionString
        };




        public DataTable GetCorporateById(string corporateId, string UCorpId)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,

                    CommandText = "spGetCorporate"
                };
                cmd.Parameters.Add("@CorporateId", SqlDbType.NVarChar).Value = corporateId;
                cmd.Parameters.Add("@UCorpId", SqlDbType.NVarChar).Value = UCorpId;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetCorporateById. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }

        public DataTable GetCorporateUId(string corporateId, string Name)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,

                    CommandText = "spGetCorporateUid"
                };
                cmd.Parameters.Add("@CorporateId", SqlDbType.NVarChar).Value = corporateId;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = Name;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetCorporateById.Error: " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetPolicyByCoporateId(string corporateId,string UCorpId)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetPolicyByCoporateId"
                };
                cmd.Parameters.Add("@CorporateId", SqlDbType.NVarChar).Value = corporateId;
                cmd.Parameters.Add("@UCorpId", SqlDbType.NVarChar).Value = UCorpId;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch(Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetPolicyByCoporateId. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }

        public DataTable GetCorporateSubsidaries(string corporateParentId)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetCorporateSubsidaries"
                };
                cmd.Parameters.Add("@CorporateParentId", SqlDbType.NVarChar).Value = corporateParentId;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetCorporateSubsidaries. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        
        }

        public DataTable GetMenuRights(int rightId)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetMenuRights"
                };
                cmd.Parameters.Add("@RightId", SqlDbType.Int).Value = rightId;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetMenuRights. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
             
        }
        public DataTable GetUserRights(string userName)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetUserRights"
                };
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetUserRights. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetUserByEmail(string emailAddress)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetUserByEmail"
                };
                cmd.Parameters.Add("@EmailAddress", SqlDbType.VarChar).Value = emailAddress;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetUserByEmail. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetUserByUsername(string userName)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetUserByUsername"
                };
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetUserByUsername. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetUserByUserId(int userId)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetUserByUserId"
                };
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = userId;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetUserByUserId. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetClaimByMemberClaimsID(int memberClaimsID)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetClaimByMemberClaimsID"
                };
                cmd.Parameters.Add("@MemberClaimsID", SqlDbType.Int).Value = memberClaimsID;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetClaimByMemberClaimsID. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetBenefitByBenefitCode(string benefitCode)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetBenefitByBenefitCode"
                };
                cmd.Parameters.Add("@BenefitCode", SqlDbType.VarChar).Value = benefitCode;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetBenefitByBenefitCode. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetClaimsMemberDocumentByDocID(int DocID)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetClaimsMemberDocumentByDocID"
                };
                cmd.Parameters.Add("@SourceID", SqlDbType.Int).Value = DocID;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetClaimsMemberDocumentByDocID. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetClaimsMemberDocumentByMemberClaimsId(int MemberClaimsId)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetClaimsMemberDocumentByMemberClaimsId"
                };
                cmd.Parameters.Add("@SourceID", SqlDbType.Int).Value = MemberClaimsId;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetClaimsMemberDocumentByMemberClaimsId. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetUserAccessList(string searchString, string userRole)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetUserAccessList"
                };
                cmd.Parameters.Add("@SearchString", SqlDbType.VarChar).Value = searchString;
                cmd.Parameters.Add("@UserRole", SqlDbType.VarChar).Value = userRole;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetUserAccessList. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetUserAccessMatrix(string searchString)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetUserAccessMatrix"
                };
                cmd.Parameters.Add("@SearchString", SqlDbType.VarChar).Value = searchString;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetUserAccessMatrix. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetClaimsList(string userName)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetClaimsList"
                };
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetClaimsList. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetRequiredDocList(string benefitCode)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetRequiredDocList"
                };
                cmd.Parameters.Add("@BenefitCode", SqlDbType.VarChar).Value = benefitCode;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetRequiredDocList. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetRequiredDocByDocID(int docID)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetRequiredDocByDocID"
                };
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = docID;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetRequiredDocByDocID. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetEBParameter(string keyCode)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetEBParameter"
                };
                cmd.Parameters.Add("@KeyCode", SqlDbType.VarChar).Value = keyCode;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetEBParameter. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetBenefitsMemberLevel(string memberId, string CorpId)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetBenefitsMemberLevel"
                };
                cmd.Parameters.Add("@MemberID", SqlDbType.VarChar).Value = memberId;
                cmd.Parameters.Add("@CorporateId", SqlDbType.VarChar).Value = CorpId;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetBenefitsMemberLevel. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetBenefitsPolicyLevel(string CorpId)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetBenefitsPolicyLevel"
                };
                cmd.Parameters.Add("@CorporateId", SqlDbType.VarChar).Value = CorpId;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetBenefitsPolicyLevel. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetClaimsMemberList(string searchString, string searchType)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetClaimsMemberList"
                };
                cmd.Parameters.Add("@SearchString", SqlDbType.VarChar).Value = searchString;
                cmd.Parameters.Add("@SearchType", SqlDbType.VarChar).Value = searchType;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetClaimsMemberList. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetClaimsMemberByMemberSourceID(string sourceID)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetClaimsMemberByMemberSourceID"
                };
                cmd.Parameters.Add("@MemberSourceID", SqlDbType.VarChar).Value = sourceID;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetClaimsMemberByMemberSourceID. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetMemberClaimsByID(int sourceID)
        {

            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetMemberClaimsByID"
                };
                cmd.Parameters.Add("@MemberSourceID", SqlDbType.Int).Value = sourceID;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetMemberClaimsByID. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public List<DataRow> GetMenus(string userName)
        {
           
            var rights = GetUserRights(userName);
            var menuList = new List<DataRow>();
            if (rights != null)
            {
                try
                {
                    foreach (DataRow right in rights.Rows)
                    {
                        var menus = GetMenuRights(Convert.ToInt32(right["Id"]));
                        foreach (DataRow menu in menus.Rows)
                        {

                            menuList.Add(menu);
                        }
                    }
                }
                catch (Exception ex)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetMenus. Error : " + ex.Message, "StoredProcService");

                }
            }
            return menuList;
        }

        public bool IsRightInRole(int rightsID, int roleID)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spIsRightInRole"
                };
                cmd.Parameters.Add("@RightsId", SqlDbType.Int).Value = rightsID;
                cmd.Parameters.Add("@RoleID", SqlDbType.Int).Value = roleID;
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in IsRightInRole. Error : " + ex.Message, "StoredProcService");
            }
            return dt.Rows.Count>0;
        }
        public string GetBusinessRegistrationNo(string corporateName)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetBusinessRegistrationNo"
                };
                cmd.Parameters.Add("@CorporateName", SqlDbType.VarChar).Value = corporateName;
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }

            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetBusinessRegistrationNo. Error : " + ex.Message, "StoredProcService");
            }
            return dt.Rows[0]["BusinessRegistrationNo"].ToString();
        }

        public DataTable GetAllCorporates()
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetAllCorporates"
                };


                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetAllCorporates. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }

        public DataTable GetCorporatesByUser(string userName)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetCorporatesByUser"
                };
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetCorporatesByUser. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetPolicyByOwnership(string corporateId, string owner)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetPolicyByOwnership"
                };
                cmd.Parameters.Add("@CorporateId", SqlDbType.NVarChar).Value = corporateId;
                cmd.Parameters.Add("@Owner", SqlDbType.VarChar).Value = owner;
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetPolicyByOwnership. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public int InsMemberClaimsDocument(bool ImageNewIndicator, int MemberClaimsId, int DocumentId, string UploadedDocumentName, byte[] Base64Value, DateTime UploadDateTime)
        {

            object val;
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spInsMemberClaimsDocument";
                cmd.Parameters.Add("@ImageNewIndicator", SqlDbType.Bit).Value = ImageNewIndicator;
                cmd.Parameters.Add("@MemberClaimsId", SqlDbType.Int).Value = MemberClaimsId;
                cmd.Parameters.Add("@DocumentId", SqlDbType.Int).Value = DocumentId;
                cmd.Parameters.Add("@UploadedDocumentName", SqlDbType.VarChar).Value = UploadedDocumentName;
                cmd.Parameters.Add("@Base64Value", SqlDbType.VarBinary).Value = Base64Value;
                cmd.Parameters.Add("@UploadDateTime", SqlDbType.DateTime).Value = UploadDateTime;                

                var outParm = new SqlParameter("@ID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParm);

                cmd.ExecuteNonQuery();
                val = outParm.Value;

            }
            catch (Exception ex)
            {
                val = 0;
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in InsMemberClaimsDocument. Error : " + ex.Message, "StoredProcService");
            }
            int Id = (int)val;

            return Id;
        }
        public int InsMemberClaims(ClaimSubmissionModel objClaim, DataRow drMember, DataRow drUser, ClaimSubmissionPolicy policy)
        {

            object val;
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spInsMemberClaims";
                cmd.Parameters.Add("@DataSource", SqlDbType.VarChar).Value = "EBP";
                cmd.Parameters.Add("@SubmitterId", SqlDbType.Int).Value = drUser != null ? Convert.ToInt32(drUser["Id"].ToString()) : 0;
                cmd.Parameters.Add("@SubmissionDate", SqlDbType.DateTime).Value = DateTime.Now;
                cmd.Parameters.Add("@MemberBenefitId", SqlDbType.Int).Value = DBNull.Value;
                cmd.Parameters.Add("@BenefitSourceId", SqlDbType.VarChar).Value = DBNull.Value;
                cmd.Parameters.Add("@BenefitCode", SqlDbType.VarChar).Value = objClaim.BenefitCodeOri ?? string.Empty;

                var dtCauseCGLSMap = GetEBParameter("ClaimCauseCGLSMap");
                if (dtCauseCGLSMap.Rows.Count > 0)
                {
                    var causeCGLSMap = dtCauseCGLSMap.AsEnumerable().Where(w => w["Value"].ToString().Equals(objClaim.CauseOfEvent)).Select(s => s["Description"].ToString()).FirstOrDefault();

                    if (!string.IsNullOrEmpty(causeCGLSMap))
                        cmd.Parameters.Add("@EventCause", SqlDbType.VarChar).Value = causeCGLSMap;
                    else //CGLS has it as mandatory field, should be put as "Others" for other claim types with no Cause Of Events
                        cmd.Parameters.Add("@EventCause", SqlDbType.VarChar).Value = "Others";
                }
                else
                {
                    //Cater for CGLS values in auto registration
                    if (objClaim.CauseOfEvent == "A")
                        cmd.Parameters.Add("@EventCause", SqlDbType.VarChar).Value = "Accidental - MVA";
                    else if (objClaim.CauseOfEvent == "N")
                        cmd.Parameters.Add("@EventCause", SqlDbType.VarChar).Value = "Others";
                    else //CGLS has it as mandatory field, should be put as "Others" for other claim types with no Cause Of Events
                        cmd.Parameters.Add("@EventCause", SqlDbType.VarChar).Value = "Others";
                }


                cmd.Parameters.Add("@EventCauseOri", SqlDbType.VarChar).Value = objClaim.CauseOfEvent ?? string.Empty;
                cmd.Parameters.Add("@Accident", SqlDbType.VarChar).Value = objClaim.Accident ?? string.Empty;
                cmd.Parameters.Add("@EventCauseDesc", SqlDbType.VarChar).Value = objClaim.EventDescription ?? string.Empty;
                cmd.Parameters.Add("@PortalClaimNo", SqlDbType.VarChar).Value = DBNull.Value;
                cmd.Parameters.Add("@CGLSClaimNo", SqlDbType.VarChar).Value = DBNull.Value;
                cmd.Parameters.Add("@ClaimStatus", SqlDbType.VarChar).Value = Common.Constants.Application.PortalStatus.NewSubmission;
                cmd.Parameters.Add("@CGLSRemark", SqlDbType.VarChar).Value = DBNull.Value;
                cmd.Parameters.Add("@IsAppeal", SqlDbType.Bit).Value = false;
                cmd.Parameters.Add("@EventDate", SqlDbType.DateTime).Value = new DateTime(Convert.ToInt32(objClaim.YearOfEvent), Convert.ToInt32(objClaim.MonthOfEvent), Convert.ToInt32(objClaim.DayOfEvent));
                if (drMember != null)
                {
                    cmd.Parameters.Add("@ICNo", SqlDbType.VarChar).Value = drMember["ICNo"].ToString();
                    cmd.Parameters.Add("@IDType", SqlDbType.VarChar).Value = drMember["IDType"].ToString();
                    cmd.Parameters.Add("@IDNo", SqlDbType.VarChar).Value = drMember["IDNo"].ToString();
                    cmd.Parameters.Add("@MemberName", SqlDbType.VarChar).Value = drMember["MemberName"].ToString();
                }
                else
                {
                    cmd.Parameters.Add("@ICNo", SqlDbType.VarChar).Value = objClaim.MemberIDTypeKeyIn == "NRIC" ? objClaim.MemberIDNoKeyIn : string.Empty;
                    cmd.Parameters.Add("@IDType", SqlDbType.VarChar).Value = objClaim.MemberIDTypeKeyIn ?? string.Empty;
                    cmd.Parameters.Add("@IDNo", SqlDbType.VarChar).Value = objClaim.MemberIDTypeKeyIn == "OtherIDNo" ? objClaim.MemberIDNoKeyIn : string.Empty;
                    cmd.Parameters.Add("@MemberName", SqlDbType.VarChar).Value = objClaim.MemberNameKeyIn ?? string.Empty;
                }                
                cmd.Parameters.Add("@PolicyId", SqlDbType.VarChar).Value = policy.PolicyID ?? string.Empty;
                cmd.Parameters.Add("@Acknowledgement", SqlDbType.Bit).Value = true;
                cmd.Parameters.Add("@Base64ApplicationForm", SqlDbType.VarBinary).Value = DBNull.Value;
                cmd.Parameters.Add("@ClaimRefDraftId", SqlDbType.Int).Value = DBNull.Value;
                cmd.Parameters.Add("@BankName", SqlDbType.VarChar).Value = policy.BankName ?? string.Empty;
                cmd.Parameters.Add("@AccountHolderName", SqlDbType.VarChar).Value = policy.AccountHolderName ?? string.Empty;
                cmd.Parameters.Add("@AccountNo", SqlDbType.VarChar).Value = policy.BankAccountNo ?? string.Empty;
                cmd.Parameters.Add("@AccountHolderIdType", SqlDbType.VarChar).Value = policy.IDType ?? string.Empty;
                cmd.Parameters.Add("@AccountHolderIdNo", SqlDbType.VarChar).Value = policy.IDNo ?? string.Empty;
                cmd.Parameters.Add("@AccountHolderContactNo", SqlDbType.VarChar).Value = policy.ContactNo ?? string.Empty;
                cmd.Parameters.Add("@AccountHolderEmail", SqlDbType.VarChar).Value = policy.EmailAddress ?? string.Empty;
                cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = true;
                cmd.Parameters.Add("@BankROC", SqlDbType.VarChar).Value = policy.BankROC ?? string.Empty;

                var outParm = new SqlParameter("@ID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParm);

                cmd.ExecuteNonQuery();
                val = outParm.Value;

            }
            catch (Exception ex)
            {
                val = 0;
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in InsMemberClaims. Error : " + ex.Message, "StoredProcService");
            }
            int Id = (int)val;

            return Id;
        }
        public int InsFileUpload(string uploadType,string corpId, string policyId,string fileName, DateTime dateNow,string uploadBy, string mappedFileName,string localPath, string destinationPath, int version, int recordCount)
        {

            object val;
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spInsFileUpload";
                cmd.Parameters.Add("@UploadType", SqlDbType.VarChar).Value = uploadType;
                cmd.Parameters.Add("@CorpId", SqlDbType.VarChar).Value = corpId;
                cmd.Parameters.Add("@PolicyId", SqlDbType.VarChar).Value = policyId;
                cmd.Parameters.Add("@FileName", SqlDbType.VarChar).Value = fileName;
                cmd.Parameters.Add("@UploadedDateTime", SqlDbType.DateTime).Value = dateNow;
                cmd.Parameters.Add("@UploadedBy", SqlDbType.VarChar).Value = uploadBy;
                cmd.Parameters.Add("@MappedFileName", SqlDbType.VarChar).Value = mappedFileName;
                cmd.Parameters.Add("@LocalPath", SqlDbType.VarChar).Value = localPath;
                cmd.Parameters.Add("@DestinationPath", SqlDbType.VarChar).Value = destinationPath;
                cmd.Parameters.Add("@Version", SqlDbType.Int).Value = version;
                cmd.Parameters.Add("@RecordCount", SqlDbType.Int).Value = recordCount;

                var outParm = new SqlParameter("@ID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParm);
                
                cmd.ExecuteNonQuery();
                val = outParm.Value;

            }
            catch (Exception ex)
            {
                val = 0;
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in InsFileUpload. Error : " + ex.Message, "StoredProcService");
            }
            int Id =(int)val;
             
            return Id;
        }
        public int InsUserUploadFile(string fileName, DateTime dateNow, string uploadBy, byte[] binaryBits)
        {

            object val;
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spInsUserUploadFile";
                cmd.Parameters.Add("@FileName", SqlDbType.VarChar).Value = fileName;
                cmd.Parameters.Add("@UploadedDateTime", SqlDbType.DateTime).Value = dateNow;
                cmd.Parameters.Add("@UploadedBy", SqlDbType.VarChar).Value = uploadBy;
                cmd.Parameters.Add("@BinaryBits", SqlDbType.VarBinary).Value = binaryBits;

                var outParm = new SqlParameter("@ID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParm);

                cmd.ExecuteNonQuery();
                val = outParm.Value;

            }
            catch (Exception ex)
            {
                val = 0;
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in InsUserUploadFile. Error : " + ex.Message, "StoredProcService");
            }
            int Id = (int)val;

            return Id;
        }
        public int InsUserUpload(string userName, string fullName, string emailAddress, string mobilePhone, string gender, string icNo, string businessRegNo, string status, int id, bool isOwner, string exceptionMessage)
        {

            object val;
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spInsUserUpload";
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;
                cmd.Parameters.Add("@FullName", SqlDbType.VarChar).Value = fullName;
                cmd.Parameters.Add("@EmailAddress", SqlDbType.VarChar).Value = emailAddress;
                cmd.Parameters.Add("@MobilePhone", SqlDbType.VarChar).Value = mobilePhone;
                cmd.Parameters.Add("@Gender", SqlDbType.VarChar).Value = gender;
                cmd.Parameters.Add("@ICNo", SqlDbType.VarChar).Value = icNo;
                cmd.Parameters.Add("@BusinessRegNo", SqlDbType.NVarChar).Value = businessRegNo;
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value = status;
                cmd.Parameters.Add("@FileID", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@IsOwner", SqlDbType.Bit).Value = isOwner;
                cmd.Parameters.Add("@ExceptionMessage", SqlDbType.VarChar).Value = exceptionMessage;

                var outParm = new SqlParameter("@ID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParm);

                cmd.ExecuteNonQuery();
                val = outParm.Value;

            }
            catch (Exception ex)
            {
                val = 0;
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in InsUserUpload. Error : " + ex.Message, "StoredProcService");
            }
            int Id = (int)val;

            return Id;
        }
        public int InsFileUploadFallout(int fileUploadId,string fallout)
        {

            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spInsFileUploadFallout"
                };
                cmd.Parameters.Add("@FileUploadId", SqlDbType.Int).Value = fileUploadId;
                cmd.Parameters.Add("@Fallout", SqlDbType.VarChar).Value = fallout;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            { 
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in InsFileUploadFallout. Error : " + ex.Message, "StoredProcService");
            }
            return 1;
        }
        
        public DataTable GetFileUpload(string corpId)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetFileUpload"
                };
                cmd.Parameters.Add("@CorporateId", SqlDbType.VarChar).Value = corpId;
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetFileUpload. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }

        public DataTable GetUserUploadFileByUserName(string userRole)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetUserUploadFileByUserName"
                };
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userRole;
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetUserUploadFileByUserName. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetAuditLog(string userName, string searchString, string searchType, DateTime searchStartDate, DateTime searchEndDate)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetAuditLog"
                };
                cmd.Parameters.Add("@SearchString", SqlDbType.VarChar).Value = searchString;
                cmd.Parameters.Add("@SearchType", SqlDbType.VarChar).Value = searchType;
                cmd.Parameters.Add("@SearchStartDate", SqlDbType.DateTime).Value = searchStartDate;
                cmd.Parameters.Add("@SearchEndDate", SqlDbType.DateTime).Value = searchEndDate;
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetAuditLog. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
        public DataTable GetFileUploadFallouts(int fileUploadId)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetFileUploadFallouts"
                };
                cmd.Parameters.Add("@FileUploadId", SqlDbType.Int).Value = fileUploadId;
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetFileUploadFallouts. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }

        public bool IsUserExist(string userName)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spIsUserExist"
                };
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in IsUserExist. Error : " + ex.Message, "StoredProcService");
            }

            return dt.Rows.Count > 0;
        }

        public DataTable GetUserDetails(string userName)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spIsUserExist"
                };
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetUserDetails. Error : " + ex.Message, "StoredProcService");
            }

            return dt;
        }
        public DataTable GetUserPasswordHistory(string userName)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetUserPasswordHistory"
                };
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetUserPasswordHistory. Error : " + ex.Message, "StoredProcService");
            }

            return dt;
        }
        public void RemoveOldUserPasswordHistory(string userName, int noOfRecordsToDelete)
        {
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spRemoveOldUserPasswordHistory";
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;
                cmd.Parameters.Add("@NoOfPasswordHistory", SqlDbType.Int).Value = noOfRecordsToDelete;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in RemoveOldUserPasswordHistory. Error : " + ex.Message, "StoredProcService");
            }
        }
        public int InsUserPasswordHistory(string userName, string hash)
        {
            object val;
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spInsUserPasswordHistory";
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;
                cmd.Parameters.Add("@Hash", SqlDbType.VarChar).Value = hash;

                var outParm = new SqlParameter("@ID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParm);

                cmd.ExecuteNonQuery();
                val = outParm.Value;

            }
            catch (Exception ex)
            {
                val = 0;
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in InsUserPasswordHistory. Error : " + ex.Message, "StoredProcService");
            }
            int Id = (int)val;

            return Id;
        }
        public void UpdateLastLoginDate(string userName)
        {

            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spUpdLastLoginDate";
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in UpdateLastLoginDate. Error : " + ex.Message, "StoredProcService");
            }
        }
        public void UpdateUserIsActive(string userName, bool isActive)
        {
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spUpdateUserIsActive";
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;
                cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in UpdateUserIsActive. Error : " + ex.Message, "StoredProcService");
            }
        }
        public void UpdateUserIsDelete(string userName, bool isDelete)
        {
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spUpdateUserIsDelete";
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;
                cmd.Parameters.Add("@IsDelete", SqlDbType.Bit).Value = isDelete;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in UpdateUserIsDelete. Error : " + ex.Message, "StoredProcService");
            }
        }
        public void UpdateLastPasswordChangeDate(string userName)
        {
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spUpdLastPasswordChangeDate";
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in UpdateLastPasswordChangeDate. Error : " + ex.Message, "StoredProcService");
            }
        }
        public void UpdateFileUploadStatusByMappedFileName(string MappedFileName, string Status)
        {
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spUpdFileUploadStatus";
                cmd.Parameters.Add("@MappedFileName", SqlDbType.VarChar).Value = MappedFileName;
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value = Status;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in UpdateFileUploadStatus. Error : " + ex.Message, "StoredProcService");
            }
        }

        public void UpdateFileUploadStatusByID(int ID, string Status)
        {
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spUpdFileUploadStatusByID";
                cmd.Parameters.Add("@ID", SqlDbType.Int).Value = ID;
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value = Status;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in UpdateFileUploadStatusByID. Error : " + ex.Message, "StoredProcService");
            }
        }

        public DataTable GetPolicyDetails(string policyId, string corporateId,string UCorpId = null)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetPolicyDetails"
                };
                cmd.Parameters.Add("@PolicySourceId", SqlDbType.VarChar).Value = policyId;
                cmd.Parameters.Add("@CorpSourceId", SqlDbType.VarChar).Value = corporateId;
                cmd.Parameters.Add("@UCorpId", SqlDbType.VarChar).Value = UCorpId;
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetPolicyDetails. Error : " + ex.Message, "StoredProcService");
            }
            return dt;

        }

        public DataTable GetPolicyProduct(string policyId)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetPolicyProduct"
                };
                cmd.Parameters.Add("@PolicySourceId", SqlDbType.VarChar).Value = policyId;
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetPolicyProduct. Error : " + ex.Message, "StoredProcService");
            }
            return dt;

        }

        public void UpdateLastLoginAttempt(string userName, bool reinitiate)
        {
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spUpdateLastLoginAttempt";
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;
                cmd.Parameters.Add("@Reinitiate", SqlDbType.Int).Value = !reinitiate ? 0 : 1;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in UpdateLastLoginAttempt. Error : " + ex.Message, "StoredProcService");
            }
        }
        public void UpdateMemberClaimsEFormByID(int memberClaimsId, byte[] eFormBytes)
        {
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spUpdateMemberClaimsEFormByID";
                cmd.Parameters.Add("@ID", SqlDbType.Int).Value = memberClaimsId;
                cmd.Parameters.Add("@Base64ApplicationForm", SqlDbType.VarBinary).Value = eFormBytes;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in UpdateMemberClaimsEFormByID. Error : " + ex.Message, "StoredProcService");
            }
        }

        public void ProcessCreateUser(string userName, string fullName, string corpId, bool isOwner, string emailAddress, string phoneNo, string gender, string icNo, int roleID)
        {
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spProcessCreateUser";
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;
                cmd.Parameters.Add("@FullName", SqlDbType.VarChar).Value = fullName;
                cmd.Parameters.Add("@BusinessRegNo", SqlDbType.VarChar).Value = corpId;
                cmd.Parameters.Add("@IsOwner", SqlDbType.Bit).Value = isOwner;
                cmd.Parameters.Add("@EmailAddress", SqlDbType.VarChar).Value = emailAddress;
                cmd.Parameters.Add("@MobilePhone", SqlDbType.VarChar).Value = phoneNo;
                cmd.Parameters.Add("@Gender", SqlDbType.VarChar).Value = gender;
                cmd.Parameters.Add("@ICNo", SqlDbType.VarChar).Value = icNo;
                cmd.Parameters.Add("@RoleID", SqlDbType.Int).Value = roleID;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in ProcessCreateUser. Error : " + ex.Message, "StoredProcService");
            }
        }
        public void ProcessDeleteUser(string userName)
        {
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spProcessDeleteUser";
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in ProcessDeleteUser. Error : " + ex.Message, "StoredProcService");
            }
        }
        public void UpdateUserDetails(string userName, string emailAddress, string phoneNo)
        {
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spUpdateUserDetails";
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;
                cmd.Parameters.Add("@EmailAddress", SqlDbType.VarChar).Value = emailAddress;
                cmd.Parameters.Add("@MobilePhone", SqlDbType.VarChar).Value = phoneNo;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in UpdateUserDetails. Error : " + ex.Message, "StoredProcService");
            }
        }

        public DataTable GetInsuredGroupSubsidaries(string corporateId,string userName,string policyId,bool IsOwner)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetInsuredGroupSubsidaries"
                };
                cmd.Parameters.Add("@CorpId", SqlDbType.VarChar).Value = corporateId;
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;
                cmd.Parameters.Add("@PolicyId", SqlDbType.VarChar).Value = policyId;
                cmd.Parameters.Add("@IsOwner", SqlDbType.Bit).Value = IsOwner;
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetInsuredGroupSubsidaries. Error : " + ex.Message, "StoredProcService");
            }
            return dt;

        }
        public DataTable GetCorporateByUserName(string userName,string UCorpId)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetCorporateByUserName"
                };
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;
                cmd.Parameters.Add("@UCorpId", SqlDbType.VarChar).Value = UCorpId;
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetCorporateByUserName. Error : " + ex.Message, "StoredProcService");
            }

            //return dt.Rows[0]["SourceId"].ToString();
            return dt;
        }
        public int InsAuditTrail(DateTime date,string type, string userName, string description , string menu)
        {

            object val;
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spInsAuditTrail";
                cmd.Parameters.Add("@Date", SqlDbType.DateTime).Value = date;
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;
                cmd.Parameters.Add("@Type", SqlDbType.VarChar).Value = type;
                cmd.Parameters.Add("@Description", SqlDbType.VarChar).Value = description;
                cmd.Parameters.Add("@Menu", SqlDbType.VarChar).Value = menu;

                var outParm = new SqlParameter("@ID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParm);

                cmd.ExecuteNonQuery();
                val = outParm.Value;
            }
            catch (Exception ex)
            {
                val = 0;
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetCorporateByUserName. Error : " + ex.Message, "StoredProcService");
            }
            int Id = (int)val;

            return Id;
        }
        public DataTable GetFileUploadMandatoryColumn(string product)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetFileUploadMandatoryColumn"
                };
                cmd.Parameters.Add("@Product", SqlDbType.VarChar).Value = product;
               
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetFileUploadMapping. Error : " + ex.Message, "StoredProcService");
            }

             
            return dt;
        }

        public DataTable GetFileUploadMappingColumns()
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetFileUploadMappingColumn"
                };


                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetFileUploadMappingColumns. Error : " + ex.Message, "StoredProcService");
            }
            

            return dt;
        }

        public bool IsFileUploadExist(string FileName, string ROC, string ContractNo, int Version)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spIsFileUploadExist"
                };
                cmd.Parameters.Add("@FileName", SqlDbType.VarChar).Value = FileName;
                cmd.Parameters.Add("@ROC", SqlDbType.VarChar).Value = ROC;
                cmd.Parameters.Add("@ContractNo", SqlDbType.VarChar).Value = ContractNo;
                cmd.Parameters.Add("@Version", SqlDbType.Int).Value = Version;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in IsFileUploadExist. Error : " + ex.Message, "StoredProcService");
            }

            return dt.Rows.Count > 0;
        }

        public bool IsCorporateExist(string CorpID)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetCorporate"
                };
                cmd.Parameters.Add("@CorporateId", SqlDbType.VarChar).Value = CorpID;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in IsCorporateExist. Error : " + ex.Message, "StoredProcService");
            }

            return dt.Rows.Count > 0;
        }

        public bool IsUserUploadFileExist(string fileName, string userName)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spIsFileUploadExist"
                };
                cmd.Parameters.Add("@FileName", SqlDbType.VarChar).Value = fileName;
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userName;

                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in IsUserUploadFileExist. Error : " + ex.Message, "StoredProcService");
            }

            return dt.Rows.Count > 0;
        }

        public void UpdateFileUploadLinks(int ID, string RPAExceptionLink, string CGLSExceptionLink, string InvoiceLink, string ExceptionLink)
        {
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spUpdFileUploadLinks";
                cmd.Parameters.Add("@ID", SqlDbType.Int).Value = ID;
                cmd.Parameters.Add("@RPAExceptionLink", SqlDbType.VarChar).Value = RPAExceptionLink;
                cmd.Parameters.Add("@CGLSExceptionLink", SqlDbType.VarChar).Value = CGLSExceptionLink;
                cmd.Parameters.Add("@InvoiceLink", SqlDbType.VarChar).Value = InvoiceLink;
                cmd.Parameters.Add("@ExceptionLink", SqlDbType.VarChar).Value = ExceptionLink;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in UpdateFileUploadLinks. Error : " + ex.Message, "StoredProcService");
            }
        }

        public void UpdateFileUploadBinary(int id, byte[] fileBytes, string fileType)
        {
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spUpdFileUploadBinary";
                cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@BinaryBits", SqlDbType.VarBinary).Value = fileBytes;
                cmd.Parameters.Add("@FileType", SqlDbType.VarChar).Value = fileType;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in UpdateFileUploadBinary. Error : " + ex.Message, "StoredProcService");
            }
        }

        public void UpdateUserUploadStatus(int id, string exceptionMessage, string status)
        {
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spUpdUserUploadStatus";
                cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@ExceptionMessage", SqlDbType.VarChar).Value = exceptionMessage;
                cmd.Parameters.Add("@Status", SqlDbType.VarChar).Value = status;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in UpdateUserUploadStatus. Error : " + ex.Message, "StoredProcService");
            }
        }

        public void UpdateUserUploadFileExceptionStatus(int id, byte[] fileBinary, string status, string approvedBy)
        {
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spUpdUserUploadFileException";
                cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                if (fileBinary != null)
                    cmd.Parameters.Add("@BinaryBits", SqlDbType.VarBinary).Value = fileBinary;
                cmd.Parameters.Add("@Status", SqlDbType.VarChar).Value = status;
                cmd.Parameters.Add("@ApprovedBy", SqlDbType.VarChar).Value = approvedBy;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in UpdateUserUploadFileExceptionStatus. Error : " + ex.Message, "StoredProcService");
            }
        }

        public void RemoveUserUploadFile(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn
                };
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                else
                {
                    cmd.Connection.Close();
                    conn.Open();
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spRemoveUserUploadFile";
                cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in RemoveUserUploadFile. Error : " + ex.Message, "StoredProcService");
            }
        }

        public DataTable GetUserUploadByFileId(int fileUploadId)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetUserUploadByFileId"
                };
                cmd.Parameters.Add("@FileUploadId", SqlDbType.Int).Value = fileUploadId;
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetUserUploadByFileId. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }

        public DataTable GetUserUploadFileById(int fileUploadId)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetUserUploadFileById"
                };
                cmd.Parameters.Add("@FileUploadId", SqlDbType.Int).Value = fileUploadId;
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetUserUploadFileById. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }

        public DataTable GetFileUploadById(int fileUploadId)
        {
            var dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spGetFileUploadById"
                };
                cmd.Parameters.Add("@FileUploadId", SqlDbType.Int).Value = fileUploadId;
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, AuditType.Error, UserName, "Error in GetFileUploadById. Error : " + ex.Message, "StoredProcService");
            }
            return dt;
        }
    }
}
