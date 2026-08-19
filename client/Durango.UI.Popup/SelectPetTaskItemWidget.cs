using System;
using System.Collections.Generic;
using System.Text;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Ability;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class SelectPetTaskItemWidget : MonoBehaviour
{
	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _costLabel;

	[SerializeField]
	private UILabel _rewardInfoLabel;

	[SerializeField]
	private SelectableButton _selectButton;

	[SerializeField]
	private UIWidget _rewardsWidget;

	[SerializeField]
	private UILabel _rewardLabel;

	[SerializeField]
	private SelectPetTaskRewardItemWidget _rewardItemBase;

	[SerializeField]
	private UIWidget _rewardPlusWidget;

	private RectLayoutComponent _layout;

	private string _petTaskId;

	private readonly List<UIWidget> _rewardWidgets = new List<UIWidget>();

	private ListObjectPool<SelectPetTaskRewardItemWidget> _rewardItemWidgets;

	private bool _isInit;

	public event Action<string> Clicked;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		SelectableButton selectButton = _selectButton;
		selectButton.Clicked = (Action)Delegate.Combine(selectButton.Clicked, (Action)delegate
		{
			if (this.Clicked != null)
			{
				this.Clicked(_petTaskId);
			}
		});
		_layout = GetComponent<RectLayoutComponent>();
		_rewardLabel.text = T._("생산 가능");
		_rewardItemWidgets = new ListObjectPool<SelectPetTaskRewardItemWidget>();
		_rewardItemWidgets.BaseObject = _rewardItemBase;
		_rewardItemWidgets.UseBase = true;
		_layout.UpdateOnSizeChange();
	}

	public void Set(Messages.Pet pet, string taskId)
	{
		Init();
		_petTaskId = taskId;
		PetTask petTask = SingletonDict<string, PetTask>.Get(taskId);
		if (petTask == null)
		{
			return;
		}
		float quantity = pet.Statistics.DerivedAbilities.Get(Derived.AnimalProductQuantity, 0f);
		_iconSprite.spriteName = petTask.Icon;
		_nameLabel.text = petTask.Name.ToString();
		_costLabel.text = $"{TimedeltaFormatter.Format(petTask.Duration)}  [51A3C3][icon=pet_energy][-] {petTask.HungryRequired:0}";
		using (Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop())
		{
			StringBuilder value = reusable.Value;
			if (petTask.Exp > 0)
			{
				if (value.Length > 0)
				{
					value.Append("  ");
				}
				value.Append("[icon=icon_exp_pet] ").Append(petTask.Exp);
			}
			int productQuantity = petTask.GetProductQuantity(quantity);
			if (productQuantity > 0)
			{
				if (value.Length > 0)
				{
					value.Append("  ");
				}
				value.Append("[icon=bg_itemview_durability_white] ").Append(productQuantity);
			}
			_rewardInfoLabel.text = value.ToString();
		}
		FillRewards(pet, petTask);
		if (pet.Statistics.Level < petTask.UnlockLevel)
		{
			_selectButton.Text = T._("{0:lv:} 이상", petTask.UnlockLevel);
			_selectButton.Disabled = true;
		}
		else
		{
			_selectButton.Text = T._("시작");
			_selectButton.Disabled = false;
		}
		_layout.UpdateLayout();
	}

	private void FillRewards(Messages.Pet pet, [NotNull] PetTask task)
	{
		if (KUtility.GetSize(task.ProducedPrototype) + KUtility.GetSize(task.RandomPrototype) == 0)
		{
			_rewardsWidget.gameObject.SetActive(value: false);
			return;
		}
		_rewardsWidget.gameObject.SetActive(value: true);
		_rewardWidgets.Clear();
		_rewardItemWidgets.BeginLoad();
		bool flag = false;
		int level = pet.Statistics.Level;
		if (task.ProducedPrototype != null)
		{
			foreach (KeyValuePair<string, float[]> item in task.ProducedPrototype)
			{
				SelectPetTaskRewardItemWidget next = _rewardItemWidgets.GetNext();
				next.Set(new KeyValuePair<string, int>(item.Key, level), isBonus: false);
				_rewardWidgets.Add(next.GetComponent<UIWidget>());
			}
		}
		if (task.RandomPrototype != null)
		{
			foreach (KeyValuePair<string, float[]> item2 in task.RandomPrototype)
			{
				if (!flag)
				{
					flag = true;
					_rewardWidgets.Add(_rewardPlusWidget);
				}
				SelectPetTaskRewardItemWidget next2 = _rewardItemWidgets.GetNext();
				next2.Set(new KeyValuePair<string, int>(item2.Key, level), isBonus: true);
				_rewardWidgets.Add(next2.GetComponent<UIWidget>());
			}
		}
		_rewardItemWidgets.EndLoad();
		_rewardPlusWidget.gameObject.SetActive(flag);
		if (_rewardWidgets.Count != 0)
		{
			UIWidget component = _rewardWidgets[0].transform.parent.GetComponent<UIWidget>();
			_rewardWidgets.Reverse();
			UIUtility.WidgetsReposition(_rewardWidgets, component, Vector3.left, 10f);
		}
	}
}
