using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using O_Dzhumyk_MefistoTheatre.Data;
using O_Dzhumyk_MefistoTheatre.Services;
using O_Dzhumyk_MefistoTheatre.ViewModels.Home;

namespace O_Dzhumyk_MefistoTheatre.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PostContentFormatter _formatter;
        private const int DefaultPageSize = 15;

        public HomeController(ApplicationDbContext context, PostContentFormatter formatter)
        {
            _context = context;
            _formatter = formatter;
        }

        // GET: /Home/Index or /?pageNumber=2
        public async Task<IActionResult> Index(string? category, int pageNumber = 1)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            var query = _context.Posts.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category.Name == category);
            }

            var totalItemCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItemCount / (double)DefaultPageSize);

            if (pageNumber > totalPages && totalPages > 0)
            {
                return RedirectToAction(nameof(Index), new { category, pageNumber = totalPages });
            }

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * DefaultPageSize)
                .Take(DefaultPageSize)
                .Select(post => new
                {
                    post.Id,
                    post.Title,
                    post.Content,
                    AuthorFullName = post.Author.FullName,
                    CategoryName = post.Category.Name,
                    post.CreatedAt,
                })
                .ToListAsync();

            var viewModel = new BlogViewModel
            {
                // Excerpts are made in memory: cutting HTML in SQL could leave half a tag behind
                Posts = posts.Select(post => new PostViewModel
                {
                    Id = post.Id,
                    Title = post.Title,
                    Content = _formatter.ToExcerpt(post.Content),
                    AuthorFullName = post.AuthorFullName,
                    CategoryName = post.CategoryName,
                    CreatedAt = post.CreatedAt,
                }).ToList(),
                PageTitle = string.IsNullOrWhiteSpace(category) ? "Latest News & Reviews" : category,
                Category = category,
                CurrentPage = pageNumber,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        public IActionResult About()
        {
            // Static content for About page.
            var viewModel = new AboutViewModel
            {
                PageTitle = "About Mefisto Theatre Glasgow",
                Content = @"Welcome to Mefisto Theatre, a vibrant hub of theatrical innovation nestled in the heart of Glasgow. Since our inception, we've been dedicated to bringing captivating and thought-provoking performances to our diverse audience.
                            Our mission transcends mere entertainment; we aim to ignite imaginations, provoke conversations, and foster a deep appreciation for the performing arts. From classic dramas to contemporary experimental pieces, our repertoire is as varied as our audience.
                            Our commitment to the Glasgow community is unwavering. We strive to be more than just a theatre; we are a cultural landmark, a place where stories come to life, and where every visit leaves a lasting impression.
                            Join us in celebrating the magic of theatre, right here in Glasgow."
            };

            return View(viewModel);
        }

        // GET: /Home/Contact
        public IActionResult Contact()
        {
            // Static content for Contact page.
            var viewModel = new ContactViewModel
            {
                PageTitle = "Contact Us",
                Address = "123 Theatre Lane, Glasgow, G1 1AB, United Kingdom",
                Phone = "+44 141 123 4567",
                Email = "enquiries@mefistotheatre.example.co.uk"
            };

            return View(viewModel);
        }

        [Route("Home/Error")]
        public IActionResult Error() => View("Error", 500);

        [Route("Home/StatusCode/{code:int}")]
        public IActionResult HttpStatus(int code) => View("Error", code);
    }
}
