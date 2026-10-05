using Microsoft.AspNetCore.Mvc;
using WebApplication5.Models;

namespace WebApplication5.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private static readonly List<Book> Books = new List<Book>
        {
            new Book { Id = 1, Title = "Кобзар", Author = "Тарас Шевченко", Year = 1840 },
            new Book { Id = 2, Title = "Тіні забутих предків", Author = "Михайло Коцюбинський", Year = 1911 },
            new Book { Id = 3, Title = "Місто", Author = "Валер'ян Підмогильний", Year = 1928 }
        };

        [HttpGet]
        public IActionResult GetBooks()
        {
            return Ok(Books);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetBookById(int id)
        {
            var book = Books.FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                return NotFound("Book not found");
            }

            return Ok(book);
        }

        [HttpGet("search")]
        public IActionResult SearchBooks([FromQuery] string title, [FromQuery] string? author)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Параметр 'title' є обов'язковим.");
            }

            var query = Books.Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(author))
            {
                query = query.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
            }

            return Ok(query.ToList());
        }

        [HttpPost]
        public IActionResult CreateBook([FromBody] Book newBook)
        {
            if (string.IsNullOrWhiteSpace(newBook.Title) || string.IsNullOrWhiteSpace(newBook.Author))
            {
                return BadRequest("Поля 'Title' та 'Author' є обов'язковими.");
            }

            if (newBook.Year < 1800)
            {
                return BadRequest("Рік видання має бути не менше 1800.");
            }

            int newId = Books.Count > 0 ? Books.Max(b => b.Id) + 1 : 1;
            newBook.Id = newId;

            Books.Add(newBook);

            return CreatedAtAction(nameof(GetBookById), new { id = newBook.Id }, newBook);
        }

        [HttpPut("{id:int}")]
        public IActionResult UpdateBook(int id, [FromBody] Book updatedBook)
        {
            var existingBook = Books.FirstOrDefault(b => b.Id == id);
            if (existingBook == null)
            {
                return NotFound("Book not found");
            }

            if (string.IsNullOrWhiteSpace(updatedBook.Title) || string.IsNullOrWhiteSpace(updatedBook.Author))
            {
                return BadRequest("Поля 'Title' та 'Author' є обов'язковими.");
            }

            if (updatedBook.Year < 1800)
            {
                return BadRequest("Рік видання має бути не менше 1800.");
            }

            existingBook.Title = updatedBook.Title;
            existingBook.Author = updatedBook.Author;
            existingBook.Year = updatedBook.Year;

            return Ok(existingBook);
        }

        [HttpDelete("{id:int}")]
        public IActionResult DeleteBook(int id)
        {
            var book = Books.FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                return NotFound("Book not found");
            }

            Books.Remove(book);

            return NoContent();
        }
    }
}
