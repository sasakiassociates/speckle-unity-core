﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sentry;
using Speckle.Core.Api;
using Speckle.Core.Api.SubscriptionModels;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace Speckle.ConnectorUnity.Ops
{

	/// <summary>
	///   A Speckle Receiver, it's a wrapper around a basic Speckle Client
	///   that handles conversions and subscriptions for you
	/// </summary>
	[ExecuteAlways]
	[AddComponentMenu("Speckle/Receiver")]
	public class Receiver : SpeckleClient
	{
		[SerializeField] private bool autoReceive;
		[SerializeField] private bool deleteOld = true;
		[SerializeField] private Texture preview;
		[SerializeField] private int commitIndex;

		[SerializeField] private bool showPreview = true;
		[SerializeField] private bool renderPreview = true;

		public Action<GameObject> onDataReceivedAction;

		public Texture Preview
		{
			get => preview;
		}

		public List<Commit> Commits { get; protected set; }

		public string StreamUrl
		{
			get => stream == null || !stream.IsValid() ? "no stream" : stream.GetUrl(false);
		}

		public Commit activeCommit
		{
			get => Commits.Valid(commitIndex) ? Commits[commitIndex] : null;
		}

		public bool ShowPreview
		{
			get => showPreview;
			set => showPreview = value;
		}

		private void OnDestroy()
		{
			client?.CommitCreatedSubscription?.Dispose();
		}

		public event Action onPreviewSet;

		public override void SetBranch(int i)
		{
			base.SetBranch(i);
			Commits = branch != null ? branch.commits.items : new List<Commit>();
			SetCommit(0);
		}

		public void SetCommit(int i)
		{
			commitIndex = Commits.Check(i);

			if (activeCommit != null)
			{
				stream.Init($"{client.ServerUrl}/streams/{stream.Id}/commits/{activeCommit.id}");

				SpeckleUnity.Console.Log("Active commit loaded! " + activeCommit);

				UpdatePreview().Forget();
			}
		}

		private async UniTask UpdatePreview()
		{
			if (stream == null || !stream.IsValid())
				await UniTask.Yield();

			preview = await stream.GetPreview();

			onPreviewSet?.Invoke();

			await UniTask.Yield();
		}

		protected override void SetSubscriptions()
		{
			if (client != null && autoReceive)
			{
				client.SubscribeCommitCreated(stream.Id);
				client.OnCommitCreated += (sender, commit) => OnCommitCreated?.Invoke(commit);
				client.SubscribeCommitUpdated(stream.Id);
				client.OnCommitUpdated += (sender, commit) => OnCommitUpdated?.Invoke(commit);
			}
		}

		protected override async UniTask LoadStream()
		{
			await base.LoadStream();

			if (branches != null)
				Commits = branches.FirstOrDefault().commits.items;

			name = nameof(Receiver) + $"-{stream.Id}";
		}

		/// <summary>
		///   Gets and converts the data of the last commit on the Stream
		/// </summary>
		/// <returns></returns>
		public async UniTask Receive( bool sendReceive = false)
		{
			progress = 0f;
			isWorking = true;
			
			try
			{
				SpeckleUnity.Console.Log("Receive Started");

				// get the reference object from the commit
				Base @base = await this.GetCommitData();
				
				if (@base == null)
				{
					SpeckleUnity.Console.Warn("The data pulled from stream was not recieved correctly");
					await UniTask.Yield();
					return;
				}
				
				// TODO: check if this getting the commit updates the instance
				if (sendReceive)
					this.CommitReceived().Forget();

				// TODO: handle the process for update objects and not just force deleting
				if (deleteOld && _root != null)
					SpeckleUnity.SafeDestroy(_root.gameObject);

				_root = new GameObject().AddComponent<SpeckleNode>();
				
				// TODO: woof this is brutal! 

				await _root.Set(@base);
				
				if (converter == null)
				{
					SpeckleUnity.Console.Warn("No active converter found!");
					await UniTask.Yield();
					return;
				}
				
				SpeckleUnity.Console.Log("Converting Started");

				// TODO: This needs to work with checking only the data inside the node and not the whole commit
				// _root = ConvertRecursively(@base);

				Debug.Log("Conversion complete");

				onDataReceivedAction?.Invoke(_root.gameObject);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}
			finally
			{
				progress = 0f;
				isWorking = false;

				await UniTask.Yield();
			}
		}

		public void ConstructHierarchy( Base @base )
		{
			
		}

		// private async UniTask<ReadOnlyCollection<DisplayMesh>> BufferDisplayMesh(Base @base)
		// {
		// 	var buffer = new List<DisplayMesh>();
		//
		// 	// TODO: get display mesh data
		// 	// TODO: read through any properties for
		//
		// 	return new ReadOnlyCollection<DisplayMesh>(buffer);
		// }

		// TODO: separate and call hierarchy setup so it doesn't block data writing 
		// TODO: move data building to awaitable
		// TODO: bind object with data to format. 
		// TODO: move awaitable function 
		private GameObject ConvertRecursively(object value)
		{
			if (value == null)
			{
				SpeckleUnity.Console.Warn("No valid object for converting to untiy");
				return null;
			}
			


			//it's a simple type or not a Base
			if (value.GetType().IsSimpleType() || !(value is Base @base)) return null;

			return converter.CanConvertToNative(@base) ?
				TryConvertToNative(@base) : // supported object so convert that 
				TryConvertProperties(@base); // not supported but might have props
		}

		private GameObject RecurseTreeToNative(object @object, string containerName = null)
		{
			if (!IsList(@object))
				return ConvertRecursively(@object);

			var list = ((IEnumerable)@object).Cast<object>();

			var go = new GameObject(containerName.Valid() ? containerName : "List");

			var objects = list.Select(x => RecurseTreeToNative(x)).Where(x => x != null).ToList();

			if (objects.Any())
				objects.ForEach(x => x.transform.SetParent(go.transform));

			return go;
		}

		private GameObject TryConvertProperties(Base @base)
		{
			var go = new GameObject(@base.speckle_type);

			var props = new List<GameObject>();

			foreach (var prop in @base.GetMemberNames().ToList())
			{
				var goo = RecurseTreeToNative(@base[prop]);
				if (goo != null)
				{
					goo.name = prop;
					goo.transform.SetParent(go.transform);
					props.Add(goo);
				}
			}

			//if no children is valid, return null
			if (!props.Any())
			{
				SpeckleUnity.SafeDestroy(go);
				return null;
			}

			return go;
		}

		private GameObject TryConvertToNative(Base @base)
		{
			try
			{
				var go = converter.ConvertToNative(@base) as GameObject;

				if (go == null)
				{
					Debug.LogWarning("Object was not converted correclty");
					return null;
				}

				if (HasElements(@base, out var elements))
				{
					var goo = RecurseTreeToNative(elements, "Elements");

					if (goo != null)
						goo.transform.SetParent(go.transform);
				}
				return go;
			}
			catch (Exception e)
			{
				SpeckleUnity.Console.Exception(new SpeckleException(e.Message, e, true, SentryLevel.Error));
				return null;
			}
		}

		private static bool IsList(object @object)
		{
			if (@object == null)
				return false;

			var type = @object.GetType();
			return typeof(IEnumerable).IsAssignableFrom(type) && !typeof(IDictionary).IsAssignableFrom(type) && type != typeof(string);
		}

		private static bool HasElements(Base @base, out List<Base> items)
		{
			items = null;

			if (@base["elements"] is List<Base> l && l.Any())
				items = l;

			return items != null;
		}

		public void RenderPreview(bool render)
		{
			renderPreview = render;
			RenderPreview();
		}

		public void RenderPreview()
		{
			Debug.Log($"Render preview? {renderPreview}");
		}

		public readonly struct DisplayMesh
		{
			public readonly Vector3[] verts;
			public readonly int[] tris;

			public DisplayMesh(Vector3[] verts, int[] tris)
			{
				this.verts = verts;
				this.tris = tris;
			}

		}

		#region Subscriptions
		public UnityAction<CommitInfo> OnCommitCreated;
		public UnityAction<CommitInfo> OnCommitUpdated;
		#endregion

	}
}