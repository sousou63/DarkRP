namespace Scenebox.Tools;

public class ToolAttribute : System.Attribute
{
    public string Title { get; }
    public string Description { get; }
    public string Group { get; }
    public string LongDescription { get; }

    public ToolAttribute( string title, string desc, string group = "" )
    {
        Title = title;
        Description = desc;
        Group = group;
    }
}