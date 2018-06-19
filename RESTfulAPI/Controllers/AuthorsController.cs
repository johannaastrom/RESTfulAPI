using RESTfulAPI.Services;
using Microsoft.AspNetCore.Mvc;
using RESTfulAPI.Models;
using System.Collections.Generic;
using RESTfulAPI.Helpers;
using AutoMapper;
using System;

namespace RESTfulAPI.Controllers
{
	[Route("api/authors")]

	public class AuthorsController : Controller
    {
		private ILibraryRepository _libraryRepository;

		public AuthorsController(ILibraryRepository libraryRepository)
		{
			_libraryRepository = libraryRepository;
		}

		[HttpGet()]
		public IActionResult GetAuthors()
		{
				var authorsFromRepo = _libraryRepository.GetAuthors();

				var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
				return Ok(authors);
		}

		[HttpGet("{id}")]
		public IActionResult GetAuthor(Guid id)
		{
			var authorFromRepo = _libraryRepository.GetAuthor(id);

			if (authorFromRepo == null)
				return NotFound();

			var author = Mapper.Map<AuthorDto>(authorFromRepo);
			return Ok(author);
		}
	}
}
