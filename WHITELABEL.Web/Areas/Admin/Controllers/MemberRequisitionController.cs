﻿using log4net;
using NonFactors.Mvc.Grid;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WHITELABEL.Data;
using WHITELABEL.Data.Models;
using WHITELABEL.Web.Controllers;
using WHITELABEL.Web.Helper;
using WHITELABEL.Web.Models;

namespace WHITELABEL.Web.Areas.Admin.Controllers
{
    [Authorize]
    public class MemberRequisitionController : AdminBaseController
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Environment.MachineName);
        public void initpage()
        {
            try
            {
                ViewBag.ControllerName = "Member Hosting";
                SystemClass sclass = new SystemClass();
                string userID = sclass.GetLoggedUser();
                long userid = long.Parse(userID);
                var dbmain = new DBContext();
                if (userID != null && userID != "")
                {
                    TBL_MASTER_MEMBER currUser = dbmain.TBL_MASTER_MEMBER.SingleOrDefault(c => c.MEM_ID == userid && c.ACTIVE_MEMBER == true && c.MEMBER_ROLE==1);
                    if (currUser != null)
                    {
                        Session["WhiteLevelUserId"] = currUser.MEM_ID;
                        // Session["UserName"] = currUser.UserName;
                    }
                }
                if (Session["WhiteLevelUserId"] == null)
                {
                    //Response.Redirect(Url.Action("Index", "Login", new { area = "" }));
                    Response.Redirect(Url.Action("Index", "AdminLogin", new { area = "Area" }));
                    return;
                }
                bool Islogin = false;

                if (Session["WhiteLevelUserId"] != null)
                {
                    Islogin = true;
                    ViewBag.CurrentUserId = MemberCurrentUser.MEM_ID;
                }
                ViewBag.Islogin = Islogin;
            }
            catch (Exception e)
            {
                //ViewBag.UserName = CurrentUser.UserId;
                Console.WriteLine(e.InnerException);
                return;
            }
        }

        public ActionResult Index()
        {
            if (Session["WhiteLevelUserId"] != null)
            {
                initpage();
                return View();
            }
            else
            {
                Session["WhiteLevelUserId"] = null;
                Session["WhiteLevelUserName"] = null;
                Session["UserType"] = null;
                Session.Remove("WhiteLevelUserId");
                Session.Remove("WhiteLevelUserName");
                Session.Remove("UserType");
                return RedirectToAction("Index", "AdminLogin", new { area = "Admin" });
            }   
        }
        public PartialViewResult IndexGrid()
        {
            try
            {
                var db = new DBContext();
                //var transactionlist = (from x in db.TBL_BALANCE_TRANSFER_LOGS
                //                       join y in db.TBL_MASTER_MEMBER on x.FROM_MEMBER equals y.MEM_ID where x.STATUS== "Pending"
                //                       select new
                //                       {
                //                           Touser = "PowerAdmin",
                //                           FromUser = y.UName,
                //                           REQUEST_DATE = x.REQUEST_DATE,
                //                           AMOUNT = x.AMOUNT,
                //                           BANK_ACCOUNT = x.BANK_ACCOUNT,
                //                           TRANSACTION_DETAILS = x.TRANSACTION_DETAILS,
                //                           SLN = x.SLN
                //                       }).AsEnumerable().Select(z => new TBL_BALANCE_TRANSFER_LOGS
                //                       {
                //                           ToUser = z.Touser,
                //                           FromUser = z.FromUser,
                //                           AMOUNT=z.AMOUNT,
                //                           REQUEST_DATE = z.REQUEST_DATE,
                //                           BANK_ACCOUNT = z.BANK_ACCOUNT,
                //                           TRANSACTION_DETAILS = z.TRANSACTION_DETAILS,
                //                           SLN = z.SLN
                //                       }).ToList();
                //return PartialView("IndexGrid", transactionlist);
                return PartialView(CreateExportableGridINExcel());
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MemberRequisition(Admin), method:- IndexGrid (GET) Line No:- 114", ex);
                throw ex;
            }

        }

        [HttpGet]
        public FileResult GridExportIndex()
        {
            // Using EPPlus from nuget
            using (ExcelPackage package = new ExcelPackage())
            {
                Int32 row = 2;
                Int32 col = 1;

                package.Workbook.Worksheets.Add("Data");
                IGrid<TBL_BALANCE_TRANSFER_LOGS> grid = CreateExportableGridINExcel();
                ExcelWorksheet sheet = package.Workbook.Worksheets["Data"];

                foreach (IGridColumn column in grid.Columns)
                {
                    sheet.Cells[1, col].Value = column.Title;
                    sheet.Column(col++).Width = 18;
                }

                foreach (IGridRow<TBL_BALANCE_TRANSFER_LOGS> gridRow in grid.Rows)
                {
                    col = 1;
                    foreach (IGridColumn column in grid.Columns)
                        sheet.Cells[row, col++].Value = column.ValueFor(gridRow);

                    row++;
                }

                return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                //return File(fileContents: package.GetAsByteArray(), contentType: "application/unknown");
            }
        }

        private IGrid<TBL_BALANCE_TRANSFER_LOGS> CreateExportableGridINExcel()
        {
            var db = new DBContext();
            var transactionlist = (from x in db.TBL_BALANCE_TRANSFER_LOGS
                                   join y in db.TBL_MASTER_MEMBER on x.FROM_MEMBER equals y.MEM_ID
                                   where x.STATUS == "Pending"  && x.TO_MEMBER ==MemberCurrentUser.MEM_ID
                                   select new
                                   {
                                       Touser = "White Level",
                                       TransId=x.TransactionID,
                                       FromUser = y.UName,
                                       Reference_NO=x.REFERENCE_NO,
                                       REQUEST_DATE = x.REQUEST_DATE,
                                       AMOUNT = x.AMOUNT,
                                       BANK_ACCOUNT = x.BANK_ACCOUNT,
                                       TRANSACTION_DETAILS = x.TRANSACTION_DETAILS,
                                       SLN = x.SLN,
                                       INSERTED_BY = x.INSERTED_BY,
                                       InsertedByUserName= (db.TBL_MASTER_MEMBER.Where(s => s.MEM_ID == x.INSERTED_BY).Select(a => a.UName).FirstOrDefault())
                                   }).AsEnumerable().Select(z => new TBL_BALANCE_TRANSFER_LOGS
                                   {
                                       ToUser = z.Touser,
                                       TransactionID=z.TransId,
                                       REFERENCE_NO=z.Reference_NO,
                                       FromUser = z.FromUser,
                                       AMOUNT = z.AMOUNT,
                                       REQUEST_DATE = z.REQUEST_DATE,
                                       BANK_ACCOUNT = z.BANK_ACCOUNT,
                                       TRANSACTION_DETAILS = z.TRANSACTION_DETAILS,
                                       SLN = z.SLN,
                                       INSERTED_USERNAME = z.InsertedByUserName
                                   }).ToList().OrderByDescending(x=>x.SLN);

            IGrid<TBL_BALANCE_TRANSFER_LOGS> grid = new Grid<TBL_BALANCE_TRANSFER_LOGS>(transactionlist);
            grid.ViewContext = new ViewContext { HttpContext = HttpContext };
            grid.Query = Request.QueryString;
            grid.Columns.Add(model => model.TransactionID).Titled("Trans Id");
            grid.Columns.Add(model => model.ToUser).Titled("To User");
            grid.Columns.Add(model => model.FromUser).Titled("From Member");
            grid.Columns.Add(model => model.REFERENCE_NO).Titled("Reference No");
            grid.Columns.Add(model => model.REQUEST_DATE).Titled("Req Date").Formatted("{0:d}").MultiFilterable(true);
            grid.Columns.Add(model => model.AMOUNT).Titled("Amount");
            grid.Columns.Add(model => model.BANK_ACCOUNT).Titled("Bank Acnt");
            grid.Columns.Add(model => model.TRANSACTION_DETAILS).Titled("Pay Method");
            grid.Columns.Add(model => model.INSERTED_USERNAME).Titled("Inserted By");
            grid.Columns.Add(model => model.SLN).Titled("").Encoded(false).Filterable(false).Sortable(false)
                .RenderedAs(model => "<a href='" + @Url.Action("RequisitionDetails", "MemberRequisition", new { area = "Admin", transId = Encrypt.EncryptMe(model.SLN.ToString()) }) + "' class='btn btn-primary btn-xs'>Edit</a>");
            //grid.Columns.Add(model => model.SLN).Titled("").Encoded(false).Filterable(false).Sortable(false)
            //        .RenderedAs(model => "<button class='btn btn-primary btn-xs' data-toggle='modal' data-target='.transd' id='transactionvalueid' data-id=" + model.SLN + " onclick='getvalue(" + model.SLN + ");'>Approve</button>");
            grid.Columns.Add(model => model.SLN).Titled("").Encoded(false).Filterable(false).Sortable(false)
        .RenderedAs(model => "<button class='btn btn-primary btn-xs' data-id=" + model.SLN + " onclick='updateStatus(" + model.SLN + ");'>Approve</button>");
            grid.Columns.Add(model => model.SLN).Titled("").Encoded(false).Filterable(false).Sortable(false)
        .RenderedAs(model => "<button class='btn btn-primary btn-xs' data-id=" + model.SLN + " onclick='TransactionDecline(" + model.SLN + ");'>Decline</button>");
            //grid.Columns.Add(model => model.SLN).Titled("").Encoded(false).Filterable(false).Sortable(false)
            //        .RenderedAs(model => "<a href='javascript:void(0)' class='btn btn-denger btn-xs' onclick='DeactivateTransactionlist(" + model.SLN + ");return 0;'>Deactivate</a>");

            grid.Pager = new GridPager<TBL_BALANCE_TRANSFER_LOGS>(grid);
            grid.Processors.Add(grid.Pager);
            grid.Pager.RowsPerPage = 6;

            //foreach (IGridColumn column in grid.Columns)
            //{
            //    column.Filter.IsEnabled = true;
            //    column.Sort.IsEnabled = true;
            //}

            return grid;
        }

        public ActionResult RequisitionDetails(string transId = "")
        {
            if (Session["WhiteLevelUserId"] != null)
            {
                try
                {
                    var db = new DBContext();
                    if (transId == "")
                    {
                        var memberService = (from x in db.TBL_MASTER_MEMBER
                                             where x.CREATED_BY == MemberCurrentUser.MEM_ID
                                             select new
                                             {
                                                 MEM_ID = x.MEM_ID,
                                                 UName = x.MEMBER_NAME
                                             }).AsEnumerable().Select(z => new MemberView
                                             {
                                                 IDValue = z.MEM_ID.ToString(),
                                                 TextValue = z.UName
                                             }).ToList().Distinct();
                        ViewBag.MemberService = new SelectList(memberService, "IDValue", "TextValue");

                        var BankInformation = (from x in db.TBL_SETTINGS_BANK_DETAILS
                                               where x.MEM_ID == MemberCurrentUser.MEM_ID
                                               select new
                                               {
                                                   BankID = x.SL_NO,
                                                   BankName = (x.BANK + "-" + x.ACCOUNT_NO)
                                               }).AsEnumerable().Select(z => new ViewBankDetails
                                               {
                                                   BankID = z.BankID.ToString(),
                                                   BankName = z.BankName
                                               }).ToList().Distinct();
                        ViewBag.BankInformation = new SelectList(BankInformation, "BankID", "BankName");
                        ViewBag.checkbank = "0";
                        return View();
                    }
                    else
                    {
                        string decripttransId = Decrypt.DecryptMe(transId);
                        long transID = long.Parse(decripttransId);
                        var TransactionInfo = db.TBL_BALANCE_TRANSFER_LOGS.Where(x => x.SLN == transID).FirstOrDefault();
                        var memberService = (from x in db.TBL_MASTER_MEMBER
                                             where x.MEM_ID == TransactionInfo.FROM_MEMBER
                                             select new
                                             {
                                                 MEM_ID = x.MEM_ID,
                                                 UName = x.UName
                                             }).AsEnumerable().Select(z => new MemberView
                                             {
                                                 IDValue = z.MEM_ID.ToString(),
                                                 TextValue = z.UName
                                             }).ToList().Distinct();
                        ViewBag.MemberService = new SelectList(memberService, "IDValue", "TextValue");

                        var BankInformation = (from x in db.TBL_SETTINGS_BANK_DETAILS
                                               where x.MEM_ID == MemberCurrentUser.MEM_ID
                                               select new
                                               {
                                                   BankID = x.SL_NO,
                                                   BankName = (x.BANK + "-" + x.ACCOUNT_NO)
                                               }).AsEnumerable().Select(z => new ViewBankDetails
                                               {
                                                   BankID = z.BankID.ToString(),
                                                   BankName = z.BankName
                                               }).ToList().Distinct();
                        ViewBag.BankInformation = new SelectList(BankInformation, "BankID", "BankName");
                        TransactionInfo.FromUser = memberService.Select(x => x.TextValue).FirstOrDefault();                        
                        TransactionInfo.PAYMENT_METHOD = TransactionInfo.PAYMENT_METHOD;                        
                        TransactionInfo.REQUEST_DATE = Convert.ToDateTime(TransactionInfo.REQUEST_DATE.ToString("yyyy-MM-dd").Substring(0, 10));                        
                        TransactionInfo.BANK_ACCOUNT = TransactionInfo.BANK_ACCOUNT;
                        TransactionInfo.TRANSACTION_DETAILS = TransactionInfo.TRANSACTION_DETAILS;
                        TransactionInfo.BANK_CHARGES = TransactionInfo.BANK_CHARGES;
                        ViewBag.checkbank = "1";
                        return View(TransactionInfo);
                    }

                }
                catch (Exception ex)
                {
                    Logger.Error("Controller:-  MemberRequisition(Admin), method:- RequisitionDetails (GET) Line No:- 293", ex);
                    return RedirectToAction("Exception", "ErrorHandler", new { area = "" });
                    throw ex;
                }
            }
            else
            {
                Session["WhiteLevelUserId"] = null;
                Session["WhiteLevelUserName"] = null;
                Session["UserType"] = null;
                Session.Remove("WhiteLevelUserId");
                Session.Remove("WhiteLevelUserName");
                Session.Remove("UserType");
                return RedirectToAction("Index", "AdminLogin", new { area = "Admin" });
            }


        }
        [HttpPost]        
        [ValidateAntiForgeryToken]
        //[ValidateInput(true)]
        public async Task<ActionResult> RequisitionDetails(TBL_BALANCE_TRANSFER_LOGS objval)
        {
            var db = new DBContext();
            using (System.Data.Entity.DbContextTransaction ContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    //long userId = long.Parse(objval.FromUser);
                    // var membertype = db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == userId).FirstOrDefault();
                    var checkAvailableMember = db.TBL_MASTER_MEMBER.Where(x=>x.MEM_ID==objval.FROM_MEMBER).FirstOrDefault();
                    if (checkAvailableMember != null)
                    {

                        var translist = await db.TBL_BALANCE_TRANSFER_LOGS.Where(x => x.SLN == objval.SLN).FirstOrDefaultAsync();
                        if (translist != null)
                        {
                            translist.REQUEST_DATE = objval.REQUEST_DATE;
                            translist.REQUEST_TIME = System.DateTime.Now;
                            translist.BANK_ACCOUNT = objval.BANK_ACCOUNT;
                            translist.AMOUNT = objval.AMOUNT;
                            translist.PAYMENT_METHOD = objval.PAYMENT_METHOD;
                            translist.TRANSFER_METHOD = "Cash";
                            translist.FromUser = "test";
                            translist.TRANSACTION_DETAILS = objval.TRANSACTION_DETAILS;
                            translist.BANK_CHARGES = objval.BANK_CHARGES;
                            translist.INSERTED_BY = MemberCurrentUser.MEM_ID;
                            db.Entry(translist).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                            //return RedirectToAction("Index");
                        }
                        else
                        {
                            //long fromuser = long.Parse(objval.FromUser);
                            var checkrefNo = db.TBL_BALANCE_TRANSFER_LOGS.Where(x => x.REFERENCE_NO == objval.REFERENCE_NO).FirstOrDefault();
                            if (checkrefNo != null)
                            {
                                checkrefNo.REQUEST_DATE = objval.REQUEST_DATE;
                                checkrefNo.REQUEST_TIME = System.DateTime.Now;
                                checkrefNo.BANK_ACCOUNT = objval.BANK_ACCOUNT;
                                checkrefNo.AMOUNT = objval.AMOUNT;
                                checkrefNo.PAYMENT_METHOD = objval.PAYMENT_METHOD;
                                checkrefNo.TRANSFER_METHOD = "Cash";
                                checkrefNo.FromUser = "Admin";
                                checkrefNo.BANK_CHARGES = objval.BANK_CHARGES;
                                checkrefNo.TRANSACTION_DETAILS = objval.TRANSACTION_DETAILS;
                                checkrefNo.INSERTED_BY = MemberCurrentUser.MEM_ID;
                                db.Entry(checkrefNo).State = System.Data.Entity.EntityState.Modified;
                                await db.SaveChangesAsync();
                            }
                            else
                            {
                                long fromuser = objval.FROM_MEMBER;
                                objval.TransactionID = MemberCurrentUser.MEM_ID + "" + fromuser + DateTime.Now.ToString("yyyyMMdd") + "" + DateTime.Now.ToString("HHMMss");
                                objval.TO_MEMBER = MemberCurrentUser.MEM_ID;
                                objval.FROM_MEMBER = fromuser;
                                objval.REQUEST_DATE = Convert.ToDateTime(objval.REQUEST_DATE);
                                objval.REQUEST_TIME = System.DateTime.Now;
                                objval.BANK_ACCOUNT = objval.BANK_ACCOUNT;
                                objval.STATUS = "Pending";
                                objval.TRANSFER_METHOD = "Cash";
                                objval.BANK_CHARGES = objval.BANK_CHARGES;
                                objval.INSERTED_BY = MemberCurrentUser.MEM_ID;
                                db.TBL_BALANCE_TRANSFER_LOGS.Add(objval);
                                await db.SaveChangesAsync();
                            }

                            Logger.Info("");
                            //return RedirectToAction("Index");
                        }
                        ContextTransaction.Commit();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var BankInformation = (from x in db.TBL_SETTINGS_BANK_DETAILS
                                               where x.MEM_ID == MemberCurrentUser.MEM_ID
                                               select new
                                               {
                                                   BankID = x.SL_NO,
                                                   BankName = (x.BANK + "-" + x.ACCOUNT_NO)
                                               }).AsEnumerable().Select(z => new ViewBankDetails
                                               {
                                                   BankID = z.BankID.ToString(),
                                                   BankName = z.BankName
                                               }).ToList().Distinct();
                        ViewBag.BankInformation = new SelectList(BankInformation, "BankID", "BankName");
                        ViewBag.checkbank = "0";
                        ViewBag.msg = "Please provide valid super distributor user name";
                        return View("RequisitionDetails");
                    }
                }
                catch (Exception ex)
                {
                    ContextTransaction.Rollback();
                    Logger.Error("Controller:-  MemberRequisition(Admin), method:- RequisitionDetails (POST) Line No:- 363", ex);
                    return RedirectToAction("Exception", "ErrorHandler", new { area = "" });
                    throw ex;
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeactivateTransactionDetails(string transid = "")
        {
            var db = new DBContext();
            using (System.Data.Entity.DbContextTransaction ContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    long TransID = long.Parse(transid);
                    var transDeactivate = await db.TBL_BALANCE_TRANSFER_LOGS.Where(x => x.SLN == TransID).FirstOrDefaultAsync();
                    transDeactivate.STATUS = "Decline";
                    db.Entry(transDeactivate).State = System.Data.Entity.EntityState.Modified;
                    await db.SaveChangesAsync();
                    ContextTransaction.Commit();
                    return Json(new { Result = "true" });
                }
                catch (Exception ex)
                {
                    Logger.Error("Controller:-  MemberRequisition(Admin), method:- DeactivateTransactionDetails (POST) Line No:- 389", ex);
                    ContextTransaction.Rollback();
                    return Json(new { Result = "false" });
                }
            }

        }

        [HttpPost]        
        public JsonResult getTransdata(string TransId = "")
        {
            try
            {
                var db = new DBContext();
                long transid = long.Parse(TransId);
                //var listdetails = db.TBL_BALANCE_TRANSFER_LOGS.Where(x => x.SLN == transid).FirstOrDefault();
                var listdetails = (from x in db.TBL_BALANCE_TRANSFER_LOGS
                                   join y in db.TBL_MASTER_MEMBER on x.FROM_MEMBER equals y.MEM_ID
                                   where x.SLN == transid
                                   select new
                                   {
                                       Touser = "PowerAdmin",
                                       FromUser = y.UName,
                                       REQUEST_DATE = x.REQUEST_DATE,
                                       AMOUNT = x.AMOUNT,
                                       BANK_ACCOUNT = x.BANK_ACCOUNT,
                                       TRANSACTION_DETAILS = x.TRANSACTION_DETAILS,
                                       SLN = x.SLN
                                   }).AsEnumerable().Select(z => new TBL_BALANCE_TRANSFER_LOGS
                                   {
                                       ToUser = z.Touser,
                                       FromUser = z.FromUser,
                                       AMOUNT = z.AMOUNT,
                                       REQUEST_DATE = z.REQUEST_DATE,
                                       BANK_ACCOUNT = z.BANK_ACCOUNT,
                                       TRANSACTION_DETAILS = z.TRANSACTION_DETAILS,
                                       SLN = z.SLN
                                   }).FirstOrDefault();

                return Json(new { Result = "true", data = listdetails });
            }
            catch (Exception ex)
            {
                var listdetails = new TBL_BALANCE_TRANSFER_LOGS();
                Logger.Error("Controller:-  MemberRequisition(Admin), method:- getTransdata (POST) Line No:- 433", ex);
                return Json(new { Result = "false", data = listdetails });
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<JsonResult> ChangeTransactionStatus(string trandate = "", string TransationStatus = "", string slnval = "")
        public async Task<JsonResult> ChangeTransactionStatus(string slnval = "")
        {
            var db = new DBContext();
            using (System.Data.Entity.DbContextTransaction ContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    long sln = long.Parse(slnval);
                    decimal addamount = 0;
                    decimal memb_bal = 0;
                    decimal trans_bal = 0;
                    decimal availableamt = 0;
                    decimal addavilamt = 0;
                    decimal closingamt = 0;
                    decimal addclosingamt = 0;
                    var transinfo = await db.TBL_BALANCE_TRANSFER_LOGS.Where(x => x.SLN == sln).FirstOrDefaultAsync();

                    var checkAvailAmtWL = await (from valamt in db.TBL_MASTER_MEMBER
                                           join mainbal in db.TBL_ACCOUNTS on valamt.INTRODUCER equals mainbal.MEM_ID
                                           where valamt.MEM_ID == transinfo.FROM_MEMBER
                                           select mainbal).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefaultAsync();

                    if(Convert.ToDecimal(transinfo.AMOUNT) <= Convert.ToDecimal(checkAvailAmtWL.CLOSING))
                    {

                        var membtype = await (from mm in db.TBL_MASTER_MEMBER
                                        join
                                        rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                        where mm.MEM_ID == transinfo.FROM_MEMBER
                                        select new
                                        {
                                            RoleId = mm.MEMBER_ROLE,
                                            roleName = rm.ROLE_NAME,
                                            Amount = mm.BALANCE
                                        }).FirstOrDefaultAsync();
                        //transinfo.REQUEST_DATE = Convert.ToDateTime(trandate);
                        transinfo.STATUS = "Approve";
                        transinfo.FromUser = "test";
                        transinfo.APPROVAL_DATE = DateTime.Now;
                        transinfo.APPROVAL_TIME = DateTime.Now;
                        transinfo.APPROVED_BY = "Power Admin";
                        db.Entry(transinfo).State = System.Data.Entity.EntityState.Modified;
                        await db.SaveChangesAsync();

                        //memb_bal = decimal.Parse(memberlist.BALANCE.ToString());
                        trans_bal = decimal.Parse(transinfo.AMOUNT.ToString());
                        addamount = memb_bal + trans_bal;

                        //db.Entry(memberlist).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();

                        decimal Opening_amt = 0;
                        decimal Closing_amt = 0;
                        decimal Trans_amt = 0;
                        decimal Amount = 0;
                        decimal AddAmount = 0;
                        decimal AddOpening_amt = 0;
                        decimal AddClosing_amt = 0;
                        var amtobj = await db.TBL_ACCOUNTS.Where(x => x.MEM_ID == transinfo.FROM_MEMBER).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefaultAsync();
                        if (amtobj != null)
                        {
                            Opening_amt = amtobj.OPENING;
                            closingamt = amtobj.CLOSING;
                            Amount = amtobj.AMOUNT;
                            AddAmount = decimal.Parse(transinfo.AMOUNT.ToString()) + closingamt;

                        }
                        else
                        {
                            AddAmount = decimal.Parse(transinfo.AMOUNT.ToString())+ decimal.Parse(membtype.Amount.ToString());
                        }
                        TBL_ACCOUNTS objacnt = new TBL_ACCOUNTS()
                        {
                            API_ID = 0,
                            MEM_ID = transinfo.FROM_MEMBER,
                            MEMBER_TYPE = membtype.roleName,
                            TRANSACTION_TYPE = transinfo.PAYMENT_METHOD,
                            TRANSACTION_DATE = Convert.ToDateTime(transinfo.REQUEST_DATE),
                            TRANSACTION_TIME = DateTime.Now,
                            DR_CR = "CR",
                            AMOUNT = decimal.Parse(transinfo.AMOUNT.ToString()),
                            NARRATION = transinfo.TRANSACTION_DETAILS,
                            OPENING = closingamt,
                            CLOSING = AddAmount,
                            REC_NO = 0,
                            COMM_AMT = 0
                        };
                        db.TBL_ACCOUNTS.Add(objacnt);
                        await db.SaveChangesAsync();
                        if (transinfo.BANK_CHARGES > 0)
                        {
                            decimal SubAmt_Val = AddAmount - Convert.ToDecimal(transinfo.BANK_CHARGES);
                            TBL_ACCOUNTS objcomval = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = transinfo.FROM_MEMBER,
                                MEMBER_TYPE = membtype.roleName,
                                TRANSACTION_TYPE = transinfo.PAYMENT_METHOD,
                                TRANSACTION_DATE = Convert.ToDateTime(transinfo.REQUEST_DATE),
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "DR",
                                AMOUNT = decimal.Parse(transinfo.BANK_CHARGES.ToString()),
                                NARRATION = transinfo.TRANSACTION_DETAILS,
                                OPENING = objacnt.CLOSING,
                                CLOSING = SubAmt_Val,
                                REC_NO = 0,
                                COMM_AMT = 0
                            };
                            db.TBL_ACCOUNTS.Add(objcomval);
                            await    db.SaveChangesAsync();                        

                            var memberlistval = await db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == transinfo.FROM_MEMBER).FirstOrDefaultAsync();
                            memberlistval.BALANCE = SubAmt_Val;
                            db.Entry(memberlistval).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                        #region For White level
                        decimal AmtSub = 0;
                        decimal AmtCloseSub = 0;

                        AmtSub = checkAvailAmtWL.CLOSING;
                        AmtCloseSub = checkAvailAmtWL.CLOSING - decimal.Parse(transinfo.AMOUNT.ToString()) - decimal.Parse(transinfo.BANK_CHARGES.ToString());

                        TBL_ACCOUNTS objWLacnt = new TBL_ACCOUNTS()
                        {
                            API_ID = 0,
                            MEM_ID = checkAvailAmtWL.MEM_ID,
                            MEMBER_TYPE = checkAvailAmtWL.MEMBER_TYPE,
                            TRANSACTION_TYPE = transinfo.PAYMENT_METHOD,
                            TRANSACTION_DATE = Convert.ToDateTime(transinfo.REQUEST_DATE),
                            TRANSACTION_TIME = DateTime.Now,
                            DR_CR = "DR",
                            AMOUNT = decimal.Parse(transinfo.AMOUNT.ToString()),
                            NARRATION = transinfo.TRANSACTION_DETAILS,
                            OPENING = AmtSub,
                            CLOSING = AmtCloseSub,
                            REC_NO = 0,
                            COMM_AMT = 0
                        };
                        db.TBL_ACCOUNTS.Add(objWLacnt);
                        await db.SaveChangesAsync();
                        var updatewhitelevelAmt = await db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == checkAvailAmtWL.MEM_ID).FirstOrDefaultAsync();
                        updatewhitelevelAmt.BALANCE = AmtCloseSub;
                        db.Entry(updatewhitelevelAmt).State = System.Data.Entity.EntityState.Modified;
                        await db.SaveChangesAsync();
                        #endregion

                        var memberlist = await db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == transinfo.FROM_MEMBER).FirstOrDefaultAsync();
                        memberlist.BALANCE = AddAmount;
                        db.Entry(memberlist).State = System.Data.Entity.EntityState.Modified;
                        await db.SaveChangesAsync();

                        //#region For Whitelevel

                        //var WlAccount = await db.TBL_ACCOUNTS.Where(x => x.MEM_ID ==MemberCurrentUser.MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefaultAsync();
                        //decimal WLOpeningAmt = 0;
                        //decimal WLClosingAmt = 0;
                        //decimal WLTrnAmt = 0;
                        //if (WlAccount != null)
                        //{
                        //    WLOpeningAmt = WlAccount.OPENING;
                        //    WLClosingAmt = WlAccount.CLOSING;
                        //    WLTrnAmt=WLClosingAmt
                        //}
                        //#endregion
                        EmailHelper emailhelper = new EmailHelper();
                        string mailbody = "Hi " + memberlist.UName + ",<p>Your requisition approved by "+ updatewhitelevelAmt.UName + " .</p>";
                        emailhelper.SendUserEmail(memberlist.EMAIL_ID, "Requisition Approve", mailbody);

                        ContextTransaction.Commit();
                        return Json(new { Result = "true" });
                    }
                    else
                    {
                        return Json(new { Result = "Pending" });
                    }
                }
                catch (Exception ex)
                {
                    ContextTransaction.Rollback();
                    Logger.Error("Controller:-  MemberRequisition(Admin), method:- ChangeTransactionStatus (POST) Line No:- 599", ex);
                    return Json(new { Result = "false" });
                }
            }


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<JsonResult> TransactionDecline(string trandate = "", string TransationStatus = "", string slnval = "")
        public async Task<JsonResult> TransactionDecline(string slnval = "")
        {
            var db = new DBContext();
            using (System.Data.Entity.DbContextTransaction ContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    //var db = new DBContext();
                    long sln = long.Parse(slnval);
                    var transinfo = await db.TBL_BALANCE_TRANSFER_LOGS.Where(x => x.SLN == sln).FirstOrDefaultAsync();
                    //transinfo.REQUEST_DATE = Convert.ToDateTime(trandate);
                    transinfo.STATUS = "Decline";
                    transinfo.FromUser = "test";
                    transinfo.APPROVAL_DATE = DateTime.Now;
                    transinfo.APPROVAL_TIME = DateTime.Now;
                    transinfo.APPROVED_BY = "Power Admin";
                    db.Entry(transinfo).State = System.Data.Entity.EntityState.Modified;
                    await db.SaveChangesAsync();
                    EmailHelper emailhelper = new EmailHelper();
                    var memberlist = await db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == transinfo.FROM_MEMBER).FirstOrDefaultAsync();
                    string mailbody = "Hi " + memberlist.UName + ",<p>Your requisition has been declined.</p>";
                    emailhelper.SendUserEmail(memberlist.EMAIL_ID, "Requisition Decline", mailbody);
                    ContextTransaction.Commit();
                    return Json(new { Result = "true" });
                }
                catch (Exception ex)
                {
                    Logger.Error("Controller:-  MemberRequisition(Admin), method:- TransactionDecline (POST) Line No:- 631", ex);
                    ContextTransaction.Rollback();
                    return Json(new { Result = "false " });
                }
            }


        }


        public ActionResult DeclinedRequisition()
        {
            if (Session["WhiteLevelUserId"] != null)
            {
                initpage();
                return View();
            }
            else
            {
                Session["WhiteLevelUserId"] = null;
                Session["WhiteLevelUserName"] = null;
                Session["UserType"] = null;
                Session.Remove("WhiteLevelUserId");
                Session.Remove("WhiteLevelUserName");
                Session.Remove("UserType");
                return RedirectToAction("Index", "AdminLogin", new { area = "Admin" });
            }
        }
        public PartialViewResult DeclinedIndexGrid()
        {
            try
            {
                var db = new DBContext();
                var transactionlist = (from x in db.TBL_BALANCE_TRANSFER_LOGS
                                       join y in db.TBL_MASTER_MEMBER on x.FROM_MEMBER equals y.MEM_ID
                                       where x.STATUS == "Decline" && x.TO_MEMBER == MemberCurrentUser.MEM_ID
                                       select new
                                       {
                                           Touser = "White Level",
                                           TransId = x.TransactionID,
                                           FromUser = y.UName,
                                           Reference_NO = x.REFERENCE_NO,
                                           REQUEST_DATE = x.REQUEST_DATE,
                                           AMOUNT = x.AMOUNT,
                                           BANK_ACCOUNT = x.BANK_ACCOUNT,
                                           TRANSACTION_DETAILS = x.TRANSACTION_DETAILS,
                                           SLN = x.SLN
                                       }).AsEnumerable().Select((z,index) => new TBL_BALANCE_TRANSFER_LOGS
                                       {
                                           Serial_No=index+1,
                                           ToUser = z.Touser,
                                           TransactionID = z.TransId,
                                           REFERENCE_NO = z.Reference_NO,
                                           FromUser = z.FromUser,
                                           AMOUNT = z.AMOUNT,
                                           REQUEST_DATE = z.REQUEST_DATE,
                                           BANK_ACCOUNT = z.BANK_ACCOUNT,
                                           TRANSACTION_DETAILS = z.TRANSACTION_DETAILS,
                                           SLN = z.SLN
                                       }).ToList();
                return PartialView("DeclinedIndexGrid", transactionlist);
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MemberRequisition(Admin), method:- IndexGrid (GET) Line No:- 114", ex);
                throw ex;
            }

        }


        [ValidateAntiForgeryToken]
        //public async Task<JsonResult> TransactionDecline(string trandate = "", string TransationStatus = "", string slnval = "")
        public async Task<JsonResult> ActivateRequisition(string slnval = "")
        {
            var db = new DBContext();
            using (System.Data.Entity.DbContextTransaction ContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    //var db = new DBContext();
                    long sln = long.Parse(slnval);
                    var transinfo = await db.TBL_BALANCE_TRANSFER_LOGS.Where(x => x.SLN == sln).FirstOrDefaultAsync();
                    //transinfo.REQUEST_DATE = Convert.ToDateTime(trandate);
                    transinfo.STATUS = "Pending";
                    transinfo.FromUser = "test";
                    transinfo.APPROVAL_DATE = DateTime.Now;
                    transinfo.APPROVAL_TIME = DateTime.Now;
                    transinfo.APPROVED_BY = "Power Admin";
                    db.Entry(transinfo).State = System.Data.Entity.EntityState.Modified;
                    await db.SaveChangesAsync();
                    var memberlist = await db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == transinfo.FROM_MEMBER).FirstOrDefaultAsync();
                    EmailHelper emailhelper = new EmailHelper();
                    string mailbody = "Hi " + memberlist.UName + ",<p>Your requisition has been activated.</p>";
                    emailhelper.SendUserEmail(memberlist.EMAIL_ID, "Requisition Activated", mailbody);
                    ContextTransaction.Commit();
                    return Json(new { Result = "true" });
                }
                catch (Exception ex)
                {
                    Logger.Error("Controller:-  MemberRequisition(Admin), method:- TransactionDecline (POST) Line No:- 631", ex);
                    ContextTransaction.Rollback();
                    return Json(new { Result = "false " });
                }
            }


        }



        public ActionResult RequisitionReport()
        {
            if (Session["WhiteLevelUserId"] != null)
            {
                initpage();
                return View();
            }
            else
            {
                Session["WhiteLevelUserId"] = null;
                Session["WhiteLevelUserName"] = null;
                Session["UserType"] = null;
                Session.Remove("WhiteLevelUserId");
                Session.Remove("WhiteLevelUserName");
                Session.Remove("UserType");
                return RedirectToAction("Index", "AdminLogin", new { area = "Admin" });
            }
        }

        [HttpPost]        
        public JsonResult DateWiseSearchRequisition(string Datefrom = "", string dateto = "")
        {
            try
            {
                var db = new DBContext();
                DateTime datefrom = DateTime.Parse(Datefrom, System.Globalization.CultureInfo.GetCultureInfo("en-Us").DateTimeFormat);
                DateTime DateTO = DateTime.Parse(dateto, System.Globalization.CultureInfo.GetCultureInfo("en-Us").DateTimeFormat);
                var memberinfo = db.TBL_BALANCE_TRANSFER_LOGS.Where(x => x.REQUEST_DATE >= datefrom && x.REQUEST_DATE <= DateTO).ToList();
                return Json(new { Result = "true", infor = memberinfo });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "false" });
            }

        }
        public string FormatDate(string date, string format)
        {
            try
            {
                return DateTime.Parse(date).ToString(format);
            }
            catch (FormatException)
            {
                return string.Empty;
            }
        }

        public PartialViewResult RequisitionGrid()
        {
            try
            {
                //var db = new DBContext();              
                //    var transactionlist = (from x in db.TBL_BALANCE_TRANSFER_LOGS
                //                           join y in db.TBL_MASTER_MEMBER on x.FROM_MEMBER equals y.MEM_ID                                           
                //                           select new
                //                           {
                //                               Touser = "PowerAdmin",
                //                               FromUser = y.UName,
                //                               REQUEST_DATE = x.REQUEST_DATE,
                //                               AMOUNT = x.AMOUNT,
                //                               BANK_ACCOUNT = x.BANK_ACCOUNT,
                //                               TRANSACTION_DETAILS = x.TRANSACTION_DETAILS,
                //                               STATUS = x.STATUS,
                //                               APPROVED_BY = x.APPROVED_BY,
                //                               APPROVAL_DATE = x.APPROVAL_DATE,
                //                               SLN = x.SLN
                //                           }).AsEnumerable().Select(z => new TBL_BALANCE_TRANSFER_LOGS
                //                           {
                //                               ToUser = z.Touser,
                //                               FromUser = z.FromUser,
                //                               AMOUNT = z.AMOUNT,
                //                               REQUEST_DATE = z.REQUEST_DATE,
                //                               BANK_ACCOUNT = z.BANK_ACCOUNT,
                //                               TRANSACTION_DETAILS = z.TRANSACTION_DETAILS,
                //                               STATUS = z.STATUS,
                //                               APPROVED_BY = z.APPROVED_BY,
                //                               APPROVAL_DATE = z.APPROVAL_DATE,
                //                               SLN = z.SLN
                //                           }).ToList();
                ////return PartialView("IndexGrid", transactionlist);


                //var transactiondetails = db.TBL_BALANCE_TRANSFER_LOGS.ToList();
                return PartialView(CreateExportableGrid());


            }
            catch (Exception ex)
            {
                throw ex;
            }
            //return PartialView();
        }
        
        [HttpGet]
        public FileResult ExportIndex()
        {
            // Using EPPlus from nuget
            using (ExcelPackage package = new ExcelPackage())
            {
                Int32 row = 2;
                Int32 col = 1;

                package.Workbook.Worksheets.Add("Data");
                IGrid<TBL_BALANCE_TRANSFER_LOGS> grid = CreateExportableGrid();
                ExcelWorksheet sheet = package.Workbook.Worksheets["Data"];

                foreach (IGridColumn column in grid.Columns)
                {
                    sheet.Cells[1, col].Value = column.Title;
                    sheet.Column(col++).Width = 18;
                }

                foreach (IGridRow<TBL_BALANCE_TRANSFER_LOGS> gridRow in grid.Rows)
                {
                    col = 1;
                    foreach (IGridColumn column in grid.Columns)
                        sheet.Cells[row, col++].Value = column.ValueFor(gridRow);

                    row++;
                }

                return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                //return File(fileContents: package.GetAsByteArray(), contentType: "application/unknown");
            }
        }

        private IGrid<TBL_BALANCE_TRANSFER_LOGS> CreateExportableGrid()
        {
            var db = new DBContext();
            var transactionlist = (from x in db.TBL_BALANCE_TRANSFER_LOGS
                                   join y in db.TBL_MASTER_MEMBER on x.FROM_MEMBER equals y.MEM_ID where x.TO_MEMBER != 0
                                   select new
                                   {
                                       Touser = "White Level",
                                       transId=x.TransactionID,
                                       FromUser = y.UName,
                                       REQUEST_DATE = x.REQUEST_DATE,
                                       AMOUNT = x.AMOUNT,
                                       BANK_ACCOUNT = x.BANK_ACCOUNT,
                                       TRANSACTION_DETAILS = x.TRANSACTION_DETAILS,
                                       STATUS = x.STATUS,
                                       APPROVED_BY = x.APPROVED_BY,
                                       APPROVAL_DATE = x.APPROVAL_DATE,
                                       SLN = x.SLN
                                   }).AsEnumerable().Select(z => new TBL_BALANCE_TRANSFER_LOGS
                                   {
                                       ToUser = z.Touser,
                                       TransactionID=z.transId,
                                       FromUser = z.FromUser,
                                       AMOUNT = z.AMOUNT,
                                       REQUEST_DATE = z.REQUEST_DATE,
                                       BANK_ACCOUNT = z.BANK_ACCOUNT,
                                       TRANSACTION_DETAILS = z.TRANSACTION_DETAILS,
                                       STATUS = z.STATUS,
                                       APPROVED_BY = z.APPROVED_BY,
                                       APPROVAL_DATE = z.APPROVAL_DATE,
                                       SLN = z.SLN
                                   }).ToList();

            IGrid<TBL_BALANCE_TRANSFER_LOGS> grid = new Grid<TBL_BALANCE_TRANSFER_LOGS>(transactionlist);
            grid.ViewContext = new ViewContext { HttpContext = HttpContext };
            grid.Query = Request.QueryString;
            grid.Columns.Add(model => model.TransactionID).Titled("Trans Id");
            grid.Columns.Add(model => model.ToUser).Titled("To User");
            grid.Columns.Add(model => model.FromUser).Titled("From Member");
            grid.Columns.Add(model => model.REQUEST_DATE).Titled("Req Date").Formatted("{0:d}").MultiFilterable(true);
            grid.Columns.Add(model => model.AMOUNT).Titled("Amount");
            grid.Columns.Add(model => model.BANK_ACCOUNT).Titled("Bank Acnt");
            grid.Columns.Add(model => model.TRANSACTION_DETAILS).Titled("Pay Method");
            grid.Columns.Add(model => model.STATUS).Titled("STATUS");
            grid.Columns.Add(model => model.APPROVAL_DATE).Titled("Apprv/Decline Date").Formatted("{0:d}").MultiFilterable(true);
            grid.Columns.Add(model => model.APPROVED_BY).Titled("Apprv By");
            grid.Pager = new GridPager<TBL_BALANCE_TRANSFER_LOGS>(grid);
            grid.Processors.Add(grid.Pager);
            grid.Pager.RowsPerPage = 6;

            foreach (IGridColumn column in grid.Columns)
            {
                column.Filter.IsEnabled = true;
                column.Sort.IsEnabled = true;
            }

            return grid;
        }
        
        public ActionResult GetPeople(string query)
        {
            return Json(_GetPeople(query), JsonRequestBehavior.AllowGet);
        }

        private List<Autocomplete> _GetPeople(string query)
        {
            List<Autocomplete> people = new List<Autocomplete>();
            var db = new DBContext();
            try
            {
                var results = (from p in db.TBL_MASTER_MEMBER
                               where (p.UName).Contains(query) && p.INTRODUCER == MemberCurrentUser.MEM_ID
                               orderby p.UName
                               select p).ToList();
                foreach (var r in results)
                {
                    // create objects
                    Autocomplete Username = new Autocomplete();

                    //Username.FromUser = string.Format("{0} {1}", r.UName);
                    Username.Name = (r.UName);
                    Username.Id = r.MEM_ID;

                    people.Add(Username);
                }

            }
            catch (EntityCommandExecutionException eceex)
            {
                if (eceex.InnerException != null)
                {
                    throw eceex.InnerException;
                }
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return people;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CheckReferenceNo(string referenceno)
        {
            var context = new DBContext();
            var User = await context.TBL_BALANCE_TRANSFER_LOGS.Where(model => model.REFERENCE_NO == referenceno && model.TO_MEMBER==MemberCurrentUser.MEM_ID).FirstOrDefaultAsync();
            if (User != null)
            {
                var list = context.TBL_WHITE_LEVEL_HOSTING_DETAILS.Where(x => x.MEM_ID == User.FROM_MEMBER).FirstOrDefault();
                return Json(new { result = "unavailable" });
                //return Json(new { result = "unavailable",Mem_Name= list.DOMAIN,mem_Id =User.FROM_MEMBER,Req_Date=User.REQUEST_DATE,amt=User.AMOUNT,Bankid=User.BANK_ACCOUNT,paymethod=User.PAYMENT_METHOD,Transdetails=User.TRANSACTION_DETAILS,BankCharges=User.BANK_CHARGES});
            }
            else
            {
                return Json(new { result = "available" });
            }
        }
    }
}