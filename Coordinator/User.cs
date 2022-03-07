namespace Coordinator;

public class User
{
    public long TelegramId { get; } 
    
    public string FirstName  { get; } 
    
    public string? Language  { get; } 
    
    public string? City  { get; } 

    
    public User(long telegramId, string firstName, string? language = null, string? city=null)
    {
        TelegramId = telegramId;
        FirstName = firstName;
        Language = language;
        City = city;
    }
    

}