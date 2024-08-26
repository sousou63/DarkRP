using System;

namespace Utils;

public static class TraceUtils
{
	/// <summary>
	/// Performs a forward line trace from the camera and returns the hit position.
	/// </summary>
	/// <returns>The position of the hit point, or Vector3.Zero if no hit is detected.</returns>
	public static Vector3 ForwardLineTrace(Scene scene, GameTransform origin, int range)
	{
		try
		{

			// Starting position of the line (camera position)
			var start = origin.Position;

			// Direction of the line (the direction the camera is facing)
			var direction = origin.World.Forward;

			// Calculate the end position based on direction and interact range
			var end = start + direction * range;

			// Perform a line trace (raycast) to detect objects in the line of sight ( raycast ignore the player )
			var hitResult = scene.Trace.Ray( start, end ).Run();

			// Check if the trace hit is valid
			return hitResult is { GameObject: not null, Hit: true } ?
				// Return the hit position
				hitResult.EndPosition :
				// Return a default value if no hit is detected
				Vector3.Zero;
		}
		catch (Exception e)
		{
			// Log any errors that occur during the line trace
			Log.Error(e);
			return Vector3.Zero;
		}
	}

}
