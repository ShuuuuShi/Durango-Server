using UnityEngine;

public class PortraitModeActiveController : MonoBehaviour
{
	[SerializeField]
	private GameObject[] _onlyLandscape;

	[SerializeField]
	private GameObject[] _onlyPortrait;

	private bool _isInit;

	private bool _isPortrait;

	private void OnEnable()
	{
		UpdateActiveState();
	}

	private void OnPortraitMode(bool isPortrait)
	{
		UpdateActiveState();
	}

	private void UpdateActiveState()
	{
		bool isPortraitMode = UIManager.IsPortraitMode;
		if (!_isInit || isPortraitMode != _isPortrait)
		{
			_isInit = true;
			_isPortrait = isPortraitMode;
			for (int i = 0; i < _onlyLandscape.Length; i++)
			{
				_onlyLandscape[i].SetActive(!_isPortrait);
			}
			for (int j = 0; j < _onlyPortrait.Length; j++)
			{
				_onlyPortrait[j].SetActive(_isPortrait);
			}
		}
	}
}
