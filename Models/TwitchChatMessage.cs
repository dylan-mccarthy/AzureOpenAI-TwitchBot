public class TwitchChatMessage
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Message { get; set; }
    public DateTime? Timestamp { get; set; }

    public TwitchChatMessage(string username, string message)
    {
        Id = Guid.NewGuid();
        Username = username;
        Message = message;
        Timestamp = DateTime.Now;
    }
}