using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Logging;
using Speckle.Core.Transports;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{

	/// <summary>
	///   A Speckle Sender, it's a wrapper around a basic Speckle Client
	///   that handles conversions for you
	/// </summary>
	[AddComponentMenu("Speckle/Sender")]
	[ExecuteAlways]
	public class Sender : SpeckleClient
	{

		[SerializeField] private string commitMessage;

		public UnityAction<string> onDataSent;

		private ServerTransport transport;

		public async UniTask<string> Send(SpeckleNode obj, string message = null, CancellationTokenSource tokenSource = null)
		{
			if (obj != null)
				_root = obj;

			return await Send(message, tokenSource);
		}

		public async UniTask<string> Send(string message = null, CancellationTokenSource tokenSource = null)
		{
			var objectId = "";

			if (!IsReady())
			{
				SpeckleUnity.Console.Warn($"{this.name} is not ready");
				return objectId;
			}

			if (_root == null)
			{
				SpeckleUnity.Console.Warn("No objects were found to send! Stopping call");
				return objectId;
			}

			token = tokenSource?.Token ?? this.GetCancellationTokenOnDestroy();

			var data = _root.SceneToData(converter, token);

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

				Debug.Log($"Commit to {branch.name}");

				var commitId = await client.CommitCreate(
					this.GetCancellationTokenOnDestroy(),
					new CommitCreateInput()
					{
						objectId = objectId,
						streamId = stream.Id,
						branchName = branch.name,
						message = message.Valid() ? message : commitMessage.Valid() ? commitMessage : $"Objects from Unity {data.totalChildrenCount}",
						sourceApplication = SpeckleUnity.HostApp,
						totalChildrenCount = (int)data.GetTotalChildrenCount()
					}).AsUniTask();

				//TODO: point to new commit 
				Debug.Log($"commit created! {commitId}");

				transport?.Dispose();
				onDataSent?.Invoke(objectId);

				await UniTask.Yield();
			}

			catch
				(SpeckleException e)
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

		protected override async UniTask LoadStream()
		{
			await base.LoadStream();
			
			name = nameof(Sender) + $"-{stream.Id}";
		}

	}
}