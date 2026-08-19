using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class DialogueGroup : DialogueGroupBase
{
	[SerializeField]
	private UISprite _textBackground;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UIWidget _touchHand;

	private bool _blurOn;

	protected override void Start()
	{
		base.Start();
		UIEventListener.Get(_textBackground.gameObject).onPress = delegate(GameObject go, bool state)
		{
			OnPressDialogue(state);
		};
	}

	protected override void SetChoiceCount(int count)
	{
		base.SetChoiceCount(count);
		if (count > 0)
		{
			UIWidget widget = ChoicePool.Get<ChoiceButton>(0).Widget;
			int width = widget.width;
			int height = widget.height;
			if (base.IsPortrait)
			{
				int num = 300 - height * count;
				num /= count + 1;
				int num2 = 150 - height / 2 - num;
				ChoicePool.BaseObject.transform.localPosition = new Vector3(0f, num2, 0f);
				ChoicePool.Reposition(Vector3.down, num);
			}
			else
			{
				int num3 = 1280 - width * count;
				num3 /= count + 1;
				int num4 = -640 + width / 2 + num3;
				ChoicePool.BaseObject.transform.localPosition = new Vector3(num4, 0f, 0f);
				ChoicePool.Reposition(Vector3.right, num3);
			}
		}
	}

	protected override void BlurOn()
	{
		if (!_blurOn)
		{
			_blurOn = true;
			BlurMaskingGroup blurMaskingGroup = UIManager.FindScript<BlurMaskingGroup>();
			if (blurMaskingGroup != null)
			{
				blurMaskingGroup.ClearObject();
				blurMaskingGroup.AddObject(base.gameObject);
				blurMaskingGroup.OnPressBlur = OnPressBlur;
				blurMaskingGroup.Open();
			}
			VisibleController.Hide(VisibleType.Base, hide: true, "GuideBlur");
		}
	}

	protected override void BlurOff()
	{
		if (_blurOn)
		{
			_blurOn = false;
			BlurMaskingGroup blurMaskingGroup = UIManager.FindScript<BlurMaskingGroup>();
			if (blurMaskingGroup != null)
			{
				blurMaskingGroup.Close();
			}
			VisibleController.Hide(VisibleType.Base, hide: false, "GuideBlur");
		}
	}

	private bool OnPressBlur(bool pressed)
	{
		OnPressDialogue(pressed);
		return true;
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		MainWidget.topAnchor.absolute = ((!base.IsPortrait) ? 150 : 300);
		UIUtility.UpdateAnchors(MainWidget.transform);
	}

	protected override void SetDialogue(Context ctx)
	{
		if (ctx.Name.IsBlank || Current.Portrait != null)
		{
			_nameLabel.gameObject.SetActive(value: false);
		}
		else
		{
			_nameLabel.gameObject.SetActive(value: true);
			_nameLabel.text = ctx.Name;
			_nameLabel.color = ctx.Name.Color;
		}
		DialogueLabel.leftAnchor.absolute = ((!_nameLabel.gameObject.activeSelf) ? (-_touchHand.leftAnchor.absolute) : (_nameLabel.rightAnchor.absolute + 40));
		DialogueLabel.text = ctx.Message;
		_touchHand.gameObject.SetActive(value: false);
		base.SetDialogue(ctx);
	}

	protected override void TypeWriterDialouge_Finished()
	{
		_touchHand.gameObject.SetActive(value: true);
	}

	protected override void TypeWriteSystem_Finished()
	{
		_touchHand.gameObject.SetActive(value: true);
	}
}
