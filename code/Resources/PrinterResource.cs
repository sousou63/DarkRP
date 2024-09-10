using Sandbox.GameResources;

namespace Sandbox.Resources;

[GameResource("DarkRp/Printer", "printer", "A basic printer definition")]
public class PrinterResource : GameResource
{
	[Category("Display")] public string Name { get; set; }
	[Category("Display")] public string Description { get; set; }
	[Category("Display")] public Color DisplayColor { get; set; }
	[Category("Display")] public float Price { get; set; }
	[Category("Display")] public int Order { get; set; }
	
	[Category("Appearance")] public Color ModelColor { get; set; }
	[Category("Appearance")] public Material Material { get; set; }
	
	/// <summary>
	/// The timer for the printer to generate money in seconds
	/// </summary>
	[Category("Logic")] public float Timer { get; set; }
}
