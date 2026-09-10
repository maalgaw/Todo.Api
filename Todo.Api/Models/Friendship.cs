using System.Text.Json.Serialization;

namespace Todo.Api.Models;

public enum FriendshipStatus
{
    Pending = 0,
    Accepted = 1
}

public class Friendship
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    [JsonIgnore]
    public User? User { get; set; }
    
    public int FriendId { get; set; }
    [JsonIgnore]
    public User? Friend { get; set; }
    
    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
