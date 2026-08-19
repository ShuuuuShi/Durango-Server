using JetBrains.Annotations;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class StoryViewNode : MonoBehaviour
{
	[EnumList(typeof(Chapter.Kind), false, 0, -1)]
	[SerializeField]
	private StoryViewPage[] _pages;

	public Chapter.Kind Kind { get; private set; }

	public Chapter Chapter { get; private set; }

	public void Set([NotNull] Chapter chapter, bool locked)
	{
		Chapter.Kind kind2 = (Kind = ((!locked) ? chapter.GetKind() : Chapter.Kind.Locked));
		Chapter.Kind kind3 = kind2;
		Chapter = chapter;
		for (int i = 0; i < _pages.Length; i++)
		{
			StoryViewPage storyViewPage = _pages[i];
			bool flag = i == (int)kind3;
			storyViewPage.gameObject.SetActive(flag);
			if (flag)
			{
				storyViewPage.Set(chapter, locked);
			}
		}
	}
}
