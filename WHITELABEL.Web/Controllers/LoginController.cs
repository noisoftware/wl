using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WHITELABEL.Data;
using WHITELABEL.Web.Helper;
using WHITELABEL.Web.Models;

namespace WHITELABEL.Web.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index(string returnUrl)
        {
            SystemClass sclass = new SystemClass();
            string userID = sclass.GetLoggedUser();
            if (Url.IsLocalUrl(returnUrl) && !string.IsNullOrEmpty(returnUrl))
            {
                ViewBag.ReturnURL = returnUrl;
            }
            if (Session["UserId"] != null)
            {
                //Response.RedirectToRoute("Dashboard", "Index");
                string username = Session["UserId"].ToString();
                var db = new DBContext();
                var userinfo = db.TBL_MASTER_MEMBER.Where(x => x.UName == username).FirstOrDefault();
                
                if (userinfo.MEMBER_ROLE == 1)
                {
                    Response.Redirect(Url.Action("Index", "WhiteLevelAdmin", new { area = "Admin" }));
                }
                else if (userinfo.MEMBER_ROLE == null)
                {
                    Response.Redirect(Url.Action("Index", "WhiteLevelAdmin", new { area = "Admin" }));
                }
                else if (userinfo.UNDER_WHITE_LEVEL == null)
                {
                    Response.RedirectToRoute("Dashboard", "Index");
                }


                
            }
            LoginViewModel model = new LoginViewModel();
            if (Request.Cookies["Login"] != null)
            {
                model.Email = Request.Cookies["Login"].Values["EmailID"];
                model.Password = Request.Cookies["Login"].Values["Password"];
            }
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(LoginViewModel User, string ReturnURL = "")
        {
            SystemClass sclass = new SystemClass();
            string userID = sclass.GetLoggedUser();
            using (var db = new DBContext())
            {
                var GetUser = await db.TBL_AUTH_ADMIN_USERS.FirstOrDefaultAsync(x => x.USER_MOBILE == User.MEMBER_MOBILE);
                if (GetUser != null)
                {
                    if (!GetUser.ACTIVE_USER || GetUser.USER_PASSWORD_MD5 != User.Password)
                    {
                        ViewBag.Message = "Invalid Credential or Access Denied";
                        FormsAuthentication.SignOut();
                        return View();
                    }
                    else
                    {
                        Session["PowerAdminUserId"] = GetUser.USER_ID;
                        Session["PowerAdminUserName"] = GetUser.USER_NAME;
                        Session["UserType"] = "Power Admin";
                        HttpCookie AuthCookie;
                        System.Web.Security.FormsAuthentication.SetAuthCookie(GetUser.USER_NAME + "||" + Encrypt.EncryptMe(GetUser.USER_ID.ToString()), true);
                        AuthCookie = System.Web.Security.FormsAuthentication.GetAuthCookie(GetUser.USER_NAME + "||" + Encrypt.EncryptMe(GetUser.USER_ID.ToString()), true);
                        AuthCookie.Expires = DateTime.Now.Add(new TimeSpan(130, 0, 0, 0));
                        Response.Cookies.Add(AuthCookie);
                        return RedirectToAction("Index", "PowerAdminHome", new { area = "PowerAdmin" });
                    }
                }
                else
                {
                    var GetMember = await db.TBL_MASTER_MEMBER.SingleOrDefaultAsync(x => x.MEMBER_MOBILE == User.MEMBER_MOBILE && x.User_pwd == User.Password && x.ACTIVE_MEMBER == true);
                    if (GetMember != null)
                    {
                        if (GetMember.MEMBER_ROLE == 1)
                        {
                            if (GetMember.ACTIVE_MEMBER == false || GetMember.User_pwd != User.Password)
                            {
                                ViewBag.Message = "Invalid Credential or Access Denied";
                                FormsAuthentication.SignOut();
                                return View();
                            }
                            else
                            {
                                Session["WhiteLevelUserId"] = GetMember.MEM_ID;
                                Session["WhiteLevelUserName"] = GetMember.UName;
                                Session["UserType"] = "White Level";
                                HttpCookie AuthCookie;
                                System.Web.Security.FormsAuthentication.SetAuthCookie(GetMember.UName + "||" + Encrypt.EncryptMe(GetMember.MEM_ID.ToString()), true);
                                AuthCookie = System.Web.Security.FormsAuthentication.GetAuthCookie(GetMember.UName + "||" + Encrypt.EncryptMe(GetMember.MEM_ID.ToString()), true);
                                AuthCookie.Expires = DateTime.Now.Add(new TimeSpan(130, 0, 0, 0));
                                Response.Cookies.Add(AuthCookie);
                                //Response.Redirect(FormsAuthentication.GetRedirectUrl(GetUser.USER_NAME.ToString(), true));
                                return RedirectToAction("Index", "WhiteLevelAdmin", new { area = "Admin" });
                            }
                        }
                        else if (GetMember.MEMBER_ROLE == 2)
                        {
                            if (GetMember.ACTIVE_MEMBER == false || GetMember.User_pwd != User.Password)
                            {
                                ViewBag.Message = "Invalid Credential or Access Denied";
                                FormsAuthentication.SignOut();
                                return View();
                            }
                            else
                            {
                                Session["UserId"] = GetMember.MEM_ID;
                                Session["UserName"] = GetMember.UName;

                                HttpCookie AuthCookie;
                                System.Web.Security.FormsAuthentication.SetAuthCookie(GetMember.UName + "||" + Encrypt.EncryptMe(GetMember.MEM_ID.ToString()), true);
                                AuthCookie = System.Web.Security.FormsAuthentication.GetAuthCookie(GetMember.UName + "||" + Encrypt.EncryptMe(GetMember.MEM_ID.ToString()), true);
                                AuthCookie.Expires = DateTime.Now.Add(new TimeSpan(130, 0, 0, 0));
                                Response.Cookies.Add(AuthCookie);
                                return RedirectToAction("Index", "WhiteLevelAdmin", new { area = "Admin" });
                                //Response.Redirect(FormsAuthentication.GetRedirectUrl(GetUser.USER_NAME.ToString(), true));
                            }
                        }
                        else if (GetMember.MEMBER_ROLE == 3)
                        {
                            if (GetMember.ACTIVE_MEMBER == false || GetMember.User_pwd != User.Password)
                            {
                                ViewBag.Message = "Invalid Credential or Access Denied";
                                FormsAuthentication.SignOut();
                                return View();
                            }
                            else
                            {
                                Session["SuperDistributorId"] = GetMember.MEM_ID;
                                Session["SuperDistributorUserName"] = GetMember.UName;
                                Session["UserType"] = "Super Distributor";
                                HttpCookie AuthCookie;
                                System.Web.Security.FormsAuthentication.SetAuthCookie(GetMember.UName + "||" + Encrypt.EncryptMe(GetMember.MEM_ID.ToString()), true);
                                AuthCookie = System.Web.Security.FormsAuthentication.GetAuthCookie(GetMember.UName + "||" + Encrypt.EncryptMe(GetMember.MEM_ID.ToString()), true);
                                AuthCookie.Expires = DateTime.Now.Add(new TimeSpan(130, 0, 0, 0));
                                Response.Cookies.Add(AuthCookie);
                                return RedirectToAction("Index", "SuperDashboard", new { area = "Super" });
                                //Response.Redirect(FormsAuthentication.GetRedirectUrl(GetUser.USER_NAME.ToString(), true));
                            }
                        }
                        else if (GetMember.MEMBER_ROLE == 4)
                        {
                            if (GetMember.ACTIVE_MEMBER == false || GetMember.User_pwd != User.Password)
                            {
                                ViewBag.Message = "Invalid Credential or Access Denied";
                                FormsAuthentication.SignOut();
                                return View();
                            }
                            else
                            {
                                Session["DistributorUserId"] = GetMember.MEM_ID;
                                Session["DistributorUserName"] = GetMember.UName;
                                Session["UserType"] = "Distributor";
                                HttpCookie AuthCookie;
                                System.Web.Security.FormsAuthentication.SetAuthCookie(GetMember.UName + "||" + Encrypt.EncryptMe(GetMember.MEM_ID.ToString()), true);
                                AuthCookie = System.Web.Security.FormsAuthentication.GetAuthCookie(GetMember.UName + "||" + Encrypt.EncryptMe(GetMember.MEM_ID.ToString()), true);
                                AuthCookie.Expires = DateTime.Now.Add(new TimeSpan(130, 0, 0, 0));
                                Response.Cookies.Add(AuthCookie);
                                return RedirectToAction("Index", "DistributorDashboard", new { area = "Distributor" });
                                //Response.Redirect(FormsAuthentication.GetRedirectUrl(GetUser.USER_NAME.ToString(), true));
                            }
                        }
                        else if (GetMember.MEMBER_ROLE == 5)
                        {
                            if (GetMember.ACTIVE_MEMBER == false || GetMember.User_pwd != User.Password)
                            {
                                ViewBag.Message = "Invalid Credential or Access Denied";
                                FormsAuthentication.SignOut();
                                return View();
                            }
                            else
                            {
                                Session["MerchantUserId"] = GetMember.MEM_ID;
                                Session["MerchantUserName"] = GetMember.UName;
                                Session["UserType"] = "Merchant";
                                HttpCookie AuthCookie;
                                System.Web.Security.FormsAuthentication.SetAuthCookie(GetMember.UName + "||" + Encrypt.EncryptMe(GetMember.MEM_ID.ToString()), true);
                                AuthCookie = System.Web.Security.FormsAuthentication.GetAuthCookie(GetMember.UName + "||" + Encrypt.EncryptMe(GetMember.MEM_ID.ToString()), true);
                                AuthCookie.Expires = DateTime.Now.Add(new TimeSpan(130, 0, 0, 0));
                                Response.Cookies.Add(AuthCookie);
                                return RedirectToAction("Index", "MerchantDashboard", new { area = "Merchant" });
                                //Response.Redirect(FormsAuthentication.GetRedirectUrl(GetUser.USER_NAME.ToString(), true));
                            }
                        }
                        else
                        {
                            ViewBag.Message = "Invalid Credential or Access Denied";
                            return View();
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Invalid Credential or Access Denied";
                        return View();
                    }
                }
            }
        }

        [AllowAnonymous]
        public ActionResult LogOut()
        {
            if (Session["UserId"] != null)
            {

                FormsAuthentication.SignOut();
                Session["UserId"] = null;
                Session["UserName"] = null;
                Session.Clear();
                Session.Remove("UserId");
                Session.Remove("UserName");
                SystemClass sclass = new SystemClass();
                return RedirectToAction("Index", "Login");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

    }
}
