using Sandbox.Entity;
using Sandbox.GameSystems.Player;
using Sandbox.Resources;

namespace Entity.Interactable.Printer
{
	public sealed class PrinterLogic : BaseEntity
	{
		[Property] public GameObject PrinterFan { get; set; }
		[Property] public float PrinterFanSpeed { get; set; } = 1000f;
		[Property, Sync] public float PrinterCurrentMoney { get; set; } = 0f;
		[Property] public float PrinterTimerMoney { get; set; } = 25f;
		[Property] public float PrinterMaxMoney { get; set; } = 8000f;

		private TimeSince _lastUsed = 0; // Set the timer

		private PrinterResource _currentPrinterType; // Store the current printer resource

		/// <summary>
		/// Interact with the printer. This comes from the IInteractable interface inherited from the Interactable class.
		/// </summary>
		public override void InteractUse( SceneTraceResult tr, GameObject player )
		{
			Log.Info( "Interacting with printer" );
			if ( PrinterCurrentMoney > 0 )
			{
				var playerStats = player.Components.Get<Player>();
				if ( playerStats == null ) { return; }

				playerStats.UpdateBalance( PrinterCurrentMoney );
				ResetPrinterMoney();
				Sound.Play( "audio/money.sound", Transform.World.Position );
			}
		}

		protected override void OnFixedUpdate()
		{
			// Determine the timer based on the printer type
			float printerTimer = GetPrinterTimer();
			// If the timer has passed, add money
			if ( _lastUsed >= printerTimer )
			{
				if ( PrinterCurrentMoney < PrinterMaxMoney )
				{
					PrinterCurrentMoney += PrinterTimerMoney; // Add money to the printer
				}
				_lastUsed = 0; // Reset the timer
			}
			SpinFan();
		}

		private void SpinFan()
		{
			// Calculate the rotation amount based on PrinterFanSpeed and Time.Delta
			var rotationAmount = PrinterFanSpeed * Time.Delta;

			// Apply the rotation relative to the GameObject's current rotation
			PrinterFan.Transform.Rotation *= Rotation.FromAxis( Vector3.Left, -rotationAmount );
		}

		// Method to set the current printer type and update its color
		public void SetPrinterType( PrinterResource resource )
		{
			_currentPrinterType = resource;
			// Automatically update the color when the printer type is set
			UpdatePrinterColor();
		}

		[Broadcast]
		public void ResetPrinterMoney()
		{
			PrinterCurrentMoney = 0f;
		}

		// Method to get the correct timer based on the printer type
		private float GetPrinterTimer()
		{
			return _currentPrinterType.Timer;
		}

		// Method to update the printer color based on the printer type
		private void UpdatePrinterColor()
		{
			Color newColor = _currentPrinterType.ModelColor;

			// Assuming there's a component responsible for rendering the model
			var ModelRenderer = GameObject.Components.Get<ModelRenderer>();
			if ( ModelRenderer is null )
			{
				Log.Warning( "ModelRenderer component not found" );
				return;
			}

			ModelRenderer.Tint = newColor;
			// PrinterFan.Components.Get<ModelRenderer>().Tint = newColor;
			ModelRenderer.MaterialOverride = _currentPrinterType.Material;
		}
	}
}
