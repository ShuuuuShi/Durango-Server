using Player;
using UnityEngine;
using Yaml;

public class CombatTargetPortraits : MonoBehaviour
{
	[SerializeField]
	private UITexture _portraitPlayer;

	[SerializeField]
	private UISprite _portraitAnimal;

	[SerializeField]
	private UISprite _portraitArtifact;

	[SerializeField]
	private UILabel _textPlayerFreq;

	private DamageableEntity _entity;

	public void SetPortrait(DamageableEntity entity)
	{
		if (_entity != null && _entity == entity)
		{
			return;
		}
		_entity = entity;
		((Component)_portraitPlayer).gameObject.SetActive(false);
		((Component)_portraitAnimal).gameObject.SetActive(false);
		((Component)_portraitArtifact).gameObject.SetActive(false);
		if ((Object)(object)_textPlayerFreq != (Object)null)
		{
			_textPlayerFreq.text = string.Empty;
		}
		if (!(_entity != null))
		{
			return;
		}
		CharacterBehavior component = _entity.GameObject.GetComponent<CharacterBehavior>();
		if ((Object)(object)component != (Object)null)
		{
			if (component is PlayerBehavior)
			{
				KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(component.EntityId, delegate(PlayerInfo info)
				{
					if (info.Valid)
					{
						((Component)_portraitPlayer).gameObject.SetActive(true);
						PortraitBuilder.Set(info.GetPortraitArgument(), _portraitPlayer);
						if ((Object)(object)_textPlayerFreq != (Object)null)
						{
							_textPlayerFreq.text = $"#{info.Freq:0000}";
						}
					}
				});
			}
			else if (component is AnimalBehavior)
			{
				string id = $"#combat_target_portrait_{AnimalYaml.GetId(component.EntityTypeId)}";
				string text = IconMap.Get(id);
				if (text != null)
				{
					((Component)_portraitAnimal).gameObject.SetActive(true);
					_portraitAnimal.spriteName = text;
				}
			}
		}
		else
		{
			Artifact component2 = _entity.GameObject.GetComponent<Artifact>();
			if ((Object)(object)component2 != (Object)null)
			{
				((Component)_portraitArtifact).gameObject.SetActive(true);
				_portraitArtifact.spriteName = component2.Blueprint.Icon;
			}
		}
	}
}
