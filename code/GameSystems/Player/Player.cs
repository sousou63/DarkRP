namespace GameSystems.Player
{
  // Should find better name for this class
  // like Player
  public class PlayerConnObject
  {
    public string Name { get; set; }
    public List<UserGroup> UserGroups { get; set; }
    public GameObject GameObject { get; set; }
    public Connection Connection { get; set; }

    public PlayerConnObject( GameObject gameObject, Connection connection, List<UserGroup> userGroups )
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
      public bool CheckPermission( PermissionLevel permissionLevel )
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

      public void SetRank( UserGroup userGroup )
      {
        UserGroups.Clear();
        UserGroups.Add( userGroup );
      }

      public void AddRank( UserGroup userGroup )
      {
        UserGroups.Add( userGroup );
      }

      public void RemoveRank( UserGroup userGroup )
      {
        UserGroups.Remove( userGroup );
      }

      public UserGroup GetHighestUserGroup()
      {
        return UserGroups.MaxBy(x => x.PermissionLevel);
    }
  }
}
