// This is a simple controller class that defines an endpoint for fetching user information.
// public class UserController
// {
//     [HttpGet("/user")]
//     public string GetUser()
//     {
//         return "Fetching user from UserController";
//     }
// }

public class User { 
    public int Id { get; set; } 
    public string Name { get; set; } 
}

public class UserController
{
    private static List<User> _users = new() { new User { Id = 1, Name = "Admin" } };

    [HttpGet("/users")]
    public string GetAll() => System.Text.Json.JsonSerializer.Serialize(_users);

    [HttpPost("/users")]
    public string Create(User user)
    {
        user.Id = _users.Count + 1;
        _users.Add(user);
        return "User Created";
    }

    [HttpPut("/users")]
    public string Update(User updatedUser)
    {
        var user = _users.FirstOrDefault(u => u.Id == updatedUser.Id);
        if (user != null)
        {
            user.Name = updatedUser.Name;
            return "User Updated";
        }
        return "User Not Found";
    }

    [HttpDelete("/users/{id}")]
    public string Delete(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            _users.Remove(user);
            return "User Deleted";
        }
        return "User Not Found";
    }
}