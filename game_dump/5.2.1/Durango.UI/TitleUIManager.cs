using UnityEngine;

namespace Durango.UI;

public class TitleUIManager : MonoBehaviour
{
	private static TitleUIManager _instance;

	private LinkedPrefabs _linkedPrefabs;

	private void Awake()
	{
		_instance = this;
		_linkedPrefabs = new LinkedPrefabs(base.gameObject, ResourceSingleton<UIPrefabMap>.Instance().GetTitle());
		_linkedPrefabs.Load(delegate(GameObject obj)
		{
			obj.SetActive(value: false);
		}, null);
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	private void Start()
	{
		_linkedPrefabs.FindScript<TitleMenuGroup>().StartGame();
	}

	public static T Find<T>() where T : Component
	{
		return _instance._linkedPrefabs.FindScript<T>();
	}
}
