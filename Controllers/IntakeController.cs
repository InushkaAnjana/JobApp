using JobAppHR.Models;
using JobAppHR.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace JobAppHR.Controllers
{
    [Authorize(Policy = "AdminUserPolicy")]
    public class IntakeController : Controller
    {
        private readonly IDBOperations _DBOperations;
        private readonly IUtilityFn _UtilityFn;
        private string loginMsg = "";

        public IntakeController(IDBOperations dbOperations, IUtilityFn utilityFn)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
            loginMsg = "Your session is expired. Please re-login.";
        }
        public IActionResult Index(string? msg)
        {
            string sql = "SELECT A.IntakeID, A.JobPositionID, A.StartDate, A.ClosingDate, A.AgeLimit, A.IntakeYearMonth, A.IntakeCode, A.FinalConfirmed, B.JobPositionName FROM Intake A INNER JOIN JobPosition B ON A.JobPositionID = B.JobPositionID WHERE A.ClosingDate IS NOT NULL";
            DataTable tmpTable = _DBOperations.SelectRows(sql);

            List<IntakeViewModel> list = _UtilityFn.ConvertToList<IntakeViewModel>(tmpTable);

            ViewBag.Message = msg;

            return View(list);
        }

        // GET: IntakeController/Details/5
        // this action method is called from the filter process views as well
        public ActionResult Details(string intakeCode, string? jobPositionName)
        {
            string fieldList = "*";
            DataTable tmpTable = _DBOperations.SelectRows("INTAKE", fieldList, "INTAKECODE", intakeCode, "", "");

            if (tmpTable.Rows.Count == 0)
            {
                return BadRequest();
            }
            else
            {
                List<IntakeViewModel> list = _UtilityFn.ConvertToList<IntakeViewModel>(tmpTable);
                IntakeViewModel item = list.First();
                item.JobPositionName = jobPositionName;

                return View(item);
            }
        }

        // GET: IntakeController/Create
        public ActionResult Create(string? msg)
        {
            List<SelectListItem> dataList = _DBOperations.AnyDataList("JOBPOSITION", "JOBPOSITIONID", "JOBPOSITIONNAME", "", "JOBPOSITIONNAME");
            ViewBag.JobPosition = new SelectList(dataList, "Value", "Text");
            ViewBag.Message = msg;
            return View();
        }

        // POST: IntakeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Intake intake)
        {
            string userId = "";
            string message = "";

            User user = _UtilityFn.GetCurrentUser();
            userId = user.UserId;

            try
            {
                string sql = "SELECT ALRequired, OLRequired, HERequired FROM JobPosition WHERE JobPositionID = " + intake.JobPositionID;
                DataTable dataTable = _DBOperations.SelectRows(sql);

                if (dataTable.Rows.Count > 0)
                {
                    intake.OLRequired = dataTable.Rows[0]["OLRequired"].ToString() == "True" ? 1: 0;
                    intake.ALRequired = dataTable.Rows[0]["ALRequired"].ToString() == "True" ? 1: 0;
                    intake.HERequired = dataTable.Rows[0]["HERequired"].ToString() == "True" ? 1: 0;
                }

                intake.InsertedBy = userId;
                intake.InsertedOn = DateTime.Now;

                List<Intake> list = new()
                {
                    intake
                };

                DataTable tmpTable = new();

                tmpTable = _UtilityFn.ConvertToDataTable(list);

                //remove fields that need not to be updated
                tmpTable.Columns.Remove("UpdatedBy");
                tmpTable.Columns.Remove("UpdatedOn");
                tmpTable.Columns.Remove("FinalConfirmed");
                tmpTable.Columns.Remove("FinalConfirmedBy");
                tmpTable.Columns.Remove("FinalConfirmedOn");

                message = _DBOperations.InsertRecords("INTAKE", tmpTable, true, "INTAKEID");

                if (message == "SUCCESS")
                {
                    message = "Record successfully saved.";
                    return RedirectToAction("Index", new { msg = message });
                }
                else
                {
                    message = "Record could not be saved. Please try again. " + message;
                    return RedirectToAction("Create", new { msg = message });
                }
            }
            catch (Exception ex)
            {
                message = "Record could not be saved. Please try again. " + message;
                return RedirectToAction("Create", new { msg = message });
            }
        }

        [HttpGet]
        public JsonResult GetJobPositionCode(int jobPositionID)
        {
            var jobPositionCode = _DBOperations.GetJobPositionCodeById(jobPositionID);
            return Json(new { jobPositionCode });
        }


        // GET: IntakeController/Edit/5
        public ActionResult Edit(int id)
        {
            string fieldList = "*";
            DataTable tmpTable = _DBOperations.SelectRows("INTAKE", fieldList, "INTAKEID", id.ToString(), "", "int");

            if (tmpTable.Rows.Count == 0)
            {
                return BadRequest();
            }
            else
            {
                List<Intake> list = _UtilityFn.ConvertToList<Intake>(tmpTable);
                Intake item = list.First();

                List<SelectListItem> dataList = _DBOperations.AnyDataList("JobPosition", "JobPositionID", "JobPositionName", "", "JobPositionName");
                ViewBag.JobPosition = new SelectList(dataList, "Value", "Text", item.JobPositionID);

                return View(item);
            }
        }


        // POST: IntakeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Intake intake)
        {
            string userId = "";
            string message = "";

            User user = _UtilityFn.GetCurrentUser();
            userId = user.UserId;

            try
            {
                intake.UpdatedBy = userId;
                intake.UpdatedOn = DateTime.Now;

                List<Intake> list = new List<Intake> { intake };
                DataTable tmpTable = _UtilityFn.ConvertToDataTable(list);

                //remove fields that need not to be updated
                tmpTable.Columns.Remove("InsertedBy");
                tmpTable.Columns.Remove("InsertedOn");
                tmpTable.Columns.Remove("FinalConfirmed"); 
                tmpTable.Columns.Remove("FinalConfirmedBy");
                tmpTable.Columns.Remove("FinalConfirmedOn");

                message = _DBOperations.UpdateRecords("INTAKE", tmpTable, "INTAKEID", id.ToString(), "int");

                if (message == "SUCCESS")
                {
                    message = "Record successfully saved.";
                    return RedirectToAction("Index", new { msg = message });
                }
                else
                {
                    message = "Record could not be saved. Please try again. " + message;
                    return RedirectToAction("Edit", new { id = id, msg = message });
                }
            }
            catch (Exception ex)
            {
                message = "Record could not be saved. Please try again. " + message;
                return RedirectToAction("Edit", new { id = id, msg = message });
            }
        }

        // GET: IntakeController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: IntakeController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

    }
}
