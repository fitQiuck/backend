using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenessansAPI.DataAccess.AppDBContexts;
using RenessansAPI.Domain.Enums;
using RenessansAPI.Service.DTOs.PermissionsDto;
using RenessansAPI.Service.DTOs.RolesDto;
using RenessansAPI.Service.DTOs.UsersDto;
using RenessansAPI.Service.IService;

namespace RenessansAPI.Service.Seeders;

public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        IUserService userService,
        IPermissionService permissionService,
        IRoleService roleService,
        ILogger logger)
    {
        var permissions = new List<PermissionForCreationDto>
        {
            new() { Id = Guid.Parse("1905e421-80d5-49f1-9b4b-904e2369006f"), Name = "Permission_Get", Description = "Permission_Get"},
            new() { Id = Guid.Parse("782a10cb-a697-4389-bd43-dc43d21d59bf"), Name = "Permission_Delete", Description = "Permission_Delete"},
            new() { Id = Guid.Parse("e7e8d3d0-1388-4742-8897-d5af6f1afc53"), Name = "Permission_Update", Description = "Permission_Update"},
            new() { Id = Guid.Parse("4f42f1a6-0abb-4678-8744-cb24d209cb2c"), Name = "Permission_Create", Description = "Permission_Create"},

            new() { Id = Guid.Parse("a644ad33-fdc0-4137-aa81-2389d1caaac1"), Name = "Role_Get", Description = "Role_Get"},
            new() { Id = Guid.Parse("229c93fa-9757-44c0-8ffc-bd197b4e0212"), Name = "Role_Delete", Description = "Role_Delete"},
            new() { Id = Guid.Parse("f565ff60-a69c-4bc6-b6c3-9e7dd4ca6b99"), Name = "Role_Update", Description = "Role_Update"},
            new() { Id = Guid.Parse("780ae01b-a51f-437e-8ffd-190b08c141da"), Name = "Role_Create", Description = "Role_Create"},

            new() { Id = Guid.Parse("0196ec44-a809-780c-9292-f45abf7ae43d"), Name = "User_Get", Description = "User_Get" },
            new() { Id = Guid.Parse("0196ec45-6a24-775c-91cb-002e412b3dfb"), Name = "User_Create", Description = "User_Create" },
            new() { Id = Guid.Parse("0196ec45-94a7-75ac-8403-2e004b739e43"), Name = "User_Delete", Description = "User_Delete" },
            new() { Id = Guid.Parse("0196ec45-c6bf-7e94-b5bf-5abff77cd823"), Name = "User_Update", Description = "User_Update" }
        };

        foreach (var permission in permissions)
        {
            if (!await db.Permissions.AnyAsync(p => p.Id == permission.Id))
            {
                await permissionService.CreateAsync(permission);
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation(" Permissionlar tekshirildi va keraklilari yaratildi.");

        // 2. SuperAdmin rolini yaratish yoki yangilash
        var superAdmin = await db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == "SuperAdmin");

        var allPermissionIds = await db.Permissions.Select(p => p.Id).ToListAsync();
        Guid roleId;

        if (superAdmin is null)
        {
            var role = await roleService.CreateAsync(new RoleForCreationDto
            {
                Name = "SuperAdmin",
                Description = "All-access Super Admin",
                RolePermissions = allPermissionIds
            });

            roleId = role.Id;
            await db.SaveChangesAsync();
            logger.LogInformation(" SuperAdmin roli yaratildi va barcha permissionlar biriktirildi.");
        }
        else
        {
            logger.LogInformation("ℹ️ SuperAdmin roli mavjud. ID: {RoleId}", superAdmin.Id);
            roleId = superAdmin.Id;
        }

        // 3. Gest roli (faqat agar mavjud bo‘lmasa)
        var gestRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Gest");
        if (gestRole is null)
        {

            await roleService.CreateAsync(new RoleForCreationDto
            {
                Name = "Gest",
                Description = "Starter user",
                RolePermissions = allPermissionIds
            });
            await db.SaveChangesAsync();
            logger.LogInformation(" Gest roli yaratildi.");
        }
        else
        {
            logger.LogInformation(" Gest roli mavjud. ID: {RoleId}", gestRole.Id);
        }

        // 4. Admin foydalanuvchini yaratish (agar mavjud bo‘lmasa)
        if (!await db.Users.AnyAsync(u => u.UserName == "Admin"))
        {
            var userDto = new UserForCreationDto
            {
                FirstName = "Admin",
                LastName = "Admin",
                UserName = "Admin",
                Email = "bekhzodkeldiyorov@gmail.com",
                Password = "tV0N2Oq4xCnPhciMVH", 
                UserStatus = UserStatus.Active,
                RolesId = roleId
            };

            await userService.CreateAsync(userDto);
            await db.SaveChangesAsync();
            logger.LogInformation(" Admin foydalanuvchi yaratildi va SuperAdmin roliga biriktirildi.");
        }
        else
        {
            logger.LogInformation(" Admin foydalanuvchi allaqachon mavjud.");
        }
    }
}
