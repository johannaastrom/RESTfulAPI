using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTfulAPI.Models
{
    public class LinkDto
    {
		public string Href { get; private set; }

		public string Rel { get; private set; }

		public string Method { get; private set; }

		public LinkDto(string href, string rel, string method)
		{
			Href = href;
			Rel = Rel;
			Method = method;
		}
	}
}
