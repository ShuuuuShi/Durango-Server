using UnityEngine;

public class OnBlurMasking : MonoBehaviour
{
	[SerializeField]
	private UIWidget[] _toggleObjects;

	[SerializeField]
	private TweenerPlayer _blurTweener;

	private void Start()
	{
		OnBlur(enable: false);
	}

	public void OnBlur(bool enable)
	{
		int i = 0;
		for (int num = ((_toggleObjects != null) ? _toggleObjects.Length : 0); i < num; i++)
		{
			_toggleObjects[i].alpha = ((!enable) ? 0f : 1f);
		}
		if (!((Object)(object)_blurTweener == (Object)null))
		{
			if (enable)
			{
				_blurTweener.Play();
			}
			else
			{
				_blurTweener.Stop();
			}
		}
	}
}
