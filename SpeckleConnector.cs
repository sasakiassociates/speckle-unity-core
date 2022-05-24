using System;
using System.Collections;
using Speckle.Core.Kits;
using UnityEngine;

namespace Speckle.ConnectorUnity
{
	public static partial class SpeckleConnector
	{
		public const string HostApp = HostApplications.Unity.Name;

		public const string NameSpace = "Speckle";

		internal static bool Valid(this IList list, int count) => list != null && count >= 0 && count < list.Count;

		internal static bool Valid(this ICollection list) => list.Valid(0);

		internal static bool Valid(this ICollection list, int count) => list != null && count >= 0 && count < list.Count;

		internal static bool Valid(this string name) => !string.IsNullOrEmpty(name);

		public static class Console
		{
			public const string title = "speckle-connector:";

			public static void Log(string msg)
			{
				Debug.Log(title + msg);
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