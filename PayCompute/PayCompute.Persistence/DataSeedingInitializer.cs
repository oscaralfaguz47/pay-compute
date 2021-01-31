using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PayCompute.Persistence
{
    public static class DataSeedingInitializer
    {
        public static async Task UserAndRoleSeedAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Manager", "Staff" };
            foreach (var role in roles)
            {
                var roleExist = await roleManager.RoleExistsAsync(role);
                if (!roleExist)
                {
                    IdentityResult result = await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            //Create Admin User
            if (userManager.FindByEmailAsync("oscar.alfaguz47@gmail.com").Result == null)
            {
                IdentityUser user = new IdentityUser
                {
                    UserName = "oscar.alfaguz47@gmail.com",
                    Email = "oscar.alfaguz47@gmail.com"
                };
                IdentityResult identityResult = userManager.CreateAsync(user, "Password1").Result;
                if (identityResult.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Admin").Wait();
                }
            }

            //Create Manager user
            if (userManager.FindByEmailAsync("oscaralfaro47@hotmail.com").Result == null)
            {
                IdentityUser user = new IdentityUser
                {
                    UserName = "oscaralfaro47@hotmail.com",
                    Email = "oscaralfaro47@hotmail.com"
                };
                IdentityResult identityResult = userManager.CreateAsync(user, "Password1").Result;
                if (identityResult.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Manager").Wait();
                }
            }

            //Create Staff User
            if (userManager.FindByEmailAsync("motorolaz3oscar@gmail.com").Result == null)
            {
                IdentityUser user = new IdentityUser
                {
                    UserName = "motorolaz3oscar@gmail.com",
                    Email = "motorolaz3oscar@gmail.com"
                };
                IdentityResult identityResult = userManager.CreateAsync(user, "Password1").Result;
                if (identityResult.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Staff").Wait();
                }
            }
            //Create  No Role user
            if (userManager.FindByEmailAsync("oscar.alfaro@oceanscode.com").Result == null)
            {
                IdentityUser user = new IdentityUser
                {
                    UserName = "oscar.alfaro@oceanscode.com",
                    Email = "oscar.alfaro@oceanscode.com"
                };
                IdentityResult identityResult = userManager.CreateAsync(user, "Password1").Result;
                //No Role assigned to oscar.alfaro@oceanscode.com
            }

        }

    }
}
