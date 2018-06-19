using RESTfulAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace RESTfulAPI.Controllers
{
	public class AuthorsController : Controller
    {
		private ILibraryRepository _libraryRepository;

		public AuthorsController(ILibraryRepository libraryRepository)
		{
			_libraryRepository = libraryRepository;
		}

		public IActionResult GetAuthors()
		{
			var authorsFromRepo = _libraryRepository.GetAuthors();
			return new JsonResult(authorsFromRepo);
		}
	}
}
