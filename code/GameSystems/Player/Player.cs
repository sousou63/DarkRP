namespace GameSystems.Player
{
  // Should find better name for this class
  // like Player
  public class PlayerConnObject
  {
    public GameObject GameObject { get; set; }
    public Connection Connection { get; set; }

    public PlayerConnObject( GameObject gameObject, Connection connection )
    {
      GameObject = gameObject;
      Connection = connection;
    }
  }
}