using System;

namespace UserGroups {
  public enum PermissionLevel {
    User,
    Moderator,
    Admin,
    SuperAdmin,
    Developer
  }
  public class UserGroup {
    /// <summary>
    /// The name of the user group. This cannot contain spaces or special characters.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The display name of the user group.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// The permission level of the user group. Higher levels have more permissions.
    /// 100 - Developer
    /// 99 - superadmin
    /// </summary>
    public PermissionLevel PermissionLevel { get; set; }

    public Color Color { get; set; }

    public UserGroup(string name, string displayName, PermissionLevel permissionLevel, Color color) {
      // No real reason. Just keeps it clean.
      if ( name.Contains(" ") ) {
        throw new ArgumentException("User group name cannot contain spaces.");
      }
      Name = name;
      DisplayName = displayName;
      PermissionLevel = permissionLevel;
      Color = color;
      Log.Info($"User group {DisplayName} created with permission level {PermissionLevel}.");
    }
  }
}