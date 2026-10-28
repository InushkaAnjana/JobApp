using JobAppHR.Models;
using JobAppHR.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace JobAppHR.Controllers
{
    [Authorize(Policy = "NormalUserPolicy")]
    public class PQInstituteController : Controller
    {
        private readonly IDBOperations _DBOperations;
        private readonly IUtilityFn _UtilityFn;
        private string loginMsg = "";

        public PQInstituteController(IDBOperations dbOperations, IUtilityFn utilityFn)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
            loginMsg = "Your session is expired. Please re-login.";
        }

        public IActionResult Index(string? msg)
        {
            string fieldList = "*";
            DataTable tmpTable = _DBOperations.SelectRows("PQINSTITUTE", fieldList, "", "", "");

            List<PQInstitute> list = _UtilityFn.ConvertToList<PQInstitute>(tmpTable);

            ViewBag.Message = msg;

            return View(list);
        }

        // GET: PQInstituteController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PQInstituteController/Create
        public ActionResult Create()
        {
            List<SelectListItem> dataList = new List<SelectListItem>();

            //dataList = _DBOperations.AnyDataList("EXAMINATION", "EXAMCODE", "EXAMNAME", "", "EXAMNAME");
            //ViewBag.ExamCode = new SelectList(dataList, "Value", "Text");

            return View();
        }

        // POST: PQInstituteController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PQInstitute institute)
        {
            string userid = "";
            string message = "";

            //if (HttpContext.Session.GetString("userid") != null)
            //    userid = HttpContext.Session.GetString("userid");
            //else
            //    return RedirectToAction("Login", "Main", new { message = loginMsg });

            try
            {
                List<PQInstitute> list = new List<PQInstitute>();

                list.Add(institute);

                DataTable tmpTable = new DataTable();

                tmpTable = _UtilityFn.ConvertToDataTable(list);

                message = _DBOperations.InsertRecords("PQINSTITUTE", tmpTable, true, "PQINSTITUTEID");

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

        // GET: PQInstituteController/Edit/5
        public ActionResult Edit(int id)
        {
            string fieldList = "*";
            DataTable tmpTable = _DBOperations.SelectRows("PQINSTITUTE", fieldList, "PQINSTITUTEID", id.ToString(), "", "int");

            if (tmpTable.Rows.Count == 0)
            {
                return BadRequest();
            }
            else
            {
                List<PQInstitute> list = _UtilityFn.ConvertToList<PQInstitute>(tmpTable);

                PQInstitute item = list.First();

                List<SelectListItem> dataList = new List<SelectListItem>();

                //dataList = _DBOperations.AnyDataList("EXAMINATION", "EXAMCODE", "EXAMNAME", "", "EXAMNAME");
                //ViewBag.ExamCode = new SelectList(dataList, "Value", "Text", item.ExamCode);

                return View(item);
            }
        }

        // POST: PQInstituteController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, PQInstitute institute)
        {
            string userid = "";
            string message = "";

            //if (HttpContext.Session.GetString("userid") != null)
            //    userid = HttpContext.Session.GetString("userid");
            //else
            //    return RedirectToAction("Login", "Main", new { message = loginMsg });

            try
            {
                List<PQInstitute> list = new List<PQInstitute>();

                list.Add(institute);

                DataTable tmpTable = new DataTable();

                tmpTable = _UtilityFn.ConvertToDataTable(list);

                message = _DBOperations.UpdateRecords("PQINSTITUTE", tmpTable, "PQINSTITUTEID", id.ToString(), "int");

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
                return View(institute);
            }
        }

        // GET: PQInstituteController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PQInstituteController/Delete/5
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
