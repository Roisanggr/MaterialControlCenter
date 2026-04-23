using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MaterialControlCenter.Controllers
{
    public class AutomateFlowController : BaseController
    {

        private async Task<string> TriggerLogicApp(HttpClient client, string url, object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpException((int)response.StatusCode, responseContent);

            return responseContent;
        }

        [HttpGet]
        public async Task<ActionResult> TriggerApproval(int approvalListId, int status, int sourceDataID)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var responseContent1 = await TriggerLogicApp(
                        client,
                        "https://default5f40b94dde924c81a62a4014455791.e6.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/a8f5892a94274723b826eba5657e2355/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=bNiRcB05cQPL1E97sincPIvnh6gR_TMo345Y--l2xE8",
                        new
                        {
                            ApprovalListID = approvalListId,
                            StatusListID = status,
                            sourceDataID = sourceDataID
                        }
                    );
                    int.TryParse(responseContent1, out int systemResponseFlow1);

                    if (systemResponseFlow1 != 200)
                    {
                        await dbCentralizedNotification.UpdateApprovalListStatusAsync(approvalListId, status);
                        await CheckAndUpdateMasterStatusAsync(sourceDataID);

                        return Json(new
                        {
                            success = true,
                            flowStatus = systemResponseFlow1,
                            message = "Approved without sending an email to the next approver."
                        }, JsonRequestBehavior.AllowGet);

                    }

                    string responseContent2 = null;
                    if (status != 3)
                    {
                        responseContent2 = await TriggerLogicApp(
                            client,
                            "https://default5f40b94dde924c81a62a4014455791.e6.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/b0c35a87b4b147058f4418e3b01bcfde/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=nx46XuilBqrku-woNjGQGFceOJDWlOguIUKEJWuLLZM",
                            new { sourceDataID = sourceDataID }
                        );
                    }

                    var message = status != 3
                        ? "Approval completed successfully."
                        : "Rejection completed successfully.";

                    return Json(new
                    {
                        success = true,
                        flowStatus = 200,
                        message
                    }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public async Task<ActionResult> TriggerApprovalManual(int approvalListId, int status, int sourceDataID)
        {
            try
            {
                // Update approval list
                await dbCentralizedNotification.UpdateApprovalListStatusAsync(approvalListId, status);

                // Update master status
                await CheckAndUpdateMasterStatusAsync(sourceDataID);

                return Json(new
                {
                    success = true,
                    statusCode = 200,
                    message = status == 3
                        ? "Rejected successfully."
                        : "Approved successfully."
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateApprovalStatus(int approvalListId, int sourceDataId, int status)
        {
            try
            {
                var updated = await dbCentralizedNotification.UpdateApprovalListStatusAsync(approvalListId, status);

                if (!updated)
                    return Json(new { success = false, message = "Failed to update approval list." });

                await CheckAndUpdateMasterStatusAsync(sourceDataId);

                return Json(new { success = true, message = "Status updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        public async Task CheckAndUpdateMasterStatusAsync(int sourceDataId)
        {
            var statuses = await dbCentralizedNotification.GetStatusesBySourceDataIdAsync(sourceDataId);

            if (statuses.Count == 0)
                return;

            if (statuses.Any(s => s == 3))
            {
                await dbCentralizedNotification.UpdateMasterStatusAsync(sourceDataId, 5);
                return;
            }

            if (statuses.All(s => s == 2))
            {
                await dbCentralizedNotification.UpdateMasterStatusAsync(sourceDataId, 4);
                return;
            }
            return;
        }

    }
}