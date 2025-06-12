namespace BlazorAdmin.Models;

public class GetUserResponse
{
    public User User { get; set; }

    public GetUserResponse()
    {
        User = new();
    }
}
