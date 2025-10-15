using apiv4.Controllers;
using apiv4.Data;
using apiv4.Models;
using Microsoft.EntityFrameworkCore;

namespace apiv4.Repositories
{
    public class BookRepo : IBookRepo
    {
        private readonly ApiContext _context;
        public BookRepo(ApiContext context)
        {
            _context = context;
        }

        //***************************************************************
        // Nu async Task
        public async Task Add(Book book)
        {
            await _context.BookSet.AddAsync(book); 
            await _context.SaveChangesAsync();
        }

        // Nu async Task<Book?>
        public async Task<Book?> Get(short id)
        {
            return await _context.BookSet.FindAsync(id);
        }

        // Nu async Task<List<Book>>
        public async Task<List<Book>> GetBooks()
        {
            return await _context.BookSet.ToListAsync();
        }
    }
}
