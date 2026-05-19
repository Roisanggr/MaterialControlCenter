using MaterialControlCenter.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
using System.Windows.Media.Imaging;

namespace MaterialControlCenter.Controllers
{
    public class DataFormsController : BaseController
    {

        public ActionResult GetPartMasterUpdated(string data, string tc, bool exact = false)
        {
            try
            {
                var partMasterList = dbScrap.getPartMasterUpdated(data, tc, exact);

                foreach (var part in partMasterList)
                {
                    part.PartNum = $"{part.ToyNum}-{part.PartNum}";
                }

                return Json(new { success = true, data = partMasterList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Internal error: " + ex.Message);
            }
        }

        public ActionResult GetTcAndType()
        {
            try
            {

                List<TcAndTypeMaster> tcAndTypes = dbScrap.getTcAndType();

                if (tcAndTypes != null && tcAndTypes.Count > 0)
                {
                    return Json(new { success = true, data = tcAndTypes }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "No data found!" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = "An error occurred: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetTcAndTypeWithFacility(string facility)
        {
            try
            {
                var tcAndTypes = dbScrap.getTcAndTypeByFacility(facility);

                return Json(new
                {
                    success = true,
                    data = tcAndTypes
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult getScrapCodeByLocation(string location, int? TC = null)
        {
            List<ScrapCode> scrapCodeData = new List<ScrapCode>();


            switch (location)
            {
                case "P1":
                    scrapCodeData = dbScrap.getScrapCodeByLocation("MIDC");
                    break;
                case "P2":
                    scrapCodeData = dbScrap.getScrapCodeByLocation("Fashion Dolls");
                    break;
                case "P2S3":
                    scrapCodeData = dbScrap.getScrapCodeByLocation("P2S3");
                    break;
                case "P2S4":
                    scrapCodeData = dbScrap.getScrapCodeByLocation("P2S4");
                    break;
                case "P2S5":
                    scrapCodeData = dbScrap.getScrapCodeByLocation("P2S5");
                    break;
                default:
                    scrapCodeData = dbScrap.getScrapCodeByLocation(null);
                    break;
            }

            var allTcData = dbScrap.getTcAndType();
            var tcInfo = allTcData.FirstOrDefault(t => t.TC == 22);
            List<string> qvCodes = new List<string>();

            if (tcInfo != null && !string.IsNullOrEmpty(tcInfo.ShowScrapCode))
            {

                qvCodes = tcInfo.ShowScrapCode.Split(',').Select(c => c.Trim()).ToList();
            }

            if (TC.HasValue)
            {
                if (TC.Value == 22 && qvCodes.Any())
                {

                    scrapCodeData = scrapCodeData
                        .Where(s => qvCodes.Contains(s.Code))
                        .ToList();
                }
                else
                {
                    scrapCodeData = scrapCodeData
                        .Where(s => !qvCodes.Contains(s.Code))
                        .ToList();
                }
            }
            else
            {
                scrapCodeData = scrapCodeData
                    .Where(s => !qvCodes.Contains(s.Code))
                    .ToList();
            }

            var orderedData = scrapCodeData.OrderBy(s => s.Code).ToList();

            return Json(new { success = true, data = orderedData }, JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public JsonResult GetScrapCodeRemarks(string scrapCode = "", string remarks = "", string application = "", string location = "", string area = "")
        {
            try
            {
                var locationMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "MIDC",          "P1"   },
            { "Fashion Dolls", "P2"   },
            { "P2S3",          "P2S3" },
            { "P2S4",          "P2S4" },
            { "P2S5",          "P2S5" }
        };

                List<ScrapCode> allScrapCodes = dbScrap.GetAllScrapCodes();
                List<ScrapCode> allPiaCodes = dbScrap.GetAllPiaCodes();
                List<ScrapCode> allTprCodes = dbScrap.GetAllTprCodes();

                // Scrap remarks
                var scrapData = dbScrap.GetScrapCodeRemarks().Select(r => new
                {
                    r.IdRemarks,
                    r.ScrapCode,
                    r.Remarks,
                    Application = "Scrap",
                    Location = locationMap.TryGetValue(
                        allScrapCodes.FirstOrDefault(c => c.Code == r.ScrapCode)?.Location ?? "", out string loc)
                        ? loc
                        : (allScrapCodes.FirstOrDefault(c => c.Code == r.ScrapCode)?.Location ?? "-")
                });

                // PIA remarks
                var piaData = dbScrap.GetPiaCodeRemarks().Select(r => new
                {
                    r.IdRemarks,
                    r.ScrapCode,
                    r.Remarks,
                    Application = "PIA",
                    Location = allPiaCodes.FirstOrDefault(c => c.Code == r.ScrapCode)?.Location ?? "-"
                });

                // TPR remarks
                var tprData = dbScrap.GetTprCodeRemarks().Select(r => new
                {
                    r.IdRemarks,
                    r.ScrapCode,
                    r.Remarks,
                    Application = "TPR",
                    Location = allTprCodes.FirstOrDefault(c => c.Code == r.ScrapCode)?.Location ?? "-"
                });

                // Gabungkan semua ke anonymous type yang seragam
                var allData = scrapData
                    .Select(r => new { r.IdRemarks, r.ScrapCode, r.Remarks, r.Application, r.Location })
                    .Concat(piaData
                        .Select(r => new { r.IdRemarks, r.ScrapCode, r.Remarks, r.Application, r.Location }))
                    .Concat(tprData
                        .Select(r => new { r.IdRemarks, r.ScrapCode, r.Remarks, r.Application, r.Location }))
                    .ToList();

                // Filter
                if (!string.IsNullOrEmpty(scrapCode))
                    allData = allData.Where(r => r.ScrapCode != null && r.ScrapCode.Contains(scrapCode)).ToList();

                if (!string.IsNullOrEmpty(remarks))
                    allData = allData.Where(r => r.Remarks != null && r.Remarks.Contains(remarks)).ToList();

                if (!string.IsNullOrEmpty(application))
                    allData = allData.Where(r => r.Application.Equals(application, StringComparison.OrdinalIgnoreCase)).ToList();

                if (!string.IsNullOrEmpty(location))
                    allData = allData.Where(r => r.Location != null && r.Location.Equals(location, StringComparison.OrdinalIgnoreCase)).ToList();

                return Json(new
                {
                    success = true,
                    data = allData.OrderBy(r => r.Application).ThenBy(r => r.ScrapCode).ToList()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace }, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult GetWorkCenters(string facility = null)
        {
            try
            {
                var data = dbScrap.GetWorkCenters(facility);
                return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetEmployeeByNameOrKpk(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Json(new { success = false, message = "Invalid input" }, JsonRequestBehavior.AllowGet);

            var employees = dbSSO.GetEmployeeMasterSSO();

            var found = employees
                .Where(e =>
                    (e.Kpk != null && e.Kpk.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (e.Name != null && e.Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                )
                .Take(10)
                .Select(e => new
                {
                    kpk = e.Kpk,
                    name = e.Name
                })
                .ToList();

            if (found.Any())
            {
                return Json(new { success = true, data = found }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "Not found" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetScrapCodeApprovalsWithUsers()
        {

            List<MccApprovalRule> approvals = dbScrap.GetMccApprovalRules("SCRAP");


            List<UserToolRoomModel> allUsers = dbScrap.GetAllUsersScrap();


            var grouped = approvals
                .OrderBy(a => a.Code)
                .ThenBy(a => a.RoleId)
                .Select(a => new
                {
                    a.Id,
                    ScrapCode = a.Code,
                    Role_Id = a.RoleId,
                    a.RequiredApproverCount,
                    ScrapTcType = a.TcType,
                    minValue = a.MinValue,
                    maxValue = a.MaxValue,
                    commit = a.Cmmit,
                    a.Tc,
                    PriorityScrapCase = a.Priority,
                    Users = allUsers
                            .Where(u => u.RoleId == a.RoleId)
                            .Select(u => new
                            {
                                u.Kpk,
                                u.Facility,
                                u.CodeResponsibility
                            })
                            .ToList()
                })
                .ToList();

            return Json(new { success = true, data = grouped }, JsonRequestBehavior.AllowGet);
        }


        public List<EmployeeMasterModelSSO> GetSupervisorChain(string kpk)
        {
            var chain = new List<EmployeeMasterModelSSO>();
            var visited = new HashSet<string>();

            string currentKpk = kpk;

            while (!string.IsNullOrEmpty(currentKpk))
            {
                var employee = dbSSO.GetUserByKpkSSO(currentKpk);
                if (employee == null)
                    break;
                chain.Add(employee);
                if (string.IsNullOrEmpty(employee.Supervisor) || employee.Supervisor == currentKpk)
                    break;
                if (visited.Contains(employee.Supervisor))
                    break;

                visited.Add(currentKpk);
                currentKpk = employee.Supervisor;
            }

            return chain;
        }

        [HttpGet]
        public JsonResult GetSupervisorChainByKpk(string kpk)
        {
            try
            {
                var chain = GetSupervisorChain(kpk);
                return Json(new
                {
                    success = true,
                    data = chain.Select(c => new {
                        c.Kpk,
                        c.Name,
                        c.Email,
                        c.Supervisor
                    }).ToList()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }




        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> InsertScrapAsync(ScrapRequest request)
        {
            string newIdScrap = null;
            int centralizedId = 0;

            try
            {
                if (request.detectedKPKGlobal == null || !request.detectedKPKGlobal.Any())
                {
                    Response.StatusCode = 400;
                    return Json(new { Error = "Approval list is empty. Please wait for the system to map approvers or check your inputs." });
                }

                string decodedPdf = null;
                if (!string.IsNullOrEmpty(request.PdfBase64))
                {
                    decodedPdf = Uri.UnescapeDataString(request.PdfBase64);

                    // jsPDF 2.5.1 outputs: "data:application/pdf;filename=generated.pdf;base64,<data>"
                    // The old StartsWith("data:application/pdf;base64,") never matched because of
                    // the extra "filename=generated.pdf;" segment. Use IndexOf to handle any format.
                    int marker = decodedPdf.IndexOf(";base64,", StringComparison.OrdinalIgnoreCase);
                    if (marker >= 0)
                        decodedPdf = decodedPdf.Substring(marker + ";base64,".Length);
                }

                // Validate PDF base64 is present and decodable before any DB writes
                if (string.IsNullOrEmpty(decodedPdf))
                {
                    Response.StatusCode = 400;
                    return Json(new { Error = "PDF attachment is missing. Please try again." });
                }
                try
                {
                    Convert.FromBase64String(decodedPdf);
                }
                catch (FormatException)
                {
                    Response.StatusCode = 400;
                    return Json(new { Error = "PDF attachment is corrupted. Please try again." });
                }
                Debug.Print($"[InsertScrap] PDF validated OK — {decodedPdf.Length} chars");

                // Step 1 — Save all data to DB
                newIdScrap = dbScrap.InsertScrapMaster(request.Master);
                if (request.Parts != null && request.Parts.Any())
                {
                    foreach (var part in request.Parts)
                    {
                        part.IdScrap = newIdScrap;
                        dbScrap.InsertScrapPart(part);
                    }
                }

                var centralizedData = PrepareCentralizedSourceData(request, newIdScrap);
                centralizedId = dbCentralizedNotification.InsertCentralizedSourceData(centralizedData);
                await InsertInitiatorAndApprovalListAsync(centralizedId, request, newIdScrap, decodedPdf);

                // Verify the PDF was actually persisted in DB before triggering email
                string storedBase64 = await dbCentralizedNotification.GetNextPendingApprovalBase64Async(centralizedId);
                if (string.IsNullOrEmpty(storedBase64))
                {
                    Debug.Print($"[InsertScrap] CRITICAL — Base64 not found in DB for centralizedId={centralizedId}");
                    await TryRollbackAsync(newIdScrap, centralizedId);
                    Response.StatusCode = 500;
                    return Json(new { Error = "PDF attachment was not stored correctly. No data was saved. Please try again." });
                }
                Debug.Print($"[InsertScrap] DB verification OK — stored {storedBase64.Length} chars for centralizedId={centralizedId}");

                // Step 2 — Trigger email notification via Power Automate.
                // Only sourceDataID is sent — the PA flow reads the attachment from the DB itself.
                // If this fails the DB inserts above are rolled back so no data is kept.
                string paErrorMessage = null;
                try
                {
                    using (var client = new HttpClient())
                    {
                        var payload = new { sourceDataID = centralizedId };
                        var json = JsonConvert.SerializeObject(payload);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = await client.PostAsync(
                            "https://default5f40b94dde924c81a62a4014455791.e6.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/b0c35a87b4b147058f4418e3b01bcfde/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=nx46XuilBqrku-woNjGQGFceOJDWlOguIUKEJWuLLZM",
                            content
                        );

                        if (!response.IsSuccessStatusCode)
                        {
                            var body = await response.Content.ReadAsStringAsync();
                            paErrorMessage = $"Email notification service responded with HTTP {(int)response.StatusCode}. Detail: {body}";
                        }
                    }
                }
                catch (Exception paEx)
                {
                    paErrorMessage = $"Email notification service is unreachable: {paEx.Message}";
                }

                if (paErrorMessage != null)
                {
                    Debug.Print("[PowerAutomate] " + paErrorMessage);
                    await TryRollbackAsync(newIdScrap, centralizedId);
                    Response.StatusCode = 500;
                    return Json(new { Error = $"Email notification failed — no data was saved. Please try again.\n\nDetail: {paErrorMessage}" });
                }

                return Json(new
                {
                    Message = "Scrap Master, Parts, and Centralized data successfully saved",
                    IdScrap = newIdScrap,
                    PartsCount = request.Parts?.Count ?? 0,
                    CentralizedId = centralizedId
                });
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                if (ex.InnerException != null)
                    Debug.Print("Inner Exception: " + ex.InnerException.Message);

                await TryRollbackAsync(newIdScrap, centralizedId);
                Response.StatusCode = 500;
                return Json(new { Error = ex.Message });
            }
        }

        private async Task TryRollbackAsync(string idScrap, int centralizedId)
        {
            if (centralizedId > 0)
            {
                try
                {
                    await dbCentralizedNotification.RollbackCentralizedDataAsync(centralizedId);
                }
                catch (Exception ex1)
                {
                    Debug.Print("[Rollback Centralized Data] Failed: " + ex1.Message);
                }
            }

            if (!string.IsNullOrEmpty(idScrap))
            {
                try
                {
                    await dbScrap.RollbackScrapDataAsync(idScrap);
                }
                catch (Exception ex2)
                {
                    Debug.Print("[Rollback Scrap Data] Failed: " + ex2.Message);
                }
            }
        }



        private CentralizedSourceDataModel PrepareCentralizedSourceData(ScrapRequest request, string newIdScrap)
        {
            decimal totalQty = request.Parts?.Sum(p => p.Qty) ?? 0;
            decimal totalValue = request.Parts?.Sum(p => p.Value) ?? 0m;

            string tcDesc = $"in TC {request.Master.TC}";
            if (!string.IsNullOrEmpty(request.Master.SpecialCodeTcCompanion))
                tcDesc += $" and TC Companion {request.Master.SpecialCodeTcCompanion}";

            return new CentralizedSourceDataModel
            {
                Centralized_SystemList_ID = 2,
                Centralized_SourceData_TableName = "scrap_master",
                Centralized_SourceData_Master_ID = Convert.ToInt32(newIdScrap),
                Centralized_SourceData_Master_Title = $"Document {newIdScrap} - Scrap Code {request.Master.ScrapCode}",
                Centralized_SourceData_Master_Desc = $"This document requests approval for a total of {totalQty} scrap units, with a total value of Rp {totalValue:N2} {tcDesc}.",
                Centralized_SourceData_Master_Status = 1,
                Centralized_SourceData_Master_CreatedDate = DateTime.Now
            };
        }



        private async Task InsertInitiatorAndApprovalListAsync(int centralizedId, ScrapRequest request, string newIdScrap, string base64Pdf)
        {

            string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(newIdScrap));
            string link = $"http://ptmi-stage/ScrapNotice/ScrapRecords/DetailScrap?token={token}";

            if (!string.IsNullOrEmpty(request.Master.InitiatorKpk))
            {
                var initiator = new CentralizedInitiator
                {
                    Centralized_SourceData_ID = centralizedId,
                    Centralized_Initiator_KPK = request.Master.InitiatorKpk
                };

                try
                {
                    dbCentralizedNotification.InsertCentralizedInitiator(initiator);

                }
                catch (SqlException ex)
                {

                    foreach (SqlError err in ex.Errors)
                    {
                        Debug.Print($"[Initiator] Line {err.LineNumber}: {err.Message}");
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.Print("[Initiator] Exception: " + ex.Message);
                    throw;
                }
            }

            // --- Insert Approval List ---
            if (request.detectedKPKGlobal != null && request.detectedKPKGlobal.Any())
            {
                int step = 1;
                foreach (var k in request.detectedKPKGlobal)
                {
                    string kpkValue = k.kpk;

                    var approval = new CentralizedApprovalListModel
                    {
                        Centralized_SourceData_ID = centralizedId,
                        Centralized_ApprovalList_Step = step++,
                        Centralized_StatusList_ID = 1,
                        Centralized_ApprovalList_Date = null,
                        Centralized_ApprovalList_Link = link,
                        Centralized_ApprovalList_KpkApproval = kpkValue,
                        Centralized_ApprovalList_Base64 = base64Pdf
                    };

                    try
                    {
                        int insertedId = await dbCentralizedNotification.InsertApprovalListAsync(approval);

                    }
                    catch (SqlException ex)
                    {

                        foreach (SqlError err in ex.Errors)
                        {
                            Debug.Print($"[ApprovalList] Line {err.LineNumber}: {err.Message}");
                        }
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Debug.Print("[ApprovalList] Exception: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        public List<EmployeeMasterModelSSO> GetSubordinateChain(string kpk)
        {
            var result = new List<EmployeeMasterModelSSO>();
            var visited = new HashSet<string>();

            var queue = new Queue<string>();
            queue.Enqueue(kpk);

            while (queue.Count > 0)
            {
                string currentKpk = queue.Dequeue();
                var subordinates = dbSSO.GetUsersBySupervisorKpk(currentKpk);

                foreach (var emp in subordinates)
                {
                    if (!visited.Contains(emp.Kpk))
                    {
                        result.Add(emp);
                        visited.Add(emp.Kpk);
                        queue.Enqueue(emp.Kpk);
                    }
                }
            }

            return result;
        }


        [HttpPost]
        public JsonResult AddScrapDisposal(ScrapDisposalModel model)
        {
            try
            {

                bool success = dbScrap.InsertScrapDisposal(model);

                if (!success)
                    return Json(new { success = false, message = "Failed to insert data." });

                return Json(new { success = true, message = "Scrap disposal added successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetTypeScraps(bool includeDeleted = false)
        {
            var data = dbScrap.GetAllTypeScrap();
            if (!includeDeleted)
            {
                data = data.Where(x => x.IsDelete == false).ToList();
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SubmitScrapCode(ScrapCode model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Invalid data." }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(model.Application))
            {
                return Json(new { success = false, message = "Application must be selected." }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(model.Facility) && string.IsNullOrWhiteSpace(model.Location))
            {
                return Json(new { success = false, message = "Facility must be selected." }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(model.Code) ||
                (string.IsNullOrWhiteSpace(model.Description) && string.IsNullOrWhiteSpace(model.Name)))
            {
                return Json(new { success = false, message = "Code and Description are required." }, JsonRequestBehavior.AllowGet);
            }

            if (model.Application.Equals("PIA", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(model.Area))
            {
                return Json(new { success = false, message = "Area is required for PIA." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var createdByKpk = Kpk == "(unknown)" ? null : Kpk;
                var createdByName = string.IsNullOrWhiteSpace(Name) || Name == "No Name" ? null : Name;

                bool isSaved = dbScrap.SubmitScrapCode(model, createdByKpk, createdByName);

                if (isSaved)
                {
                    return Json(new { success = true, message = "Code successfully saved." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Failed to save code." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        



        [HttpPost]
        public JsonResult DeleteScrapCodeRemark(int id)
        {
            try
            {
                bool success = dbScrap.DeleteRemark(id);

                if (success)
                    return Json(new { success = true, message = "Remark successfully deleted." });
                else
                    return Json(new { success = false, message = "No remark was deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult DeleteScrapCode(int idRemarks)
        {
            try
            {
                bool isDeleted = dbScrap.DeleteScrapCode(idRemarks);

                if (isDeleted)
                {
                    return Json(new { success = true, message = "Scrap Code successfully deleted." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "No record found to delete." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAllScrapCode(
            int pageNumber = 1,
            int pageSize = 10,
            string scrapCode = "",
            string location = "",
            string application = "SCRAP",
            string area = "")
        {
            string selectedApplication = string.IsNullOrWhiteSpace(application)
                ? string.Empty
                : application.Trim().ToUpperInvariant();

            bool hasAreaFilter = !string.IsNullOrWhiteSpace(area);
            List<ScrapCode> allScrapCodes;

            if (hasAreaFilter)
            {
                allScrapCodes = new List<ScrapCode>();
                allScrapCodes.AddRange(dbScrap.GetAllScrapCodes());
                allScrapCodes.AddRange(dbScrap.GetAllPiaCodes());
                allScrapCodes.AddRange(dbScrap.GetAllTprCodes());

                if (!string.IsNullOrWhiteSpace(selectedApplication))
                {
                    allScrapCodes = allScrapCodes
                        .Where(s => !string.IsNullOrWhiteSpace(s.Application) && s.Application.Equals(selectedApplication, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
            }
            else
            {
                switch (selectedApplication)
                {
                    case "PIA":
                        allScrapCodes = dbScrap.GetAllPiaCodes();
                        break;
                    case "TPR":
                        allScrapCodes = dbScrap.GetAllTprCodes();
                        break;
                    default:
                        selectedApplication = "SCRAP";
                        allScrapCodes = dbScrap.GetAllScrapCodes();
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(scrapCode))
            {
                allScrapCodes = allScrapCodes
                    .Where(s => !string.IsNullOrWhiteSpace(s.Code) && s.Code.IndexOf(scrapCode, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                allScrapCodes = allScrapCodes
                    .Where(s => !string.IsNullOrWhiteSpace(s.Location) && s.Location.IndexOf(location, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(area))
            {
                allScrapCodes = allScrapCodes
                    .Where(s => !string.IsNullOrWhiteSpace(s.Area) && s.Area.IndexOf(area, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            int totalCount = allScrapCodes.Count;
            var pagedData = allScrapCodes
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Json(new
            {
                success = true,
                data = pagedData,
                totalCount = totalCount,
                application = string.IsNullOrWhiteSpace(selectedApplication) ? (hasAreaFilter ? "ALL" : "SCRAP") : selectedApplication
            }, JsonRequestBehavior.AllowGet);
        }



        public JsonResult GetAllScrapCodeSpecialApprovals(
           string scrapCode = null,
           int? roleId = null,
           int? requiredApproverCount = null,
           string scrapTcType = null,
           int? minValue = null,
           int? maxValue = null,
           string commit = null,
           int? priority = null,
           int pageNumber = 1,
           int pageSize = 10)
        {
            IEnumerable<ScrapCodeSpecialCaseApprovalRequirement> approvals = dbScrap.GetAllScrapCodeSpecialApprovals();
            List<RoleModel> roles = dbScrap.GetAllRoles();


            approvals = approvals
                .Where(a => string.IsNullOrEmpty(scrapCode) ||
                            (!string.IsNullOrEmpty(a.ScrapCode) && a.ScrapCode.IndexOf(scrapCode, StringComparison.OrdinalIgnoreCase) >= 0))
                .Where(a => !roleId.HasValue || a.Role_Id == roleId.Value)
                .Where(a => !requiredApproverCount.HasValue || a.RequiredApproverCount == requiredApproverCount.Value)
                .Where(a => string.IsNullOrEmpty(scrapTcType) ||
                            (!string.IsNullOrEmpty(a.ScrapTcType) && a.ScrapTcType.IndexOf(scrapTcType, StringComparison.OrdinalIgnoreCase) >= 0))
                .Where(a => !minValue.HasValue || a.minValue >= minValue.Value)
                .Where(a => !maxValue.HasValue || a.maxValue <= maxValue.Value)
                .Where(a => string.IsNullOrEmpty(commit) ||
                            (!string.IsNullOrEmpty(a.commit) && a.commit.IndexOf(commit, StringComparison.OrdinalIgnoreCase) >= 0))
                .Where(a => !priority.HasValue || a.PriorityScrapCase == priority.Value);
            var approvalsWithRoleName = approvals
                .Select(a => new
                {
                    a.Id,
                    a.ScrapCode,
                    a.Role_Id,
                    RoleName = roles.FirstOrDefault(r => r.RoleId == a.Role_Id)?.Name ?? "Unknown",
                    a.RequiredApproverCount,
                    a.ScrapTcType,
                    a.minValue,
                    a.maxValue,
                    a.commit,
                    a.PriorityScrapCase
                });

            // Urutkan + Pagination
            var sortedData = approvalsWithRoleName
                .OrderBy(a => a.ScrapCode)
                .ThenBy(a => a.PriorityScrapCase);

            int totalCount = sortedData.Count();

            var pagedData = sortedData
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Json(new
            {
                success = true,
                data = pagedData,
                totalCount = totalCount
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetScrapResponsibleByKpk(string kpk)
        {
            var allUsers = dbScrap.GetAllUsersScrap();

            // filter berdasarkan KPK
            var user = allUsers.FirstOrDefault(u => u.Kpk == kpk);

            if (user == null)
            {
                return Json(new { success = false, message = "KPK not found. Please contact Admin to register in the Scrap System." }, JsonRequestBehavior.AllowGet);
            }


            return Json(new
            {
                success = true,
                kpk = user.Kpk,
                scrapCodes = user.CodeResponsibility
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetTCCompanions()
        {
            try
            {
                var items = await dbScrap.GetTCCompanionsAsync();

                return Json(new
                {
                    success = true,
                    data = items
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetCodesByApplication(string application)
        {
            try
            {
                List<object> codes;

                switch ((application ?? "").Trim().ToUpper())
                {
                    case "PIA":
                        codes = dbScrap.GetAllPiaCodes()
                            .OrderBy(c => c.Location).ThenBy(c => c.Code)
                            .Select(c => new {
                                value = c.IdRemarks.ToString(),
                                label = $"{c.Location} - {c.Code} - {c.Name}"
                            })
                            .Cast<object>().ToList();
                        break;

                    case "TPR":
                        codes = dbScrap.GetAllTprCodes()
                            .OrderBy(c => c.Location).ThenBy(c => c.Code)
                            .Select(c => new {
                                value = c.Code,
                                label = $"{c.Location} - {c.Code} - {c.Name}"
                            })
                            .Cast<object>().ToList();
                        break;

                    default: // Scrap
                        var locationMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "MIDC",          "P1" },
                    { "Fashion Dolls", "P2" },
                    { "P2S3",          "P2S3" },
                    { "P2S4",          "P2S4" },
                    { "P2S5",          "P2S5" }
                };

                        codes = dbScrap.GetAllScrapCodes()
                            .OrderBy(c => c.Location).ThenBy(c => c.Code)
                            .Select(c => {
                                string displayLocation = locationMap.TryGetValue(c.Location ?? "", out string mapped)
                                    ? mapped
                                    : (c.Location ?? "-");
                                return new
                                {
                                    value = c.Code,
                                    label = $"{displayLocation} - {c.Code} - {c.Name}"
                                };
                            })
                            .Cast<object>().ToList();
                        break;
                }

                return Json(new { success = true, data = codes }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult AddScrapCodeRemark(string Application, string ScrapCode, string Remarks)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ScrapCode) || string.IsNullOrWhiteSpace(Remarks))
                    return Json(new { success = false, message = "Code and Remarks are required." });

                bool result;

                switch ((Application ?? "").Trim().ToUpper())
                {
                    case "PIA":
                        if (!int.TryParse(ScrapCode, out int piaCodeId))
                            return Json(new { success = false, message = "Invalid PIA Code." });
                        result = dbScrap.InsertPiaCodeRemark(piaCodeId, Remarks);
                        break;

                    case "TPR":
                        result = dbScrap.InsertTprCodeRemark(ScrapCode, Remarks);
                        break;

                    default: // Scrap
                        result = dbScrap.InsertScrapCodeRemark(ScrapCode, Remarks);
                        break;
                }

                if (result)
                    return Json(new { success = true, message = "Remarks saved successfully." });
                else
                    return Json(new { success = false, message = "Failed to save remarks." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetRemarksByPiaCode(string piaCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(piaCode))
                    return Json(new { success = false, message = "PIA Code is required." }, JsonRequestBehavior.AllowGet);

                var allRemarks = dbScrap.GetPiaCodeRemarks();

                var filtered = allRemarks
                    .Where(r => r.ScrapCode != null &&
                                r.ScrapCode.Equals(piaCode, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => r.Remarks)
                    .Select(r => new
                    {
                        Id = r.IdRemarks,
                        RemarksText = r.Remarks
                    })
                    .ToList();

                if (!filtered.Any())
                    return Json(new { success = false, message = "No remarks found for this PIA Code." }, JsonRequestBehavior.AllowGet);

                return Json(new { success = true, data = filtered }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetPiaCodeByLocation(string location, int? TC = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(location))
                    return Json(new { success = false, message = "Location is required." }, JsonRequestBehavior.AllowGet);

                var piaCodes = dbScrap.GetAllPiaCodes()
                    .Where(p => p.Location != null &&
                                p.Location.Equals(location, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p => p.Code)
                    .Select(p => new
                    {
                        Code = p.Code,
                        Name = p.Name,
                    })
                    .ToList();

                if (!piaCodes.Any())
                    return Json(new { success = false, message = "No PIA Code found for location: " + location }, JsonRequestBehavior.AllowGet);

                return Json(new { success = true, data = piaCodes }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetApprovalChain(
        string application,     // "SCRAP" | "PIA"
        string initiatorKpk,    // KPK user yang submit
        string facility,        // facility dokumen (P1, P2, P2S3, ...)
        string scrapCode,       // selected scrap/pia code from document header
        string partsJson,       // JSON: array of { value, code, tcType, commit }
        decimal? totalValue = null) // opsional — kalau ada override total
        {
            try
            {
                // ── 1. Parse parts ───────────────────────────────────────────────
                var parts = new List<PartApprovalContext>();
                if (!string.IsNullOrWhiteSpace(partsJson))
                {
                    try
                    {
                        parts = JsonConvert.DeserializeObject<List<PartApprovalContext>>(partsJson)
                                ?? new List<PartApprovalContext>();
                    }
                    catch
                    {
                        return Json(new { success = false, message = "Invalid partsJson format." },
                            JsonRequestBehavior.AllowGet);
                    }
                }

                // ── 2. Hitung max absolute value per-part ────────────────────────
                decimal maxAbsValue = parts.Any()
                    ? parts.Max(p => Math.Abs(p.Value))
                    : (totalValue ?? 0m);

                // ── 3. Ambil semua rules untuk aplikasi ini ──────────────────────
                var rules = dbScrap.GetMccApprovalRules(application);
                var allRoles = dbScrap.GetAllRoles();  // List<RoleModel> { RoleId, Name }
                var supChain = GetSupervisorChain(initiatorKpk);

                Debug.Print($"[GetApprovalChain] app={application}, scrapCode={scrapCode}, maxAbsValue={maxAbsValue}, " +
                            $"rules={rules.Count}, chainLen={supChain.Count}");

                // ── 4. Filter rules yang berlaku untuk context ini ───────────────
                var activeRules = rules.Where(rule =>
                {
                    // A. Value range
                    bool valueOk;
                    if (rule.MinValue == null && rule.MaxValue == null)
                        valueOk = true;
                    else
                    {
                        bool minOk = rule.MinValue == null || maxAbsValue >= (decimal)rule.MinValue;
                        bool maxOk = rule.MaxValue == null || maxAbsValue < (decimal)rule.MaxValue;
                        valueOk = minOk && maxOk;
                    }
                    if (!valueOk) return false;

                    // B. Code filter (priority: document header code, fallback: part code)
                    if (!string.IsNullOrEmpty(rule.Code))
                    {
                        bool codeMatch = false;

                        if (!string.IsNullOrWhiteSpace(scrapCode))
                        {
                            codeMatch = string.Equals(rule.Code, scrapCode, StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            codeMatch = parts.Any(p =>
                                string.Equals(p.Code, rule.Code, StringComparison.OrdinalIgnoreCase));
                        }

                        if (!codeMatch) return false;
                    }

                    // C. TcType filter
                    if (!string.IsNullOrEmpty(rule.TcType))
                    {
                        bool tcTypeMatch = parts.Any(p =>
                            string.Equals(p.TcType, rule.TcType, StringComparison.OrdinalIgnoreCase));
                        if (!tcTypeMatch) return false;
                    }

                    // D. Commit filter
                    if (!string.IsNullOrEmpty(rule.Cmmit) &&
                        string.Equals(rule.Cmmit, "X", StringComparison.OrdinalIgnoreCase))
                    {
                        bool commitMatch = parts.Any(p => p.Commit);
                        if (!commitMatch) return false;
                    }

                    return true;
                }).ToList();

                var rulesForResolution = activeRules
                    .OrderBy(r => r.Priority ?? int.MaxValue)
                    .ThenBy(r => r.Id)
                    .ToList();

                // SCRAP flow: step 1 selalu role_id 4 (Supervisor),
                // setelah itu lanjut mengikuti priority dari mcc_approval_rule.
                if (string.Equals(application, "SCRAP", StringComparison.OrdinalIgnoreCase))
                {
                    var supervisorRule = rulesForResolution.FirstOrDefault(r => r.RoleId == 4)
                        ?? rules
                            .Where(r => r.RoleId == 4)
                            .OrderBy(r => r.Priority ?? int.MaxValue)
                            .ThenBy(r => r.Id)
                            .FirstOrDefault();

                    if (supervisorRule != null)
                    {
                        rulesForResolution = rulesForResolution
                            .Where(r => r.RoleId != 4)
                            .ToList();

                        rulesForResolution.Insert(0, supervisorRule);
                    }
                }

                Debug.Print($"[GetApprovalChain] activeRules after filter={activeRules.Count}");

                // ── 5. Resolve KPK per rule ──────────────────────────────────────
                var approvalChain = new List<object>();

                foreach (var rule in rulesForResolution)
                {
                    // Nama role dari tabel [Scrap].[dbo].[role]
                    string roleName = allRoles.FirstOrDefault(r => r.RoleId == rule.RoleId)?.Name?.ToLower().Trim()
                                      ?? "";

                    string resolvedKpk = null;
                    string resolvedName = null;
                    string displayRole = allRoles.FirstOrDefault(r => r.RoleId == rule.RoleId)?.Name
                                          ?? $"Role {rule.RoleId}";

                    // Resolve berdasarkan nama role:
                    //   "manager"  → supervisor langsung initiator (supChain[1])
                    //   "director" → 2 level atas initiator (supChain[2])
                    //   lainnya    → cari dari tabel user by role_id + facility
                    if (roleName.Contains("manager") && !roleName.Contains("material"))
                    {
                        // Manager langsung initiator
                        var mgr = supChain.ElementAtOrDefault(1);
                        resolvedKpk = mgr?.Kpk;
                        resolvedName = mgr?.Name ?? mgr?.Kpk;
                    }
                    else if (roleName.Contains("director") && !roleName.Contains("material"))
                    {
                        // Director langsung initiator (level ke-3 di chain)
                        var dir = supChain.ElementAtOrDefault(2);
                        var mgr = supChain.ElementAtOrDefault(1);
                        if (dir != null &&
                            !string.Equals(dir.Kpk, mgr?.Kpk, StringComparison.OrdinalIgnoreCase))
                        {
                            resolvedKpk = dir.Kpk;
                            resolvedName = dir.Name ?? dir.Kpk;
                        }
                    }
                    else
                    {
                        // Material Planner, Material Manager, Material Director, PresDir, dll
                        // → cari dari tabel user by role_id + facility
                        var userByRole = dbScrap.GetUserByRoleAndFacility(rule.RoleId, facility);
                        resolvedKpk = userByRole?.Kpk;
                        resolvedName = userByRole?.Name ?? userByRole?.Kpk;

                        // Fallback khusus Material Manager:
                        // jika tidak ada di tabel user, cari supervisor dari Material Planner
                        if (resolvedKpk == null && roleName.Contains("material manager"))
                        {
                            // Cari Material Planner yang sudah diresolved sebelumnya
                            var matPlannerEntry = approvalChain
                                .Cast<dynamic>()
                                .FirstOrDefault(x => ((string)x.role).ToLower().Contains("material planner"));

                            if (matPlannerEntry != null)
                            {
                                string matPlannerKpk = (string)matPlannerEntry.kpk;
                                var matPlannerEmp = dbSSO.GetUserByKpkSSO(matPlannerKpk);
                                if (matPlannerEmp?.Supervisor != null)
                                {
                                    var matMgr = dbSSO.GetUserByKpkSSO(matPlannerEmp.Supervisor);
                                    resolvedKpk = matMgr?.Kpk;
                                    resolvedName = matMgr?.Name ?? matMgr?.Kpk;
                                }
                            }
                        }
                    }

                    // Hanya tambahkan jika KPK berhasil di-resolve
                    if (!string.IsNullOrEmpty(resolvedKpk))
                    {
                        approvalChain.Add(new
                        {
                            kpk = resolvedKpk,
                            name = resolvedName ?? resolvedKpk,
                            role = displayRole,
                            roleId = rule.RoleId,
                            priority = rule.Priority,
                            ruleId = rule.Id
                        });
                    }
                    else
                    {
                        Debug.Print($"[GetApprovalChain] WARNING: Could not resolve KPK for rule id={rule.Id}, " +
                                    $"role={displayRole}, roleId={rule.RoleId}");
                    }
                }

                // ── 6. Deduplicate by KPK (pertahankan entry pertama / priority terkecil) ──
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var result = new List<object>();
                foreach (var entry in approvalChain)
                {
                    string kpkVal = (string)((dynamic)entry).kpk;
                    if (!string.IsNullOrEmpty(kpkVal) && seen.Add(kpkVal))
                        result.Add(entry);
                }

                return Json(new
                {
                    success = true,
                    application = application,
                    scrapCode = scrapCode,
                    maxAbsValue = maxAbsValue,
                    activeRuleCount = activeRules.Count,
                    totalSteps = result.Count,
                    data = result
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Debug.Print("[GetApprovalChain] ERROR: " + ex.ToString());
                return Json(new { success = false, message = ex.Message },
                    JsonRequestBehavior.AllowGet);
            }
        }


        public class PartApprovalContext
        {
            public decimal Value { get; set; }   // totalValue part (bisa negatif)
            public string Code { get; set; }   // scrapCode / piaCode (ftypit/cmidit)
            public string TcType { get; set; }   // typeit: F/R/A/X
            public bool Commit { get; set; }   // commit flag dari part master
        }

    }
}