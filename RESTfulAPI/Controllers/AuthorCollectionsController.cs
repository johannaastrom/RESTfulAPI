using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using RESTfulAPI.Entities;
using RESTfulAPI.Helpers;
using RESTfulAPI.Models;
using RESTfulAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTfulAPI.Controllers
{
	[Route("api/authorcollections")]
    public class AuthorCollectionsController : Controller
    {
		private ILibraryRepository _libraryRepository;

		public AuthorCollectionsController(ILibraryRepository libraryRepository)
		{
			_libraryRepository = libraryRepository;
		}

		[HttpPost]
		public IActionResult CreateAuthorCollection([FromBody] IEnumerable<AuthorForCreationDto> authorCollection)
		{
			if (authorCollection == null)
				return BadRequest();

			var authorEntities = Mapper.Map<IEnumerable<Author>>(authorCollection);

			foreach (var author in authorEntities)
			{
				_libraryRepository.AddAuthor(author);
			}

			if (!_libraryRepository.Save())
				throw new Exception("Creating an author collection failed on save");

			var authorCollectionToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

			var idsAsString = string.Join(",", authorCollectionToReturn.Select(a => a.Id));

			return CreatedAtRoute("GetAturhoCollection", new { ids = idsAsString }, authorCollectionToReturn);

			//return Ok();
		}

		[HttpGet("({ids})")]
		public IActionResult GetAuthorCollection([ModelBinder(BinderType =typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
		{
			if (ids == null)
				return BadRequest();

			var authorEntities = _libraryRepository.GetAuthors(ids);

			if (ids.Count() != authorEntities.Count())
				return NotFound();

			var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

			return Ok(authorsToReturn);
		}

    }
}
