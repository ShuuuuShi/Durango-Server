using System.Collections.Generic;
using System.Linq;
using Messages;

public static class MusicsExtension
{
	public static int GetTotalMusicCount(this Musics msg)
	{
		return KUtility.GetSize(msg._Musics) + KUtility.GetSize(msg.SharedMusics);
	}

	public static IEnumerable<KeyValuePair<MusicId, Music>> GetAllMusics(this Musics msg)
	{
		return msg.GetSharedMusics().Concat(msg.GetMyMusics());
	}

	public static IEnumerable<KeyValuePair<MusicId, Music>> GetMyMusics(this Musics msg)
	{
		if (msg._Musics == null)
		{
			yield break;
		}
		foreach (KeyValuePair<int, Music> i in msg._Musics)
		{
			yield return new KeyValuePair<MusicId, Music>(i.Key, i.Value);
		}
	}

	public static IEnumerable<KeyValuePair<MusicId, Music>> GetSharedMusics(this Musics msg)
	{
		if (msg.SharedMusics == null)
		{
			yield break;
		}
		foreach (KeyValuePair<string, Music> i in msg.SharedMusics)
		{
			yield return new KeyValuePair<MusicId, Music>(i.Key, i.Value);
		}
	}
}
