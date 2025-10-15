using apiv4.Constants;
using apiv4.Models;
using apiv4.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace apiv4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly ILogger<BookController> _logger;
        private readonly IBookRepo _bookrepo;

        public BookController(ILogger<BookController> logger, IBookRepo bookrepo)
        {
            _logger = logger;
            _bookrepo = bookrepo;
        }
        //***************************************************************

        [HttpGet("{id}")]
        public BookController? Get(int id) 
        { return _bookrepo.Get(id); }

        [Authorize(Roles = ApiRole.User)]
        [HttpGet]
        public List<Book> GetAllBooks()
        {
            return _bookrepo.GetBooks();
        }
    }
    
}
