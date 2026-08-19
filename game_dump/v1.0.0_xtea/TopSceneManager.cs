using UnityEngine;
using UnityEngine.UI;

public class TopSceneManager : MonoBehaviour
{
	public GameObject webview;

	public Text countDownText;

	private int countDown = 10;

	private void Start()
	{
		((MonoBehaviour)this).InvokeRepeating("Show", 1f, 1f);
	}

	private void Show()
	{
		countDown--;
		countDownText.text = "Show web view in " + countDown + "s";
		if (countDown == 0)
		{
			webview.SetActive(true);
			((MonoBehaviour)this).CancelInvoke();
		}
	}
}
