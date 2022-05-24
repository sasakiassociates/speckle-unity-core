using System;
using System.Collections.Generic;
using System.Linq;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{

	public abstract class ScriptableSpeckleConverter : ScriptableObject, ISpeckleConverter
	{
		[Header("Speckle Converter Informations")]
		[SerializeField] protected string description;
		[SerializeField] protected string author;
		[SerializeField] protected string websiteOrEmail;

		[Space]
		[SerializeField] protected List<ComponentConverter> otherConverters;

		private Dictionary<string, ComponentConverter> compiled;

		public HashSet<Exception> ConversionErrors { get; } = new HashSet<Exception>();

		public List<ApplicationPlaceholderObject> ContextObjects { get; set; } = new List<ApplicationPlaceholderObject>();

		public ProgressReport Report { get; protected set; }

		public IEnumerable<string> GetServicedApplications() => new[] { HostApplications.Unity.Name };

		public virtual void SetContextObjects(List<ApplicationPlaceholderObject> objects)
		{
			ContextObjects = objects;
		}

		public virtual void SetPreviousContextObjects(List<ApplicationPlaceholderObject> objects)
		{
			ContextObjects = objects;
		}

		public virtual void SetContextDocument(object doc)
		{
			Debug.Log("Empty call from SetContextDocument");
		}

		public virtual void SetConverterSettings(object settings)
		{
			Debug.Log($"Converter Settings being set with {settings}");
		}

		public abstract Base ConvertToSpeckle(object @object);
		public abstract object ConvertToNative(Base @base);

		public virtual List<Base> ConvertToSpeckle(List<object> objects) => objects.Select(ConvertToSpeckle).ToList();

		public virtual List<object> ConvertToNative(List<Base> objects) => objects.Select(ConvertToNative).ToList();

		public virtual bool CanConvertToSpeckle(object @object) => @object is Component comp && CanConvertToSpeckle(comp);

		public virtual bool CanConvertToNative(Base @object)
		{
			CheckIfCompiled(true);

			return compiled != null && compiled.Any(x => x.Value.CanConvertToNative(@object));
		}

		protected void CheckIfCompiled(bool toUnity, bool force = false)
		{
			if (force || compiled == null || !compiled.Any())
			{
				compiled = new Dictionary<string, ComponentConverter>();

				foreach (var field in GetType().GetFields())
					if (field.FieldType == typeof(ComponentConverter) && Activator.CreateInstance(field.FieldType) is ComponentConverter c)
						compiled.Add(c.targetType(toUnity), c);
			}
		}

		public virtual bool CanConvertToSpeckle(Component @object)
		{
			CheckIfCompiled(false);

			return compiled != null && compiled.Any(x => x.Value.CanConvertToSpeckle(@object));
		}

		#region converter properties
		public string Name
		{
			get => name;
		}

		public string Description
		{
			get => description;
		}

		public string Author
		{
			get => author;
		}

		public string WebsiteOrEmail
		{
			get => websiteOrEmail;
		}
		#endregion converter properties

	}
}