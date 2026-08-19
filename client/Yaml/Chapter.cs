using System;
using System.Collections.Generic;
using System.Linq;
using Durango.UI;
using Newtonsoft.Json;

namespace Yaml;

public class Chapter
{
	public enum Kind
	{
		Normal,
		Text,
		Image,
		Movie,
		Locked
	}

	[JsonProperty(PropertyName = "chapter")]
	public int ChapterNum;

	[JsonProperty(PropertyName = "title")]
	public Gettext Title;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "image")]
	public string Image;

	[JsonProperty(PropertyName = "movie")]
	public Dictionary<string, string> Movie;

	[JsonProperty(PropertyName = "quests")]
	public string[] Quests;

	public Kind GetKind()
	{
		if (KUtility.GetSize(Movie) > 0)
		{
			return Kind.Movie;
		}
		if (string.IsNullOrEmpty(Description))
		{
			return Kind.Image;
		}
		if (string.IsNullOrEmpty(Image))
		{
			return Kind.Text;
		}
		return Kind.Normal;
	}

	public void PlayMovie(Action onFinished = null)
	{
		if (KUtility.GetSize(Movie) == 0)
		{
			return;
		}
		string text = Movie.Get(LocalizeSystem.Locale);
		if (string.IsNullOrEmpty(text))
		{
			text = Movie.Get("en_US");
			if (string.IsNullOrEmpty(text))
			{
				text = Movie.First().Value;
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			FullScreenMovieGroupBase.Play(text, once: false, onFinished);
		}
	}

	public void PlayMovie(int chapterNum, Action onFinished = null)
	{
		if (LocalizeSystem.LocaleLanguage == "ko")
		{
			switch (chapterNum)
			{
			case 7:
				FullScreenMovieGroupBase.Play("http://kyllox.pe.kr/movies/standard/the_final_battle.mp4", once: false, onFinished);
				break;
			case 8:
				FullScreenMovieGroupBase.Play("http://kyllox.pe.kr/movies/ko/last_transmission.mp4", once: false, onFinished);
				break;
			default:
				UIManager.SystemMsg("파일이 존재하지 않습니다.");
				break;
			}
		}
		else
		{
			switch (chapterNum)
			{
			case 7:
				FullScreenMovieGroupBase.Play("http://kyllox.pe.kr/movies/standard/the_final_battle.mp4", once: false, onFinished);
				break;
			case 8:
				FullScreenMovieGroupBase.Play("http://kyllox.pe.kr/movies/en/last_transmission.mp4", once: false, onFinished);
				break;
			default:
				UIManager.SystemMsg("The file is not Exist.");
				break;
			}
		}
	}
}
