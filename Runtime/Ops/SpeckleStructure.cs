using System;
using System.Collections.Generic;

namespace Speckle.ConnectorUnity.Ops
{
	[Serializable]
	public class SpeckleStructure
	{
		public SpeckleStructure()
		{
			layers = new List<SpeckleLayer>();
			global = new SpeckleLayer();
		}

		public List<SpeckleLayer> layers;

		public SpeckleLayer global;
		
		public void Add(SpeckleLayer layer)
		{
			layers ??= new List<SpeckleLayer>();
			layers.Add(layer);
		}
	}
}