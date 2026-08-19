using System.Collections.Generic;
using UnityEngine;

public class NewsAlarmWidget : MonoBehaviour
{
	private class NewsData
	{
		public ulong Id;

		public string Text;

		public float Until;

		public float Period;

		public float NextShowAt;
	}

	[SerializeField]
	private UILabel _newsLabel;

	[SerializeField]
	private float _scrollSpeed;

	private AnimationWidget _animWidget;

	private bool _isShow;

	private NewsData _currentNews;

	private float _nextUpdateAt;

	private readonly List<NewsData> _newsList = new List<NewsData>();

	public AnimationWidget AnimWidget => (!((Object)(object)_animWidget == (Object)null)) ? _animWidget : (_animWidget = ((Component)this).GetComponent<AnimationWidget>());

	private void Start()
	{
		if (!_isShow)
		{
			AnimWidget.SetAlpha(0f, useTween: false);
		}
		((Behaviour)this).enabled = _newsList.Count > 0;
	}

	[ExposedInEditor(null)]
	public void Register(ulong id, string text, float since, float until, float period)
	{
		int num = IndexOf(id);
		text = text.Replace('\n', ' ');
		if (num == -1)
		{
			NewsData newsData = new NewsData();
			newsData.Id = id;
			newsData.Text = text;
			newsData.Until = until;
			newsData.Period = period;
			newsData.NextShowAt = since;
			NewsData item = newsData;
			_newsList.Add(item);
		}
		else
		{
			NewsData newsData2 = _newsList[num];
			newsData2.Text = text;
			newsData2.Until = until;
			newsData2.Period = period;
			newsData2.NextShowAt = since;
		}
		if (!_isShow)
		{
			ShowNextNews();
		}
	}

	public void Remove(ulong id)
	{
		int num = IndexOf(id);
		if (num != -1)
		{
			_newsList.RemoveAt(num);
		}
	}

	private void Show(NewsData news)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		_isShow = true;
		_currentNews = news;
		_newsLabel.text = news.Text;
		Vector3 pos = Vector3.Lerp(AnimWidget.Widget.localCorners[2], AnimWidget.Widget.localCorners[3], 0.5f);
		_newsLabel.SetPosition(pos, 0f, 0.5f);
		AnimWidget.Alpha = 1f;
	}

	private void Hide()
	{
		if (_isShow)
		{
			_isShow = false;
			AnimWidget.Alpha = 0f;
		}
	}

	private void OnEndNews()
	{
		if (_currentNews != null)
		{
			_currentNews.NextShowAt = Time.time + _currentNews.Period;
		}
		ShowNextNews();
	}

	private void ShowNextNews()
	{
		float time = Time.time;
		for (int i = 0; i < _newsList.Count; i++)
		{
			NewsData newsData = _newsList[i];
			if (newsData.Until < time)
			{
				_newsList.RemoveAt(i);
				i--;
			}
		}
		int count = _newsList.Count;
		int num = IndexOf((_currentNews != null) ? _currentNews.Id : 0);
		NewsData newsData2 = null;
		_nextUpdateAt = 0f;
		for (int j = 0; j < count; j++)
		{
			int index = (num + j + 1) % count;
			NewsData newsData3 = _newsList[index];
			if (newsData3.NextShowAt < time)
			{
				_nextUpdateAt = 0f;
				newsData2 = newsData3;
				break;
			}
			_nextUpdateAt = ((!(_nextUpdateAt > 0f)) ? newsData3.NextShowAt : Mathf.Min(_nextUpdateAt, newsData3.NextShowAt));
		}
		if (newsData2 == null)
		{
			Hide();
		}
		else
		{
			Show(newsData2);
		}
		((Behaviour)this).enabled = count > 0;
	}

	private int IndexOf(ulong id)
	{
		for (int i = 0; i < _newsList.Count; i++)
		{
			if (_newsList[i].Id == id)
			{
				return i;
			}
		}
		return -1;
	}

	private void Update()
	{
		if (_isShow)
		{
			UpdateNewsLabelPosition();
		}
		else
		{
			WaitNextNews();
		}
	}

	private void UpdateNewsLabelPosition()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)_newsLabel).transform.localPosition;
		localPosition.x -= Time.deltaTime * _scrollSpeed;
		((Component)_newsLabel).transform.localPosition = localPosition;
		Vector3 val = AnimWidget.Widget.localCorners[0];
		if (_newsLabel.GetPosition(1f, 0f).x < val.x)
		{
			OnEndNews();
		}
	}

	private void WaitNextNews()
	{
		if (_nextUpdateAt > 0f && _nextUpdateAt < Time.time)
		{
			ShowNextNews();
		}
	}
}
