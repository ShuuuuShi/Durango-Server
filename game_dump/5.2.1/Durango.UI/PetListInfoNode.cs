using Durango.UI.Control;
using L10N;
using Messages;
using Shared.Ability;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class PetListInfoNode : SelectableWidget
{
	[SerializeField]
	private UISprite _portraitSprite;

	[SerializeField]
	private UISprite _portraitBg;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _infoLabel;

	[SerializeField]
	private UILabel _oldLabel;

	[SerializeField]
	private GameObject _deadMark;

	public Messages.Pet Pet { get; private set; }

	private void Update()
	{
		if (!(Pet.EntityId == string.Empty))
		{
			UpdateGauge();
		}
	}

	public void Set(Messages.Pet pet)
	{
		Pet = pet;
		Animal animal = SingletonDict<int, Animal>.Get(pet.GetAnimalType());
		_portraitSprite.spriteName = ((animal != null) ? animal.Portrait : string.Empty);
		_nameLabel.text = pet.GetPetName(includeRank: true);
		UpdateGauge();
		_infoLabel.text = GetInfoText(Pet);
		_oldLabel.text = ((!pet.Stat.IsOld) ? string.Empty : string.Format("<alert>{0}</alert>", T._("노화됨")));
	}

	private string GetInfoText(Messages.Pet pet)
	{
		if (pet.CageInfo.HasValue && !string.IsNullOrEmpty(pet.CageInfo.Value.RegionId))
		{
			return T._("[FFFFFF96]{0:lv:}[-] [FFFFFF64][icon=bg_line_height][-] [FFFFFF64][icon=icon_map_pinpoint][-] [FFFFFF96]{1}[-]", pet.Statistics.Level, pet.CageInfo.Value.RegionName);
		}
		return T._("[FFFFFF96]{0:lv:}[-] [FFFFFF64][icon=bg_line_height][-] [FFFFFF64][icon=icon_bag][-] [FFFFFF96]{1}/{2:0}[-]", pet.Statistics.Level, pet.Stat.InventoryUsage, pet.Statistics.DerivedAbilities.Get(Derived.InventoryCapacity, 0f));
	}

	private void UpdateGauge()
	{
		bool flag = ((Pet.Stat.Life != null) ? Pet.Stat.Life.Ratio() : 0f) <= 0f;
		if (_deadMark != null)
		{
			_deadMark.gameObject.SetActive(flag);
		}
		_portraitBg.color = ((!flag) ? new Color32(130, 124, 102, byte.MaxValue) : new Color32(113, 43, 17, byte.MaxValue));
	}
}
