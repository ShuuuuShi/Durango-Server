using Durango.Logic.Estate;
using Durango.Logic.Interactions;
using Durango.Render.Camera;
using Durango.Terrain;
using Durango.UI.Control;
using JetBrains.Annotations;
using Shared.Estate;
using UnityEngine;

namespace Durango.UI;

public class InteractionHelperLabel : SelectableWidget
{
	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UISprite _bgSprite;

	[SerializeField]
	private GameObject _permissionIconObject;

	protected ImmovableBase _immovable;

	private CharacterBehavior _movable;

	private readonly TargetPosition _targetPosition = new TargetPosition();

	public GameObject Target { get; private set; }

	public float TweenAlphaDelta { get; set; }

	public bool IsShow { get; set; }

	public void Set(GameObject obj)
	{
		Target = obj;
		_targetPosition.Set(obj);
		if ((bool)obj)
		{
			_immovable = obj.GetComponent<ImmovableBase>();
			_movable = obj.GetComponent<CharacterBehavior>();
		}
		else
		{
			_immovable = null;
			_movable = null;
		}
		UpdateContents();
	}

	public virtual void UpdateContents()
	{
		_permissionIconObject.SetActive(value: false);
		if (Target == null)
		{
			return;
		}
		SelectableObject component = Target.GetComponent<SelectableObject>();
		string icon = null;
		Color col = Color.white;
		Color c = Color.white;
		string text;
		if ((bool)component)
		{
			text = component.GetName();
		}
		else if ((bool)_immovable)
		{
			Artifact artifact = _immovable as Artifact;
			if ((bool)artifact)
			{
				GetIconFromArtifact(artifact, out icon, ref col);
				artifact.CheckPermissionForMe(_permissionIconObject.SetActive);
			}
			else
			{
				GetIconFromImmovable(_immovable, out icon, ref col);
				if (DataHelper.IsWarpRushTargetObject(_immovable.EntityType))
				{
					c = PresetColor.UIYellow;
				}
			}
			text = _immovable.GetName();
		}
		else if ((bool)_movable)
		{
			PlayerBehavior playerBehavior = _movable as PlayerBehavior;
			if ((bool)playerBehavior)
			{
				GetIconFromPlayer(playerBehavior, out icon, ref col);
			}
			else
			{
				GetIconFromCharacter(_movable, out icon, ref col);
			}
			text = _movable.GetName();
		}
		else
		{
			text = Target.name;
		}
		string text2 = NGUIText.EncodeColor24(col);
		string text3 = NGUIText.EncodeColor24(c);
		_nameLabel.text = ((!string.IsNullOrEmpty(icon)) ? string.Format("[{0}][icon={1}:1.5][-] [{3}]{2}[-]", text2, icon, text, text3) : text);
		int width = _nameLabel.width;
		int height = _nameLabel.height;
		base.Widget.SetDimensions(width + 70, height + 70);
		_bgSprite.UpdateAnchors();
	}

	private static void GetIconFromPlayer([NotNull] PlayerBehavior player, out string icon, ref Color col)
	{
		icon = ((!player.IsAlive) ? "icon_map_dead" : "icon_map_player");
		if (!player.IsLocalPlayer && player.IsAlive)
		{
			col = PlayerFloatingGroup.GetPlayerColor(player, col);
		}
	}

	private static void GetIconFromCharacter(CharacterBehavior character, out string icon, ref Color col)
	{
		icon = ((!character.IsAlive) ? "icon_map_poi_animal_dead" : "icon_map_poi_animal");
		PetAI component = character.GetComponent<PetAI>();
		if ((bool)component)
		{
			GameObject master = component.Master;
			string entityId = ObjectIdentifier.GetEntityId(master);
			if (entityId == GameManager.PlayerId)
			{
				col = new Color32(122, 172, byte.MaxValue, byte.MaxValue);
			}
			else if (GameSystem<SocialSystem>.Instance().IsFriend(entityId))
			{
				col = PresetColor.UIFriendlyPink;
			}
		}
		else
		{
			col = new Color32(byte.MaxValue, 192, 0, byte.MaxValue);
		}
	}

	private static void GetIconFromArtifact(Artifact artifact, out string icon, ref Color col)
	{
		icon = ((artifact.Blueprint != null) ? artifact.Blueprint.ArtifactIcon : null);
		EstateInfo estateInfo = artifact.GetEstateInfo();
		if (estateInfo == null)
		{
			return;
		}
		if (estateInfo.IsLocalPlayers())
		{
			switch (estateInfo.License.Type)
			{
			case OwnerType.Player:
			case OwnerType.PersonalPlayer:
				col = new Color32(122, 172, byte.MaxValue, byte.MaxValue);
				break;
			case OwnerType.ClanEstate:
				col = PresetColor.PlayerClan;
				break;
			}
		}
		else if ((estateInfo.License.Type == OwnerType.Player || estateInfo.License.Type == OwnerType.PersonalPlayer) && GameSystem<SocialSystem>.Instance().IsFriend(estateInfo.License.OwnerId))
		{
			col = PresetColor.UIFriendlyPink;
		}
	}

	private static void GetIconFromImmovable(ImmovableBase immovable, out string icon, ref Color col)
	{
		icon = null;
		int entityType = immovable.EntityType;
		BiomeSpriteInfo biomeSpriteInfo = DataHelper.GetBiomeSpriteInfo(entityType);
		if (biomeSpriteInfo != null)
		{
			icon = biomeSpriteInfo.Icon;
			if (icon == null)
			{
				icon = IconMap.Get(biomeSpriteInfo.SpriteObjectType);
			}
		}
	}

	public void UpdatePosition()
	{
		if (_targetPosition.TryGet(out var pos))
		{
			base.transform.localPosition = MainCamera.WorldToNGUIPos(pos);
		}
	}

	private void OnDrag(Vector2 delta)
	{
		UIManager.SetCurrentUITouchEvent(enable: false);
	}
}
