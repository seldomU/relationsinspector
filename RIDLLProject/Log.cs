using UnityEngine;

namespace RelationsInspector
{
	internal static class Log
	{
		enum LogType { Message, Warning, Error }    // exceptions too?

		public static void Message( string message )
		{
			LogItem( LogType.Message, message );
		}

		public static void Warning( string warning )
		{
			LogItem( LogType.Warning, warning );
		}

		public static void Error( string error )
		{
			LogItem( LogType.Error, error );
		}

		static void LogItem( LogType type, string item )
		{
			if ( !Settings.Instance.logToConsole )
				return;

			string prefix = "Relations inspector: ";
			switch ( type )
			{
				case LogType.Message:
					Debug.Log( prefix + item );
					break;

				case LogType.Warning:
					Debug.LogWarning( prefix + item );
					break;

				case LogType.Error:
					Debug.LogError( prefix + item );
					break;
			}
		}
	}
}
