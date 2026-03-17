using System.Collections.Specialized;
using System.Xml;

namespace IBatisNet.Common.Xml
{
    /// <summary>
    ///     Summary description for NodeUtils.
    /// </summary>
    public sealed class NodeUtils
    {
        /// <summary>
        ///     Searches for the attribute with the specified name in this attributes list.
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="name">The key</param>
        /// <returns></returns>
        public static string GetStringAttribute(NameValueCollection attributes, string name)
        {
            var value = attributes[name];
            if (value == null) return string.Empty;

            return value;
        }

        /// <summary>
        ///     Searches for the attribute with the specified name in this attributes list.
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="name">The key</param>
        /// <param name="def">The default value to be returned if the attribute is not found.</param>
        /// <returns></returns>
        public static string GetStringAttribute(NameValueCollection attributes, string name, string def)
        {
            var value = attributes[name];
            if (value == null) return def;

            return value;
        }

        /// <summary>
        ///     Searches for the attribute with the specified name in this attributes list.
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="name">The key</param>
        /// <param name="def">The default value to be returned if the attribute is not found.</param>
        /// <returns></returns>
        public static byte GetByteAttribute(NameValueCollection attributes, string name, byte def)
        {
            var value = attributes[name];
            if (value == null) return def;

            return XmlConvert.ToByte(value);
        }

        /// <summary>
        ///     Searches for the attribute with the specified name in this attributes list.
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="name">The key</param>
        /// <param name="def">The default value to be returned if the attribute is not found.</param>
        /// <returns></returns>
        public static int GetIntAttribute(NameValueCollection attributes, string name, int def)
        {
            var value = attributes[name];
            if (value == null) return def;

            return XmlConvert.ToInt32(value);
        }

        /// <summary>
        ///     Searches for the attribute with the specified name in this attributes list.
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="name">The key</param>
        /// <param name="def">The default value to be returned if the attribute is not found.</param>
        /// <returns></returns>
        public static bool GetBooleanAttribute(NameValueCollection attributes, string name, bool def)
        {
            var value = attributes[name];
            if (value == null) return def;

            return XmlConvert.ToBoolean(value);
        }

        /// <summary>
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static NameValueCollection ParseAttributes(XmlNode node)
        {
            return ParseAttributes(node, null);
        }

        /// <summary>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        public static NameValueCollection ParseAttributes(XmlNode node, NameValueCollection variables)
        {
            var attributes = new NameValueCollection();
            var count = node.Attributes.Count;
            for (var i = 0; i < count; i++)
            {
                var attribute = node.Attributes[i];
                var value = ParsePropertyTokens(attribute.Value, variables);
                attributes.Add(attribute.Name, value);
            }

            return attributes;
        }


        /// <summary>
        ///     Replace properties by their values in the given string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static string ParsePropertyTokens(string str, NameValueCollection properties)
        {
            var OPEN = "${";
            var CLOSE = "}";

            var newString = str;
            if (newString != null && properties != null)
            {
                var start = newString.IndexOf(OPEN);
                var end = newString.IndexOf(CLOSE);

                while (start > -1 && end > start)
                {
                    var prepend = newString.Substring(0, start);
                    var append = newString.Substring(end + CLOSE.Length);

                    var index = start + OPEN.Length;
                    var propName = newString.Substring(index, end - index);
                    var propValue = properties.Get(propName);
                    if (propValue == null)
                        newString = prepend + propName + append;
                    else
                        newString = prepend + propValue + append;
                    start = newString.IndexOf(OPEN);
                    end = newString.IndexOf(CLOSE);
                }
            }

            return newString;
        }
    }
}