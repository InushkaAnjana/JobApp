using JobAppHR.Models;
using JobAppHR.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace JobAppHR.Controllers
{
    [Authorize(Policy = "NormalUserPolicy")]
    public class QualNameController : Controller
    {
        private readonly IDBOperations _DBOperations;
        private readonly IUtilityFn _UtilityFn;
        private string loginMsg = "";

        public QualNameController(IDBOperations dbOperations, IUtilityFn utilityFn)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
            loginMsg = "Your session is expired. Please re-login.";
        }

        public ActionResult Index(string? msg)
        {
            string sql = "SELECT A.QNameID, A.QualName, B.QualType FROM QualName A INNER JOIN QualType B ON A.QTypeID = B.QTypeID";
            DataTable tmpTable = _DBOperations.SelectRows(sql);
            
            List<QualNameViewModel> list = _UtilityFn.ConvertToList<QualNameViewModel>(tmpTable);

            ViewBag.Message = msg;

            return View(list);
        }

        // GET: QualNameController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: QualNameController/Create
        public ActionResult Create(string? msg)
        {
            List<SelectListItem> dataList = new List<SelectListItem>();

            dataList = _DBOperations.AnyDataList("QUALTYPE", "QTYPEID", "QUALTYPE", "", "QUALTYPE");            
            ViewBag.QualType = new SelectList(dataList, "Value", "Text");
            
            ViewBag.Message = msg;
            
            return View();
        }

        // POST: QualNameController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(QualNames qualNames)
        {
            string userid = "";
            string message = "";

            //if (HttpContext.Session.GetString("userid") != null)
            //    userid = HttpContext.Session.GetString("userid");
            //else
            //    return RedirectToAction("Login", "Main", new { message = loginMsg });

            try
            {
                List<QualNames> list = new List<QualNames>
                {
                    qualNames
                };

                DataTable tmpTable = new DataTable();

                tmpTable = _UtilityFn.ConvertToDataTable(list);

                message = _DBOperations.InsertRecords("QUALNAME", tmpTable, true, "QNAMEID");

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

        // GET: QualNameController/Edit/5
        public ActionResult Edit(int id, string? msg)
        {
            // Fetch the QualNames record by its ID
            DataTable tmpTable = _DBOperations.SelectRows("QUALNAME", "*", "QNAMEID", id.ToString(), "", "int");

            if (tmpTable.Rows.Count == 0)
            {
                return BadRequest();
            }
            else
            {
                List<QualNames> list = _UtilityFn.ConvertToList<QualNames>(tmpTable);
                QualNames item = list.First();

                List<SelectListItem> dataList = _DBOperations.AnyDataList("QUALTYPE", "QTYPEID", "QUALTYPE", "", "QUALTYPE");
                ViewBag.QualType = new SelectList(dataList, "Value", "Text", item.QTypeID);
                ViewBag.Message = msg;

                return View(item);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, QualNames qualNames)
        {
            string userid = "";
            string message = "";

            //if (HttpContext.Session.GetString("userid") != null)
            //    userid = HttpContext.Session.GetString("userid");
            //else
            //    return RedirectToAction("Login", "Main", new { message = loginMsg });

            try
            {
                qualNames.QNameID = id;

                DataTable updatedData = _UtilityFn.ConvertToDataTable(new List<QualNames> { qualNames });
                message = _DBOperations.UpdateRecords("QUALNAME", updatedData, "QNAMEID", id.ToString(), "int");

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



        // GET: QualNameController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: QualNameController/Delete/5
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
