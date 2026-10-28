using DocumentFormat.OpenXml.Office2010.Excel;
using JobAppHR.Models;
using JobAppHR.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace JobAppHR.Controllers
{
    [Authorize(Policy = "AdminUserPolicy")]
    public class UserManagementController : Controller
    {
        // GET: UserMangementController
        private readonly IDBOperations _DBOperations;
        private readonly IUtilityFn _UtilityFn;
        private string loginMsg = "";

        public UserManagementController(IDBOperations dbOperations, IUtilityFn utilityFn)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
            loginMsg = "Your session is expired. Please re-login.";
        }
        public ActionResult Index(string? msg)
        {
            //string sql = "SELECT A.IntakeID, A.JobPositionID, A.StartDate, A.ClosingDate, A.AgeLimit, A.IntakeYearMonth, A.IntakeCode, A.FinalConfirmed, B.JobPositionName FROM Intake A INNER JOIN JobPosition B ON A.JobPositionID = B.JobPositionID WHERE A.ClosingDate IS NOT NULL";
            string sql = "SELECT U.UserId, U.UserGroup, U.UserEmail, U.UserName, U.ActiveStatus, G.GroupName FROM Users U INNER JOIN UserGroup G ON U.UserGroup = G.UserGroup WHERE U.UserGroup > 0 ORDER BY UserId";
            DataTable tmpTable = _DBOperations.SelectRows(sql);

            List<User> list = _UtilityFn.ConvertToList<User>(tmpTable);

            ViewBag.Message = msg;

            return View(list);
        }

        // GET: UserMangementController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UserMangementController/Create
        public ActionResult Create()
        {
            List<SelectListItem> dataList = _DBOperations.AnyDataList("USERGROUP", "USERGROUP", "GROUPNAME", "", "USERGROUP");
            ViewBag.UserGroup = new SelectList(dataList, "Value", "Text");

            return View();
        }

        // POST: UserMangementController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(User user)
        {
            string userid = "";
            string message = "";

            try
            {
                List<User> list = new List<User>();

                list.Add(user);

                DataTable tmpTable = new DataTable();

                tmpTable = _UtilityFn.ConvertToDataTable(list);
                tmpTable.Columns.Remove("UserRole");
                tmpTable.Columns.Remove("GroupName");

                message = _DBOperations.InsertRecords("USERS", tmpTable, false);

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
            catch
            {
                message = "Record could not be saved. Please try again. " + message;
                return RedirectToAction("Create", new { msg = message });
            }
        }

        // GET: UserMangementController/Edit/5
        public ActionResult Edit(string id)
        {
            string fieldList = "*";
            DataTable tmpTable = _DBOperations.SelectRows("USERS", fieldList, "USERID", id, "", "");

            if (tmpTable.Rows.Count == 0)
            {
                return BadRequest();
            }
            else
            {
                List<User> list = _UtilityFn.ConvertToList<User>(tmpTable);
                User item = list.First();
                List<SelectListItem> dataList = _DBOperations.AnyDataList("USERGROUP", "USERGROUP", "GROUPNAME", "", "USERGROUP");
                ViewBag.UserGroup = new SelectList(dataList, "Value", "Text");

                return View(item);
            }
        }

        // POST: UserMangementController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, User user)
        {
            string userid = "";
            string message = "";

            try
            {
                List<User> list = new List<User>();

                list.Add(user);

                DataTable tmpTable = new DataTable();

                tmpTable = _UtilityFn.ConvertToDataTable(list);
                tmpTable.Columns.Remove("UserRole");
                tmpTable.Columns.Remove("GroupName");

                message = _DBOperations.UpdateRecords("USERS", tmpTable, "USERID", id.ToString(), "int");

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
            catch
            {
                message = "Record could not be saved. Please try again. " + message;
                return RedirectToAction("Edit", new { id = id, msg = message });
            }
        }

        // GET: UserMangementController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UserMangementController/Delete/5
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
