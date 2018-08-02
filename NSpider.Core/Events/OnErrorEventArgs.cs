using System;

namespace NSpider.Core.Events
{

    /// <summary>
    /// 爬虫异常事件
    /// </summary>
    public class OnErrorEventArgs
    {

        /// <summary>
        /// 爬虫URL地址
        /// </summary>
        public string Uri { get; }

        public string Ip { get; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception Exception { get; }

        public RequestInfo RequestInfo { get; }


        public OnErrorEventArgs(string uri, string ip, Exception exception, RequestInfo requestInfo)
        {
            this.Uri = uri;
            this.Ip = ip;
            this.Exception = exception;
            RequestInfo = requestInfo;
        }
    }
}
