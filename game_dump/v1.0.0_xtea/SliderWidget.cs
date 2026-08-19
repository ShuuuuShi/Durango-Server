using UnityEngine;

public class SliderWidget : MonoBehaviour
{
	[SerializeField]
	public UISprite Bg;

	[SerializeField]
	public UISprite Upper;

	[SerializeField]
	public UIWidget Main;

	[SerializeField]
	public UILabel MinText;

	[SerializeField]
	public UILabel MaxText;

	[SerializeField]
	public UIWidget Circle;

	public OptionItem Parent { get; set; }

	public float Max { get; set; }

	public float Min { get; set; }

	public float Threshold { get; set; }

	public string StringConverter { get; set; }

	public float ModifyRatio { get; set; }
}
