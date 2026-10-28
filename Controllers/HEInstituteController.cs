using JobAppHR.Models;
using JobAppHR.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace JobAppHR.Controllers
{
    [Authorize(Policy = "NormalUserPolicy")]
    public class HEInstituteController : Controller
    {
        private readonly IDBOperations _DBOperations;
        private readonly IUtilityFn _UtilityFn;
        private string loginMsg = "";

        public HEInstituteController(IDBOperations dbOperations, IUtilityFn utilityFn)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
            loginMsg = "Your session is expired. Please re-login.";
        }
        public IActionResult Index(string? msg)
        {
            string fieldList = "*";
            DataTable tmpTable = _DBOperations.SelectRows("HEINSTITUTE", fieldList, "", "", "");

            List<HEInstitute> list = _UtilityFn.ConvertToList<HEInstitute>(tmpTable);

            ViewBag.Message = msg;

            return View(list);
        }

        // GET: HEInstituteController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: HEInstituteController/Create
        public ActionResult Create(string? msg)
        {
            ViewBag.Message = msg;
            return View();
        }

        // POST: HEInstituteController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            string userid = "";
            string message = "";

            //if (HttpContext.Session.GetString("userid") != null)
            //    userid = HttpContext.Session.GetString("userid");
            //else
            //    return RedirectToAction("Login", "Main", new { message = loginMsg });

            try
            {
                HEInstitute record = new();

                record.HEInstituteName = collection["HEInstituteName"];

                List<HEInstitute> list = new List<HEInstitute>();

                list.Add(record);

                DataTable tmpTable = new DataTable();

                tmpTable = _UtilityFn.ConvertToDataTable(list);

                message = _DBOperations.InsertRecords("HEINSTITUTE", tmpTable, true, "HEINSTITUTEID");

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
                return View();
            }
        }

        // GET: HEInstituteController/Edit/5
        public ActionResult Edit(int id)
        {
            string fieldList = "*";
            DataTable tmpTable = _DBOperations.SelectRows("HEINSTITUTE", fieldList, "HEINSTITUTEID", id.ToString(), "", "int");

            if (tmpTable.Rows.Count == 0)
            {
                return BadRequest();
            }
            else
            {
                List<HEInstitute> list = _UtilityFn.ConvertToList<HEInstitute>(tmpTable);

                HEInstitute item = list.First();

                return View(item);
            }
        }

        // POST: HEInstituteController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            string userid = "";
            string message = "";

            //if (HttpContext.Session.GetString("userid") != null)
            //    userid = HttpContext.Session.GetString("userid");
            //else
            //    return RedirectToAction("Login", "Main", new { message = loginMsg });

            try
            {
                HEInstitute record = new();

                record.HEInstituteName = collection["HEInstituteName"];

                List<HEInstitute> list = new List<HEInstitute>();

                list.Add(record);

                DataTable tmpTable = new DataTable();

                tmpTable = _UtilityFn.ConvertToDataTable(list);

                message = _DBOperations.UpdateRecords("HEINSTITUTE", tmpTable, "HEINSTITUTEID", id.ToString(), "int");

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
                return View();
            }
        }

        // GET: HEInstituteController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: HEInstituteController/Delete/5
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
