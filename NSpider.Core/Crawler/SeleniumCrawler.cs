using NSpider.Core.Events;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSpider.Core.Crawler
{
    public class SeleniumCrawler
    {
        public IWebDriver Driver { get; set; }

        public event EventHandler<OnStartEventArgs> OnStart;
        public event EventHandler<OnCompletedEventArgs> OnCompleted;
        public event EventHandler<OnErrorEventArgs> OnError;

        public SeleniumCrawler(IWebDriver driver)
        {
            this.Driver = driver;
        }


        public async Task<bool> Start(string uri, Script script, Operation operation, Action<IWebDriver, WebDriverWait> redirect, string ip = null)
        {
            try
            {
                return await Task.Factory.StartNew(() =>
                {
                    var links = new List<string>();

                    OnStart?.Invoke(this, new OnStartEventArgs(uri));
                    var watch = new Stopwatch();
                    watch.Start();
                    Driver.Manage().Window.Maximize();

                    var driverWait = new WebDriverWait(Driver, TimeSpan.FromMinutes(2));
                 
                    //执行自定义逻辑
                    operation.Action?.Invoke(uri, Driver, driverWait, redirect, links, ip);
                    if (script != null) Driver.ExecuteJavaScript(script.Code, script.Args);
                    //获取当前任务线程ID
                    var threadId = Thread.CurrentThread.ManagedThreadId;
                    //获取请求执行时间
                    var milliseconds = watch.ElapsedMilliseconds;
                    var info = new RequestInfo()
                    {
                        Cookies = Driver.Manage().Cookies.AllCookies.ToList()
                    };

                    OnCompleted?.Invoke(this, new OnCompletedEventArgs(uri, ip, threadId, milliseconds, info));
                    return true;

                });
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new OnErrorEventArgs(uri, ip, ex, new RequestInfo
                {
                    Cookies = Driver.Manage().Cookies.AllCookies.ToList()
                }));
                return false;
            }
            finally
            {
                Driver.Close();
                Driver.Quit();
            }
        }
    }
}
