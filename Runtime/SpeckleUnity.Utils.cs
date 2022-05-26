using System.Collections;
using UnityEngine;

namespace Speckle.ConnectorUnity
{
	public static partial class SpeckleUnity
	{
		public static int Check(this IList list, int index) => list.Valid(index) ? index : 0;
		
		public static bool Valid(this IList list) => list.Valid(0);

		public static bool Valid(this IList list, int count) => list != null && count >= 0 && count < list.Count;

		public static bool Valid(this ICollection list) => list.Valid(0);

		public static bool Valid(this ICollection list, int count) => list != null && count >= 0 && count < list.Count;

		public static bool Valid(this string name) => !string.IsNullOrEmpty(name);

		
		public static void SafeDestroy(Object obj)
		{
			if (Application.isPlaying)
				Object.Destroy(obj);

			else
				Object.DestroyImmediate(obj);

		}
	}
}