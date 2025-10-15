using apiv4.Controllers;
using apiv4.Models;

namespace apiv4.Repositories
{
    public interface IBookRepo
    {
        // Bytte från void till Task
        Task Add(Book book);

        // Bytte från BookController? till Task<Book?>
        Task<Book?> Get(short id);

        // Bytte från List<Book> till Task<List<Book>>
        Task<List<Book>> GetBooks();

    }
}
