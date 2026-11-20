namespace backend.Models.Auth.DTOs;

public class SignupResponse
{
    public string Message {get; set;} = string.Empty;
    public string UserId {get; set;} = string.Empty;
    public string Fullname {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string? ProfilePhoto {get; set;}
}