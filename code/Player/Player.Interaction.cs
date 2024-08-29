using System;
using Entity.Interactable;

namespace Sandbox.GameSystems.Player
{
	public partial class Player
	{
		[Property, Group( "Interaction" )] public float InteractRange { get; set; } = 120f;
		[Property, Group( "Interaction" )] public bool DrawDebugInteract { get; set; } = false;
		[Property, Group( "Interaction" )] public string InteractTag { get; set; } = "Interact";

		private SceneTraceResult _interactionTraceResult;
		private GameObject _currentOutlinedObject;

		protected void OnFixedUpdateInteraction()
		{
			if ( Network.IsProxy ) return;

			Interact();

			if ( DrawDebugInteract )
			{
				DrawDebug();
			}
		}

		void Interact()
		{
			var start = _camera.Transform.Position;
			var direction = _camera.Transform.World.Forward;
			var end = start + direction * InteractRange;

			_interactionTraceResult = Scene.Trace.IgnoreGameObject( GameObject ).Ray( start, end ).Run();

			var hitObject = _interactionTraceResult.GameObject;
			if ( hitObject != null && hitObject.Tags.Has( InteractTag ) )
			{
				UpdateOutline( hitObject );

				if ( Input.Pressed( "Use" ) ) HandleInteraction( "Use" );
				if ( Input.Pressed( "Use Special" ) ) HandleInteraction( "Use Special" );
				if ( Input.Pressed( "attack1" ) ) HandleInteraction( "attack1" );
				if ( Input.Pressed( "attack2" ) ) HandleInteraction( "attack2" );
			}
			else
			{
				UpdateOutline( null ); // no object selected we reset outline
			}
		}

		void UpdateOutline( GameObject newObject )
		{
			if ( _currentOutlinedObject == newObject ) return;

			// reset the outline to full transparency
			if ( _currentOutlinedObject != null )
			{
				var previousOutline = _currentOutlinedObject.Components.Get<HighlightOutline>();
				if ( previousOutline != null )
				{
					previousOutline.Color = new Color( 1, 1, 1, 0 );
				}
			}

			// apply the outline to the current object
			if ( newObject != null )
			{
				var newOutline = newObject.Components.Get<HighlightOutline>();
				if ( newOutline != null )
				{
					newOutline.Color = Color.White;
				}
			}

			_currentOutlinedObject = newObject;
		}

		void DrawDebug()
		{
			Gizmo.Draw.LineSphere( _interactionTraceResult.EndPosition, 3, 8 );
			Log.Info( $"Hit: {_interactionTraceResult.GameObject} at {_interactionTraceResult.EndPosition}" );
		}

		private void HandleInteraction( string inputKey )
		{
			try
			{
				var interactable = _interactionTraceResult.GameObject?.Components.Get<IInteractable>();
				if ( interactable == null ) return;

				switch ( inputKey )
				{
					case "Use":
						interactable.InteractUse( _interactionTraceResult, GameObject );
						break;
					case "Use Special":
						interactable.InteractSpecial( _interactionTraceResult, GameObject );
						break;
					case "attack1":
						interactable.InteractAttack1( _interactionTraceResult, GameObject );
						break;
					case "attack2":
						interactable.InteractAttack2( _interactionTraceResult, GameObject );
						break;
				}
			}
			catch ( Exception e )
			{
				Log.Error( e );
			}
		}
	}
}

