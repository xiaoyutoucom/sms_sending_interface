using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using Robin.Web.Mvc.Controllers;
using Microsoft.Extensions.Configuration;

namespace WebProxy
{
    public class UCSRestRequest
    {
        enum EBodyType : uint
        {
            EType_XML = 0,
            EType_JSON
        };

        public static IConfigurationRoot Config { get; set; }
        //服务地址
        private string m_restAddress = null;
        //服务地址端口
        private string m_restPort = null;
        //用户ID
        private string m_mainAccount = null;
        //用户token
        private string m_mainToken =null;
        //应用APPID
        private string m_appId = null;
        private bool m_isWriteLog = false;

        private EBodyType m_bodyType = EBodyType.EType_JSON;

        /// <summary>
        /// 服务器api版本
        /// </summary>
        const string softVer = "2014-06-30";

        /// <summary>
        /// 初始化函数
        /// </summary>
        /// <param name="serverIP">服务器地址</param>
        /// <param name="serverPort">服务器端口</param>
        /// <returns></returns>
        public bool init(string restAddress, string restPort)
        {
            this.m_restAddress = restAddress;
            this.m_restPort = restPort;

            if (m_restAddress == null || m_restAddress.Length < 0 || m_restPort == null || m_restPort.Length < 0 || Convert.ToInt32(m_restPort) < 0)
                return false;

            return true;
        }

        /// <summary>
        /// 设置主帐号信息
        /// </summary>
        /// <param name="accountSid">主帐号</param>
        /// <param name="accountToken">主帐号令牌</param>
        public void setAccount(string accountSid, string accountToken)
        {
            this.m_mainAccount = accountSid;
            this.m_mainToken = accountToken;
        }

        /// <summary>
        /// 设置应用ID
        /// </summary>
        /// <param name="appId">应用ID</param>
        public void setAppId(string appId)
        {
            this.m_appId = appId;
        }

        /// <summary>
        /// 日志开关
        /// </summary>
        /// <param name="enable">日志开关</param>
        public void enabeLog(bool enable)
        {
            this.m_isWriteLog = enable;
        }

        /// <summary>
        /// 获取日志路径
        /// </summary>
        /// <returns>日志路径</returns>
        public string GetLogPath()
        {
            string dllpath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            dllpath = dllpath.Substring(8, dllpath.Length - 8);    // 8是 file:// 的长度
            return System.IO.Path.GetDirectoryName(dllpath) + "\\log.txt";
        }



        public void CofigHelp()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();
            Config = config;
            //用户ID
            m_mainAccount = Config["AccountSid"];
            //用户token
             m_mainToken = Config["Token"];
            //应用APPID
            m_appId = Config["AppID"];
            m_restAddress = Config["restAddress"];
        }


    /// <summary>
    /// 发送短信
    /// </summary>
    /// <param name="to">短信接收端手机号码</param>
    /// <param name="templateId">短信模板ID</param>
    /// <param name="param">内容数据，用于替换模板中{数字}</param>
    /// <exception cref="ArgumentNullException">参数不能为空</exception>
    /// <exception cref="Exception"></exception>
    /// <returns>包体内容</returns>
    public string SendSMS(string to, string templateId, string param)
        {
            CofigHelp();
            if (to == null)
            {
                throw new ArgumentNullException("to");
            }

            if (templateId == null)
            {
                throw new ArgumentNullException("templateId");
            }

            try
            {
                string date = DateTime.Now.ToString("yyyyMMddHHmmss");

                // 构建URL内容
                string sigstr = MD5Encrypt(m_mainAccount + m_mainToken + date);
                string uriStr;
                string xml = (m_bodyType == EBodyType.EType_XML ? ".xml" : "");
                uriStr = string.Format("https://"+m_restAddress+"/{0}/Accounts/{1}/Messages/templateSMS{2}?sig={3}",  softVer, m_mainAccount, xml, sigstr);
                //uriStr = "https://open.ucpaas.com/ol/sms/sendsms";//服务地址、服务端口、软件版本、主账户（用户ID）、
                //https://api.ucpaas.com/{SoftVersion}/Accounts/{accountSid}/{function}/{operation}?sig={SigParameter}   //function:业务功能；operation：业务操作

                Uri address = new Uri(uriStr);

                WriteLog("SendSMS url = " + uriStr);

                // 创建网络请求  
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
                setCertificateValidationCallBack();

                // 构建Head
                request.Method = "POST";

                Encoding myEncoding = Encoding.GetEncoding("utf-8");
                byte[] myByte = myEncoding.GetBytes(m_mainAccount + ":" + date);
                string authStr = Convert.ToBase64String(myByte);
                request.Headers.Add("Authorization", authStr);


                // 构建Body
                StringBuilder data = new StringBuilder();

                if (m_bodyType == EBodyType.EType_XML)
                {
                    request.Accept = "application/xml";
                    request.ContentType = "application/xml;charset=utf-8";

                    data.Append("<?xml version='1.0' encoding='utf-8'?><templateSMS>");
                    data.Append("<appId>").Append(m_appId).Append("</appId>");
                    data.Append("<templateId>").Append(templateId).Append("</templateId>");
                    data.Append("<to>").Append(to).Append("</to>");
                    data.Append("<param>").Append(param).Append("</param>");
                    data.Append("</templateSMS>");
                }
                else
                {
                    request.Accept = "application/json";
                    request.ContentType = "application/json;charset=utf-8";

                    data.Append("{");
                    data.Append("\"templateSMS\":{");                   
                    //data.Append("\"token\":\"").Append(m_mainAccount).Append("\"");
                    //data.Append("\"sid\":\"").Append(m_mainAccount).Append("\"");
                    data.Append("\"appId\":\"").Append(m_appId).Append("\"");
                    data.Append(",\"templateId\":\"").Append(templateId).Append("\"");
                    data.Append(",\"to\":\"").Append(to).Append("\"");
                    data.Append(",\"param\":\"").Append(param).Append("\"");
                    data.Append("}}");
                }

                byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

                WriteLog("CreateSubAccount requestBody = " + data.ToString());

                // 开始请求
                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }

                // 获取请求
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseStr = reader.ReadToEnd();

                    WriteLog("CreateSubAccount responseBody = " + responseStr);

                    if (responseStr != null && responseStr.Length > 0)
                    {
                        return responseStr;
                    }
                }
                return null;
            }
            catch (Exception e)
            {

                throw e;
            }

        }

        /// <summary>
        /// 获取模板内容
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public string GetTemplateById(string templateId)
        {
            CofigHelp();
            if (templateId == null)
            {
                throw new ArgumentNullException("templateId");
            }

            string date = DateTime.Now.ToString("yyyyMMddHHmmss");
            string url = "https://"+ m_restAddress + "/ol/sms/getsmstemplate";

            Uri address = new Uri(url);
            HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;

            // 构建Head
            request.Method = "POST";

            //Encoding myEncoding = Encoding.GetEncoding("utf-8");
            //byte[] myByte = myEncoding.GetBytes("4188e9e2406eed67b837dae1e9f4c933:" + date);
            //string authStr = Convert.ToBase64String(myByte);
            //request.Headers.Add("Authorization", authStr);


            // 构建Body
            StringBuilder data = new StringBuilder();

            request.Accept = "application/json";
            request.ContentType = "application/json;charset=utf-8";

            data.Append("{");
            data.Append("\"sid\":\"").Append(m_mainAccount).Append("\"");
            data.Append(",\"token\":\"").Append(m_mainToken).Append("\"");
            data.Append(",\"appid\":\"").Append(m_appId).Append("\"");
            data.Append(",\"templateid\":\"").Append(templateId).Append("\"");
            data.Append("}");


            byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

            //WriteLog("CreateSubAccount requestBody = " + data.ToString());

            // 开始请求
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }

            // 获取请求
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Get the response stream  
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseStr = reader.ReadToEnd();

                if (responseStr != null && responseStr.Length > 0)
                {
                    return responseStr;
                }
                else
                {
                    return null;
                }
            }

            
        }


        #region MD5 和 https交互函数定义
        private void WriteLog(string log)
        {
            if (m_isWriteLog)
            {
                string strFilePath = GetLogPath();
                System.IO.FileStream fs = new System.IO.FileStream(strFilePath, System.IO.FileMode.Append);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.Default);
                sw.WriteLine(DateTime.Now.ToString() + "\t" + log);
                sw.Close();
                fs.Close();
            }
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="source">原内容</param>
        /// <returns>加密后内容</returns>
        public static string MD5Encrypt(string source)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            System.Security.Cryptography.MD5 md5Hasher = System.Security.Cryptography.MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(source));

            // Create a new Stringbuilder to collect the bytes and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("X2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        /// <summary>
        /// 设置服务器证书验证回调
        /// </summary>
        public void setCertificateValidationCallBack()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = CertificateValidationResult;
        }

        /// <summary>
        ///  证书验证回调函数  
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="cer"></param>
        /// <param name="chain"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool CertificateValidationResult(object obj, System.Security.Cryptography.X509Certificates.X509Certificate cer, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }
        #endregion
    }

}