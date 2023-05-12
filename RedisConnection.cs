using StackExchange.Redis;
using Newtonsoft.Json;

public class RedisConnection
{
    private ConnectionMultiplexer _redis;
    public RedisConnection(string connectionString)
    {
        _redis = ConnectionMultiplexer.Connect(connectionString);
    }

    public void AddTwitchChatMessage(TwitchChatMessage message){
        IDatabase db = _redis.GetDatabase();
        db.StringSet(message.Id.ToString(), JsonConvert.SerializeObject(message));
    }

    public TwitchChatMessage GetTwitchChatMessage(string id){
        IDatabase db = _redis.GetDatabase();
        string? message = db.StringGet(id);
        if(message != null){
            return JsonConvert.DeserializeObject<TwitchChatMessage>(message);
        }
        return null;
    }

    public void DeleteTwitchChatMessage(string id){
        IDatabase db = _redis.GetDatabase();
        db.KeyDelete(id);
    }

    public List<TwitchChatMessage> GetTwitchChatMessageByUsername(string username){
        IDatabase db = _redis.GetDatabase();
        var keys = _redis.GetServer(_redis.GetEndPoints()[0]).Keys(pattern: "*").ToList();
        List<TwitchChatMessage> messages = new List<TwitchChatMessage>();
        foreach(var key in keys){
            string? message = db.StringGet(key);
            if(message != null){
                TwitchChatMessage twitchChatMessage = JsonConvert.DeserializeObject<TwitchChatMessage>(message);
                if(twitchChatMessage.Username == username){
                    messages.Add(twitchChatMessage);
                }
            }
        }
        //sort messages by timestamp
        messages.Sort((x, y) => DateTime.Compare(x.Timestamp.Value, y.Timestamp.Value));
        return messages;
    }
}