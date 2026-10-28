using JobAppHR.Models;
using JobAppHR.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace JobAppHR.Controllers
{
    [Authorize(Policy = "NormalUserPolicy")]
    public class MembershipTypeController : Controller
    {
        private readonly IDBOperations _DBOperations;
        private readonly IUtilityFn _UtilityFn;
        private string loginMsg = "";

        public MembershipTypeController(IDBOperations dbOperations, IUtilityFn utilityFn)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
            loginMsg = "Your session is expired. Please re-login.";
        }

        public IActionResult Index(string? msg)
        {
            string fieldList = "*";
            DataTable tmpTable = _DBOperations.SelectRows("MEMBERSHIPTYPE", fieldList, "", "", "");

            List<MembershipTypes> list = _UtilityFn.ConvertToList<MembershipTypes>(tmpTable);

            ViewBag.Message = msg;

            return View(list);
        }

        // GET: MembershipTypeController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: MembershipTypeController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MembershipTypeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MembershipTypes membershipTypes)
        {
            string userid = "";
            string message = "";

            //if (HttpContext.Session.GetString("userid") != null)
            //    userid = HttpContext.Session.GetString("userid");
            //else
            //    return RedirectToAction("Login", "Main", new { message = loginMsg });

            try
            {
                List<MembershipTypes> list = new List<MembershipTypes>();

                list.Add(membershipTypes);

                DataTable tmpTable = new DataTable();

                tmpTable = _UtilityFn.ConvertToDataTable(list);

                message = _DBOperations.InsertRecords("MEMBERSHIPTYPE", tmpTable, true, "MEMBERSHIPTYPEID");

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

        // GET: MembershipTypeController/Edit/5
        public ActionResult Edit(int id)
        {
            string fieldList = "*";
            DataTable tmpTable = _DBOperations.SelectRows("MEMBERSHIPTYPE", fieldList, "MEMBERSHIPTYPEID", id.ToString(), "", "int");

            if (tmpTable.Rows.Count == 0)
            {
                return BadRequest();
            }
            else
            {
                List<MembershipTypes> list = _UtilityFn.ConvertToList<MembershipTypes>(tmpTable);

                MembershipTypes item = list.First();

                return View(item);
            }
        }

        // POST: MembershipTypeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, MembershipTypes membershipTypes)
        {
            string userid = "";
            string message = "";

            //if (HttpContext.Session.GetString("userid") != null)
            //    userid = HttpContext.Session.GetString("userid");
            //else
            //    return RedirectToAction("Login", "Main", new { message = loginMsg });

            try
            {
                List<MembershipTypes> list = new List<MembershipTypes>();

                list.Add(membershipTypes);

                DataTable tmpTable = new DataTable();

                tmpTable = _UtilityFn.ConvertToDataTable(list);

                message = _DBOperations.UpdateRecords("MEMBERSHIPTYPE", tmpTable, "MEMBERSHIPTYPEID", id.ToString(), "int");

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
                return View(membershipTypes);
            }
        }

        // GET: MembershipTypeController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MembershipTypeController/Delete/5
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
