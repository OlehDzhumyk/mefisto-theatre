using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using O_Dzhumyk_MefistoTheatre.Data;
using O_Dzhumyk_MefistoTheatre.Models;
using O_Dzhumyk_MefistoTheatre.ViewModels.Posts; 
using System.ComponentModel.DataAnnotations;


namespace O_Dzhumyk_MefistoTheatre.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public PostsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(string category)
        {
            var postsQuery = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                postsQuery = postsQuery.Where(p => p.Category.Name == category);
                ViewData["CurrentCategory"] = category;
            }

            var posts = await postsQuery.ToListAsync();
            return View(posts);
        }

        [AllowAnonymous]
        [HttpGet("Posts/Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound($"Post with ID {id} not found.");
            }

            var currentUser = User.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(User) : null;
            bool canEditPost = currentUser != null && (User.IsInRole("Admin") || User.IsInRole("Staff") || post.AuthorId == currentUser.Id);
            bool canDeletePost = User.IsInRole("Admin") || User.IsInRole("Staff");
            bool canAddComment = currentUser != null && !(currentUser.IsBanned); // Simplified null check
            bool isUserSuspended = currentUser?.IsBanned ?? false;

            var viewModel = new PostDetailsViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                AuthorFullName = post.Author?.FullName ?? "Unknown Author",
                AuthorId = post.AuthorId,
                CategoryName = post.Category?.Name ?? "Uncategorized",
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                Comments = post.Comments
                             .OrderBy(c => c.CreatedAt)
                             .Select(c => new CommentViewModel
                             {
                                 Id = c.Id,
                                 Content = c.Content,
                                 AuthorFullName = c.Author?.FullName ?? "Unknown Author",
                                 AuthorId = c.AuthorId ?? string.Empty,
                                 CreatedAt = c.CreatedAt,
                                 // Check if user is Admin OR the comment author
                                 CanDelete = User.IsInRole("Admin") || (currentUser != null && c.AuthorId == currentUser.Id)
                             }).ToList(),
                CanAddComment = canAddComment,
                IsUserAuthenticated = User.Identity?.IsAuthenticated == true,
                IsUserSuspended = isUserSuspended,
                CanEditPost = canEditPost,
                CanDeletePost = canDeletePost
            };

            return View(viewModel);
        }


        [HttpGet]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Create()
        {
            var viewModel = new PostFormViewModel
            {
                // Mode property is less critical now but kept for potential future use
                // Mode = FormMode.Create,
                Categories = await GetCategoriesAsync()
            };
            // Returns the dedicated Views/Posts/Create.cshtml view
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Staff,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,CategorySelection,NewCategoryName")] PostFormViewModel model)
        {
            // Resolve category first as it might add validation errors
            var categoryResult = await ResolveCategoryIdAsync(model);

            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategoriesAsync();
                // Returns the Create view with current model data and validation errors
                return View(model);
            }

            // Although ModelState might be valid, category resolution itself could have failed
            if (!categoryResult.isValid || !categoryResult.categoryId.HasValue)
            {
                model.Categories = await GetCategoriesAsync();
                return View(model); // Return to Create view
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge(); // Should not happen due to [Authorize]
            }

            var post = new Post
            {
                Title = model.Title,
                Content = model.Content,
                AuthorId = currentUser.Id,
                CategoryId = categoryResult.categoryId.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null // Explicitly null for new posts
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Post created successfully.";
            return RedirectToAction(nameof(Details), new { id = post.Id });
        }


        [HttpGet("Posts/Edit/{id:int}")]
        [Authorize] // Authorize attribute checks authentication; specific checks below
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid post ID.");
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound($"Post with ID {id} not found.");
            }

            // Authorization check: Must be post author or Admin/Staff
            var currentUser = await _userManager.GetUserAsync(User);
            bool isAuthorized = User.IsInRole("Admin") || User.IsInRole("Staff") || post.AuthorId == currentUser?.Id;

            if (!isAuthorized)
            {
                return Forbid(); // User is authenticated but not allowed to edit this post
            }

            var viewModel = new PostFormViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CategorySelection = post.CategoryId.ToString(),
                Categories = await GetCategoriesAsync()
            };

            // Returns the dedicated Views/Posts/Edit.cshtml view
            return View(viewModel);
        }


        [HttpPost("Posts/Edit/{id:int}")]
        [Authorize] // Authorize attribute checks authentication; specific checks below
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,CategorySelection,NewCategoryName")] PostFormViewModel model)
        {
            // Prevent parameter tampering: Route ID must match Form ID
            if (id != model.Id || id <= 0)
            {
                return BadRequest("ID mismatch or invalid ID provided.");
            }

            // Resolve category first
            var categoryResult = await ResolveCategoryIdAsync(model);

            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategoriesAsync();
                // Returns the Edit view with current model data and validation errors
                return View(model);
            }

            if (!categoryResult.isValid || !categoryResult.categoryId.HasValue)
            {
                model.Categories = await GetCategoriesAsync();
                return View(model); // Return to Edit view
            }

            // Fetch the existing post *again* from DB for security and concurrency checks
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                TempData["ErrorMessage"] = "Post not found. It might have been deleted.";
                return RedirectToAction(nameof(Index));
            }

            // Authorization check again for the POST request
            var currentUser = await _userManager.GetUserAsync(User);
            bool isAuthorized = User.IsInRole("Admin") || User.IsInRole("Staff") || post.AuthorId == currentUser?.Id;
            if (!isAuthorized)
            {
                return Forbid();
            }

            // Update post properties
            post.Title = model.Title;
            post.Content = model.Content;
            post.CategoryId = categoryResult.categoryId.Value;
            post.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Post updated successfully.";
                return RedirectToAction(nameof(Details), new { id = post.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle case where data was changed between loading and saving
                if (!await _context.Posts.AnyAsync(e => e.Id == id))
                {
                    return NotFound($"Post with ID {id} was deleted during the edit process.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "The post has been modified by another user. Please reload the data and try again.");
                    model.Categories = await GetCategoriesAsync();
                    return View(model); // Return to Edit view
                }
            }
            catch (Exception /* ex */) // Catch other potential DB errors
            {
                // Log the exception ex
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while saving the post.");
                model.Categories = await GetCategoriesAsync();
                return View(model); // Return to Edit view
            }
        }


        [HttpPost("Posts/DeletePost/{id:int}")]
        [Authorize(Roles = "Staff,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid post ID.");
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                TempData["ErrorMessage"] = "Post not found.";
                return RedirectToAction(nameof(Index));
            }

            // Remove associated comments first (safer than relying solely on cascade delete)
            var commentsToRemove = await _context.Comments.Where(c => c.PostId == id).ToListAsync();
            if (commentsToRemove.Any())
            {
                _context.Comments.RemoveRange(commentsToRemove);
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Post '{post.Title}' and its comments were deleted successfully.";
            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment([Bind("postId, content")] CommentInputModel input)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid comment data.";
                if (input.postId > 0)
                    return RedirectToAction(nameof(Details), new { id = input.postId });
                else
                    return RedirectToAction(nameof(Index)); // Fallback
            }

            // Manual check still useful for specific rules not covered by attributes
            if (string.IsNullOrWhiteSpace(input.content) || input.content.Length > 1000)
            {
                TempData["ErrorMessage"] = "Comment cannot be empty or exceed 1000 characters.";
                return RedirectToAction(nameof(Details), new { id = input.postId });
            }

            var postExists = await _context.Posts.AnyAsync(p => p.Id == input.postId);
            if (!postExists)
            {
                return NotFound($"Post with ID {input.postId} not found.");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) { return Challenge(); }

            // Prevent banned users from commenting
            if (currentUser.IsBanned)
            {
                TempData["ErrorMessage"] = "Your account is suspended, and you cannot post comments.";
                return RedirectToAction(nameof(Details), new { id = input.postId });
            }

            var comment = new Comment
            {
                Content = input.content.Trim(),
                AuthorId = currentUser.Id,
                PostId = input.postId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Comment added successfully.";
            return RedirectToAction(nameof(Details), new { id = input.postId });
        }

        [HttpPost("Posts/DeleteComment/{id:int}")]
        [Authorize(Roles = "Staff,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id, [Bind("postId")] DeleteCommentInputModel input)
        {
            if (id <= 0 || input.postId <= 0)
            {
                return BadRequest("Invalid comment or post ID.");
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                TempData["ErrorMessage"] = "Comment not found.";
                return RedirectToAction(nameof(Details), new { id = input.postId });
            }

            // Verify comment belongs to the specified post for consistency
            if (comment.PostId != input.postId)
            {
                return BadRequest("Comment does not belong to the specified post.");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) { return Challenge(); }

            // Authorization: Admin or comment author
            if (!User.IsInRole("Admin") && comment.AuthorId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You are not authorized to delete this comment.";
                return RedirectToAction(nameof(Details), new { id = comment.PostId });
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Comment deleted successfully.";
            return RedirectToAction(nameof(Details), new { id = comment.PostId });
        }


        // --- Private Helper Methods ---

        private async Task<(int? categoryId, bool isValid)> ResolveCategoryIdAsync(PostFormViewModel model)
        {
            int categoryId;

            if (model.CategorySelection == "new")
            {
                if (string.IsNullOrWhiteSpace(model.NewCategoryName))
                {
                    ModelState.AddModelError(nameof(model.NewCategoryName), "New category name is required when 'Create New' is selected.");
                    return (null, false);
                }

                string trimmedNewName = model.NewCategoryName.Trim();
                string categoryNameToCompareLower = trimmedNewName.ToLowerInvariant();

                // Prevent duplicate category names (case-insensitive check)
                bool exists = await _context.Categories
                    .AnyAsync(c => c.Name.ToLower() == categoryNameToCompareLower);

                if (exists)
                {
                    ModelState.AddModelError(nameof(model.NewCategoryName), $"A category named '{trimmedNewName}' already exists.");
                    return (null, false);
                }

                var newCategory = new Category { Name = trimmedNewName };
                _context.Categories.Add(newCategory);
                await _context.SaveChangesAsync(); // Save immediately to get the ID
                categoryId = newCategory.Id;
            }
            else if (int.TryParse(model.CategorySelection, out categoryId))
            {
                // Verify the selected category ID actually exists
                bool validCategory = await _context.Categories.AnyAsync(c => c.Id == categoryId);
                if (!validCategory)
                {
                    ModelState.AddModelError(nameof(model.CategorySelection), "The selected category is invalid.");
                    return (null, false);
                }
            }
            else // Handle cases where selection is empty or invalid
            {
                ModelState.AddModelError(nameof(model.CategorySelection), "Please select a valid category or choose 'Create New'.");
                return (null, false);
            }

            return (categoryId, true);
        }

        private async Task<List<SelectListItem>> GetCategoriesAsync()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            // Add helper options for the dropdown
            categories.Insert(0, new SelectListItem { Value = "", Text = "-- Select Category --", Disabled = true, Selected = true });
            categories.Insert(1, new SelectListItem { Value = "new", Text = "-- Create New Category --" });

            return categories;
        }

    } // End Controller

    // --- Supporting Input Models ---
    public class CommentInputModel
    {
        [Required]
        public int postId { get; set; }

        [Required(ErrorMessage = "Comment content cannot be empty.")]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string content { get; set; } = string.Empty; // Initialize to avoid null warnings
    }

    public class DeleteCommentInputModel
    {
        [Required]
        public int postId { get; set; }
    }

} // End Namespace