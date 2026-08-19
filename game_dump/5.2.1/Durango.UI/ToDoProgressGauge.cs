using UnityEngine;

namespace Durango.UI;

public class ToDoProgressGauge : MonoBehaviour
{
	[SerializeField]
	private UISprite _bar;

	[SerializeField]
	private UILabel _label;

	public void Set(int currentProgress, int targetProgress)
	{
		_label.text = $"<em>{currentProgress}</em> / {targetProgress}";
		_bar.fillAmount = (float)currentProgress / (float)targetProgress;
	}
}
