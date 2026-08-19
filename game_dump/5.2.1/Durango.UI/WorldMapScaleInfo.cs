using UnityEngine;

namespace Durango.UI;

public class WorldMapScaleInfo : MonoBehaviour
{
	private const int DisplayDistanceRepeatCount = 8;

	[SerializeField]
	private UILabel _textDistance;

	[SerializeField]
	private UIWidget _widgetSelf;

	[SerializeField]
	private UIWidget _widgetRuler;

	public void Refresh(float zoomScale, float meterPerPixel)
	{
		float num = ((!(zoomScale > 0f)) ? 0f : ((float)_widgetSelf.width * meterPerPixel / zoomScale));
		if (num > 0f)
		{
			int num2 = 1;
			for (int i = 0; i < 8; i++)
			{
				if ((float)num2 >= num)
				{
					break;
				}
				if (i % 3 == 1)
				{
					num2 *= 5;
					num2 /= 2;
				}
				else
				{
					num2 *= 2;
				}
			}
			SetDistanceText(num2);
			SetRulerLength((float)num2 / num);
		}
		else
		{
			SetDistanceText(0);
			SetRulerLength(0f);
		}
	}

	private void SetRulerLength(float ratio)
	{
		if (ratio > 0f)
		{
			_widgetRuler.gameObject.SetActive(value: true);
			_widgetRuler.width = (int)((float)_widgetSelf.width * ratio);
		}
		else
		{
			_widgetRuler.gameObject.SetActive(value: false);
		}
	}

	private void SetDistanceText(int distance)
	{
		if (distance > 0)
		{
			_textDistance.gameObject.SetActive(value: true);
			_textDistance.text = ((distance / 1000 <= 0) ? $"{distance}m" : $"{distance / 1000}km");
		}
		else
		{
			_textDistance.gameObject.SetActive(value: false);
		}
	}
}
