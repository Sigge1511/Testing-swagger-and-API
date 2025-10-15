using apiv4.Controllers;
using apiv4.Models;

namespace apiv4.Repositories
{
    public interface IBookRepo
    {
        public BookController? Get(int id);
        public List<Book> GetBooks();

    }
}
