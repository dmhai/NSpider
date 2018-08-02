using System;
using System.Collections.Generic;
using System.Text;

namespace NSpider.Core.AutoMapper
{
    public class AutoMapAttribute : Attribute
    {
        public Type[] ToSource { get; private set; }

        public AutoMapAttribute(params Type[] toSource)
        {
            this.ToSource = toSource;
        }
    }
}
