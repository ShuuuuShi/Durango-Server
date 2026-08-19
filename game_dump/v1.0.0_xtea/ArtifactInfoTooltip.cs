using System.Collections.Generic;
using System.Text;
using ItemSystem;
using L10N;
using Messages;
using Player;
using TimerData;
using UnityEngine;

[RequireComponent(typeof(InfoTooltip))]
public class ArtifactInfoTooltip : MonoBehaviour
{
	private InfoTooltip _tooltip;

	private string _title;

	private IList<TagData> _tags;

	private ArtifactState _artifactStates;

	private float _timeLabelUpdateAt;

	private string _helperNames;

	private string _residentNames;

	private void Update()
	{
		if (_timeLabelUpdateAt > 0f && _timeLabelUpdateAt < Time.time)
		{
			UpdateTimerLabel();
		}
	}

	private void Set(ItemSystem.ArtifactCapsule capsule)
	{
		_title = GameSystem<RecipeSystem>.Instance().GetBlueprint(capsule.BlueprintId).LocalizedName;
		_tags = capsule.Tags;
		_artifactStates = default(ArtifactState);
		_helperNames = null;
		_residentNames = null;
		Show();
	}

	private void Set(Artifact artifact)
	{
		_title = artifact.GetName();
		_tags = artifact.Tags;
		_artifactStates = artifact.ArtifactState;
		_helperNames = null;
		_residentNames = null;
		Show();
	}

	private void Show()
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		_tooltip = ((Component)this).GetComponent<InfoTooltip>();
		Refresh();
		_tooltip.AddOnFinished(OnFinish);
		_tooltip.AutoPosition = false;
		_tooltip.Show(3600f);
		UIWidget rootAnchor = UIManager.GetRootAnchor(UIBase.AnchorType.Base);
		if (UIManager.IsPortraitMode)
		{
			Vector3 position = rootAnchor.GetPosition(0f, 1f);
			position.x += 10f;
			position.y -= 120f;
			_tooltip.Widget.SetPosition(position, 0f, 1f);
		}
		else
		{
			Vector3 position2 = rootAnchor.GetPosition(0f, 0.5f);
			position2.x += 10f;
			_tooltip.Widget.SetPosition(position2, 0f, 0.5f);
		}
	}

	private void OnFinish()
	{
		Object.Destroy((Object)(object)this);
	}

	private void Refresh()
	{
		_tooltip.SetTitle(_title);
		UpdateTimerLabel();
		int num = 0;
		for (int i = 0; i < _tags.Count; i++)
		{
			TagData tagData = _tags[i];
			_tooltip.SetInfo(num++, tagData.LocalizedName, T._("{0:lv:}", tagData.Level));
		}
		if (_artifactStates.Postprocess.HasValue && _artifactStates.Postprocess.Value.MaxHelperCount > 0)
		{
			Postprocess value = _artifactStates.Postprocess.Value;
			GetPostprocessInfo(value, out var key, out var value2);
			if (KUtility.GetSize(value.Helpers) > 0)
			{
				if (_helperNames == null)
				{
					KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfos(value.Helpers, ResponseHelpersInfo, useOldCache: true);
				}
				else
				{
					value2 = $"{value2}\n{_helperNames}".Trim();
				}
			}
			_tooltip.SetInfo(num++, key, value2);
		}
		if (_artifactStates.Farming.HasValue)
		{
			GetFarmingInfo(_artifactStates.Farming.Value, out var key2, out var value3);
			_tooltip.SetInfo(num++, key2, value3);
		}
		if (_artifactStates.Home.HasValue)
		{
			Home value4 = _artifactStates.Home.Value;
			GetHomeInfo(value4, out var key3, out var value5);
			if (KUtility.GetSize(value4.ResidentEntityIds) > 0)
			{
				if (_residentNames == null)
				{
					KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfos(value4.ResidentEntityIds, ResponseResidentsInfo, useOldCache: true);
				}
				else
				{
					value5 = $"{value5}\n{_residentNames}".Trim();
				}
			}
			_tooltip.SetInfo(num++, key3, value5);
		}
		if (!_artifactStates.Crack.HasValue)
		{
			return;
		}
		Crack value6 = _artifactStates.Crack.Value;
		double bufferedServerTime_Enhanced = Connections.Frontend.GetBufferedServerTime_Enhanced();
		double? activatedSince = value6.ActivatedSince;
		if (activatedSince.HasValue && !(value6.ActivatedSince.Value > bufferedServerTime_Enhanced))
		{
			double? activatedUntil = value6.ActivatedUntil;
			if (!activatedUntil.HasValue || !(activatedUntil.Value <= bufferedServerTime_Enhanced))
			{
				return;
			}
		}
		_tooltip.SetInfo(num++, T._("활성화에 필요한 티스톤"), T._("{0}/{1}", value6.CurrentInvestment, value6.RequiredInvestment));
	}

	private void UpdateTimerLabel()
	{
		string subtitle = string.Empty;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		KeyValuePair<double, double>? repairement = _artifactStates.Repairement;
		if (repairement.HasValue && _artifactStates.Repairement.Value.Value > predictedServerTime)
		{
			subtitle = T._("[icon_skill_time] 수리 중");
			_timeLabelUpdateAt = Time.time + 60f;
		}
		else if (_artifactStates.Durability != null)
		{
			double num = _artifactStates.Durability.When(0f, predictedServerTime);
			double num2 = num - predictedServerTime;
			_timeLabelUpdateAt = Time.time + 60f;
			if (num2 > 60.0)
			{
				string arg = TimerSystem.TimeToString(num2, TimePeriod.Min, 2);
				subtitle = $"[icon_skill_time] {arg}";
			}
			else if (_artifactStates.Durability.Get() < 1f)
			{
				if (num2 > 0.0)
				{
					subtitle = T._("곧 파괴됨");
				}
				else
				{
					subtitle = T._("파괴됨");
					_timeLabelUpdateAt = 0f;
				}
			}
		}
		_tooltip.SetSubtitle(subtitle);
	}

	private void GetPostprocessInfo(Postprocess postprocess, out string key, out string value)
	{
		key = T._("마무리 참여 인원");
		value = ((postprocess.Helpers.Length < postprocess.MaxHelperCount) ? T._("{0} / {1}명", postprocess.Helpers.Length, postprocess.MaxHelperCount) : T._("[9E0B0F]{0} / {1}명[-]", postprocess.Helpers.Length, postprocess.MaxHelperCount));
	}

	private void GetFarmingInfo(Farming farming, out string key, out string value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder.AppendLine(T._("작물"));
		stringBuilder2.AppendLine(farming.PlantName);
		double num = farming.GrowsUntil - Connections.Frontend.GetPredictedServerTime();
		string value2;
		if (num > 0.0)
		{
			value2 = TimerSystem.TimeToString(num, TimePeriod.Min, 1);
			if (string.IsNullOrEmpty(value2))
			{
				value2 = T._("곧 수확 가능");
			}
		}
		else
		{
			value2 = T._("수확 가능");
		}
		stringBuilder.AppendLine(T._("수확까지"));
		stringBuilder2.AppendLine(value2);
		stringBuilder.AppendLine(T._("필요 물의 양"));
		stringBuilder2.AppendLine((farming.Water.Key >= farming.Water.Value) ? T._("충분") : (farming.Water.Value - farming.Water.Key).ToString());
		stringBuilder.AppendLine(T._("기후 적합성"));
		stringBuilder2.AppendLine(LocalizeUtil.Get(farming.BiomeFitness));
		stringBuilder.AppendLine(T._("비옥도"));
		stringBuilder2.AppendLine($"{(int)(farming.Fertilized * 100f)}%");
		stringBuilder.AppendLine(T._("퇴비"));
		if (farming.SurplusFertilizer > 0)
		{
			stringBuilder2.AppendLine(T._("{0} (잉여량: {1})", farming.Fertilizer, farming.SurplusFertilizer));
		}
		else
		{
			stringBuilder2.AppendLine(farming.Fertilizer.ToString());
		}
		key = stringBuilder.ToString().Trim();
		value = stringBuilder2.ToString().Trim();
	}

	private void GetHomeInfo(Home home, out string key, out string value)
	{
		key = T._("수용 인원");
		value = ((home.ResidentEntityIds.Length < home.Capacity) ? $"{home.ResidentEntityIds.Length} / {home.Capacity}" : $"[9E0B0F]{home.ResidentEntityIds.Length} / {home.Capacity}[-]");
	}

	private void ResponseHelpersInfo(Player.PlayerInfo[] playerInfos)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < playerInfos.Length; i++)
		{
			stringBuilder.AppendLine((!playerInfos[i].Valid) ? T._("알수없음") : playerInfos[i].Name);
		}
		_helperNames = stringBuilder.ToString().Trim();
	}

	private void ResponseResidentsInfo(Player.PlayerInfo[] playerInfos)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < playerInfos.Length; i++)
		{
			stringBuilder.AppendLine((!playerInfos[i].Valid) ? T._("알수없음") : playerInfos[i].Name);
		}
		_residentNames = stringBuilder.ToString().Trim();
	}

	public static void Show(InfoTooltip tooltip, Artifact artifact)
	{
		if (!((Object)(object)artifact == (Object)null))
		{
			ArtifactInfoTooltip artifactInfoTooltip = ((Component)tooltip).gameObject.AddComponent<ArtifactInfoTooltip>();
			artifactInfoTooltip.Set(artifact);
		}
	}

	public static void Show(InfoTooltip tooltip, ItemSystem.ArtifactCapsule capsule)
	{
		if (capsule != null)
		{
			ArtifactInfoTooltip artifactInfoTooltip = ((Component)tooltip).gameObject.AddComponent<ArtifactInfoTooltip>();
			artifactInfoTooltip.Set(capsule);
		}
	}
}
