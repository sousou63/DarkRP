using System;

namespace Scenebox;

public class Camera : Weapon, ICameraOverride
{
    public bool IsActive => IsEquipped && Network.IsOwner;

    public float RollOffset { get; set; } = 0f;
    public float FieldOfView { get; set; } = 100f;

    public void UpdateCamera()
    {
        if ( !IsActive ) return;
        if ( !Player.IsFirstPerson ) Player.IsFirstPerson = true;

        if ( Input.Pressed( "reload" ) )
        {
            ResetView();
        }

        bool canMove = true;

        if ( Input.Pressed( "attack1" ) )
        {
            TakePhoto();
        }

        if ( Input.Down( "attack2" ) )
        {
            FieldOfView += Input.MouseDelta.y * 0.2f;
            FieldOfView = FieldOfView.Clamp( 1f, 140f );
            RollOffset += Input.MouseDelta.x * 0.02f;
            RollOffset = RollOffset.Clamp( -180f, 180f );
            canMove = false;
        }

        var eyeAngles = Player.Head.Transform.Rotation.Angles();
        var sens = Preferences.Sensitivity * FieldOfView / 100f;
        if ( canMove )
        {
            eyeAngles.pitch += Input.MouseDelta.y * sens / 100f;
            eyeAngles.yaw -= Input.MouseDelta.x * sens / 100f;
        }
        eyeAngles.roll = 0f;
        eyeAngles.pitch = eyeAngles.pitch.Clamp( -89.9f, 89.9f );
        Player.Head.Transform.Rotation = eyeAngles;

        Scene.Camera.Transform.Position = Player.Head.Transform.Position;
        Scene.Camera.Transform.Rotation = eyeAngles + new Angles( 0, 0, RollOffset );
        Scene.Camera.FieldOfView = FieldOfView;
        Player.Direction = eyeAngles;
    }

    void ResetView()
    {
        RollOffset = 0f;
        FieldOfView = 100f;
    }

    void TakePhoto()
    {
        // if ( !FileSystem.Data.DirectoryExists( "photos" ) )
        // {
        //     FileSystem.Data.CreateDirectory( "photos" );
        // }

        // string datetime = DateTime.Now.ToString( "yyyy-MM-dd_HH-mm-ss" );
        // int ind = 0;
        // while ( FileSystem.Data.FileExists( $"photos/{datetime}.png" ) )
        // {
        //     ind++;
        //     datetime = DateTime.Now.ToString( $"yyyy-MM-dd_HH-mm-ss" ) + $" ({ind})";
        // }

        // Chatbox.Instance.AddEntry( "📸", "Photo saved as " + datetime + ".png", "notification" );

        // var texture = Texture.CreateRenderTarget().WithSize( (int)Screen.Width, (int)Screen.Height ).Create();
        // Scene.Camera.RenderToTexture( texture );

        // Color32[] colors = texture.GetPixels();

        // var stream = FileSystem.Data.OpenWrite( $"photos/{datetime}.png" );


        BroadcastPhoto();
    }

    [Broadcast]
    void BroadcastPhoto()
    {
        var sound = Sound.Play( "camera.picture", Transform.Position );
        if ( Network.IsOwner )
        {
            sound.Volume = 0.1f;
            sound.ListenLocal = true;
        }
    }
}