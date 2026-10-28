using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using JobAppHR.Repository;
using JobAppHR.Models;
using System.Linq;
using System.Data;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;

namespace JobAppHR.Controllers
{
    [Authorize(Policy = "NormalUserPolicy")]
    public class FilterProcessController : Controller
    {
        private readonly IDBOperations _DBOperations;
        private readonly IFilterProcess _FilterProcess;
        private readonly IUtilityFn _UtilityFn;
        private string loginMsg = "";
        private string eligibleRemark = "User decided as eligible";
        private string noneligibleRemark = "User decided as non-eligible";
        public FilterProcessController(IDBOperations dbOperations, IUtilityFn utilityFn, IFilterProcess filterProcess)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
            _FilterProcess = filterProcess;
            loginMsg = "Your session is expired. Please re-login.";
        }

        /*
            1) FilterSummary - FilterProcess.cs
            2) FilterByStage - FilterProcessController > return the view with filtered data
            3) FilterBy_____ - FilterProcessController > returns the list
            4) FilterBy_____ - FilterProcess.cs > actual filtering code
            5) Update____Filter - FilterProcessController
        */

        public IActionResult Index(string? selectedIntakeCode)
        {
            //List<SelectListItem> dataList;
            List<string> IntakeCodeList = new List<string>();
            List<string> TPCodeList = new List<string>();

            //dataList = _DBOperations.AnyDataList("INTAKE", "INTAKECODE", "INTAKECODE", "(FilterMode = 'AUTO') AND (FinalConfirmed IS NULL OR FinalConfirmed = 0)", "INTAKECODE DESC");
            //ViewBag.IntakeCode = new SelectList(dataList, "Value", "Text", selectedIntakeCode);

            DataTable dataTable = _DBOperations.SelectRows("INTAKE", "IntakeCode", "", "", "(FilterMode = 'AUTO') AND (FinalConfirmed IS NULL OR FinalConfirmed = 0)");
            string intakeCode = "";

            if (!string.IsNullOrEmpty(selectedIntakeCode))
            {
                ViewBag.IsSelected = "YES";
                ViewBag.SelectedIntakeCode = selectedIntakeCode;
            }

            foreach (DataRow dr in dataTable.Rows)
            {
                intakeCode = dr["IntakeCode"].ToString();

                if (intakeCode.EndsWith("/TP"))
                {
                    TPCodeList.Add(intakeCode);
                }
                else
                {
                    IntakeCodeList.Add(intakeCode);
                }
            }

            ViewBag.IntakeCode = IntakeCodeList;
            ViewBag.TPCode = TPCodeList;

            return View();
        }

        public IActionResult ViewSummary(string intakeCode)
        {
            List<FilterSummary> list = new();
            string intakeName = "";

            if (! string.IsNullOrWhiteSpace(intakeCode))
            {
                DataTable tmpTable = _FilterProcess.FilterSummary(intakeCode);
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

        public IActionResult ViewProgress(string applicationCode)
        {
            List<FilterProgress> list = _FilterProcess.ShowProgress(applicationCode);

            return View(list);
        }

        public async Task<IActionResult> FilterByStage(string intakeCode, string currentStage, string currentStatus = "PASS")
        {
            ViewBag.IntakeCode = intakeCode;

            switch (currentStage)
            {
                case "0":
                    {
                        return View("FilterByAge", FilterByAge(intakeCode));
                    }
                case "1":
                    {
                        Intake intake = GetIntakeData(intakeCode);

                        if (intake.ALRequired == 1)
                            return View("FilterByAL", await FilterByAL(intakeCode, currentStage));
                        else if (intake.OLRequired == 1)
                            return View("FilterByOL", await FilterByOL(intakeCode, currentStage));
                        else
                            return View("FilterByHEPQ", FilterByHEPQ(intakeCode, currentStage, currentStatus));
                    }
                case "2":
                    {
                        return View("FilterByOL", await FilterByOL(intakeCode, currentStage));
                    }
                case "3":
                    {
                        return View("FilterByHEPQ", FilterByHEPQ(intakeCode, currentStage, currentStatus));
                    }
                case "4":
                    {
                        if (currentStatus == "TO-CHECK")
                            return View("FilterByHEPQ", FilterByHEPQ(intakeCode, currentStage, currentStatus));
                        else
                            return View("FilterFinal", FilterFinal(intakeCode, currentStage));
                    }
                case "FINAL":
                    {
                        return View("ShowFinal", ShowFinal(intakeCode, currentStage));
                    }
                case "FAIL":
                    {
                        return View("ShowFail", ShowFail(intakeCode, currentStage));
                    }
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAgeFilter(IFormCollection formCollection)
        {
            string userId = "";

            User user = _UtilityFn.GetCurrentUser();
            userId = user.UserId;

            DataTable applicationCodesTable = new DataTable();

            string remarks = "", message = "";
            string currentStage = "1";
            string intakeCode = formCollection["intakeCode"];
            ViewBag.IntakeCode = intakeCode;

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ApplicationCode");
            dataTable.Columns.Add("IntakeCode");
            dataTable.Columns.Add("CurrentStage");
            dataTable.Columns.Add("CurrentStatus");
            dataTable.Columns.Add("Stage1Status"); 
            dataTable.Columns.Add("Stage1Remarks");
            dataTable.Columns.Add("Stage1UpdatedBy");
            dataTable.Columns.Add("Stage1UpdatedOn");

            // Get form data arrays - fix the key names to match the actual form field names
            string[] applicationCodeArray = formCollection.ContainsKey("ApplicationCode") ? 
                formCollection["ApplicationCode"].ToArray() : Array.Empty<string>();
            
            string[] currentStatusArray = formCollection.ContainsKey("CurrentStatus") ? 
                formCollection["CurrentStatus"].ToArray() : Array.Empty<string>();
            
            string[] remarksArray = formCollection.ContainsKey("Remarks") ? 
                formCollection["Remarks"].ToArray() : Array.Empty<string>();

            if (applicationCodeArray.Length > 0)
            {
                for (int i = 0; i < applicationCodeArray.Length; i++)
                {
                    DataRow dr = dataTable.NewRow();
                    remarks = "";

                    dr["ApplicationCode"] = applicationCodeArray[i];
                    dr["IntakeCode"] = intakeCode;
                    dr["CurrentStage"] = currentStage;
                    dr["CurrentStatus"] = i < currentStatusArray.Length ? currentStatusArray[i] : "PASS";
                    dr["Stage1Status"] = i < currentStatusArray.Length ? currentStatusArray[i] : "PASS";

                    if ((i < currentStatusArray.Length ? currentStatusArray[i] : "PASS") == "FAIL")
                        remarks = noneligibleRemark + ",";

                    dr["Stage1Remarks"] = remarks + (i < remarksArray.Length ? remarksArray[i] : "");
                    dr["Stage1UpdatedBy"] = userId;
                    dr["Stage1UpdatedOn"] = DateTime.Now;

                    dataTable.Rows.Add(dr);
                }

                //update eligible list
                message = _DBOperations.InsertRecords("FilteredData", dataTable, false);
            }

            //update overage list
            if (message == "SUCCESS")
            {
                //copy only the application code and intake code to a new table
                applicationCodesTable = dataTable.DefaultView.ToTable("", false, "ApplicationCode");
                dataTable.Rows.Clear();

                string sql = "SELECT ApplicationCode, IntakeCode FROM Application WHERE (IntakeCode = '" + intakeCode + "') AND (Processed IS NULL OR Processed = '') AND (Overage <> '') AND (SaveStatus = 'OK')";

                DataTable dataTable2 = _DBOperations.SelectRows(sql);

                dataTable.Merge(dataTable2);

                foreach(DataRow dr in dataTable.Rows)
                {
                    dr["CurrentStage"] = currentStage;
                    dr["CurrentStatus"] = "FAIL";
                    dr["Stage1Status"] = "FAIL";
                    dr["Stage1Remarks"] = "Overage";
                    dr["Stage1UpdatedBy"] = HttpContext.Session.GetString("UserId");
                    dr["Stage1UpdatedOn"] = DateTime.Now;
                }

                dataTable.AcceptChanges();

                message = _DBOperations.InsertRecords("FilteredData", dataTable, false);
            }

            //update processed field in application table
            //1. merge the applicationcodes set with overage appl.codes
            //2. remove other columns retaining only the appl.code field
            //3. add new col as processed
            //4. update the application db tbl and make processed column as YES

            applicationCodesTable.Merge(dataTable);

            for(int i = 1; i < dataTable.Columns.Count; i++)
            {
                applicationCodesTable.Columns.Remove(dataTable.Columns[i].ColumnName);
            }

            applicationCodesTable.Columns.Add("Processed");

            foreach(DataRow dr in applicationCodesTable.Rows)
            {
                dr["Processed"] = "YES";
            }

            applicationCodesTable.AcceptChanges();

            //update the application table processed field
            _DBOperations.UpdateRecords("Application", applicationCodesTable, "ApplicationCode");

            // Redirect to next stage with intake code preserved in URL
            return RedirectToAction("FilterByStage", new { intakeCode = intakeCode, currentStage = "1", currentStatus = "PASS" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateALFilter(IFormCollection formCollection)
        {
            string userId = "";

            User user = _UtilityFn.GetCurrentUser();
            userId = user.UserId;

            DataTable applicationCodesTable = new();

            string remarks = "", message = "";
            string sql = "";
            string currentStage = "2";
            string intakeCode = formCollection["intakeCode"];
            ViewBag.IntakeCode = intakeCode;

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ApplicationCode");
            dataTable.Columns.Add("CurrentStage");
            dataTable.Columns.Add("CurrentStatus");
            dataTable.Columns.Add("Stage2Status");
            dataTable.Columns.Add("Stage2Remarks");
            dataTable.Columns.Add("Stage2UpdatedBy");
            dataTable.Columns.Add("Stage2UpdatedOn");

            // Get form data arrays - fix the key names to match the actual form field names
            string[] applicationCodeArray = formCollection.ContainsKey("ApplicationCode") ? 
                formCollection["ApplicationCode"].ToArray() : Array.Empty<string>();
            
            string[] currentStatusArray = formCollection.ContainsKey("CurrentStatus") ? 
                formCollection["CurrentStatus"].ToArray() : Array.Empty<string>();
            
            string[] remarksArray = formCollection.ContainsKey("Remarks") ? 
                formCollection["Remarks"].ToArray() : Array.Empty<string>();

            if (applicationCodeArray.Length > 0)
            {
                for (int i = 0; i < applicationCodeArray.Length; i++)
                {
                    DataRow dr = dataTable.NewRow();
                    remarks = "";

                    dr["ApplicationCode"] = applicationCodeArray[i];
                    dr["CurrentStage"] = currentStage;
                    dr["CurrentStatus"] = i < currentStatusArray.Length ? currentStatusArray[i] : "PASS";
                    dr["Stage2Status"] = i < currentStatusArray.Length ? currentStatusArray[i] : "PASS";

                    if ((i < currentStatusArray.Length ? currentStatusArray[i] : "PASS") == "FAIL")
                        remarks = noneligibleRemark + ",";

                    dr["Stage2Remarks"] = remarks + (i < remarksArray.Length ? remarksArray[i] : "");
                    dr["Stage2UpdatedBy"] = userId;
                    dr["Stage2UpdatedOn"] = DateTime.Now;

                    dataTable.Rows.Add(dr);
                }

                //update eligible list - use UpdateRecords to update existing FilteredData records
                message = _DBOperations.UpdateRecords("FilteredData", dataTable, "ApplicationCode");
            }

            //update rest of the rows in FilteredData as stage 2 status FAIL
            dataTable.Rows.Clear();
            sql = "SELECT ApplicationCode FROM FilteredData WHERE IntakeCode = '" + intakeCode + "' AND Stage1Status = 'PASS' AND Stage2Status IS NULL";
            DataTable dataTable2 = _DBOperations.SelectRows(sql);

            dataTable.Merge(dataTable2);

            foreach (DataRow dr in dataTable.Rows)
            {
                dr["CurrentStage"] = currentStage;
                dr["CurrentStatus"] = "FAIL";
                dr["Stage2Status"] = "FAIL";
                dr["Stage2Remarks"] = "A/L results not sufficient";
                dr["Stage2UpdatedBy"] = userId;
                dr["Stage2UpdatedOn"] = DateTime.Now;
            }

            dataTable.AcceptChanges();
            message = _DBOperations.UpdateRecords("FilteredData", dataTable, "ApplicationCode");

            // Redirect to next stage with intake code preserved in URL
            return RedirectToAction("FilterByStage", new { intakeCode = intakeCode, currentStage = "2", currentStatus = "PASS" });
        }

        [HttpPost]
        public IActionResult UpdateOLFilter(IFormCollection formCollection)
        {
            string userId = "";

            User user = _UtilityFn.GetCurrentUser();
            userId = user.UserId;

            string remarks = "";
            string message = "";
            string sql = "";
            string currentStage = "3";
            string intakeCode = formCollection["intakeCode"];
            ViewBag.IntakeCode = intakeCode;

            DataTable dataTable = new();
            dataTable.Columns.Add("ApplicationCode");
            dataTable.Columns.Add("CurrentStage");
            dataTable.Columns.Add("CurrentStatus");
            dataTable.Columns.Add("Stage3Status");
            dataTable.Columns.Add("Stage3Remarks");
            dataTable.Columns.Add("Stage3UpdatedBy");
            dataTable.Columns.Add("Stage3UpdatedOn");

            // Get form data arrays - fix the key names to match the actual form field names
            string[] applicationCodeArray = formCollection.ContainsKey("ApplicationCode") ? 
                formCollection["ApplicationCode"].ToArray() : Array.Empty<string>();
            
            string[] currentStatusArray = formCollection.ContainsKey("CurrentStatus") ? 
                formCollection["CurrentStatus"].ToArray() : Array.Empty<string>();
            
            string[] remarksArray = formCollection.ContainsKey("Remarks") ? 
                formCollection["Remarks"].ToArray() : Array.Empty<string>();

            if (applicationCodeArray.Length > 0)
            {
                for (int i = 0; i < applicationCodeArray.Length; i++)
                {
                    DataRow dr = dataTable.NewRow();
                    remarks = "";

                    dr["ApplicationCode"] = applicationCodeArray[i];
                    dr["CurrentStage"] = currentStage;
                    dr["CurrentStatus"] = i < currentStatusArray.Length ? currentStatusArray[i] : "PASS";
                    dr["Stage3Status"] = i < currentStatusArray.Length ? currentStatusArray[i] : "PASS";

                    if ((i < currentStatusArray.Length ? currentStatusArray[i] : "PASS") == "FAIL")
                        remarks = noneligibleRemark + ",";

                    dr["Stage3Remarks"] = remarks + (i < remarksArray.Length ? remarksArray[i] : "");
                    dr["Stage3UpdatedBy"] = userId;
                    dr["Stage3UpdatedOn"] = DateTime.Now;

                    dataTable.Rows.Add(dr);
                }

                //update eligible list
                message = _DBOperations.UpdateRecords("FilteredData", dataTable, "ApplicationCode");
            }

            //update rest of the rows in FilteredData as stage 3 status FAIL
            dataTable.Rows.Clear();
            
            Intake intake = GetIntakeData(intakeCode);
            if (intake.ALRequired == 1)
                sql = "SELECT ApplicationCode FROM FilteredData WHERE IntakeCode = '" + intakeCode + "' AND Stage2Status = 'PASS' AND Stage3Status IS NULL";
            else //when A/L not required
                sql = "SELECT ApplicationCode FROM FilteredData WHERE IntakeCode = '" + intakeCode + "' AND Stage1Status = 'PASS' AND Stage3Status IS NULL";
            
            DataTable dataTable2 = _DBOperations.SelectRows(sql);

            dataTable.Merge(dataTable2);

            foreach (DataRow dr in dataTable.Rows)
            {
                dr["CurrentStage"] = currentStage;
                dr["CurrentStatus"] = "FAIL";
                dr["Stage3Status"] = "FAIL";
                dr["Stage3Remarks"] = "O/L results not sufficient";
                dr["Stage3UpdatedBy"] = userId;
                dr["Stage3UpdatedOn"] = DateTime.Now;
            }

            dataTable.AcceptChanges();
            message = _DBOperations.UpdateRecords("FilteredData", dataTable, "ApplicationCode");

            // Redirect to next stage with intake code preserved in URL
            return RedirectToAction("FilterByStage", new { intakeCode = intakeCode, currentStage = "3", currentStatus = "PASS" });
        }

        [HttpPost]
        public IActionResult UpdateHEPQFilter(IFormCollection formCollection)
        {
            string userId = "";

            User user = _UtilityFn.GetCurrentUser();
            userId = user.UserId;

            string remarks = "";
            string sql = "";
            string currentStage = "4";
            string intakeCode = formCollection["intakeCode"];
            ViewBag.IntakeCode = intakeCode;

            DataTable dataTable = new();
            dataTable.Columns.Add("ApplicationCode");
            dataTable.Columns.Add("CurrentStage");
            dataTable.Columns.Add("CurrentStatus");
            dataTable.Columns.Add("Stage4Status");
            dataTable.Columns.Add("Stage4Remarks");
            dataTable.Columns.Add("Stage4UpdatedBy");
            dataTable.Columns.Add("Stage4UpdatedOn");

            string[] applicationCodeArray = formCollection.ContainsKey("ApplicationCode") ? 
                formCollection["ApplicationCode"].ToArray() : Array.Empty<string>();
            
            string[] currentStatusArray = formCollection.ContainsKey("CurrentStatus") ? 
                formCollection["CurrentStatus"].ToArray() : Array.Empty<string>();
            
            string[] remarksArray = formCollection.ContainsKey("Remarks") ? 
                formCollection["Remarks"].ToArray() : Array.Empty<string>();

            for (int i = 0; i < applicationCodeArray.Length; i++)
            {
                DataRow dr = dataTable.NewRow();
                remarks = "";

                if ((i < currentStatusArray.Length ? currentStatusArray[i] : "") == "FAIL" || (i < currentStatusArray.Length ? currentStatusArray[i] : "") == "")
                    remarks = "Higher/Professional Qualifications not sufficient,";

                dr["ApplicationCode"] = applicationCodeArray[i];
                dr["CurrentStage"] = currentStage;
                dr["CurrentStatus"] = i < currentStatusArray.Length ? currentStatusArray[i] : "";
                dr["Stage4Status"] = i < currentStatusArray.Length ? currentStatusArray[i] : "";
                dr["Stage4Remarks"] = remarks + (i < remarksArray.Length ? remarksArray[i] : "");
                dr["Stage4UpdatedBy"] = userId;
                dr["Stage4UpdatedOn"] = DateTime.Now;

                dataTable.Rows.Add(dr);
            }

            //update eligible list
            var message = _DBOperations.UpdateRecords("FilteredData", dataTable, "ApplicationCode");

            // Redirect to next stage with intake code preserved in URL
            return RedirectToAction("FilterByStage", new { intakeCode = intakeCode, currentStage = "4", currentStatus = "PASS" });
        }

        [HttpPost]
        public IActionResult UpdateFinal(IFormCollection formCollection)
        {
            string userId = "";

            User user = _UtilityFn.GetCurrentUser();
            userId = user.UserId;

            string remarks = "";
            string sql = "";
            string currentStage = "FINAL";
            string intakeCode = formCollection["intakeCode"];
            ViewBag.IntakeCode = intakeCode;

            DataTable dataTable = new();
            dataTable.Columns.Add("ApplicationCode");
            dataTable.Columns.Add("CurrentStage");
            dataTable.Columns.Add("CurrentStatus");
            dataTable.Columns.Add("FinalStatus");
            dataTable.Columns.Add("FinalRemarks");
            dataTable.Columns.Add("FinalUpdatedBy");
            dataTable.Columns.Add("FinalUpdatedOn");

            string[] applicationCodeArray = formCollection.ContainsKey("ApplicationCode") ? 
                formCollection["ApplicationCode"].ToArray() : Array.Empty<string>();
            
            string[] currentStatusArray = formCollection.ContainsKey("CurrentStatus") ? 
                formCollection["CurrentStatus"].ToArray() : Array.Empty<string>();
            
            string[] remarksArray = formCollection.ContainsKey("Remarks") ? 
                formCollection["Remarks"].ToArray() : Array.Empty<string>();

            if (applicationCodeArray.Length == 0)
            {
                // No data to process, redirect back
                return RedirectToAction("Index", new { selectedIntakeCode = intakeCode });
            }

            for (int i = 0; i < applicationCodeArray.Length; i++)
            {
                DataRow dr = dataTable.NewRow();
                remarks = "";

                if ((i < currentStatusArray.Length ? currentStatusArray[i] : "PASS") == "FAIL")
                    remarks = noneligibleRemark + "-" + userId + ",";

                if (i < remarksArray.Length && remarksArray[i].Trim() != "")
                    remarks = remarks + remarksArray[i].Trim() + "-" + userId;

                dr["ApplicationCode"] = applicationCodeArray[i];
                dr["CurrentStage"] = currentStage;
                dr["CurrentStatus"] = i < currentStatusArray.Length ? currentStatusArray[i] : "PASS";
                dr["FinalStatus"] = i < currentStatusArray.Length ? currentStatusArray[i] : "PASS";
                dr["FinalRemarks"] = remarks;
                dr["FinalUpdatedBy"] = userId;
                dr["FinalUpdatedOn"] = DateTime.Now;

                dataTable.Rows.Add(dr);
            }

            //update eligible list
            var message = _DBOperations.UpdateRecords("FilteredData", dataTable, "ApplicationCode");

            return RedirectToAction("Index", new { selectedIntakeCode = intakeCode });
        }

        //AdjustFinal is not same as UpdateFinal
        //UpdateFinal takes records which are previously not updated for final stage
        //but AdjustFinal takes records which are previously updated as PASS for final stage
        //so in AdjustFinal when and only if status is set to FAIL we need to update them again
        [HttpPost]
        public IActionResult AdjustFinal(IFormCollection formCollection)
        {
            string userId = "";

            User user = _UtilityFn.GetCurrentUser();
            userId = user.UserId;

            string remarks = "";
            string sql = "";
            string currentStage = "FINAL";
            string intakeCode = formCollection["intakeCode"];
            ViewBag.IntakeCode = intakeCode;

            DataTable dataTable = new();
            dataTable.Columns.Add("ApplicationCode");
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
            string[] remarksArray = (string[])formCollection[key.First()];

            for (int i = 0; i < applicationCodeArray.Length; i++)
            {
                if (currentStatusArray[i] == "FAIL")
                {
                    DataRow dr = dataTable.NewRow();

                    remarks = noneligibleRemark + "-" + userId + ",";

                    if (remarksArray[i].Trim() != "")
                        remarks = remarks + remarksArray[i].Trim() + "-" + userId; ;

                    dr["ApplicationCode"] = applicationCodeArray[i];
                    dr["CurrentStage"] = currentStage;
                    dr["CurrentStatus"] = currentStatusArray[i];
                    dr["FinalStatus"] = currentStatusArray[i];
                    dr["FinalRemarks"] = remarks;
                    dr["FinalUpdatedBy"] = userId;
                    dr["FinalUpdatedOn"] = DateTime.Now;

                    dataTable.Rows.Add(dr);
                }
            }

            //update eligible list
            var message = _DBOperations.UpdateRecords("FilteredData", dataTable, "ApplicationCode");

            // Redirect to show final results with intake code preserved in URL
            return RedirectToAction("FilterByStage", new { intakeCode = intakeCode, currentStage = "FINAL", currentStatus = "PASS" });
        }


        //UpdateFail takes records which are previously updated as FAIL for final stage
        //so in UpdateFail when and only if status is set to PASS we need to update them again
        //but later they wanted to update just the remarks saying something like "need to check further"
        [HttpPost]
        public IActionResult UpdateFail(IFormCollection formCollection)
        {
            string userId = "";

            User user = _UtilityFn.GetCurrentUser();
            userId = user.UserId;

            string remarks = "";
            string sql = "";
            string currentStage = "FINAL";
            string intakeCode = formCollection["intakeCode"];
            ViewBag.IntakeCode = intakeCode;

            DataTable dataTable = new();
            dataTable.Columns.Add("ApplicationCode");
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
            string[] remarksArray = (string[])formCollection[key.First()];

            key = formCollection.Keys.Where(k => k.StartsWith("item.FailedRemarks"));
            string[] failedRemarksArray = (string[])formCollection[key.First()];

            for (int i = 0; i < applicationCodeArray.Length; i++)
            {
                remarks = "";

                if (currentStatusArray[i] == "PASS")
                {
                    DataRow dr = dataTable.NewRow();

                    remarks = eligibleRemark + "-" + userId + ",";

                    if (remarksArray[i].Trim() != "")
                        remarks = remarks + remarksArray[i].Trim() + "-" + userId; ;

                    dr["ApplicationCode"] = applicationCodeArray[i];
                    dr["CurrentStage"] = currentStage;
                    dr["CurrentStatus"] = currentStatusArray[i];
                    dr["FinalStatus"] = currentStatusArray[i];
                    dr["FinalRemarks"] = failedRemarksArray[i] + "," + remarks;
                    dr["FinalUpdatedBy"] = userId;
                    dr["FinalUpdatedOn"] = DateTime.Now;

                    dataTable.Rows.Add(dr);
                }
                else if (currentStatusArray[i] == "FAIL")
                {
                    DataRow dr = dataTable.NewRow();

                    //remarks = eligibleRemark + "-" + userId + ",";

                    if (remarksArray[i].Trim() != "")
                        remarks = remarks + remarksArray[i].Trim() + "-" + userId; ;

                    dr["ApplicationCode"] = applicationCodeArray[i];
                    dr["CurrentStage"] = currentStage;
                    dr["CurrentStatus"] = currentStatusArray[i];
                    dr["FinalStatus"] = currentStatusArray[i];
                    dr["FinalRemarks"] = failedRemarksArray[i] + "," + remarks;
                    dr["FinalUpdatedBy"] = userId;
                    dr["FinalUpdatedOn"] = DateTime.Now;

                    dataTable.Rows.Add(dr);

                }
            }

            //update eligible list
            var message = _DBOperations.UpdateRecords("FilteredData", dataTable, "ApplicationCode");

            return RedirectToAction("Index", new { selectedIntakeCode = intakeCode });
        }

        public ActionResult ConfirmFinal(string intakeCode) 
        {
            ApiResponse response = new();

            if (intakeCode.Contains("/TP"))
                response = _FilterProcess.ConfirmFinal(intakeCode, "TP");
            else
                response = _FilterProcess.ConfirmFinal(intakeCode, "");

            if (response.isSuccess)
            {
                ViewBag.Message = "SUCCESS";
                ViewBag.IntakeCode = intakeCode;
            }
            else
            {
                ViewBag.Message = "";
            }

            return View("ProcessComplete");
        }

        //-----------Private Methods--------------
        //-----------Age Filter-------------------
        private List<AgeFilter> FilterByAge(string intakeCode)
        {
            List<AgeFilter> list = new();
            if (intakeCode != null)
            {
                list = _FilterProcess.FilterByAge(intakeCode);
            }

            return list;
        }

        //-----------A/L Filter-------------------
        private async Task<List<ALFilter>> FilterByAL(string intakeCode, string currentStage)
        {
            List<ALFilter> list = new();
            if (intakeCode != null)
            {
                list = await _FilterProcess.FilterByAL(intakeCode, currentStage);
            }

            return list;
        }

        //-----------O/L Filter-------------------
        private async Task<List<OLFilter>> FilterByOL(string intakeCode, string currentStage)
        {
            List<OLFilter> list = new();
            if (intakeCode != null)
            {
                list = await _FilterProcess.FilterByOL(intakeCode, currentStage);
            }

            return list;
        }

        //-----------HE and PQ Filter-------------------
        private List<HEPQFilter> FilterByHEPQ(string intakeCode, string currentStage, string currentStatus = "PASS")
        {
            List<HEPQFilter> list = new();
            if (intakeCode != null)
            {
                list = _FilterProcess.FilterByHEPQ(intakeCode, currentStage, currentStatus);
            }

            return list;
        }

        //-----------Final Filter-------------------
        private List<FinalFilter> FilterFinal(string intakeCode, string currentStage)
        {
            List<FinalFilter> list = new();
            if (intakeCode != null)
            {
                list = _FilterProcess.FilterFinal(intakeCode, currentStage);
            }

            return list;
        }

        //-----------Show Final List-------------------
        private List<FinalFilter> ShowFinal(string intakeCode, string currentStage)
        {
            List<FinalFilter> list = new();
            if (intakeCode != null)
            {
                list = _FilterProcess.ShowFinal(intakeCode, currentStage);
            }

            return list;
        }

        //-----------Show Fail List-------------------
        private List<FailedApplicants> ShowFail(string intakeCode, string currentStage)
        {
            List<FailedApplicants> list = new();
            if (intakeCode != null)
            {
                list = _FilterProcess.ShowFail(intakeCode, currentStage);
            }

            return list;
        }

        private Intake GetIntakeData(string intakeCode)
        {
            DataTable dataTable = _DBOperations.SelectRows("INTAKE", "IntakeCode, ALRequired, OLRequired","IntakeCode",intakeCode,"");
            List<Intake> list = _UtilityFn.ConvertToList<Intake>(dataTable);
            return list.First();
        }

        [HttpGet]
        public JsonResult GetJobPositionName(string intakeCode)
        {
            var jobPositionName = _DBOperations.GetJobPositionName(intakeCode);
            return Json(new { jobPositionName });
        }
    }
}
