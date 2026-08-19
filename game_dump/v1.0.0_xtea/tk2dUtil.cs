using UnityEngine;

public static class tk2dUtil
{
	private static string label = string.Empty;

	private static bool undoEnabled;

	public static bool UndoEnabled
	{
		get
		{
			return undoEnabled;
		}
		set
		{
			undoEnabled = value;
		}
	}

	public static void BeginGroup(string name)
	{
		undoEnabled = true;
		label = name;
	}

	public static void EndGroup()
	{
		label = string.Empty;
	}

	public static void DestroyImmediate(Object obj)
	{
		if (!(obj == (Object)null))
		{
			Object.DestroyImmediate(obj);
		}
	}

	public static GameObject CreateGameObject(string name)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		return new GameObject(name);
	}

	public static Mesh CreateMesh()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		Mesh val = new Mesh();
		val.MarkDynamic();
		return val;
	}

	public static T AddComponent<T>(GameObject go) where T : Component
	{
		return go.AddComponent<T>();
	}

	public static void SetActive(GameObject go, bool active)
	{
		if (active != go.activeSelf)
		{
			go.SetActive(active);
		}
	}

	public static void SetTransformParent(Transform t, Transform parent)
	{
		t.parent = parent;
	}
}
