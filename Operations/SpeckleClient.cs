using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Converter;
using Speckle.Core.Api;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Speckle.ConnectorUnity
{
	public interface IveMadeProgress
	{
		public float CurrentProgress { get; set; }
	}

	public interface ISpeckleClient
	{
		public SpeckleStream stream { get; }
		public Branch branch { get; }
		public List<Branch> branches { get; }
		public ClientCache cache { get; }
		public UniTask<bool> SetStream(SpeckleStream stream);
	}

	[Serializable]
	public class ClientCache
	{
		public int branchIndex;
		public int commitIndex;
		public int converterIndex;
	}

	// BUG: issue with refreshing object data to editor, probably something with serializing the branch or commit data  
	public abstract class SpeckleClient : MonoBehaviour, ISpeckleClient, IveMadeProgress
	{

		[SerializeField] protected GameObject _root;
		[SerializeField] protected SpeckleStream _stream;
		[SerializeField] protected List<ScriptableSpeckleConverter> _converters;

		[SerializeField] protected float progressAmount;

		[SerializeField] [HideInInspector]
		private ClientCache _cache;

		private List<Branch> _branches = new List<Branch>();

		/// <summary>
		///   a disposable speckle client that we use to access speckly things
		/// </summary>
		protected Client client;

		/// <summary>
		///   an internal toggle to use with uni-task commands
		/// </summary>
		protected bool isCanceled;

		/// <summary>
		///   Event hooked in to Speckle API when calling to the client object
		/// </summary>
		public Action<string, Exception> onErrorReport;

		/// <summary>
		///   An unformatted progress report during send or receive calls
		/// </summary>
		public Action<ConcurrentDictionary<string, int>> onProgressReport;

		/// <summary>
		///   Event for knowing total child count when a stream is pulled in
		/// </summary>
		public Action<int> onTotalChildrenCountKnown;

		public int totalChildCount { get; protected set; }

		public bool isWorking { get; protected set; }

		/// <summary>
		///   A list of all converters available for this client object
		/// </summary>
		public List<ScriptableSpeckleConverter> converters
		{
			get => _converters.Valid() ? _converters : new List<ScriptableSpeckleConverter>();
		}

		/// <summary>
		///   the active converter for this client object
		/// </summary>
		protected ScriptableSpeckleConverter converter
		{
			get => _converters.Valid(cache.converterIndex) ? _converters[cache.converterIndex] : null;
		}

		protected virtual void OnEnable()
		{
			// TODO: during the build process this should compile and store these objects. 
			#if UNITY_EDITOR
			_converters = GetAllInstances<ScriptableSpeckleConverter>();
			#endif

			onTotalChildrenCountKnown = i => totalChildCount = i;

			SetStream(stream).Forget();
		}

		private void OnDisable()
		{
			CleanUp();
		}

		private void OnDestroy()
		{
			CleanUp();
		}

		public SpeckleStream stream
		{
			get => _stream;
			protected set => _stream = value;
		}

		public Branch branch
		{
			get => branches.Valid(cache.branchIndex) ? branches[cache.branchIndex] : null;
		}
		public List<Branch> branches
		{
			get => _branches.Valid() ? _branches : new List<Branch>();
			protected set => _branches = value;
		}

		/// <summary> Necessary setup for interacting with a speckle stream from unity </summary>
		/// <param name="newStream">root stream object to use, will default to editor field</param>
		/// <returns></returns>
		public async UniTask<bool> SetStream(SpeckleStream newStream)
		{
			stream = newStream;
			if (stream == null || !stream.IsValid())
			{
				SpeckleConnector.Console.Log("Speckle stream object is not setup correctly");
				return false;
			}

			cache = new ClientCache();

			await LoadStream();

			SetSubscriptions();

			onRepaint?.Invoke();

			return client != null;
		}

		/// <summary>
		///   Temporary client data that stores simple interfacing data
		/// </summary>
		public ClientCache cache
		{
			get => _cache;
			protected set => _cache = value;
		}

		public float CurrentProgress
		{
			get => progressAmount;
			set => progressAmount = value;
		}

		public event Action onRepaint;

		public virtual void SetBranch(int i)
		{
			cache.branchIndex = branches.Check(i);
		}

		public void SetConverter(int i)
		{
			cache.converterIndex = _converters.Check(i);
		}

		/// <summary> Necessary setup for interacting with a speckle stream from unity</summary>
		/// <param name="rootStream">root stream object to use, will default to editor field</param>
		/// <param name="onProgressAction">Action to run when there is download/conversion progress</param>
		/// <param name="onErrorAction">Action to run on error</param>
		/// <param name="onTotalChildCountAction">Report for total child count</param>
		public async UniTask<bool> SetStream(
			SpeckleStream rootStream,
			Action<ConcurrentDictionary<string, int>> onProgressAction,
			Action<string, Exception> onErrorAction,
			Action<int> onTotalChildCountAction
		)
		{
			onErrorReport = onErrorAction;
			onProgressReport = onProgressAction;
			onTotalChildrenCountKnown = onTotalChildCountAction;

			return await SetStream(rootStream);
		}

		protected virtual async UniTask LoadStream()
		{
			var account = await stream.GetAccount();

			client = new Client(account);

			branches = await client.StreamGetBranches(this.GetCancellationTokenOnDestroy(), stream.Id);
		}

		/// <summary>
		///   Internal method for client objects to handle their subscriptions
		/// </summary>
		protected virtual void SetSubscriptions()
		{
			if (client == null) SpeckleConnector.Console.Log($"No active client on {name} to read from");
		}

		/// <summary>
		///   Check if stream and client is active and ready to use
		/// </summary>
		/// <returns></returns>
		protected bool IsReady()
		{
			var res = true;

			if (stream == null || !stream.IsValid())
			{
				SpeckleConnector.Console.Log($"No active stream ready for {name} to use");
				res = false;
			}

			if (client == null)
			{
				SpeckleConnector.Console.Log($"No active client for {name} to use");
				res = false;
			}

			return res;
		}

		/// <summary>
		///   Clean up to any client things
		/// </summary>
		protected virtual void CleanUp()
		{
			client?.Dispose();
		}

		#if UNITY_EDITOR
		public static List<T> GetAllInstances<T>() where T : ScriptableObject
		{
			var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
			var items = new List<T>();
			foreach (var g in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(g);
				items.Add(AssetDatabase.LoadAssetAtPath<T>(path));
			}
			return items;
		}
		#endif
	}
}