using System;
using System.Collections.Generic;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{
	[Serializable]
	public class SpeckleStructure
	{
		public SpeckleStructure()
		{
			layers = new List<SpeckleLayer>();
		}

		public List<SpeckleLayer> layers;
		
		public void Add(SpeckleLayer layer)
		{
			layers ??= new List<SpeckleLayer>();
			layers.Add(layer);
		}
	}
}