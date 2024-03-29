﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WHITELABEL.Data;
using WHITELABEL.Data.Models;
using WHITELABEL.Web.Models;
using WHITELABEL.Web.Helper;
using System.Data.Entity.Core;
using WHITELABEL.Web.Areas.Merchant.Models;
using WHITELABEL.Web.ServiceApi.RECHARGE.PORTIQUE;
using static WHITELABEL.Web.Helper.InstantPayApi;
using NonFactors.Mvc.Grid;
using OfficeOpenXml;
using log4net;
using System.Threading.Tasks;
using System.Data.Entity;

namespace WHITELABEL.Web.Areas.Merchant.Controllers
{
    [Authorize]
    public class MerchantDMRDashboardController : MerchantBaseController
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Environment.MachineName);
        public void initpage()
        {
            try
            {
                ViewBag.ControllerName = "Merchant Dashboard";
                SystemClass sclass = new SystemClass();
                string userID = sclass.GetLoggedUser();
                long userid = long.Parse(userID);
                var dbmain = new DBContext();
                if (userID != null && userID != "")
                {
                    TBL_MASTER_MEMBER currUser = dbmain.TBL_MASTER_MEMBER.SingleOrDefault(c => c.MEM_ID == userid && c.MEMBER_ROLE == 5 && c.ACTIVE_MEMBER == true);
                    if (currUser != null)
                    {
                        Session["MerchantUserId"] = currUser.MEM_ID;
                        // Session["UserName"] = currUser.UserName;
                    }
                    else
                    {
                        Response.Redirect(Url.Action("Index", "MerchantLogin", new { area = "Merchant" }));
                        return;
                    }
                }
                if (Session["MerchantUserId"] == null)
                {
                    //Response.Redirect(Url.Action("Index", "Login", new { area = "" }));
                    Response.Redirect(Url.Action("Index", "MerchantLogin", new { area = "Merchant" }));
                    return;
                }
                bool Islogin = false;
                if (Session["MerchantUserId"] != null)
                {
                    Islogin = true;
                    ViewBag.CurrentUserId = CurrentMerchant.MEM_ID;
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

        // GET: Merchant/MerchantDMRDashboard
        #region Beneficiary transactionList            
        public ActionResult Index()
        {
            initpage();
            return View();
        }
        public PartialViewResult DMRTransactionList()
        {
            return PartialView(DisplayDMRTransaction());
        }
        private IGrid<TBL_DMR_FUND_TRANSFER_DETAILS> DisplayDMRTransaction()
        {
            try
            {
                var db = new DBContext();
                var remitterid = Session["MerchantDMRId"].ToString();
                var DMRTransaction = db.TBL_DMR_FUND_TRANSFER_DETAILS.Where(x => x.REMITTER_ID == remitterid).ToList();

                ////var bankdetails = db.TBL_SETTINGS_BANK_DETAILS.Where(x => x.ISDELETED == 0  && x.MEM_ID==MemberCurrentUser.MEM_ID).ToList();
                //var bankdetails = db.TBL_REMITTER_BENEFICIARY_INFO.Where(x => x.IsActive == 0 && x.RemitterID == remitterid).ToList();

                IGrid<TBL_DMR_FUND_TRANSFER_DETAILS> grid = new Grid<TBL_DMR_FUND_TRANSFER_DETAILS>(DMRTransaction);
                grid.ViewContext = new ViewContext { HttpContext = HttpContext };
                grid.Query = Request.QueryString;
                grid.Columns.Add(model => model.IPAY_ID).Titled("IPAY_ID").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.REF_NO).Titled("REF_NO").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.OPR_ID).Titled("OPR_ID").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.NAME).Titled("NAME").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.OPENING_BAL).Titled("OPENING_BAL").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.AMOUNT).Titled("AMOUNT").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.CHARGED_AMT).Titled("CHARGED_AMT").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.LOCKED_AMT).Titled("LOCKED_AMT").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.BANK_ALIAS).Titled("BANK_ALIAS").Filterable(true).Sortable(true);
                //grid.Columns.Add(model => model.TXNID).Titled("STATUS").Encoded(false).Filterable(false).Sortable(false)
                //    .RenderedAs(model => "<label class='label " + (model.STATUS == "Transaction Successful" ? "label-success" : "label-danger") + "'> " + model.STATUS + " </label>");                
                grid.Columns.Add(model => model.TXNID).Titled("STATUS").Encoded(false).Filterable(false).Sortable(false)
                    .RenderedAs(model => "<label class='label " + (model.STATUS == "Transaction Successful" ? "label-success" : model.STATUS == "PENDING" ? "label-danger" : "label-info") + "'> " + model.STATUS + " </label>");
                grid.Columns.Add(model => model.REQ_DATE).Titled("REQ_DATE").Filterable(true).Sortable(true).MultiFilterable(true);
                grid.Columns.Add(model => model.REFUNF_ALLOWED).Titled("REFUNF_ALLOWED NAME").Filterable(true).Sortable(true);
                //grid.Columns.Add(model => model.ID).Titled("").Encoded(false).Filterable(false).Sortable(false)
                //    .RenderedAs(model => "<a href='javascript:void(0)' class='btn btn-denger btn-xs' onclick='DeActivateBeneficiary(" + model.ID + ");return 0;'>DELETE</a>");
                grid.Pager = new GridPager<TBL_DMR_FUND_TRANSFER_DETAILS>(grid);
                grid.Processors.Add(grid.Pager);
                grid.Pager.RowsPerPage = 6;

                //foreach (IGridColumn column in grid.Columns)
                //{
                //    column.Filter.IsEnabled = true;
                //    column.Sort.IsEnabled = true;
                //}

                return grid;
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantDMRDashboard(Merchant), method:- DisplayDMRTransaction(GET) Line No:- 125", ex);
                throw ex;
            }

        }
        #endregion

        public ActionResult DMRInformation()
        {
            if (Session["MerchantDMRId"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "MerchantDMRLogin", new { area = "Merchant" });
            }
        }

        #region Benificiary information add
        public ActionResult ADDBENEFICIARY()
        {
            initpage();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PostDMRBenificiaryform(DMRBeneficiarySenderModel objval)
        {
            try
            {
                const string agentId = "2";
                var db = new DBContext();
                var Mem_IDval = long.Parse(CurrentMerchant.MEM_ID.ToString());
                var remitterid = Session["MerchantDMRId"].ToString();

                var PaymentValidation = MoneyTransferAPI.BeneficiaryRegistration(remitterid, objval.MobileNo, objval.Name, objval.IFSC, objval.Account);

                string errorcode = string.IsNullOrEmpty(PaymentValidation.statuscode.Value) ? PaymentValidation.statuscode.Value : PaymentValidation.statuscode.Value;//res.res_code;
                //if (PaymentValidation != "Unknown Error")
                //{
                //if (PaymentValidation.statuscode == "TXN")
                if (errorcode == "TXN")
                {
                    var ipat_id = PaymentValidation.data.remitter.id;
                    var Benificiary_id = PaymentValidation.data.beneficiary.id.Value;
                    var RemitterId = ipat_id.Value;

                    TBL_REMITTER_BENEFICIARY_INFO objbeneficiary = new TBL_REMITTER_BENEFICIARY_INFO()
                    {
                        BeneficiaryID = Benificiary_id.ToString(),
                        BeneficiaryName = objval.Name,
                        Mobile = objval.MobileNo,
                        Account = objval.Account,
                        Bank = "",
                        IFSC = objval.IFSC,
                        Status = 0,
                        IMPS = 0,
                        Last_success_Name = "",
                        Last_Sucess_IMPS = "",
                        RemitterID = remitterid,
                        MEM_ID = Mem_IDval,
                        IsActive = 0
                    };
                    db.TBL_REMITTER_BENEFICIARY_INFO.Add(objbeneficiary);
                    await db.SaveChangesAsync();
                    var msg = PaymentValidation.status.Value;
                    var msgcode = PaymentValidation.statuscode.Value;
                    return Json(new { remitterid = remitterid, beneficiaryid = objbeneficiary.BeneficiaryID, status = msg, msgcode = msgcode });
                    //return Json(new {remitterid= remitterid, beneficiaryid= objbeneficiary.BeneficiaryID, status = msg });
                    //return Json(new { remitterid = obj.TXNID, beneficiaryid = obj.REQUESTID, status = msg });
                }
                else
                {
                    return Json(new { remitterid = "", beneficiaryid = "", status = PaymentValidation.status.Value, msgcode = PaymentValidation.statuscode.Value });
                    ///return Json(PaymentValidation.status);
                }
                //}
                //else
                //{
                //    return Json(PaymentValidation);
                //}
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantDMRDashboard(Merchant), method:- PostDMRBenificiaryform(POST) Line No:- 210", ex);
                return Json(ex.InnerException.InnerException);
                throw ex;
                //ViewBag.Message = "Invalid Credential or Access Denied";
                //return Json("Error");
            }

        }

        [HttpPost]
        public JsonResult ValidateBeneficiary(string remitterID, string BeneficiaryID, string otp)
        {
            try
            {
                var BeneficiaryValidation = MoneyTransferAPI.BeneficiaryRegistrationValidate(remitterID, BeneficiaryID, otp);
                if (BeneficiaryValidation.statuscode == "TXN")
                {
                    return Json(BeneficiaryValidation.status.Value);
                }
                else
                {
                    return Json(BeneficiaryValidation.status.Value);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantDMRDashboard(Merchant), method:- ValidateBeneficiary(POST) Line No:- 236", ex);
                return Json("Error");
                throw ex;
            }
        }
        [HttpPost]
        public JsonResult ResendDMROTP(string remitterID, string BeneficiaryID)
        {
            try
            {
                var BeneficiaryValidation = MoneyTransferAPI.BeneficiaryRegistrationResendOTP(remitterID, BeneficiaryID);
                if (BeneficiaryValidation == "TXN")
                {
                    return Json(BeneficiaryValidation);
                }
                else
                {
                    return Json(BeneficiaryValidation);
                }
                //return Json("");
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantDMRDashboard(Merchant), method:- ResendDMROTP(POST) Line No:- 259", ex);
                return Json("Error");
                throw ex;
            }
        }
        #endregion

        #region ADD Benificiary Account
        public ActionResult AddBeneficiaryAccount()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult PostAddBeneficiaryAccount(DMRAccountVerification objval)
        {
            try
            {
                const string agentId = "2";
                var db = new DBContext();
                var AcccountValidation = MoneyTransferAPI.BeneficiaryAccountVerification(objval.RemitterMobile, objval.Account, objval.IFSC, agentId);
                if (AcccountValidation == "TXN")
                {
                    return Json(AcccountValidation);
                }
                else
                {
                    return Json(AcccountValidation);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantDMRDashboard(Merchant), method:- PostAddBeneficiaryAccount(POST) Line No:- 291", ex);
                return Json("Error");
                throw ex;
            }
            //return Json("");
        }
        #endregion

        #region Verification Beneficiary account 

        public ActionResult VERIFYBENEFICIARYACCOUNT()
        {
            return View();
        }
        [HttpPost]
        public JsonResult PostVerifyBeneficiaryAccount(BeneficiaryAccountVerification objval)
        {
            try
            {
                //const string agentId = "2";
                const string agentId = "395Y36706";
                var db = new DBContext();

                var remitterid = Session["MerchantDMRId"].ToString();

                var BeneficiaryValidation = MoneyTransferAPI.BeneficiaryAccountVerification(objval.RemitterMobileNo, objval.BankAccountNo, objval.IFSCCode, agentId);
                if (BeneficiaryValidation.statuscode == "TXN")
                {
                    var Verified = BeneficiaryValidation.data.verification_status.Value;
                    var ipay_id = BeneficiaryValidation.data.ipay_id.Value;
                    //var RemitterId = ipat_id.Value;
                    //TBL_API_RESPONSE_OUTPUT obj = new TBL_API_RESPONSE_OUTPUT()
                    //{
                    //    TXNID = RemitterId,
                    //    REQUESTID = Benificiary_id,
                    //    MOBILENO = objval.MobileNo,
                    //    STATUSID = 0,
                    //    DESCRIPTION = "Beneficiary Registration",
                    //    AMOUNT = 0,
                    //    BALANCE = 0,
                    //    DATE = System.DateTime.Now,
                    //    OPREFNO = "",
                    //    CREATEDATE = System.DateTime.Now,
                    //    MEM_ID = CurrentMerchant.MEM_ID,
                    //    STATUS = true,
                    //    RECHARGETYPE = "DMR Beneficiary Add"
                    //};
                    //db.TBL_API_RESPONSE_OUTPUT.Add(obj);
                    //db.SaveChanges();
                    var msg = BeneficiaryValidation.status.Value;
                    return Json(new { IPayId = ipay_id, Verified = Verified });
                }
                else
                {
                    //return Json(PaymentValidation);
                    //ViewBag.Message = "Invalid Credential or Access Denied";
                    return Json(BeneficiaryValidation.statuscode);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantDMRDashboard(Merchant), method:- PostVerifyBeneficiaryAccount(POST) Line No:- 352", ex);
                return Json(ex.InnerException.InnerException);
                throw ex;
            }
        }

        #endregion

        #region Beneficiary List 
        public ActionResult BeneficiaryAccountList()
        {
            return View();
        }
        public PartialViewResult IndexGrid()
        {
            var db = new DBContext();
            var remitterid = Session["MerchantDMRId"].ToString();
            var remtterMob = db.TBL_DMR_REMITTER_INFORMATION.FirstOrDefault(x => x.RemitterID == remitterid);
            var PaymentValidation = MoneyTransferAPI.RemitterDetails(remtterMob.MobileNo);
            if (PaymentValidation.statuscode == "TXN")
            {
                var limit = PaymentValidation.data.remitter_limit[0];
                var limitTotal = limit.limit.total;
                var beneficiarylist = PaymentValidation.data.beneficiary;
                foreach (var listitem in beneficiarylist)
                {
                    string beneid = listitem.id.Value;
                    var benelist = db.TBL_REMITTER_BENEFICIARY_INFO.Where(x => x.BeneficiaryID == beneid).FirstOrDefault();
                    if (benelist != null)
                    {
                        benelist.Bank = listitem.bank.Value;
                        db.Entry(benelist).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            //var bankdetails = db.TBL_SETTINGS_BANK_DETAILS.Where(x => x.ISDELETED == 0  && x.MEM_ID==MemberCurrentUser.MEM_ID).ToList();
            var bankdetails = db.TBL_REMITTER_BENEFICIARY_INFO.Where(x => x.IsActive == 0 && x.RemitterID == remitterid).ToList();
            return PartialView("IndexGrid", bankdetails);
        }
        private IGrid<TBL_REMITTER_BENEFICIARY_INFO> CreateExportableGrid()
        {
            try
            {
                var db = new DBContext();
                var remitterid = Session["MerchantDMRId"].ToString();
                var remtterMob = db.TBL_DMR_REMITTER_INFORMATION.FirstOrDefault(x => x.RemitterID == remitterid);
                var PaymentValidation = MoneyTransferAPI.RemitterDetails(remtterMob.MobileNo);
                if (PaymentValidation.statuscode == "TXN")
                {
                    var limit = PaymentValidation.data.remitter_limit[0];
                    var limitTotal = limit.limit.total;
                    var beneficiarylist = PaymentValidation.data.beneficiary;
                    foreach (var listitem in beneficiarylist)
                    {
                        string beneid = listitem.id.Value;
                        var benelist = db.TBL_REMITTER_BENEFICIARY_INFO.Where(x => x.BeneficiaryID == beneid).FirstOrDefault();
                        if (benelist != null)
                        {
                            benelist.Bank = listitem.bank.Value;
                            db.Entry(benelist).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                //var bankdetails = db.TBL_SETTINGS_BANK_DETAILS.Where(x => x.ISDELETED == 0  && x.MEM_ID==MemberCurrentUser.MEM_ID).ToList();
                var bankdetails = db.TBL_REMITTER_BENEFICIARY_INFO.Where(x => x.IsActive == 0 && x.RemitterID == remitterid).ToList();

                IGrid<TBL_REMITTER_BENEFICIARY_INFO> grid = new Grid<TBL_REMITTER_BENEFICIARY_INFO>(bankdetails);
                grid.ViewContext = new ViewContext { HttpContext = HttpContext };
                grid.Query = Request.QueryString;
                grid.Columns.Add(model => model.BeneficiaryID).Titled("BENEFICIARY ID").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.Bank).Titled("BANK").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.IFSC).Titled("IFSC").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.Mobile).Titled("MOBILE").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.BeneficiaryName).Titled("BENEFICIARY NAME").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.ID).Titled("").Encoded(false).Filterable(false).Sortable(false)
                    .RenderedAs(model => "<a href='javascript:void(0)' class='btn btn-danger btn-xs' onclick='DeActivateBeneficiary(" + model.ID + ");return 0;'>DELETE</a>");
                grid.Pager = new GridPager<TBL_REMITTER_BENEFICIARY_INFO>(grid);
                grid.Processors.Add(grid.Pager);
                grid.Pager.RowsPerPage = 6;

                //foreach (IGridColumn column in grid.Columns)
                //{
                //    column.Filter.IsEnabled = true;
                //    column.Sort.IsEnabled = true;
                //}

                return grid;
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantDMRDashboard(Merchant), method:- CreateExportableGrid(POST) Line No:- 421", ex);
                throw ex;
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteBeneficiary(string id)
        {
            try
            {
                var db = new DBContext();
                var Mem_IDval = long.Parse(CurrentMerchant.MEM_ID.ToString());
                var remitterid = Session["MerchantDMRId"].ToString();
                long benid = long.Parse(id);
                var benefiVal = db.TBL_REMITTER_BENEFICIARY_INFO.Where(x => x.ID == benid).FirstOrDefault();
                var PaymentValidation = MoneyTransferAPI.BeneficiaryDelete(benefiVal.RemitterID, benefiVal.BeneficiaryID);

                string errorcode = string.IsNullOrEmpty(PaymentValidation.statuscode.Value) ? PaymentValidation.statuscode.Value : PaymentValidation.statuscode.Value;//res.res_code;
                //if (PaymentValidation != "Unknown Error")
                //{
                //if (PaymentValidation.statuscode == "TXN")
                if (errorcode == "TXN")
                {
                    var ipat_id = benefiVal.RemitterID;
                    var Benificiary_id = benefiVal.BeneficiaryID;
                    var RemitterId = ipat_id;
                    var msg = PaymentValidation.status.Value;
                    var msgcode = PaymentValidation.statuscode.Value;
                    return Json(new { remitterid = remitterid, beneficiaryid = benefiVal.BeneficiaryID, status = msg, msgcode = msgcode, idval = id });
                    //return Json(new {remitterid= remitterid, beneficiaryid= objbeneficiary.BeneficiaryID, status = msg });  beneficiaryid = objbeneficiary.BeneficiaryID,
                    //return Json(new { remitterid = obj.TXNID, beneficiaryid = obj.REQUESTID, status = msg });
                }
                else
                {
                    return Json(new { remitterid = "", beneficiaryid = "", status = PaymentValidation.status.Value, msgcode = PaymentValidation.statuscode.Value });
                    ///return Json(PaymentValidation.status);
                }

            }
            catch (Exception ex)
            {
                return Json(ex.InnerException.InnerException.ToString());
                throw ex;
            }

        }

        [HttpPost]
        public JsonResult BeneficiaryAccountdeleteValidate(string remitterID, string BeneficiaryID, string otp, string Idval)
        {
            try
            {
                var BeneficiaryValidation = MoneyTransferAPI.BeneficiaryDeleteValidate(remitterID, BeneficiaryID, otp);
                if (BeneficiaryValidation.statuscode == "TXN")
                {
                    long idval = long.Parse(Idval);
                    var db = new DBContext();
                    var deletebeneficiaryaccount = db.TBL_REMITTER_BENEFICIARY_INFO.Where(x => x.ID == idval).FirstOrDefault();
                    deletebeneficiaryaccount.IsActive = 1;
                    db.Entry(deletebeneficiaryaccount).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Json(BeneficiaryValidation.status.Value);
                }
                else
                {
                    return Json(BeneficiaryValidation.status.Value);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantDMRDashboard(Merchant), method:- BeneficiaryAccountdeleteValidate(POST) Line No:- 492", ex);
                return Json("Error");
                throw ex;
            }
        }

        #endregion

        public ActionResult REMITTANCELIST()
        {
            return View();
        }

        #region Money remittance fund transfer
        public ActionResult MONEYREMITTANCE()
        {
            return View();
        }
        public PartialViewResult DisplayBeneficiaryList()
        {
            var db = new DBContext();
            var remitterid = Session["MerchantDMRId"].ToString();
            //var bankdetails = db.TBL_SETTINGS_BANK_DETAILS.Where(x => x.ISDELETED == 0  && x.MEM_ID==MemberCurrentUser.MEM_ID).ToList();
            var bankdetails = db.TBL_REMITTER_BENEFICIARY_INFO.Where(x => x.IsActive == 0 && x.RemitterID == remitterid).ToList();
            return PartialView("DisplayBeneficiaryList", bankdetails);
            //return PartialView(DisplayBeneficiaryAccountDetails());
        }
        private IGrid<TBL_REMITTER_BENEFICIARY_INFO> DisplayBeneficiaryAccountDetails()
        {
            try
            {
                var db = new DBContext();
                var remitterid = Session["MerchantDMRId"].ToString();
                //var bankdetails = db.TBL_SETTINGS_BANK_DETAILS.Where(x => x.ISDELETED == 0  && x.MEM_ID==MemberCurrentUser.MEM_ID).ToList();
                var bankdetails = db.TBL_REMITTER_BENEFICIARY_INFO.Where(x => x.IsActive == 0 && x.RemitterID == remitterid).ToList();

                IGrid<TBL_REMITTER_BENEFICIARY_INFO> grid = new Grid<TBL_REMITTER_BENEFICIARY_INFO>(bankdetails);
                grid.ViewContext = new ViewContext { HttpContext = HttpContext };
                grid.Query = Request.QueryString;
                grid.Columns.Add(model => model.BeneficiaryID).Titled("BENEFICIARY ID").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.BeneficiaryName).Titled("BENEFICIARY NAME").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.Bank).Titled("BANK").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.IFSC).Titled("IFSC").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.Account).Titled("ACCOUNT").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.Mobile).Titled("MOBILE").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.ID).Titled("").Encoded(false).Filterable(false).Sortable(false)
                .RenderedAs(model => "<a href='" + @Url.Action("FundTransfer", "MerchantDMRDashboard", new { area = "Merchant", TransID = Encrypt.EncryptMe(model.ID.ToString()) }) + "' class='btn btn-success'>Fund Transfer</a>");
                //grid.Columns.Add(model => model.ID).Titled("").Encoded(false).Filterable(false).Sortable(false)
                //    .RenderedAs(model => "<a href='javascript:void(0)' class='btn btn-denger btn-xs'>Fund Transfer</a>");
                grid.Pager = new GridPager<TBL_REMITTER_BENEFICIARY_INFO>(grid);
                grid.Processors.Add(grid.Pager);
                grid.Pager.RowsPerPage = 6;

                //foreach (IGridColumn column in grid.Columns)
                //{
                //    column.Filter.IsEnabled = true;
                //    column.Sort.IsEnabled = true;
                //}

                return grid;
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantDMRDashboard(Merchant), method:- DisplayBeneficiaryAccountDetails(POST) Line No:- 550", ex);
                throw ex;
            }

        }
        public async Task<ActionResult> FundTransfer(string TransID)
        {
            try
            {
                string decrptSlId = Decrypt.DecryptMe(TransID);
                long ValID = long.Parse(decrptSlId);
                var db = new DBContext();
                var beneficiarydetails = await db.TBL_REMITTER_BENEFICIARY_INFO.FirstOrDefaultAsync(x => x.ID == ValID);
                MoneyTransferModelView objval = new MoneyTransferModelView()
                {
                    RemitterId = beneficiarydetails.RemitterID,
                    BeneficiaryID = beneficiarydetails.BeneficiaryID,
                    RemitterMobileNo = beneficiarydetails.Mobile,
                    BeneficiaryName = beneficiarydetails.BeneficiaryName,
                    BeneficiaryAccount = beneficiarydetails.Account,
                    BeneficiaryBankName = beneficiarydetails.BeneficiaryName,
                    BeneficiaryIFSC = beneficiarydetails.IFSC
                };
                return View(objval);
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantDMRDashboard(Merchant), method:- FundTransfer(GET) Line No:- 577", ex);
                throw ex;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> FundTransfer(MoneyTransferModelView objval)
        {
            try
            {
                const string agentId = "395Y36706";
                var db = new DBContext();
                var Mem_IDval = long.Parse(CurrentMerchant.MEM_ID.ToString());
                var remitterid = Session["MerchantDMRId"].ToString();
                if (objval.Amount > 0)
                {
                    var remitterInfo = (from remitt in db.TBL_REMITTER_BENEFICIARY_INFO join dmrremitter in db.TBL_DMR_REMITTER_INFORMATION on remitt.RemitterID equals dmrremitter.RemitterID
                                        where remitt.MEM_ID == CurrentMerchant.MEM_ID
                                        select new
                                        {
                                            remitterMobile = remitt.Mobile
                                        }).FirstOrDefault();

                    ////var PaymentValidation1 = MoneyTransferAPI.BeneficiaryAccountVerification(objval.RemitterMobileNo, objval.BeneficiaryAccount, objval.BeneficiaryIFSC, agentId);
                    var PaymentValidation = MoneyTransferAPI.FundTransfer(objval.RemitterMobileNo, objval.BeneficiaryID, agentId, objval.Amount.ToString(), objval.PaymentMode);
                    string errorcode = string.IsNullOrEmpty(PaymentValidation.statuscode.Value) ? PaymentValidation.statuscode.Value : PaymentValidation.statuscode.Value;//res.res_code;                                    
                    //string errorcode = "TXN";
                    if (errorcode == "TXN")
                    {
                        var ipat_id = PaymentValidation.data.ipay_id;
                        var ref_no = PaymentValidation.data.ref_no;
                        TimeSpan timeval = new TimeSpan();
                        var opr_id = PaymentValidation.data.opr_id;
                        var Name = PaymentValidation.data.name;
                        var OpeningAmt = PaymentValidation.data.opening_bal;
                        var amount = PaymentValidation.data.amount;
                        var charged_amt = PaymentValidation.data.charged_amt;
                        var locked_amt = PaymentValidation.data.locked_amt;
                        var bank_alias = PaymentValidation.data.bank_alias;
                        var msg = PaymentValidation.status.Value;
                        var msgcode = PaymentValidation.statuscode.Value;
                        TBL_DMR_FUND_TRANSFER_DETAILS objfund = new TBL_DMR_FUND_TRANSFER_DETAILS()
                        {
                            IPAY_ID = ipat_id,
                            REF_NO = ref_no,
                            OPR_ID = opr_id,
                            NAME = Name,
                            OPENING_BAL = Convert.ToDecimal(OpeningAmt),
                            AMOUNT = Convert.ToDecimal(amount),
                            CHARGED_AMT = Convert.ToDecimal(charged_amt),
                            LOCKED_AMT = Convert.ToDecimal(locked_amt),
                            BANK_ALIAS = bank_alias,
                            STATUS = msg,
                            STATUSCODE = msgcode,
                            TXNDATE = DateTime.Now,
                            BANKREFNO = "",
                            REMARKS = "",
                            MEM_ID = Mem_IDval,
                            VERIFICATIONSTATUS = "",
                            REFUNF_ALLOWED = 0,
                            REMITTER_ID = remitterid
                        };
                        db.TBL_DMR_FUND_TRANSFER_DETAILS.Add(objfund);
                        await db.SaveChangesAsync();
                        var checkPaymentStatus = MoneyTransferAPI.FundTransferStatus(ipat_id.Value);
                        string ipayval = ipat_id.Value;
                        string checktransfer = string.IsNullOrEmpty(checkPaymentStatus.statuscode.Value) ? checkPaymentStatus.statuscode.Value : checkPaymentStatus.statuscode.Value;//res.res_code;                
                        if (checktransfer == "TXN")
                        {
                            var updatetransstatus = await db.TBL_DMR_FUND_TRANSFER_DETAILS.Where(x => x.IPAY_ID == ipayval).FirstOrDefaultAsync();
                            if (updatetransstatus != null)
                            {
                                updatetransstatus.VERIFICATIONSTATUS = checkPaymentStatus.status.Value;
                                updatetransstatus.REQ_DATE = Convert.ToDateTime(checkPaymentStatus.data.req_dt.Value);
                                updatetransstatus.NAME = checkPaymentStatus.data.beneficiary_name.Value;
                                db.Entry(updatetransstatus).State = System.Data.Entity.EntityState.Modified;
                                await db.SaveChangesAsync();
                                CommissionDistributionHelper objComm = new CommissionDistributionHelper();

                                string valueDMRComm = objComm.DistributeCommission(long.Parse(Mem_IDval.ToString()), "DMR", decimal.Parse(objval.Amount.ToString()), decimal.Parse(charged_amt.ToString()), decimal.Parse(OpeningAmt.ToString()), "DMI", "DMR");

                            }
                        }
                        ViewBag.message = checkPaymentStatus.status.Value;

                        return RedirectToAction("FundTransferSuccess", "MerchantDMRDashboard", new { area = "Merchant", TransID = Encrypt.EncryptMe(ipayval.ToString()) });
                        //return RedirectToAction("FundTransferSuccess", "MerchantDMRDashboard", new { area = "Merchant", TransID = "12345" });
                    }
                    else
                    {
                        ViewBag.message = PaymentValidation.status;
                        return View();
                    }
                }
                else
                {
                    ViewBag.message = "Enter Some Amount";
                    return View();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantDMRDashboard(Merchant), method:- FundTransfer(POST) Line No:- 674", ex);
                throw ex;
            }
        }

        public async Task<ActionResult> FundTransferSuccess(string TransID)
        {
            try
            {
                if (TransID != "" && TransID != null)
                {
                    string decrptSlId = Decrypt.DecryptMe(TransID);
                    //long ValID = long.Parse(decrptSlId);
                    var db = new DBContext();
                    var translist = await db.TBL_DMR_FUND_TRANSFER_DETAILS.Where(x => x.IPAY_ID == TransID).FirstOrDefaultAsync();
                    if (translist != null)
                    {
                        ViewBag.TransactionID = translist.IPAY_ID;
                        ViewBag.TransactionDate = translist.REQ_DATE;
                        ViewBag.BeneficiaryName = translist.NAME;
                        ViewBag.QrderId = translist.OPR_ID;
                        ViewBag.Amount = translist.AMOUNT;
                        ViewBag.ChargeAmt = translist.CHARGED_AMT;
                        ViewBag.status = translist.VERIFICATIONSTATUS;
                        return View();
                    }
                    else
                    {
                        Response.Redirect(Url.Action("DMRInformation", "MerchantDMRDashboard", new { area = "Merchant" }));
                        return View();
                    }
                }
                else
                {
                    Response.Redirect(Url.Action("DMRInformation", "MerchantDMRDashboard", new { area = "Merchant" }));
                    return View();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantDMRDashboard(Merchant), method:- FundTransferSuccess(GEt) Line No:- 714", ex);
                throw;
            }

        }

        #endregion

        #region Beneficiary Account Verify
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult VerifyBeneficiaryAccount(string id)
        {
            try
            {
                const string agentId = "395Y36706";
                var db = new DBContext();
                var Mem_IDval = long.Parse(CurrentMerchant.MEM_ID.ToString());
                var remitterid = Session["MerchantDMRId"].ToString();
                long benid = long.Parse(id);
                var benefiVal = db.TBL_REMITTER_BENEFICIARY_INFO.Where(x => x.ID == benid).FirstOrDefault();

                var getremitterinfo = (from benef in db.TBL_REMITTER_BENEFICIARY_INFO
                                       join rem in db.TBL_DMR_REMITTER_INFORMATION on benef.RemitterID equals rem.RemitterID
                                       select new
                                       {
                                           mobileno = rem.MobileNo
                                       }).FirstOrDefault();

                var PaymentValidation = MoneyTransferAPI.BeneficiaryAccountVerification(getremitterinfo.mobileno, benefiVal.Account.ToString(), benefiVal.IFSC.ToString(), agentId);
                //var PaymentValidation = MoneyTransferAPI.BeneficiaryDelete(benefiVal.RemitterID, benefiVal.BeneficiaryID);

                string errorcode = string.IsNullOrEmpty(PaymentValidation.statuscode.Value) ? PaymentValidation.statuscode.Value : PaymentValidation.statuscode.Value;//res.res_code;
                //if (PaymentValidation != "Unknown Error")
                //{
                //if (PaymentValidation.statuscode == "TXN")
                if (errorcode == "TXN")
                {
                    var ipat_id = benefiVal.RemitterID;
                    var Benificiary_id = benefiVal.BeneficiaryID;
                    var RemitterId = ipat_id;
                    var msg = PaymentValidation.status.Value;
                    var msgcode = PaymentValidation.statuscode.Value;
                    var Verification_Status = PaymentValidation.data.verification_status.Value;
                    var bankrefno = PaymentValidation.data.bankrefno.Value;
                    var ipay_id = PaymentValidation.data.ipay_id.Value;

                    var updatebenefstatus = db.TBL_REMITTER_BENEFICIARY_INFO.Where(x => x.ID == benid).FirstOrDefault();
                    if (updatebenefstatus != null)
                    {
                        updatebenefstatus.Verification_Status = Verification_Status;
                        updatebenefstatus.BankRefNo = bankrefno;
                        updatebenefstatus.Ipay_Id = ipay_id;
                        db.Entry(updatebenefstatus).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }

                    decimal Trans_Amt = 1;
                    #region Retailer Commission                        
                    var membtype = (from mm in db.TBL_MASTER_MEMBER
                                    join
                                        rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                    where mm.MEM_ID == CurrentMerchant.MEM_ID
                                    select new
                                    {
                                        RoleId = mm.MEMBER_ROLE,
                                        roleName = rm.ROLE_NAME,
                                        Amount = mm.BALANCE
                                    }).FirstOrDefault();
                    var tbl_account = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == CurrentMerchant.MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                    if (tbl_account != null)
                    {
                        decimal ClosingAmt = tbl_account.CLOSING;
                        decimal SubAmt = ClosingAmt - Trans_Amt;
                        TBL_ACCOUNTS objaccnt = new TBL_ACCOUNTS()
                        {
                            API_ID = 0,
                            MEM_ID = CurrentMerchant.MEM_ID,
                            MEMBER_TYPE = membtype.roleName,
                            TRANSACTION_TYPE = "Verification",
                            TRANSACTION_DATE = System.DateTime.Now,
                            TRANSACTION_TIME = DateTime.Now,
                            DR_CR = "DR",
                            AMOUNT = Trans_Amt,
                            NARRATION = "Account Verification",
                            OPENING = ClosingAmt,
                            CLOSING = SubAmt,
                            REC_NO = 0,
                            COMM_AMT = 0
                        };
                        db.TBL_ACCOUNTS.Add(objaccnt);
                        db.SaveChanges();
                    }
                    #endregion
                    //return Json(new { remitterid = remitterid, beneficiaryid = benefiVal.BeneficiaryID, status = msg, msgcode = msgcode, idval = id });
                    return Json(new { Result = "true" });
                }
                else
                {
                    return Json(new { Result = "fail" });
                    //return Json(new { remitterid = "", beneficiaryid = "", status = PaymentValidation.status.Value, msgcode = PaymentValidation.statuscode.Value });
                    ///return Json(PaymentValidation.status);
                }

            }
            catch (Exception ex)
            {
                return Json(ex.InnerException.InnerException.ToString());
                throw ex;
            }

        }

        #endregion
    }
}