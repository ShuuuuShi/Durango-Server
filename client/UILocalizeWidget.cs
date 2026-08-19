using System;
using System.Collections.Generic;
using Durango.System;
using Durango.Utils.Extensions;
using UnityEngine;

public class UILocalizeWidget : MonoBehaviour
{
	[Serializable]
	public class LocalizeData
	{
		public string Locale;

		public NPCountry Country;

		public string Sprite;

		public Texture Texture;

		public Material Material;

		public int Width;

		public int Height;

		public bool Disabled;
	}

	public const string DefaultLocale = "ko_KR";

	public const NPCountry DefaultCountry = NPCountry.Korea;

	[SerializeField]
	public List<LocalizeData> DataList = new List<LocalizeData>();

	[SerializeField]
	public bool UseCountry;

	private void OnEnable()
	{
		OnLocalize();
	}

	private void OnLocalize()
	{
		string locale = LocalizeSystem.Locale;
		NPCountry country = Platform.Instance.Country;
		if ((UseCountry && country == NPCountry.Korea) || (!UseCountry && locale == "ko_KR"))
		{
			return;
		}
		for (int i = 1; i < DataList.Count; i++)
		{
			LocalizeData localizeData = DataList[i];
			if (UseCountry && localizeData.Country == country)
			{
				Apply(localizeData);
				return;
			}
			if (!UseCountry && localizeData.Locale.ContainsIgnoreCase(locale))
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
			UISprite component = GetComponent<UISprite>();
			if (component == null)
			{
				return;
			}
			component.spriteName = data.Sprite;
			uIWidget = component;
		}
		else if (data.Texture != null || data.Material != null)
		{
			UITexture component2 = GetComponent<UITexture>();
			if (component2 == null)
			{
				return;
			}
			if (data.Material == null)
			{
				component2.mainTexture = data.Texture;
			}
			else
			{
				component2.material = data.Material;
				component2.mainTexture = data.Texture;
			}
			uIWidget = component2;
		}
		else
		{
			uIWidget = GetComponent<UIWidget>();
		}
		if (uIWidget != null)
		{
			if (data.Width != 0)
			{
				uIWidget.width = data.Width;
			}
			if (data.Height != 0)
			{
				uIWidget.height = data.Height;
			}
		}
		base.gameObject.SetActive(!data.Disabled);
	}
}
