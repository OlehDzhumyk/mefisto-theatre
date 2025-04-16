using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using O_Dzhumyk_MefistoTheatre.Data;
using O_Dzhumyk_MefistoTheatre.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public static class SeedData
{
    public static async Task InitializeAsync(
        ApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await context.Database.MigrateAsync();

        // --- 1. Create Roles ---
        var roles = new[] { "Admin", "Staff", "Member" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // --- 2. Create Users (Expanded) ---

        // Helper function remains the same
        async Task<User?> CreateUserIfNotExists(string email, string fullName, string password, string[] userRoles, bool isBanned = false)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true,
                    IsBanned = isBanned,
                    AboutMe = $"Seed user: {fullName}"
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(user, userRoles);
                    Console.WriteLine($"Successfully created user: {email}"); // Added success log
                    return user;
                }
                else
                {
                    Console.WriteLine($"Error creating user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    return null;
                }
            }
            return user;
        }

        // --- Define User Lists ---
        var adminUsers = new List<User>();
        var staffUsers = new List<User>();
        var memberUsers = new List<User>();
        var bannedUsers = new List<User>();

        // --- Create Core Users ---
        var adminUser = await CreateUserIfNotExists("admin@example.com", "Alice Admin", "Admin@123", new[] { "Admin", "Staff" });
        var staffUser1 = await CreateUserIfNotExists("staff1@example.com", "Bob Staff", "Staff@123", new[] { "Staff" });
        var memberUser1 = await CreateUserIfNotExists("member1@example.com", "Charlie Member", "Member@123", new[] { "Member" });
        var bannedUser1 = await CreateUserIfNotExists("banned1@example.com", "David Banned", "Banned@123", new[] { "Member" }, isBanned: true);

        if (adminUser != null) adminUsers.Add(adminUser);
        if (staffUser1 != null) staffUsers.Add(staffUser1);
        if (memberUser1 != null) memberUsers.Add(memberUser1);
        if (bannedUser1 != null) bannedUsers.Add(bannedUser1);

        // --- Create Additional Staff Users ---
        var staffUser2 = await CreateUserIfNotExists("staff2@example.com", "Eve Equity", "Staff@123", new[] { "Staff" });
        var staffUser3 = await CreateUserIfNotExists("staff3@example.com", "Frank FrontOfHouse", "Staff@123", new[] { "Staff" });
        if (staffUser2 != null) staffUsers.Add(staffUser2);
        if (staffUser3 != null) staffUsers.Add(staffUser3);

        // --- Create Additional Member Users ---
        for (int i = 2; i <= 12; i++) // Create members 2 through 12
        {
            var member = await CreateUserIfNotExists($"member{i}@example.com", $"Member User {i}", "Member@123", new[] { "Member" });
            if (member != null) memberUsers.Add(member);
        }

        // Combine all non-banned users for easier random selection later
        var allActiveUsers = new List<User>();
        allActiveUsers.AddRange(adminUsers);
        allActiveUsers.AddRange(staffUsers);
        allActiveUsers.AddRange(memberUsers);

        // Combine all users including banned for comment generation
        var allUsersIncludingBanned = new List<User>();
        allUsersIncludingBanned.AddRange(allActiveUsers);
        allUsersIncludingBanned.AddRange(bannedUsers);


        // Check if we have at least the essential users needed
        if (!adminUsers.Any() || !staffUsers.Any() || !memberUsers.Any())
        {
            Console.WriteLine("Error: Could not ensure base users exist. Aborting further seeding.");
            return;
        }


        // --- 3. Seed Categories ---
        // (Keep category seeding logic as before)
        Category? generalCategory = null;
        Category? announcementsCategory = null;
        Category? performancesCategory = null;
        Category? behindScenesCategory = null;
        Category? communityCategory = null;

        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new Category { Name = "General" },
                new Category { Name = "Announcements" },
                new Category { Name = "Performances" },
                new Category { Name = "Behind the Scenes" },
                new Category { Name = "Community" }
            };
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
            Console.WriteLine("Successfully seeded categories."); // Log success
        }

        generalCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "General");
        announcementsCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Announcements");
        performancesCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Performances");
        behindScenesCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Behind the Scenes");
        communityCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Community");

        if (generalCategory == null || announcementsCategory == null || performancesCategory == null || behindScenesCategory == null || communityCategory == null)
        {
            Console.WriteLine("Error: Could not ensure base categories exist. Aborting further seeding.");
            return;
        }

        // --- 4. Seed Posts (Expanded) ---
        if (!await context.Posts.AnyAsync())
        {
            Console.WriteLine("Seeding posts..."); // Log start
            var posts = new List<Post>();
            var random = new Random();
            var baseDate = DateTime.UtcNow.AddYears(-1); // Start posts from 1 year ago

            // Use Admin and Staff for posts
            var postAuthors = new List<User>();
            postAuthors.AddRange(adminUsers);
            postAuthors.AddRange(staffUsers);
            if (!postAuthors.Any())
            {
                Console.WriteLine("Error: No Admin or Staff users available to author posts.");
                return; // Can't proceed without authors
            }


            for (int i = 1; i <= 30; i++) // Increased to 30 posts
            {
                // Pick a random author from Admins/Staff
                var author = postAuthors[random.Next(postAuthors.Count)];

                Category category;
                switch (i % 5) // Cycle through categories
                {
                    case 0: category = announcementsCategory; break;
                    case 1: category = performancesCategory; break;
                    case 2: category = behindScenesCategory; break;
                    case 3: category = communityCategory; break;
                    default: category = generalCategory; break;
                }

                string title = $"Mefisto Update {i}: Focus on {category.Name}";
                // Use the updated content generator (no image)
                string content = GeneratePostContent(i, category.Name);

                posts.Add(new Post
                {
                    Title = title,
                    Content = content,
                    AuthorId = author.Id,
                    CategoryId = category.Id,
                    CreatedAt = baseDate.AddDays(random.Next(365)).AddHours(random.Next(24)), // Random date within the last year
                    Author = author // Explicitly set navigation property if needed immediately (usually not necessary)
                });
            }

            context.Posts.AddRange(posts);
            await context.SaveChangesAsync();
            Console.WriteLine($"Successfully seeded {posts.Count} posts."); // Log success
        }

        // --- 5. Seed Comments (Expanded Users & Refined Text) ---
        var firstPost = await context.Posts.OrderBy(p => p.Id).FirstOrDefaultAsync();
        bool commentsSeeded = firstPost != null && await context.Comments.AnyAsync(c => c.PostId == firstPost.Id);

        if (!commentsSeeded && firstPost != null && allUsersIncludingBanned.Any())
        {
            Console.WriteLine("Seeding comments..."); // Log start
            var allPosts = await context.Posts.Include(p => p.Author).ToListAsync(); // Include author if needed for context
            var random = new Random();
            var commentsToAdd = new List<Comment>();

            foreach (var post in allPosts)
            {
                int numberOfComments = random.Next(8, 21); // Add 8 to 20 comments per post
                for (int i = 0; i < numberOfComments; i++)
                {
                    // Pick a random commenter from ALL users (incl. banned)
                    // Adjust probabilities if desired
                    User commenter = allUsersIncludingBanned[random.Next(allUsersIncludingBanned.Count)];

                    var maxHoursAfterPost = Math.Max(1, (int)(DateTime.UtcNow - post.CreatedAt).TotalHours); // Ensure at least 1 hour range
                    var commentDate = post.CreatedAt.AddHours(random.Next(1, maxHoursAfterPost));
                    if (commentDate > DateTime.UtcNow) commentDate = DateTime.UtcNow;

                    commentsToAdd.Add(new Comment
                    {
                        PostId = post.Id,
                        AuthorId = commenter.Id,
                        // Use the updated comment generator (no index number)
                        Content = GenerateCommentContent(commenter.FullName ?? "User", post.Title),
                        CreatedAt = commentDate,
                        Author = commenter // Set navigation property if needed immediately
                    });
                }
            }

            context.Comments.AddRange(commentsToAdd);
            await context.SaveChangesAsync();
            Console.WriteLine($"Successfully seeded {commentsToAdd.Count} comments."); // Log success
        }
        else if (commentsSeeded)
        {
            Console.WriteLine("Comments already seeded, skipping."); // Log skip
        }
        else if (firstPost == null)
        {
            Console.WriteLine("No posts found, cannot seed comments."); // Log skip due to no posts
        }
        else if (!allUsersIncludingBanned.Any())
        {
            Console.WriteLine("No users found, cannot seed comments."); // Log skip due to no users
        }
    }


    // Updated Helper: Generate post content (removed image placeholder)
    private static string GeneratePostContent(int postIndex, string categoryName)
    {
        string loremShort = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer nec odio. Praesent libero. Sed cursus ante dapibus diam.";
        string loremMedium = "Sed nisi. Nulla quis sem at nibh elementum imperdiet. Duis sagittis ipsum. Praesent mauris. Fusce nec tellus sed augue semper porta. Mauris massa. Vestibulum lacinia arcu eget nulla. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos.";
        string loremLong = "Curabitur sodales ligula in libero. Sed dignissim lacinia nunc. Curabitur tortor. Pellentesque nibh. Aenean quam. In scelerisque sem at dolor. Maecenas mattis. Sed convallis tristique sem. Proin ut ligula vel nunc egestas porttitor. Morbi lectus risus, iaculis vel, suscipit quis, luctus non, massa. Fusce ac turpis quis ligula lacinia aliquet. Mauris ipsum. Nulla metus metus, ullamcorper vel, tincidunt sed, euismod in, nibh. Quisque volutpat condimentum velit.";
        string theatreDesc = @"
            <p>At Mefisto Theatre, Glasgow, we are passionate about performance. Our mission is to ignite imaginations and provoke conversations through diverse and captivating theatre.</p>
            <p>From classic dramas to contemporary experimental pieces, our repertoire reflects the vibrant spirit of our city. We are more than a stage; we are a community landmark where stories come alive.</p>
            <p>Join us in celebrating the transformative power of theatre.</p>
        ";

        switch (categoryName)
        {
            case "Announcements":
                return $"<h4>Important Update #{postIndex}</h4><p>Exciting news from Mefisto Theatre! We're thrilled to announce details about our upcoming season. {loremShort}</p><p>{loremMedium}</p>";
            case "Performances":
                return $"<h4>Performance Spotlight: Show {postIndex}</h4><p>Reflecting on a recent performance, the audience was captivated by... {loremMedium}</p><p>The direction and stage design perfectly complemented the actors' talents. {loremShort}</p>";
            case "Behind the Scenes":
                // Image placeholder removed
                return $"<h4>A Glimpse Backstage (Post {postIndex})</h4><p>What does it take to bring a Mefisto production to life? Meet the dedicated crew working tirelessly behind the curtains. {loremMedium}</p><p>From lighting technicians to costume designers, every role is crucial. {loremLong}</p>";
            case "Community":
                return $"<h4>Mefisto in the Community #{postIndex}</h4><p>We believe theatre is for everyone. Read about our latest community outreach program and workshops. {theatreDesc}</p><p>{loremShort}</p>";
            case "General":
            default:
                return $"<h4>Theatre Musings {postIndex}</h4><p>A space for general thoughts and discussions on the world of performing arts. {loremShort}</p><blockquote>{loremMedium}</blockquote><p>{loremLong}</p>";
        }
    }

    // Updated Helper: Generate comment content (removed index number)
    private static string GenerateCommentContent(string authorName, string postTitle)
    {
        // Added more varied comments
        string[] comments = {
            "Great post!", "Really insightful, thanks.", "I agree completely.",
            "Interesting perspective on this.", "Looking forward to more details.", "Wow, fascinating!",
            $"Thanks for the update, {authorName}!", "Can't wait!", "This is very helpful information.",
            $"Regarding '{postTitle.Substring(0, Math.Min(postTitle.Length, 20)).Trim()}', I feel...",
            "Could you expand on that a bit?", "Well written post.", "Fantastic news!", "Keep up the great work!",
            "This clarifies things significantly.", "Nice summary, very clear.", "Love seeing this!", "I attended the last event, it was amazing!",
            "Does this affect ticket prices?", "When will more info be available?", "So exciting!", "Glad to see Mefisto thriving!"
        };
        var random = new Random();
        // Just return the random comment text
        return comments[random.Next(comments.Length)];
    }
}