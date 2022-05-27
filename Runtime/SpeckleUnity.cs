using System;
using System.Collections;
using Speckle.Core.Kits;
using UnityEngine;

namespace Speckle.ConnectorUnity
{
	public static partial class SpeckleUnity
	{
		public const string HostApp = HostApplications.Unity.Name;

		public const string NameSpace = "Speckle";

		public static class Console
		{
			public const string title = "speckle-connector:";

			public static void Log(string msg)
			{
				Debug.Log(title + " " + msg);
			}
			public static void Exception(Exception exception)
			{
				Debug.LogException(exception);
			}
			public static void Warn(string message)
			{
				Debug.LogWarning(title + message);
			}
		}
	}
}