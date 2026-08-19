using System.Text;
using Messages;
using Shared.Battle;
using UnityEngine;

public class DamageWidget : MonoBehaviour
{
	private UIWidget _widget;

	[SerializeField]
	private UISpriteLabel _expainTextLabel;

	[SerializeField]
	private UILabel _damageLabel;

	[SerializeField]
	private UILabel _criticalLabel;

	[SerializeField]
	private int _padding;

	[SerializeField]
	private int _lineMargin;

	[SerializeField]
	private Color _hitColor;

	[SerializeField]
	private Color _missedColor;

	public UIWidget Widget => (!((Object)(object)_widget != (Object)null)) ? (_widget = ((Component)this).GetComponent<UIWidget>()) : _widget;

	public void Set(Damage damage)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		_expainTextLabel.text = GetDamageOptionString(damage);
		_damageLabel.text = UIManager.GetDamageString(damage);
		bool active = (damage.Effects & DamageEffects.Critical) != 0;
		((Component)_criticalLabel).gameObject.SetActive(active);
		if (damage.Value <= 0)
		{
			_damageLabel.fontSize = 24;
		}
		else if (damage.Result == DamageResult.Missed)
		{
			_damageLabel.color = _missedColor;
			_damageLabel.fontSize = 48;
		}
		else
		{
			_damageLabel.color = _hitColor;
			_damageLabel.fontSize = 60;
		}
		UpdateLayout();
	}

	private string GetDamageOptionString(Damage damage)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(LocalizeSystem.Format("#damage_dir_explain_text", LocalizeSystem.Get($"#damage_option_dir_{damage.Direction.ToString().ToLower()}")));
		stringBuilder.AppendLine(LocalizeSystem.Format("#damage_body_part_expain_text", LocalizeSystem.Get($"#body_part_{damage.Part}")));
		return stringBuilder.ToString().Trim();
	}

	public void UpdateLayout()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		Vector2 printedSize = _expainTextLabel.Label.printedSize;
		Vector2 val = ((!((Component)_criticalLabel).gameObject.activeSelf) ? Vector2.zero : _criticalLabel.printedSize);
		Vector2 printedSize2 = _damageLabel.printedSize;
		Vector2 val2 = default(Vector2);
		val2.x = KMathUtil.Max(val.x, printedSize.x, printedSize2.x);
		val2.y = val.y + printedSize.y + printedSize2.y + (float)_lineMargin;
		Widget.width = (int)val2.x + _padding * 2;
		Widget.height = (int)val2.y + _padding * 2;
		Vector3 val3 = Widget.localCorners[1];
		int padding = _padding;
		UIWidget label = _expainTextLabel.Label;
		Vector3 localPosition = val3 + new Vector3((float)_padding, (float)(-padding));
		localPosition.x += label.pivotOffset.x * (float)label.width;
		localPosition.y -= (1f - label.pivotOffset.y) * (float)label.height;
		((Component)label).transform.localPosition = localPosition;
		padding += label.height;
		padding += (int)val.y + _lineMargin;
		UIWidget damageLabel = _damageLabel;
		Vector3 localPosition2 = val3 + new Vector3((float)_padding, (float)(-padding));
		localPosition2.x += damageLabel.pivotOffset.x * (float)damageLabel.width;
		localPosition2.y -= (1f - damageLabel.pivotOffset.y) * (float)damageLabel.height;
		((Component)damageLabel).transform.localPosition = localPosition2;
		_criticalLabel.UpdateAnchors();
	}

	public void ShowAnimation()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		TweenPosition component = ((Component)_damageLabel).GetComponent<TweenPosition>();
		component.from = ((Component)_damageLabel).transform.localPosition + Vector3.down * 50f;
		component.to = ((Component)_damageLabel).transform.localPosition;
		component.ResetToBeginning();
		component.PlayForward();
		TweenPosition component2 = ((Component)_expainTextLabel).GetComponent<TweenPosition>();
		component2.from = ((Component)_expainTextLabel).transform.localPosition + Vector3.down * 50f;
		component2.to = ((Component)_expainTextLabel).transform.localPosition;
		component2.ResetToBeginning();
		component2.PlayForward();
		TweenAlpha component3 = ((Component)_damageLabel).GetComponent<TweenAlpha>();
		component3.from = 0f;
		component3.to = 1f;
		component3.ResetToBeginning();
		component3.PlayForward();
		TweenAlpha component4 = ((Component)_expainTextLabel).GetComponent<TweenAlpha>();
		component4.from = 0f;
		component4.to = 1f;
		component4.ResetToBeginning();
		component4.PlayForward();
		TweenScale component5 = ((Component)_damageLabel).GetComponent<TweenScale>();
		component5.ResetToBeginning();
		component5.PlayForward();
	}
}
