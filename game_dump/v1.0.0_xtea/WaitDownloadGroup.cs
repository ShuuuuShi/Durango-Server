using EncyclopediaData;
using UnityEngine;

public class WaitDownloadGroup : MonoBehaviour
{
	[SerializeField]
	private UILabel _contentLabel;

	[SerializeField]
	private float _term;

	private float _timer;

	private void OnEnable()
	{
		_timer = _term;
		ChangeContent();
	}

	private void Update()
	{
		if (_timer > 0f)
		{
			_timer -= Time.deltaTime;
			return;
		}
		ChangeContent();
		_timer = _term;
	}

	private void ChangeContent()
	{
		int num = EncyclopediaSystem.RandomMemoGet(MemoType.Fiction, save: false);
		_contentLabel.text = ((num != -1) ? EncyclopediaSystem.GetMemoFullText(MemoType.Fiction, num) : string.Empty);
	}
}
