namespace Scenebox.Tools;

public abstract class BaseTool
{
    public Toolgun Toolgun;

    public virtual string Attack1Control => "";
    public virtual string Attack2Control => "";
    public virtual string ReloadControl => "";

    public virtual void OnEquip()
    {

    }

    public virtual void OnUnequip()
    {

    }

    public virtual void PrimaryUseStart()
    {

    }

    public virtual void PrimaryUseUpdate()
    {

    }

    public virtual void PrimaryUseEnd()
    {

    }

    public virtual void SecondaryUseStart()
    {

    }

    public virtual void SecondaryUseUpdate()
    {

    }

    public virtual void SecondaryUseEnd()
    {

    }

    public string GetName()
    {
        return TypeLibrary.GetAttribute<ToolAttribute>( GetType() ).Title;
    }

    public string GetDescription()
    {
        return TypeLibrary.GetAttribute<ToolAttribute>( GetType() ).Description;
    }

    public string GetLongDescription()
    {
        var attr = TypeLibrary.GetAttribute<DescriptionAttribute>( GetType() );
        if ( string.IsNullOrWhiteSpace( attr?.Value ) )
            return GetDescription();
        return attr.Value;
    }

    public string GetGroup()
    {
        return TypeLibrary.GetAttribute<ToolAttribute>( GetType() ).Group;
    }

    public List<(string, string)> GetControls()
    {
        var controls = new List<(string, string)>();

        if ( !string.IsNullOrEmpty( Attack1Control ) )
            controls.Add( ("Attack1", Attack1Control) );

        if ( !string.IsNullOrEmpty( Attack2Control ) )
            controls.Add( ("Attack2", Attack2Control) );

        if ( !string.IsNullOrEmpty( ReloadControl ) )
            controls.Add( ("Reload", ReloadControl) );

        return controls;
    }
}