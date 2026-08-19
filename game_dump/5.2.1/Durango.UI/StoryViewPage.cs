using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class StoryViewPage : MonoBehaviour
{
	[CanBeNull]
	[SerializeField]
	private UILabel _chapter;

	[CanBeNull]
	[SerializeField]
	private UILabel _title;

	[CanBeNull]
	[SerializeField]
	private UILabel _description;

	[CanBeNull]
	[SerializeField]
	private UITexture _image;

	[CanBeNull]
	[SerializeField]
	private GameObject _play;

	public void Set([NotNull] Chapter chapter, bool locked)
	{
		if (_chapter != null)
		{
			_chapter.text = T._("[icon=icon_chapter_wave] 챕터 {0} [icon=icon_chapter_wave]", chapter.ChapterNum);
		}
		if (_title != null)
		{
			if (locked)
			{
				_title.text = T._("이전 챕터를 완료하면 개방 됩니다.");
			}
			else
			{
				_title.text = chapter.Title;
			}
		}
		if (_description != null)
		{
			_description.text = chapter.Description;
		}
		if (_image != null && !string.IsNullOrEmpty(chapter.Image))
		{
			string assetPath = "UI/DialogueImage/" + chapter.Image + ".mat";
			Singleton<AssetBundleManager>.Instance().RequestAsset(assetPath, typeof(Material), delegate(Object asset)
			{
				if (!(asset == null))
				{
					_image.material = asset as Material;
				}
			});
		}
		if (_play != null && chapter.GetKind() == Chapter.Kind.Movie)
		{
			UIEventListener.Get(_play).onClick = delegate
			{
				chapter.PlayMovie(chapter.ChapterNum);
			};
		}
	}
}
