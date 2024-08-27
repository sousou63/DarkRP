using System.Text.Json.Serialization;
using Sandbox;

namespace Scenebox;

public static class SceneboxPreferences
{

    public static SceneboxSettings Settings
    {
        get
        {
            if ( _settings is null )
            {
                var file = "/settings.json";
                _settings = FileSystem.Data.ReadJson( file, new SceneboxSettings() );
            }
            return _settings;
        }
    }
    static SceneboxSettings _settings;

    public static void Save()
    {
        FileSystem.Data.WriteJson( "/settings.json", Settings );
    }

}

public class SceneboxSettings
{
    public bool HostMultiplayer { get; set; } = true;

    public float FieldOfView { get; set; } = 90f;
}