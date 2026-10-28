using DocumentFormat.OpenXml.Spreadsheet;
using JobAppHR.Models;
using JobAppHR.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace JobAppHR.Controllers
{
    [Authorize(Policy = "NormalUserPolicy")]
    public class TPIntakeController : Controller
    {
        private readonly IDBOperations _DBOperations;
        private readonly IUtilityFn _UtilityFn;
        private string loginMsg = "";

        public TPIntakeController(IDBOperations dbOperations, IUtilityFn utilityFn)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
            loginMsg = "Your session is expired. Please re-login.";
        }

        // GET: TPIntakeController
        public ActionResult Index(string? msg)
        {
            string sql = "SELECT A.IntakeID, A.JobPositionID, A.IntakeCode, A.FilterMode, A.AgeLimit, B.JobPositionName FROM Intake A INNER JOIN JobPosition B ON A.JobPositionID = B.JobPositionID WHERE A.ClosingDate IS NULL AND A.IntakeYearMonth IS NULL";
            DataTable tmpTable = _DBOperations.SelectRows(sql);

            List<IntakeViewModel> list = _UtilityFn.ConvertToList<IntakeViewModel>(tmpTable);

            ViewBag.Message = msg;
            ViewBag.IsTalentPoolEnabled = _DBOperations.IsTalentPoolEnabled();
            ViewBag.UserGroup = User.FindFirst("UserGroup")?.Value;

            return View(list);
        }

        [HttpPost]
        public IActionResult ToggleTalentPoolStatus([FromBody] bool isEnabled)
        {
            var userGroup = User.FindFirst("UserGroup")?.Value;
            if (userGroup != "1" && userGroup != "0")
            {
                return Forbid();
            }

            try
            {
                _DBOperations.UpdateTalentPoolStatus(isEnabled);
                return Ok(new { success = true, message = "Status updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: TPIntakeController/Details/5
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

        // GET: TPIntakeController/Create
        public ActionResult Create(string? msg)
        {
            List<SelectListItem> dataList = _DBOperations.AnyDataList("JOBPOSITION", "JOBPOSITIONID", "JOBPOSITIONNAME", "TALENTPOOLJOB = 'YES'", "JOBPOSITIONNAME");
            ViewBag.JobPosition = new SelectList(dataList, "Value", "Text");
            ViewBag.Message = msg;
            return View();
        }

        // POST: TPIntakeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TalentPoolIntake intake)
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
                    intake.OLRequired = dataTable.Rows[0]["OLRequired"].ToString() == "True" ? 1 : 0;
                    intake.ALRequired = dataTable.Rows[0]["ALRequired"].ToString() == "True" ? 1 : 0;
                    intake.HERequired = dataTable.Rows[0]["HERequired"].ToString() == "True" ? 1 : 0;
                }

                intake.InsertedBy = userId;
                intake.InsertedOn = DateTime.Now;

                List<TalentPoolIntake> list = new()
                {
                    intake
                };

                DataTable tmpTable = new();

                tmpTable = _UtilityFn.ConvertToDataTable(list);

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

        // GET: TPIntakeController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TPIntakeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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

        // GET: TPIntakeController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TPIntakeController/Delete/5
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
