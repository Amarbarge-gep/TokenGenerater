using Gep.Cumulus.Encryption;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using RestSharp;

namespace GetToken
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.Write("Auto Generate (y/n): ");
            var skip = Console.ReadLine();
            string username = "";
            string password = "";
            string bpcCode = "";
            string partnerCode = "";
            if (skip != "y")
            {
                Console.Write("User Name: ");
                username = Console.ReadLine();
                Console.Write("Password: ");
                password = Console.ReadLine();
                Console.Write("BPC: ");
                bpcCode = Console.ReadLine();
                Console.Write("Partner Code: ");
                partnerCode = Console.ReadLine();
            }
            username = SecurityEncryptionUtility.AESEncrypt(username != null && username.Length > 0
                ? username
                : "petronas.admin@gep.com");
            password = SecurityEncryptionUtility.AESEncrypt(password != null && password.Length > 0
                ? password
                : "MZa8!TRjE(S");
            bpcCode = SecurityEncryptionUtility.AESEncrypt(bpcCode != null && bpcCode.Length > 0
                ? bpcCode
                : "70021790");
            partnerCode =
                SecurityEncryptionUtility.AESEncrypt(partnerCode != null && partnerCode.Length > 0
                    ? partnerCode
                    : "0");
            while (true)
            {
                var client = new RestSharp.RestClient("https://smartdev-sts.gep.com/SmartAuth/GetJWTForGEPAutomationSuite");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", "{\r\n   \"gaun\":\"" + username + "\",\r\n   \"gapw\":\"" + password + "\",\r\n   \"bpc\":\"" + bpcCode + "\",\r\n   \"partnerCode\":\"" + partnerCode + "\",\r\n   \"smartFlag\":false\r\n}\r\n", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                var objResp = JsonConvert.DeserializeObject<ResponseObj>(response.Content);
                if (objResp.encryptedToken != null)
                {
                    Console.WriteLine(objResp.encryptedToken);
                    Clipboard.SetText(objResp.encryptedToken);
                    StringBuilder stb = new StringBuilder();
                    string filePath = File.ReadAllText(Directory.GetCurrentDirectory() + "\\filepath.txt");
                    //@"C:\GIT\nexxe_plugin\nexxe.bootstrapper_1\nexxe.bootstrapper\Nexxe.App.Host\app-host\src\assets\mockData.js";
                    if (filePath.Length > 0 && File.Exists(filePath))
                    {
                        foreach (var el in File.ReadLines(filePath).Skip(1))
                        {
                            stb.AppendLine(el.Replace("var userInfo =", "").Replace(";", ""));
                        }

                        Newtonsoft.Json.Linq.JObject myJsonObject =
                            ((Newtonsoft.Json.Linq.JObject) JsonConvert.DeserializeObject(stb.ToString()));
                        myJsonObject["Token"] =
                            objResp.encryptedToken;
                        StringBuilder fstb = new StringBuilder();
                        foreach (var el in File
                            .ReadLines(filePath)
                            .Take(1))
                        {
                            fstb.AppendLine(el);
                        }

                        fstb.AppendLine("var userInfo =" +
                                        JsonConvert.SerializeObject(myJsonObject, Formatting.Indented) + ";");
                        File.WriteAllText(filePath, fstb.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("API is not returning the token");
                }

                Console.ReadKey();
            }
        }
    }
    public class ResponseObj
    {
        public string authcode { get; set; }
        public string encryptedToken { get; set; }

    }
}
