using Sandbox.Resources;

namespace Sandbox;

public static class PrinterProvider
{
	public static HashSet<PrinterResource> PrinterTypes = new();
	
	static PrinterProvider()
	{
		Log.Info("Loading printers...");
		var printers = ResourceLibrary.GetAll<PrinterResource>( "data/printers" );
		foreach ( var printerResource in ResourceLibrary.GetAll<PrinterResource>("data/printers") )
		{
			Log.Info($"Loaded printer: {printerResource.Name}");
			PrinterTypes.Add( printerResource );
		}
	}

	public static IEnumerable<PrinterResource> GetOrderedPrinterTypes()
	{
		return PrinterTypes.OrderBy( p => p.Order );
	}
}
