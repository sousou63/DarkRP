using UserGroups;
namespace PlayerInfo
{
  public class Player
  {
    public string Name { get; set; }
    public List<UserGroup> UserGroups { get; set; }
    public GameObject GameObject { get; set; }
    public Connection Connection { get; set; }

    public Player( GameObject gameObject, Connection connection, List<UserGroup> userGroups )
    {
      GameObject = gameObject;
      Connection = connection;
      Name = connection.DisplayName;
      UserGroups = userGroups;
    }

    /// <summary>
    /// Checks if the player is part of a UserGroup with the required permission level.
    /// Returns true if the player has the required permission level.
    /// </summary>
    public bool CheckPermission( int permissionLevel )
    {
      foreach ( var userGroup in UserGroups )
      {
        if ( userGroup.PermissionLevel >= permissionLevel )
        {
          return true;
        }
      }
      return false;
    }

    public UserGroup GetHighestUserGroup()
    {
      UserGroup highestUserGroup = null;
      foreach ( var userGroup in UserGroups )
      {
        if ( highestUserGroup == null || userGroup.PermissionLevel > highestUserGroup.PermissionLevel )
        {
          highestUserGroup = userGroup;
        }
      }
      return highestUserGroup;
    }
  }
}