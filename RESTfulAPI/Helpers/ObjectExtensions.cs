using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RESTfulAPI.Helpers
{
	public static class ObjectExtensions
	{
		public static ExpandoObject ShapeData<TSource>(this TSource source, string fields)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			var dataShapedObject = new ExpandoObject();

			if (string.IsNullOrWhiteSpace(fields))
			{
				var propertyInfos = typeof(TSource)
					.GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

				foreach (var propertyInfo in propertyInfos)
				{
					var propertyValue = propertyInfo.GetValue(source);
				}
			}
		}
	}
}
