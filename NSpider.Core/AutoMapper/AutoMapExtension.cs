using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSpider.Core.AutoMapper
{
    public static class AutoMapExtension
    {
        public static TDestination MapTo<TDestination>(this object source)
        {

            return Mapper.Map<TDestination>(source);


        }

        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            return Mapper.Map(source, destination);
        }
    }
}
