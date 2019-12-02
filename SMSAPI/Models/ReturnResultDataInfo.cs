using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMSAPI
{
    /// <summary>
    /// 返回结果数据信息类
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
    public class ReturnResultDataInfo<T> where T : new()
    {
        /// <summary>
        /// 调用结果编码，其余编码后期补充
        /// 0：成功
        /// 1：失败
        /// 2：部分成功
        /// 97：用户未登陆
        /// 98：访问未授权的功能
        /// 99：程序发生异常
        /// </summary>
        public ResultStatusEnum resultCode { set; get; }
        /// <summary>
        /// 调用结果返回的数据
        /// </summary>
        public T rows { set; get; }
        /// <summary>
        /// 返回数据总条数，分页专用参数注意不是返回数据集合的数据条数
        /// </summary>
        public int? total { get; set; }
        /// <summary>
        /// 调用结果返回的相关提示信息
        /// </summary>
        public string msg { set; get; }
        /// <summary>
        /// 执行失败提示的内容的参数
        /// </summary>
        public List<string> parametersList
        {
            get;
            set;
        }
    }
}
  