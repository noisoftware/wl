using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WHITELABEL.Web.Helper
{
    public static class InstantPayApi
    {
        public static string root = "https://www.instantpay.in";
        public static string token = "a1835840ab7df82c4b44947999e4f9cb";

        //public static string ApiUrl = " https://services.apiscript.in/rechargetest";
        public static string ApiUrl = " https://services.apiscript.in/recharge";

        //Encode token
        public static string ApiToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.IntcIlRpbWVTdGFtcFwiOlwiMTU2NDA1NjI0OFwiLFwiRW1haWxJRFwiOlwiam95ZGVlcG5haWhhdGkxOTg4QGdtYWlsLmNvbVwifSI.aw7Wr03NAWnVXWm9r-RPPsCccqwf9g_L65QtHdqIEBw";
        public static string ApiUserName = "APIWE1536338";
        public static string ApiPassword = "185701";

        public static class PaymentAPI
        {
            public static string GetOperatorCode(string OperatorName)
            {
                string OperatorCode = "";
                switch (OperatorName.ToUpper())
                {
                    case "AIRTEL":
                        OperatorCode = "MA";
                        break;
                    case "AIRTEL*":
                        OperatorCode = "MAL";
                        break;
                    case "IDEA":
                        OperatorCode = "MI";
                        break;
                    case "IDEA*":
                        OperatorCode = "MIL";
                        break;
                    case "VODAFONE":
                        OperatorCode = "MV";
                        break;
                    case "VODAFONE*":
                        OperatorCode = "MVL";
                        break;
                    case "BSNL TOPUP":
                        OperatorCode = "MBT";
                        break;
                    case "BSNL STV":
                        OperatorCode = "MBS";
                        break;
                    case "BSNL TOPUP*":
                        OperatorCode = "MBTL";
                        break;
                    case "BSNL STV*":
                        OperatorCode = "MBSL";
                        break;
                    case "AIRCEL":
                        OperatorCode = "MAC";
                        break;
                    case "AIRCEL*":
                        OperatorCode = "MACL";
                        break;
                    case "DOCOMO":
                        OperatorCode = "MD";
                        break;
                    case "DOCOMO SPECIAL":
                        OperatorCode = "MDS";
                        break;
                    case "DOCOMO*":
                        OperatorCode = "MDL";
                        break;
                    case "DOCOMO SPECIAL*":
                        OperatorCode = "MDSL";
                        break;
                    case "MTNL TOPUP":
                        OperatorCode = "MMT";
                        break;
                    case "MTNL STV":
                        OperatorCode = "MMS";
                        break;
                    case "RELIANCE JIO":
                        OperatorCode = "MJ";
                        break;
                    case "JIO":
                        OperatorCode = "MJ";
                        break;
                    case "JIO*":
                        OperatorCode = "MJL";
                        break;
                    case "TATA INDICOM":
                        OperatorCode = "MTI";
                        break;
                    default:
                        OperatorCode = "";
                        break;
                }
                return OperatorCode;
            }

            public static dynamic RechargeApiRequest(string OperatorName, string MobileNo, string Amount, string ClientId)
            {
                try
                {
                    string operatorcode = GetOperatorCode(OperatorName);
                    if (operatorcode != "")
                    {
                        string APIMsg = string.Empty;
                        string url = ApiUrl + "/api?username=" + ApiUserName + "&pwd=" + ApiPassword + "&operatorcode="+ operatorcode + "&number="+ MobileNo + "&amount="+ Amount + "&client_id=12348&token=" + ApiToken;
                        var res = GetResponse(url, "POST", new Dictionary<string, string>());
                        return res;
                    }
                    else
                    {
                        return "Service is not Available!";
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            public static dynamic TransactionStatus(string recharge_id)
            {
                try
                {
                    string APIMsg = string.Empty;
                    string url = ApiUrl + "/status?username=" + ApiUserName + "&pwd=" + ApiPassword + "&recharge_id=" + recharge_id + "&token=" + ApiToken;
                    //string url = TestApiUrl + "/status?username=" + ApiUserName + "&pwd=" + ApiPassword + "&client_id=12348&token=" + ApiToken;
                    var res = GetResponse(url, "POST", new Dictionary<string, string>());
                    return res;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            public static dynamic GetAccountBalance()
            {
                try
                {
                    string APIMsg = string.Empty;
                    string url = ApiUrl + "/balance? username =" + ApiUserName + "&pwd=" + ApiPassword + "&token=" + ApiToken;
                    var res = GetResponse(url, "POST", new Dictionary<string, string>());
                    return res;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }



            public static string Validation(string agentid, string amount, string spkey, string account)
            {
                try
                {
                    string transactionstatus = string.Empty;
                    var listinfo = "";
                    string url = root + "/ws/api/transaction?format=json&token=" + token + "&agentid=" + agentid + "&amount=" + amount + "&spkey=" + spkey + "&account=" + account + "&mode=VALIDATE&optional1={{optional1}}&optional2={{optional2}}&optional3={{optional3}}&optional4={{optional4}}&optional5={{optional5}}&optional6={{optional6}}&optional7={{optional7}}&optional8={{optional8}}&optional9={{optional9}}&outletid={{outletid}}&endpointip={{customer_ip}}&customermobile={{customermobile}}&paymentmode={{paymentmode}}&paymentchannel={{paymentchannel}}";
                    var res = GetResponse(url, "GET", new Dictionary<string, string>());
                    string errorcode = res.ipay_errorcode;
                    string des = res.ipay_errordesc;
                    if (errorcode == "TXN")
                    {
                        transactionstatus = errorcode;
                    }
                    else
                    {
                        transactionstatus = InstantPayError.GetError(errorcode);
                    }
                    return transactionstatus;
                    //if(res.)
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            public static dynamic Payment(string agentid, string amount, string spkey, string account)
            {
                try
                {
                    string APIMsg = string.Empty;
                    string url = root + "/ws/api/transaction?format=json&token=" + token + "&spkey=" + spkey + "&agentid=" + agentid + "&amount=" + amount + "&account=" + account + "&optional1={{optional1}}&optional2={{optional2}}&optional3={{optional3}}&optional4={{optional4}}&optional5={{optional5}}&optional6={{optional6}}&optional7={{optional7}}&optional8={{optional8}}&optional9={{optional9}}&outletid={{outletid}}&endpointip={{customer_ip}}&customermobile={{customermobile}}&paymentmode={{paymentmode}}&paymentchannel={{paymentchannel}}";
                    var res = GetResponse(url, "GET", new Dictionary<string, string>());
                    string errorcode = res.res_code;//res.res_code;
                    string status = res.status;
                    if (errorcode == "TXN")
                    {
                        //APIMsg = errorcode;
                        string ipay_id = res.ipay_id;
                        //APIMsg = errorcode;
                        return res;
                    }
                    else
                    {
                        //APIMsg = InstantPayError.GetError(res.ipay_errorcode.Value);
                        return res;
                    }
                    //return APIMsg;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            public static dynamic StatusCheck(string agentid)
            {
                try
                {
                    string APIMsg = string.Empty;
                    string url = root + "/ws/api/getMIS?format=json&token=" + token + "&agentid=" + agentid;
                    var res = GetResponse(url, "GET", new Dictionary<string, string>());
                    string errorcode = res.res_code;
                    if (errorcode == "TXN")
                    {
                        //APIMsg = errorcode;
                        return res;
                    }
                    else
                    {
                        //APIMsg = InstantPayError.GetError(errorcode);
                        return res;
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            public static dynamic ServiceProviderDetails(string spkey)
            {
                try
                {
                    string url = root + "/ws/api/serviceproviders?token=" + token + "&spkey=" + spkey + "&format=json";
                    var res = GetResponse(url, "GET", new Dictionary<string, string>());
                    string errorcode = res.ipay_errorcode;
                    if (errorcode == "TXN")
                    {
                        return res;
                    }
                    else
                    {
                        return InstantPayError.GetError(errorcode);
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }
        public static class ElectricityPaymentAPI
        {
            public static string ElectricityValidation(string agentid, string amount, string Mobile, string spkey, string account, string pin, string outletid)
            {
                try
                {
                    string transactionstatus = string.Empty;
                    var listinfo = "";
                    string url = root + "/ws/api/transaction?format=json&token=" + token + "&agentid=" + agentid + "&amount=" + amount + "&spkey=" + spkey + "&account=" + account + "&mode=VALIDATE&optional1={{optional1}}&optional2={{optional2}}&optional3={{optional3}}&optional4={{optional4}}&optional5={{optional5}}&optional6={{optional6}}&optional7={{optional7}}&optional8=remarks&optional9=" + pin + "&outletid=" + outletid + "&endpointip={{customer_ip}}&customermobile=" + Mobile + "&paymentmode=CASH&paymentchannel=AGT";
                    var res = GetResponse(url, "GET", new Dictionary<string, string>());
                    string errorcode = res.ipay_errorcode;
                    string des = res.ipay_errordesc;
                    if (errorcode == "TXN")
                    {
                        transactionstatus = errorcode;
                    }
                    else
                    {
                        transactionstatus = InstantPayError.GetError(errorcode);
                    }
                    return transactionstatus;
                    //if(res.)
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            public static dynamic ElectricityPayment(string agentid, string amount, string Mobile, string spkey, string account, string pin, string outletid)
            {
                try
                {
                    string APIMsg = string.Empty;
                    string url = root + "/ws/api/transaction?format=json&token=" + token + "&spkey=" + spkey + "&agentid=" + agentid + "&amount=" + amount + "&account=" + account + "&optional1={{optional1}}&optional2={{optional2}}&optional3={{optional3}}&optional4={{optional4}}&optional5={{optional5}}&optional6={{optional6}}&optional7={{optional7}}&optional8={{optional8}}&optional9=" + pin + "&outletid=" + outletid + "&endpointip={{customer_ip}}&customermobile=" + Mobile + "&paymentmode={{paymentmode}}&paymentchannel={{paymentchannel}}";
                    var res = GetResponse(url, "GET", new Dictionary<string, string>());
                    string errorcode = res.res_code;//res.res_code;
                    string status = res.status;
                    if (errorcode == "TXN")
                    {
                        //APIMsg = errorcode;
                        string ipay_id = res.ipay_id;
                        //APIMsg = errorcode;
                        return res;
                    }
                    else
                    {
                        //APIMsg = InstantPayError.GetError(res.ipay_errorcode.Value);
                        return res;
                    }
                    //return APIMsg;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            public static dynamic ElectricityStatusCheck(string agentid)
            {
                try
                {
                    string APIMsg = string.Empty;
                    string url = root + "/ws/api/getMIS?format=json&token=" + token + "&agentid=" + agentid;
                    var res = GetResponse(url, "GET", new Dictionary<string, string>());
                    string errorcode = res.res_code;
                    if (errorcode == "TXN")
                    {
                        //APIMsg = errorcode;
                        return res;
                    }
                    else
                    {
                        //APIMsg = InstantPayError.GetError(errorcode);
                        return res;
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            public static dynamic ElectricityServiceProviderDetails(string spkey)
            {
                try
                {
                    string url = root + "/ws/api/serviceproviders?token=" + token + "&spkey=" + spkey + "&format=json";
                    var res = GetResponse(url, "GET", new Dictionary<string, string>());
                    string errorcode = res.ipay_errorcode;
                    if (errorcode == "TXN")
                    {
                        return res;
                    }
                    else
                    {
                        return InstantPayError.GetError(errorcode);
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        public static class MoneyTransferAPI
        {
            public static dynamic RemitterDetails(string mobile)
            {
                string url = root + "/ws/dmi/remitter_details";
                var param = new Dictionary<string, dynamic> {
                    {
                        "token", token
                    },
                    {
                        "request", new Dictionary<string, string> {
                            { "mobile", mobile}
                        }

                    }
                };
                var res = GetResponse(url, "POST", param);
                if (res.statuscode.Value == "TXN")
                {
                    return res;
                }
                else
                {
                    //return InstantPayError.GetError(res.statuscode.Value);
                    return res;
                }
            }
            public static string RemitterID = string.Empty;
            public static dynamic RemitterRegistration(string mobile, string name, string pincode)
            {
                string url = root + "/ws/dmi/remitter";
                var param = new Dictionary<string, dynamic> {
                    {
                        "token", token
                    },
                    {
                        "request", new Dictionary<string, string> {
                            { "mobile" , mobile },
                            { "name" , name },
                            { "pincode" , pincode }
                        }
                    }
                };
                var res = GetResponse(url, "POST", param);
                if (res.statuscode.Value == "TXN")
                {
                    RemitterID = res.data.remitter.id;
                    return res;
                }
                else
                {
                    //return InstantPayError.GetError(res.statuscode.Value);
                    return res;
                }
            }
            public static dynamic BeneficiaryRegistration(string remitterid, string mobile, string name, string ifsc, string account)
            {
                string url = root + "/ws/dmi/beneficiary_register";
                var param = new Dictionary<string, dynamic> {
                    {
                        "token", token
                    },
                    {
                        "request", new Dictionary<string, string> {
                            { "remitterid",remitterid},
                            { "mobile" , mobile },
                            { "name" , name },
                            {"ifsc",ifsc },
                            { "account",account}
                        }
                    }
                };
                var res = GetResponse(url, "POST", param);
                if (res.statuscode.Value == "TXN")
                {
                    return res;
                }
                else
                {
                    //return InstantPayError.GetError(res.statuscode.Value);
                    return res;
                }
            }
            public static dynamic BeneficiaryRegistrationResendOTP(string remitterid, string beneficiaryid)
            {
                try
                {
                    string url = root + "/ws/dmi/beneficiary_resend_otp";
                    var param = new Dictionary<string, dynamic> {
                    {
                        "token", token
                    },
                    {
                        "request", new Dictionary<string, string> {
                             { "remitterid",remitterid},
                             { "beneficiaryid",beneficiaryid}
                        }
                    }
                };
                    var res = GetResponse(url, "POST", param);
                    if (res.statuscode.Value == "TXN")
                    {
                        return res;
                    }
                    else
                    {
                        //return InstantPayError.GetError(res.statuscode.Value);
                        return res;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            public static dynamic BeneficiaryRegistrationValidate(string remitterid, string beneficiaryid, string otp)
            {
                try
                {
                    string url = root + "/ws/dmi/beneficiary_register_validate";
                    var param = new Dictionary<string, dynamic> {
                    {
                        "token", token
                    },
                    {
                        "request", new Dictionary<string, string> {
                             { "remitterid",remitterid},
                             { "beneficiaryid",beneficiaryid},
                             {"otp", otp}
                        }
                    }
                };
                    var res = GetResponse(url, "POST", param);
                    if (res.statuscode.Value == "TXN")
                    {
                        return res;
                    }
                    else
                    {
                        //return InstantPayError.GetError(res.statuscode.Value);
                        return res;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            public static dynamic BeneficiaryAccountVerification(string remittermobile, string account, string ifsc, string agentid)
            {
                try
                {
                    string url = root + "/ws/imps/account_validate";
                    var param = new Dictionary<string, dynamic> {
                    {
                        "token", token
                    },
                    {
                        "request", new Dictionary<string, string> {
                             { "remittermobile",remittermobile},
                             { "account",account},
                             {"ifsc", ifsc},
                             {"agentid",agentid }
                        }
                    }
                };
                    var res = GetResponse(url, "POST", param);
                    if (res.statuscode.Value == "TXN")
                    {
                        return res;
                    }
                    else
                    {
                        //return InstantPayError.GetError(res.statuscode.Value);
                        return res;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            public static dynamic BeneficiaryDelete(string remitterid, string beneficiaryid)
            {
                try
                {
                    string url = root + "/ws/dmi/beneficiary_remove";
                    var param = new Dictionary<string, dynamic> {
                    {
                        "token", token
                    },
                    {
                        "request", new Dictionary<string, string> {
                            { "beneficiaryid",beneficiaryid},
                            { "remitterid",remitterid}

                        }
                    }
                };
                    var res = GetResponse(url, "POST", param);
                    if (res.statuscode.Value == "TXN")
                    {
                        return res;
                    }
                    else
                    {
                        //return InstantPayError.GetError(res.statuscode.Value);
                        return res;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            public static dynamic BeneficiaryDeleteValidate(string remitterid, string beneficiaryid, string otp)
            {
                try
                {
                    string url = root + "/ws/dmi/beneficiary_remove_validate";
                    var param = new Dictionary<string, dynamic> {
                    {
                        "token", token
                    },
                    {
                        "request", new Dictionary<string, string> {
                            { "beneficiaryid",beneficiaryid},
                            { "remitterid",remitterid},
                            {"otp",otp }

                        }
                    }
                };
                    var res = GetResponse(url, "POST", param);
                    if (res.statuscode.Value == "TXN")
                    {
                        return res;
                    }
                    else
                    {
                        //return InstantPayError.GetError(res.statuscode.Value);
                        return res;
                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            public static dynamic FundTransfer(string remittermobile, string beneficiaryid, string agentid, string amount, string mode)
            {
                try
                {
                    string url = root + "/ws/dmi/transfer";
                    var param = new Dictionary<string, dynamic> {
                    {
                        "token", token
                    },
                    {
                        "request", new Dictionary<string, string> {
                            { "remittermobile",remittermobile},
                            { "beneficiaryid",beneficiaryid},
                            { "agentid",agentid},
                            { "amount",amount},
                            {"mode",mode }

                        }
                    }
                };
                    var res = GetResponse(url, "POST", param);
                    if (res.statuscode.Value == "TXN")
                    {
                        return res;
                    }
                    else
                    {
                        //return InstantPayError.GetError(res.statuscode.Value);
                        return res;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            public static dynamic FundTransferStatus(string ipayid)
            {
                try
                {
                    string url = root + "/ws/dmi/transfer_status";
                    var param = new Dictionary<string, dynamic> {
                    {
                        "token", token
                    },
                    {
                        "request", new Dictionary<string, string> {
                            { "ipayid",ipayid}

                        }
                    }
                };
                    var res = GetResponse(url, "POST", param);
                    if (res.statuscode.Value == "TXN")
                    {
                        return res;
                    }
                    else
                    {
                        //return InstantPayError.GetError(res.statuscode.Value);
                        return res;
                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            public static dynamic GetBankDetails(string account)
            {
                try
                {
                    string url = root + "/ws/dmi/bank_details";
                    var param = new Dictionary<string, dynamic> {
                    {
                        "token", token
                    },
                    {
                        "request", new Dictionary<string, string> {
                            { "account",account}

                        }
                    }
                };
                    var res = GetResponse(url, "POST", param);
                    if (res.statuscode.Value == "TXN")
                    {
                        return res;
                    }
                    else
                    {
                        return InstantPayError.GetError(res.statuscode.Value);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            public static dynamic RemitterKYC(string Mobile, string Aadhaar)
            {
                try
                {
                    string url = root + "/ws/kyc/remitter";
                    var param = new Dictionary<string, dynamic> {
                    {
                        "token", token
                    },
                    {
                        "request", new Dictionary<string, string> {
                            { "mobile_number",Mobile},
                            { "aadhaar_number",Aadhaar}

                        }
                    }
                };
                    var res = GetResponse(url, "POST", param);
                    if (res.statuscode.Value == "TXN")
                    {
                        return res;
                    }
                    else
                    {
                        return InstantPayError.GetError(res.statuscode.Value);
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            }

            public static dynamic RemitterKYCValidate(string otp, string mobile_number, string aadhaar_number, string aadhaar_card, string filename)
            {
                try
                {
                    string url = root + "/ws/kyc/remitter/validate";
                    var param = new Dictionary<string, dynamic> {
                    {
                        "token", token
                    },
                    {
                        "request", new Dictionary<string, string> {
                            { "otp",otp},
                            { "mobile_number",mobile_number},
                            { "aadhaar_number",aadhaar_number},
                            { "aadhaar_card",aadhaar_card},
                            { "filename",filename}
                        }
                    }
                };
                    var res = GetResponse(url, "POST", param);
                    if (res.statuscode.Value == "TXN")
                    {
                        return res;
                    }
                    else
                    {
                        return InstantPayError.GetError(res.statuscode.Value);
                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }

            public static dynamic RemitterKYCV2(dynamic Request)
            {

                //string Url = SIMTIKFRONT.Web.Domain.AppSettings.DwollaURL + "/customers/" + DwollaId + "/documents";
                string Url = root + "/ws/kyc/remitter/validate_v2";
                HttpContent fileStreamContent = new StreamContent(Request.InputStream);
                using (HttpClient client = ClientHelper.GetDocClient())
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        Uri uploadUrlUri = new Uri(Url);

                        //formData.Add(new StringContent("idCard"), "documentType");

                        formData.Add(fileStreamContent, "token", token);
                        formData.Add(fileStreamContent, "aadhaar_card", "");
                        formData.Add(fileStreamContent, "otp", "");
                        formData.Add(fileStreamContent, "mobile_number", "9903116214");
                        formData.Add(fileStreamContent, "aadhaar_number", "9903116214");

                        var response = client.PostAsync(uploadUrlUri, formData).Result;

                        return Regex.Split(response.Headers.Location.ToString(), "/")[4];

                    }
                }


            }


        }

        private static dynamic GetResponse<T>(string url, string method, Dictionary<string, T> param)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                using (HttpClient client = ClientHelper.GetClient())
                {
                    switch (method.ToUpper())
                    {
                        case "GET":
                            {
                                response = client.GetAsync(url).Result;
                                break;
                            }
                        case "POST":
                            {
                                response = client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(param), Encoding.UTF8, "application/json")).Result;
                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException();
                            }
                    }
                    response.EnsureSuccessStatusCode();
                    return string.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result) ? response : JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception e)
            {
                // Handle exception
                throw e;
            }
        }
        public static class ClientHelper
        {
            public static HttpClient GetClient()
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                return client;
            }
            public static HttpClient GetDocClient()
            {
                HttpClient Docclient = new HttpClient();
                Docclient.DefaultRequestHeaders.Accept.Clear();
                Docclient.DefaultRequestHeaders.Add("Accept", "application/json");
                Docclient.DefaultRequestHeaders.Add("Content-Type", "application/json");
                Docclient.DefaultRequestHeaders.Add("ContentType", "multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW");
                return Docclient;
            }
        }

        public static class OutletApi
        {
            public static dynamic VerifyOutlet(string mobile)
            {
                string url = root + "/ws/outlet/sendOTP";
                var param = new Dictionary<string, dynamic> {
                    {
                        "token", token
                    },
                    {
                        "request", new Dictionary<string, string> {
                            { "mobile", mobile}
                        }

                    }
                };
                var res = GetResponse(url, "POST", param);
                if (res.statuscode.Value == "TXN")
                {
                    return res;
                }
                else
                {
                    //return InstantPayError.GetError(res.statuscode.Value);
                    return res;
                }
            }
            public static dynamic RegisterOutlet(string mobile, string otp, string email, string company, string name, string address, string pincode)
            {
                string url = root + "/ws/outlet/register";
                var param = new Dictionary<string, dynamic> {
                    {
                        "token", token
                    },
                    {
                        "request", new Dictionary<string, string> {
                            { "mobile", mobile},
                            { "otp" , otp },
                            { "email" , email },
                            {"company",company },
                            { "name",name},
                            { "address",address},
                            { "pincode",pincode}
                        }

                    }
                };
                var res = GetResponse(url, "POST", param);
                if (res.statuscode.Value == "TXN")
                {
                    return res;
                }
                else
                {
                    //return InstantPayError.GetError(res.statuscode.Value);
                    return res;
                }
            }
        }

               

    }
}