using UnityEngine;

[AddComponentMenu("NGUI/Examples/Slider Colors")]
public class UISliderColors : MonoBehaviour
{
	public UISprite sprite;

	public Color[] colors = (Color[])(object)new Color[3]
	{
		Color.red,
		Color.yellow,
		Color.green
	};

	private UIProgressBar mBar;

	private UIBasicSprite mSprite;

	private void Start()
	{
		mBar = ((Component)this).GetComponent<UIProgressBar>();
		mSprite = ((Component)this).GetComponent<UIBasicSprite>();
		Update();
	}

	private void Update()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)sprite == (Object)null || colors.Length == 0)
		{
			return;
		}
		float num = ((!((Object)(object)mBar != (Object)null)) ? mSprite.fillAmount : mBar.value);
		num *= (float)(colors.Length - 1);
		int num2 = Mathf.FloorToInt(num);
		Color color = colors[0];
		if (num2 >= 0)
		{
			if (num2 + 1 >= colors.Length)
			{
				color = ((num2 >= colors.Length) ? colors[colors.Length - 1] : colors[num2]);
			}
			else
			{
				float num3 = num - (float)num2;
				color = Color.Lerp(colors[num2], colors[num2 + 1], num3);
			}
		}
		color.a = sprite.color.a;
		sprite.color = color;
	}
}
