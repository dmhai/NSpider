using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSpider.Core.Crawler
{
    public class Operation
    {
        public Action<string, IWebDriver, WebDriverWait, Action<IWebDriver, WebDriverWait>, List<string>, string> Action { get; set; }

        public Func<IWebDriver, bool> Condition { get; set; }

        public int Timeout { get; set; }

    }
}
