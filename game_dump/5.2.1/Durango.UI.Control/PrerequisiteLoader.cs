using System;
using System.Text;
using L10N;
using UnityEngine;

namespace Durango.UI.Control;

public class PrerequisiteLoader : MonoBehaviour
{
	[SerializeField]
	private UISlider _loadingSlider;

	[SerializeField]
	private UILabel _loadingProgress;

	private float _lastFileProgress;

	private string _lastFileName;

	private int _lastCount;

	public int TotalCount { get; set; }

	public void DetailedProgressChanged(float progress)
	{
		_lastFileProgress = progress;
		UpdateLoadingProgress();
	}

	public void ProgressChanged(int count, int retryCount, string fileName)
	{
		_lastFileProgress = 0f;
		int num = fileName.LastIndexOf("$", StringComparison.Ordinal);
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
		_lastFileName = fileName;
		_lastCount = count;
		_loadingSlider.value = ((TotalCount <= 0) ? 1f : ((float)_lastCount / (float)TotalCount));
		UpdateLoadingProgress();
	}

	private void UpdateLoadingProgress()
	{
		string text = T._("다운로드 중: {0} / {1}", _lastCount, TotalCount);
		if (Debug.isDebugBuild)
		{
			text += $" ({_lastFileName} {_lastFileProgress * 100f:0.0}%)";
		}
		_loadingProgress.text = text;
	}
}
