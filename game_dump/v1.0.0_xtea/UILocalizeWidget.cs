using System;
using System.Collections.Generic;
using UnityEngine;

public class UILocalizeWidget : MonoBehaviour
{
	[Serializable]
	public class LocalizeData
	{
		public string Locale;

		public string Sprite;

		public Texture Texture;

		public Material Material;

		public int Width;

		public int Height;

		public bool Disabled;
	}

	[SerializeField]
	public List<LocalizeData> DataList = new List<LocalizeData>();

	private void OnEnable()
	{
		OnLocalize();
	}

	private void OnLocalize()
	{
		if (LocalizeSystem.Locale == "ko_KR")
		{
			return;
		}
		for (int i = 1; i < DataList.Count; i++)
		{
			LocalizeData localizeData = DataList[i];
			if (localizeData.Locale.ContainsIgnoreCase(LocalizeSystem.Locale))
			{
				Apply(localizeData);
				return;
			}
		}
		if (DataList.Count > 0)
		{
			Apply(DataList[0]);
		}
	}

	public void Apply(LocalizeData data)
	{
		UIWidget uIWidget;
		if (!string.IsNullOrEmpty(data.Sprite))
		{
			UISprite component = ((Component)this).GetComponent<UISprite>();
			if ((Object)(object)component == (Object)null)
			{
				return;
			}
			component.spriteName = data.Sprite;
			uIWidget = component;
		}
		else if ((Object)(object)data.Texture != (Object)null || (Object)(object)data.Material != (Object)null)
		{
			UITexture component2 = ((Component)this).GetComponent<UITexture>();
			if ((Object)(object)component2 == (Object)null)
			{
				return;
			}
			component2.material = data.Material;
			if ((Object)(object)data.Material != (Object)null)
			{
				component2.mainTexture = data.Texture;
			}
			uIWidget = component2;
		}
		else
		{
			uIWidget = ((Component)this).GetComponent<UIWidget>();
		}
		if (!((Object)(object)uIWidget == (Object)null))
		{
			if (data.Width != 0)
			{
				uIWidget.width = data.Width;
			}
			if (data.Height != 0)
			{
				uIWidget.height = data.Height;
			}
			((Component)this).gameObject.SetActive(!data.Disabled);
		}
	}
}
