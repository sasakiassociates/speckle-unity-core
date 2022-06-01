using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{

	/// <summary>
	/// A speckle node is pretty much the reference object that is first pulled from a commit
	/// </summary>
	[AddComponentMenu("Speckle/Node")]
	public class SpeckleNode : MonoBehaviour
	{

		[SerializeField, HideInInspector]
		private string id;

		/// <summary>
		/// Reference object id
		/// </summary>
		public string Id => id;

		[SerializeField, HideInInspector]
		private string appId;

		/// <summary>
		/// Reference to application ID
		/// </summary>
		public string AppId => appId;

		[SerializeField, HideInInspector]
		private int childCount;

		/// <summary>
		/// Total child count 
		/// </summary>
		public int ChildCount => childCount;

		[SerializeField] private SpeckleStructure hierarchy;

		/// <summary>
		/// Setup the hierarchy for the commit coming in
		/// </summary>
		/// <param name="data">The object to convert</param>
		/// <param name="converter">Speckle Converter to parse objects with</param>
		/// <param name="token">Cancellation token</param>
		/// <returns></returns>
		public UniTask DataToScene(Base data, ISpeckleConverter converter, CancellationToken token)
		{
			id = data.id;
			appId = data.applicationId;
			childCount = (int)data.totalChildrenCount;
			name = $"Node: {id}";

			hierarchy = new SpeckleStructure();
			var defaultLayer = new GameObject("Default").AddComponent<SpeckleLayer>();

			if (converter == null)
			{
				SpeckleUnity.Console.Warn("No valid converter to use during conversion ");
				return UniTask.CompletedTask;
			}

			// if the commit contains no lists or trees
			if (converter.CanConvertToNative(data))
				defaultLayer.ConvertToLayer(data, converter);
			else
			{
				// check if there are layers in the ref object
				foreach (var member in data.GetMemberNames())
				{
					var obj = data[member];

					if (!obj.IsList())
					{
						// if the object is a regular speckle object it gets added to the general layer 
						defaultLayer.ConvertToLayer(obj, converter);
						continue;
					}

					// this object is a list with objects inside it 
					var layer = SpeckleUnity.ListToLayer(member, ((IEnumerable)obj).Cast<object>(), converter, token, transform, Debug.Log);

					hierarchy.Add(layer);
				}
			}

			if (defaultLayer.Layers.Any())
			{
				defaultLayer.SetObjectParent(transform);
				hierarchy.Add(defaultLayer);
			}
			else
				SpeckleUnity.SafeDestroy(defaultLayer.gameObject);

			return UniTask.CompletedTask;
		}

		public Base SceneToData(ISpeckleConverter converter, CancellationToken token)
		{
			var data = new Base();

			foreach (var layer in hierarchy.layers)
			{
				if (token.IsCancellationRequested)
					return data;

				data[layer.LayerName] = LayerToBase(layer, converter, token);
			}

			return data;
		}

		private static Base LayerToBase(SpeckleLayer layer, ISpeckleConverter converter, CancellationToken token)
		{
			var layerBase = new Base();
			try
			{
				var layerObjects = new List<Base>();

				foreach (var item in layer.Data)
				{
					if (token.IsCancellationRequested)
						return layerBase;

					var @base = ConvertRecursively(item, converter, token);

					if (@base != null)
						layerObjects.Add(@base);
				}

				layerBase["@Objects"] = layerObjects;
			}

			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Warn(e.Message);
				return layerBase;
			}

			try
			{
				foreach (var nestedLayer in layer.Layers)
				{
					if (token.IsCancellationRequested)
						return layerBase;

					layerBase[nestedLayer.LayerName] = LayerToBase(nestedLayer, converter, token);
				}
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Warn(e.Message);
				return layerBase;
			}

			return layerBase;
		}

		private static Base ConvertRecursively(GameObject item, ISpeckleConverter converter, CancellationToken token)
		{
			var @base = new Base();

			if (token.IsCancellationRequested || item == null)
				return @base;

			if (converter.CanConvertToSpeckle(item))
				@base = converter.ConvertToSpeckle(item);

			if (CheckForChildren(item, converter, token, out var objs))
				@base["@Objects"] = objs;

			return @base;
		}

		private static bool CheckForChildren(GameObject go, ISpeckleConverter converter, CancellationToken token, out List<Base> objs)
		{
			objs = new List<Base>();

			if (go != null && go.transform.childCount > 0)
			{
				foreach (Transform child in go.transform)
				{
					if (token.IsCancellationRequested)
						return false;

					var converted = ConvertRecursively(child.gameObject, converter, token);
					if (converted != null)
						objs.Add(converted);
				}
			}

			return objs.Any();
		}
	}
}