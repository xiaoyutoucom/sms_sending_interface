using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMSAPI
{
    public class BaseSearchRequestPageModel
    {
        /// <summary>
        /// 当前页码（1开始）
        /// </summary>
        public int PageIndex { get; set; } = 1;
        /// <summary>
        /// 获取数据条数
        /// </summary>
        public int PageSize { get; set; } = 5;

        public string SortName { get; set; }

        public string SortOrder { get; set; }

    }
}