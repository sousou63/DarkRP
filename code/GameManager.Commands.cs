using System;

namespace Scenebox;

public partial class GameManager
{
    [ConCmd( "kill" )]
    public static void KillLocalPlayer()
    {
        Player.Local?.Kill();
    }
}