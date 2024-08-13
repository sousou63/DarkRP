namespace PlayerDetails
{
  public class Player
  {
    public GameObject GameObject { get; set; }
    public Connection Connection { get; set; }

    public Player( GameObject gameObject, Connection connection )
    {
      GameObject = gameObject;
      Connection = connection;
    }
  }
}