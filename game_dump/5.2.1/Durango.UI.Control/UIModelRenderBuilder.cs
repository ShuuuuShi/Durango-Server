using Durango.Utils;
using UnityEngine;

namespace Durango.UI.Control;

public class UIModelRenderBuilder : Singleton<UIModelRenderBuilder>
{
	[SerializeField]
	private UIModelRender _baseComponent;

	private ListObjectPool<UIModelRender> _list;

	protected override void OnAwake()
	{
		_list = new ListObjectPool<UIModelRender>();
		_list.BaseObject = _baseComponent;
		_list.Clear();
	}

	public static UIModelRender Make()
	{
		if (!Singleton<UIModelRenderBuilder>.HasInstance())
		{
			return null;
		}
		ListObjectPool<UIModelRender> list = Singleton<UIModelRenderBuilder>.Instance()._list;
		int num = -1;
		for (int i = 0; i < list.Count; i++)
		{
			if (!list[i].gameObject.activeSelf && num == -1)
			{
				num = i;
				break;
			}
		}
		UIModelRender uIModelRender;
		if (num == -1)
		{
			num = list.Count;
			uIModelRender = list.Add();
		}
		else
		{
			uIModelRender = list[num];
		}
		uIModelRender.gameObject.SetActive(value: true);
		uIModelRender.transform.localPosition = (float)(num + 1) * 1000f * Vector3.right;
		return uIModelRender;
	}

	public static void Release(UIModelRender renderer)
	{
		if (!(renderer == null))
		{
			renderer.gameObject.SetActive(value: false);
		}
	}
}
