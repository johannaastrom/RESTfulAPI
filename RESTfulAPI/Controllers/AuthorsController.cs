﻿using RESTfulAPI.Services;
using Microsoft.AspNetCore.Mvc;
using RESTfulAPI.Models;
using System.Collections.Generic;
using RESTfulAPI.Helpers;
using AutoMapper;
using System;
using RESTfulAPI.Entities;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace RESTfulAPI.Controllers
{
	[Route("api/authors")]

	public class AuthorsController : Controller
	{
		private ILibraryRepository _libraryRepository;
		private IUrlHelper _urlHelper;
		private IPropertyMappingService _propertyMappingService;
		private ITypeHelperService _typeHelperService;

		public AuthorsController(ILibraryRepository libraryRepository, IUrlHelper urlHelper, IPropertyMappingService propertyMappingService, ITypeHelperService typeHelperService)
		{
			_libraryRepository = libraryRepository;
			_urlHelper = urlHelper;
			_propertyMappingService = propertyMappingService;
			_typeHelperService = typeHelperService;
		}

		[HttpGet(Name = "GetAuthors")]
		public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters)
		{
			if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(authorsResourceParameters.OrderBy))
				return BadRequest();

			if (!_typeHelperService.HasTypeProperties<AuthorDto>(authorsResourceParameters.Fields))
				return BadRequest();

			var authorsFromRepo = _libraryRepository.GetAuthors(authorsResourceParameters);

			var paginationMetadata = new
			{
				totalCount = authorsFromRepo.TotalCount,
				pageSize = authorsFromRepo.PageSize,
				currentPage = authorsFromRepo.CurrectPage,
				totalPages = authorsFromRepo.TotalPages
			};

			Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

			var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);

			var links = CreateLinksForAuthors(authorsResourceParameters, authorsFromRepo.HasNext, authorsFromRepo.HasPrevious);

			var shapedAuthors = authors.ShapeData(authorsResourceParameters.Fields);

			var shapedAuthorsWithLinks = shapedAuthors.Select(author =>
				{
					var authorAsDictionary = authors as IDictionary<string, object>;
					var authorsLinks = CreateLinksForAuthor(
						(Guid)authorAsDictionary["id"], authorsResourceParameters.Fields);

					authorAsDictionary.Add("links", authorsLinks);

					return authorAsDictionary;
				});

			var linkedCollectionResource = new
			{
				value = shapedAuthorsWithLinks,
				links = links
			};

			return Ok(linkedCollectionResource);
		}

		private string CreateAuthorsResoureUri(AuthorsResourceParameters authorsResourceParameters, ResourceUriType type)
		{
			switch (type)
			{
				case ResourceUriType.PreviousPage:
					return _urlHelper.Link("GetAuthors",
					new
					{
						fields = authorsResourceParameters.Fields,
						orderBy = authorsResourceParameters.OrderBy,
						searchQuery = authorsResourceParameters.SearchQuery,
						genre = authorsResourceParameters.Genre,
						pageNumber = authorsResourceParameters.PageNumber - 1,
						pageSize = authorsResourceParameters.PageSize
					});
				case ResourceUriType.NextPage:
					return _urlHelper.Link("GetAuthors",
					new
					{
						fields = authorsResourceParameters.Fields,
						orderBy = authorsResourceParameters.OrderBy,
						searchQuery = authorsResourceParameters.SearchQuery,
						genre = authorsResourceParameters.Genre,
						pageNumber = authorsResourceParameters.PageNumber + 1,
						pageSize = authorsResourceParameters.PageSize
					});
				case ResourceUriType.Current:
				default:
					return _urlHelper.Link("GetAuthors",
						new
						{
							fields = authorsResourceParameters.Fields,
							orderBy = authorsResourceParameters.OrderBy,
							searchQuery = authorsResourceParameters.SearchQuery,
							genre = authorsResourceParameters.Genre,
							pageNumber = authorsResourceParameters.PageNumber,
							pageSize = authorsResourceParameters.PageSize
						});
			}
		}

		[HttpGet("{id}", Name = "GetAuthor")]
		public IActionResult GetAuthor(Guid id, [FromQuery] string fields)
		{
			var authorFromRepo = _libraryRepository.GetAuthor(id);

			if (authorFromRepo == null)
				return NotFound();

			var author = Mapper.Map<AuthorDto>(authorFromRepo);

			var links = CreateLinksForAuthor(id, fields);

			var linkedResourceToReturn = author.ShapeData(fields)
				as IDictionary<string, object>;

			linkedResourceToReturn.Add("links", links);

			return Ok(linkedResourceToReturn);
		}

		[HttpPost]
		public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
		{
			if (author == null)
				return BadRequest();

			var authorEntity = Mapper.Map<Author>(author);

			_libraryRepository.AddAuthor(authorEntity);

			if (!_libraryRepository.Save())
				throw new Exception("Creating an author failed on save");
			//return StatusCode(500, "A problem happened with handling your request");

			var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);

			var links = CreateLinksForAuthor(authorToReturn.Id, null);

			var linkedResourceToReturn = authorToReturn.ShapeData(null)
				as IDictionary<string, object>;

			linkedResourceToReturn.Add("links", links);

			return CreatedAtRoute("GetAuthor", new { id = linkedResourceToReturn["id"] }, linkedResourceToReturn);
		}

		[HttpPost("{id}")]
		public IActionResult BlockAuthorCollection(Guid id)
		{
			if (_libraryRepository.AuthorExists(id))
				return new StatusCodeResult(StatusCodes.Status409Conflict);

			return Ok();
		}

		[HttpDelete("{id}", Name = "DeleteAuthor")]
		public IActionResult DeleteAuthor(Guid id)
		{
			var authorFromRepo = _libraryRepository.GetAuthor(id);

			if (authorFromRepo == null)
				return NotFound();

			_libraryRepository.DeleteAuthor(authorFromRepo);

			if (!_libraryRepository.Save())
				throw new Exception($"Deleting author {id} failed on save.");

			return NoContent();
		}

		private IEnumerable<LinkDto> CreateLinksForAuthor(Guid id, string fields)
		{
			var links = new List<LinkDto>();

			if (string.IsNullOrWhiteSpace(fields))
			{
				links.Add(
					new LinkDto(_urlHelper.Link("GetAuthor", new { id = id }),
					"self",
					"GET"));
			}
			else
			{
				links.Add(
					new LinkDto(_urlHelper.Link("GetAuthor", new { id = id, fields = fields }),
					"self",
					"GET"));
			}

			links.Add(
				new LinkDto(_urlHelper.Link("DeleteAuthor", new { id = id }),
				"delete_author",
				"DELETE"));

			links.Add(
				new LinkDto(_urlHelper.Link("CreatesBookForAuthor", new { authorId = id }),
				"create_book_for_author",
				"POST"));

			links.Add(
				new LinkDto(_urlHelper.Link("GetBooksForAuthor", new { authorId = id }),
				"books",
				"GET"));

			return links;
		}

		private IEnumerable<LinkDto> CreateLinksForAuthors(
			AuthorsResourceParameters authorsResourceParameters, bool hasNext, bool hasPrevious)
		{
			var links = new List<LinkDto>();

			links.Add(
				new LinkDto(CreateAuthorsResoureUri(authorsResourceParameters,
				ResourceUriType.Current), 
				"self", "GET"));

			if (hasNext)
			{
				links.Add(new LinkDto(CreateAuthorsResoureUri(authorsResourceParameters,
				ResourceUriType.NextPage), 
				"nextPage", "GET"));
			}
			if (hasPrevious)
			{
				links.Add(new LinkDto(CreateAuthorsResoureUri(authorsResourceParameters,
				ResourceUriType.PreviousPage), 
				"self", "GET"));
			}

			return links;
		}
	}
}
