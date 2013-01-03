using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NewRelicConfiguration
{
    public enum Metric
    {
        /// <summary>
        /// Note - this shouldn't ever come up as the serializer should see it as the
        /// default value.
        /// </summary>
        [XmlEnum(Name="unset")]
        Unspecified,
        [XmlEnum(Name="none")]
        None,
        [XmlEnum(Name="scoped")]
        Scoped,
        [XmlEnum(Name="unscoped")]
        Unscoped,
        [XmlEnum(Name="both")]
        Both
    }
}
