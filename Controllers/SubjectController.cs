using JobAppHR.Models;
using JobAppHR.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace JobApp.Controllers
{
    [Authorize(Policy = "NormalUserPolicy")]
    public class SubjectController : Controller
    {
        // GET: SubjectController
        private readonly IDBOperations _DBOperations;
        private readonly IUtilityFn _UtilityFn;
        private string loginMsg = "";

        public SubjectController(IDBOperations dbOperations, IUtilityFn utilityFn)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
            loginMsg = "Your session is expired. Please re-login.";
        }

        public ActionResult Index(string? msg)
        {
            string fieldList = "*";
            DataTable tmpTable = _DBOperations.SelectRows("SUBJECT", fieldList, "", "MANDATORY = 'NO'", "");

            List<Subject> list = _UtilityFn.ConvertToList<Subject>(tmpTable);

            ViewBag.Message = msg;

            return View(list);
        }

        // GET: SubjectController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: SubjectController/Create
        public ActionResult Create(string? msg)
        {
            List<SelectListItem> dataList = _DBOperations.AnyDataList("EXAMINATION", "EXAMCODE", "EXAMNAME", "", "EXAMNAME");
            ViewBag.ExamCode = new SelectList(dataList, "Value", "Text");
            ViewBag.Message = msg;
            return View();
        }

        // POST: SubjectController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Subject subject)
        {
            string userid = "";
            string message = "";

            //if (HttpContext.Session.GetString("userid") != null)
            //    userid = HttpContext.Session.GetString("userid");
            //else
            //    return RedirectToAction("Login", "Main", new { message = loginMsg });

            try
            {
                List<Subject> list = new List<Subject>();

                list.Add(subject);

                DataTable tmpTable = new DataTable();

                tmpTable = _UtilityFn.ConvertToDataTable(list);

                message = _DBOperations.InsertRecords("SUBJECT", tmpTable, true, "SUBJECTID");

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

        // GET: SubjectController/Edit/5
        public ActionResult Edit(int id, string? msg)
        {
            string fieldList = "*";
            DataTable tmpTable = _DBOperations.SelectRows("SUBJECT", fieldList, "SUBJECTID", id.ToString(), "", "int");

            if (tmpTable.Rows.Count == 0)
            {
                return BadRequest();
            }
            else
            {
                List<Subject> list = _UtilityFn.ConvertToList<Subject>(tmpTable);
                Subject item = list.First();

                List<SelectListItem> dataList = _DBOperations.AnyDataList("EXAMINATION", "EXAMCODE", "EXAMNAME", "", "EXAMNAME");
                ViewBag.ExamCode = new SelectList(dataList, "Value", "Text", item.ExamCode);
                ViewBag.Message = msg;

                return View(item);
            }
        }

        // POST: SubjectController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Subject subject)
        {
            string userid = "";
            string message = "";

            //if (HttpContext.Session.GetString("userid") != null)
            //    userid = HttpContext.Session.GetString("userid");
            //else
            //    return RedirectToAction("Login", "Main", new { message = loginMsg });

            try
            {
                List<Subject> list = new List<Subject>();

                list.Add(subject);

                DataTable tmpTable = new DataTable();

                tmpTable = _UtilityFn.ConvertToDataTable(list);

                message = _DBOperations.UpdateRecords("SUBJECT", tmpTable, "SUBJECTID", id.ToString(), "int");

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

        // GET: SubjectController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: SubjectController/Delete/5
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
