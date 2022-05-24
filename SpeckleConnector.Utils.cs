using System.Collections;

namespace Speckle.ConnectorUnity
{
	public static partial class SpeckleConnector
	{
		internal static bool Valid(this IList list) => list.Valid(0);
		internal static int Check(this IList list, int index) => list.Valid(index) ? index : 0;
	}
}