namespace WHITELABEL.Web.Areas.Merchant.Models
{
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
    public class CommissionDistributionHelper
    {
        //public string CommissionDistributon(long Mem_ID, string status, decimal Trans_Amt, decimal ChargeAmt, decimal OpeningAmt, string serviceprovider, string rechargeType)
        //{
        //    var db = new DbContext();
        //    return "";
        //}
        #region Commission Distribution setting
        public string DistributeCommission(long Mem_ID, string status, decimal Trans_Amt, decimal ChargeAmt, decimal OpeningAmt, string serviceprovider, string rechargeType)
        {
            var db = new DBContext();
            using (System.Data.Entity.DbContextTransaction ContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    var DIS_MEM_ID = db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == Mem_ID).Select(z => z.INTRODUCER).FirstOrDefault();
                    var SUP_MEM_ID = db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == DIS_MEM_ID).Select(z => z.INTRODUCER).FirstOrDefault();
                    var WHT_MEM_ID = db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == SUP_MEM_ID).Select(z => z.UNDER_WHITE_LEVEL).FirstOrDefault();
                    if (status == "MOBILE")
                    {
                        var MerchantComm= (from detailscom in db.TBL_WHITE_LEVEL_COMMISSION_SLAB
                                            join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                                            on detailscom.SLN equals commslabMob.SLAB_ID join mem in db.TBL_MASTER_MEMBER on detailscom.MEM_ID equals mem.UNDER_WHITE_LEVEL
                                            where commslabMob.OPERATOR_CODE == serviceprovider && detailscom.SLAB_TYPE==1 && mem.MEMBER_ROLE==5
                                            select new
                                            {
                                                SLN = commslabMob.SLN,
                                                commPer = commslabMob.MERCHANT_COM_PER
                                            }).FirstOrDefault();

                        //var MerchantComm2 = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                        //                    join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                        //                    on detailscom.RECHARGE_SLAB equals commslabMob.SLAB_ID
                        //                    where detailscom.WHITE_LEVEL_ID == DIS_MEM_ID && detailscom.INTRODUCER_ID == Mem_ID && commslabMob.OPERATOR_CODE == serviceprovider
                        //                    select new
                        //                    {
                        //                        SLN = commslabMob.SLN,
                        //                        commPer = commslabMob.COMM_PERCENTAGE
                        //                    }).FirstOrDefault();
                        //var MerchantComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                        //                    join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                        //                    on detailscom.RECHARGE_SLAB equals commslabMob.SLAB_ID
                        //                    where detailscom.WHITE_LEVEL_ID == DIS_MEM_ID && detailscom.INTRODUCER_ID == Mem_ID && commslabMob.OPERATOR_CODE == serviceprovider
                        //                    select new
                        //                    {
                        //                        SLN = commslabMob.SLN,
                        //                        commPer = commslabMob.COMM_PERCENTAGE
                        //                    }).FirstOrDefault();
                        #region Retailer Commission                        
                        var membtype = (from mm in db.TBL_MASTER_MEMBER
                                        join
                                            rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                        where mm.MEM_ID == Mem_ID
                                        select new
                                        {
                                            RoleId = mm.MEMBER_ROLE,
                                            roleName = rm.ROLE_NAME,
                                            Amount = mm.BALANCE
                                        }).FirstOrDefault();
                        var tbl_account = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == Mem_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_account != null)
                        {
                            decimal ClosingAmt = tbl_account.CLOSING;
                            decimal SubAmt = ClosingAmt - Trans_Amt;
                            TBL_ACCOUNTS objaccnt = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Mem_ID,
                                MEMBER_TYPE = membtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "DR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Recharge",
                                OPENING = ClosingAmt,
                                CLOSING = SubAmt,
                                REC_NO = 0,
                                COMM_AMT = 0
                            };
                            db.TBL_ACCOUNTS.Add(objaccnt);
                            db.SaveChanges();

                            decimal getPer = 0;
                            if (MerchantComm != null)
                            {
                                getPer = (Trans_Amt * MerchantComm.commPer) / 100;
                            }

                            decimal CommAddClosing = SubAmt + getPer;
                            TBL_ACCOUNTS objCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Mem_ID,
                                MEMBER_TYPE = membtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "CR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = SubAmt,
                                CLOSING = CommAddClosing,
                                REC_NO = 0,
                                COMM_AMT = getPer
                            };
                            db.TBL_ACCOUNTS.Add(objCommPer);
                            db.SaveChanges();

                            var getmemberInfo = db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == Mem_ID).FirstOrDefault();
                            if (getmemberInfo != null)
                            {
                                getmemberInfo.BALANCE = CommAddClosing;
                                db.Entry(getmemberInfo).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        #endregion

                        var DistributorComm = (from detailscom in db.TBL_WHITE_LEVEL_COMMISSION_SLAB
                                               join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                                               on detailscom.SLN equals commslabMob.SLAB_ID join mem in db.TBL_MASTER_MEMBER on detailscom.MEM_ID equals mem.UNDER_WHITE_LEVEL
                                               where commslabMob.OPERATOR_CODE == serviceprovider && detailscom.SLAB_TYPE == 1 && mem.MEMBER_ROLE == 4
                                               select new
                                               {
                                                   SLN = commslabMob.SLN,
                                                   commPer = commslabMob.DISTRIBUTOR_COM_PER
                                               }).FirstOrDefault();
                        //var DistributorComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                        //                       join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                        //                       on detailscom.RECHARGE_SLAB equals commslabMob.SLAB_ID
                        //                       where detailscom.WHITE_LEVEL_ID == SUP_MEM_ID && detailscom.INTRODUCER_ID == DIS_MEM_ID && commslabMob.OPERATOR_CODE == serviceprovider
                        //                       select new
                        //                       {
                        //                           SLN = commslabMob.SLN,
                        //                           commPer = commslabMob.COMM_PERCENTAGE
                        //                       }).FirstOrDefault();
                        #region Distributor Commission
                        //decimal DisGapComm = decimal.Parse(DistributorComm.commPer.ToString()) - decimal.Parse(MerchantComm.commPer.ToString());
                        decimal DisGapComm = 0;
                        if (DistributorComm != null)
                        {
                            DisGapComm = decimal.Parse(DistributorComm.commPer.ToString());
                        }

                        decimal DisGapCommAmt = (Trans_Amt * DisGapComm) / 100;
                        var tbl_accountDistributor = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == DIS_MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_accountDistributor != null)
                        {
                            var CheckDisCommission = db.TBL_DETAILS_MEMBER_COMMISSION_SLAB.Where(x => x.INTRODUCER_ID == DIS_MEM_ID).FirstOrDefault();
                            var Dismembtype = (from mm in db.TBL_MASTER_MEMBER
                                               join
                                                   rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                               where mm.MEM_ID == DIS_MEM_ID
                                               select new
                                               {
                                                   RoleId = mm.MEMBER_ROLE,
                                                   roleName = rm.ROLE_NAME,
                                                   Amount = mm.BALANCE
                                               }).FirstOrDefault();
                            decimal DisClosingAmt = tbl_accountDistributor.CLOSING;
                            //decimal SubAmt = ClosingAmt - Trans_Amt;                   
                            //decimal DisgetPer = (Trans_Amt * checkDiscomm.CommPercentage) / 100;
                            decimal CommDisAddClosing = tbl_accountDistributor.CLOSING + DisGapCommAmt;
                            long Dis_ID = long.Parse(DIS_MEM_ID.ToString());
                            TBL_ACCOUNTS objDisCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Dis_ID,
                                MEMBER_TYPE = Dismembtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "CR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = DisClosingAmt,
                                CLOSING = CommDisAddClosing,
                                REC_NO = 0,
                                COMM_AMT = DisGapCommAmt
                            };
                            db.TBL_ACCOUNTS.Add(objDisCommPer);
                            db.SaveChanges();

                            var getmemberInfo = db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == Dis_ID).FirstOrDefault();
                            if (getmemberInfo != null)
                            {
                                getmemberInfo.BALANCE = CommDisAddClosing;
                                db.Entry(getmemberInfo).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        #endregion
                        var SuperComm = (from detailscom in db.TBL_WHITE_LEVEL_COMMISSION_SLAB
                                         join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                                          on detailscom.SLN equals commslabMob.SLAB_ID join mem in db.TBL_MASTER_MEMBER on detailscom.MEM_ID equals mem.UNDER_WHITE_LEVEL
                                         where commslabMob.OPERATOR_CODE == serviceprovider && detailscom.SLAB_TYPE == 1 && mem.MEMBER_ROLE == 3
                                         select new
                                         {
                                             SLN = commslabMob.SLN,
                                             commPer = commslabMob.SUPER_COM_PER
                                         }).FirstOrDefault();
                        //var SuperComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                        //                 join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                        //                  on detailscom.RECHARGE_SLAB equals commslabMob.SLAB_ID
                        //                 where detailscom.WHITE_LEVEL_ID == WHT_MEM_ID && detailscom.INTRODUCER_ID == SUP_MEM_ID && commslabMob.OPERATOR_CODE == serviceprovider
                        //                 select new
                        //                 {
                        //                     SLN = commslabMob.SLN,
                        //                     commPer = commslabMob.COMM_PERCENTAGE
                        //                 }).FirstOrDefault();
                        //decimal SupGapComm = decimal.Parse(SuperComm.commPer.ToString()) - decimal.Parse(DistributorComm.commPer.ToString());

                        decimal SupGapComm = 0;
                        if (SuperComm != null)
                        {
                            SupGapComm = decimal.Parse(SuperComm.commPer.ToString());
                        }
                        decimal SupGapCommAmt = (Trans_Amt * SupGapComm) / 100;
                        #region Super Commission
                        var tbl_accountSuper = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == SUP_MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_accountSuper != null)
                        {
                            var Supmembtype = (from mm in db.TBL_MASTER_MEMBER
                                               join
                                                   rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                               where mm.MEM_ID == SUP_MEM_ID
                                               select new
                                               {
                                                   RoleId = mm.MEMBER_ROLE,
                                                   roleName = rm.ROLE_NAME,
                                                   Amount = mm.BALANCE
                                               }).FirstOrDefault();
                            decimal SupClosingAmt = tbl_accountSuper.CLOSING;
                            decimal CommSupAddClosing = tbl_accountSuper.CLOSING + SupGapCommAmt;
                            long Sup_ID = long.Parse(SUP_MEM_ID.ToString());
                            TBL_ACCOUNTS objSupCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Sup_ID,
                                MEMBER_TYPE = Supmembtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "CR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = SupClosingAmt,
                                CLOSING = CommSupAddClosing,
                                REC_NO = 0,
                                COMM_AMT = SupGapCommAmt
                            };
                            db.TBL_ACCOUNTS.Add(objSupCommPer);
                            db.SaveChanges();

                            var getmemberInfo = db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == Sup_ID).FirstOrDefault();
                            if (getmemberInfo != null)
                            {
                                getmemberInfo.BALANCE = CommSupAddClosing;
                                db.Entry(getmemberInfo).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        #endregion                        
                        var WhiteComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                                         join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                                          on detailscom.RECHARGE_SLAB equals commslabMob.SLAB_ID
                                         where detailscom.WHITE_LEVEL_ID == WHT_MEM_ID && detailscom.INTRODUCER_ID == 0 && commslabMob.OPERATOR_CODE == serviceprovider 
                                         select new
                                         {
                                             SLN = commslabMob.SLN,
                                             commPer = commslabMob.COMM_PERCENTAGE
                                         }).FirstOrDefault();

                        decimal WLComm = decimal.Parse(MerchantComm!=null? MerchantComm.commPer.ToString():"0") + decimal.Parse(DistributorComm!=null?DistributorComm.commPer.ToString():"0") + decimal.Parse(SuperComm!=null?SuperComm.commPer.ToString():"0");
                        decimal WTLGapComm = 0;
                        if (WhiteComm != null)
                        {
                            WTLGapComm = decimal.Parse(WhiteComm.commPer.ToString()) - WLComm;
                        }
                        //decimal WTLGapComm = decimal.Parse(WhiteComm.commPer.ToString());
                        //decimal WTLGapComm =WLComm;
                        decimal WTLGapCommAmt = (Trans_Amt * WTLGapComm) / 100;
                        #region White level Commission payment Structure
                        var tbl_accountWhiteLevel = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == WHT_MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_accountWhiteLevel != null)
                        {
                            var WLmembtype = (from mm in db.TBL_MASTER_MEMBER
                                              join
                                                  rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                              where mm.MEM_ID == WHT_MEM_ID
                                              select new
                                              {
                                                  RoleId = mm.MEMBER_ROLE,
                                                  roleName = rm.ROLE_NAME,
                                                  Amount = mm.BALANCE
                                              }).FirstOrDefault();
                            decimal WLClosingAmt = tbl_accountWhiteLevel.CLOSING;
                            //decimal SubAmt = ClosingAmt - Trans_Amt;                  
                            decimal CommWLAddClosing = tbl_accountWhiteLevel.CLOSING + WTLGapCommAmt;
                            long WL_ID = long.Parse(WHT_MEM_ID.ToString());
                            TBL_ACCOUNTS objWLCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = WL_ID,
                                MEMBER_TYPE = WLmembtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "CR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = WLClosingAmt,
                                CLOSING = CommWLAddClosing,
                                REC_NO = 0,
                                COMM_AMT = WTLGapCommAmt
                            };
                            db.TBL_ACCOUNTS.Add(objWLCommPer);
                            db.SaveChanges();

                            var getmemberInfo = db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == WL_ID).FirstOrDefault();
                            if (getmemberInfo != null)
                            {
                                getmemberInfo.BALANCE = CommWLAddClosing;
                                db.Entry(getmemberInfo).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }

                        }
                        #endregion

                        ContextTransaction.Commit();
                        return "Success";
                    }
                    else if (status == "UTILITY")
                    {
                        var MerchantComm = (from detailscom in db.TBL_WHITE_LEVEL_COMMISSION_SLAB
                                            join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                                            on detailscom.SLN equals commslabMob.SLAB_ID
                                            join mem in db.TBL_MASTER_MEMBER on detailscom.MEM_ID equals mem.UNDER_WHITE_LEVEL
                                            where commslabMob.OPERATOR_CODE == serviceprovider && detailscom.SLAB_TYPE == 2 && mem.MEMBER_ROLE == 5
                                            select new
                                            {
                                                SLN = commslabMob.SLN,
                                                commPer = commslabMob.MERCHANT_COM_PER
                                            }).FirstOrDefault();
                        //var MerchantComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                        //                    join commslabMob in db.TBL_COMM_SLAB_UTILITY_RECHARGE
                        //                    on detailscom.BILLPAYMENT_SLAB equals commslabMob.SLAB_ID
                        //                    where detailscom.WHITE_LEVEL_ID == DIS_MEM_ID && detailscom.INTRODUCER_ID == Mem_ID && commslabMob.OPERATOR_CODE == serviceprovider
                        //                    select new
                        //                    {
                        //                        SLN = commslabMob.SLN,
                        //                        commPer = commslabMob.COMM_PERCENTAGE
                        //                    }).FirstOrDefault();
                        #region Retailer Commission                        
                        var membtype = (from mm in db.TBL_MASTER_MEMBER
                                        join
                                            rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                        where mm.MEM_ID == Mem_ID
                                        select new
                                        {
                                            RoleId = mm.MEMBER_ROLE,
                                            roleName = rm.ROLE_NAME,
                                            Amount = mm.BALANCE
                                        }).FirstOrDefault();
                        var tbl_account = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == Mem_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_account != null)
                        {
                            decimal ClosingAmt = tbl_account.CLOSING;
                            decimal SubAmt = ClosingAmt - Trans_Amt;
                            TBL_ACCOUNTS objaccnt = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Mem_ID,
                                MEMBER_TYPE = membtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "DR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Recharge",
                                OPENING = ClosingAmt,
                                CLOSING = SubAmt,
                                REC_NO = 0,
                                COMM_AMT = 0
                            };
                            db.TBL_ACCOUNTS.Add(objaccnt);
                            db.SaveChanges();

                            decimal getPer = (Trans_Amt * (MerchantComm!=null? MerchantComm.commPer:0)) / 100;
                            decimal CommAddClosing = SubAmt + getPer;
                            TBL_ACCOUNTS objCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Mem_ID,
                                MEMBER_TYPE = membtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "CR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = SubAmt,
                                CLOSING = CommAddClosing,
                                REC_NO = 0,
                                COMM_AMT = getPer
                            };
                            db.TBL_ACCOUNTS.Add(objCommPer);
                            db.SaveChanges();

                            var getmemberInfo = db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == Mem_ID).FirstOrDefault();
                            if (getmemberInfo != null)
                            {
                                getmemberInfo.BALANCE = CommAddClosing;
                                db.Entry(getmemberInfo).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }

                        }
                        #endregion

                        var DistributorComm = (from detailscom in db.TBL_WHITE_LEVEL_COMMISSION_SLAB
                                               join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                                               on detailscom.SLN equals commslabMob.SLAB_ID
                                               join mem in db.TBL_MASTER_MEMBER on detailscom.MEM_ID equals mem.UNDER_WHITE_LEVEL
                                               where commslabMob.OPERATOR_CODE == serviceprovider && detailscom.SLAB_TYPE == 2 && mem.MEMBER_ROLE == 4
                                               select new
                                               {
                                                   SLN = commslabMob.SLN,
                                                   commPer = commslabMob.DISTRIBUTOR_COM_PER
                                               }).FirstOrDefault();
                        //var DistributorComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                        //                       join commslabMob in db.TBL_COMM_SLAB_UTILITY_RECHARGE
                        //                       on detailscom.BILLPAYMENT_SLAB equals commslabMob.SLAB_ID
                        //                       where detailscom.WHITE_LEVEL_ID == SUP_MEM_ID && detailscom.INTRODUCER_ID == DIS_MEM_ID && commslabMob.OPERATOR_CODE == serviceprovider
                        //                       select new
                        //                       {
                        //                           SLN = commslabMob.SLN,
                        //                           commPer = commslabMob.COMM_PERCENTAGE
                        //                       }).FirstOrDefault();
                        #region Distributor Commission
                        //decimal DisGapComm = decimal.Parse(DistributorComm.commPer.ToString()) - decimal.Parse(MerchantComm.commPer.ToString());
                        decimal DisGapComm = decimal.Parse(DistributorComm!=null?DistributorComm.commPer.ToString():"0");
                        decimal DisGapCommAmt = (Trans_Amt * DisGapComm) / 100;
                        var tbl_accountDistributor = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == DIS_MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_accountDistributor != null)
                        {
                            var CheckDisCommission = db.TBL_DETAILS_MEMBER_COMMISSION_SLAB.Where(x => x.INTRODUCER_ID == DIS_MEM_ID).FirstOrDefault();
                            var Dismembtype = (from mm in db.TBL_MASTER_MEMBER
                                               join
                                                   rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                               where mm.MEM_ID == DIS_MEM_ID
                                               select new
                                               {
                                                   RoleId = mm.MEMBER_ROLE,
                                                   roleName = rm.ROLE_NAME,
                                                   Amount = mm.BALANCE
                                               }).FirstOrDefault();
                            decimal DisClosingAmt = tbl_accountDistributor.CLOSING;
                            //decimal SubAmt = ClosingAmt - Trans_Amt;                   
                            //decimal DisgetPer = (Trans_Amt * checkDiscomm.CommPercentage) / 100;
                            decimal CommDisAddClosing = tbl_accountDistributor.CLOSING + DisGapCommAmt;
                            long Dis_ID = long.Parse(DIS_MEM_ID.ToString());
                            TBL_ACCOUNTS objDisCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Dis_ID,
                                MEMBER_TYPE = Dismembtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "CR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = DisClosingAmt,
                                CLOSING = CommDisAddClosing,
                                REC_NO = 0,
                                COMM_AMT = DisGapCommAmt
                            };
                            db.TBL_ACCOUNTS.Add(objDisCommPer);
                            db.SaveChanges();

                            var getmemberInfo = db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == Dis_ID).FirstOrDefault();
                            if (getmemberInfo != null)
                            {
                                getmemberInfo.BALANCE = CommDisAddClosing;
                                db.Entry(getmemberInfo).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        #endregion
                        //var SuperComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                        //                 join commslabMob in db.TBL_COMM_SLAB_UTILITY_RECHARGE
                        //                  on detailscom.BILLPAYMENT_SLAB equals commslabMob.SLAB_ID
                        //                 where detailscom.WHITE_LEVEL_ID == WHT_MEM_ID && detailscom.INTRODUCER_ID == SUP_MEM_ID && commslabMob.OPERATOR_CODE == serviceprovider
                        //                 select new
                        //                 {
                        //                     SLN = commslabMob.SLN,
                        //                     commPer = commslabMob.COMM_PERCENTAGE
                        //                 }).FirstOrDefault();
                        var SuperComm = (from detailscom in db.TBL_WHITE_LEVEL_COMMISSION_SLAB
                                         join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                                          on detailscom.SLN equals commslabMob.SLAB_ID
                                         join mem in db.TBL_MASTER_MEMBER on detailscom.MEM_ID equals mem.UNDER_WHITE_LEVEL
                                         where commslabMob.OPERATOR_CODE == serviceprovider && detailscom.SLAB_TYPE == 2 && mem.MEMBER_ROLE == 3
                                         select new
                                         {
                                             SLN = commslabMob.SLN,
                                             commPer = commslabMob.SUPER_COM_PER
                                         }).FirstOrDefault();
                        //decimal SupGapComm = decimal.Parse(SuperComm.commPer.ToString()) - decimal.Parse(DistributorComm.commPer.ToString());
                        decimal SupGapComm = decimal.Parse(SuperComm!=null?SuperComm.commPer.ToString():"0") ;
                        decimal SupGapCommAmt = (Trans_Amt * SupGapComm) / 100;
                        #region Super Commission
                        var tbl_accountSuper = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == SUP_MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_accountSuper != null)
                        {
                            var Supmembtype = (from mm in db.TBL_MASTER_MEMBER
                                               join
                                                   rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                               where mm.MEM_ID == SUP_MEM_ID
                                               select new
                                               {
                                                   RoleId = mm.MEMBER_ROLE,
                                                   roleName = rm.ROLE_NAME,
                                                   Amount = mm.BALANCE
                                               }).FirstOrDefault();
                            decimal SupClosingAmt = tbl_accountSuper.CLOSING;
                            decimal CommSupAddClosing = tbl_accountSuper.CLOSING + SupGapCommAmt;
                            long Sup_ID = long.Parse(SUP_MEM_ID.ToString());
                            TBL_ACCOUNTS objSupCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Sup_ID,
                                MEMBER_TYPE = Supmembtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "CR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = SupClosingAmt,
                                CLOSING = CommSupAddClosing,
                                REC_NO = 0,
                                COMM_AMT = SupGapCommAmt
                            };
                            db.TBL_ACCOUNTS.Add(objSupCommPer);
                            db.SaveChanges();

                            var getmemberInfo = db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == Sup_ID).FirstOrDefault();
                            if (getmemberInfo != null)
                            {
                                getmemberInfo.BALANCE = CommSupAddClosing;
                                db.Entry(getmemberInfo).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        #endregion                        
                        var WhiteComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                                         join commslabMob in db.TBL_COMM_SLAB_UTILITY_RECHARGE
                                          on detailscom.BILLPAYMENT_SLAB equals commslabMob.SLAB_ID
                                         where detailscom.WHITE_LEVEL_ID == WHT_MEM_ID && detailscom.INTRODUCER_ID == 0 && commslabMob.OPERATOR_CODE == serviceprovider
                                         select new
                                         {
                                             SLN = commslabMob.SLN,
                                             commPer = commslabMob.COMM_PERCENTAGE
                                         }).FirstOrDefault();
                        decimal WL_Com = decimal.Parse(MerchantComm!=null?MerchantComm.commPer.ToString():"0") + decimal.Parse(DistributorComm!=null?DistributorComm.commPer.ToString():"0") + decimal.Parse(SuperComm!=null?SuperComm.commPer.ToString():"0");

                        decimal WTLGapComm = decimal.Parse(WhiteComm!=null?WhiteComm.commPer.ToString():"0") - WL_Com;
                        //decimal WTLGapComm = decimal.Parse(WhiteComm.commPer.ToString()) - decimal.Parse(SuperComm.commPer.ToString());
                        decimal WTLGapCommAmt = (Trans_Amt * WTLGapComm) / 100;
                        #region White level Commission payment Structure
                        var tbl_accountWhiteLevel = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == WHT_MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_accountWhiteLevel != null)
                        {
                            var WLmembtype = (from mm in db.TBL_MASTER_MEMBER
                                              join
                                                  rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                              where mm.MEM_ID == WHT_MEM_ID
                                              select new
                                              {
                                                  RoleId = mm.MEMBER_ROLE,
                                                  roleName = rm.ROLE_NAME,
                                                  Amount = mm.BALANCE
                                              }).FirstOrDefault();
                            decimal WLClosingAmt = tbl_accountWhiteLevel.CLOSING;
                            //decimal SubAmt = ClosingAmt - Trans_Amt;                  
                            decimal CommWLAddClosing = tbl_accountWhiteLevel.CLOSING + WTLGapCommAmt;
                            long WL_ID = long.Parse(WHT_MEM_ID.ToString());
                            TBL_ACCOUNTS objWLCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = WL_ID,
                                MEMBER_TYPE = WLmembtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "CR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = WLClosingAmt,
                                CLOSING = CommWLAddClosing,
                                REC_NO = 0,
                                COMM_AMT = WTLGapCommAmt
                            };
                            db.TBL_ACCOUNTS.Add(objWLCommPer);
                            db.SaveChanges();

                            var getmemberInfo = db.TBL_MASTER_MEMBER.Where(x => x.MEM_ID == WL_ID).FirstOrDefault();
                            if (getmemberInfo != null)
                            {
                                getmemberInfo.BALANCE = CommWLAddClosing;
                                db.Entry(getmemberInfo).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        #endregion
                        ContextTransaction.Commit();
                        return "Success";
                    }
                    else if (status == "WATER")
                    {
                        //var MerchantComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                        //                    join commslabMob in db.TBL_COMM_SLAB_UTILITY_RECHARGE
                        //                    on detailscom.BILLPAYMENT_SLAB equals commslabMob.SLAB_ID
                        //                    where detailscom.WHITE_LEVEL_ID == DIS_MEM_ID && detailscom.INTRODUCER_ID == Mem_ID && commslabMob.OPERATOR_CODE == serviceprovider
                        //                    select new
                        //                    {
                        //                        SLN = commslabMob.SLN,
                        //                        commPer = commslabMob.COMM_PERCENTAGE
                        //                    }).FirstOrDefault();
                        var MerchantComm = (from detailscom in db.TBL_WHITE_LEVEL_COMMISSION_SLAB
                                            join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                                            on detailscom.SLN equals commslabMob.SLAB_ID
                                            join mem in db.TBL_MASTER_MEMBER on detailscom.MEM_ID equals mem.UNDER_WHITE_LEVEL
                                            where commslabMob.OPERATOR_CODE == serviceprovider && mem.MEMBER_ROLE == 5
                                            select new
                                            {
                                                SLN = commslabMob.SLN,
                                                commPer = commslabMob.MERCHANT_COM_PER
                                            }).FirstOrDefault();
                        #region Retailer Commission                        
                        var membtype = (from mm in db.TBL_MASTER_MEMBER
                                        join
                                            rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                        where mm.MEM_ID == Mem_ID
                                        select new
                                        {
                                            RoleId = mm.MEMBER_ROLE,
                                            roleName = rm.ROLE_NAME,
                                            Amount = mm.BALANCE
                                        }).FirstOrDefault();
                        var tbl_account = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == Mem_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_account != null)
                        {
                            decimal ClosingAmt = tbl_account.CLOSING;
                            decimal SubAmt = ClosingAmt - Trans_Amt;
                            TBL_ACCOUNTS objaccnt = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Mem_ID,
                                MEMBER_TYPE = membtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "DR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Recharge",
                                OPENING = ClosingAmt,
                                CLOSING = SubAmt,
                                REC_NO = 0,
                                COMM_AMT = 0
                            };
                            db.TBL_ACCOUNTS.Add(objaccnt);
                            db.SaveChanges();
                            decimal getPer = (Trans_Amt * MerchantComm.commPer) / 100;
                            decimal CommAddClosing = SubAmt + getPer;
                            TBL_ACCOUNTS objCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Mem_ID,
                                MEMBER_TYPE = membtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "CR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = SubAmt,
                                CLOSING = CommAddClosing,
                                REC_NO = 0,
                                COMM_AMT = getPer
                            };
                            db.TBL_ACCOUNTS.Add(objCommPer);
                            db.SaveChanges();
                        }
                        #endregion

                        //var DistributorComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                        //                       join commslabMob in db.TBL_COMM_SLAB_UTILITY_RECHARGE
                        //                       on detailscom.BILLPAYMENT_SLAB equals commslabMob.SLAB_ID
                        //                       where detailscom.WHITE_LEVEL_ID == SUP_MEM_ID && detailscom.INTRODUCER_ID == DIS_MEM_ID && commslabMob.OPERATOR_CODE == serviceprovider
                        //                       select new
                        //                       {
                        //                           SLN = commslabMob.SLN,
                        //                           commPer = commslabMob.COMM_PERCENTAGE
                        //                       }).FirstOrDefault();
                        var DistributorComm = (from detailscom in db.TBL_WHITE_LEVEL_COMMISSION_SLAB
                                               join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                                               on detailscom.SLN equals commslabMob.SLAB_ID
                                               join mem in db.TBL_MASTER_MEMBER on detailscom.MEM_ID equals mem.UNDER_WHITE_LEVEL
                                               where commslabMob.OPERATOR_CODE == serviceprovider && mem.MEMBER_ROLE == 4
                                               select new
                                               {
                                                   SLN = commslabMob.SLN,
                                                   commPer = commslabMob.DISTRIBUTOR_COM_PER
                                               }).FirstOrDefault();
                        #region Distributor Commission
                        //decimal DisGapComm = decimal.Parse(DistributorComm.commPer.ToString()) - decimal.Parse(MerchantComm.commPer.ToString());
                        decimal DisGapComm = decimal.Parse(DistributorComm.commPer.ToString());
                        decimal DisGapCommAmt = (Trans_Amt * DisGapComm) / 100;
                        var tbl_accountDistributor = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == DIS_MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_accountDistributor != null)
                        {
                            var CheckDisCommission = db.TBL_DETAILS_MEMBER_COMMISSION_SLAB.Where(x => x.INTRODUCER_ID == DIS_MEM_ID).FirstOrDefault();
                            var Dismembtype = (from mm in db.TBL_MASTER_MEMBER
                                               join
                                                   rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                               where mm.MEM_ID == DIS_MEM_ID
                                               select new
                                               {
                                                   RoleId = mm.MEMBER_ROLE,
                                                   roleName = rm.ROLE_NAME,
                                                   Amount = mm.BALANCE
                                               }).FirstOrDefault();
                            decimal DisClosingAmt = tbl_accountDistributor.CLOSING;
                            //decimal SubAmt = ClosingAmt - Trans_Amt;                   
                            //decimal DisgetPer = (Trans_Amt * checkDiscomm.CommPercentage) / 100;
                            decimal CommDisAddClosing = tbl_accountDistributor.CLOSING + DisGapCommAmt;
                            long Dis_ID = long.Parse(DIS_MEM_ID.ToString());
                            TBL_ACCOUNTS objDisCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Dis_ID,
                                MEMBER_TYPE = Dismembtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "CR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = DisClosingAmt,
                                CLOSING = CommDisAddClosing,
                                REC_NO = 0,
                                COMM_AMT = DisGapCommAmt
                            };
                            db.TBL_ACCOUNTS.Add(objDisCommPer);
                            db.SaveChanges();
                        }
                        #endregion
                        //var SuperComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                        //                 join commslabMob in db.TBL_COMM_SLAB_UTILITY_RECHARGE
                        //                  on detailscom.BILLPAYMENT_SLAB equals commslabMob.SLAB_ID
                        //                 where detailscom.WHITE_LEVEL_ID == WHT_MEM_ID && detailscom.INTRODUCER_ID == SUP_MEM_ID && commslabMob.OPERATOR_CODE == serviceprovider
                        //                 select new
                        //                 {
                        //                     SLN = commslabMob.SLN,
                        //                     commPer = commslabMob.COMM_PERCENTAGE
                        //                 }).FirstOrDefault();
                        var SuperComm = (from detailscom in db.TBL_WHITE_LEVEL_COMMISSION_SLAB
                                         join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                                          on detailscom.SLN equals commslabMob.SLAB_ID
                                         join mem in db.TBL_MASTER_MEMBER on detailscom.MEM_ID equals mem.UNDER_WHITE_LEVEL
                                         where commslabMob.OPERATOR_CODE == serviceprovider && mem.MEMBER_ROLE == 3
                                         select new
                                         {
                                             SLN = commslabMob.SLN,
                                             commPer = commslabMob.SUPER_COM_PER
                                         }).FirstOrDefault();
                        //decimal SupGapComm = decimal.Parse(SuperComm.commPer.ToString()) - decimal.Parse(DistributorComm.commPer.ToString());
                        decimal SupGapComm = decimal.Parse(SuperComm.commPer.ToString());
                        decimal SupGapCommAmt = (Trans_Amt * SupGapComm) / 100;
                        #region Super Commission
                        var tbl_accountSuper = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == SUP_MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_accountSuper != null)
                        {
                            var Supmembtype = (from mm in db.TBL_MASTER_MEMBER
                                               join
                                                   rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                               where mm.MEM_ID == SUP_MEM_ID
                                               select new
                                               {
                                                   RoleId = mm.MEMBER_ROLE,
                                                   roleName = rm.ROLE_NAME,
                                                   Amount = mm.BALANCE
                                               }).FirstOrDefault();
                            decimal SupClosingAmt = tbl_accountSuper.CLOSING;
                            decimal CommSupAddClosing = tbl_accountSuper.CLOSING + SupGapCommAmt;
                            long Sup_ID = long.Parse(SUP_MEM_ID.ToString());
                            TBL_ACCOUNTS objSupCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Sup_ID,
                                MEMBER_TYPE = Supmembtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "CR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = SupClosingAmt,
                                CLOSING = CommSupAddClosing,
                                REC_NO = 0,
                                COMM_AMT = SupGapCommAmt
                            };
                            db.TBL_ACCOUNTS.Add(objSupCommPer);
                            db.SaveChanges();
                        }
                        #endregion                        
                        var WhiteComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                                         join commslabMob in db.TBL_COMM_SLAB_UTILITY_RECHARGE
                                          on detailscom.BILLPAYMENT_SLAB equals commslabMob.SLAB_ID
                                         where detailscom.WHITE_LEVEL_ID == WHT_MEM_ID && detailscom.INTRODUCER_ID == 0 && commslabMob.OPERATOR_CODE == serviceprovider
                                         select new
                                         {
                                             SLN = commslabMob.SLN,
                                             commPer = commslabMob.COMM_PERCENTAGE
                                         }).FirstOrDefault();
                        decimal Gap_WLlevel = decimal.Parse(MerchantComm.commPer.ToString()) + decimal.Parse(DistributorComm.commPer.ToString()) + decimal.Parse(SuperComm.commPer.ToString());

                        //decimal WTLGapComm = decimal.Parse(WhiteComm.commPer.ToString()) - decimal.Parse(SuperComm.commPer.ToString());
                        decimal WTLGapComm = decimal.Parse(WhiteComm.commPer.ToString()) - Gap_WLlevel;
                        decimal WTLGapCommAmt = (Trans_Amt * WTLGapComm) / 100;
                        #region White level Commission payment Structure
                        var tbl_accountWhiteLevel = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == WHT_MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_accountWhiteLevel != null)
                        {
                            var WLmembtype = (from mm in db.TBL_MASTER_MEMBER
                                              join
                                                  rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                              where mm.MEM_ID == WHT_MEM_ID
                                              select new
                                              {
                                                  RoleId = mm.MEMBER_ROLE,
                                                  roleName = rm.ROLE_NAME,
                                                  Amount = mm.BALANCE
                                              }).FirstOrDefault();
                            decimal WLClosingAmt = tbl_accountWhiteLevel.CLOSING;
                            //decimal SubAmt = ClosingAmt - Trans_Amt;                  
                            decimal CommWLAddClosing = tbl_accountWhiteLevel.CLOSING + WTLGapCommAmt;
                            long WL_ID = long.Parse(WHT_MEM_ID.ToString());
                            TBL_ACCOUNTS objWLCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = WL_ID,
                                MEMBER_TYPE = WLmembtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "CR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = WLClosingAmt,
                                CLOSING = CommWLAddClosing,
                                REC_NO = 0,
                                COMM_AMT = WTLGapCommAmt
                            };
                            db.TBL_ACCOUNTS.Add(objWLCommPer);
                            db.SaveChanges();
                        }
                        #endregion
                        ContextTransaction.Commit();
                        return "Success";
                    }
                    else if (status == "ELECTRICITY")
                    {
                        //var MerchantComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                        //                    join commslabMob in db.TBL_COMM_SLAB_UTILITY_RECHARGE
                        //                    on detailscom.BILLPAYMENT_SLAB equals commslabMob.SLAB_ID
                        //                    where detailscom.WHITE_LEVEL_ID == DIS_MEM_ID && detailscom.INTRODUCER_ID == Mem_ID && commslabMob.OPERATOR_CODE == serviceprovider
                        //                    select new
                        //                    {
                        //                        SLN = commslabMob.SLN,
                        //                        commPer = commslabMob.COMM_PERCENTAGE
                        //                    }).FirstOrDefault();
                        var MerchantComm = (from detailscom in db.TBL_WHITE_LEVEL_COMMISSION_SLAB
                                            join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                                            on detailscom.SLN equals commslabMob.SLAB_ID
                                            join mem in db.TBL_MASTER_MEMBER on detailscom.MEM_ID equals mem.UNDER_WHITE_LEVEL
                                            where commslabMob.OPERATOR_CODE == serviceprovider && mem.MEMBER_ROLE == 5
                                            select new
                                            {
                                                SLN = commslabMob.SLN,
                                                commPer = commslabMob.MERCHANT_COM_PER
                                            }).FirstOrDefault();
                        #region Retailer Commission                        
                        var membtype = (from mm in db.TBL_MASTER_MEMBER
                                        join
                                            rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                        where mm.MEM_ID == Mem_ID
                                        select new
                                        {
                                            RoleId = mm.MEMBER_ROLE,
                                            roleName = rm.ROLE_NAME,
                                            Amount = mm.BALANCE
                                        }).FirstOrDefault();
                        var tbl_account = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == Mem_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_account != null)
                        {
                            decimal ClosingAmt = tbl_account.CLOSING;
                            decimal SubAmt = ClosingAmt - Trans_Amt;
                            TBL_ACCOUNTS objaccnt = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Mem_ID,
                                MEMBER_TYPE = membtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "DR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Recharge",
                                OPENING = ClosingAmt,
                                CLOSING = SubAmt,
                                REC_NO = 0,
                                COMM_AMT = 0
                            };
                            db.TBL_ACCOUNTS.Add(objaccnt);
                            db.SaveChanges();
                            decimal getPer = (Trans_Amt * MerchantComm.commPer) / 100;
                            decimal CommAddClosing = SubAmt + getPer;
                            TBL_ACCOUNTS objCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Mem_ID,
                                MEMBER_TYPE = membtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "CR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = SubAmt,
                                CLOSING = CommAddClosing,
                                REC_NO = 0,
                                COMM_AMT = getPer
                            };
                            db.TBL_ACCOUNTS.Add(objCommPer);
                            db.SaveChanges();
                        }
                        #endregion

                        //var DistributorComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                        //                       join commslabMob in db.TBL_COMM_SLAB_UTILITY_RECHARGE
                        //                       on detailscom.BILLPAYMENT_SLAB equals commslabMob.SLAB_ID
                        //                       where detailscom.WHITE_LEVEL_ID == SUP_MEM_ID && detailscom.INTRODUCER_ID == DIS_MEM_ID && commslabMob.OPERATOR_CODE == serviceprovider
                        //                       select new
                        //                       {
                        //                           SLN = commslabMob.SLN,
                        //                           commPer = commslabMob.COMM_PERCENTAGE
                        //                       }).FirstOrDefault();
                        var DistributorComm = (from detailscom in db.TBL_WHITE_LEVEL_COMMISSION_SLAB
                                               join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                                               on detailscom.SLN equals commslabMob.SLAB_ID
                                               join mem in db.TBL_MASTER_MEMBER on detailscom.MEM_ID equals mem.UNDER_WHITE_LEVEL
                                               where commslabMob.OPERATOR_CODE == serviceprovider && mem.MEMBER_ROLE == 4
                                               select new
                                               {
                                                   SLN = commslabMob.SLN,
                                                   commPer = commslabMob.DISTRIBUTOR_COM_PER
                                               }).FirstOrDefault();
                        #region Distributor Commission
                        //decimal DisGapComm = decimal.Parse(DistributorComm.commPer.ToString()) - decimal.Parse(MerchantComm.commPer.ToString());
                        decimal DisGapComm = decimal.Parse(DistributorComm.commPer.ToString());
                        decimal DisGapCommAmt = (Trans_Amt * DisGapComm) / 100;
                        var tbl_accountDistributor = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == DIS_MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_accountDistributor != null)
                        {
                            var CheckDisCommission = db.TBL_DETAILS_MEMBER_COMMISSION_SLAB.Where(x => x.INTRODUCER_ID == DIS_MEM_ID).FirstOrDefault();
                            var Dismembtype = (from mm in db.TBL_MASTER_MEMBER
                                               join
                                                   rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                               where mm.MEM_ID == DIS_MEM_ID
                                               select new
                                               {
                                                   RoleId = mm.MEMBER_ROLE,
                                                   roleName = rm.ROLE_NAME,
                                                   Amount = mm.BALANCE
                                               }).FirstOrDefault();
                            decimal DisClosingAmt = tbl_accountDistributor.CLOSING;
                            //decimal SubAmt = ClosingAmt - Trans_Amt;                   
                            //decimal DisgetPer = (Trans_Amt * checkDiscomm.CommPercentage) / 100;
                            decimal CommDisAddClosing = tbl_accountDistributor.CLOSING + DisGapCommAmt;
                            long Dis_ID = long.Parse(DIS_MEM_ID.ToString());
                            TBL_ACCOUNTS objDisCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Dis_ID,
                                MEMBER_TYPE = Dismembtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "CR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = DisClosingAmt,
                                CLOSING = CommDisAddClosing,
                                REC_NO = 0,
                                COMM_AMT = DisGapCommAmt
                            };
                            db.TBL_ACCOUNTS.Add(objDisCommPer);
                            db.SaveChanges();
                        }
                        #endregion
                        //var SuperComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                        //                 join commslabMob in db.TBL_COMM_SLAB_UTILITY_RECHARGE
                        //                  on detailscom.BILLPAYMENT_SLAB equals commslabMob.SLAB_ID
                        //                 where detailscom.WHITE_LEVEL_ID == WHT_MEM_ID && detailscom.INTRODUCER_ID == SUP_MEM_ID && commslabMob.OPERATOR_CODE == serviceprovider
                        //                 select new
                        //                 {
                        //                     SLN = commslabMob.SLN,
                        //                     commPer = commslabMob.COMM_PERCENTAGE
                        //                 }).FirstOrDefault();
                        var SuperComm = (from detailscom in db.TBL_WHITE_LEVEL_COMMISSION_SLAB
                                         join commslabMob in db.TBL_COMM_SLAB_MOBILE_RECHARGE
                                          on detailscom.SLN equals commslabMob.SLAB_ID
                                         join mem in db.TBL_MASTER_MEMBER on detailscom.MEM_ID equals mem.UNDER_WHITE_LEVEL
                                         where commslabMob.OPERATOR_CODE == serviceprovider && mem.MEMBER_ROLE == 3
                                         select new
                                         {
                                             SLN = commslabMob.SLN,
                                             commPer = commslabMob.SUPER_COM_PER
                                         }).FirstOrDefault();
                        //decimal SupGapComm = decimal.Parse(SuperComm.commPer.ToString()) - decimal.Parse(DistributorComm.commPer.ToString());
                        decimal SupGapComm = decimal.Parse(SuperComm.commPer.ToString());
                        decimal SupGapCommAmt = (Trans_Amt * SupGapComm) / 100;
                        #region Super Commission
                        var tbl_accountSuper = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == SUP_MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_accountSuper != null)
                        {
                            var Supmembtype = (from mm in db.TBL_MASTER_MEMBER
                                               join
                                                   rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                               where mm.MEM_ID == SUP_MEM_ID
                                               select new
                                               {
                                                   RoleId = mm.MEMBER_ROLE,
                                                   roleName = rm.ROLE_NAME,
                                                   Amount = mm.BALANCE
                                               }).FirstOrDefault();
                            decimal SupClosingAmt = tbl_accountSuper.CLOSING;
                            decimal CommSupAddClosing = tbl_accountSuper.CLOSING + SupGapCommAmt;
                            long Sup_ID = long.Parse(SUP_MEM_ID.ToString());
                            TBL_ACCOUNTS objSupCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Sup_ID,
                                MEMBER_TYPE = Supmembtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "CR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = SupClosingAmt,
                                CLOSING = CommSupAddClosing,
                                REC_NO = 0,
                                COMM_AMT = SupGapCommAmt
                            };
                            db.TBL_ACCOUNTS.Add(objSupCommPer);
                            db.SaveChanges();
                        }
                        #endregion                        
                        var WhiteComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                                         join commslabMob in db.TBL_COMM_SLAB_UTILITY_RECHARGE
                                          on detailscom.BILLPAYMENT_SLAB equals commslabMob.SLAB_ID
                                         where detailscom.WHITE_LEVEL_ID == WHT_MEM_ID && detailscom.INTRODUCER_ID == 0 && commslabMob.OPERATOR_CODE == serviceprovider
                                         select new
                                         {
                                             SLN = commslabMob.SLN,
                                             commPer = commslabMob.COMM_PERCENTAGE
                                         }).FirstOrDefault();
                        decimal WL_Com = decimal.Parse(MerchantComm.commPer.ToString()) + decimal.Parse(DistributorComm.commPer.ToString()) + decimal.Parse(SuperComm.commPer.ToString());
                        //decimal WTLGapComm = decimal.Parse(WhiteComm.commPer.ToString()) - decimal.Parse(SuperComm.commPer.ToString());
                        decimal WTLGapComm = decimal.Parse(WhiteComm.commPer.ToString()) - WL_Com;
                        decimal WTLGapCommAmt = (Trans_Amt * WTLGapComm) / 100;
                        #region White level Commission payment Structure
                        var tbl_accountWhiteLevel = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == WHT_MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_accountWhiteLevel != null)
                        {
                            var WLmembtype = (from mm in db.TBL_MASTER_MEMBER
                                              join
                                                  rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                              where mm.MEM_ID == WHT_MEM_ID
                                              select new
                                              {
                                                  RoleId = mm.MEMBER_ROLE,
                                                  roleName = rm.ROLE_NAME,
                                                  Amount = mm.BALANCE
                                              }).FirstOrDefault();
                            decimal WLClosingAmt = tbl_accountWhiteLevel.CLOSING;
                            //decimal SubAmt = ClosingAmt - Trans_Amt;                  
                            decimal CommWLAddClosing = tbl_accountWhiteLevel.CLOSING + WTLGapCommAmt;
                            long WL_ID = long.Parse(WHT_MEM_ID.ToString());
                            TBL_ACCOUNTS objWLCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = WL_ID,
                                MEMBER_TYPE = WLmembtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "CR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = WLClosingAmt,
                                CLOSING = CommWLAddClosing,
                                REC_NO = 0,
                                COMM_AMT = WTLGapCommAmt
                            };
                            db.TBL_ACCOUNTS.Add(objWLCommPer);
                            db.SaveChanges();
                        }
                        #endregion
                        ContextTransaction.Commit();
                        return "Success";
                    }
                    else if (status == "DMR")
                    {
                        var MerchantComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                                            join commslabMob in db.TBL_COMM_SLAB_DMR_PAYMENT
                                            on detailscom.DMR_SLAB equals commslabMob.SLAB_ID
                                            where detailscom.WHITE_LEVEL_ID == DIS_MEM_ID && detailscom.INTRODUCER_ID == Mem_ID && commslabMob.DMT_TYPE == "DOMESTIC"
                                            select new
                                            {
                                                SLN = commslabMob.SLN,
                                                Slab_ID = commslabMob.SLAB_ID,
                                                Slab_From = commslabMob.SLAB_FROM,
                                                Slab_To = commslabMob.SLAB_TO,
                                                commPer = commslabMob.COMM_PERCENTAGE,

                                            }).ToList();
                        decimal commamt = 0;

                        foreach (var comslab in MerchantComm)
                        {
                            if (Trans_Amt >= comslab.Slab_From && Trans_Amt <= comslab.Slab_To)
                            {
                                commamt = comslab.commPer;

                            }
                        }

                        #region Retailer Commission                        
                        var membtype = (from mm in db.TBL_MASTER_MEMBER
                                        join
                                            rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                        where mm.MEM_ID == Mem_ID
                                        select new
                                        {
                                            RoleId = mm.MEMBER_ROLE,
                                            roleName = rm.ROLE_NAME,
                                            Amount = mm.BALANCE
                                        }).FirstOrDefault();
                        var tbl_account = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == Mem_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_account != null)
                        {
                            decimal ClosingAmt = tbl_account.CLOSING;
                            decimal SubAmt = ClosingAmt - Trans_Amt;
                            TBL_ACCOUNTS objaccnt = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Mem_ID,
                                MEMBER_TYPE = membtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "DR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "DMT",
                                OPENING = ClosingAmt,
                                CLOSING = SubAmt,
                                REC_NO = 0,
                                COMM_AMT = 0
                            };
                            db.TBL_ACCOUNTS.Add(objaccnt);
                            db.SaveChanges();
                            //decimal getPer = (Trans_Amt * MerchantComm.commPer) / 100;
                            decimal getPer = 0;

                            decimal CommAddClosing = SubAmt - commamt;
                            TBL_ACCOUNTS objCommPer = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Mem_ID,
                                MEMBER_TYPE = membtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "DR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = SubAmt,
                                CLOSING = CommAddClosing,
                                REC_NO = 0,
                                COMM_AMT = commamt
                            };
                            db.TBL_ACCOUNTS.Add(objCommPer);
                            db.SaveChanges();
                        }
                        #endregion

                        var DistributorComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                                               join commslabMob in db.TBL_COMM_SLAB_DMR_PAYMENT
                                               on detailscom.DMR_SLAB equals commslabMob.SLAB_ID
                                               where detailscom.WHITE_LEVEL_ID == SUP_MEM_ID && detailscom.INTRODUCER_ID == DIS_MEM_ID && commslabMob.DMT_TYPE == "DOMESTIC"
                                               select new
                                               {
                                                   SLN = commslabMob.SLN,
                                                   Slab_ID = commslabMob.SLAB_ID,
                                                   Slab_From = commslabMob.SLAB_FROM,
                                                   Slab_To = commslabMob.SLAB_TO,
                                                   commPer = commslabMob.COMM_PERCENTAGE,
                                               }).ToList();
                        decimal DistributorDMRComm = 0;
                        foreach (var comslab in DistributorComm)
                        {
                            if (Trans_Amt >= comslab.Slab_From && Trans_Amt <= comslab.Slab_To)
                            {
                                DistributorDMRComm = comslab.commPer;
                            }
                        }

                        decimal DmrDisgapcomm = 0;
                        DmrDisgapcomm = commamt - DistributorDMRComm;

                        #region Distributor DMR Commission                        
                        var DIsmembtype = (from mm in db.TBL_MASTER_MEMBER
                                           join
                                               rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                           where mm.MEM_ID == DIS_MEM_ID
                                           select new
                                           {
                                               RoleId = mm.MEMBER_ROLE,
                                               roleName = rm.ROLE_NAME,
                                               Amount = mm.BALANCE
                                           }).FirstOrDefault();
                        var tbl_accountDMRDis = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == DIS_MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_accountDMRDis != null)
                        {
                            decimal DMRDISClosingAmt = tbl_accountDMRDis.CLOSING;
                            //decimal DMRDISSubAmt = DMRDISClosingAmt - Trans_Amt;

                            decimal getPer = 0;

                            decimal DMRDisCommAddClosing = DMRDISClosingAmt - DmrDisgapcomm;
                            long dis_idval = long.Parse(DIS_MEM_ID.ToString());
                            TBL_ACCOUNTS objCommPerDis = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = dis_idval,
                                MEMBER_TYPE = DIsmembtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "DR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = DMRDISClosingAmt,
                                CLOSING = DMRDisCommAddClosing,
                                REC_NO = 0,
                                COMM_AMT = DmrDisgapcomm
                            };
                            db.TBL_ACCOUNTS.Add(objCommPerDis);
                            db.SaveChanges();
                        }
                        #endregion



                        var SuperComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                                         join commslabMob in db.TBL_COMM_SLAB_DMR_PAYMENT
                                          on detailscom.DMR_SLAB equals commslabMob.SLAB_ID
                                         where detailscom.WHITE_LEVEL_ID == WHT_MEM_ID && detailscom.INTRODUCER_ID == SUP_MEM_ID && commslabMob.DMT_TYPE == "DOMESTIC"
                                         select new
                                         {
                                             SLN = commslabMob.SLN,
                                             Slab_ID = commslabMob.SLAB_ID,
                                             Slab_From = commslabMob.SLAB_FROM,
                                             Slab_To = commslabMob.SLAB_TO,
                                             commPer = commslabMob.COMM_PERCENTAGE,

                                         }).ToList();
                        decimal SuperDMRComm = 0;
                        foreach (var comslab in SuperComm)
                        {
                            if (Trans_Amt >= comslab.Slab_From && Trans_Amt <= comslab.Slab_To)
                            {
                                SuperDMRComm = comslab.commPer;
                            }
                        }
                        decimal DmrSUPgapcomm = 0;
                        DmrSUPgapcomm = DistributorDMRComm - SuperDMRComm;
                        #region Super DMR Commission                        
                        var Supmembtype = (from mm in db.TBL_MASTER_MEMBER
                                           join
                                               rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                           where mm.MEM_ID == SUP_MEM_ID
                                           select new
                                           {
                                               RoleId = mm.MEMBER_ROLE,
                                               roleName = rm.ROLE_NAME,
                                               Amount = mm.BALANCE
                                           }).FirstOrDefault();
                        var tbl_accountSupDis = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == SUP_MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_accountSupDis != null)
                        {
                            decimal DMRSupClosingAmt = tbl_accountSupDis.CLOSING;
                            //decimal DMRDISSubAmt = DMRDISClosingAmt - Trans_Amt;

                            decimal getPer = 0;

                            decimal DMRSUPCommAddClosing = DMRSupClosingAmt - DmrSUPgapcomm;
                            long sup_idval = long.Parse(SUP_MEM_ID.ToString());
                            TBL_ACCOUNTS objCommPerSup = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = sup_idval,
                                MEMBER_TYPE = Supmembtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "DR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = DMRSupClosingAmt,
                                CLOSING = DMRSUPCommAddClosing,
                                REC_NO = 0,
                                COMM_AMT = DmrSUPgapcomm
                            };
                            db.TBL_ACCOUNTS.Add(objCommPerSup);
                            db.SaveChanges();
                        }
                        #endregion



                        var WhiteComm = (from detailscom in db.TBL_DETAILS_MEMBER_COMMISSION_SLAB
                                         join commslabMob in db.TBL_COMM_SLAB_DMR_PAYMENT
                                          on detailscom.DMR_SLAB equals commslabMob.SLAB_ID
                                         where detailscom.WHITE_LEVEL_ID == WHT_MEM_ID && detailscom.INTRODUCER_ID == 0 && commslabMob.DMT_TYPE == "DOMESTIC"
                                         select new
                                         {
                                             SLN = commslabMob.SLN,
                                             Slab_ID = commslabMob.SLAB_ID,
                                             Slab_From = commslabMob.SLAB_FROM,
                                             Slab_To = commslabMob.SLAB_TO,
                                             commPer = commslabMob.COMM_PERCENTAGE,

                                         }).ToList();

                        decimal WhiteDMRComm = 0;
                        foreach (var comslab in WhiteComm)
                        {
                            if (Trans_Amt >= comslab.Slab_From && Trans_Amt <= comslab.Slab_To)
                            {
                                WhiteDMRComm = comslab.commPer;
                            }
                        }
                        decimal DmrWhitegapcomm = 0;
                        DmrWhitegapcomm = SuperDMRComm - WhiteDMRComm;
                        #region White DMR Commission                        
                        var Whitemembtype = (from mm in db.TBL_MASTER_MEMBER
                                             join
                                                 rm in db.TBL_MASTER_MEMBER_ROLE on mm.MEMBER_ROLE equals rm.ROLE_ID
                                             where mm.MEM_ID == WHT_MEM_ID
                                             select new
                                             {
                                                 RoleId = mm.MEMBER_ROLE,
                                                 roleName = rm.ROLE_NAME,
                                                 Amount = mm.BALANCE
                                             }).FirstOrDefault();
                        var tbl_accountWhiteDis = db.TBL_ACCOUNTS.Where(z => z.MEM_ID == WHT_MEM_ID).OrderByDescending(z => z.TRANSACTION_TIME).FirstOrDefault();
                        if (tbl_accountWhiteDis != null)
                        {
                            decimal DMRWgiteClosingAmt = tbl_accountWhiteDis.CLOSING;
                            //decimal DMRDISSubAmt = DMRDISClosingAmt - Trans_Amt;

                            decimal getPer = 0;

                            decimal DMRWhiteCommAddClosing = DMRWgiteClosingAmt - DmrWhitegapcomm;
                            long Wht_idval = long.Parse(WHT_MEM_ID.ToString());
                            TBL_ACCOUNTS objCommPerWL = new TBL_ACCOUNTS()
                            {
                                API_ID = 0,
                                MEM_ID = Wht_idval,
                                MEMBER_TYPE = Whitemembtype.roleName,
                                TRANSACTION_TYPE = rechargeType,
                                TRANSACTION_DATE = System.DateTime.Now,
                                TRANSACTION_TIME = DateTime.Now,
                                DR_CR = "DR",
                                AMOUNT = Trans_Amt,
                                NARRATION = "Commission",
                                OPENING = DMRWgiteClosingAmt,
                                CLOSING = DMRWhiteCommAddClosing,
                                REC_NO = 0,
                                COMM_AMT = DmrWhitegapcomm
                            };
                            db.TBL_ACCOUNTS.Add(objCommPerWL);
                            db.SaveChanges();
                        }
                        #endregion
                        ContextTransaction.Commit();
                        return "Success";
                    }
                }
                catch (Exception ex)
                {
                    return "Fail";
                    ContextTransaction.Rollback();
                    throw ex;
                }
            }
            return "";
        }
        #endregion
    }

}