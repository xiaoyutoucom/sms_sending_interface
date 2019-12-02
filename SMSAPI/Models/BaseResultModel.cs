using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMSAPI
{
    /// <summary>
    /// 返回信息的基类
    /// </summary>
    public class BaseResultModel
    {


        public ResultStatusEnum errorCode { get; set; }

        public int rowCount { get; set; }

        public string msg { get; set; }
    }
}