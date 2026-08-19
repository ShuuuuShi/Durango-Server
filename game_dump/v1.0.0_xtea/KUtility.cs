using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BestHTTP;
using Building_;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Shared.Etc;
using UnityEngine;
using Yaml;
using Yaml.Util;

public static class KUtility
{
	private static JsonSerializerSettings _setting;

	private static readonly Vector2[] DefaultFlinchingLerpSample = (Vector2[])(object)new Vector2[8]
	{
		new Vector2(0f, 0f),
		new Vector2(0.06f, 1f),
		new Vector2(0.125f, 0.9f),
		new Vector2(0.18f, 1f),
		new Vector2(0.25f, 0.9f),
		new Vector2(0.31f, 0.95f),
		new Vector2(0.7f, 0f),
		new Vector2(1f, 0f)
	};

	private static readonly XXHash RandomHash = new XXHash(1);

	private static JsonSerializerSettings Setting
	{
		get
		{
			if (_setting == null)
			{
				_setting = new JsonSerializerSettings();
				_setting.Converters.Add(new KJsonConverter());
				_setting.Converters.Add(new KeyValuePairConverter());
			}
			return _setting;
		}
	}

	public static float GetRandomHash(ulong entityId)
	{
		return GetRandomHash((int)(entityId >> 32), (int)entityId);
	}

	public static float GetRandomHash(int x, int y)
	{
		return RandomHash.Value(x, y);
	}

	public static HTTPRequest RequestUrl(string url, Action<byte[]> callback, bool disableCache = false, Dictionary<string, string> headers = null, Dictionary<string, string> fields = null)
	{
		HTTPRequest hTTPRequest = new HTTPRequest(new Uri(url), (fields != null) ? HTTPMethods.Post : HTTPMethods.Get, delegate(HTTPRequest originalRequest, HTTPResponse response)
		{
			bool isCached;
			byte[] obj = ProcessResult(originalRequest, out isCached);
			if (callback != null)
			{
				callback(obj);
			}
		});
		hTTPRequest.AddHeader("Accept-Encoding", "gzip");
		if (headers == null)
		{
			hTTPRequest.AddHeader("Accept", "application/json");
		}
		else
		{
			Dictionary<string, string>.Enumerator enumerator = headers.GetEnumerator();
			while (enumerator.MoveNext())
			{
				hTTPRequest.AddHeader(enumerator.Current.Key, enumerator.Current.Value);
			}
		}
		if (fields != null)
		{
			Dictionary<string, string>.Enumerator enumerator2 = fields.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				hTTPRequest.AddField(enumerator2.Current.Key, enumerator2.Current.Value);
			}
		}
		hTTPRequest.DisableCache = disableCache;
		hTTPRequest.Send();
		return hTTPRequest;
	}

	public static HTTPRequest RequestYml<T>(string url, Action<T> callback, bool disableCache = false)
	{
		return RequestUrl(url, delegate(byte[] bytes)
		{
			if (callback != null)
			{
				callback((bytes == null) ? default(T) : ParseMsgPack<T>(bytes));
			}
		}, disableCache);
	}

	public static byte[] ProcessResult(HTTPRequest request, out bool isCached)
	{
		switch (request.State)
		{
		case HTTPRequestStates.Finished:
			if (request.Response.IsSuccess)
			{
				isCached = request.Response.IsFromCache;
				return request.Response.Data;
			}
			break;
		}
		isCached = false;
		return null;
	}

	public static bool TryParseHostPort(string address, out string host, out int port)
	{
		host = string.Empty;
		port = 0;
		if (string.IsNullOrEmpty(address))
		{
			return false;
		}
		try
		{
			host = address.Substring(0, address.LastIndexOf(':'));
			string s = address.Substring(host.Length + 1, address.Length - host.Length - 1);
			if (!int.TryParse(s, out port))
			{
				return false;
			}
		}
		catch (ArgumentException)
		{
			return false;
		}
		return true;
	}

	public static GameObject FindObjectByName(GameObject entity, string name, bool includeInactive = false)
	{
		Transform val = FindTransformByName(entity, name, includeInactive);
		if (Object.op_Implicit((Object)(object)val))
		{
			return ((Component)val).gameObject;
		}
		return null;
	}

	public static Transform FindTransformByName(GameObject entity, string name, bool includeInactive = false)
	{
		Transform[] componentsInChildren = entity.GetComponentsInChildren<Transform>(includeInactive);
		if (componentsInChildren == null)
		{
			return null;
		}
		int num = componentsInChildren.Length;
		for (int i = 0; i < num; i++)
		{
			if (((Object)componentsInChildren[i]).name == name)
			{
				return componentsInChildren[i];
			}
		}
		return null;
	}

	public static string ToString(Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return NGUIText.EncodeColor(color);
	}

	public static Color ToColor(string hex)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		return (Color)((hex?.Length ?? 0) switch
		{
			6 => NGUIText.ParseColor24(hex), 
			8 => NGUIText.ParseColor32(hex, 0), 
			_ => Color.white, 
		});
	}

	public static T ParseMsgPack<T>(byte[] data)
	{
		T result = default(T);
		if (data == null || data.Length == 0)
		{
			return result;
		}
		return ParseJson<T>(Encoding.UTF8.GetString(data));
	}

	public static T ParseJson<T>(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return default(T);
		}
		try
		{
			return JsonConvert.DeserializeObject<T>(json, Setting);
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
		return default(T);
	}

	public static T ParseJson<T>(byte[] data)
	{
		if (data == null)
		{
			return default(T);
		}
		return ParseJson<T>(Encoding.UTF8.GetString(data));
	}

	public static T ParseJson<T>(JToken jToken)
	{
		if (jToken == null)
		{
			return default(T);
		}
		T result;
		using (JTokenReader reader = new JTokenReader(jToken))
		{
			JsonSerializer jsonSerializer = JsonSerializer.Create(Setting);
			try
			{
				result = jsonSerializer.Deserialize<T>(reader);
			}
			catch (JsonSerializationException ex)
			{
				result = default(T);
				Debug.LogError((object)("Json Parse Error\n" + ex.Message + jToken));
			}
		}
		return result;
	}

	public static T ParseJsonFile<T>(string fileName)
	{
		Object obj = Resources.Load(fileName);
		TextAsset val = (TextAsset)(object)((obj is TextAsset) ? obj : null);
		if ((Object)(object)val == (Object)null)
		{
			Debug.LogError((object)("Cannot load json file - " + fileName));
			return default(T);
		}
		return ParseJson<T>(val.text);
	}

	public static string SerializeJson<T>(T data, bool indented = false, JsonSerializerSettings settings = null)
	{
		string result = string.Empty;
		try
		{
			result = JsonConvert.SerializeObject(data, indented ? Formatting.Indented : Formatting.None, (settings == null) ? Setting : settings);
		}
		catch (JsonSerializationException ex)
		{
			Debug.LogError((object)("Json Serialize Error\n" + ex.Message + data));
		}
		return result;
	}

	public static byte[] SerializeJsonToBytes<T>(T data, bool indented = false, JsonSerializerSettings settings = null)
	{
		string s = SerializeJson(data, indented, settings);
		return Encoding.UTF8.GetBytes(s);
	}

	public static Vector3 GetInteractionPosition(GameObject obj, bool ignoreY = true)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		CharacterBehavior component = obj.GetComponent<CharacterBehavior>();
		Vector3 result;
		if (Object.op_Implicit((Object)(object)component))
		{
			result = component.InteractionPosition;
		}
		else
		{
			ImmovableBase component2 = obj.GetComponent<ImmovableBase>();
			result = ((!Object.op_Implicit((Object)(object)component2)) ? obj.transform.position : component2.InteractionPosition);
		}
		if (ignoreY)
		{
			result.y = 0f;
		}
		return result;
	}

	public static float FlinchingFunc(float flPercent, Vector2[] flinchingLerpSample = null)
	{
		if (flinchingLerpSample == null)
		{
			flinchingLerpSample = DefaultFlinchingLerpSample;
		}
		int num = flinchingLerpSample.Length;
		for (int i = 1; i < num; i++)
		{
			if (flPercent < flinchingLerpSample[i].x)
			{
				float num2 = flinchingLerpSample[i].x - flinchingLerpSample[i - 1].x;
				if (num2 > 0f)
				{
					float num3 = flPercent - flinchingLerpSample[i - 1].x;
					float num4 = num3 / num2;
					return Mathf.Lerp(flinchingLerpSample[i - 1].y, flinchingLerpSample[i].y, num4);
				}
			}
		}
		return flinchingLerpSample[flinchingLerpSample.Length - 1].y;
	}

	public static bool RayCastContextAction(Ray ray, int mask, string tagname, out GameObject pickingObject)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		int count;
		RaycastHit[] hits = KCollisionUtility.RayCast(ray, float.PositiveInfinity, mask, out count);
		Transform transformOfNearestHit = GetTransformOfNearestHit(hits, count, tagname);
		if ((Object)(object)transformOfNearestHit != (Object)null)
		{
			pickingObject = ((Component)transformOfNearestHit).gameObject;
			return true;
		}
		pickingObject = null;
		return false;
	}

	private static Transform GetTransformOfNearestHit(RaycastHit[] hits, int count, string tagname)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		Transform val2 = null;
		for (int i = 0; i < count; i++)
		{
			RaycastHit val3 = hits[i];
			Transform val4 = ((!((Object)(object)((RaycastHit)(ref val3)).collider == (Object)null)) ? ((Component)((RaycastHit)(ref val3)).collider).transform : ((RaycastHit)(ref val3)).transform);
			if ((Object)(object)val2 != (Object)null && ((RaycastHit)(ref val)).distance <= ((RaycastHit)(ref val3)).distance)
			{
				continue;
			}
			while ((Object)(object)val4 != (Object)null)
			{
				if (tagname == null || ((Component)val4).gameObject.CompareTag(tagname))
				{
					val = val3;
					val2 = val4;
					break;
				}
				val4 = val4.parent;
			}
		}
		return val2;
	}

	public static void DelayedCall(MonoBehaviour owner, Action func, float delay)
	{
		if (func != null)
		{
			if (delay < 0f)
			{
				func();
			}
			else
			{
				owner.StartCoroutine(CoDelayedCall(func, delay));
			}
		}
	}

	private static IEnumerator CoDelayedCall(Action func, float delay)
	{
		if (func != null)
		{
			yield return (object)new WaitForSeconds(delay);
			func();
		}
	}

	public static bool UseSharedMaterial(GameObject gameObject)
	{
		return false;
	}

	public static Material[] GetMaterials(Renderer renderer)
	{
		return renderer.materials;
	}

	public static Material GetMaterial(Renderer renderer)
	{
		return renderer.material;
	}

	public static string ToCamelCase(string value)
	{
		return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Replace('_', ' ')).Replace(" ", string.Empty);
	}

	public static Vector3 DirectionToAngle(Direction dir)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		return (Vector3)(dir switch
		{
			Direction.SouthWest => Vector3.up * 0f, 
			Direction.SouthEast => Vector3.up * -90f, 
			Direction.NorthWest => Vector3.up * 90f, 
			Direction.NorthEast => Vector3.up * 180f, 
			Direction.West => Vector3.up * 45f, 
			Direction.South => Vector3.up * -45f, 
			Direction.East => Vector3.up * -135f, 
			Direction.North => Vector3.up * 135f, 
			_ => throw new ArgumentException("Invalid Direction - " + dir), 
		});
	}

	public static Vector2 GetDirectionPivot(Direction dir)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		return (Vector2)(dir switch
		{
			Direction.SouthWest => new Vector2(0f, 0.5f), 
			Direction.SouthEast => new Vector2(0.5f, 0f), 
			Direction.NorthWest => new Vector2(0.5f, 1f), 
			Direction.NorthEast => new Vector2(1f, 0.5f), 
			Direction.West => new Vector2(0f, 1f), 
			Direction.South => new Vector2(0f, 0f), 
			Direction.East => new Vector2(1f, 0f), 
			Direction.North => new Vector2(1f, 1f), 
			_ => throw new ArgumentException("Invalid Direction - " + dir), 
		});
	}

	public static Direction RotationToDirection(Rotation rotation)
	{
		return rotation switch
		{
			Rotation.None => Direction.SouthWest, 
			Rotation.Quarter => Direction.SouthEast, 
			Rotation.Half => Direction.NorthEast, 
			Rotation.ThreeQuarter => Direction.NorthWest, 
			_ => throw new ArgumentException("Invalid Rotation - " + rotation), 
		};
	}

	public static ulong Max(params ulong[] values)
	{
		ulong num = 0uL;
		int i = 0;
		for (int num2 = values.Length; i < num2; i++)
		{
			if (values[i] > num)
			{
				num = values[i];
			}
		}
		return num;
	}

	public static double GetTimestamp()
	{
		return (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
	}

	public static IMotionPlayable FindMotionPlayable(GameObject obj)
	{
		MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
		int num = components.Length;
		for (int i = 0; i < num; i++)
		{
			if (components[i] is IMotionPlayable result)
			{
				return result;
			}
		}
		return null;
	}

	public static string FindMotionContains(GameObject gameObject, string nameContains)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		Animation[] componentsInChildren = gameObject.GetComponentsInChildren<Animation>(true);
		if (componentsInChildren.Length == 0)
		{
			return null;
		}
		if (nameContains == null)
		{
			return null;
		}
		foreach (AnimationState item in componentsInChildren[0])
		{
			AnimationState val = item;
			if (val.name.ContainsIgnoreCase(nameContains))
			{
				return val.name;
			}
		}
		return null;
	}

	public static int GetSize<T>(ICollection<T> collection)
	{
		return collection?.Count ?? 0;
	}

	public static float GetBoundRadius(int entityTypeId)
	{
		if (1000 <= entityTypeId && entityTypeId <= 2000)
		{
			return Singleton<PlayerEntityContainer>.Instance.player.bound_radius;
		}
		if (2000 <= entityTypeId && entityTypeId <= 3000 && SingletonDict<int, Animal>.Instance.TryGetValue(entityTypeId, out var value))
		{
			return value.bound_radius;
		}
		if (6000 <= entityTypeId && entityTypeId <= 10000)
		{
			Building_.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().BlueprintContainer.GetBlueprint(entityTypeId);
			if (blueprint != null)
			{
				return blueprint.BoundRadius;
			}
		}
		return 1f;
	}
}
