#if UNITY_WINRT && !UNITY_EDITOR && !UNITY_WP8
#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Newtonsoft.Json.Serialization
{
	internal struct ResolverContractKey : IEquatable<ResolverContractKey>
	{
		private readonly Type _resolverType;
		private readonly Type _contractType;

		public ResolverContractKey(Type resolverType, Type contractType)
		{
			_resolverType = resolverType;
			_contractType = contractType;
		}

		public override int GetHashCode()
		{
			return _resolverType.GetHashCode() ^ _contractType.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ResolverContractKey))
				return false;

			return Equals((ResolverContractKey)obj);
		}

		public bool Equals(ResolverContractKey other)
		{
			return (_resolverType == other._resolverType && _contractType == other._contractType);
		}
	}

	/// <summary>
	/// Used by <see cref="JsonSerializer"/> to resolves a <see cref="JsonContract"/> for a given <see cref="Type"/>.
	/// </summary>
	public class DefaultContractResolver : IContractResolver
	{
		private static readonly IContractResolver _instance = new DefaultContractResolver(true);
		internal static IContractResolver Instance
		{
			get { return _instance; }
		}
		private static readonly IList<JsonConverter> BuiltInConverters = new List<JsonConverter>
      {
        new ExpandoObjectConverter(),
        new XmlNodeConverter(),
        new JsonValueConverter(),
        new KeyValuePairConverter(),
        new BsonObjectIdConverter(),
        new RegexConverter()
      };

		private static Dictionary<ResolverContractKey, JsonContract> _sharedContractCache;
		private static readonly object _typeContractCacheLock = new object();

		private Dictionary<ResolverContractKey, JsonContract> _instanceContractCache;
		private readonly bool _sharedCache;

		/// <summary>
		/// Gets a value indicating whether members are being get and set using dynamic code generation.
		/// This value is determined by the runtime permissions available.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if using dynamic code generation; otherwise, <c>false</c>.
		/// </value>
		public bool DynamicCodeGeneration
		{
			get { return JsonTypeReflector.DynamicCodeGeneration; }
		}

		private Newtonsoft.Json.Utilities.BindingFlags DefaultMembersSearchFlags = Newtonsoft.Json.Utilities.BindingFlags.Instance | Newtonsoft.Json.Utilities.BindingFlags.Public;

		/// <summary>
		/// Gets or sets a value indicating whether compiler generated members should be serialized.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if serialized compiler generated members; otherwise, <c>false</c>.
		/// </value>
		public bool SerializeCompilerGeneratedMembers { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultContractResolver"/> class.
		/// </summary>
		public DefaultContractResolver()
			: this(false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultContractResolver"/> class.
		/// </summary>
		/// <param name="shareCache">
		/// If set to <c>true</c> the <see cref="DefaultContractResolver"/> will use a cached shared with other resolvers of the same type.
		/// Sharing the cache will significantly performance because expensive reflection will only happen once but could cause unexpected
		/// behavior if different instances of the resolver are suppose to produce different results. When set to false it is highly
		/// recommended to reuse <see cref="DefaultContractResolver"/> instances with the <see cref="JsonSerializer"/>.
		/// </param>
		public DefaultContractResolver(bool shareCache)
		{
			_sharedCache = shareCache;
		}

		private Dictionary<ResolverContractKey, JsonContract> GetCache()
		{
			if (_sharedCache)
				return _sharedContractCache;
			else
				return _instanceContractCache;
		}

		private void UpdateCache(Dictionary<ResolverContractKey, JsonContract> cache)
		{
			if (_sharedCache)
				_sharedContractCache = cache;
			else
				_instanceContractCache = cache;
		}

		/// <summary>
		/// Resolves the contract for a given type.
		/// </summary>
		/// <param name="type">The type to resolve a contract for.</param>
		/// <returns>The contract for a given type.</returns>
		public virtual JsonContract ResolveContract(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			JsonContract contract;
			ResolverContractKey key = new ResolverContractKey(GetType(), type);
			Dictionary<ResolverContractKey, JsonContract> cache = GetCache();
			if (cache == null || !cache.TryGetValue(key, out contract))
			{
				contract = CreateContract(type);

				// avoid the possibility of modifying the cache dictionary while another thread is accessing it
				lock (_typeContractCacheLock)
				{
					cache = GetCache();
					Dictionary<ResolverContractKey, JsonContract> updatedCache =
					  (cache != null)
						? new Dictionary<ResolverContractKey, JsonContract>(cache)
						: new Dictionary<ResolverContractKey, JsonContract>();
					updatedCache[key] = contract;

					UpdateCache(updatedCache);
				}
			}

			return contract;
		}

		/// <summary>
		/// Gets the serializable members for the type.
		/// </summary>
		/// <param name="objectType">The type to get serializable members for.</param>
		/// <returns>The serializable members for the type.</returns>
		protected virtual List<MemberInfo> GetSerializableMembers(Type objectType)
		{
			bool ignoreSerializableAttribute;
			ignoreSerializableAttribute = true;

			MemberSerialization memberSerialization = JsonTypeReflector.GetObjectMemberSerialization(objectType, ignoreSerializableAttribute);

			List<MemberInfo> allMembers = ReflectionUtils.GetFieldsAndProperties(objectType, Newtonsoft.Json.Utilities.BindingFlags.Public | Newtonsoft.Json.Utilities.BindingFlags.NonPublic | Newtonsoft.Json.Utilities.BindingFlags.Instance | Newtonsoft.Json.Utilities.BindingFlags.Static)
			  .Where(m => !ReflectionUtils.IsIndexedProperty(m)).ToList();

			List<MemberInfo> serializableMembers = new List<MemberInfo>();

			if (memberSerialization != MemberSerialization.Fields)
			{
				DataContractAttribute dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(objectType);

				List<MemberInfo> defaultMembers = ReflectionUtils.GetFieldsAndProperties(objectType, DefaultMembersSearchFlags)
				 .Where(m => !ReflectionUtils.IsIndexedProperty(m)).ToList();

				foreach (MemberInfo member in allMembers)
				{
					// exclude members that are compiler generated if set
					if (SerializeCompilerGeneratedMembers || !member.IsDefined(typeof(CompilerGeneratedAttribute), true))
					{
						if (defaultMembers.Contains(member))
						{
							// add all members that are found by default member search
							serializableMembers.Add(member);
						}
						else
						{
							// add members that are explicitly marked with JsonProperty/DataMember attribute
							// or are a field if serializing just fields
							if (JsonTypeReflector.GetAttribute<JsonPropertyAttribute>(member) != null)
								serializableMembers.Add(member);
							else if (dataContractAttribute != null && JsonTypeReflector.GetAttribute<DataMemberAttribute>(member) != null)
								serializableMembers.Add(member);
							else if (memberSerialization == MemberSerialization.Fields && member.MemberType() == Newtonsoft.Json.Utilities.MemberTypes.Field)
								serializableMembers.Add(member);
						}
					}
				}

				Type match;
				// don't include EntityKey on entities objects... this is a bit hacky
				if (objectType.AssignableToTypeName("System.Data.Objects.DataClasses.EntityObject", out match))
					serializableMembers = serializableMembers.Where(ShouldSerializeEntityMember).ToList();
			}
			else
			{
				// serialize all fields
				foreach (MemberInfo member in allMembers)
				{
					FieldInfo field = member as FieldInfo;
					if (field != null && !field.IsStatic)
						serializableMembers.Add(member);
				}
			}

			return serializableMembers;
		}

		private bool ShouldSerializeEntityMember(MemberInfo memberInfo)
		{
			PropertyInfo propertyInfo = memberInfo as PropertyInfo;
			if (propertyInfo != null)
			{
				if (propertyInfo.PropertyType.IsGenericType() && propertyInfo.PropertyType.GetGenericTypeDefinition().FullName == "System.Data.Objects.DataClasses.EntityReference`1")
					return false;
			}

			return true;
		}

		/// <summary>
		/// Creates a <see cref="JsonObjectContract"/> for the given type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>A <see cref="JsonObjectContract"/> for the given type.</returns>
		protected virtual JsonObjectContract CreateObjectContract(Type objectType)
		{
			JsonObjectContract contract = new JsonObjectContract(objectType);
			InitializeContract(contract);

			bool ignoreSerializableAttribute;
			ignoreSerializableAttribute = true;

			contract.MemberSerialization = JsonTypeReflector.GetObjectMemberSerialization(contract.NonNullableUnderlyingType, ignoreSerializableAttribute);
			contract.Properties.AddRange(CreateProperties(contract.NonNullableUnderlyingType, contract.MemberSerialization));

			JsonObjectAttribute attribute = JsonTypeReflector.GetJsonObjectAttribute(contract.NonNullableUnderlyingType);
			if (attribute != null)
				contract.ItemRequired = attribute._itemRequired;

			ConstructorInfo overrideConstructor = GetAttributeConstructor(contract.NonNullableUnderlyingType);

			// check if a JsonConstructorAttribute has been defined and use that
			if (overrideConstructor != null)
			{
				contract.OverrideConstructor = overrideConstructor;
				contract.ConstructorParameters.AddRange(CreateConstructorParameters(overrideConstructor, contract.Properties));
			}
			else if (contract.MemberSerialization == MemberSerialization.Fields)
			{

			}
			else if (contract.DefaultCreator == null || contract.DefaultCreatorNonPublic)
			{
				ConstructorInfo constructor = GetParametrizedConstructor(contract.NonNullableUnderlyingType);
				if (constructor != null)
				{
					contract.ParametrizedConstructor = constructor;
					contract.ConstructorParameters.AddRange(CreateConstructorParameters(constructor, contract.Properties));
				}
			}

			MemberInfo extensionDataMember = GetExtensionDataMemberForType(contract.NonNullableUnderlyingType);
			if (extensionDataMember != null)
				SetExtensionDataDelegates(contract, extensionDataMember);

			return contract;
		}

		private MemberInfo GetExtensionDataMemberForType(Type type)
		{
			IEnumerable<MemberInfo> members = GetClassHierarchyForType(type).SelectMany(baseType =>
			{
				IList<MemberInfo> m = new List<MemberInfo>();
				m.AddRange(baseType.GetProperties(Newtonsoft.Json.Utilities.BindingFlags.Public | Newtonsoft.Json.Utilities.BindingFlags.NonPublic | Newtonsoft.Json.Utilities.BindingFlags.Instance | Newtonsoft.Json.Utilities.BindingFlags.DeclaredOnly));
				m.AddRange(baseType.GetFields(Newtonsoft.Json.Utilities.BindingFlags.Public | Newtonsoft.Json.Utilities.BindingFlags.NonPublic | Newtonsoft.Json.Utilities.BindingFlags.Instance | Newtonsoft.Json.Utilities.BindingFlags.DeclaredOnly));

				return m;
			});

			MemberInfo extensionDataMember = members.LastOrDefault(m =>
			{
				Newtonsoft.Json.Utilities.MemberTypes memberType = m.MemberType();
				if (memberType != Newtonsoft.Json.Utilities.MemberTypes.Property && memberType != Newtonsoft.Json.Utilities.MemberTypes.Field)
					return false;

				// last instance of attribute wins on type if there are multiple
				if (!m.IsDefined(typeof(JsonExtensionDataAttribute), false))
					return false;

				Type t = ReflectionUtils.GetMemberUnderlyingType(m);

				Type dictionaryType;
				if (ReflectionUtils.ImplementsGenericDefinition(t, typeof(IDictionary<,>), out dictionaryType))
				{
					Type keyType = dictionaryType.GetGenericArguments()[0];
					Type valueType = dictionaryType.GetGenericArguments()[1];

					if (keyType.IsAssignableFrom(typeof(string)) && valueType.IsAssignableFrom(typeof(JToken)))
						return true;
				}

				throw new JsonException("Invalid extension data attribute on '{0}'. Member '{1}' type must implement IDictionary<string, JToken>.".FormatWith(CultureInfo.InvariantCulture, GetClrTypeFullName(m.DeclaringType), m.Name));
			});

			return extensionDataMember;
		}

		private static void SetExtensionDataDelegates(JsonObjectContract contract, MemberInfo member)
		{
			JsonExtensionDataAttribute extensionDataAttribute = ReflectionUtils.GetAttribute<JsonExtensionDataAttribute>(member);
			if (extensionDataAttribute == null)
				return;

			Type t = ReflectionUtils.GetMemberUnderlyingType(member);

			Type dictionaryType;
			ReflectionUtils.ImplementsGenericDefinition(t, typeof(IDictionary<,>), out dictionaryType);

			Type keyType = dictionaryType.GetGenericArguments()[0];
			Type valueType = dictionaryType.GetGenericArguments()[1];
			bool isJTokenValueType = typeof(JToken).IsAssignableFrom(valueType);

			// change type to a class if it is the base interface so it can be instantiated if needed
			if (ReflectionUtils.IsGenericDefinition(t, typeof(IDictionary<,>)))
				t = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

			MethodInfo addMethod = t.GetMethod("Add", new[] { keyType, valueType });
			Func<object, object> getExtensionDataDictionary = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(member);
			Action<object, object> setExtensionDataDictionary = JsonTypeReflector.ReflectionDelegateFactory.CreateSet<object>(member);
			Func<object> createExtensionDataDictionary = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(t);
			MethodCall<object, object> setExtensionDataDictionaryValue = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(addMethod);

			ExtensionDataSetter extensionDataSetter = (o, key, value) =>
			{
				object dictionary = getExtensionDataDictionary(o);
				if (dictionary == null)
				{
					dictionary = createExtensionDataDictionary();
					setExtensionDataDictionary(o, dictionary);
				}

				// convert object value to JToken so it is compatible with dictionary
				// could happen because of primitive types, type name handling and references
				if (isJTokenValueType && !(value is JToken))
					value = JToken.FromObject(value);

				setExtensionDataDictionaryValue(dictionary, key, value);
			};

			Type enumerableWrapper = typeof(DictionaryEnumerator<,>).MakeGenericType(keyType, valueType);
			ConstructorInfo constructors = enumerableWrapper.GetConstructors().First();
			MethodCall<object, object> createEnumerableWrapper = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(constructors);

			ExtensionDataGetter extensionDataGetter = o =>
			{
				object dictionary = getExtensionDataDictionary(o);
				if (dictionary == null)
					return null;

				return (IEnumerable<KeyValuePair<object, object>>)createEnumerableWrapper(null, dictionary);
			};

			if (extensionDataAttribute.ReadData)
				contract.ExtensionDataSetter = extensionDataSetter;

			if (extensionDataAttribute.WriteData)
				contract.ExtensionDataGetter = extensionDataGetter;
		}

		internal struct DictionaryEnumerator<TEnumeratorKey, TEnumeratorValue> : IEnumerable<KeyValuePair<object, object>>, IEnumerator<KeyValuePair<object, object>>
		{
			private readonly IEnumerator<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> _e;

			public DictionaryEnumerator(IEnumerable<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> e)
			{
				ValidationUtils.ArgumentNotNull(e, "e");
				_e = e.GetEnumerator();
			}

			public bool MoveNext()
			{
				return _e.MoveNext();
			}

			public void Reset()
			{
				_e.Reset();
			}

			public KeyValuePair<object, object> Current
			{
				get { return new KeyValuePair<object, object>(_e.Current.Key, _e.Current.Value); }
			}

			public void Dispose()
			{
				_e.Dispose();
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
			{
				return this;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this;
			}
		}

		private ConstructorInfo GetAttributeConstructor(Type objectType)
		{
			IList<ConstructorInfo> markedConstructors = objectType.GetConstructors(Newtonsoft.Json.Utilities.BindingFlags.Instance | Newtonsoft.Json.Utilities.BindingFlags.Public | Newtonsoft.Json.Utilities.BindingFlags.NonPublic).Where(c => c.IsDefined(typeof(JsonConstructorAttribute), true)).ToList();

			if (markedConstructors.Count > 1)
				throw new JsonException("Multiple constructors with the JsonConstructorAttribute.");
			else if (markedConstructors.Count == 1)
				return markedConstructors[0];

			// little hack to get Version objects to deserialize correctly
			if (objectType == typeof(Version))
				return objectType.GetConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int) });

			return null;
		}

		private ConstructorInfo GetParametrizedConstructor(Type objectType)
		{
			IList<ConstructorInfo> constructors = objectType.GetConstructors(Newtonsoft.Json.Utilities.BindingFlags.Public | Newtonsoft.Json.Utilities.BindingFlags.Instance).ToList();

			if (constructors.Count == 1)
				return constructors[0];
			else
				return null;
		}

		/// <summary>
		/// Creates the constructor parameters.
		/// </summary>
		/// <param name="constructor">The constructor to create properties for.</param>
		/// <param name="memberProperties">The type's member properties.</param>
		/// <returns>Properties for the given <see cref="ConstructorInfo"/>.</returns>
		protected virtual IList<JsonProperty> CreateConstructorParameters(ConstructorInfo constructor, JsonPropertyCollection memberProperties)
		{
			var constructorParameters = constructor.GetParameters();

			JsonPropertyCollection parameterCollection = new JsonPropertyCollection(constructor.DeclaringType);

			foreach (ParameterInfo parameterInfo in constructorParameters)
			{
				// it is possible to generate a ParameterInfo with a null name using Reflection.Emit
				// protect against an ArgumentNullException from GetClosestMatchProperty by testing for null here
				JsonProperty matchingMemberProperty = (parameterInfo.Name != null) ? memberProperties.GetClosestMatchProperty(parameterInfo.Name) : null;

				// type must match as well as name
				if (matchingMemberProperty != null && matchingMemberProperty.PropertyType != parameterInfo.ParameterType)
					matchingMemberProperty = null;

				// ensure that property will have a name from matching property or from parameterinfo
				// parameterinfo could have no name if generated by a proxy (I'm looking at you Castle)
				if (matchingMemberProperty != null || parameterInfo.Name != null)
				{
					JsonProperty property = CreatePropertyFromConstructorParameter(matchingMemberProperty, parameterInfo);

					if (property != null)
					{
						parameterCollection.AddProperty(property);
					}
				}
			}

			return parameterCollection;
		}

		/// <summary>
		/// Creates a <see cref="JsonProperty"/> for the given <see cref="ParameterInfo"/>.
		/// </summary>
		/// <param name="matchingMemberProperty">The matching member property.</param>
		/// <param name="parameterInfo">The constructor parameter.</param>
		/// <returns>A created <see cref="JsonProperty"/> for the given <see cref="ParameterInfo"/>.</returns>
		protected virtual JsonProperty CreatePropertyFromConstructorParameter(JsonProperty matchingMemberProperty, ParameterInfo parameterInfo)
		{
			JsonProperty property = new JsonProperty();
			property.PropertyType = parameterInfo.ParameterType;

			bool allowNonPublicAccess;
			SetPropertySettingsFromAttributes(property, parameterInfo, parameterInfo.Name, parameterInfo.Member.DeclaringType, MemberSerialization.OptOut, out allowNonPublicAccess);

			property.Readable = false;
			property.Writable = true;

			// "inherit" values from matching member property if unset on parameter
			if (matchingMemberProperty != null)
			{
				property.PropertyName = (property.PropertyName != parameterInfo.Name) ? property.PropertyName : matchingMemberProperty.PropertyName;
				property.Converter = property.Converter ?? matchingMemberProperty.Converter;
				property.MemberConverter = property.MemberConverter ?? matchingMemberProperty.MemberConverter;

				if (!property._hasExplicitDefaultValue && matchingMemberProperty._hasExplicitDefaultValue)
					property.DefaultValue = matchingMemberProperty.DefaultValue;

				property._required = property._required ?? matchingMemberProperty._required;
				property.IsReference = property.IsReference ?? matchingMemberProperty.IsReference;
				property.NullValueHandling = property.NullValueHandling ?? matchingMemberProperty.NullValueHandling;
				property.DefaultValueHandling = property.DefaultValueHandling ?? matchingMemberProperty.DefaultValueHandling;
				property.ReferenceLoopHandling = property.ReferenceLoopHandling ?? matchingMemberProperty.ReferenceLoopHandling;
				property.ObjectCreationHandling = property.ObjectCreationHandling ?? matchingMemberProperty.ObjectCreationHandling;
				property.TypeNameHandling = property.TypeNameHandling ?? matchingMemberProperty.TypeNameHandling;
			}

			return property;
		}

		/// <summary>
		/// Resolves the default <see cref="JsonConverter" /> for the contract.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>The contract's default <see cref="JsonConverter" />.</returns>
		protected virtual JsonConverter ResolveContractConverter(Type objectType)
		{
			return JsonTypeReflector.GetJsonConverter(objectType, objectType);
		}

		private Func<object> GetDefaultCreator(Type createdType)
		{
			return JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(createdType);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Runtime.Serialization.DataContractAttribute.#get_IsReference()")]
		private void InitializeContract(JsonContract contract)
		{
			JsonContainerAttribute containerAttribute = JsonTypeReflector.GetJsonContainerAttribute(contract.NonNullableUnderlyingType);
			if (containerAttribute != null)
			{
				contract.IsReference = containerAttribute._isReference;
			}
			else
			{
				DataContractAttribute dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(contract.NonNullableUnderlyingType);
				// doesn't have a null value
				if (dataContractAttribute != null && dataContractAttribute.IsReference)
					contract.IsReference = true;
			}

			contract.Converter = ResolveContractConverter(contract.NonNullableUnderlyingType);

			// then see whether object is compadible with any of the built in converters
			contract.InternalConverter = JsonSerializer.GetMatchingConverter(BuiltInConverters, contract.NonNullableUnderlyingType);

			if (ReflectionUtils.HasDefaultConstructor(contract.CreatedType, true)
			  || contract.CreatedType.IsValueType())
			{
				contract.DefaultCreator = GetDefaultCreator(contract.CreatedType);

				contract.DefaultCreatorNonPublic = (!contract.CreatedType.IsValueType() &&
													ReflectionUtils.GetDefaultConstructor(contract.CreatedType) == null);
			}

			ResolveCallbackMethods(contract, contract.NonNullableUnderlyingType);
		}

		private void ResolveCallbackMethods(JsonContract contract, Type t)
		{
			List<SerializationCallback> onSerializing;
			List<SerializationCallback> onSerialized;
			List<SerializationCallback> onDeserializing;
			List<SerializationCallback> onDeserialized;
			List<SerializationErrorCallback> onError;

			GetCallbackMethodsForType(t, out onSerializing, out onSerialized, out onDeserializing, out onDeserialized, out onError);

			if (onSerializing != null)
			{
				if (!t.IsGenericType() || (t.GetGenericTypeDefinition() != typeof(ConcurrentDictionary<,>)))
					contract.OnSerializingCallbacks.AddRange(onSerializing);
			}

			if (onSerialized != null)
				contract.OnSerializedCallbacks.AddRange(onSerialized);

			if (onDeserializing != null)
				contract.OnDeserializingCallbacks.AddRange(onDeserializing);

			if (onDeserialized != null)
			{
				// ConcurrentDictionary throws an error here so don't use its OnDeserialized - http://json.codeplex.com/discussions/257093
				if (!t.IsGenericType() || (t.GetGenericTypeDefinition() != typeof(ConcurrentDictionary<,>)))
					contract.OnDeserializedCallbacks.AddRange(onDeserialized);
			}

			if (onError != null)
				contract.OnErrorCallbacks.AddRange(onError);
		}

		private void GetCallbackMethodsForType(Type type, out List<SerializationCallback> onSerializing, out List<SerializationCallback> onSerialized, out List<SerializationCallback> onDeserializing, out List<SerializationCallback> onDeserialized, out List<SerializationErrorCallback> onError)
		{
			onSerializing = null;
			onSerialized = null;
			onDeserializing = null;
			onDeserialized = null;
			onError = null;

			foreach (Type baseType in GetClassHierarchyForType(type))
			{
				// while we allow more than one OnSerialized total, only one can be defined per class
				MethodInfo currentOnSerializing = null;
				MethodInfo currentOnSerialized = null;
				MethodInfo currentOnDeserializing = null;
				MethodInfo currentOnDeserialized = null;
				MethodInfo currentOnError = null;

				foreach (MethodInfo method in baseType.GetMethods(Newtonsoft.Json.Utilities.BindingFlags.NonPublic | Newtonsoft.Json.Utilities.BindingFlags.Public | Newtonsoft.Json.Utilities.BindingFlags.Instance | Newtonsoft.Json.Utilities.BindingFlags.DeclaredOnly))
				{
					// compact framework errors when getting parameters for a generic method
					// lame, but generic methods should not be callbacks anyway
					if (method.ContainsGenericParameters)
						continue;

					Type prevAttributeType = null;
					ParameterInfo[] parameters = method.GetParameters();

					if (IsValidCallback(method, parameters, typeof(OnSerializingAttribute), currentOnSerializing, ref prevAttributeType))
					{
						onSerializing = onSerializing ?? new List<SerializationCallback>();
						onSerializing.Add(JsonContract.CreateSerializationCallback(method));
						currentOnSerializing = method;
					}
					if (IsValidCallback(method, parameters, typeof(OnSerializedAttribute), currentOnSerialized, ref prevAttributeType))
					{
						onSerialized = onSerialized ?? new List<SerializationCallback>();
						onSerialized.Add(JsonContract.CreateSerializationCallback(method));
						currentOnSerialized = method;
					}
					if (IsValidCallback(method, parameters, typeof(OnDeserializingAttribute), currentOnDeserializing, ref prevAttributeType))
					{
						onDeserializing = onDeserializing ?? new List<SerializationCallback>();
						onDeserializing.Add(JsonContract.CreateSerializationCallback(method));
						currentOnDeserializing = method;
					}
					if (IsValidCallback(method, parameters, typeof(OnDeserializedAttribute), currentOnDeserialized, ref prevAttributeType))
					{
						onDeserialized = onDeserialized ?? new List<SerializationCallback>();
						onDeserialized.Add(JsonContract.CreateSerializationCallback(method));
						currentOnDeserialized = method;
					}
					if (IsValidCallback(method, parameters, typeof(OnErrorAttribute), currentOnError, ref prevAttributeType))
					{
						onError = onError ?? new List<SerializationErrorCallback>();
						onError.Add(JsonContract.CreateSerializationErrorCallback(method));
						currentOnError = method;
					}
				}
			}
		}

		private List<Type> GetClassHierarchyForType(Type type)
		{
			List<Type> ret = new List<Type>();

			Type current = type;
			while (current != null && current != typeof(object))
			{
				ret.Add(current);
				current = current.BaseType();
			}

			// Return the class list in order of simple => complex
			ret.Reverse();
			return ret;
		}

		/// <summary>
		/// Creates a <see cref="JsonDictionaryContract"/> for the given type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>A <see cref="JsonDictionaryContract"/> for the given type.</returns>
		protected virtual JsonDictionaryContract CreateDictionaryContract(Type objectType)
		{
			JsonDictionaryContract contract = new JsonDictionaryContract(objectType);
			InitializeContract(contract);

			contract.PropertyNameResolver = ResolvePropertyName;

			return contract;
		}

		/// <summary>
		/// Creates a <see cref="JsonArrayContract"/> for the given type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>A <see cref="JsonArrayContract"/> for the given type.</returns>
		protected virtual JsonArrayContract CreateArrayContract(Type objectType)
		{
			JsonArrayContract contract = new JsonArrayContract(objectType);
			InitializeContract(contract);

			return contract;
		}

		/// <summary>
		/// Creates a <see cref="JsonPrimitiveContract"/> for the given type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>A <see cref="JsonPrimitiveContract"/> for the given type.</returns>
		protected virtual JsonPrimitiveContract CreatePrimitiveContract(Type objectType)
		{
			JsonPrimitiveContract contract = new JsonPrimitiveContract(objectType);
			InitializeContract(contract);

			return contract;
		}

		/// <summary>
		/// Creates a <see cref="JsonLinqContract"/> for the given type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>A <see cref="JsonLinqContract"/> for the given type.</returns>
		protected virtual JsonLinqContract CreateLinqContract(Type objectType)
		{
			JsonLinqContract contract = new JsonLinqContract(objectType);
			InitializeContract(contract);

			return contract;
		}

		/// <summary>
		/// Creates a <see cref="JsonDynamicContract"/> for the given type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>A <see cref="JsonDynamicContract"/> for the given type.</returns>
		protected virtual JsonDynamicContract CreateDynamicContract(Type objectType)
		{
			JsonDynamicContract contract = new JsonDynamicContract(objectType);
			InitializeContract(contract);

			contract.PropertyNameResolver = ResolvePropertyName;
			contract.Properties.AddRange(CreateProperties(objectType, MemberSerialization.OptOut));

			return contract;
		}

		/// <summary>
		/// Creates a <see cref="JsonStringContract"/> for the given type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>A <see cref="JsonStringContract"/> for the given type.</returns>
		protected virtual JsonStringContract CreateStringContract(Type objectType)
		{
			JsonStringContract contract = new JsonStringContract(objectType);
			InitializeContract(contract);

			return contract;
		}

		/// <summary>
		/// Determines which contract type is created for the given type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>A <see cref="JsonContract"/> for the given type.</returns>
		protected virtual JsonContract CreateContract(Type objectType)
		{
			Type t = ReflectionUtils.EnsureNotNullableType(objectType);

			if (IsJsonPrimitiveType(objectType))
				return CreatePrimitiveContract(objectType);

			if (JsonTypeReflector.GetJsonObjectAttribute(t) != null)
				return CreateObjectContract(objectType);

			if (JsonTypeReflector.GetJsonArrayAttribute(t) != null)
				return CreateArrayContract(objectType);

			if (JsonTypeReflector.GetJsonDictionaryAttribute(t) != null)
				return CreateDictionaryContract(objectType);

			if (t == typeof(JToken) || t.IsSubclassOf(typeof(JToken)))
				return CreateLinqContract(objectType);

			if (CollectionUtils.IsDictionaryType(t))
				return CreateDictionaryContract(objectType);

			if (typeof(IEnumerable).IsAssignableFrom(t))
				return CreateArrayContract(objectType);

			if (CanConvertToString(t))
				return CreateStringContract(objectType);

			if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(t))
				return CreateDynamicContract(objectType);

			return CreateObjectContract(objectType);
		}

		internal static bool IsJsonPrimitiveType(Type t)
		{
			PrimitiveTypeCode typeCode = ConvertUtils.GetTypeCode(t);

			return (typeCode != PrimitiveTypeCode.Empty && typeCode != PrimitiveTypeCode.Object);
		}

		internal static bool CanConvertToString(Type type)
		{
			if (type == typeof(Type) || type.IsSubclassOf(typeof(Type)))
				return true;

			return false;
		}

		private static bool IsValidCallback(MethodInfo method, ParameterInfo[] parameters, Type attributeType, MethodInfo currentCallback, ref Type prevAttributeType)
		{
			if (!method.IsDefined(attributeType, false))
				return false;

			if (currentCallback != null)
				throw new JsonException("Invalid attribute. Both '{0}' and '{1}' in type '{2}' have '{3}'.".FormatWith(CultureInfo.InvariantCulture, method, currentCallback, GetClrTypeFullName(method.DeclaringType), attributeType));

			if (prevAttributeType != null)
				throw new JsonException("Invalid Callback. Method '{3}' in type '{2}' has both '{0}' and '{1}'.".FormatWith(CultureInfo.InvariantCulture, prevAttributeType, attributeType, GetClrTypeFullName(method.DeclaringType), method));

			if (method.IsVirtual)
				throw new JsonException("Virtual Method '{0}' of type '{1}' cannot be marked with '{2}' attribute.".FormatWith(CultureInfo.InvariantCulture, method, GetClrTypeFullName(method.DeclaringType), attributeType));

			if (method.ReturnType != typeof(void))
				throw new JsonException("Serialization Callback '{1}' in type '{0}' must return void.".FormatWith(CultureInfo.InvariantCulture, GetClrTypeFullName(method.DeclaringType), method));

			if (attributeType == typeof(OnErrorAttribute))
			{
				if (parameters == null || parameters.Length != 2 || parameters[0].ParameterType != typeof(StreamingContext) || parameters[1].ParameterType != typeof(ErrorContext))
					throw new JsonException("Serialization Error Callback '{1}' in type '{0}' must have two parameters of type '{2}' and '{3}'.".FormatWith(CultureInfo.InvariantCulture, GetClrTypeFullName(method.DeclaringType), method, typeof(StreamingContext), typeof(ErrorContext)));
			}
			else
			{
				if (parameters == null || parameters.Length != 1 || parameters[0].ParameterType != typeof(StreamingContext))
					throw new JsonException("Serialization Callback '{1}' in type '{0}' must have a single parameter of type '{2}'.".FormatWith(CultureInfo.InvariantCulture, GetClrTypeFullName(method.DeclaringType), method, typeof(StreamingContext)));
			}

			prevAttributeType = attributeType;

			return true;
		}

		internal static string GetClrTypeFullName(Type type)
		{
			if (type.IsGenericTypeDefinition() || !type.ContainsGenericParameters())
				return type.FullName;

			return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[] { type.Namespace, type.Name });
		}

		/// <summary>
		/// Creates properties for the given <see cref="JsonContract"/>.
		/// </summary>
		/// <param name="type">The type to create properties for.</param>
		/// /// <param name="memberSerialization">The member serialization mode for the type.</param>
		/// <returns>Properties for the given <see cref="JsonContract"/>.</returns>
		protected virtual IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			List<MemberInfo> members = GetSerializableMembers(type);
			if (members == null)
				throw new JsonSerializationException("Null collection of seralizable members returned.");

			JsonPropertyCollection properties = new JsonPropertyCollection(type);

			foreach (MemberInfo member in members)
			{
				JsonProperty property = CreateProperty(member, memberSerialization);

				if (property != null)
					properties.AddProperty(property);
			}

			IList<JsonProperty> orderedProperties = properties.OrderBy(p => p.Order ?? -1).ToList();
			return orderedProperties;
		}

		/// <summary>
		/// Creates the <see cref="IValueProvider"/> used by the serializer to get and set values from a member.
		/// </summary>
		/// <param name="member">The member.</param>
		/// <returns>The <see cref="IValueProvider"/> used by the serializer to get and set values from a member.</returns>
		protected virtual IValueProvider CreateMemberValueProvider(MemberInfo member)
		{
			// warning - this method use to cause errors with Intellitrace. Retest in VS Ultimate after changes
			IValueProvider valueProvider;

			valueProvider = new ExpressionValueProvider(member);

			return valueProvider;
		}

		/// <summary>
		/// Creates a <see cref="JsonProperty"/> for the given <see cref="MemberInfo"/>.
		/// </summary>
		/// <param name="memberSerialization">The member's parent <see cref="MemberSerialization"/>.</param>
		/// <param name="member">The member to create a <see cref="JsonProperty"/> for.</param>
		/// <returns>A created <see cref="JsonProperty"/> for the given <see cref="MemberInfo"/>.</returns>
		protected virtual JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			JsonProperty property = new JsonProperty();
			property.PropertyType = ReflectionUtils.GetMemberUnderlyingType(member);
			property.DeclaringType = member.DeclaringType;
			property.ValueProvider = CreateMemberValueProvider(member);

			bool allowNonPublicAccess;
			SetPropertySettingsFromAttributes(property, member, member.Name, member.DeclaringType, memberSerialization, out allowNonPublicAccess);

			if (memberSerialization != MemberSerialization.Fields)
			{
				property.Readable = ReflectionUtils.CanReadMemberValue(member, allowNonPublicAccess);
				property.Writable = ReflectionUtils.CanSetMemberValue(member, allowNonPublicAccess, property.HasMemberAttribute);
			}
			else
			{
				// write to readonly fields
				property.Readable = true;
				property.Writable = true;
			}
			property.ShouldSerialize = CreateShouldSerializeTest(member);

			SetIsSpecifiedActions(property, member, allowNonPublicAccess);

			return property;
		}

		private void SetPropertySettingsFromAttributes(JsonProperty property, object attributeProvider, string name, Type declaringType, MemberSerialization memberSerialization, out bool allowNonPublicAccess)
		{
			DataContractAttribute dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(declaringType);

			MemberInfo memberInfo = attributeProvider as MemberInfo;

			DataMemberAttribute dataMemberAttribute;
			if (dataContractAttribute != null && memberInfo != null)
				dataMemberAttribute = JsonTypeReflector.GetDataMemberAttribute((MemberInfo)memberInfo);
			else
				dataMemberAttribute = null;

			JsonPropertyAttribute propertyAttribute = JsonTypeReflector.GetAttribute<JsonPropertyAttribute>(attributeProvider);
			if (propertyAttribute != null)
				property.HasMemberAttribute = true;

			string mappedName;
			if (propertyAttribute != null && propertyAttribute.PropertyName != null)
				mappedName = propertyAttribute.PropertyName;
			else if (dataMemberAttribute != null && dataMemberAttribute.Name != null)
				mappedName = dataMemberAttribute.Name;
			else
				mappedName = name;

			property.PropertyName = ResolvePropertyName(mappedName);
			property.UnderlyingName = name;

			bool hasMemberAttribute = false;
			if (propertyAttribute != null)
			{
				property._required = propertyAttribute._required;
				property.Order = propertyAttribute._order;
				property.DefaultValueHandling = propertyAttribute._defaultValueHandling;
				hasMemberAttribute = true;
			}
			else if (dataMemberAttribute != null)
			{
				property._required = (dataMemberAttribute.IsRequired) ? Required.AllowNull : Required.Default;
				property.Order = (dataMemberAttribute.Order != -1) ? (int?)dataMemberAttribute.Order : null;
				property.DefaultValueHandling = (!dataMemberAttribute.EmitDefaultValue) ? (DefaultValueHandling?)DefaultValueHandling.Ignore : null;
				hasMemberAttribute = true;
			}

			bool hasJsonIgnoreAttribute =
			  JsonTypeReflector.GetAttribute<JsonIgnoreAttribute>(attributeProvider) != null
				// automatically ignore extension data dictionary property if it is public
			  || JsonTypeReflector.GetAttribute<JsonExtensionDataAttribute>(attributeProvider) != null;

			if (memberSerialization != MemberSerialization.OptIn)
			{
				bool hasIgnoreDataMemberAttribute = false;

				hasIgnoreDataMemberAttribute = (JsonTypeReflector.GetAttribute<IgnoreDataMemberAttribute>(attributeProvider) != null);

				// ignored if it has JsonIgnore or NonSerialized or IgnoreDataMember attributes
				property.Ignored = (hasJsonIgnoreAttribute || hasIgnoreDataMemberAttribute);
			}
			else
			{
				// ignored if it has JsonIgnore/NonSerialized or does not have DataMember or JsonProperty attributes
				property.Ignored = (hasJsonIgnoreAttribute || !hasMemberAttribute);
			}

			// resolve converter for property
			// the class type might have a converter but the property converter takes presidence
			property.Converter = JsonTypeReflector.GetJsonConverter(attributeProvider, property.PropertyType);
			property.MemberConverter = JsonTypeReflector.GetJsonConverter(attributeProvider, property.PropertyType);

			DefaultValueAttribute defaultValueAttribute = JsonTypeReflector.GetAttribute<DefaultValueAttribute>(attributeProvider);
			if (defaultValueAttribute != null)
				property.DefaultValue = defaultValueAttribute.Value;

			property.NullValueHandling = (propertyAttribute != null) ? propertyAttribute._nullValueHandling : null;
			property.ReferenceLoopHandling = (propertyAttribute != null) ? propertyAttribute._referenceLoopHandling : null;
			property.ObjectCreationHandling = (propertyAttribute != null) ? propertyAttribute._objectCreationHandling : null;
			property.TypeNameHandling = (propertyAttribute != null) ? propertyAttribute._typeNameHandling : null;
			property.IsReference = (propertyAttribute != null) ? propertyAttribute._isReference : null;

			property.ItemIsReference = (propertyAttribute != null) ? propertyAttribute._itemIsReference : null;
			property.ItemConverter =
			  (propertyAttribute != null && propertyAttribute.ItemConverterType != null)
				? JsonConverterAttribute.CreateJsonConverterInstance(propertyAttribute.ItemConverterType)
				: null;
			property.ItemReferenceLoopHandling = (propertyAttribute != null) ? propertyAttribute._itemReferenceLoopHandling : null;
			property.ItemTypeNameHandling = (propertyAttribute != null) ? propertyAttribute._itemTypeNameHandling : null;

			allowNonPublicAccess = false;
			if ((DefaultMembersSearchFlags & Newtonsoft.Json.Utilities.BindingFlags.NonPublic) == Newtonsoft.Json.Utilities.BindingFlags.NonPublic)
				allowNonPublicAccess = true;
			if (propertyAttribute != null)
				allowNonPublicAccess = true;
			if (memberSerialization == MemberSerialization.Fields)
				allowNonPublicAccess = true;

			if (dataMemberAttribute != null)
			{
				allowNonPublicAccess = true;
				property.HasMemberAttribute = true;
			}
		}

		private Predicate<object> CreateShouldSerializeTest(MemberInfo member)
		{
			MethodInfo shouldSerializeMethod = member.DeclaringType.GetMethod(JsonTypeReflector.ShouldSerializePrefix + member.Name, ReflectionUtils.EmptyTypes);

			if (shouldSerializeMethod == null || shouldSerializeMethod.ReturnType != typeof(bool))
				return null;

			MethodCall<object, object> shouldSerializeCall =
			  JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(shouldSerializeMethod);

			return o => (bool)shouldSerializeCall(o);
		}

		private void SetIsSpecifiedActions(JsonProperty property, MemberInfo member, bool allowNonPublicAccess)
		{
			MemberInfo specifiedMember = member.DeclaringType.GetProperty(member.Name + JsonTypeReflector.SpecifiedPostfix);
			if (specifiedMember == null)
				specifiedMember = member.DeclaringType.GetField(member.Name + JsonTypeReflector.SpecifiedPostfix);

			if (specifiedMember == null || ReflectionUtils.GetMemberUnderlyingType(specifiedMember) != typeof(bool))
			{
				return;
			}

			Func<object, object> specifiedPropertyGet = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(specifiedMember);

			property.GetIsSpecified = o => (bool)specifiedPropertyGet(o);

			if (ReflectionUtils.CanSetMemberValue(specifiedMember, allowNonPublicAccess, false))
				property.SetIsSpecified = JsonTypeReflector.ReflectionDelegateFactory.CreateSet<object>(specifiedMember);
		}

		/// <summary>
		/// Resolves the name of the property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns>Name of the property.</returns>
		protected internal virtual string ResolvePropertyName(string propertyName)
		{
			return propertyName;
		}

		/// <summary>
		/// Gets the resolved name of the property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns>Name of the property.</returns>
		public string GetResolvedPropertyName(string propertyName)
		{
			// this is a new method rather than changing the visibility of ResolvePropertyName to avoid
			// a breaking change for anyone who has overidden the method
			return ResolvePropertyName(propertyName);
		}
	}
}

#endif