using System;

namespace GameSystems;

public class Database // Don't touch it's waiting when the time will come (when garry releases servers)
{
	public bool IsLocal { get; set; }
	public string Host { get; set; }
	public UInt16 Port { get; set; }
	public string Login { get; set; }
	public string Password { get; set; }
	public Database( bool il, string h, UInt16 po, string l, string pa )
	{
		this.IsLocal = il;
		this.Host = h;
		this.Port = po;
		this.Login = l;
		this.Password = pa;
	}
	public Database()
	{
		if ( FileSystem.Data.FileExists("saveddatabase.json"))
		{
			SavedDatabase loaded = FileSystem.Data.ReadJson<SavedDatabase>( "saveddatabase.json" );
			this.IsLocal = loaded.IsLocal;	
			this.Host = loaded.Host;
			this.Port = loaded.Port;
			this.Login = loaded.Login;
			this.Password = loaded.Password;
			Log.Info( "Database Host: " + this.Host );
		}
		else
		{
			this.IsLocal = true;
			this.Host = "localhost";
			this.Port = 80;
			this.Login = "login";
			this.Password = "password";
			FileSystem.Data.WriteJson( "saveddatabase.json", new SavedDatabase( this ) );
		}
	}
}
