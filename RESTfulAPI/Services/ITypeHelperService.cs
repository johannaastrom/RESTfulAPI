using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTfulAPI.Services
{
    public interface ITypeHelperService
    {
		bool HasTypeProperties<T>(string fields);
    }
}
