using System;
using System.Collections.Generic;
using AutoGuide;
using ItemSystem;
using K1Network;
using Messages;
using PlayGuide;
using Shared.Guide;
using StatisticsData;
using UnityEngine;

public class AutoGuideSystem : GameSystem<AutoGuideSystem>
{
	private const string NewPrefKey = "new_auto_guide_";

	private const int MaxOffers = 3;

	private readonly List<Template> _templates = new List<Template>();

	private readonly bool[] _isNewTemplate = new bool[3];

	private Dictionary<string, float> _achievementRatioDict;

	private bool _waitAdvisorTargets;

	private bool _waitOffers;

	public bool IsWaitingResponse => _waitOffers || _waitAdvisorTargets;

	public int Progress { get; private set; }

	public StatisticsData.Title TargetTitle { get; private set; }

	public event Action TemplateUpdated;

	public event Action ProgressUpdated;

	public List<Template> GetTemplates()
	{
		return _templates;
	}

	public int GetNewTemplateCount()
	{
		int num = 0;
		for (int i = 0; i < _templates.Count; i++)
		{
			if (_isNewTemplate[i])
			{
				num++;
			}
		}
		return num;
	}

	public void UpdateAchievementRatio()
	{
		_waitAdvisorTargets = true;
		Connections.Frontend.Send(default(GetAdvisorTargets));
	}

	public float GetAchievementRatio(string title)
	{
		if (_achievementRatioDict == null)
		{
			return 0f;
		}
		return _achievementRatioDict.Get(title, 0f);
	}

	public void SelectTitle(string titleId)
	{
		SelectTargetTitle msg = default(SelectTargetTitle);
		msg.TitleId = titleId;
		Connections.Frontend.Send(msg);
		for (int i = 0; i < _isNewTemplate.Length; i++)
		{
			SetIsNew((OfferType)i, isNew: true, invokeEvent: false);
		}
	}

	public void CancelTemplate(OfferType key)
	{
		ReissueOffer msg2 = default(ReissueOffer);
		msg2.OfferType = key;
		Connections.Frontend.Send(msg2).On(delegate(Offers msg, PacketHeader header)
		{
			OffersReceived(msg, header);
		});
		SetIsNew(key, isNew: true, invokeEvent: false);
	}

	public void SetGuided(OfferType key, bool guided)
	{
		MonitorOffer msg = default(MonitorOffer);
		msg.OfferType = key;
		msg.Monitoring = guided;
		Connections.Frontend.Send(msg);
		SetIsNew(key, isNew: false);
	}

	public void DoAction(Template template)
	{
		SetIsNew(template.Key, isNew: false);
		ToDoBase toDo = template.GetToDo();
		if (toDo != null)
		{
			MarketGroup marketGroup = UIManager.FindScript<MarketGroup>();
			TagFilter[] array = null;
			TagFilter[] materials = null;
			if (toDo is GetSlotItemToDo getSlotItemToDo)
			{
				array = getSlotItemToDo.RequiredTags;
				materials = getSlotItemToDo.RequiredMaterials;
			}
			if (toDo is GetItemToDo getItemToDo)
			{
				array = getItemToDo.RequiredTags;
			}
			if (array != null)
			{
				marketGroup.OpenAndSearch(array, materials);
			}
			else
			{
				toDo.OnClicked();
			}
		}
	}

	public void SetLastSelected(ToDoBase todo)
	{
		for (int i = 0; i < _templates.Count; i++)
		{
			Template template = _templates[i];
			template.LastSelected = _templates[i].GetToDo() == todo;
		}
	}

	public bool GetIsNew(OfferType type)
	{
		return _isNewTemplate[(int)type];
	}

	public void SetIsNew(OfferType type, bool isNew, bool invokeEvent = true)
	{
		if (_isNewTemplate[(int)type] != isNew)
		{
			_isNewTemplate[(int)type] = isNew;
			PlayerPrefs.SetInt("new_auto_guide_" + (int)type, isNew ? 1 : 0);
			PlayerPrefs.Save();
			if (invokeEvent && this.TemplateUpdated != null)
			{
				this.TemplateUpdated();
			}
		}
	}

	public bool IsGuided(string recipeId)
	{
		for (int i = 0; i < _templates.Count; i++)
		{
			string text = string.Empty;
			Template template = _templates[i];
			switch (template.Type)
			{
			case TemplateType.Build:
				text = ((BuildGoal)template.Goal).BlueprintId;
				break;
			case TemplateType.Craft:
				text = ((CraftGoal)template.Goal).RecipeId;
				break;
			}
			if (text == recipeId)
			{
				return true;
			}
		}
		return false;
	}

	private void Awake()
	{
		Connections.Frontend.On<AdvisorTargets>(AdvisorTargetsReceived);
		Connections.Frontend.On<Offers>(OffersReceived);
		Connections.Frontend.On<UpdateOffer>(UpdateOfferReceived);
		Connections.Frontend.On<OfferCompleted>(OfferCompletedReceived);
		Connections.Frontend.On<UpdateAdvisorProgress>(UpdateProgress);
		KSingleton<GameManager>.Instance().Ready += GameManager_Ready;
		for (int i = 0; i < 3; i++)
		{
			_isNewTemplate[i] = PlayerPrefs.GetInt("new_auto_guide_" + i, 1) != 0;
		}
	}

	private void AdvisorTargetsReceived(AdvisorTargets msg, PacketHeader header)
	{
		_achievementRatioDict = msg.Titles;
		_waitAdvisorTargets = false;
		if (this.TemplateUpdated != null)
		{
			this.TemplateUpdated();
		}
	}

	private void OffersReceived(Offers msg, PacketHeader header)
	{
		for (int i = 0; i < _templates.Count; i++)
		{
			Template template = _templates[i];
			template.Destroy();
		}
		_templates.Clear();
		Dictionary<OfferType, TodoTemplate>.Enumerator enumerator = msg._Offers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			OfferType key = enumerator.Current.Key;
			TodoTemplate value = enumerator.Current.Value;
			AddTemplate(key, value);
		}
		DoUpdateProgress(msg.Progress);
		TargetTitle = GameSystem<StatisticsSystem>.Instance().GetTitle(msg.TargetTitleId);
		_waitOffers = false;
		if (this.TemplateUpdated != null)
		{
			this.TemplateUpdated();
		}
	}

	private void UpdateOfferReceived(UpdateOffer msg, PacketHeader header)
	{
		RemoveTemplate(msg.OfferType);
		AddTemplate(msg.OfferType, msg.Offer);
		if (this.TemplateUpdated != null)
		{
			this.TemplateUpdated();
		}
	}

	private void OfferCompletedReceived(OfferCompleted msg, PacketHeader header)
	{
		RemoveTemplate(msg.OfferType);
		TodoTemplate? newOffer = msg.NewOffer;
		if (newOffer.HasValue)
		{
			AddTemplate(msg.OfferType, msg.Offer);
			SetIsNew(msg.OfferType, isNew: true, invokeEvent: false);
		}
		if (this.TemplateUpdated != null)
		{
			this.TemplateUpdated();
		}
	}

	private void UpdateProgress(UpdateAdvisorProgress msg, PacketHeader header)
	{
		DoUpdateProgress(msg.Progress);
		if (this.ProgressUpdated != null)
		{
			this.ProgressUpdated();
		}
	}

	private void DoUpdateProgress(KeyValuePair<int, int> progress)
	{
		Progress = ((progress.Value != 0) ? (progress.Key * 100 / progress.Value) : 0);
	}

	private void GameManager_Ready()
	{
		_waitOffers = true;
		Connections.Frontend.Send(default(GetOffers));
	}

	private Template GetTemplate(OfferType key)
	{
		for (int i = 0; i < _templates.Count; i++)
		{
			Template template = _templates[i];
			if (template.Key == key)
			{
				return template;
			}
		}
		return null;
	}

	private void AddTemplate(OfferType key, TodoTemplate todo)
	{
		Template template = TemplateFactory.Create(key, todo);
		int num;
		for (num = _templates.Count; num >= 1; num--)
		{
			Template template2 = _templates[num - 1];
			if (template2.Key <= template.Key)
			{
				break;
			}
		}
		_templates.Insert(num, template);
	}

	private void RemoveTemplate(OfferType key)
	{
		for (int num = _templates.Count - 1; num >= 0; num--)
		{
			Template template = _templates[num];
			if (template.Key == key)
			{
				template.Destroy();
				_templates.RemoveAt(num);
			}
		}
	}
}
