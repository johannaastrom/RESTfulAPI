using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RESTfulAPI.Services
{
	public class TypeHelperService : ITypeHelperService
	{
		public bool TypeHasProperties<T>(string fields)
		{
			if (string.IsNullOrWhiteSpace(fields))
				return true;

			var fieldwsAfterSplit = fields.Split(',');

			foreach (var field in fieldwsAfterSplit)
			{
				var propertyName = field.Trim();

				var propertyInfo = typeof(T)
					.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

				if (propertyInfo == null)
					return false;
			}
			return true;
		}

		bool ITypeHelperService.HasTypeProperties<T>(string fields)
		{
			throw new NotImplementedException();
		}
	}
}

