using System.Threading.Tasks;
using BlazorAdmin.Models;

namespace BlazorAdmin.Interfaces;

public interface IUserManagementService
{
    Task<CreateUserResponse> Create(CreateUserRequest user);
    Task<GetUserResponse> Edit(GetUserResponse user);
    Task Delete(string id);
    Task<GetUserResponse> GetById(string id);
    Task<GetUserResponse> GetByName(string userName);
    Task<UserListResponse> List();
}
