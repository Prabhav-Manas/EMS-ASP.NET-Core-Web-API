namespace backend.Models.Auth.DTOs;

public class LoginResponse
{
    public string Message {get; set;} = string.Empty;
    public string UserId {get; set;} = string.Empty;
    public string AccessToken {get; set;} = string.Empty;
    public int ExpiresIn {get; set;}
    public string Fullname {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string? ProfilePhoto {get; set;} 
}