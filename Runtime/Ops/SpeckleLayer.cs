using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{
	[Serializable]
	public class SpeckleLayer
	{

		[SerializeField] private Transform parent;
		[SerializeField] private List<GameObject> data;
		[SerializeField] private string name;
		[SerializeField] private List<SpeckleLayer> layers;

		public List<GameObject> Data
		{
			get => data.Valid() ? data : new List<GameObject>();
		}

		public string LayerName => this.name;

		public SpeckleLayer()
		{
			data = new List<GameObject>();
		}

		public SpeckleLayer(string name)
		{
			this.name = name;
			data = new List<GameObject>();
		}

		public void Parent(Transform t)
		{
			if (t == null)
				return;
			

			parent = t;

			if (Data.Any())
				Data.ForEach(x => x.transform.SetParent(parent));
		}

		public void Add(SpeckleLayer layer)
		{
			layers ??= new List<SpeckleLayer>();
			layers.Add(layer);
		}

		public void Add(GameObject @object)
		{
			data ??= new List<GameObject>();
			data.Add(@object);
		}


	}
}