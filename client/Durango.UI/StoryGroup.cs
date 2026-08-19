using Durango.Logic;
using Durango.Logic.Quest;
using Durango.UI.Popup;
using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

[Uri("Story")]
public class StoryGroup : UIBase
{
	[SerializeField]
	private GameObject _main;

	[SerializeField]
	private StoryMainViewer _viewer;

	[SerializeField]
	private UILabel _noData;

	private void Start()
	{
		base.TryClose();
	}

	protected override bool TryOpen()
	{
		if (!base.TryOpen())
		{
			return false;
		}
		_noData.gameObject.SetActive(value: false);
		_viewer.gameObject.SetActive(value: false);
		string epicCategory = GameSystem<QuestSystem>.Instance().EpicCategory;
		Category cat = GameSystem<QuestSystem>.Instance().GetCategory(GameSystem<QuestSystem>.Instance().EpicCategory);
		if (cat == null)
		{
			SetNoData(T._("퀘스트 목록을 찾을 수 없습니다."));
			return true;
		}
		LoadingRingWidget loadingRing = UIManager.Popup.LoadingRing;
		loadingRing.AttachToWidget(_main);
		cat.GetQuestList(delegate
		{
			UIManager.Popup.LoadingRing.DetachFromWidget(_main);
			Chapters chapters = SingletonDict<string, Chapters>.Instance.Get(cat.Key);
			if (chapters == null)
			{
				SetNoData(T._("챕터 정보를 찾을 수 없습니다."));
			}
			else
			{
				_viewer.gameObject.SetActive(value: true);
				_viewer.Set(chapters);
			}
		});
		return true;
	}

	private void SetNoData(string text)
	{
		_noData.gameObject.SetActive(value: true);
		_noData.text = text;
	}
}
