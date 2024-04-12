using Microsoft.AspNetCore.Identity;

namespace Intextwo.Models.ViewModels
{
    public class EditUserViewModel
    {
        public IdentityUser User { get; set; }
        public List<string> Roles { get; set; }
        public List<string> SelectedRoles { get; set; }

    }
}
