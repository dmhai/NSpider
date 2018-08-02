using System;
using System.Collections.Generic;
using System.Text;

namespace NSpider.Core.Events
{
    /// <summary>
    /// 爬虫启动事件
    /// </summary>
    public class OnStartEventArgs
    {
        /// <summary>
        ///  爬虫URL地址
        /// </summary>
        public string Uri { get; set; }

        public OnStartEventArgs(string uri)
        {   
            Uri = uri;
        }
    }
}
