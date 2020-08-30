/// <summary>
/// Simple class for storing player data.
/// </summary>
public class PlayerInfo
{
    /// <summary>
    /// Creates a PlayerInfo object
    /// </summary>
    /// <param name="name">The name of the player</param>
    /// <param name="isHost">If the player is the host</param>
    public PlayerInfo(string name, bool isHost)
    {
        this.name = name;
        this.isHost = isHost;
    }

    /// <summary>
    /// The name of the player
    /// </summary>
    public string name;

    /// <summary>
    /// If the player is the host
    /// </summary>
    public bool isHost;
}
