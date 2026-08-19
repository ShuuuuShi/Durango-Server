using AutoGuide;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

public class AutoGuideTemplateNode : MonoBehaviour
{
	[SerializeField]
	private UILabel _name;

	[SerializeField]
	private UILabel _description;

	[SerializeField]
	private UISprite _guidedIcon;

	[SerializeField]
	private Color _selectedColor;

	[SerializeField]
	private GameObject _closeBtn;

	[SerializeField]
	private GameObject _newIcon;

	private Color _nameDefaultColor;

	private Color _descriptionDefaultColor;

	public Template Template { get; private set; }

	public bool Selected
	{
		get
		{
			return Template.LastSelected;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			if (_nameDefaultColor == default(Color))
			{
				_nameDefaultColor = _name.color;
			}
			if (_descriptionDefaultColor == default(Color))
			{
				_descriptionDefaultColor = _description.color;
			}
			_name.color = ((!value) ? _nameDefaultColor : _selectedColor);
			_description.color = ((!value) ? _descriptionDefaultColor : _selectedColor);
			_guidedIcon.color = ((!value) ? _nameDefaultColor : _selectedColor);
			Template.LastSelected = value;
			if (value && (Object)(object)((RaycastHit)(ref UICamera.lastHit)).collider != (Object)null && (Object)(object)((Component)((RaycastHit)(ref UICamera.lastHit)).collider).gameObject == (Object)(object)((Component)this).gameObject)
			{
				GameSystem<AutoGuideSystem>.Instance().SetIsNew(Template.Key, isNew: false);
			}
		}
	}

	private void Awake()
	{
		UIEventListener.Get(_closeBtn).onClick = CloseBtn_OnClick;
	}

	private void CloseBtn_OnClick(GameObject go)
	{
		UIManager.MessageBox.Show(T._("이 과제를 삭제하고 다른 과제를 받으시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				GameSystem<AutoGuideSystem>.Instance().CancelTemplate(Template.Key);
			}
		});
	}

	public void Set([NotNull] Template template)
	{
		Template = template;
		_name.text = Template.TitleText;
		((Component)_guidedIcon).gameObject.SetActive(template.IsGuided());
		_description.text = T._("{0} | <em>{1}</em> 학점", LocalizeUtil.Get(Template.Difficulty), Template.Point);
		_newIcon.SetActive(GameSystem<AutoGuideSystem>.Instance().GetIsNew(Template.Key));
	}
}
