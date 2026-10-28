using DocumentFormat.OpenXml.Spreadsheet;
using JobAppHR.Models;
using JobAppHR.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace JobAppHR.Controllers
{
    [Authorize(Policy = "NormalUserPolicy")]
    public class ManualProcessController : Controller
    {
        private readonly IDBOperations _DBOperations;
        private readonly IManualProcess _ManualProcess;
        private readonly IFilterProcess _FilterProcess;
        private readonly IUtilityFn _UtilityFn;
        private string loginMsg = "";
        private string eligibleRemark = "User decided as eligible";
        private string noneligibleRemark = "User decided as non-eligible";

        public ManualProcessController(IDBOperations dbOperations, IUtilityFn utilityFn, IManualProcess manualProcess, IFilterProcess filterProcess)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
            _ManualProcess = manualProcess;
            _FilterProcess = filterProcess;
            loginMsg = "Your session is expired. Please re-login.";
        }
        public IActionResult Index(string? selectedIntakeCode)
        {
            List<SelectListItem> dataList;

            //selectedIntakeCode = "ENG/2024/07";

            dataList = _DBOperations.AnyDataList("INTAKE", "INTAKECODE", "INTAKECODE", "(FilterMode = 'MANUAL') AND (FinalConfirmed IS NULL OR FinalConfirmed = 0)", "INTAKECODE DESC");
            ViewBag.IntakeCode = new SelectList(dataList, "Value", "Text", selectedIntakeCode);

            if (! string.IsNullOrEmpty(selectedIntakeCode))
                ViewBag.IsSelected = "YES";

            return View();
        }

        public IActionResult ViewSummary(string intakeCode)
        {
            List<FilterSummary> list = new();
            string intakeName = "";

            if (!String.IsNullOrWhiteSpace(intakeCode))
            {
                DataTable tmpTable = _ManualProcess.FilterSummary(intakeCode);
                list = _UtilityFn.ConvertToList<FilterSummary>(tmpTable);
                
                // Get intake name
                string sql = "SELECT jp.JobPositionName FROM INTAKE i INNER JOIN JobPosition jp ON i.JobPositionID = jp.JobPositionID WHERE i.IntakeCode = '" + intakeCode + "'";
                DataTable intakeTable = _DBOperations.SelectRows(sql);
                if (intakeTable.Rows.Count > 0)
                {
                    intakeName = intakeTable.Rows[0]["JobPositionName"].ToString();
                }
            }

            ViewBag.IntakeName = intakeName;
            return PartialView("_FilterSummary", list);
        }

        public IActionResult FilterByStage(string intakeCode, string currentStage, string? currentStatus)
        {
            ViewBag.IntakeCode = intakeCode;
            List<ManualFilter> list = new();

            DataTable dataTable = _DBOperations.SelectRows("INTAKE", "OLRequired, ALRequired, HERequired", "IntakeCode", intakeCode, "");
            if (dataTable.Rows.Count > 0)
            {
                ViewBag.OLRequired = dataTable.Rows[0]["OLRequired"].ToString();
                ViewBag.ALRequired = dataTable.Rows[0]["ALRequired"].ToString();
                ViewBag.HERequired = dataTable.Rows[0]["HERequired"].ToString();
            }
            else
            {
                ViewBag.OLRequired = "1";
                ViewBag.ALRequired = "1";
                ViewBag.HERequired = "1";
            }

            switch (currentStage)
            {
                case "0":
                    {
                        list = _ManualProcess.FilterByAll(intakeCode, currentStage, "");
                        ViewBag.CurrentStage = currentStage;
                        ViewBag.CurrentStageName = "Original applicant list";
                        return View("FilterFinal", list);
                    }
                case "FINAL":
                    {
                        list = _ManualProcess.FilterByAll(intakeCode, currentStage, currentStatus);
                        ViewBag.CurrentStage = currentStage;
                        ViewBag.CurrentStatus = currentStatus;
                        if (currentStatus == "TO-CHECK")
                            ViewBag.CurrentStageName = "To-Check list";
                        else
                            ViewBag.CurrentStageName = "Final list";

                        return View("FilterFinal", list);
                    }
                case "FAIL":
                    {
                        list = _ManualProcess.FilterByAll(intakeCode, currentStage, "FAIL");
                        ViewBag.CurrentStage = currentStage;
                        ViewBag.CurrentStatus = "FAIL";
                        ViewBag.CurrentStageName = "Fail list";
                        return View("FilterFinal", list);
                    }
            }

            return View();

        }

        [HttpPost]
        public IActionResult UpdateFinal(IFormCollection formCollection)
        {

            DataTable applicationCodesTable = new();

            string remarks = "";
            string newRemarks = "";
            string oldStatus = "";
            string currentStatus = "";
            string message = "";
            string sql = "";
            string prevStage = formCollection["currentStage"];
            string currentStage = "FINAL";
            string intakeCode = formCollection["intakeCode"];
            string userId = "";
            ViewBag.IntakeCode = intakeCode;

            User user = _UtilityFn.GetCurrentUser();
            userId = user.UserId;

            DataTable dataTable = new();
            dataTable.Columns.Add("ApplicationCode");
            dataTable.Columns.Add("IntakeCode");
            dataTable.Columns.Add("CurrentStage");
            dataTable.Columns.Add("CurrentStatus");
            dataTable.Columns.Add("FinalStatus");
            dataTable.Columns.Add("FinalRemarks");
            dataTable.Columns.Add("FinalUpdatedBy");
            dataTable.Columns.Add("FinalUpdatedOn");

            var key = formCollection.Keys.Where(k => k.StartsWith("item.ApplicationCode"));
            string[] applicationCodeArray = (string[])formCollection[key.First()];

            key = formCollection.Keys.Where(k => k.StartsWith("item.CurrentStatus"));
            string[] currentStatusArray = (string[])formCollection[key.First()];

            key = formCollection.Keys.Where(k => k.StartsWith("item.Remarks"));
            string[] oldRemarksArray = (string[])formCollection[key.First()];

            //key = formCollection.Keys.Where(k => k.StartsWith("newremarks"));
            //string[] newRemarksArray = (string[])formCollection[key.First()];

            for (int i = 0; i < applicationCodeArray.Length; i++)
            {
                DataRow dr = dataTable.NewRow();
                remarks = "";
                newRemarks = "";
                newRemarks = formCollection["newremarks_" + i.ToString()];
                oldStatus = formCollection["oldstatus_" + i.ToString()];
                currentStatus = currentStatusArray[i];

                if (! string.IsNullOrWhiteSpace(newRemarks))
                    newRemarks = newRemarks + "-" + userId + ",";

                remarks = oldRemarksArray[i].Trim() + newRemarks;

                if (currentStatus == "PASS")
                {
                    if (string.IsNullOrEmpty(oldStatus) || oldStatus == "FAIL" || oldStatus == "TO-CHECK")
                        remarks = remarks + eligibleRemark + "-" + userId + ",";
                }
                else if (currentStatus == "FAIL")
                {
                    if (string.IsNullOrEmpty(oldStatus) || oldStatus == "PASS" || oldStatus == "TO-CHECK")
                        remarks = remarks + noneligibleRemark + "-" + userId + ",";
                }
                else //to-check
                {
                    if (string.IsNullOrEmpty(oldStatus) || oldStatus != "TO-CHECK")
                        remarks = remarks + "need to check-" + userId + ",";
                }

                dr["ApplicationCode"] = applicationCodeArray[i];
                dr["IntakeCode"] = intakeCode;
                dr["CurrentStage"] = currentStage;
                dr["CurrentStatus"] = currentStatus;
                dr["FinalStatus"] = currentStatus;
                dr["FinalRemarks"] = remarks.Substring(0, remarks.Length > 500 ? 500 : remarks.Length);
                dr["FinalUpdatedBy"] = userId;
                dr["FinalUpdatedOn"] = DateTime.Now;

                dataTable.Rows.Add(dr);
            }

            if (prevStage == "0") //first time filtering
            {
                //insert into FilteredData
                message = _DBOperations.InsertRecords("FilteredData", dataTable, false);

                //update the application table processed field
                applicationCodesTable.Merge(dataTable);

                for (int i = 1; i < dataTable.Columns.Count; i++)
                {
                    applicationCodesTable.Columns.Remove(dataTable.Columns[i].ColumnName);
                }

                applicationCodesTable.Columns.Add("Processed");

                foreach (DataRow dr in applicationCodesTable.Rows)
                {
                    dr["Processed"] = "YES";
                }

                _DBOperations.UpdateRecords("Application", applicationCodesTable, "ApplicationCode");
            }
            else
            {
                //update eligible list
                message = _DBOperations.UpdateRecords("FilteredData", dataTable, "ApplicationCode");
            }

            //return View("ShowFinal", ShowFinal(intakeCode, currentStage));
            return RedirectToAction("Index", new {selectedIntakeCode = intakeCode});
        }

        /// <summary>
        /// Per-row AJAX save for real-time collaborative screening.
        /// Saves a single applicant's status/remarks change immediately.
        /// </summary>
        [HttpPost]
        public IActionResult UpdateSingleApplicant([FromBody] SingleApplicantUpdate model)
        {
            try
            {
                User user = _UtilityFn.GetCurrentUser();
                string userId = user.UserId;
                string userName = user.UserName;

                string remarks = "";
                string currentStage = "FINAL";

                // Build remarks based on status change
                if (!string.IsNullOrWhiteSpace(model.NewRemarks))
                    remarks = model.NewRemarks + "-" + userId + ",";

                if (model.NewStatus == "PASS")
                {
                    if (string.IsNullOrEmpty(model.OldStatus) || model.OldStatus == "FAIL" || model.OldStatus == "TO-CHECK")
                        remarks = remarks + eligibleRemark + "-" + userId + ",";
                }
                else if (model.NewStatus == "FAIL")
                {
                    if (string.IsNullOrEmpty(model.OldStatus) || model.OldStatus == "PASS" || model.OldStatus == "TO-CHECK")
                        remarks = remarks + noneligibleRemark + "-" + userId + ",";
                }
                else // TO-CHECK
                {
                    if (string.IsNullOrEmpty(model.OldStatus) || model.OldStatus != "TO-CHECK")
                        remarks = remarks + "need to check-" + userId + ",";
                }

                // Truncate remarks to 500 chars
                if (remarks.Length > 500)
                    remarks = remarks.Substring(0, 500);

                DataTable dataTable = new();
                dataTable.Columns.Add("ApplicationCode");
                dataTable.Columns.Add("IntakeCode");
                dataTable.Columns.Add("CurrentStage");
                dataTable.Columns.Add("CurrentStatus");
                dataTable.Columns.Add("FinalStatus");
                dataTable.Columns.Add("FinalRemarks");
                dataTable.Columns.Add("FinalUpdatedBy");
                dataTable.Columns.Add("FinalUpdatedOn");

                DataRow dr = dataTable.NewRow();
                dr["ApplicationCode"] = model.ApplicationCode;
                dr["IntakeCode"] = model.IntakeCode;
                dr["CurrentStage"] = currentStage;
                dr["CurrentStatus"] = model.NewStatus;
                dr["FinalStatus"] = model.NewStatus;
                dr["FinalRemarks"] = remarks;
                dr["FinalUpdatedBy"] = userId;
                dr["FinalUpdatedOn"] = DateTime.Now;
                dataTable.Rows.Add(dr);

                string message;

                if (model.PreviousStage == "0") // First-time processing — insert
                {
                    message = _DBOperations.InsertRecords("FilteredData", dataTable, false);

                    // Also update Application.Processed = YES
                    DataTable appTable = new();
                    appTable.Columns.Add("ApplicationCode");
                    appTable.Columns.Add("Processed");
                    DataRow appRow = appTable.NewRow();
                    appRow["ApplicationCode"] = model.ApplicationCode;
                    appRow["Processed"] = "YES";
                    appTable.Rows.Add(appRow);
                    _DBOperations.UpdateRecords("Application", appTable, "ApplicationCode");
                }
                else // Subsequent edit — update
                {
                    message = _DBOperations.UpdateRecords("FilteredData", dataTable, "ApplicationCode");
                }

                return Json(new
                {
                    success = message == "SUCCESS",
                    updatedBy = userId,
                    updatedByName = userName,
                    updatedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    remarks = remarks
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public IActionResult ConfirmFinal(string intakeCode)
        {
            //for both intake types final freezed list is saved calling the same method in Repository/FilterProcess.cs
            ApiResponse response = new();

            if (intakeCode.Contains("/TP"))
                response = _FilterProcess.ConfirmFinal(intakeCode, "TP");
            else
                response = _FilterProcess.ConfirmFinal(intakeCode, "");

            if (response.isSuccess)
            {
                ViewBag.Message = "SUCCESS";
                ViewBag.IntakeCode = intakeCode;
                ViewBag.FreezeNo = response.result.ToString();
            }
            else
            {
                ViewBag.Message = "";
            }

            return View("ProcessComplete");
        }

        [HttpGet]
        public JsonResult GetJobPositionName(string intakeCode)
        {
            var jobPositionName = _DBOperations.GetJobPositionName(intakeCode);
            return Json(new { jobPositionName });
        }
    }
}
