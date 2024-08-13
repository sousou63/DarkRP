using System;

/// <summary>
/// Specifies that a method that will not be called if the local connection is not
/// an admin or there is an RPC caller and they are not an admin.
/// </summary>
[AttributeUsage( AttributeTargets.Method )]
[CodeGenerator( CodeGeneratorFlags.WrapMethod | CodeGeneratorFlags.Static, "AdminAttribute.OnAdminInvoked" )]
public class AdminAttribute : Attribute 
{
	internal static void OnAdminInvoked( WrappedMethod m, params object[] args )
	{
		var isAdmin = Connection.Local.IsAdmin();
		var isRpcCallerUnauthorized = Rpc.Caller is not null && !Rpc.Caller.IsAdmin();
		if ( !isAdmin || isRpcCallerUnauthorized )
			return;

		m.Resume();
	}
}
