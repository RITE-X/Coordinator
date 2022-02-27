using System.Configuration;

namespace Coordinator.App;

public class BotSettingsInit
{
    public static readonly string Token;
    
    public static readonly long AdminChatId;
    
    static BotSettingsInit()
    {
        
        Token = ConfigurationManager.AppSettings["token"];
        AdminChatId= Convert.ToInt64(ConfigurationManager.AppSettings["adminId"]);
            
    }
}

