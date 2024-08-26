using System;

namespace GameSystems;

public class SavedDatabase // Don't touch it's waiting when the time will come (when garry releases servers)
{
	public bool IsLocal { get; set; }
	public string Host { get; set; }
	public UInt16 Port { get; set; }
	public string Login { get; set; }
	public string Password { get; set; }
	public SavedDatabase()
	{
	}

	public SavedDatabase( Database db )
	{
		this.IsLocal = db.IsLocal;
		this.Host = db.Host;
		this.Port = db.Port;
		this.Login = db.Login;
		this.Password = db.Password;
	}
}
