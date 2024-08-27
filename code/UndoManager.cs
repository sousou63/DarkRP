using System;
using Sandbox.Audio;
using Scenebox.UI;

namespace Scenebox;

public sealed class UndoManager : Component
{
    public static UndoManager Instance { get; private set; }
    public record Entry( string message, List<Guid> ids, Action undo );

    List<Entry> Stack = new();

    protected override void OnAwake()
    {
        Instance = this;
    }

    protected override void OnUpdate()
    {
        if ( Input.Pressed( "Undo" ) )
        {
            Undo();
        }
    }

    protected override void OnFixedUpdate()
    {
        for ( int i = Stack.Count - 1; i >= 0; i-- )
        {
            var entry = Stack[i];
            foreach ( var id in entry.ids )
            {
                if ( !Scene.Directory.FindByGuid( id ).IsValid() )
                {
                    Stack.RemoveAt( i );
                    break; // Skip to the next entry
                }
            }
        }
    }

    public void Add( string message, Guid id, Action undo )
    {
        Add( message, new List<Guid>() { id }, undo );
    }

    public void Add( string message, List<Guid> ids, Action undo )
    {
        Stack.Add( new Entry( message, ids, undo ) );
    }

    public void AddGameObject( Guid id, string message = "Undone Prop" )
    {
        Add( message, new List<Guid>() { id }, () => GameManager.Instance?.BroadcastDestroyObject( id ) );
    }

    public void Undo()
    {
        if ( Stack.Count == 0 ) return;

        Sound.Play( "ui.undo" ).TargetMixer = Mixer.FindMixerByName( "UI" );

        var entry = Stack[Stack.Count - 1];
        NotificationPanel.Instance?.AddEntry( "undo", entry.message, 3f, false );
        entry.undo?.Invoke();
        Stack.RemoveAt( Stack.Count - 1 );
    }
}