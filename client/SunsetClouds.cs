using System.Collections.Generic;
using UnityEngine;

public class SunsetClouds : MonoBehaviour
{
	[SerializeField]
	private UIWidget[] _cloudBaseArray;

	[SerializeField]
	private float _cloudHorzMinSpace = 64f;

	[SerializeField]
	private float _cloudHorzMaxSpace = 128f;

	[SerializeField]
	private float _cloudMoveSpeedPerSec = 10f;

	private List<UIWidget> _clouds = new List<UIWidget>();

	private int _activeCloudCount;

	private UIWidget _widget;

	private void Start()
	{
		UIWidget[] cloudBaseArray = _cloudBaseArray;
		foreach (UIWidget uIWidget in cloudBaseArray)
		{
			uIWidget.gameObject.SetActive(value: false);
		}
		_widget = GetComponent<UIWidget>();
	}

	private void Update()
	{
		for (int i = 0; i < _activeCloudCount; i++)
		{
			UIWidget uIWidget = _clouds[i];
			Vector3 localPosition = uIWidget.transform.localPosition;
			localPosition.x += Time.deltaTime * _cloudMoveSpeedPerSec;
			float num = uIWidget.width;
			float num2 = _widget.width - uIWidget.width;
			if (localPosition.x >= num2)
			{
				localPosition.x = 0f;
			}
			if (localPosition.x < num)
			{
				uIWidget.alpha = Mathf.Clamp01(localPosition.x / (float)uIWidget.width);
			}
			else if (num2 - (float)uIWidget.width < localPosition.x)
			{
				uIWidget.alpha = Mathf.Clamp01((num2 - localPosition.x) / (float)uIWidget.width);
			}
			else
			{
				uIWidget.alpha = 1f;
			}
			uIWidget.transform.localPosition = localPosition;
		}
	}

	public void ArrangeRandomClouds()
	{
		DeactiveAllClouds();
		for (float num = 0f; (int)num < _widget.width; num += Random.Range(_cloudHorzMinSpace, _cloudHorzMaxSpace))
		{
			UIWidget activeCloud = GetActiveCloud();
			float num2 = Random.Range(0f, _widget.height - activeCloud.height);
			activeCloud.transform.localPosition = new Vector3(num, 0f - num2, 0f);
		}
	}

	public void DeactiveAllClouds()
	{
		foreach (UIWidget cloud in _clouds)
		{
			cloud.gameObject.SetActive(value: false);
		}
		_activeCloudCount = 0;
	}

	private UIWidget GetActiveCloud()
	{
		UIWidget uIWidget;
		if (_clouds.Count > _activeCloudCount)
		{
			uIWidget = _clouds[_activeCloudCount];
		}
		else
		{
			int num = Random.Range(0, _cloudBaseArray.Length);
			UIWidget uIWidget2 = _cloudBaseArray[num];
			uIWidget = Object.Instantiate(uIWidget2.gameObject, base.transform).GetComponent<UIWidget>();
			_clouds.Add(uIWidget);
		}
		_activeCloudCount++;
		uIWidget.gameObject.SetActive(value: true);
		return uIWidget;
	}
}
