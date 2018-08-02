using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NSpider.Core.Events
{

    /// <summary>
    /// 爬虫完成事件
    /// </summary>
    public class OnCompletedEventArgs
    {
        /// <summary>
        /// 爬虫URL地址
        /// </summary>
        public string Uri { get; }

        public string Ip { get; }

        /// <summary>
        /// 任务线程ID
        /// </summary>
        public int ThreadId { get; }


        public RequestInfo RequestInfo { get; }

        /// <summary>
        /// 爬虫请求执行时间
        /// </summary>
        public long Milliseconds { get; }

        public OnCompletedEventArgs(string uri, string ip, int threadId, long milliseconds, RequestInfo requestInfo)
        {
            this.Ip = ip;
            this.Uri = uri;
            this.ThreadId = threadId;
            this.Milliseconds = milliseconds;
            this.RequestInfo = requestInfo;
        }
    }
}
