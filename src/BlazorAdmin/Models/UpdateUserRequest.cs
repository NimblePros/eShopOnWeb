namespace BlazorAdmin.Models;

public class UpdateUserRequest
{
    public User User{  get; set; }

    public UpdateUserRequest()
    {
        User = new();
    }
}
