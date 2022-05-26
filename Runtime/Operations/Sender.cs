using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity
{

	/// <summary>
	///   A Speckle Sender, it's a wrapper around a basic Speckle Client
	///   that handles conversions for you
	/// </summary>
	[AddComponentMenu("Speckle/Sender")]
	[ExecuteAlways]
	public class Sender : SpeckleClient
	{
		[SerializeField] private List<GameObject> objectsToSend;

		public UnityAction<string> onDataSent;

		private ServerTransport transport;

		public async UniTask<string> Send(List<GameObject> objs = null, string message = null, CancellationTokenSource cancellationToken = null)
		{
			var objectId = "";

			if (!IsReady())
				return objectId;

			// TODO: This feels pretty silly. It should be something similar to the way selections are made in rhino or revit.
			if (objs == null || !objs.Any())
			{
				SpeckleUnity.Console.Log("No objects were passed to the sender - checking others");

				if (objectsToSend.Valid())
				{
					SpeckleUnity.Console.Log("Using objects in editor list");
					objs = objectsToSend;
				}
				else if (_root != null)
				{
					SpeckleUnity.Console.Log($"Using the root object with {_root.transform.childCount} kids");
					objs = new List<GameObject>() { _root };
				}
				else
				{
					SpeckleUnity.Console.Warn("No objects were found to send! Stopping call");
					return objectId;
				}
			}

			var data = objs.Count > 1 ? ConvertRecursively(objs) : ConvertRecursively(objs[0]);

			try
			{
				SpeckleUnity.Console.Log("Sending data");

				transport = new ServerTransport(client.Account, stream.Id);

				objectId = await Operations.Send(
					data,
					this.GetCancellationTokenOnDestroy(),
					new List<ITransport>() { transport },
					useDefaultCache: true,
					onProgressAction: onProgressReport,
					onErrorAction: onErrorReport,
					disposeTransports: false
				).AsUniTask();

				Debug.Log($"data sent! {objectId}");

				Debug.Log($"Commit to {stream.BranchName}");

				var commit = await client.CommitCreate(
					this.GetCancellationTokenOnDestroy(),
					new CommitCreateInput()
					{
						objectId = objectId,
						streamId = stream.Id,
						branchName = stream.BranchName, // TODO: fix how the speckle stream object holds data... 
						message = message.Valid() ? message : $"Objects from Unity {data.totalChildrenCount}",
						sourceApplication = SpeckleUnity.HostApp,
						totalChildrenCount = (int)data.GetTotalChildrenCount()
					}).AsUniTask();

				Debug.Log($"commit created! {commit}");

				transport?.Dispose();
				onDataSent?.Invoke(objectId);

				await UniTask.Yield();
			}

			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Exception(e);
			}

			return objectId;
		}

		protected override void CleanUp()
		{
			base.CleanUp();
			transport?.Dispose();
		}

		#region private methods
		private Base ConvertRecursively(IEnumerable<GameObject> objs)
		{
			return new Base()
			{
				["objects"] = objs.Select(ConvertRecursively).Where(x => x != null).ToList()
			};
		}

		protected override async UniTask LoadStream()
		{
			await base.LoadStream();
			name = nameof(Sender) + $"-{stream.Id}";
		}

		private Base ConvertRecursively(GameObject go)
		{
			if (converter.CanConvertToSpeckle(go))
				try
				{
					return converter.ConvertToSpeckle(go);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}

			return CheckForChildren(go, out var objs) ?
				new Base { ["objects"] = objs } : null;
		}

		private bool CheckForChildren(GameObject go, out List<Base> objs)
		{
			objs = new List<Base>();

			if (go != null && go.transform.childCount > 0)
			{
				foreach (Transform child in go.transform)
				{
					var converted = ConvertRecursively(child.gameObject);
					if (converted != null)
						objs.Add(converted);
				}
			}

			return objs.Any();
		}
		#endregion

	}
}