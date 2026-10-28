using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Fingers10.ExcelExport.ActionResults;
using JobAppHR.Models;
using JobAppHR.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace JobAppHR.Controllers
{
    [Authorize(Policy = "NormalUserPolicy")]
    public class EligibleApplicantController : Controller
    {
        private readonly IDBOperations _DBOperations;
        private readonly IUtilityFn _UtilityFn;
        private readonly IFilterProcess _FilterProcess;
        private string loginMsg = "";

        public EligibleApplicantController(IDBOperations dbOperations, IUtilityFn utilityFn, IFilterProcess filterProcess)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
            loginMsg = "Your session is expired. Please re-login.";
            _FilterProcess = filterProcess;
        }

        public IActionResult IntakeSummary()
        {
            string sql = "SELECT A.IntakeYearMonth, A.IntakeCode, A.FinalConfirmed, B.JobPositionName FROM Intake A INNER JOIN JobPosition B ON A.JobPositionID = B.JobPositionID WHERE A.FinalConfirmed = 1 ORDER BY IntakeID DESC";
            DataTable tmpTable = _DBOperations.SelectRows(sql);

            List<IntakeViewModel> list = _UtilityFn.ConvertToList<IntakeViewModel>(tmpTable);

            return View(list);
        }

        public IActionResult TalentPoolIntakes()
        {
            string sql = "SELECT DISTINCT IntakeCode FROM FreezeSummary WHERE FreezeNo > 0 ORDER BY IntakeCode";
            DataTable tmpTable = _DBOperations.SelectRows(sql);

            ViewBag.IntakeCode = _UtilityFn.ConvertToSelectList(tmpTable, "IntakeCode", "IntakeCode");

            return View();
        }

        public IActionResult FreezeSummary(string intakeCode)
        {
            string sql = "SELECT * FROM FreezeSummary WHERE IntakeCode = '" + intakeCode + "' ORDER BY FreezeNo DESC";
            DataTable tmpTable = _DBOperations.SelectRows(sql);

            List<FreezeSummary> list = _UtilityFn.ConvertToList<FreezeSummary>(tmpTable);

            ViewBag.IntakeCode = intakeCode;

            return PartialView("_FreezeSummary", list);
        }

        //-----------Final Filter-------------------
        public IActionResult ShowFinal(string intakeCode, int? freezeNo = 0)
        {
            string currentStage = "FINAL";

            List<FinalFilter> list = new();
            if (intakeCode != null)
            {
                list = _FilterProcess.ShowFinal(intakeCode, currentStage, freezeNo);
            }

            ViewBag.IntakeCode = intakeCode;
            ViewBag.FreezeNo = freezeNo;

            return View(list);
        }

        public IActionResult ShowFail(string intakeCode, int? freezeNo = 0)
        {
            string currentStage = "FINAL";

            List<FailedApplicants> list = new();
            if (intakeCode != null)
            {
                list = _FilterProcess.ShowFail(intakeCode, currentStage, freezeNo);
            }

            ViewBag.IntakeCode = intakeCode;
            ViewBag.FreezeNo = freezeNo;

            return View(list);
        }

        [HttpGet]
        [Authorize(Policy = "AdminUserPolicy")]
        public IActionResult ReverseStatus(string applicationCode)
        {
            ViewBag.applicationCode = applicationCode;
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "AdminUserPolicy")]
        public IActionResult ReverseStatus(string applicationCode, string remarks)
        {
            User user = _UtilityFn.GetCurrentUser();
            string userId = user.UserId;

            string sql = "UPDATE FilteredData SET FinalStatus = 'PASS', CurrentStatus = 'PASS', FinalRemarks = FinalRemarks + '," + remarks + "', FinalStatusReversedOn =  getdate(), FinalStatusReversedBy = '" + userId + "' WHERE ApplicationCode = '" + applicationCode + "'";
            string message = _DBOperations.UpdateRecords(sql);

            ViewBag.applicationCode = applicationCode;
            ViewBag.status = message;

            if (message == "SUCCESS")
                ViewBag.message = "Record successfully saved."; 
            else
                ViewBag.message = "Record could not be saved. Please try again. " + message;

            return View();
        }

        [HttpGet]
        [Authorize(Policy = "AdminUserPolicy")]
        public IActionResult MakeNotEligible(string applicationCode)
        {
            ViewBag.applicationCode = applicationCode;
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "AdminUserPolicy")]
        public IActionResult MakeNotEligible(string applicationCode, string remarks)
        {
            User user = _UtilityFn.GetCurrentUser();
            string userId = user.UserId;

            string sql = "UPDATE FilteredData SET FinalStatus = 'FAIL', CurrentStatus = 'FAIL', FinalRemarks = FinalRemarks + '," + remarks + "', FinalStatusReversedOn =  getdate(), FinalStatusReversedBy = '" + userId + "' WHERE ApplicationCode = '" + applicationCode + "'";
            string message = _DBOperations.UpdateRecords(sql);

            ViewBag.applicationCode = applicationCode;
            ViewBag.status = message;

            if (message == "SUCCESS")
                ViewBag.message = "Record successfully saved."; 
            else
                ViewBag.message = "Record could not be saved. Please try again. " + message;

            return View();
        }

        public IActionResult ShortListed(string intakeCode, int? freezeNo)
        {
            string nextStage = "EXAM"; 
            string sql = "";

            if (freezeNo.HasValue)
            {
                sql = "SELECT ExamShortListed, InterviewShortListed, JobShortListed FROM FreezeSummary WHERE IntakeCode = '" + intakeCode + "' AND FreezeNo = " + freezeNo;
            }
            else
            {
                sql = "SELECT ExamShortListed, InterviewShortListed, JobShortListed FROM Intake WHERE IntakeCode = '" + intakeCode + "' AND FinalConfirmed = 1";
            }

            DataTable tmptbl = _DBOperations.SelectRows(sql);

            if (tmptbl.Rows.Count > 0)
            {
                if (tmptbl.Rows[0][0].ToString() == "True")
                    nextStage = "INTERVIEW";
                if (tmptbl.Rows[0][1].ToString() == "True")
                    nextStage = "JOB";
                if (tmptbl.Rows[0][2].ToString() == "True")
                    nextStage = "";
            }

            List<ShortListed> list = new();
            if (intakeCode != null)
            {
                list = _FilterProcess.ShowShortListed(intakeCode, nextStage, freezeNo);
            }

            ViewBag.IntakeCode = intakeCode;
            ViewBag.CurrentStage = nextStage;
            ViewBag.FreezeNo = freezeNo;

            string[] countSummary = _FilterProcess.CountShortListed(intakeCode, freezeNo);

            if (nextStage == "EXAM" && countSummary[0] == "0")
                ViewBag.CountExamSelected = list.Count.ToString();
            else
                ViewBag.CountExamSelected = countSummary[0];

            ViewBag.CountInterviewSelected = countSummary[1];
            ViewBag.CountJobSelected = countSummary[2];

            return View(list);
        }

        public IActionResult UpdateShortListed(IFormCollection formCollection)
        {
            string userId = "";

            User user = _UtilityFn.GetCurrentUser();
            userId = user.UserId;

            string remarks = "";
            string message = "";
            string sql = "";
            string colname1 = "", colname2 = "", colname3 = "";
            string keyname = "";
            string currentStage = formCollection["currentStage"];
            string intakeCode = formCollection["intakeCode"];
            ViewBag.IntakeCode = intakeCode;
            ViewBag.CurrentStage = currentStage;

            DataTable dataTable = new();
            dataTable.Columns.Add("ApplicationCode");

            if (currentStage == "EXAM")
            {
                colname1 = "ExamSelected";
                colname2 = "ExamSelectedUpdatedBy";
                colname3 = "ExamSelectedUpdatedOn";
                keyname = "item.ExamSelected";
            }
            else if (currentStage == "INTERVIEW")
            {
                colname1 = "InterviewSelected";
                colname2 = "InterviewSelectedUpdatedBy";
                colname3 = "InterviewSelectedUpdatedOn";
                keyname = "item.InterviewSelected";
            }
            else if (currentStage == "JOB")
            {
                colname1 = "JobSelected";
                colname2 = "JobSelectedUpdatedBy";
                colname3 = "JobSelectedUpdatedOn";
                keyname = "item.JobSelected";
            }

            dataTable.Columns.Add(colname1);
            dataTable.Columns.Add(colname2);
            dataTable.Columns.Add(colname3);

            string[] applicationCodeArray = Array.Empty<string>();

            var key = formCollection.Keys.Where(k => k.StartsWith("item.ApplicationCode"));
            if (key.Any())
            {
                applicationCodeArray = (string[])formCollection[key.First()];

                key = formCollection.Keys.Where(k => k.StartsWith(keyname));
                string[] currentStatusArray = (string[])formCollection[key.First()];

                for (int i = 0; i < applicationCodeArray.Length; i++)
                {
                    DataRow dr = dataTable.NewRow();
                    remarks = "";

                    dr["ApplicationCode"] = applicationCodeArray[i];
                    dr[colname1] = currentStatusArray[i];
                    dr[colname2] = userId;
                    dr[colname3] = DateTime.Now;

                    dataTable.Rows.Add(dr);
                }

                //update short listed
                message = _DBOperations.UpdateRecords("FilteredData", dataTable, "ApplicationCode");
            }

            ViewBag.Message = message;
            return View("ProcessComplete");
        }

        public IActionResult MoveNext(string intakeCode, string currentStage, int? freezeNo)
        {
            User user = _UtilityFn.GetCurrentUser();
            string userId = user.UserId;
            string message = "";

            DataTable tmpTable = new DataTable();
            if (currentStage == "EXAM")
            {
                tmpTable.Columns.Add("IntakeCode");
                tmpTable.Columns.Add("ExamShortListed");
            }
            else if (currentStage == "INTERVIEW")
            {
                tmpTable.Columns.Add("IntakeCode");
                tmpTable.Columns.Add("InterviewShortListed");
            }
            else if (currentStage == "JOB")
            {
                tmpTable.Columns.Add("IntakeCode");
                tmpTable.Columns.Add("JobShortListed");
            }

            if (intakeCode.Contains("/TP") && freezeNo.HasValue)
            {
                tmpTable.Rows.Add(intakeCode, 1);
                message = _DBOperations.UpdateRecords("FreezeSummary", tmpTable, "INTAKECODE", intakeCode, "", "FreezeNo = " + freezeNo);
            }
            else
            {
                tmpTable.Rows.Add(intakeCode, 1);
                message = _DBOperations.UpdateRecords("INTAKE", tmpTable, "INTAKECODE", intakeCode);
            }

            ViewBag.Message = message;
            ViewBag.CurrentStage = currentStage;
            ViewBag.IntakeCode = intakeCode;

            //if (message == "SUCCESS")
            //    RedirectToAction("ShortListed",new { intakeCode });
            
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
