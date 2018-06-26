﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTfulAPI.Helpers
{
    public class PagedList<T> : List<T>
    {
		public int CurrectPage { get; private set; }

		public int TotalPages { get; private set; }

		public int PageSize { get; private set; }

		public int TotalCount { get; private set; }

		public bool HasPrevious
		{
			get
			{
				return (CurrectPage > 1);
			}
		}

		public bool HasNext
		{
			get
			{
				return (CurrectPage < TotalPages);
			}
		}

		public PagedList(List<T> items, int count, int pageNumber, int pageSize)
		{
			TotalCount = count;
			PageSize = pageSize;
			CurrectPage = pageNumber;
			TotalPages = (int)Math.Ceiling(count / (double)pageSize);
			AddRange(items);
		}

		public static PagedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
		{
			var count = source.Count();
			var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
			return new PagedList<T>(items, count, pageNumber, pageSize);
		}
	}
}
