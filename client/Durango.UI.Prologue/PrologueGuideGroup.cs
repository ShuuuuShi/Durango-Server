using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueGuideGroup : PrologueGuideGroupBase
{
	[SerializeField]
	private UILabel _nameTagLabel;

	[SerializeField]
	private GameObject _touchHand;

	protected override void Awake()
	{
		base.Awake();
		Typewriter.Finished += TypeWriter_Finished;
	}

	private void TypeWriter_Finished()
	{
		_touchHand.gameObject.SetActive(!DoNotFinishByTouch);
	}

	protected override void SetGuideMsg(string msgTxt)
	{
		base.SetGuideMsg(msgTxt);
		_touchHand.gameObject.SetActive(value: false);
	}

	protected override void DeactivateAllPortraits()
	{
		base.DeactivateAllPortraits();
		_nameTagLabel.gameObject.SetActive(value: false);
	}

	protected override void ActivateNameTag(string nameTag, bool deactivateOthers = true)
	{
		if (deactivateOthers)
		{
			DeactivateAllPortraits();
		}
		_nameTagLabel.text = LocalizeSystem.Get(nameTag);
		_nameTagLabel.gameObject.SetActive(value: true);
	}
}
