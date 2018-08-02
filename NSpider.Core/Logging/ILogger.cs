using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NSpider.Core.Logging
{
    public interface ILogger
    {
        void Fatal(string message);


        void Info(string message);


        void Warn(string message);


        void Debug(string message);


        void Error(string message);
    }
}
