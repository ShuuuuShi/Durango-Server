using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Encyclopedia;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class FarmingEncyclopediaPopup : TooltipBase
{
	[CompilerGenerated]
	private sealed class _003CCoProgressAnimation_003Ed__21 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public FarmingEncyclopediaPopup _003C_003E4__this;

		private float _003Cratio_003E5__2;

		private float _003CspeedRatio_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoProgressAnimation_003Ed__21(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			FarmingEncyclopediaPopup farmingEncyclopediaPopup = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Cratio_003E5__2 = 0f;
				_003CspeedRatio_003E5__3 = 0f;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (_003Cratio_003E5__2 < farmingEncyclopediaPopup._expRatio)
			{
				farmingEncyclopediaPopup._progressSprite.fillAmount = _003Cratio_003E5__2;
				_003CspeedRatio_003E5__3 += Time.deltaTime * 2f;
				_003Cratio_003E5__2 += Time.deltaTime * _003CspeedRatio_003E5__3;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			farmingEncyclopediaPopup._progressSprite.fillAmount = farmingEncyclopediaPopup._expRatio;
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _infoLabel;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private UISprite _progressSprite;

	[SerializeField]
	private UIWidget _masteriesWidget;

	[SerializeField]
	private KScrollView _masteries;

	private string _key;

	private bool _reset = true;

	private float _expRatio;

	public override bool DragLock => true;

	protected override void OnAwake()
	{
		base.OnAwake();
		_titleLabel.text = T._("작물 정보");
		UIEventListener uIEventListener = UIEventListener.Get(_infoLabel.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			UIWidget childSprite = UIUtility.GetChildSprite(_infoLabel, "img_loading_unknown_question1");
			if (!(childSprite == null))
			{
				WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
				widgetTooltipControl.Set(null, T._("종자 레벨에 따라 필요한 물, 비료 및 성장 시간이 달라집니다."), 400);
				widgetTooltipControl.AutoPosition = false;
				widgetTooltipControl.Show(10f);
				widgetTooltipControl.SetPosition(childSprite, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 20f));
			}
		});
		GameSystem<FarmingEncyclopediaSystem>.Instance().FarmingDataUpdated += OnFarmingDataUpdate;
		_masteries.Nodes.Init(delegate(GameObject obj)
		{
			obj.GetComponent<FarmingMasteryWidget>().MasterySelected += OnSelectMastery;
		});
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		StartCoroutine(CoProgressAnimation());
	}

	protected override void OnHide()
	{
		base.OnHide();
		_reset = true;
	}

	private void OnSelectMastery(int level, int index)
	{
		FarmingEncyclopediaData? farmingEncyclopediaData = GameSystem<FarmingEncyclopediaSystem>.Instance().GetFarmingEncyclopediaData(_key);
		if (!farmingEncyclopediaData.HasValue)
		{
			return;
		}
		FarmingEncyclopediaData value = farmingEncyclopediaData.Value;
		if (value.MasteryLevelToIndex != null && value.MasteryLevelToIndex.TryGetValue(level, out var value2))
		{
			if (index == value2)
			{
				return;
			}
			EncyclopediaItem encyclopediaItem = EncyclopediaItems.Get(EncyclopediaType.Farming, _key);
			CropInfo cropInfo = SingletonDict<string, CropInfo>.Get(_key);
			KeyValuePair<string, float>[][] array = encyclopediaItem?.GetMasteryModifiers(level);
			int num = ((array != null) ? KUtility.GetSize(array) : 0);
			KeyValuePair<string, float> keyValuePair = ((value2 >= num) ? default(KeyValuePair<string, float>) : array[value2].FirstOrDefault());
			KeyValuePair<string, float> keyValuePair2 = ((index >= num) ? default(KeyValuePair<string, float>) : array[index].FirstOrDefault());
			EncyclopediaModifiers encyclopediaModifiers = ((!string.IsNullOrEmpty(keyValuePair.Key)) ? SingletonDict<string, EncyclopediaModifiers>.Get(keyValuePair.Key) : null);
			EncyclopediaModifiers encyclopediaModifiers2 = ((!string.IsNullOrEmpty(keyValuePair2.Key)) ? SingletonDict<string, EncyclopediaModifiers>.Get(keyValuePair2.Key) : null);
			Yaml.Cost encyclopediaMasterySwap = Yaml.Util.Singleton<CostsYaml>.Instance.EncyclopediaMasterySwap;
			MessageBox messageBox = UIManager.MessageBox;
			messageBox.AddKeyValueInfo(string.Format("{0} [preset=animation_arrow] <em>{1}</em>", (encyclopediaModifiers != null) ? string.Format("{0} {1}", encyclopediaModifiers.Name, encyclopediaModifiers.GetValueString(keyValuePair.Value, null, "[icon=img_pet_arrow_up] {0}", "[icon=img_pet_arrow_down] {0}")) : keyValuePair.Key, (encyclopediaModifiers2 != null) ? string.Format("{0} {1}", encyclopediaModifiers2.Name, encyclopediaModifiers2.GetValueString(keyValuePair2.Value, null, "[icon=img_pet_arrow_up] {0}", "[icon=img_pet_arrow_down] {0}")) : keyValuePair2.Key), null);
			messageBox.ShowCostConfirm(encyclopediaMasterySwap, T._("{0} {1:lv:} 작물 특성을 변경합니다.", (cropInfo != null) ? cropInfo.Name.ToString() : _key, level), null, delegate(bool ok)
			{
				if (ok)
				{
					FarmingEncyclopediaSystem.ChangeFarmingEncyclopediaMastery(_key, level, index, isSelect: false);
				}
			});
		}
		else
		{
			if (level > value.CurrentLevel)
			{
				return;
			}
			CropInfo cropInfo2 = SingletonDict<string, CropInfo>.Get(_key);
			KeyValuePair<string, float>[][] array2 = EncyclopediaItems.Get(EncyclopediaType.Farming, _key)?.GetMasteryModifiers(level);
			int num2 = ((array2 != null) ? KUtility.GetSize(array2) : 0);
			KeyValuePair<string, float> keyValuePair3 = ((index >= num2) ? default(KeyValuePair<string, float>) : array2[index].FirstOrDefault());
			EncyclopediaModifiers encyclopediaModifiers3 = ((!string.IsNullOrEmpty(keyValuePair3.Key)) ? SingletonDict<string, EncyclopediaModifiers>.Get(keyValuePair3.Key) : null);
			MessageBox messageBox2 = UIManager.MessageBox;
			messageBox2.AddKeyValueInfo((encyclopediaModifiers3 != null) ? string.Format("{0} {1}", encyclopediaModifiers3.Name, encyclopediaModifiers3.GetValueString(keyValuePair3.Value, null, "[icon=img_pet_arrow_up] {0}", "[icon=img_pet_arrow_down] {0}")) : keyValuePair3.Key, null);
			messageBox2.Show(T._("{0} {1:lv:} 작물 특성을 선택합니다.", (cropInfo2 != null) ? cropInfo2.Name.ToString() : _key, level), null, delegate(bool ok)
			{
				if (ok)
				{
					FarmingEncyclopediaSystem.ChangeFarmingEncyclopediaMastery(_key, level, index, isSelect: true);
				}
			});
		}
	}

	private void OnFarmingDataUpdate(string key, FarmingEncyclopediaData? prev, FarmingEncyclopediaData data)
	{
		if (base.IsVisible && !(_key != key))
		{
			MarkAsChanged();
		}
	}

	public void Set(string key)
	{
		_key = key;
	}

	protected override void FillData()
	{
		if (string.IsNullOrEmpty(_key))
		{
			return;
		}
		FarmingEncyclopediaData? farmingEncyclopediaData = GameSystem<FarmingEncyclopediaSystem>.Instance().GetFarmingEncyclopediaData(_key);
		if (!farmingEncyclopediaData.HasValue)
		{
			return;
		}
		FarmingEncyclopediaData value = farmingEncyclopediaData.Value;
		CropInfo cropInfo = SingletonDict<string, CropInfo>.Get(_key);
		EncyclopediaItem encyclopediaItem = EncyclopediaItems.Get(EncyclopediaType.Farming, _key);
		bool flag = value.CurrentLevel == 0;
		bool flag2 = encyclopediaItem != null && value.CurrentLevel >= encyclopediaItem.MaxLevel;
		int num = value.NextLevelExpThreshold - value.CurrentLevelExpThreshold;
		int num2 = value.CurrentExp - value.CurrentLevelExpThreshold;
		_expRatio = ((num <= 0) ? 0f : ((float)num2 / (float)num));
		_progressSprite.fillAmount = _expRatio;
		_progressSprite.color = ((!flag2) ? ((Color)new Color32(59, 96, 123, byte.MaxValue)) : PresetColor.UIYellow);
		string text = ((num <= 0) ? "<em>{0}</em>  <weak>[size=*0.7]{1:lv:}" : "<em>{0}</em>  <weak>[size=*0.7]{1:lv:}  <bar/>  {2:N0} / {3:N0}[/size]</weak>");
		if (cropInfo == null)
		{
			_nameLabel.text = T._(text, _key, value.CurrentLevel, num2, num);
			_infoLabel.text = null;
			_descriptionLabel.text = null;
			_iconTexture.SetIcon(string.Empty);
		}
		else
		{
			_nameLabel.text = T._(text, cropInfo.Name, value.CurrentLevel, num2, num);
			_descriptionLabel.text = $"<weak>[icon=icon_popup_player_note]</weak> {cropInfo.Description}";
			if (flag)
			{
				_iconTexture.HideShadow = true;
				_iconTexture.SetIcon(cropInfo.Icon, new ItemColor(new Color(0f, 0f, 0f, 0.75f)));
			}
			else
			{
				_iconTexture.HideShadow = false;
				_iconTexture.SetIcon(cropInfo.Icon, cropInfo.ColorR, cropInfo.ColorG, cropInfo.ColorB);
			}
			using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
			StringBuilder value2 = reusable.Value;
			bool flag3 = false;
			if (cropInfo.TryGetRequiredWaterRange(out var min, out var max))
			{
				if (value2.Length > 0)
				{
					value2.Append("   <bar/>   ");
				}
				value2.Append("[9bd2ec][icon=encyclopedia_info_1][-] ");
				if (min < max)
				{
					value2.AppendFormat("{0:0}~{1:0}", min, max);
					flag3 = true;
				}
				else
				{
					value2.AppendFormat("{0:0}", min);
				}
			}
			if (cropInfo.TryGetRequiredFertilizerRange(out min, out max))
			{
				if (value2.Length > 0)
				{
					value2.Append("   <bar/>   ");
				}
				value2.Append("[af8a59][icon=encyclopedia_info_2][-] ");
				if (min < max)
				{
					value2.AppendFormat("{0:0}~{1:0}", min, max);
					flag3 = true;
				}
				else
				{
					value2.AppendFormat("{0:0}", min);
				}
			}
			if (cropInfo.TryGetGrowsUntillRange(out min, out max))
			{
				if (value2.Length > 0)
				{
					value2.Append("   <bar/>   ");
				}
				value2.Append("[icon=encyclopedia_info_3]  ");
				if (min < max)
				{
					value2.AppendFormat("{0}~{1}", TimedeltaFormatter.Format(min), TimedeltaFormatter.Format(max));
					flag3 = true;
				}
				else
				{
					value2.Append(TimedeltaFormatter.Format(min));
				}
			}
			string text2 = null;
			if (value2.Length > 0)
			{
				text2 = ((!flag3) ? $"[preset=round_box?   {value2}   ]" : string.Format("[preset=round_box?   {0}   ]  <em><help>{1}</help></em>", value2, T._("종자 레벨에 따라 필요한 물, 비료 및 성장 시간이 달라집니다.")));
			}
			_infoLabel.text = text2;
		}
		_masteries.Nodes.BeginLoad();
		KeyValuePair<int, KeyValuePair<string, float>[][]>[] array = encyclopediaItem?.GetMasteryModifiersList();
		if (array != null)
		{
			KeyValuePair<int, KeyValuePair<string, float>[][]>[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				KeyValuePair<int, KeyValuePair<string, float>[][]> keyValuePair = array2[i];
				_masteries.Nodes.GetNext().GetComponent<FarmingMasteryWidget>().Set(keyValuePair.Key, value, keyValuePair.Value);
			}
		}
		_masteries.Nodes.EndLoad();
	}

	protected override void UpdateLayout()
	{
		RectLayoutComponent component = GetComponent<RectLayoutComponent>();
		int safeWidth = UIManager.SafeWidth;
		int safeHeight = UIManager.SafeHeight;
		safeWidth = Mathf.Min(safeWidth - 60, 740);
		safeHeight -= 20;
		component.UpdateLayout(safeWidth, safeHeight);
		_masteries.UpdateLayout();
		float num = (float)_masteriesWidget.height - _masteries.ContentsLength;
		if (num > 0f)
		{
			component.UpdateLayout(safeWidth, (float)safeHeight - num);
		}
		if (_reset)
		{
			_masteries.MoveTo(0f, instant: true);
		}
		else
		{
			_masteries.MoveTo(_masteries.CurrentOffset, instant: false);
		}
		_reset = false;
	}

	private IEnumerator CoProgressAnimation()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoProgressAnimation_003Ed__21(0)
		{
			_003C_003E4__this = this
		};
	}
}
