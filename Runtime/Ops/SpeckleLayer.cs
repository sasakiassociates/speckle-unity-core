using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{
	[Serializable]
	public class SpeckleLayer
	{

		[SerializeField] private string name;
		[SerializeField] private Transform parent;
		[SerializeField] private List<GameObject> data;
		[SerializeField] private List<SpeckleLayer> layers;

		/// <summary>
		/// Active parent for all layer objects
		/// </summary>
		public Transform Parent => parent;

		/// <summary>
		/// Converted object data within layer
		/// </summary>
		public List<GameObject> Data => data.Valid() ? data : new List<GameObject>();

		/// <summary>
		/// Layer Name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Nested Layers
		/// </summary>
		public List<SpeckleLayer> Layers => layers.Valid() ? layers : new List<SpeckleLayer>();

		public SpeckleLayer()
		{
			data = new List<GameObject>();
		}

		public SpeckleLayer(string name)
		{
			this.name = name;
			data = new List<GameObject>();
		}

		/// <summary>
		/// Set parent for all objects in a layer
		/// </summary>
		/// <param name="t"></param>
		public void SetParent(Transform t)
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