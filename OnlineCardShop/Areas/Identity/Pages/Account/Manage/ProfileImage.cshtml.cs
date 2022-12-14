using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineCardShop.Controllers;
using OnlineCardShop.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.IO;
using OnlineCardShop.Data;
using System.Linq;
using OnlineCardShop.Services.Users;

namespace OnlineCardShop.Areas.Identity.Pages.Account.Manage
{
    public class ProfileImage : PageModel
    {
        private readonly UserManager<User> userManager;
        private readonly IWebHostEnvironment env;
        private readonly IUserService users;

        public ProfileImage(UserManager<User> userManager,
            IWebHostEnvironment env,
            IUserService users)
        {
            this.userManager = userManager;
            this.env = env;
            this.users = users;
        }

        public class ChangeProfileImageFormModel
        {
            [Display(Name = "Profile Image")]
            public Data.Models.ProfileImage ProfileImage { get; init; }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Profile Image")]
            public IFormFile profileImageFile { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if(user == null)
            {
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.profileImageFile == null)
            {
                this.ModelState.AddModelError(nameof(Input.profileImageFile), "There is no image selected");
            }

            if (ModelState.IsValid)
            {

                var wwwPath = this.env.WebRootPath;
                var imageDirectory = WebConstants.profileImageDirectory;
                Data.Models.ProfileImage profileImage = new();

                if (ImageIsWithinDesiredSize(Input.profileImageFile))
                {
                    string originalImageName, imageName, imagePath, imagePathForDb;

                    ProcessImageDetails(Input.profileImageFile, wwwPath, imageDirectory, out originalImageName, out imageName, out imagePath, out imagePathForDb);

                    profileImage = this.users.CreateProfileImage(imageName, imagePathForDb, originalImageName);

                    var currentUserId = userManager.GetUserId(User);

                    var currentProfileImagePath = this.users.GetProfileImagePath(currentUserId).Split('/');

                    System.IO.File.Delete(Path.Combine(wwwPath, currentProfileImagePath[0], currentProfileImagePath[1].Replace("res", string.Empty)));
                    System.IO.File.Delete(Path.Combine(wwwPath, currentProfileImagePath[0], currentProfileImagePath[1]));

                    this.users.AddProfileImageToDB(profileImage);

                    using (var fileStream = System.IO.File.Create(imagePath))
                    {
                        await Input.profileImageFile.CopyToAsync(fileStream);
                    }

                    var currentUser = this.users.GetUser(currentUserId);

                    this.users.ChangeProfileImage(currentUser, profileImage.Id);
                }
            }
            return Page();
        }
        private static bool ImageIsWithinDesiredSize(IFormFile imageFile)
        {
            return imageFile.Length > 0 && imageFile.Length <= (2 * 1024 * 1024);
        }

        private static void ProcessImageDetails(IFormFile imageFile, string wwwPath, string imageDirectory, out string originalImageName, out string imageName, out string imagePath, out string imagePathForDb)
        {
            var imageExtension = Path.GetExtension(imageFile.FileName);

            originalImageName = imageFile.FileName;
            imageName = Path.GetRandomFileName() + imageExtension;
            imagePath = Path.Combine(wwwPath, imageDirectory, imageName);
            imagePathForDb = imageDirectory + "/" + "res" + imageName;
        }
    }
}
