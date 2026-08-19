using System.Collections.Generic;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class AlarmNewsWidget : MonoBehaviour
{
	private class NewsData
	{
		public string Id;

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

	public AnimationWidget AnimWidget
	{
		get
		{
			if (_animWidget == null)
			{
				return _animWidget = GetComponent<AnimationWidget>();
			}
			return _animWidget;
		}
	}

	private void Start()
	{
		if (!_isShow)
		{
			AnimWidget.SetAlpha(0f, useTween: false);
		}
		base.enabled = _newsList.Count > 0;
	}

	[ExposedInEditor(null)]
	public void Register(string id, string text, float since, float until, float period)
	{
		int num = IndexOf(id);
		text = text.Replace('\n', ' ');
		if (num == -1)
		{
			NewsData item = new NewsData
			{
				Id = id,
				Text = text,
				Until = until,
				Period = period,
				NextShowAt = since
			};
			_newsList.Add(item);
		}
		else
		{
			NewsData newsData = _newsList[num];
			newsData.Text = text;
			newsData.Until = until;
			newsData.Period = period;
			newsData.NextShowAt = since;
		}
		if (!_isShow)
		{
			ShowNextNews();
		}
	}

	public void Remove(string id)
	{
		int num = IndexOf(id);
		if (num != -1)
		{
			_newsList.RemoveAt(num);
		}
	}

	private void Show(NewsData news)
	{
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
			if (_newsList[i].Until < time)
			{
				_newsList.RemoveAt(i);
				i--;
			}
		}
		int count = _newsList.Count;
		int num = IndexOf((_currentNews != null) ? _currentNews.Id : string.Empty);
		NewsData newsData = null;
		_nextUpdateAt = 0f;
		for (int j = 0; j < count; j++)
		{
			int index = (num + j + 1) % count;
			NewsData newsData2 = _newsList[index];
			if (newsData2.NextShowAt < time)
			{
				_nextUpdateAt = 0f;
				newsData = newsData2;
				break;
			}
			_nextUpdateAt = ((!(_nextUpdateAt > 0f)) ? newsData2.NextShowAt : Mathf.Min(_nextUpdateAt, newsData2.NextShowAt));
		}
		if (newsData == null)
		{
			Hide();
		}
		else
		{
			Show(newsData);
		}
		base.enabled = count > 0;
	}

	private int IndexOf(string id)
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
		Vector3 localPosition = _newsLabel.transform.localPosition;
		localPosition.x -= Time.deltaTime * _scrollSpeed;
		_newsLabel.transform.localPosition = localPosition;
		Vector3 vector = AnimWidget.Widget.localCorners[0];
		if (_newsLabel.GetPosition(1f, 0f).x < vector.x)
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
