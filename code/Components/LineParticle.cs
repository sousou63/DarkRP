using System.Collections.Immutable;

namespace Scenebox;

public sealed class LineParticle : Component
{
	[RequireComponent] LineRenderer LineRenderer { get; set; }

	float Duration = 1f;
	TimeSince timeSinceStart;
	Curve WidthCurve;

	public void Init( Vector3 start, Vector3 end, float duration = 0.5f )
	{
		LineRenderer.Points[0].Transform.Position = start;
		LineRenderer.Points[1].Transform.Position = (start + end) / 2;
		LineRenderer.Points[2].Transform.Position = end;
		WidthCurve = LineRenderer.Width;
		Duration = duration;
		timeSinceStart = 0;
	}

	protected override void OnUpdate()
	{
		List<Curve.Frame> frames = new();
		float val = 1f - (timeSinceStart / Duration);
		foreach ( var frame in WidthCurve.Frames )
		{
			frames.Add( new Curve.Frame( frame.Time, frame.Value * val ) );
		}
		LineRenderer.Width = LineRenderer.Width.WithFrames( frames.ToImmutableList() );
		if ( timeSinceStart > Duration )
		{
			GameObject.Destroy();
		}
	}
}
