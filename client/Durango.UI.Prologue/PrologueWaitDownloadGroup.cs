using Durango.Logic.Encyclopedia;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueWaitDownloadGroup : MonoBehaviour
{
	[SerializeField]
	public PrerequisiteLoader PrerequsiteLoader;

	[SerializeField]
	private GameObject _contents;

	[SerializeField]
	private UILabel _contentLabel;

	[SerializeField]
	private UILabel _downloadWarning;

	[SerializeField]
	private float _term;

	private float _timer;

	private void Awake()
	{
		_contents.SetActive(value: false);
	}

	public void Show()
	{
		GetComponent<UIPanel>().alpha = 1f;
		_contents.SetActive(value: true);
		_timer = _term;
		ChangeContent();
		_downloadWarning.text = string.Empty;
	}

	public void SetDonwloadWarning(string text)
	{
		_downloadWarning.text = text;
	}

	private void Update()
	{
		if (_contents.activeInHierarchy)
		{
			if (_timer > 0f)
			{
				_timer -= Time.deltaTime;
				return;
			}
			ChangeContent();
			_timer = _term;
		}
	}

	private void ChangeContent()
	{
		int randomMemo = MemoSystem.GetRandomMemo(MemoType.Fiction, save: false);
		_contentLabel.text = ((randomMemo != -1) ? MemoSystem.GetMemoFullText(MemoType.Fiction, randomMemo) : string.Empty);
	}
}
