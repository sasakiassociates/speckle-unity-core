using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Mono;
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
		[SerializeField] private string appId;
		[SerializeField] private int childCount;

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

		public UniTask Set(Base @base)
		{
			id = @base.id;
			appId = @base.applicationId;
			childCount = (int)@base.totalChildrenCount;

			return UniTask.CompletedTask;

			// TODO: Setting this props is too much and doesn't really get used properly yet. 
			// _properties = new SpeckleProperties();
			// _properties.Store(@base, excludedProps);
		}

	}
}