using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSpider.Core.AutoMapper
{
    public interface IDtoMapping
    {
        void CreateMapping(IMapperConfigurationExpression mapperConfig);
    }
}
