using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Mono;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{

	/// <summary>
	/// A speckle node is pretty much the reference object that is first pulled from a commit
	/// </summary>
	public class SpeckleNode : BaseBehaviour
	{

		[SerializeField] private string id;

		/// <summary>
		/// Reference object id
		/// </summary>
		public string Id => id;

		[SerializeField] private string appId;

		/// <summary>
		/// Reference to application ID
		/// </summary>
		public string AppId => appId;

		[SerializeField] private int childCount;

		/// <summary>
		/// Total child count 
		/// </summary>
		public int ChildCount => childCount;

		[SerializeField] private SpeckleStructure hierarchy;

		public override void SetProps(Base @base, HashSet<string> props = null)
		{
			id = @base.id;
			appId = @base.applicationId;
			childCount = (int)@base.totalChildrenCount;

			_properties = new SpeckleProperties();
			_properties.Store(@base, props ?? excludedProps);
		}

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

			if (converter == null)
			{
				SpeckleUnity.Console.Warn("No valid converter to use during conversion ");
				return UniTask.CompletedTask;
			}

			// if the commit contains no lists or trees
			if (converter.CanConvertToNative(data))
				ConvertToLayer(hierarchy.global, data, converter);
			else
			{
				// check if there are layers in the ref object
				foreach (var member in data.GetMemberNames())
				{
					var obj = data[member];

					if (!obj.IsList())
					{
						// if the object is a regular speckle object it gets added to the general layer 
						ConvertToLayer(hierarchy.global, obj, converter);
						continue;
					}

					// this object is a list with objects inside it 
					var layer = SpeckleUnity.ListToLayer(member, ((IEnumerable)obj).Cast<object>(), converter, token, transform, Debug.Log);

					hierarchy.Add(layer);
				}
			}

			return UniTask.CompletedTask;
		}

		private static void ConvertToLayer(SpeckleLayer layer, Base obj, ISpeckleConverter converter)
		{
			if (converter.ConvertToNative(obj) is GameObject o)
				layer.Add(o);
			else
				Debug.Log("Did not convert correctly");
		}

		private static void ConvertToLayer(SpeckleLayer layer, object obj, ISpeckleConverter converter)
		{
			if (obj.IsBase(out var @base))
				ConvertToLayer(layer, @base, converter);
		}

		public void SceneToData()
		{ }

	}
}