using UnityEngine;

namespace Durango.UI.Control;

public class LevelGaugeController : MonoBehaviour
{
	[SerializeField]
	private UISprite _checkBorder;

	[SerializeField]
	private UISprite _checkInner;

	[SerializeField]
	private UISprite _gaugeBg;

	[SerializeField]
	private UISprite _gaugeUpper;

	[SerializeField]
	private TweenerPlayer _acceptableEffect;

	public void SetAccepted()
	{
		_checkInner.gameObject.SetActive(value: true);
		_checkBorder.gameObject.SetActive(value: true);
		_acceptableEffect.gameObject.SetActive(value: false);
		_gaugeUpper.alpha = 1f;
	}

	public void SetFirstAcceptable()
	{
		_acceptableEffect.gameObject.SetActive(value: true);
		_acceptableEffect.Play();
		_checkBorder.gameObject.SetActive(value: true);
		_checkInner.gameObject.SetActive(value: true);
		_gaugeUpper.alpha = 1f;
	}

	public void SetAcceptable()
	{
		_acceptableEffect.gameObject.SetActive(value: false);
		_checkBorder.gameObject.SetActive(value: true);
		_checkInner.gameObject.SetActive(value: false);
		_gaugeUpper.alpha = 1f;
	}

	public void SetNonAcceptable()
	{
		_acceptableEffect.gameObject.SetActive(value: false);
		_checkBorder.gameObject.SetActive(value: false);
		_checkInner.gameObject.SetActive(value: false);
		_gaugeUpper.alpha = 0f;
	}

	public void SetGaugeHeight(float gaugeHeight)
	{
		if (gaugeHeight > 0f)
		{
			_gaugeBg.transform.localPosition = new Vector3(0f, gaugeHeight + 10f);
			_gaugeUpper.transform.localPosition = new Vector3(0f, gaugeHeight + 10f);
			int height = (int)gaugeHeight;
			_gaugeBg.height = height;
			_gaugeUpper.height = height;
		}
		else
		{
			_gaugeBg.gameObject.SetActive(value: false);
			_gaugeUpper.gameObject.SetActive(value: false);
		}
	}
}
