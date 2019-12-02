using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMSAPI
{
    public enum ResultStatusEnum
    {
        success,
        fail,
        error
    }
    /// <summary>
    /// 排序方式:0 升序，1 降序
    /// </summary>
    public enum OrderType
    {
        /// <summary>
        /// 升序
        /// </summary>
        Asc = 0,
        /// <summary>
        /// 降序
        /// </summary>
        Desc = 1
    }
}