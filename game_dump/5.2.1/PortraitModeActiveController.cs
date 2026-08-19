using UnityEngine;

public class PortraitModeActiveController : MonoBehaviour
{
	[SerializeField]
	private GameObject[] _onlyLandscape;

	[SerializeField]
	private GameObject[] _onlyPortrait;

	private bool? _isPortrait;

	private void Awake()
	{
		UIManager.AddOnScreenResized(OnScreenResize);
	}

	private void OnScreenResize()
	{
		UpdateActiveState();
	}

	private void UpdateActiveState()
	{
		bool isPortraitScreen = UIManager.IsPortraitScreen;
		if (!_isPortrait.HasValue || _isPortrait.Value != isPortraitScreen)
		{
			_isPortrait = isPortraitScreen;
			for (int i = 0; i < _onlyLandscape.Length; i++)
			{
				_onlyLandscape[i].SetActive(!isPortraitScreen);
			}
			for (int j = 0; j < _onlyPortrait.Length; j++)
			{
				_onlyPortrait[j].SetActive(isPortraitScreen);
			}
		}
	}
}
