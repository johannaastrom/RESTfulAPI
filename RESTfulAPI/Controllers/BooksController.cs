﻿using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using RESTfulAPI.Entities;
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

		[HttpGet("{id}", Name = "GetBookForAthor")]
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

		[HttpPost()]
		public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto book)
		{
			if (book == null)
				return BadRequest();

			if (!_libraryRepository.AuthorExists(authorId))
				return NotFound();

			var bookEntity = Mapper.Map<Book>(book);

			_libraryRepository.AddBookForAuthor(authorId, bookEntity);

			if (!_libraryRepository.Save())
				throw new Exception($"Creating a book for {authorId} failed on save.");

			var bookToReturn = Mapper.Map<BookDto>(bookEntity);

			return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);
		}

		[HttpDelete("{id}")]
		public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
		{
			if (!_libraryRepository.AuthorExists(authorId))
				return NotFound();

			var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);

			if (bookForAuthorFromRepo == null)
				return NotFound();

			_libraryRepository.DeleteBook(bookForAuthorFromRepo);

			if (!_libraryRepository.Save())
				throw new Exception($"Deleting book {id} for author {authorId} failed on save.");

			return NoContent();
		}

		[HttpPut("{id}")]
		public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDto book)
		{
			if (book == null)
				return BadRequest();

			if (_libraryRepository.AuthorExists(authorId))
				return NotFound();

			var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);

			if (bookForAuthorFromRepo == null)
			{
				var bookToAdd = Mapper.Map<Book>(book);
				bookToAdd.Id = id;

				_libraryRepository.AddBookForAuthor(authorId, bookToAdd);

				if (!_libraryRepository.Save())
					throw new Exception($"Upserting book {id} for author {authorId} failed on save.");

				var bookToReturn = Mapper.Map<BookDto>(bookToAdd);

				return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);
			}

			Mapper.Map(book, bookForAuthorFromRepo);

			_libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

			if (!_libraryRepository.Save())
				throw new Exception($"Updatin book {id} for author {authorId} failed on save.");

			return NoContent();
		}

		[HttpPatch("{id}")]
		public IActionResult PartallyUpdateBookForAuthor(Guid authorId, Guid id, [FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc)
		{
			if (patchDoc == null)
				return BadRequest();

			if (_libraryRepository.AuthorExists(authorId))
				return NotFound();

			var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);

			if (bookForAuthorFromRepo == null)
			{
				var bookDto = new BookForUpdateDto();
				patchDoc.ApplyTo(bookDto);

				var bookToAdd = Mapper.Map<Book>(bookDto);
				bookToAdd.Id = id;

				_libraryRepository.AddBookForAuthor(authorId, bookToAdd);

				if (!_libraryRepository.Save())
					throw new Exception($"Upserting book {id} for author {authorId} failed on save.");

				var bookToReturn = Mapper.Map<BookDto>(bookToAdd);

				return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);
			}

			var bookToPatch = Mapper.Map<BookForUpdateDto>(bookForAuthorFromRepo);

			patchDoc.ApplyTo(bookToPatch);

			//add validation

			Mapper.Map(bookToPatch, bookForAuthorFromRepo);

			_libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

			if (!_libraryRepository.Save())
				throw new Exception($"Pathcong book {id} for author {authorId} failed on save.");

			return NoContent();
		}
	}
}
