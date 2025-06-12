namespace BlazorAdmin.Models;

public class CreateUserRequest
{
    public User User{  get; set; }

    public CreateUserRequest()
    {
        User = new();
    }
}
