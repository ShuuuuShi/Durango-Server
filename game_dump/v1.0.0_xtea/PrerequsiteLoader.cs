using System.Text;
using L10N;
using UnityEngine;

public class PrerequsiteLoader : MonoBehaviour
{
	[SerializeField]
	private UISlider _loadingSlider;

	[SerializeField]
	private UILabel _loadingFileName;

	[SerializeField]
	private UILabel _loadingProgress;

	private float _lastFileProgress;

	private string _lastFileName;

	public int TotalCount { get; set; }

	public void DetailedProgressChanged(float progress)
	{
		_lastFileProgress = progress;
		UpdateLastFileProgressText();
	}

	public void ProgressChanged(int count, int retryCount, string fileName)
	{
		_lastFileProgress = 0f;
		int num = fileName.LastIndexOf("$");
		if (num >= 0)
		{
			fileName = fileName.Substring(num + 1, fileName.Length - num - 1);
		}
		if (retryCount > 0)
		{
			StringBuilder stringBuilder = new StringBuilder(fileName);
			stringBuilder.Append('.', retryCount % 5 + 1);
			fileName = stringBuilder.ToString();
		}
		string text = T._("다운로드 중: ");
		_lastFileName = text + fileName;
		_loadingProgress.text = $"{count}/{TotalCount}";
		_loadingSlider.value = (float)count / (float)TotalCount;
		UpdateLastFileProgressText();
	}

	private void UpdateLastFileProgressText()
	{
		_loadingFileName.text = $"{_lastFileName} ({_lastFileProgress * 100f:0.0}%)";
	}
}
