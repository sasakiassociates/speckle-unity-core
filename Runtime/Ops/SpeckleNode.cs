using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Mono;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{

	/// <summary>
	/// not really sure what this will be, but I think it would be helpful to have an object that mimics the container objects of speckle. 
	/// </summary>
	[Serializable]
	internal class SpeckleLayer
	{

		[SerializeField] private string name;
		[SerializeField] private Transform parent;
		[SerializeField] private List<GameObject> data;

		public SpeckleLayer()
		{ }

		public SpeckleLayer(string name)
		{
			this.name = name;
			data = new List<GameObject>();
		}

		public void Parent(Transform t)
		{
			parent = t;
		}

		public void Add(object @object)
		{ }

	}

	[Serializable]
	internal class SpeckleStructure
	{
		public SpeckleStructure()
		{
			layers = new List<SpeckleLayer>();
		}
		
		public List<SpeckleLayer> layers;

	}

	/// <summary>
	/// A speckle node is pretty much the reference object that is first pulled from a commit
	/// </summary>
	public class SpeckleNode : BaseBehaviour
	{

		[SerializeField] private string id;
		[SerializeField] private string appId;
		[SerializeField] private int childCount;

		[SerializeField] private SpeckleStructure data;

		/// <summary>
		/// Reference object id
		/// </summary>
		public string Id => id;
		public string AppId => appId;

		public int ChildCount => childCount;

		public override void SetProps(Base @base, HashSet<string> props = null)
		{
			id = @base.id;
			appId = @base.applicationId;
			childCount = (int)@base.totalChildrenCount;

			_properties = new SpeckleProperties();
			_properties.Store(@base, props ?? excludedProps);
		}

		public void CreateHierarchy(Base @base)
		{
			foreach (var n in @base.GetDynamicMemberNames())
				Debug.Log($"dynamic:{n}");

			// TODO: check if there are layers or if this is just a single object

			data = new SpeckleStructure();

			// check if there are layers in the ref object
			foreach (var member in @base.GetDynamicMembers())
			{
				Debug.Log($"dynamic member:{member}");
				var layer = new SpeckleLayer(member);
				layer.Add(@base[member]);
				data.layers.Add(layer);
			}

			foreach (var n in @base.GetInstanceMembersNames())
				Debug.Log($"instance:{n}");

			foreach (var n in @base.GetInstanceMembers())
				Debug.Log($"instance member:{n}");
		}

		public UniTask Set(Base @base)
		{
			id = @base.id;
			appId = @base.applicationId;
			childCount = (int)@base.totalChildrenCount;

			CreateHierarchy(@base);

			// this object will have all the objects within it's properties space
			// let's take that structure and rebuild it 

			return UniTask.CompletedTask;

			// TODO: Setting this props is too much and doesn't really get used properly yet. 
			// _properties = new SpeckleProperties();
			// _properties.Store(@base, excludedProps);
		}

	}
}