using System.Collections.Generic;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class SelectPetItemWidget : SelectableWidget
{
	[SerializeField]
	private RectLayoutComponent _container;

	[SerializeField]
	private UISprite _portraitSprite;

	[SerializeField]
	private ItemGradeViewer _gradeViewer;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _infoLabel;

	public Messages.Pet Pet { get; private set; }

	private void Start()
	{
		_container.UpdateOnSizeChange();
	}

	public void Set(Messages.Pet pet)
	{
		Pet = pet;
		Animal animal = SingletonDict<int, Animal>.Get(pet.GetAnimalType());
		_portraitSprite.spriteName = ((animal != null) ? animal.Portrait : string.Empty);
		_nameLabel.text = pet.GetPetName(includeRank: true);
		_infoLabel.text = string.Format("{0}  <bar/>  {1}", LocalizeUtil.FormatLevel(pet.Statistics.Level), T._("크기 {0}", pet.Stat.Size));
		_gradeViewer.SetOptions(0.5f, upward: true, 5);
		_gradeViewer.SettingBegin();
		if (pet.Stat.Tags != null)
		{
			foreach (KeyValuePair<string, int> tag in pet.Stat.Tags)
			{
				_gradeViewer.AddTagData(tag.Key, tag.Value);
			}
		}
		_gradeViewer.SettingEnd();
	}
}
