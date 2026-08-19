using System;
using Durango.Logic.Clan;
using Durango.Player;
using Durango.Render.Camera;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class CombatInspector : MonoBehaviour
{
	public enum Type
	{
		Player,
		AllyPlayer,
		EnemyPlayer,
		AllyArtifact,
		EnemyArtfaict,
		AllyPet,
		EnemyPet,
		EasyAnimal,
		HardAnimal
	}

	[Serializable]
	public struct TypeLayout
	{
		public int GaugeWidth;

		public int GaugeHeight;

		public Color GaugeColor;

		public Color GaugeTrailColor;

		public Color NameColor;
	}

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _clanLabel;

	[SerializeField]
	private UIWidget _aggroWidget;

	[SerializeField]
	private UISprite _aggroSprite;

	[SerializeField]
	private UIWidget _levelWidget;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UIWidget _gaugeWidget;

	[SerializeField]
	private UISprite _gaugeUpper;

	[SerializeField]
	private UISprite _gaugeTrail;

	[SerializeField]
	private UISprite _aggroAlert;

	private Gauge _gauge;

	private int _gaugeFrame;

	private bool? _aggroAlertOn;

	private Vector3 _offset;

	private float _gaugeTrailRatio;

	public DamageableEntity Target { get; private set; }

	public void Set(DamageableEntity entity, bool isEnemy)
	{
		Target = entity;
		Type index = Type.EasyAnimal;
		PlayerBehavior component = entity.GameObject.GetComponent<PlayerBehavior>();
		if (component != null)
		{
			index = ((!(component == PlayerBehavior.LocalPlayer)) ? ((!isEnemy) ? Type.AllyPlayer : Type.EnemyPlayer) : Type.Player);
			_nameLabel.text = component.GetName();
			_clanLabel.text = string.Empty;
			_levelLabel.text = string.Empty;
			Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(component.EntityId, OnPlayer);
			if (component.HasClan)
			{
				ClanSystem.GetClanInfo(component.ClanId, OnClan);
			}
		}
		else
		{
			_levelLabel.text = entity.GetLevel().ToString();
			AnimalBehavior component2 = entity.GameObject.GetComponent<AnimalBehavior>();
			if (component2 != null)
			{
				PetAI component3 = entity.GameObject.GetComponent<PetAI>();
				if (component3 != null)
				{
					index = ((!isEnemy) ? Type.AllyPet : Type.EnemyPet);
					_nameLabel.text = component2.GetName();
					_clanLabel.text = T._("{0}의", component3.OwnerName);
				}
				else
				{
					index = ((GameManager.Region.Level < component2.Level) ? Type.EasyAnimal : Type.HardAnimal);
					_nameLabel.text = component2.GetName();
					_clanLabel.text = string.Empty;
				}
			}
			else
			{
				Artifact component4 = entity.GameObject.GetComponent<Artifact>();
				if (component4 != null)
				{
					index = ((!isEnemy) ? Type.AllyArtifact : Type.EnemyArtfaict);
					_nameLabel.text = component4.GetName();
					_clanLabel.text = string.Empty;
				}
			}
		}
		_offset = (entity.Height - entity.GetInteractionPosition().y) * Vector3.up;
		TypeLayout typeLayout = base.transform.parent.GetComponent<CombatInspectors>().Layouts[(int)index];
		if (typeLayout.GaugeWidth <= 0 || typeLayout.GaugeHeight <= 0)
		{
			_gaugeWidget.gameObject.SetActive(value: false);
			_levelWidget.gameObject.SetActive(value: false);
		}
		else
		{
			_gaugeWidget.gameObject.SetActive(value: true);
			_levelWidget.gameObject.SetActive(value: true);
			_gaugeWidget.SetDimensions(typeLayout.GaugeWidth, typeLayout.GaugeHeight);
			_gaugeUpper.color = typeLayout.GaugeColor;
			_gaugeTrail.color = typeLayout.GaugeTrailColor;
		}
		_nameLabel.color = typeLayout.NameColor;
		if (_gaugeWidget.gameObject.activeSelf)
		{
			_nameLabel.transform.localPosition = Vector3.up * 10f;
			_clanLabel.transform.localPosition = Vector3.up * 30f;
		}
		else
		{
			_nameLabel.transform.localPosition = Vector3.zero;
			_clanLabel.transform.localPosition = Vector3.up * 20f;
		}
		UIUtility.UpdateAnchors(base.transform);
		Refresh();
	}

	public void SetSelect(bool isSelect)
	{
		_levelWidget.gameObject.SetActive(isSelect);
		_nameLabel.gameObject.SetActive(isSelect);
		_clanLabel.gameObject.SetActive(isSelect);
		if (!isSelect)
		{
			_gaugeWidget.transform.localPosition = Vector3.zero;
			base.transform.localScale = Vector3.one * 0.7f;
			return;
		}
		_gaugeWidget.transform.localPosition = Vector3.right * _levelWidget.width * 0.5f;
		_levelWidget.transform.localPosition = Vector3.left * _gaugeWidget.width * 0.5f;
		_aggroWidget.transform.localPosition = Vector3.left * ((float)_gaugeWidget.width * 0.5f + (float)_levelWidget.width * 0.5f + (float)_aggroWidget.width * 0.5f);
		base.transform.localScale = Vector3.one;
	}

	private void AggroAlert(bool show)
	{
		if (!_aggroAlertOn.HasValue || _aggroAlertOn != show)
		{
			_aggroAlertOn = show;
			_aggroAlert.gameObject.SetActive(show);
			_aggroSprite.gameObject.SetActive(show);
		}
	}

	private void OnPlayer(PlayerInfo player)
	{
		_levelLabel.text = player.Level.ToString();
	}

	private void OnClan(Clan clan)
	{
		_clanLabel.text = clan.Name;
	}

	public void Refresh()
	{
		if (!(Target == null))
		{
			base.transform.localPosition = MainCamera.WorldToNGUIPos(Target.GetInteractionPosition() + _offset);
			if (Target.GaugeChanged != _gaugeFrame || _gauge == null)
			{
				_gaugeFrame = Target.GaugeChanged;
				_gauge = Target.GetLife();
			}
			if (_gauge != null)
			{
				float ratio = _gauge.Get() / _gauge.RealMax();
				SetGaugeSpriteRatio(_gaugeUpper, ratio);
				SetTrailGaugeSprite(_gaugeUpper, _gaugeTrail, ref _gaugeTrailRatio);
			}
			AggroAlert(Target.TargetId == GameManager.PlayerId);
		}
	}

	private void SetTrailGaugeSprite([NotNull] UISprite gaugeSprite, [NotNull] UISprite trailSprite, ref float trailRatio)
	{
		float fillAmount = gaugeSprite.fillAmount;
		float num2;
		if (fillAmount < trailRatio)
		{
			float num = Time.deltaTime / 5f;
			num2 = Mathf.Max(trailRatio - num, fillAmount);
		}
		else
		{
			num2 = fillAmount;
		}
		trailRatio = num2;
		SetGaugeSpriteRatio(trailSprite, num2);
	}

	private void SetGaugeSpriteRatio([NotNull] UISprite gaugeSprite, float ratio)
	{
		int num = (int)(gaugeSprite.fillAmount * (float)gaugeSprite.width);
		int num2 = (int)((float)gaugeSprite.width * ratio);
		if (num != num2)
		{
			gaugeSprite.fillAmount = ratio;
		}
	}
}
