using Durango.UI.Control;
using UnityEngine;

public class OnBlurMasking : MonoBehaviour
{
	[SerializeField]
	private TweenerPlayer _blurTweener;

	private bool _isInit;

	private void Start()
	{
		if (!_isInit)
		{
			OnBlur(enable: false);
		}
	}

	public void OnBlur(bool enable)
	{
		_isInit = true;
		if (!(_blurTweener == null))
		{
			if (enable)
			{
				_blurTweener.gameObject.SetActive(value: true);
				_blurTweener.Play();
			}
			else
			{
				_blurTweener.gameObject.SetActive(value: false);
			}
		}
	}
}
