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

        // Hämta en specifik bok (Korrigerad returtyp och async)
        [Authorize(Roles = ApiRole.User, AuthenticationSchemes = "Identity.Application")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
        [ProducesResponseType(StatusCodes.Status403Forbidden)]    
        [ProducesResponseType(StatusCodes.Status404NotFound)]     
        public async Task<ActionResult<Book>> Get(short id)
        {
            var book = await _bookrepo.Get(id);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }

        // Hämta alla böcker (Korrigerad till async)
        [Authorize(Roles = ApiRole.User)]
        // Lägg till bok (Korrigerad till async Task)
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Book book)
        {
            await _bookrepo.Add(book);
            return CreatedAtAction(nameof(Get), new { id = book.Id }, book);
        }
    }

}
