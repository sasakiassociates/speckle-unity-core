using UnityEngine;

namespace Speckle.ConnectorUnity.Mono
{
	/// <summary>
	///   A simple version of the object Base from Speckle that contains the speckle properties type
	/// </summary>
	public class BaseBehaviour : MonoBehaviour
	{

		[SerializeField] protected SpeckleProperties _properties;

		public SpeckleProperties properties
		{
			get => _properties;
			set => _properties = value;
		}
	}
}