using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Robin.Data;
using Robin.Domain.Repositories;
using Robin.Modules;
using System.Configuration;
using Newtonsoft.Json.Linq;

namespace SMSAPI
{
    public class SmsTempController : Controller
    {
        #region 发送短信
        [HttpGet]
        public JsonResult send()
        {
            BaseResultModel res = new BaseResultModel();
            JsonResult json = new JsonResult();
            try
            {

                var Param1 = Request.QueryString["Param1"] == "" ? null : Request.QueryString["Param1"];
                var Param2 = Request.QueryString["Param2"] == "" ? null : Request.QueryString["Param2"];
                var Param3 = Request.QueryString["Param3"] == "" ? null : Request.QueryString["Param3"];
                var Param4 = Request.QueryString["Param4"] == "" ? null : Request.QueryString["Param4"];

                var phone = Request.QueryString["Phones"];
                var Param5 = Request.QueryString["Param5"] == "" ? null : Request.QueryString["Param5"];
                var Param6 = Request.QueryString["Param6"] == "" ? null : Request.QueryString["Param6"];
                var Param7 = Request.QueryString["Param7"] == "" ? null : Request.QueryString["Param7"];
                var Param8 = Request.QueryString["Param8"] == "" ? null : Request.QueryString["Param8"];
                string templatedId = Request.QueryString["TemplatedId"].ToString();
                
                string sendstr = Param1;
                if (Param2 != null)
                {
                    sendstr = sendstr + ","+ Param2;
                }
                if (Param3 != null)
                {
                    sendstr = sendstr + "," + Param3;
                }
                if (Param4 != null)
                {
                    sendstr = sendstr + "," + Param4;
                }
                if (Param5 != null)
                {
                    sendstr = sendstr + "," + Param5;
                }
                if (Param6 != null)
                {
                    sendstr = sendstr + "," + Param6;
                }
                if (Param7 != null)
                {
                    sendstr = sendstr + "," + Param7;
                }
                if (Param8 != null)
                {
                    sendstr = sendstr + "," + Param8;
                }
                WebProxy.UCSRestRequest api = new WebProxy.UCSRestRequest();
                #region 验证模板
                string strTemplate = api.GetTemplateById(templatedId);
                if (!string.IsNullOrEmpty(strTemplate))
                {
                    JObject template = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(strTemplate);
                    if (template["code"].ToString() != "000000")
                    {
                        res.errorCode = ResultStatusEnum.fail;
                        res.msg = "获取模板失败!";
                        json.Data = res;
                        return Json(json, JsonRequestBehavior.AllowGet);
                    }
                }
                #endregion

              

                //发送短信
                var content = api.SendSMS(phone, templatedId, sendstr);
                JObject callback = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                //短信单发
                if (phone.IndexOf(',') < 0)
                {
                    if (callback["resp"]["respCode"].ToString() == "000000")
                    {
                        res.errorCode = ResultStatusEnum.success;
                        res.msg = "发送成功!";
                    }
                    else
                    {
                        foreach (int item in Enum.GetValues(typeof(ReturnCode)))
                        {

                            if (callback["resp"]["respCode"].ToString() == item.ToString())
                            {                            
                                res.errorCode = ResultStatusEnum.fail;
                                res.msg = Enum.GetName(typeof(ReturnCode), item);
                                break;
                            }
                        }
                    }
                
                }
                //短信群发
                else
                {
                    int numLength = phone.Split(',').Length;
                    for (int i = 0; i < numLength; i++)
                    {
                        if (callback["resp"]["templateSMS"][i]["respCode"].ToString() == "000000")
                        {
                            res.errorCode = ResultStatusEnum.success;
                            res.msg += callback["resp"]["templateSMS"][i]["mobile"].ToString() + ":发送成功!";
                            if (i != numLength - 1)
                            {
                                res.msg += ",";
                            }
                        }
                        else
                        {
                            foreach (int item in Enum.GetValues(typeof(ReturnCode)))
                            {
                                if (callback["resp"]["templateSMS"][i]["respCode"].ToString() == item.ToString())
                                {
                                    res.msg += callback["resp"]["templateSMS"][i]["mobile"].ToString() + ":" + Enum.GetName(typeof(ReturnCode), item);

                                    if (i != numLength - 1)
                                    {
                                        res.msg += ",";
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                res.errorCode = ResultStatusEnum.error;
                res.msg = "发送异常!"+e.Message;
            }
            json.Data = res;
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
    //返回状态码参照
    public enum ReturnCode
    {
        账户或套餐包余额不足 = 100001,
        发送请求的IP不在白名单内 = 100005,
        手机号码不能为空 = 100008,
        手机号码为保护号码 = 100009,
        号码不合法 = 100015,
        账号余额被冻结 = 100016,
        余额已注销 = 100017,
        应用可用额度余额不足 = 100019,
        系统内部错误 = 100699,
        主账户sid存在非法字符 = 101105,
        开发者账户已注销 = 101108,
        主账户sid未激活 = 101109,
        主账户sid已锁定 = 101110,
        主账户sid不存在 = 101111,
        主账户sid为空 = 101112,
        缺少token参数或为空 = 101117,
        应用appid为空 = 102100,
        应用appid存在非法字符 = 102101,
        应用appid不存在 = 102102,
        应用未上线 = 102103,
        应用appid不属于该主账号 = 102105,
        未上线应用只能使用白名单中的号码 = 103126,
        该appid下templateid不存在 = 105110,
        templateid未审核通过 = 105111,
        请求的参数与模板上的变量数量不一致 = 105112,
        templateid不能为空 = 105113,
        templateid含非法字符 = 105117,
        短信模板有替换内容但缺少参数或参数为空 = 105118,
        每个参数的长度不能超过100字 = 105119,
        群发号码单次提交不能超过100个 = 105120,
        templateid已删除 = 105121,
        短信模板内容为空 = 105124,
        短信内容超过500字 = 105133,
        参数中含有特殊符号 = 105135,
        群发号码重复 = 105138,
        账号未认证 = 105140,
        主账号需为企业认证 = 105141,
        模板被定时群发任务锁定暂无法修改 = 105142,
        模板不属于该用户 = 105143,
        创建验证码模板短信需带参数 = 105144,
        对同个号码发送短信超过限定频率 = 105147,
        短信发送频率过快 = 105150,
        请求的参数格式错误 = 105152,
        手机号码格式错误 = 105153,
        短信服务请求异常e100 = 105154,
        变量数量超过100个 = 105157,
        接口不支持GET方式调用 = 105158,
        接口不支持POST方式调用 = 105159,
        请求频率过快 = 105166,
        参数sid或token错误 = 105168,
        提交失败 = 300001,
        未知 = 300002,
        空号 = 300003,
        黑名单 = 300004,
        超频 = 300005,
        系统忙 = 300006
    }
    //
    // 摘要:
    //     短信标准传输信息实体
    public class SMSModel
    {
        //
        // 摘要:
        //     短信内容
        public string Message { get; set; }
        //
        // 摘要:
        //     监测点编号
        public string MonitorId { get; set; }
        //
        // 摘要:
        //     监测点名称
        public string MonitorName { get; set; }
        //
        // 摘要:
        //     监测值
        public string TagValue { get; set; }
        //
        // 摘要:
        //     监测项
        public string TagDesc { get; set; }
        //
        // 摘要:
        //     单位
        public string Unit { get; set; }
        //
        // 摘要:
        //     时间
        public string Time { get; set; }
        //
        // 摘要:
        //     发送手机号群
        public string[] Phones { get; set; }
        //
        // 摘要:
        //     收信人群
        public string[] Names { get; set; }
        //
        // 摘要:
        //     模板编号
        public string TemplateCode { get; set; }
        //
        // 摘要:
        //     参数信息
        public string[] Params { get; set; }
    }
}