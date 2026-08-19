using System;
using Durango.Network;
using Durango.Utils;
using Durango.Utils.Extensions;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class PromotionBannerWidget : UIWidget
{
	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UILabel _title;

	[SerializeField]
	private UILabel _desc;

	[SerializeField]
	private UILabel _hudText;

	[SerializeField]
	private UITexture _texture;

	private PromotionLink _data;

	public event Action<bool> Pressed;

	public void Set(PromotionLink data)
	{
		_data = data;
		if (_title != null)
		{
			_title.text = $"<em>{data.MainText}</em>";
		}
		if (_desc != null)
		{
			_desc.text = data.SubText;
		}
		if (_hudText != null)
		{
			_hudText.text = data.HudText;
		}
		if (_background != null)
		{
			_background.color = data.BackgroundColor.ToColor();
		}
		SetTexture(_texture, data.Image);
	}

	private void OnClick()
	{
		if (_data != null)
		{
			if (!string.IsNullOrEmpty(_data.WebLink))
			{
				UIUtility.OpenUri(_data.MainText, _data.WebLink);
			}
			else if (!string.IsNullOrEmpty(_data.CommodityId))
			{
				Singleton<UIManager>.Instance().OpenUri($"Shop/Commodity/{_data.CommodityId}");
			}
		}
	}

	private void OnPress(bool press)
	{
		if (this.Pressed != null)
		{
			this.Pressed(press);
		}
	}

	private void SetTexture(UITexture texture, string imageName)
	{
		if (texture == null)
		{
			return;
		}
		string path = $"UI/Event/{imageName}.mat";
		Singleton<AssetBundleManager>.Instance().RequestAsset(path, typeof(Material), delegate(UnityEngine.Object asset)
		{
			if (!(asset == null))
			{
				texture.material = asset as Material;
			}
		});
	}

	public static bool IsShowPeriod(PromotionLink info)
	{
		double num = Times.ParseDateTimeToUnixTime(info.StartAt, double.MinValue);
		double num2 = Times.ParseDateTimeToUnixTime(info.EndAt, double.MaxValue);
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		return num < predictedServerTime && num2 > predictedServerTime;
	}
}
