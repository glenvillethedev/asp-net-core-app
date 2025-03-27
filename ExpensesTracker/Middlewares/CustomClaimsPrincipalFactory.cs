using ExpensesTracker.Models.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace ExpensesTracker.Middlewares
{
    /// <summary>
    /// Class used to add Custom Claims during login. (e.g. User's FullName)
    /// </summary>
    public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, AppRole>
    {
        public CustomClaimsPrincipalFactory(UserManager<AppUser> userManager,
                                            RoleManager<AppRole> roleManager,
                                            IOptions<IdentityOptions> optionsAccessor) : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var claimsIdentity = await base.GenerateClaimsAsync(user);

            // Add Custom Claims Here
            claimsIdentity.AddClaim(new Claim("FullName", user.Name ?? user.UserName));

            return claimsIdentity;
        }
    }
}
