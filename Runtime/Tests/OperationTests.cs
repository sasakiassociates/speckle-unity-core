using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using UnityEngine;

namespace Speckle.ConnectorUnity
{
	[ExecuteAlways]
	public class OperationTests : MonoBehaviour
	{

		private struct TestStream
		{
			public string stream;
			public string commit;
		}

		private static TestStream RC_1 = new TestStream()
		{
			stream = "89ef27f57b",
			commit = "884906295a"
		};

		private static TestStream RC_2 = new TestStream()
		{
			stream = "89ef27f57b",
			commit = "162ee5f3f5"
		};

		private static TestStream Carlos = new TestStream()
		{
			stream = "00613d79b2",
			commit = "a852ab79fb"
		};

		public async UniTask Receive(bool asTask)
		{
			var token = this.GetCancellationTokenOnDestroy();

			// await UniTask.SwitchToTaskPool();

			var s = Carlos;

			var client = new Client(AccountManager.GetDefaultAccount());
			var transport = new ServerTransport(client.Account, s.stream);

			var commitTask = client.CommitGet(token,
			                                  s.stream,
			                                  s.commit
			);

			var commit = asTask ? await commitTask : await commitTask.AsUniTask();
			
			var recTask = Operations.Receive(
				commit.referencedObject,
				token,
				transport
			);

			var @base = asTask ? await recTask : await recTask.AsUniTask();
			
			transport.Dispose();
			client.Dispose();
		}

		public void OnGUI()
		{
			if (GUI.Button(new Rect(10f, 10f, 100f, 20f), "Test Task"))
				Receive(true).Forget();
			if (GUI.Button(new Rect(10f, 40f, 100f, 20f), "Test UniTask"))
				Receive(false).Forget();
		}

	}
}