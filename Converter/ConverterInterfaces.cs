using System.Collections.Generic;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	public interface IWantContextObj
	{
		public List<ApplicationPlaceholderObject> contextObjects { set; }
	}

	public interface IComponentConverter
	{

		public string speckle_type { get; }
		public string unity_type { get; }
		public bool CanConvertToNative(Base type);
		public bool CanConvertToSpeckle(Component type);

		public GameObject ToNative(Base @base);
		public Base ToSpeckle(Component component);
	}
}