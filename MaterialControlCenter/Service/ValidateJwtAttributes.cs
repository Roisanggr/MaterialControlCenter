using MaterialControlCenter.Controllers;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;

namespace MaterialControlCenter.services
{
    public class ValidateJwtAttribute : ActionFilterAttribute
    {
        private string JWT;
        private readonly HttpClient _httpClient;
        public ValidateJwtAttribute()
        {
            _httpClient = new HttpClient();
            string hostName = System.Web.HttpContext.Current.Request.UserHostAddress;
            Debug.Print(hostName.ToString());
            JWT = GetJwtByPcName(hostName);
        }
        private string GetJwtByPcName(string hostName)
        {
            try
            {
                string apiUrl = "https://ptmi/SSO/api/getPcName";
                string requestBody = $"\"{hostName}\"";

                if (hostName == "::1")
                {
                    System.Net.IPHostEntry ipHostEntry = System.Net.Dns.GetHostEntry(hostName);
                    foreach (System.Net.IPAddress ip in ipHostEntry.AddressList)
                    {
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && ipHostEntry.HostName.Contains("corp.mattel.com"))
                        {
                            System.Net.IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(ip);
                            if (ipEntry.HostName.Contains("corp.mattel.com"))
                            {
                                requestBody = ip.ToString();
                                requestBody = $"\"{requestBody}\"";
                                break;
                            }
                        }
                    }
                }

                using (HttpClient httpClient = new HttpClient())
                {
                    using (HttpResponseMessage response = httpClient.PostAsync(apiUrl, new StringContent(requestBody, Encoding.UTF8, "application/json")).Result)
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            Debug.Print($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                            string errorContent = response.Content.ReadAsStringAsync().Result;
                            Debug.Print($"Error Content: {errorContent}");
                            return null;
                        }

                        string jsonResponse = response.Content.ReadAsStringAsync().Result;

                        dynamic jsonResponseObject = JsonConvert.DeserializeObject(jsonResponse);

                        if (jsonResponseObject != null && jsonResponseObject.Token != null)
                        {
                            string tokenValue = jsonResponseObject.Token;
                            Debug.Print("Token JWT yang diperoleh: " + tokenValue);
                            return tokenValue;
                        }
                        else
                        {
                            Debug.Print("Error: Token not found in API response.");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error: {ex.Message}");
                return null;
            }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string hostName = filterContext.HttpContext.Request.UserHostAddress;
            Debug.Print("🔍 Hostname: " + hostName);

            JWT = GetJwtByPcName(hostName); 
            Debug.Print("🛂 JWT yang diperoleh: " + (JWT ?? "null"));
            var controller = filterContext.Controller as BaseController;
            if (!IsJwtValid())
            {
                string currentUrl = filterContext.HttpContext.Request.Url?.AbsoluteUri ?? "/";
                string loginUrl = "https://ptmi/SSO/";
                filterContext.Result = new RedirectResult($"{loginUrl}?returnUrl={Uri.EscapeDataString(currentUrl)}");
                return;
            }

            filterContext.HttpContext.Items["JwtToken"] = JWT;
            if (!string.IsNullOrEmpty(JWT) && controller != null)
            {
                controller.LoadUserInfoFromJwt();
            }
            base.OnActionExecuting(filterContext);
        }

        public bool IsJwtValid()
        {
            if (string.IsNullOrEmpty(JWT))
            {
                Debug.Print("JWT tidak valid karena kosong atau null.");
                return false;
            }

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var token = handler.ReadJwtToken(JWT);
                Debug.Print("Token ValidUntil: " + token.ValidTo);
                Debug.Print("Waktu Sekarang UTC: " + DateTime.UtcNow);

                // Cek apakah token masih valid
                return token.ValidTo > DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Debug.Print("Error saat validasi JWT: " + ex.Message);
                return false;
            }
        }

    }
}
