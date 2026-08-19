using Durango.Logic.Skill;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class LearningSkillWidget : MonoBehaviour
{
	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private GameObject _imageDotLine;

	[SerializeField]
	private UIWidget _textsPane;

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private UILabel _textLevel;

	[SerializeField]
	private GameObject _currentRankPane;

	[SerializeField]
	private GameObject _rankUpPane;

	[SerializeField]
	private UIWidget _arrowPane;

	[SerializeField]
	private UIWidget _iconArrow;

	[SerializeField]
	private Color _textRankFull;

	[SerializeField]
	private Color _arrowCanRankUp;

	[SerializeField]
	private Color _arrowRankFull;

	[SerializeField]
	private Color _backgroundNormal;

	[SerializeField]
	private Color _backgroundPressed;

	[SerializeField]
	private string _formatRankNormal;

	[SerializeField]
	private string _formatRankfull;

	private bool _initialized;

	private Color _defaultTextNameColor;

	private Color _defaultArrowColor;

	[CanBeNull]
	private Node _skillNode;

	private bool _clickable;

	public bool ShowUpperDotLine
	{
		get
		{
			return _imageDotLine.activeSelf;
		}
		set
		{
			_imageDotLine.SetActive(value);
		}
	}

	public void Init()
	{
		if (!_initialized)
		{
			UIEventListener.Get(base.gameObject).onClick = OnClickGameObject;
			_defaultTextNameColor = _textName.color;
			_defaultArrowColor = _iconArrow.color;
			_initialized = true;
		}
	}

	public void SetSkill([CanBeNull] Node skillNode = null, bool clickable = false)
	{
		_skillNode = skillNode;
		_background.color = _backgroundNormal;
		Refresh(clickable);
	}

	public void Refresh(bool clickable)
	{
		_clickable = clickable;
		if (_skillNode == null)
		{
			_textsPane.gameObject.SetActive(value: false);
			_arrowPane.gameObject.SetActive(value: false);
			_rankUpPane.SetActive(value: false);
			return;
		}
		_textsPane.gameObject.SetActive(value: true);
		_textsPane.rightAnchor.absolute = (_clickable ? _arrowPane.leftAnchor.absolute : 0);
		_arrowPane.gameObject.SetActive(_clickable);
		_textName.text = _skillNode.Name;
		int num = Mathf.Min(_skillNode.Parent.Level, _skillNode.Level);
		int level = _skillNode.Level;
		bool flag = num == level;
		if (_clickable && CanRankUp(_skillNode) && !flag)
		{
			_currentRankPane.SetActive(value: false);
			_rankUpPane.SetActive(value: true);
			_iconArrow.color = _arrowCanRankUp;
		}
		else
		{
			_currentRankPane.SetActive(value: true);
			_rankUpPane.SetActive(value: false);
			_textLevel.text = string.Format((!flag) ? _formatRankNormal : _formatRankfull, T._("랭크"), num, level);
			_iconArrow.color = ((!flag) ? _defaultArrowColor : _arrowRankFull);
		}
		_textName.color = ((!flag) ? _defaultTextNameColor : _textRankFull);
		UIUtility.UpdateAnchors(base.transform);
	}

	private static bool CanRankUp([NotNull] Node skill)
	{
		if (skill.State != State.Learnable)
		{
			return skill.Parent.HasLearnableNode();
		}
		return true;
	}

	private void OnClickGameObject(GameObject go)
	{
		if (_clickable && _skillNode != null)
		{
			UISound.PlayClick(UISound.ClickType.ButtonDefault);
			UIManager.FindScript<LearningGuideGroup>().CloseAndRedirectToSkillNode(_skillNode);
		}
	}

	[UsedImplicitly]
	private void OnPress(bool press)
	{
		if (_clickable)
		{
			_background.color = ((!press) ? _backgroundNormal : _backgroundPressed);
		}
	}
}
