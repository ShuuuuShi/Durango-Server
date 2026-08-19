using Estate;
using Shared.Estate;
using UnityEngine;

public class InteractionHelperLabel : SelectableWidget
{
	[SerializeField]
	private UISpriteLabel _nameLabel;

	[SerializeField]
	private UISprite _bgSprite;

	private Transform _target;

	private CharacterBehavior _character;

	private ImmovableBase _immovable;

	public GameObject Target => (!((Object)(object)_target == (Object)null)) ? ((Component)_target).gameObject : null;

	public void Set(GameObject target)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target == (Object)null)
		{
			_target = null;
			_character = null;
			_immovable = null;
			return;
		}
		_target = target.transform;
		_character = target.GetComponent<CharacterBehavior>();
		_immovable = target.GetComponent<ImmovableBase>();
		string text = null;
		Color color = Color.white;
		string text2;
		if (Object.op_Implicit((Object)(object)_character))
		{
			PlayerBehavior playerBehavior = _character as PlayerBehavior;
			if (Object.op_Implicit((Object)(object)playerBehavior))
			{
				text = ((!playerBehavior.IsAlive) ? "icon_map_dead" : "icon_map_player");
				if (!playerBehavior.IsLocalPlayer && playerBehavior.IsAlive)
				{
					if (GameSystem<SocialSystem>.Instance().FollowingList.Contains(playerBehavior.EntityId))
					{
						color = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)122, (byte)207, byte.MaxValue));
					}
					else if (playerBehavior.ClanId != 0L)
					{
						if (playerBehavior.ClanId == PlayerBehavior.LocalPlayer.ClanId)
						{
							color = Color32.op_Implicit(new Color32((byte)102, (byte)232, (byte)56, byte.MaxValue));
						}
						else if (GameSystem<ClanSystem>.Instance().IsEnemyClan(playerBehavior.ClanId))
						{
							if (playerBehavior.IsAlive)
							{
								text = "icon_map_player_enemy";
							}
							color = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)0, (byte)0, byte.MaxValue));
						}
					}
				}
			}
			else
			{
				AnimalBehavior animalBehavior = _character as AnimalBehavior;
				if (Object.op_Implicit((Object)(object)animalBehavior))
				{
					PetAI component = ((Component)_character).GetComponent<PetAI>();
					if (Object.op_Implicit((Object)(object)component))
					{
						GameObject master = component.Master;
						ulong entityId = ObjectIdentifier.GetEntityId(master);
						if (entityId == GameManager.PlayerId)
						{
							color = Color32.op_Implicit(new Color32((byte)122, (byte)172, byte.MaxValue, byte.MaxValue));
						}
						else if (GameSystem<SocialSystem>.Instance().FollowingList.Contains(entityId))
						{
							color = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)122, (byte)207, byte.MaxValue));
						}
					}
					else
					{
						color = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)192, (byte)0, byte.MaxValue));
					}
					text = ((!animalBehavior.IsAlive) ? "icon_map_poi_animal_dead" : "icon_map_poi_animal");
				}
			}
			text2 = _character.GetName();
		}
		else if (Object.op_Implicit((Object)(object)_immovable))
		{
			Artifact artifact = _immovable as Artifact;
			if (Object.op_Implicit((Object)(object)artifact))
			{
				text = ((artifact.Blueprint != null) ? artifact.Blueprint.ArtifactIcon : null);
				ulong estateId = artifact.GetEstateId();
				EstateInfo estateInfo = GameSystem<EstateSystem>.Instance().GetEstateInfo(estateId);
				if (estateInfo != null)
				{
					if (estateInfo.OwnerType == OwnerType.Player)
					{
						if (estateInfo.Owner == GameManager.PlayerId)
						{
							color = Color32.op_Implicit(new Color32((byte)122, (byte)172, byte.MaxValue, byte.MaxValue));
						}
					}
					else if (estateInfo.OwnerType == OwnerType.ClanEstate)
					{
						if (estateInfo.Owner == PlayerBehavior.LocalPlayer.ClanId)
						{
							color = Color32.op_Implicit(new Color32((byte)102, (byte)232, (byte)56, byte.MaxValue));
						}
						else if (GameSystem<ClanSystem>.Instance().IsEnemyClan(estateInfo.Owner))
						{
							color = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)34, (byte)34, byte.MaxValue));
						}
					}
				}
			}
			else
			{
				NaturalObject naturalObject = _immovable as NaturalObject;
				if (Object.op_Implicit((Object)(object)naturalObject))
				{
					int entityType = naturalObject.EntityType;
					BiomeSpriteInfo biomeSpriteInfo = TerrainDataHelper.GetBiomeSpriteInfo(entityType);
					if (biomeSpriteInfo != null)
					{
						text = biomeSpriteInfo.Icon;
						if (text == null)
						{
							text = IconMap.Get(biomeSpriteInfo.SpriteObjectType);
						}
					}
				}
			}
			text2 = _immovable.GetName();
		}
		else
		{
			SelectableObject component2 = ((Component)_target).GetComponent<SelectableObject>();
			text2 = ((!((Object)(object)component2 != (Object)null)) ? ((Object)_target).name : component2.GetName());
		}
		_nameLabel.text = ((!string.IsNullOrEmpty(text)) ? $"[icon={text}:1.5] {text2}" : text2);
		_nameLabel.Label.color = color;
		int width = _nameLabel.Label.width;
		int height = _nameLabel.Label.height;
		base.Widget.SetDimensions(width + 70, height + 70);
		_bgSprite.UpdateAnchors();
		base.Widget.alpha = 0f;
		TweenAlpha.Begin(((Component)this).gameObject, 0.2f, 1f);
	}

	public void UpdatePosition()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 world;
		if (Object.op_Implicit((Object)(object)_character))
		{
			world = _character.InteractionPosition;
		}
		else if (Object.op_Implicit((Object)(object)_immovable))
		{
			world = _immovable.InteractionPosition;
		}
		else
		{
			if (!Object.op_Implicit((Object)(object)_target))
			{
				return;
			}
			world = _target.position;
		}
		((Component)this).transform.localPosition = MainCamera.WorldToNGUIPos(world);
	}

	private void OnDrag(Vector2 delta)
	{
		UIManager.SetCurrentUITouchEvent(enable: false);
	}
}
