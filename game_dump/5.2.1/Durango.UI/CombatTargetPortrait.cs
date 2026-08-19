using Durango.Player;
using Durango.Utils;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class CombatTargetPortrait : MonoBehaviour
{
	[SerializeField]
	private UITexture _portraitPlayer;

	[SerializeField]
	private UISprite _imageEmblemBorder;

	[SerializeField]
	private UITexture _textureClanEmblem;

	[SerializeField]
	private UISprite _portraitAnimal;

	[SerializeField]
	private UISprite _portraitArtifact;

	private DamageableEntity _entity;

	public void SetPortrait(DamageableEntity entity)
	{
		if (_entity != null && _entity == entity)
		{
			return;
		}
		_entity = entity;
		_portraitPlayer.gameObject.SetActive(value: false);
		_imageEmblemBorder.gameObject.SetActive(value: false);
		_textureClanEmblem.gameObject.SetActive(value: false);
		_portraitAnimal.gameObject.SetActive(value: false);
		_portraitArtifact.gameObject.SetActive(value: false);
		if (_entity == null)
		{
			return;
		}
		CharacterBehavior component = _entity.GameObject.GetComponent<CharacterBehavior>();
		if (component != null)
		{
			PlayerBehavior playerBehavior = component as PlayerBehavior;
			if (playerBehavior != null)
			{
				Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(playerBehavior.EntityId, delegate(PlayerInfo info)
				{
					if (info.Valid)
					{
						_portraitPlayer.gameObject.SetActive(value: true);
						PortraitBuilder.Argument portraitArgument = info.GetPortraitArgument();
						portraitArgument.Mask = null;
						PortraitBuilder.Set(portraitArgument, _portraitPlayer);
					}
				});
				ClanSystem.GetEmblem(playerBehavior.ClanId, SetClanEmblem);
			}
			else if (component is AnimalBehavior)
			{
				PetAI component2 = _entity.GameObject.GetComponent<PetAI>();
				string portrait = AnimalYaml.GetPortrait((!(component2 != null)) ? component.EntityTypeId : component2.AnimalEntityType);
				if (portrait != null)
				{
					_portraitAnimal.gameObject.SetActive(value: true);
					_portraitAnimal.spriteName = portrait;
				}
			}
		}
		else
		{
			Artifact component3 = _entity.GameObject.GetComponent<Artifact>();
			if (component3 != null)
			{
				_portraitArtifact.gameObject.SetActive(value: true);
				_portraitArtifact.spriteName = component3.Blueprint.Icon;
			}
		}
	}

	private void SetClanEmblem(Point2 pos)
	{
		if (pos.x < 0 || pos.y < 0)
		{
			_imageEmblemBorder.gameObject.SetActive(value: false);
			_textureClanEmblem.gameObject.SetActive(value: false);
		}
		else
		{
			_imageEmblemBorder.gameObject.SetActive(value: true);
			_textureClanEmblem.gameObject.SetActive(value: true);
			EmblemTexture.Set(_textureClanEmblem, pos);
		}
	}
}
