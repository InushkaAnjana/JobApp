using JobAppHR.Models;
using JobAppHR.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace JobAppHR.Controllers
{
    [Authorize(Policy = "NormalUserPolicy")]
    public class QualTypeController : Controller
    {
        private readonly IDBOperations _DBOperations;
        private readonly IUtilityFn _UtilityFn;
        private string loginMsg = "";

        public QualTypeController(IDBOperations dbOperations, IUtilityFn utilityFn)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
            loginMsg = "Your session is expired. Please re-login.";
        }
        public IActionResult Index(string? msg)
        {
            string fieldList = "*";
            DataTable tmpTable = _DBOperations.SelectRows("QUALTYPE", fieldList, "", "", "");

            List<QualTypes> list = _UtilityFn.ConvertToList<QualTypes>(tmpTable);

            ViewBag.Message = msg;

            return View(list);
        }

        // GET: QualTypeController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: QualTypeController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: QualTypeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(QualTypes qualTypes)
        {
            string userid = "";
            string message = "";

            //if (HttpContext.Session.GetString("userid") != null)
            //    userid = HttpContext.Session.GetString("userid");
            //else
            //    return RedirectToAction("Login", "Main", new { message = loginMsg });

            try
            {
                List<QualTypes> list = new List<QualTypes>();

                list.Add(qualTypes);

                DataTable tmpTable = new DataTable();

                tmpTable = _UtilityFn.ConvertToDataTable(list);

                message = _DBOperations.InsertRecords("QUALTYPE", tmpTable, true, "QTYPEID");

                if (message == "SUCCESS")
                {
                    message = "Record successfully saved.";
                    return RedirectToAction("Index", new { msg = message });
                }
                else
                {
                    message = "Record could not be saved. Please try again. " + message;
                    return RedirectToAction("Create");
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: QualTypeController/Edit/5
        public ActionResult Edit(int id)
        {
            string fieldList = "*";
            DataTable tmpTable = _DBOperations.SelectRows("QUALTYPE", fieldList, "QTYPEID", id.ToString(), "", "int");

            if (tmpTable.Rows.Count == 0)
            {
                return BadRequest();
            }
            else
            {
                List<QualTypes> list = _UtilityFn.ConvertToList<QualTypes>(tmpTable);

                QualTypes item = list.First();

                List<SelectListItem> dataList = new List<SelectListItem>();

                //dataList = _DBOperations.AnyDataList("EXAMINATION", "EXAMCODE", "EXAMNAME", "", "EXAMNAME");
                //ViewBag.ExamCode = new SelectList(dataList, "Value", "Text", item.ExamCode);

                return View(item);
            }
        }

        // POST: QualTypeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, QualTypes qualTypes)
        {
            string userid = "";
            string message = "";

            //if (HttpContext.Session.GetString("userid") != null)
            //    userid = HttpContext.Session.GetString("userid");
            //else
            //    return RedirectToAction("Login", "Main", new { message = loginMsg });

            try
            {
                List<QualTypes> list = new List<QualTypes>();

                list.Add(qualTypes);

                DataTable tmpTable = new DataTable();

                tmpTable = _UtilityFn.ConvertToDataTable(list);

                message = _DBOperations.UpdateRecords("QUALTYPE", tmpTable, "QTYPEID", id.ToString(), "int");

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
                return View(qualTypes);
            }
        }

        // GET: QualTypeController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: QualTypeController/Delete/5
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
