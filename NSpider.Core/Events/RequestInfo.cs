using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSpider.Core.Events
{
    public class RequestInfo
    {
        /// <summary>
        /// 当前Cookies
        /// </summary>
        public List<Cookie> Cookies { get; set; }
    }
}
