using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RESTfulAPI.Models;
using RESTfulAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTfulAPI.Controllers
{
	[Route("api/authors/{authorId}/books")]

    public class BooksController : Controller
    {
		private ILibraryRepository _libraryRepository;

		public BooksController(ILibraryRepository libraryRepository)
		{
			_libraryRepository = libraryRepository;
		}

		[HttpGet()]
		public IActionResult GetBooks(Guid authorId)
		{
			if (!_libraryRepository.AuthorExists(authorId))
				return NotFound();

			var booksForAuthorFromRepo = _libraryRepository.GetBooksForAuthor(authorId);

			var booksForAuthor = Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepo);

			return Ok(booksForAuthor);
		}

		[HttpGet("{id}")]
		public IActionResult GetBookForAuthor(Guid authorId, Guid id)
		{
			if (!_libraryRepository.AuthorExists(authorId))
				return NotFound();

			var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
			if (bookForAuthorFromRepo == null)
				return NotFound();

			var bookForAuthor = Mapper.Map<BookDto>(bookForAuthorFromRepo);

			return Ok(bookForAuthor);
		}
	}
}
