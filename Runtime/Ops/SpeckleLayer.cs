using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{
	public class SpeckleLayer : MonoBehaviour
	{

		[SerializeField] private Transform _parent;
		[SerializeField] private List<GameObject> _data;
		[SerializeField] private List<SpeckleLayer> _layers;

		/// <summary>
		/// Active parent for all layer objects
		/// </summary>
		public Transform Parent => _parent;

		/// <summary>
		/// Converted object data within layer
		/// </summary>
		public List<GameObject> Data => _data.Valid() ? _data : new List<GameObject>();

		/// <summary>
		/// Layer Name
		/// </summary>
		public string LayerName
		{
			get => this.name;
			set => this.name = value;
		}

		/// <summary>
		/// Nested Layers
		/// </summary>
		public List<SpeckleLayer> Layers => _layers.Valid() ? _layers : new List<SpeckleLayer>();
		

		/// <summary>
		/// Set parent for all objects in a layer
		/// </summary>
		/// <param name="t"></param>
		public void SetObjectParent(Transform t)
		{
			if (t == null)
				return;

			_parent = t;

			if (Data.Any())
				Data.ForEach(x => x.transform.SetParent(_parent));
		}

		public void Add(SpeckleLayer layer)
		{
			_layers ??= new List<SpeckleLayer>();
			_layers.Add(layer);
		}

		public void Add(GameObject @object)
		{
			_data ??= new List<GameObject>();
			_data.Add(@object);
		}

		
		
	}
}