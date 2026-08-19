using L10N;
using UnityEngine;

namespace Durango.UI;

public class DialogueGroup_PC : DialogueGroupBase
{
	[SerializeField]
	private GameObject _backPlate;

	[SerializeField]
	private GameObject _bottomBackground;

	[SerializeField]
	private UILabel _spaceBar;

	public static bool IsShow { get; private set; }

	private void Awake()
	{
		IsShow = false;
		_backPlate.SetActive(value: false);
	}

	protected override void Start()
	{
		base.Start();
		_spaceBar.text = string.Format("<shortcut_box>{0}</shortcut_box>  {1}", InputCommand.GuideUISpaceKeyUp, T._("다음"));
		GameSystem<InputSystem>.Instance().On(InputCommand.GuideUISpaceKeyDown, delegate
		{
			if (_spaceBar.gameObject.activeInHierarchy)
			{
				OnPressDialogue(pressed: true);
			}
		});
		GameSystem<InputSystem>.Instance().On(InputCommand.GuideUISpaceKeyUp, delegate
		{
			if (_spaceBar.gameObject.activeInHierarchy)
			{
				OnPressDialogue(pressed: true);
				OnPressDialogue(pressed: false);
			}
		});
	}

	private void OnDisable()
	{
		IsShow = false;
	}

	protected override void SetChoiceCount(int count)
	{
		base.SetChoiceCount(count);
		bool flag = count > 0;
		if (flag)
		{
			int num = (ChoicePool.Get<ChoiceButton>(0).Widget.width + 14) * (count - 1) / -2;
			ChoicePool.BaseObject.transform.localPosition = new Vector3(num, 0f, 0f);
			ChoicePool.Reposition(Vector3.right, 14);
		}
		Vector3 localPosition = SystemLabel.transform.localPosition;
		localPosition.y = (flag ? 116 : 0);
		SystemLabel.transform.localPosition = localPosition;
	}

	protected override void OnRefresh()
	{
		if (Current != null)
		{
			switch (Current.Type)
			{
			case Type.System:
				_bottomBackground.SetActive(value: false);
				break;
			case Type.Dialogue:
			case Type.Quiz:
				_bottomBackground.SetActive(value: true);
				break;
			}
		}
	}

	protected override void BlurOn()
	{
		_backPlate.SetActive(value: true);
	}

	protected override void BlurOff()
	{
		_backPlate.SetActive(value: false);
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		UIUtility.UpdateAnchors(MainWidget.transform);
	}

	protected override void SetDialogue(Context ctx)
	{
		string text = ((ctx.Type == Type.System) ? string.Empty : ((!string.IsNullOrEmpty(ctx.Name)) ? ((string)ctx.Name) : T._("K")));
		if (string.IsNullOrEmpty(text) || text == " ")
		{
			DialogueLabel.text = ctx.Message;
		}
		else
		{
			DialogueLabel.text = text + " : " + ctx.Message.Text;
		}
		DialogueLabel.overflowWidth = MainWidget.width;
		base.SetDialogue(ctx);
	}

	protected override void Update()
	{
		base.Update();
		IsShow = DialogueContext.activeInHierarchy;
	}
}
