using RESTfulAPI.Services;
using Microsoft.AspNetCore.Mvc;
using RESTfulAPI.Models;
using System.Collections.Generic;
using RESTfulAPI.Helpers;
using AutoMapper;
using System;
using RESTfulAPI.Entities;
using Microsoft.AspNetCore.Http;

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

			var previousPageLink = authorsFromRepo.HasPrevious ?
				CreateAuthorsResoureUri(authorsResourceParameters, ResourceUriType.PreviousPage) : null;


			var nextPageLink = authorsFromRepo.HasNext ?
				CreateAuthorsResoureUri(authorsResourceParameters, ResourceUriType.NextPage) : null;

			var paginationMetadata = new
			{
				totalCount = authorsFromRepo.TotalCount,
				pageSize = authorsFromRepo.PageSize,
				currentPage = authorsFromRepo.CurrectPage,
				totalPages = authorsFromRepo.TotalPages,
				previousPageLink = previousPageLink,
				nextPageLink = nextPageLink
			};

			Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

			var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
			return Ok(authors.ShapeData(authorsResourceParameters.Fields));
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
		public IActionResult GetAuthor(Guid id)
		{
			var authorFromRepo = _libraryRepository.GetAuthor(id);

			if (authorFromRepo == null)
				return NotFound();

			var author = Mapper.Map<AuthorDto>(authorFromRepo);
			return Ok(author);
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

			return CreatedAtRoute("GetAuthor", new { id = authorToReturn.Id }, authorToReturn);
		}

		[HttpPost("{id}")]
		public IActionResult BlockAuthorCollection(Guid id)
		{
			if (_libraryRepository.AuthorExists(id))
				return new StatusCodeResult(StatusCodes.Status409Conflict);

			return Ok();
		}

		[HttpDelete("{id}")]
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
	}
}
