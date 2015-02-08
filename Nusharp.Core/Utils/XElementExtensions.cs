using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Nusharp.Core.Utils
{
	public static class XElementExtensions
	{
		public static XElement Attr(this XElement element, XName xName, object value)
		{
			element.SetAttributeValue(xName, value);
			return element;
		}

		public static XElement Val(this XElement element, object value)
		{
			if (value != null)
				element.SetValue(value);
			return element;
		}

		public static XElement Elem(this XElement element, XElement childElement)
		{
			element.Add(childElement);
			return element;
		}

		public static XElement Start(this XElement element, XName xname)
		{
			XElement childElement = new XElement(xname);
			element.Add(childElement);
			return childElement;
		}

		public static XElement End(this XElement element)
		{
			return element.Parent;
		}

		public static string AsString(this XElement element)
		{
			return element.As<string>();
		}

		public static T As<T>(this XElement element, T defaultValue)
		{
			Type t = typeof(T);
			if (element == null)
				return defaultValue;
			return (T)As(element.Value, t);
		}

		public static T As<T>(this XElement element)
		{
			return element.As<T>(default(T));
		}

		private static object As(string value, Type type)
		{
			if (type == typeof(string))
				return value;
			if (type == typeof(bool))
				return bool.Parse(value);
			throw new NotImplementedException();
		}
		
	}
}
