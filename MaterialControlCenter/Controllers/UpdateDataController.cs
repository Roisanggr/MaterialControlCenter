using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using System.Web.Services.Description;
using MaterialControlCenter.Models;
using System.Data.SqlClient;


namespace MaterialControlCenter.Controllers
{
    public class UpdateDataController : BaseController
    {
        // GET: UpdateData
        [HttpPost]
        public JsonResult UpdateApprovals(List<ApprovalUpdateModel> updates)
        {
            if (updates == null || updates.Count == 0)
            {
                return Json(new { success = false, message = "No updates provided." });
            }

            try
            {
               
                var updateTuples = updates
                    .Select(u => (u.ApprovalListId, u.NewKpk))
                    .ToList();

             
                var result = dbCentralizedNotification.UpdateMultipleApprovals(updateTuples);

                return Json(new { success = result, message = "Approvals updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateUser(UpdateUserRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Kpk))
            {
                return Json(new { success = false, message = "KPK must be provided." });
            }

            try
            {
                var result = dbScrap.UpdateUser(request);

                return Json(new
                {
                    success = result,
                    message = result
                        ? "User has been successfully updated."
                        : "User not found or no data was updated."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred while updating the user: {ex.Message}" });
            }
        }
        [HttpPost]
        public async Task<JsonResult> AddUser(UserModelInsert request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Kpk) || string.IsNullOrWhiteSpace(request.Name))
            {
                return Json(new { success = false, message = "KPK and Name must be provided." });
            }

            try
            {
               
                var result = await dbScrap.InsertUserAsync(request);

                return Json(new
                {
                    success = result,
                    message = result
                        ? "User has been successfully added."
                        : "Failed to add user. Maybe user already exists."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred while adding the user: {ex.Message}" });
            }
        }
        [HttpPost]
        public async Task<JsonResult> AddOrReactivateUser(UserUpsertRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Kpk))
            {
                return Json(new { success = false, message = "KPK is required." });
            }

            try
            {
                string addedByKpk = Session["Kpk"]?.ToString();

                if (string.IsNullOrWhiteSpace(addedByKpk))
                {
                    return Json(new { success = false, message = "Session expired." });
                }

                var result = await dbScrap.AddOrReactivateUserAsync(
                    request,
                    addedByKpk
                );

                return Json(new
                {
                    success = result,
                    message = result
                        ? "User processed successfully."
                        : "User already active."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public JsonResult UpdateSourceDataStatus(int id, int status)
        {
            try
            {

                bool result = dbCentralizedNotification.UpdateSourceDataStatus(id, status);

                if (!result)
                    return Json(new { success = false, message = "Data not found or no rows were updated." });

                return Json(new { success = true, message = "Status updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeleteScrap(string idScrap, int sourceDataId)
        {
            if (string.IsNullOrWhiteSpace(idScrap) || sourceDataId <= 0)
            {
                return Json(new { success = false, message = "Invalid parameters." });
            }

            try
            {
                bool masterUpdated = await dbScrap.SoftDeleteScrapMasterAsync(idScrap);
                if (!masterUpdated)
                {
                    return Json(new { success = false, message = "Scrap master not found or already deleted." });
                }

                bool sourceDataUpdated = dbCentralizedNotification.UpdateSourceDataStatus(sourceDataId, 16);
                if (!sourceDataUpdated)
                {
                    return Json(new { success = false, message = "Failed to update Centralized_SourceData status." });
                }

                return Json(new { success = true, message = "Scrap has been successfully deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<JsonResult> AddUserDelegate()
        {
            Request.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
            string jsonData = new System.IO.StreamReader(Request.InputStream).ReadToEnd();
            var delegates = Newtonsoft.Json.JsonConvert.DeserializeObject<List<UserDelegate>>(jsonData);

            if (delegates == null || delegates.Count == 0)
                return Json(new { success = false, message = "No delegates provided." });

            try
            {
                int successCount = 0;
                foreach (var request in delegates)
                {
                    if (string.IsNullOrWhiteSpace(request.UserKpk) ||
                        string.IsNullOrWhiteSpace(request.DelegateKpk) ||
                        string.IsNullOrWhiteSpace(request.DelegateTime))
                        continue;

                    var newId = await dbScrap.InsertUserDelegateAsync(request);
                    if (newId > 0) successCount++;
                }

                return Json(new
                {
                    success = successCount > 0,
                    message = successCount > 0 ? "Delegates successfully added." : "Failed to add delegates."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpGet]
        public async Task<JsonResult> GetUserDelegates(
            string searchUserKpk = null,
            string searchDelegateKpk = null,
            string searchUserName = null,
            string searchDelegateName = null,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var allDelegates = await dbScrap.GetUserDelegatesAsync();
                var users = dbScrap.GetAllUsersScrap();
                var roles = dbScrap.GetAllRoles();
                var employees = dbSSO.GetEmployeeMasterSSO();
                if (!string.IsNullOrWhiteSpace(searchUserKpk))
                    allDelegates = allDelegates
                        .Where(x => string.Equals(x.UserKpk, searchUserKpk, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                if (!string.IsNullOrWhiteSpace(searchDelegateKpk))
                    allDelegates = allDelegates
                        .Where(x => string.Equals(x.DelegateKpk, searchDelegateKpk, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                if (!string.IsNullOrWhiteSpace(searchUserName))
                {
                    var matchedKpks = users
                        .Where(u => !string.IsNullOrEmpty(u.Name) &&
                                    u.Name.IndexOf(searchUserName, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Select(u => u.Kpk)
                        .ToList();

                    allDelegates = allDelegates
                        .Where(x => matchedKpks.Contains(x.UserKpk))
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(searchDelegateName))
                {
                    var matchedKpks = users
                        .Where(u => !string.IsNullOrEmpty(u.Name) &&
                                    u.Name.IndexOf(searchDelegateName, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Select(u => u.Kpk)
                        .ToList();

                    allDelegates = allDelegates
                        .Where(x => matchedKpks.Contains(x.DelegateKpk))
                        .ToList();
                }
                var resultWithNamesAndRoles = allDelegates.Select(d =>
                {
                    var user = users.FirstOrDefault(u => u.Kpk == d.UserKpk);
                    var delegateEmp = users.FirstOrDefault(u => u.Kpk == d.DelegateKpk);

                    var userSSO = employees.FirstOrDefault(e => e.Kpk == d.UserKpk);
                    var delegateSSO = employees.FirstOrDefault(e => e.Kpk == d.DelegateKpk);

                    var delegateRole = delegateEmp != null && delegateEmp.RoleId.HasValue
                        ? roles.FirstOrDefault(r => r.RoleId == delegateEmp.RoleId.Value)?.Name ?? ""
                        : "";

                    var userRole = user != null && user.RoleId.HasValue
                        ? roles.FirstOrDefault(r => r.RoleId == user.RoleId.Value)?.Name ?? ""
                        : "";

                    return new
                    {
                        d.Id,
                        d.UserKpk,
                        UserName = userSSO?.Name ?? user?.Name ?? "",
                        UserRole = userRole,
                        d.DelegateKpk,
                        DelegateName = delegateSSO?.Name ?? delegateEmp?.Name ?? "",
                        DelegateRole = delegateRole,
                        d.DelegateTime
                    };
                }).ToList();


                // --- Pagination ---
                var totalCount = resultWithNamesAndRoles.Count;
                var pagedResult = resultWithNamesAndRoles
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = pagedResult,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize,
                        totalRecords = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public async Task<JsonResult> InsertDelegateApprovalRecord(int approvalListId, int statusListId, string delegateKpk)
        {
            try
            {
                var record = new DelegateApprovalRecord
                {
                    Centralized_ApprovalList_ID = approvalListId,
                    Centralized_StatusList_ID = statusListId,
                    Delegate_ApprovalList_KpkApproval = delegateKpk
                };

                var newId = await dbScrap.InsertDelegateApprovalRecordAsync(record);

                return Json(new { success = true, id = newId }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeleteUserDelegate(int id)
        {
            try
            {
                bool isDeleted = await dbScrap.DeleteUserDelegateAsync(id);

                if (isDeleted)
                {
                    return Json(new { success = true, message = "Successfully deleted." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Data not found." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


    }
}