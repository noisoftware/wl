using System;
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
using System.Threading.Tasks;
using System.Data.Entity;
using log4net;
using System.Web.Security;
using System.IO;

namespace WHITELABEL.Web.Areas.Merchant.Controllers
{
    [Authorize]
    public class MerchantRechargeServiceController : MerchantBaseController
    {
        // GET: Merchant/MerchantRechargeService
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
        
        public ActionResult Index()
        {
            if (Session["MerchantUserId"] != null)
            {
               
                var db = new DBContext();
                initpage();
                var checkList = (from user in db.TBL_WHITELABLE_SERVICE
                                 join serv in db.TBL_SETTINGS_SERVICES_MASTER on user.SERVICE_ID equals serv.SLN where user.MEMBER_ID==CurrentMerchant.MEM_ID
                                 select new ServiceList
                                 {
                                     ServiceName = serv.SERVICE_NAME,
                                     ServiceStatus = user.ACTIVE_SERVICE
                                 }).ToList();
                //ViewBag.ActiveServiceList = checkList;
                var checkoutlet = db.TBL_MERCHANT_OUTLET_INFORMATION.Where(x => x.MEM_ID == CurrentMerchant.MEM_ID).Select(c=>c.OUTLETID).FirstOrDefault();
                if (checkoutlet != null)
                {
                    ViewBag.Outletcheck = checkoutlet;
                }
                else
                {
                    ViewBag.Outletcheck = "";
                }
                var OperatorList = db.TBL_SERVICE_PROVIDERS.Where(x => x.TYPE == "PREPAID" || x.TYPE == "POSTPAID").OrderBy(c=>c.TYPE).ToList();
                ViewBag.operatorList = OperatorList;
                var ElectricityOperator= db.TBL_SERVICE_PROVIDERS.Where(x => x.TYPE == "ELECTRICITY").OrderBy(c => c.TYPE).ToList();
                ViewBag.ElectricityOperator = ElectricityOperator;
                var WaterOperator = db.TBL_SERVICE_PROVIDERS.Where(x => x.TYPE == "WATER").OrderBy(c => c.TYPE).ToList();
                ViewBag.WaterOperator = WaterOperator;
                Session["MerchantDMRId"] = null;
                Session.Remove("MerchantDMRId");
                return View(checkList);
            }
            else
            {
                FormsAuthentication.SignOut();
                Session["MerchantUserId"] = null;
                Session["MerchantUserName"] = null;
                Session.Clear();
                Session.Remove("MerchantUserId");
                Session.Remove("MerchantUserName");
                return RedirectToAction("Index", "MerchantLogin", new { area = "Merchant" });
            }
        }

        // Mobile recharge section
        #region Mobile Recharge
        public ActionResult MobileRecharge()
        {
            if (Session["MerchantUserId"] != null)
            {
                initpage();
                //CommissionDistributionHelper objComm = new CommissionDistributionHelper();
                ////string valueComm = objComm.DistributeCommission(CurrentMerchant.MEM_ID, "MOBILE", 10, 9.91M, 15000, "ATP","Prepaid");
                ////string valueDMRComm = objComm.DistributeCommission(CurrentMerchant.MEM_ID, "DMR", 16000, 9.91M, 15000, "DMI", "DMR");
                // DistributeCommission();
                return View();
            }
            else
            {
                return RedirectToAction("Index", "MerchantLogin", new { area = "Merchant" });
            }
            
        }        
        public ActionResult GetOperatorName(string query)
        {
            return Json(_GetOperator(query), JsonRequestBehavior.AllowGet);
        }
        private List<Autocomplete> _GetOperator(string query)
        {
            List<Autocomplete> people = new List<Autocomplete>();
            var db = new DBContext();
            try
            {
                var results = (from p in db.TBL_OPERATOR_MASTER
                               where (p.OPERATORNAME).Contains(query)
                               orderby p.OPERATORNAME
                               select p).ToList();
                foreach (var r in results)
                {
                    // create objects
                    Autocomplete Username = new Autocomplete();
                    //Username.FromUser = string.Format("{0} {1}", r.UName);
                    Username.Name = (r.OPERATORNAME);
                    Username.Id = r.PRODUCTID;

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

        public ActionResult MobileRechargeCallBack()
        {
            if (Session["MerchantUserId"] != null)
            {
               
                return View("~/Areas/Merchant/Views/MerchantRechargeService/CallBack.cshtml");
            }
            else
            {
                return RedirectToAction("Index", "MerchantLogin", new { area = "Merchant" });
            }

        }

        private void GenerateLog(string message)
        {
            string msg = "";
            msg+= string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            msg += Environment.NewLine;
            msg += "-----------------------------------------------------------";
            msg += message;
            msg += Environment.NewLine;
            msg += "-----------------------------------------------------------";
            string path = Server.MapPath("~/ErrorLog/ErrorLog.txt");
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(msg);
                writer.Close();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PostMobileRecharge(MobileRechargeModel objval)
        {
            try
            {
                var db = new DBContext();
                var check_walletAmt=db.TBL_ACCOUNTS.Where(x=>x.MEM_ID==CurrentMerchant.MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                if (objval.RechargeAmt <= check_walletAmt.CLOSING)
                {
                    string agentId = Convert.ToString(Session["MerchantUserId"]); 
                    //const string agentId = "12348";
                    string OperatorName = Request.Form["OperatorName"];
                    string operatorId = Request.Form["OperatorId"];
                    long merchantid = 0;
                    long.TryParse(Session["MerchantUserId"].ToString(), out merchantid);
                    //var remitterid = Session["MerchantDMRId"].ToString();
                    var authcode = await db.TBL_API_SETTING.Where(x => x.NAME == "API_AUTHCODE").FirstOrDefaultAsync();
                    //**********************Start apiscript recharge bill api integration************************
                    string errorcode = "";

                    var AccountBalanceResponse = PaymentAPI.GetAccountBalance();
                    GenerateLog(Convert.ToString(AccountBalanceResponse));
                    errorcode = AccountBalanceResponse.message;
                    if (AccountBalanceResponse.message == "Success")
                    {
                        decimal AccountBalance = Convert.ToDecimal(AccountBalanceResponse.balance == "" ? "0" : AccountBalanceResponse.balance);
                        if (AccountBalance != 0)
                        {

                            var RechargeApiResponse = PaymentAPI.RechargeApiRequest(OperatorName, objval.ContactNo, objval.RechargeAmt.ToString(), agentId);
                            GenerateLog(Convert.ToString(RechargeApiResponse));
                            errorcode = RechargeApiResponse.message;

                            if (RechargeApiResponse.message == "Your recharge request is accepted.")
                            {
                                string status = RechargeApiResponse.recharge_status;
                                var ipat_id = "";
                                decimal trans_amt = decimal.Parse(Convert.ToString(RechargeApiResponse.amount));
                                decimal Charged_Amt = decimal.Parse(Convert.ToString(RechargeApiResponse.amount));
                                //decimal Opening_Balance = check_walletAmt.CLOSING;
                                decimal Opening_Balance = 0;
                                string getStatus = RechargeApiResponse.recharge_status == "Pending" ? "SUCCESS" : RechargeApiResponse.recharge_status;
                                string getmessage = RechargeApiResponse.message == "Your recharge request is accepted." ? "Transaction Successful" : RechargeApiResponse.message;

                                DateTime datevalue = Convert.ToDateTime(RechargeApiResponse.recharge_datetime);
                                TBL_INSTANTPAY_RECHARGE_RESPONSE insta = new TBL_INSTANTPAY_RECHARGE_RESPONSE()
                                {
                                    Ipay_Id = ipat_id,
                                    AgentId = RechargeApiResponse.client_id,
                                    Opr_Id = RechargeApiResponse.recharge_id,
                                    AccountNo = RechargeApiResponse.number,
                                    Sp_Key = "",
                                    Trans_Amt = decimal.Parse(trans_amt.ToString()),
                                    Charged_Amt = decimal.Parse(Charged_Amt.ToString()),
                                    Opening_Balance = decimal.Parse(Opening_Balance.ToString()),
                                    DateVal = System.DateTime.Now,
                                    //Status = getStatus,
                                    Status = RechargeApiResponse.recharge_status,
                                    Res_Code = "",
                                    //res_msg = getmessage,
                                    res_msg = RechargeApiResponse.message,
                                    Mem_ID = merchantid,
                                    RechargeType = objval.PrepaidRecharge
                                };
                                db.TBL_INSTANTPAY_RECHARGE_RESPONSE.Add(insta);
                                await db.SaveChangesAsync();

                                string recharge_id = Convert.ToString(RechargeApiResponse.recharge_id);
                                var TransactionStatusResponse = PaymentAPI.TransactionStatus(recharge_id);
                                GenerateLog(Convert.ToString(TransactionStatusResponse));
                                errorcode = TransactionStatusResponse.message;
                               
                                if (TransactionStatusResponse.message == "Record found")
                                {
                                    // string statuscheck = TransactionStatusResponse.recharge_status;
                                    // string outputval = TransactionStatusResponse.message;
                                    var rechargestatuscheck = await db.TBL_INSTANTPAY_RECHARGE_RESPONSE.Where(x => x.Opr_Id == recharge_id).FirstOrDefaultAsync();
                                    rechargestatuscheck.Status = getStatus;
                                    rechargestatuscheck.res_msg = getmessage;
                                    db.Entry(rechargestatuscheck).State = System.Data.Entity.EntityState.Modified;
                                    await db.SaveChangesAsync();

                                    CommissionDistributionHelper objComm = new CommissionDistributionHelper();
                                    string valueComm = objComm.DistributeCommission(merchantid, "MOBILE", objval.RechargeAmt, Charged_Amt, Opening_Balance, operatorId, "Mobile Recharge");

                                    return Json(getmessage);
                                    //return Json(errorcode);
                                }
                                //CommissionDistributionHelper objComm = new CommissionDistributionHelper();
                                //string valueComm = objComm.DistributeCommission(merchantid, "MOBILE", objval.RechargeAmt, Charged_Amt, Opening_Balance, operatorId, "Mobile Recharge");

                                //return Json(getmessage);
                                return Json("Recharge Pending");
                            }
                            else
                            {
                                return Json(errorcode);
                            }


                            // return Json("");
                        }
                        else
                        {
                            //errorcode = "Insufficient Balance";
                            errorcode = "Something problem. Please contact service provider.";
                            return Json(errorcode);
                        }
                    }
                    else
                    {
                        return Json(errorcode);
                    }

                    //CommissionDistributionHelper objComm = new CommissionDistributionHelper();
                    //string valueComm = objComm.DistributeCommission(merchantid, "MOBILE", 1, 1, 0, operatorId, "Mobile Recharge");

                    //return Json("");


                    //**********************End apiscript recharge bill api integration************************
                    //var PaymentValidation = PaymentAPI.Validation(agentId, objval.RechargeAmt.ToString(), operatorId, objval.ContactNo);
                    //if (PaymentValidation == "TXN")
                    //{
                    //    var Recharge = PaymentAPI.Payment(agentId, objval.RechargeAmt.ToString(), operatorId, objval.ContactNo);
                    //    string errorcode = string.IsNullOrEmpty(Recharge.res_code.Value) ? Recharge.ipay_errordesc.Value : Recharge.res_code.Value;//res.res_code;
                    //    if (errorcode == "TXN")
                    //    {
                    //        string status = Recharge.status;
                    //        var ipat_id = Recharge.ipay_id.Value;
                    //        decimal trans_amt = decimal.Parse(Convert.ToString(Recharge.trans_amt.Value));
                    //        decimal Charged_Amt = decimal.Parse(Convert.ToString(Recharge.charged_amt.Value));
                    //        decimal Opening_Balance = decimal.Parse(Convert.ToString(Recharge.opening_bal.Value));
                    //        DateTime datevalue = Convert.ToDateTime(Recharge.datetime.Value);
                    //        TBL_INSTANTPAY_RECHARGE_RESPONSE insta = new TBL_INSTANTPAY_RECHARGE_RESPONSE()
                    //        {
                    //            Ipay_Id = ipat_id,
                    //            AgentId = Recharge.agent_id.Value,
                    //            Opr_Id = Recharge.opr_id.Value,
                    //            AccountNo = Recharge.account_no.Value,
                    //            Sp_Key = Recharge.sp_key.Value,
                    //            Trans_Amt = decimal.Parse(trans_amt.ToString()),
                    //            Charged_Amt = decimal.Parse(Charged_Amt.ToString()),
                    //            Opening_Balance = decimal.Parse(Opening_Balance.ToString()),
                    //            DateVal = System.DateTime.Now,
                    //            Status = Recharge.status.Value,
                    //            Res_Code = Recharge.res_code.Value,
                    //            res_msg = Recharge.res_msg.Value,
                    //            Mem_ID = merchantid,
                    //            RechargeType = objval.PrepaidRecharge
                    //        };
                    //        db.TBL_INSTANTPAY_RECHARGE_RESPONSE.Add(insta);
                    //        await db.SaveChangesAsync();
                    //        var checksts = PaymentAPI.StatusCheck(agentId);
                    //        string checkval = checksts.res_code;//res.res_code;
                    //        string statuscheck = checksts.status;
                    //        string outputval = checksts.res_msg;
                    //        string ipayidval = checksts.ipay_id;
                    //        var rechargestatuscheck = await db.TBL_INSTANTPAY_RECHARGE_RESPONSE.Where(x => x.Ipay_Id == ipayidval).FirstOrDefaultAsync();
                    //        rechargestatuscheck.Status = statuscheck;
                    //        db.Entry(rechargestatuscheck).State = System.Data.Entity.EntityState.Modified;
                    //        await db.SaveChangesAsync();
                    //        CommissionDistributionHelper objComm = new CommissionDistributionHelper();
                    //        string valueComm = objComm.DistributeCommission(merchantid, "MOBILE", objval.RechargeAmt, Charged_Amt, Opening_Balance, operatorId, "Mobile Recharge");

                    //        return Json(outputval);
                    //    }
                    //    else
                    //    {
                    //        //Session["msgCheck"] = Recharge;
                    //        return Json(errorcode);
                    //    }
                    //}
                    //else
                    //{
                    //    return Json(PaymentValidation);
                    //}
                }
                else
                {
                    var msg = "Can't procceed with transaction.You don't have sufficient balance.";
                    return Json(msg);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- PostMobileRecharge(POST) Line No:- 230", ex);
                throw ex;
            }
        }
        [HttpPost]
        public async Task<JsonResult> AutoComplete(string prefix,string OperatorType)
        {
            try
            {
                var db = new DBContext();
                var OperatorValue = await (from oper in db.TBL_SERVICE_PROVIDERS
                                     where oper.SERVICE_NAME.StartsWith(prefix) && oper.TYPE == OperatorType
                                     select new
                                     {
                                         //label = oper.SERVICE_NAME + "-" + oper.RECHTYPE,
                                         label = oper.SERVICE_NAME,
                                         val = oper.SERVICE_KEY
                                     }).ToListAsync();
                return Json(OperatorValue);
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- AutoComplete(POST) Line No:- 252", ex);
                throw ex;
            }

            //var db = new DBContext();
            //var OperatorValue = (from oper in db.TBL_OPERATOR_MASTER
            //                 where oper.OPERATORNAME.StartsWith(prefix) && oper.OPERATORTYPE== OperatorType
            //                     select new
            //                {
            //                    label = oper.OPERATORNAME +"-"+ oper.RECHTYPE,
            //                    val = oper.PRODUCTID
            //               }).ToList();

           
        }
        //End Mobile recharge section      
        #endregion
        



        // DTH Recharge    
        #region DTH Recharge                   
        [HttpPost]
        public async Task<JsonResult> AutoDTHRechargeComplete(string prefix)
        {
            try
            {
                var db = new DBContext();
                var OperatorValue = await (from oper in db.TBL_SERVICE_PROVIDERS
                                     where oper.SERVICE_NAME.StartsWith(prefix) && oper.TYPE == "DTH"
                                     select new
                                     {
                                         label = oper.SERVICE_NAME,
                                         val = oper.SERVICE_KEY
                                         //val = oper.SERVICE_NAME
                                     }).ToListAsync();

                return Json(OperatorValue);
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- AutoDTHRechargeComplete(POST) Line No:- 293", ex);
                throw ex;
            }
            //var db = new DBContext();
            //var OperatorValue = (from oper in db.TBL_OPERATOR_MASTER
            //                     where oper.OPERATORNAME.StartsWith(prefix) && oper.OPERATORTYPE == "DTH"
            //                     select new
            //                     {
            //                         label = oper.OPERATORNAME + "-" + oper.RECHTYPE,
            //                         val = oper.PRODUCTID
            //                     }).ToList();

            //return Json(OperatorValue);
        }
        public ActionResult DTHRecharge()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult POSTDTHRecharge(DTHRechargeModel objval)
        public async Task<JsonResult> POSTDTHRecharge(MobileRechargeModel objval)
        {

            try
            {
                //string OperatorName = Request.Form["DTHOperatorName"];
                //    string operatorId = Request.Form["DTHOperatorId"];
                //const string agentId = "2"; 
                var db = new DBContext();
                var check_walletAmt = db.TBL_ACCOUNTS.Where(x => x.MEM_ID == CurrentMerchant.MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                if (objval.RechargeAmt <= check_walletAmt.CLOSING)
                {
                    string agentId = Convert.ToString(Session["MerchantUserId"]);
                    //const string agentId = "395Y36706";
                    string OperatorName = Request.Form["DTHOperatorName"];
                    string operatorId = Request.Form["DTHOperatorId"];
                    long merchantid = 0;
                    long.TryParse(Session["MerchantUserId"].ToString(), out merchantid);

                    var authcode = await db.TBL_API_SETTING.Where(x => x.NAME == "API_AUTHCODE").FirstOrDefaultAsync();
                    //**********************Start apiscript recharge bill api integration************************
                    string errorcode = "";

                    //var AccountBalanceResponse = PaymentAPI.GetAccountBalance();
                    //errorcode = AccountBalanceResponse.message;
                    //if (AccountBalanceResponse.message == "Success")
                    //{
                    //    decimal AccountBalance = Convert.ToDecimal(AccountBalanceResponse.balance==""?"0": AccountBalanceResponse.balance);
                    //    if (AccountBalance !=0)
                    //    {


                    var RechargeApiResponse = PaymentAPI.DthRechargeApiRequest(OperatorName, objval.ContactNo, objval.RechargeAmt.ToString(), agentId);
                    GenerateLog(Convert.ToString(RechargeApiResponse));
                    errorcode = RechargeApiResponse.message;
                    if (RechargeApiResponse.message == "Your recharge request is accepted.")
                    {
                        string status = RechargeApiResponse.recharge_status;
                        var ipat_id = "";
                        decimal trans_amt = decimal.Parse(Convert.ToString(RechargeApiResponse.amount));
                        decimal Charged_Amt = decimal.Parse(Convert.ToString(RechargeApiResponse.amount));
                        //decimal Opening_Balance = check_walletAmt.CLOSING;
                        decimal Opening_Balance = 0;
                        string getStatus = RechargeApiResponse.recharge_status == "Pending" ? "SUCCESS" : RechargeApiResponse.recharge_status;
                        string getmessage = RechargeApiResponse.message == "Your recharge request is accepted." ? "Transaction Successful" : RechargeApiResponse.message;

                        DateTime datevalue = Convert.ToDateTime(RechargeApiResponse.recharge_datetime);
                        TBL_INSTANTPAY_RECHARGE_RESPONSE insta = new TBL_INSTANTPAY_RECHARGE_RESPONSE()
                        {
                            Ipay_Id = ipat_id,
                            AgentId = RechargeApiResponse.client_id,
                            Opr_Id = RechargeApiResponse.recharge_id,
                            AccountNo = RechargeApiResponse.number,
                            Sp_Key = "",
                            Trans_Amt = decimal.Parse(trans_amt.ToString()),
                            Charged_Amt = decimal.Parse(Charged_Amt.ToString()),
                            Opening_Balance = decimal.Parse(Opening_Balance.ToString()),
                            DateVal = System.DateTime.Now,
                            Status = RechargeApiResponse.recharge_status,
                            Res_Code = "",
                            res_msg = RechargeApiResponse.message,
                            Mem_ID = merchantid,
                            RechargeType = objval.PrepaidRecharge
                        };
                        db.TBL_INSTANTPAY_RECHARGE_RESPONSE.Add(insta);
                        await db.SaveChangesAsync();

                        string recharge_id = Convert.ToString(RechargeApiResponse.recharge_id);
                        var TransactionStatusResponse = PaymentAPI.TransactionStatus(recharge_id);
                        GenerateLog(Convert.ToString(TransactionStatusResponse));
                        errorcode = TransactionStatusResponse.message;
                        if (TransactionStatusResponse.message == "Record found")
                        {
                            //string statuscheck = TransactionStatusResponse.recharge_status;
                            //string outputval = TransactionStatusResponse.message;
                            var rechargestatuscheck = await db.TBL_INSTANTPAY_RECHARGE_RESPONSE.Where(x => x.Opr_Id == recharge_id).FirstOrDefaultAsync();
                            rechargestatuscheck.Status = getStatus;
                            rechargestatuscheck.res_msg = getmessage;
                            db.Entry(rechargestatuscheck).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();

                            CommissionDistributionHelper objComm = new CommissionDistributionHelper();
                            string valueComm = objComm.DistributeCommission(merchantid, "UTILITY", objval.RechargeAmt, Charged_Amt, Opening_Balance, OperatorName, "DTH Recharge");
                            return Json(getmessage);
                            //return Json(errorcode);
                        }

                        //CommissionDistributionHelper objComm = new CommissionDistributionHelper();
                        //string valueComm = objComm.DistributeCommission(merchantid, "UTILITY", objval.RechargeAmt, Charged_Amt, Opening_Balance, OperatorName, "DTH Recharge");
                        //return Json(getmessage);

                        //return Json(errorcode);
                        return Json("Recharge Pending");
                    }
                    else
                    {
                        return Json(errorcode);
                    }


                    //CommissionDistributionHelper objComm = new CommissionDistributionHelper();
                    ////string valueComm = objComm.DistributeCommission(merchantid, "UTILITY", objval.RechargeAmt, Charged_Amt, Opening_Balance, OperatorName, "DTH Recharge");
                    //string valueComm = objComm.DistributeCommission(merchantid, "UTILITY", 100, 100, 0, OperatorName, "DTH Recharge");
                    //return Json("");

                    //    }
                    //    else
                    //    {
                    //        errorcode = "Insufficient Balance";
                    //        return Json(errorcode);
                    //    }
                    //}
                    //else
                    //{
                    //    return Json(errorcode);
                    //}

                    //return Json("");

                    //**********************End apiscript recharge bill api integration************************

                    //var PaymentValidation = PaymentAPI.Validation(agentId, objval.RechargeAmt.ToString(), operatorId, objval.ContactNo);
                    //if (PaymentValidation == "TXN")
                    //{
                    //    var Recharge = PaymentAPI.Payment(agentId, objval.RechargeAmt.ToString(), operatorId, objval.ContactNo);
                    //    string errorcode = string.IsNullOrEmpty(Recharge.res_code.Value) ? Recharge.ipay_errordesc.Value : Recharge.res_code.Value;//res.res_code;
                    //    if (errorcode == "TXN")
                    //    {

                    //        string status = Recharge.status;
                    //        var ipat_id = Recharge.ipay_id.Value;
                    //        decimal trans_amt = decimal.Parse(Convert.ToString(Recharge.trans_amt.Value));
                    //        decimal Charged_Amt = decimal.Parse(Convert.ToString(Recharge.charged_amt.Value));
                    //        decimal Opening_Balance = decimal.Parse(Convert.ToString(Recharge.opening_bal.Value));
                    //        DateTime datevalue = Convert.ToDateTime(Recharge.datetime.Value);


                    //        TBL_INSTANTPAY_RECHARGE_RESPONSE insta = new TBL_INSTANTPAY_RECHARGE_RESPONSE()
                    //        {
                    //            Ipay_Id = ipat_id,
                    //            AgentId = Recharge.agent_id.Value,
                    //            Opr_Id = Recharge.opr_id.Value,
                    //            AccountNo = Recharge.account_no.Value,
                    //            Sp_Key = Recharge.sp_key.Value,
                    //            Trans_Amt = trans_amt,
                    //            Charged_Amt = Charged_Amt,
                    //            Opening_Balance = Opening_Balance,
                    //            DateVal = System.DateTime.Now,
                    //            Status = Recharge.status.Value,
                    //            Res_Code = Recharge.res_code.Value,
                    //            res_msg = Recharge.res_msg.Value,
                    //            Mem_ID = merchantid,
                    //            RechargeType = objval.PrepaidRecharge
                    //        };
                    //        db.TBL_INSTANTPAY_RECHARGE_RESPONSE.Add(insta);
                    //        await db.SaveChangesAsync();
                    //        var checksts = PaymentAPI.StatusCheck(agentId);
                    //        string checkval = checksts.res_code;//res.res_code;
                    //        string statuscheck = checksts.status;
                    //        string outputval = checksts.res_msg;
                    //        string ipayidval = checksts.ipay_id;
                    //        var rechargestatuscheck = await db.TBL_INSTANTPAY_RECHARGE_RESPONSE.Where(x => x.Ipay_Id == ipayidval).FirstOrDefaultAsync();
                    //        rechargestatuscheck.Status = statuscheck;
                    //        db.Entry(rechargestatuscheck).State = System.Data.Entity.EntityState.Modified;
                    //        await db.SaveChangesAsync();
                    //        CommissionDistributionHelper objComm = new CommissionDistributionHelper();
                    //        string valueComm = objComm.DistributeCommission(merchantid, "UTILITY", objval.RechargeAmt, Charged_Amt, Opening_Balance, operatorId, "DTH Recharge");
                    //        //Session["msgCheck"] = "Transaction Successful";
                    //        return Json(outputval);
                    //    }
                    //    else
                    //    {
                    //        //Session["msgCheck"] = Recharge;
                    //        return Json(errorcode);
                    //    }

                    //}
                    //else
                    //{
                    //    return Json(PaymentValidation);
                    //}
                }
                else
                {
                    var msg = "Can't procceed with transaction.You don't have sufficient balance.";
                    return Json(msg);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- POSTDTHRecharge(POST) Line No:- 392", ex);
                throw ex;
            }
        }
        #endregion

        // Land Line recharge
        #region Landline Recharge
        public ActionResult LandlineRecharge()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> AutoLandlineRechargeComplete(string prefix)
        {
            try {
                var db = new DBContext();
                var OperatorValue = await (from oper in db.TBL_SERVICE_PROVIDERS
                                     where oper.SERVICE_NAME.StartsWith(prefix) && oper.TYPE == "LANDLINE"
                                     select new
                                     {
                                         label = oper.SERVICE_NAME,
                                         val = oper.SERVICE_KEY
                                     }).ToListAsync();

                return Json(OperatorValue);
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- AutoLandlineRechargeComplete(POST) Line No:- 421", ex);
                throw ex;
            }
            //var db = new DBContext();
            //var OperatorValue = (from oper in db.TBL_OPERATOR_MASTER
            //                     where oper.OPERATORNAME.StartsWith(prefix) && oper.OPERATORTYPE == "LANDLINE"
            //                     select new
            //                     {
            //                         label = oper.OPERATORNAME + "-" + oper.RECHTYPE,
            //                         val = oper.PRODUCTID
            //                     }).ToList();

            //return Json(OperatorValue);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult PostLindlineRecharge(LandlineRecharge objval)
        public JsonResult PostLindlineRecharge(LandlineRecharge objval)
        {
            try
            {
                var db = new DBContext();
                string OperatorName = Request.Form["LandlineOperatorName"];
                string operatorId = Request.Form["LandlineOperatorId"];
                const string agentId = "2";
                var PaymentValidation = PaymentAPI.Validation(agentId, objval.RechargeAmt.ToString(), operatorId, objval.ContactNo);
                if (PaymentValidation == "TXN")
                {
                    //var Recharge = PaymentAPI.Payment(agentId, objval.RechargeAmt.ToString(), "ATP", objval.ContactNo);
                    var Recharge = PaymentAPI.Payment(agentId, objval.RechargeAmt.ToString(), operatorId, objval.ContactNo);
                    if (Recharge == "TXN")
                    {
                        var ipat_id = Recharge;
                        Session["msgCheck"] = "Transaction Successful";
                    }
                    else
                    {
                        Session["msgCheck"] = Recharge;
                    }
                    return Json(Recharge);
                }
                else
                {
                    return Json(PaymentValidation);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- PostLindlineRecharge(POST) Line No:- 469", ex);
                throw ex;
            }


        }
        #endregion

        // Data Card Recharge
        #region Data Card Recharge
        public ActionResult DatacardRecharge()
        {
            return View();
        }
        #endregion
        
        #region Broadband segment
        public ActionResult BroadbandRecharge()
        {

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> AutoBroadbandRechargeComplete(string prefix)
        {
            try
            {
                var db = new DBContext();
                var OperatorValue = await (from oper in db.TBL_SERVICE_PROVIDERS
                                     where oper.SERVICE_NAME.StartsWith(prefix) && oper.TYPE == "BROADBAND"
                                     select new
                                     {
                                         label = oper.SERVICE_NAME,
                                         val = oper.SERVICE_KEY
                                     }).ToListAsync();

                return Json(OperatorValue);
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- AutoBroadbandRechargeComplete(POST) Line No:- 510", ex);
                throw ex;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult PostBroadbandRecharge(BroadbandViewModel objval)
        {
            try
            {
                string OperatorName = Request.Form["txtBroadbandOperator"];
                string operatorId = Request.Form["broadbandoperId"];
                const string agentId = "2";
                var PaymentValidation = PaymentAPI.Validation(agentId, objval.RechargeAmount.ToString(), operatorId, objval.PhoneNo);
                if (PaymentValidation == "TXN")
                {
                    //var Recharge = PaymentAPI.Payment(agentId, objval.RechargeAmt.ToString(), "ATP", objval.ContactNo);
                    var Recharge = PaymentAPI.Payment(agentId, objval.RechargeAmount.ToString(), operatorId, objval.PhoneNo);
                    if (Recharge == "TXN")
                    {
                        var ipat_id = Recharge;
                        Session["msgCheck"] = "Transaction Successful";
                    }
                    else
                    {
                        Session["msgCheck"] = Recharge;
                    }
                    return Json(Recharge);
                }
                else
                {
                    return Json(PaymentValidation);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- PostBroadbandRecharge(POST) Line No:- 547", ex);
                throw ex;
            }
        }
        #endregion#

        #region Electricity Recharge section
        public ActionResult ElectricityBillPayment()
        {
            return View();
        }
        [HttpPost]        
        public async Task<JsonResult> AutoElectricityBillService(string prefix)
        {
            try
            {
                var db = new DBContext();
                var OperatorValue = await (from oper in db.TBL_SERVICE_PROVIDERS
                                     where oper.SERVICE_NAME.StartsWith(prefix) && oper.TYPE == "ELECTRICITY"
                                     select new
                                     {
                                         label = oper.SERVICE_NAME,
                                         val = oper.SERVICE_KEY
                                     }).ToListAsync();

                return Json(OperatorValue);
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- AutoElectricityBillService(POST) Line No:- 576", ex);
                throw ex;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PostElectricityBill(ElectricityViewModel objval)
        {
            try
            {
                var db = new DBContext();
                var check_walletAmt = db.TBL_ACCOUNTS.Where(x => x.MEM_ID == CurrentMerchant.MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                if (objval.RechargeAmt <= check_walletAmt.CLOSING)
                {

                    string OperatorName = Request.Form["txtElectricityOperator"];
                    string operatorId = Request.Form["ElectricityoperId"];
                    const string agentId = "395Y36706";
                    long merchantid = CurrentMerchant.MEM_ID;
                    var Pincode = db.TBL_MERCHANT_OUTLET_INFORMATION.Where(x => x.MEM_ID == CurrentMerchant.MEM_ID).Select(z => z.PINCODE).FirstOrDefault();
                    string option9 = objval.geolocation + "|" + Pincode;
                    var outletid = db.TBL_MERCHANT_OUTLET_INFORMATION.Where(x => x.MEM_ID == CurrentMerchant.MEM_ID).Select(z => z.OUTLETID).FirstOrDefault();

                    //long.TryParse(Session["MerchantUserId"].ToString(), out merchantid);
                    var PaymentValidation = ElectricityPaymentAPI.ElectricityValidation(agentId, objval.RechargeAmt.ToString(), objval.MobileNo.ToString(), operatorId, objval.CustomerId, option9, outletid.ToString());
                    if (PaymentValidation == "TXN")
                    {
                        //var Recharge = PaymentAPI.Payment(agentId, objval.RechargeAmt.ToString(), "ATP", objval.ContactNo);
                        var Recharge = ElectricityPaymentAPI.ElectricityPayment(agentId, objval.RechargeAmt.ToString(), objval.MobileNo.ToString(), operatorId, objval.CustomerId, option9, outletid.ToString());
                        string errorcode = string.IsNullOrEmpty(Recharge.res_code.Value) ? Recharge.ipay_errordesc.Value : Recharge.res_code.Value;//res.res_code;
                        if (errorcode == "TXN")
                        {
                            string status = Recharge.status;
                            var ipat_id = Recharge.ipay_id.Value;
                            decimal trans_amt = decimal.Parse(Convert.ToString(Recharge.trans_amt.Value));
                            decimal Charged_Amt = decimal.Parse(Convert.ToString(Recharge.charged_amt.Value));
                            decimal Opening_Balance = decimal.Parse(Convert.ToString(Recharge.opening_bal.Value));
                            DateTime datevalue = Convert.ToDateTime(Recharge.datetime.Value);
                            TBL_INSTANTPAY_RECHARGE_RESPONSE insta = new TBL_INSTANTPAY_RECHARGE_RESPONSE()
                            {
                                Ipay_Id = ipat_id,
                                AgentId = Recharge.agent_id.Value,
                                Opr_Id = Recharge.opr_id.Value,
                                AccountNo = Recharge.account_no.Value,
                                Sp_Key = Recharge.sp_key.Value,
                                Trans_Amt = decimal.Parse(trans_amt.ToString()),
                                Charged_Amt = decimal.Parse(Charged_Amt.ToString()),
                                Opening_Balance = decimal.Parse(Opening_Balance.ToString()),
                                DateVal = System.DateTime.Now,
                                Status = Recharge.status.Value,
                                Res_Code = Recharge.res_code.Value,
                                res_msg = Recharge.res_msg.Value,
                                Mem_ID = merchantid,
                                RechargeType = "ELECTRICITY"
                            };
                            db.TBL_INSTANTPAY_RECHARGE_RESPONSE.Add(insta);
                            await db.SaveChangesAsync();
                            var checksts = PaymentAPI.StatusCheck(agentId);
                            string checkval = checksts.res_code;//res.res_code;
                            string statuscheck = checksts.status;
                            string outputval = checksts.res_msg;
                            string ipayidval = checksts.ipay_id;
                            var rechargestatuscheck = await db.TBL_INSTANTPAY_RECHARGE_RESPONSE.Where(x => x.Ipay_Id == ipayidval).FirstOrDefaultAsync();
                            rechargestatuscheck.Status = statuscheck;
                            db.Entry(rechargestatuscheck).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                            CommissionDistributionHelper objComm = new CommissionDistributionHelper();
                            //string valueComm = objComm.DistributeCommission(merchantid, "ELECTRICITY", objval.RechargeAmt, Charged_Amt, Opening_Balance, operatorId, "Mobile Recharge");
                            string valueComm = objComm.DistributeCommission(merchantid, "ELECTRICITY", 0, Charged_Amt, Opening_Balance, operatorId, "ELECTRICITY");
                            //Session["msgCheck"] = "Transaction Successful";
                            return Json(outputval);
                        }
                        else
                        {
                            //Session["msgCheck"] = Recharge;
                            return Json(errorcode);
                        }
                        //if (Recharge == "TXN")
                        //{
                        //    var ipat_id = Recharge;
                        //    Session["msgCheck"] = "Transaction Successful";
                        //}
                        //else
                        //{
                        //    Session["msgCheck"] = Recharge;
                        //}
                        return Json(Recharge);
                    }
                    else
                    {
                        return Json(PaymentValidation);
                    }
                }
                else
                {
                    var msg = "Can't procceed with transaction.You don't have sufficient balance.";
                    return Json(msg);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- PostElectricityBill(POST) Line No:- 613", ex);
                throw ex;
            }


        }


        #endregion

        #region Gass Bill Payment
        public ActionResult GasBillPayment()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> AutoGasBillService(string prefix)
        {
            try
            {
                var db = new DBContext();
                var OperatorValue = await (from oper in db.TBL_SERVICE_PROVIDERS
                                     where oper.SERVICE_NAME.StartsWith(prefix) && oper.TYPE == "GAS"
                                     select new
                                     {
                                         label = oper.SERVICE_NAME,
                                         val = oper.SERVICE_KEY
                                     }).ToListAsync();

                return Json(OperatorValue);
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- AutoGasBillService(POST) Line No:- 646", ex);
                throw ex;
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult PostGasBillPayment(GasBillPaymentViewModel objval)
        {
            try
            {
                string OperatorName = Request.Form["txtGassServiceOperator"];
                string operatorId = Request.Form["GassServiceOperId"];
                const string agentId = "2";
                var PaymentValidation = PaymentAPI.Validation(agentId, objval.RechargeAmount.ToString(), operatorId, objval.CustomerID);
                if (PaymentValidation == "TXN")
                {
                    //var Recharge = PaymentAPI.Payment(agentId, objval.RechargeAmt.ToString(), "ATP", objval.ContactNo);
                    var Recharge = PaymentAPI.Payment(agentId, objval.RechargeAmount.ToString(), operatorId, objval.CustomerID);
                    if (Recharge == "TXN")
                    {
                        var ipat_id = Recharge;
                        Session["msgCheck"] = "Transaction Successful";
                    }
                    else
                    {
                        Session["msgCheck"] = Recharge;
                    }
                    return Json(Recharge);
                }
                else
                {
                    return Json(PaymentValidation);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- PostGasBillPayment(POST) Line No:- 682", ex);
                throw ex;
            }

           
        }
        #endregion

        #region Insurance Payment
        public ActionResult InsurancePayment()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> AutoInsuranceService(string prefix)
        {
            try
            {
                var db = new DBContext();
                var OperatorValue = await (from oper in db.TBL_SERVICE_PROVIDERS
                                     where oper.SERVICE_NAME.StartsWith(prefix) && oper.TYPE == "INSURANCE"
                                     select new
                                     {
                                         label = oper.SERVICE_NAME,
                                         val = oper.SERVICE_KEY
                                     }).ToListAsync();

                return Json(OperatorValue);
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- AutoInsuranceService(POST) Line No:- 713", ex);
                throw ex;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult PostInsurancerecharge(InsuranceViewModel objval)
        {
            try
            {
                string OperatorName = Request.Form["txtInsuranceServiceOperator"];
                string operatorId = Request.Form["InsuranceOperId"];
                const string agentId = "2";
                var PaymentValidation = PaymentAPI.Validation(agentId, objval.PolicyAmount.ToString(), operatorId, objval.PolicyNo);
                if (PaymentValidation == "TXN")
                {
                    //var Recharge = PaymentAPI.Payment(agentId, objval.RechargeAmt.ToString(), "ATP", objval.ContactNo);
                    var Recharge = PaymentAPI.Payment(agentId, objval.PolicyAmount.ToString(), operatorId, objval.PolicyNo);
                    if (Recharge == "TXN")
                    {
                        var ipat_id = Recharge;
                        Session["msgCheck"] = "Transaction Successful";
                    }
                    else
                    {
                        Session["msgCheck"] = Recharge;
                    }
                    return Json(Recharge);
                }
                else
                {
                    return Json(PaymentValidation);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- PostInsurancerecharge(POST) Line No:- 750", ex);
                throw ex;
            }
            
        }
        #endregion
        
        #region Recharge Details list
        public ActionResult RechargeTransactionList()
        {
            if (Session["MerchantUserId"] != null)
            {
                Session["MerchantDMRId"] = null;
                Session.Remove("MerchantDMRId");
                return View();
            }
            else
            {
                Session["MerchantUserId"] = null;
                Session["MerchantUserName"] = null;
                Session["UserType"] = null;
                Session.Remove("MerchantUserId");
                Session.Remove("MerchantUserName");
                Session.Remove("UserType");
                return RedirectToAction("Index", "MerchantLogin", new { area = "Merchant" });
            }
            //return View();
        }
        public PartialViewResult PartialRechargeInfoList()
        {
            var db = new DBContext();
            //var Mem_ID = long.Parse(CurrentMerchant.MEM_ID);
            var RechargeTransaction = db.TBL_INSTANTPAY_RECHARGE_RESPONSE.Where(x => x.Mem_ID == CurrentMerchant.MEM_ID).ToList();
            return PartialView("PartialRechargeInfoList", RechargeTransaction);
            //return PartialView(DisplayRechargeransaction());
        }
        private IGrid<TBL_INSTANTPAY_RECHARGE_RESPONSE> DisplayRechargeransaction()
        {
            try
            {
                var db = new DBContext();

                var Mem_ID = long.Parse(Session["MerchantUserId"].ToString());
                var RechargeTransaction = db.TBL_INSTANTPAY_RECHARGE_RESPONSE.Where(x => x.Mem_ID == Mem_ID).ToList();

                ////var bankdetails = db.TBL_SETTINGS_BANK_DETAILS.Where(x => x.ISDELETED == 0  && x.MEM_ID==MemberCurrentUser.MEM_ID).ToList();
                //var bankdetails = db.TBL_REMITTER_BENEFICIARY_INFO.Where(x => x.IsActive == 0 && x.RemitterID == remitterid).ToList();

                IGrid<TBL_INSTANTPAY_RECHARGE_RESPONSE> grid = new Grid<TBL_INSTANTPAY_RECHARGE_RESPONSE>(RechargeTransaction);
                grid.ViewContext = new ViewContext { HttpContext = HttpContext };
                grid.Query = Request.QueryString;
                grid.Columns.Add(model => model.Ipay_Id).Titled("IPAY_ID").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.AgentId).Titled("AGENT_ID").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.Opr_Id).Titled("OPR_ID").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.AccountNo).Titled("ACCOUNT_NO.").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.Sp_Key).Titled("SERVICE PROVIDER KEY").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.Trans_Amt).Titled("TRANS_AMT").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.Charged_Amt).Titled("CHARGED_AMT").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.Opening_Balance).Titled("CLOSING BAL.").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.DateVal).Titled("RECHARGE DATE").Filterable(true).Sortable(true).MultiFilterable(true);
                grid.Columns.Add(model => model.RechargeType).Titled("RECHARGE TYPE").Filterable(true).Sortable(true).MultiFilterable(true);
                //grid.Columns.Add(model => model.Status).Titled("STATUS").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.ID).Titled("STATUS").Encoded(false).Filterable(false).Sortable(false)
                    .RenderedAs(model => "<label class='label " + (model.Status == "SUCCESS" ? "label-success" : model.Status == "FAILED" ? "label-danger": "label-info") + "'> " + model.Status + " </label>");
                grid.Columns.Add(model => model.res_msg).Titled("RECHARGE STATUS").Filterable(true).Sortable(true);
                grid.Columns.Add(model => model.RechargeType).Titled("RECHARGE TYPE").Filterable(true).Sortable(true);
                //grid.Columns.Add(model => model.ID).Titled("").Encoded(false).Filterable(false).Sortable(false)
                //    .RenderedAs(model => "<a href='javascript:void(0)' class='btn btn-denger btn-xs' onclick='DeActivateBeneficiary(" + model.ID + ");return 0;'>DELETE</a>");
                grid.Pager = new GridPager<TBL_INSTANTPAY_RECHARGE_RESPONSE>(grid);
                grid.Processors.Add(grid.Pager);
                grid.Pager.RowsPerPage = 10;

                //foreach (IGridColumn column in grid.Columns)
                //{
                //    column.Filter.IsEnabled = true;
                //    column.Sort.IsEnabled = true;
                //}

                return grid;


            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- DisplayRechargeransaction(POST) Line No:- 823", ex);
                throw ex;
            }

        }
        #endregion


        #region water Bill Payment
        public ActionResult WaterSupplyPayment()
        {
            return View();
        }
        [HttpPost]        
        public async Task<JsonResult> AutoWaterBillService(string prefix)
        {
            try
            {
                var db = new DBContext();
                var OperatorValue = await (from oper in db.TBL_SERVICE_PROVIDERS
                                           where oper.SERVICE_NAME.StartsWith(prefix) && oper.TYPE == "WATER"
                                           select new
                                           {
                                               label = oper.SERVICE_NAME,
                                               val = oper.SERVICE_KEY
                                           }).ToListAsync();

                return Json(OperatorValue);
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- AutoElectricityBillService(POST) Line No:- 576", ex);
                throw ex;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PostWaterSypplyBillPayment(ElectricityViewModel objval)
        {
            try
            {
                var db = new DBContext();
                var check_walletAmt = db.TBL_ACCOUNTS.Where(x => x.MEM_ID == CurrentMerchant.MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                if (objval.RechargeAmt <= check_walletAmt.CLOSING)
                {
                    string OperatorName = Request.Form["txtWaterSupplyOperator"];
                    string operatorId = Request.Form["WaterSupplyoperId"];
                    const string agentId = "395Y36706";
                    long merchantid = CurrentMerchant.MEM_ID;
                    //long.TryParse(Session["MerchantUserId"].ToString(), out merchantid);
                    var Pincode = db.TBL_MERCHANT_OUTLET_INFORMATION.Where(x => x.MEM_ID == CurrentMerchant.MEM_ID).Select(z => z.PINCODE).FirstOrDefault();
                    string option9 = objval.geolocation + "|" + Pincode;

                    var outletid = db.TBL_MERCHANT_OUTLET_INFORMATION.Where(x => x.MEM_ID == CurrentMerchant.MEM_ID).Select(z => z.OUTLETID).FirstOrDefault();

                    var PaymentValidation = ElectricityPaymentAPI.ElectricityValidation(agentId, objval.RechargeAmt.ToString(), objval.MobileNo.ToString(), operatorId, objval.CustomerId, option9, outletid.ToString());
                    if (PaymentValidation == "TXN")
                    {
                        //var Recharge = PaymentAPI.Payment(agentId, objval.RechargeAmt.ToString(), "ATP", objval.ContactNo);
                        var Recharge = ElectricityPaymentAPI.ElectricityPayment(agentId, objval.MobileNo.ToString(), objval.MobileNo.ToString(), operatorId, objval.CustomerId, option9, outletid.ToString());
                        string errorcode = string.IsNullOrEmpty(Recharge.res_code.Value) ? Recharge.ipay_errordesc.Value : Recharge.res_code.Value;//res.res_code;
                        if (errorcode == "TXN")
                        {
                            string status = Recharge.status;
                            var ipat_id = Recharge.ipay_id.Value;
                            decimal trans_amt = decimal.Parse(Convert.ToString(Recharge.trans_amt.Value));
                            decimal Charged_Amt = decimal.Parse(Convert.ToString(Recharge.charged_amt.Value));
                            decimal Opening_Balance = decimal.Parse(Convert.ToString(Recharge.opening_bal.Value));
                            DateTime datevalue = Convert.ToDateTime(Recharge.datetime.Value);
                            TBL_INSTANTPAY_RECHARGE_RESPONSE insta = new TBL_INSTANTPAY_RECHARGE_RESPONSE()
                            {
                                Ipay_Id = ipat_id,
                                AgentId = Recharge.agent_id.Value,
                                Opr_Id = Recharge.opr_id.Value,
                                AccountNo = Recharge.account_no.Value,
                                Sp_Key = Recharge.sp_key.Value,
                                Trans_Amt = decimal.Parse(trans_amt.ToString()),
                                Charged_Amt = decimal.Parse(Charged_Amt.ToString()),
                                Opening_Balance = decimal.Parse(Opening_Balance.ToString()),
                                DateVal = System.DateTime.Now,
                                Status = Recharge.status.Value,
                                Res_Code = Recharge.res_code.Value,
                                res_msg = Recharge.res_msg.Value,
                                Mem_ID = merchantid,
                                RechargeType = "WATER"
                            };
                            db.TBL_INSTANTPAY_RECHARGE_RESPONSE.Add(insta);
                            await db.SaveChangesAsync();
                            var checksts = PaymentAPI.StatusCheck(agentId);
                            string checkval = checksts.res_code;//res.res_code;
                            string statuscheck = checksts.status;
                            string outputval = checksts.res_msg;
                            string ipayidval = checksts.ipay_id;
                            var rechargestatuscheck = await db.TBL_INSTANTPAY_RECHARGE_RESPONSE.Where(x => x.Ipay_Id == ipayidval).FirstOrDefaultAsync();
                            rechargestatuscheck.Status = statuscheck;
                            db.Entry(rechargestatuscheck).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                            CommissionDistributionHelper objComm = new CommissionDistributionHelper();
                            //string valueComm = objComm.DistributeCommission(merchantid, "ELECTRICITY", objval.RechargeAmt, Charged_Amt, Opening_Balance, operatorId, "Mobile Recharge");
                            string valueComm = objComm.DistributeCommission(merchantid, "WATER", 0, Charged_Amt, Opening_Balance, operatorId, "WATER");
                            //Session["msgCheck"] = "Transaction Successful";
                            return Json(outputval);
                        }
                        else
                        {
                            //Session["msgCheck"] = Recharge;
                            return Json(errorcode);
                        }
                        //if (Recharge == "TXN")
                        //{
                        //    var ipat_id = Recharge;
                        //    Session["msgCheck"] = "Transaction Successful";
                        //}
                        //else
                        //{
                        //    Session["msgCheck"] = Recharge;
                        //}
                        return Json(Recharge);
                    }
                    else
                    {
                        return Json(PaymentValidation);
                    }
                }
                else
                {
                    var msg = "Can't procceed with transaction.You don't have sufficient balance.";
                    return Json(msg);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Controller:-  MerchantRechargeService(Merchant), method:- PostElectricityBill(POST) Line No:- 613", ex);
                throw ex;
            }


        }


        #endregion

    }
}