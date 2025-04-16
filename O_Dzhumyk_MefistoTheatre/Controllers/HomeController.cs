using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using O_Dzhumyk_MefistoTheatre.Data;
using O_Dzhumyk_MefistoTheatre.ViewModels.Home;

namespace MefistoTheatre.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int DefaultPageSize = 15;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Home/Index or /?pageNumber=2
        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            // Ensure the page number is at least 1.
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            int pageSize = DefaultPageSize;

            // Base query to fetch posts with related Category and Author data.
            var query = _context.Posts
                              .Include(p => p.Category)
                              .Include(p => p.Author)
                              .OrderByDescending(p => p.CreatedAt)
                              .AsNoTracking();

            // Get total count for pagination.
            var totalItemCount = await query.CountAsync();

            // Project posts to the view model with content truncation.
            var postViewModelsQuery = query
                .Select(post => new PostViewModel
                {
                    Id = post.Id,
                    Title = post.Title,
                    Content = post.Content.Length > 200
                              ? post.Content.Substring(0, 200) + "..."
                              : post.Content,
                    AuthorFullName = post.Author != null ? post.Author.FullName : "Unknown Author",
                    CategoryName = post.Category != null ? post.Category.Name : "Uncategorized",
                    CreatedAt = post.CreatedAt,
                });

            // Apply pagination.
            var paginatedPosts = await postViewModelsQuery
                                       .Skip((pageNumber - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToListAsync();

            // Calculate total pages.
            var totalPages = (int)Math.Ceiling(totalItemCount / (double)pageSize);

            var viewModel = new BlogViewModel
            {
                Posts = paginatedPosts,
                PageTitle = "Latest News & Reviews",
                CurrentPage = pageNumber,
                TotalPages = totalPages
            };

            // Redirect to the last page if the requested page exceeds total pages.
            if (pageNumber > totalPages && totalPages > 0)
            {
                return RedirectToAction(nameof(Index), new { pageNumber = totalPages });
            }

            return View(viewModel);
        }

        // GET: /Home/About
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
    }
}
