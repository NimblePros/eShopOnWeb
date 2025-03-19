using System.Threading.Tasks;
using BlazorAdmin.Models;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace BlazorAdmin.Interfaces;

public interface IUserManagementService
{
    Task<CreateUserResponse> Create(ApplicationUser user);
    Task<ApplicationUser> Edit(ApplicationUser user);
    Task Delete(string id);
    Task<ApplicationUser> GetById(string id);
    Task<UserListResponse> List();
}
