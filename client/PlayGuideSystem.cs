using System;
using System.Collections.Generic;
using Durango.Logic.Clusters;
using Durango.Logic.Encyclopedia;
using Durango.Logic.PlayGuide;
using Durango.Network;
using Durango.System;
using Durango.Terrain;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using Shared.Faction;
using Shared.Guide;
using Shared.Region;
using UnityEngine;

public class PlayGuideSystem : GameSystem<PlayGuideSystem>
{
	public enum GuideConfig
	{
		StorageKey,
		EventFile,
		FlowFile
	}

	public class GuideStorageData
	{
		public Dictionary<string, List<string>> CompletedEvents = new Dictionary<string, List<string>>();

		public Vector2 LastDogPOITile;

		public DogGuideState LastDogGuideState;

		public int LastReturnerCount;

		public int LastNomadCount;
	}

	public const string NormalFlow = "normal_flow";

	private static readonly GuideEvent BlankEvent = new GuideEvent
	{
		Name = "blank",
		ToDoCollection = null
	};

	private static readonly GuideStorageData DefaultStorageData = new GuideStorageData();

	private GuideRole _currentGuideRole = GuideRole.Tutorial;

	private GuideRole _prevGuideRole = GuideRole.Invalid;

	private bool _isMyPersonalRegion;

	private readonly Dictionary<string, GuideEvent> _eventDictionary = new Dictionary<string, GuideEvent>();

	private readonly Dictionary<string, Flow> _flowDict = new Dictionary<string, Flow>();

	private readonly List<FlowCondition> _remainFlowConditions = new List<FlowCondition>();

	private readonly List<FlowStack> _flowStacks = new List<FlowStack>();

	private readonly Queue<GuideEvent> _delayedEventQueue = new Queue<GuideEvent>();

	private readonly List<string> _completedEvents = new List<string>();

	[NotNull]
	private GuideEvent _currentEvent = BlankEvent;

	private bool _isOverlappedEventAllowed = true;

	private bool _checkPersonalNormalFlow;

	private bool _fixActivatedFactions;

	private bool? _isReturner;

	private bool? _isNomad;

	private int _returnerCount;

	private int _lastReturnerCount;

	private int _nomadCount;

	private int _lastNomadCount;

	public bool IsGuideBegin { get; private set; }

	public CustomCommand Command { get; private set; }

	public bool Initialized { get; private set; }

	private bool Enabled => !AllCompleted && _eventDictionary.Count > 0;

	private bool AllCompleted
	{
		get
		{
			for (int i = 0; i < _flowStacks.Count; i++)
			{
				FlowStack flowStack = _flowStacks[i];
				if (!flowStack.Recoder.IsFinished())
				{
					return false;
				}
			}
			return _remainFlowConditions.Count == 0;
		}
	}

	public bool PauseUpdate { get; set; }

	public int LastQuizAnswer { get; private set; }

	private bool UseLocalGuideProgress => false;

	public event Action<GuideEvent, GuideEvent> EventChanged;

	public event Action<GuideRole, GuideRole> Begun;

	public event Action<bool> ReturnerUpdated;

	public event Action<GuideEvent> HelperTargetApplied;

	public event Action<GuideEvent> HelperTargetRemoved;

	public event Action<string, string> ExternalEventOccured;

	private Flow FindFlow(string flowName)
	{
		return _flowDict.Get(flowName);
	}

	private FlowStack FindFlowStack(string flowName)
	{
		for (int i = 0; i < _flowStacks.Count; i++)
		{
			if (_flowStacks[i].Name == flowName)
			{
				return _flowStacks[i];
			}
		}
		return null;
	}

	private FlowStack AddFlowStack(string flowName)
	{
		FlowStack flowStack = CreateFlowStack(flowName);
		if (flowStack == null)
		{
			return null;
		}
		_flowStacks.Add(flowStack);
		return flowStack;
	}

	private FlowStack CreateFlowStack(string flowName, Action finished = null)
	{
		Flow flow = FindFlow(flowName);
		if (flow == null)
		{
			return null;
		}
		return new FlowStack(flowName, flow, finished);
	}

	public GuideEvent GetCurrentEvent()
	{
		return _currentEvent;
	}

	public int GetFlowStackCount()
	{
		return _flowStacks.Count;
	}

	public FlowStack GetFlowStack(int index)
	{
		return _flowStacks[index];
	}

	private void ClearAll()
	{
		SetCurrentEvent(BlankEvent);
		GameSystem<ToDoListSystem>.Instance().RemoveAll();
		PauseUpdate = false;
		StopAllCoroutines();
		int count = _remainFlowConditions.Count;
		for (int i = 0; i < count; i++)
		{
			_remainFlowConditions[i].TryUnregister();
		}
		_remainFlowConditions.Clear();
		foreach (FlowStack flowStack in _flowStacks)
		{
			RemoveFlowRelated(flowStack);
		}
		_flowStacks.Clear();
		_eventDictionary.Clear();
		_flowDict.Clear();
		Command.ClearAll();
		_delayedEventQueue.Clear();
		_completedEvents.Clear();
		IsGuideBegin = false;
		_checkPersonalNormalFlow = false;
		_fixActivatedFactions = false;
	}

	private void Awake()
	{
		Command = new CustomCommand(this);
		Connections.Frontend.On(delegate(ReturnerInfo msg, PacketHeader header)
		{
			_isReturner = msg.IsReturner;
			_returnerCount = msg.ReturnerCount;
			CheckReturnerGuide();
			float num = (float)(msg.Until - Connections.Frontend.GetPredictedServerTime());
			if (num > 0f)
			{
				KUtility.DelayedCall(this, delegate
				{
					Connections.Frontend.Send(default(GetReturnerInfo));
				}, num);
			}
			if (this.ReturnerUpdated != null)
			{
				this.ReturnerUpdated(msg.IsReturner);
			}
		});
		Connections.Frontend.On(delegate(NomadInfo msg, PacketHeader header)
		{
			_isNomad = msg.IsNomad;
			_nomadCount = msg.NomadCount;
			CheckNomadGuide();
		});
		Singleton<GameManager>.Instance().WelcomeReceived += delegate(Welcome welcome)
		{
			bool myPersonalRegion = welcome.Region.Id == welcome.PersonalRegionId;
			Initialize(welcome.Region.Role, welcome.Storage.Data, myPersonalRegion);
		};
		Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			if (!GameManager.IsPrologueMode && Enabled)
			{
				Singleton<TerrainBase>.Instance().LoadingChunksFinished += TerrainA6_OnLoadingChunksFinished;
			}
		};
		Singleton<GameManager>.Instance().AddOnReady(delegate
		{
			Connections.Frontend.Send(default(GetReturnerInfo));
			Connections.Frontend.Send(default(GetNomadInfo));
		});
		GameSystem<ToDoListSystem>.Instance().Added += delegate(ToDoCollection collection, bool b)
		{
			ApplyHelperTarget(collection.GuideEvent);
		};
		GameSystem<FactionSystem>.Instance().FactionsUpdated += FactionSystem_FactionsUpdated;
	}

	private void FactionSystem_FactionsUpdated()
	{
		FactionSystem factionSystem = GameSystem<FactionSystem>.Instance();
		if (!factionSystem.IsFactionInitialized)
		{
			return;
		}
		if (_checkPersonalNormalFlow)
		{
			BeginPersonalNormalFlow();
			_checkPersonalNormalFlow = false;
		}
		if (_fixActivatedFactions)
		{
			return;
		}
		foreach (string completedEvent in _completedEvents)
		{
			GuideEvent guideEvent = _eventDictionary.Get(completedEvent);
			if (guideEvent != null && guideEvent.ActivateFaction != FactionType.Invalid && !factionSystem.IsFactionEnabled(guideEvent.ActivateFaction))
			{
				ActivateFaction(guideEvent.ActivateFaction);
			}
		}
		if (GameManager.Region.IsAfterSafeHouse() && !factionSystem.IsFactionEnabled(FactionType.TheFirm))
		{
			ActivateFaction(FactionType.TheFirm);
		}
		if (GameManager.Region.IsAfterRural() && !factionSystem.IsFactionEnabled(FactionType.Lama))
		{
			ActivateFaction(FactionType.Lama);
		}
		ActivateFaction(FactionType.SubStory);
		_fixActivatedFactions = true;
	}

	private void Update()
	{
		if (IsGuideBegin)
		{
			for (int num = _remainFlowConditions.Count - 1; num >= 0; num--)
			{
				_remainFlowConditions[num]?.Process();
			}
		}
	}

	public void ReloadAll()
	{
		ResetGuideProgressSaved();
		Initialize((Role)_currentGuideRole, null, _isMyPersonalRegion);
		TerrainA6_OnLoadingChunksFinished();
	}

	private void LoadPlayGuideFlow()
	{
		string guideConfig = GetGuideConfig(GuideConfig.FlowFile);
		Dictionary<string, FlowJson> dictionary = Json.ReadFromFile<Dictionary<string, FlowJson>>(guideConfig);
		if (dictionary == null)
		{
			Debug.LogError("Deserialize PlayGuideFlow failed: " + guideConfig);
			return;
		}
		_flowDict.Clear();
		_remainFlowConditions.Clear();
		if (_currentGuideRole != GuideRole.Personal || _isMyPersonalRegion)
		{
			LoadPlayGuideFlowJson(dictionary, _flowDict, _remainFlowConditions, common: false);
		}
		if (IsCommonGuideEnabled())
		{
			guideConfig = GetCommonGuideConfig(GuideConfig.FlowFile);
			dictionary = Json.ReadFromFile<Dictionary<string, FlowJson>>(guideConfig);
			if (dictionary == null)
			{
				Debug.LogError("Deserialize PlayGuideFlow failed: " + guideConfig);
			}
			else
			{
				LoadPlayGuideFlowJson(dictionary, _flowDict, _remainFlowConditions, common: true);
			}
		}
	}

	private bool IsCommonGuideEnabled()
	{
		switch (_currentGuideRole)
		{
		case GuideRole.Tutorial:
		case GuideRole.Safehouse:
		case GuideRole.Instance:
		case GuideRole.Offline:
		case GuideRole.Editable:
			return false;
		default:
			return true;
		}
	}

	public static void LoadPlayGuideFlowJson(Dictionary<string, FlowJson> flowJsons, Dictionary<string, Flow> dict, List<FlowCondition> conditions, bool common)
	{
		foreach (KeyValuePair<string, FlowJson> flowJson in flowJsons)
		{
			string key = flowJson.Key;
			FlowJson value = flowJson.Value;
			Flow value2 = new Flow(value.Flow, common);
			if (!dict.ContainsKey(key))
			{
				dict.Add(key, value2);
				FlowCondition flowCondition = FlowConditionFactory.Create(value, key);
				if (flowCondition != null)
				{
					conditions.Add(flowCondition);
				}
			}
		}
	}

	private void LoadPlayGuideEvent()
	{
		_eventDictionary.Clear();
		_eventDictionary.Add(BlankEvent.Name, BlankEvent);
		string guideConfig = GetGuideConfig(GuideConfig.EventFile);
		Dictionary<string, GuideEventJson> dictionary = Json.ReadFromFile<Dictionary<string, GuideEventJson>>(guideConfig);
		if (dictionary == null)
		{
			Debug.LogError("Deserialize PlayGuideEvent failed: " + guideConfig);
			return;
		}
		if (_currentGuideRole != GuideRole.Personal || _isMyPersonalRegion)
		{
			LoadPlayGuideEventJson(_eventDictionary, dictionary);
		}
		if (IsCommonGuideEnabled())
		{
			guideConfig = GetCommonGuideConfig(GuideConfig.EventFile);
			dictionary = Json.ReadFromFile<Dictionary<string, GuideEventJson>>(guideConfig);
			if (dictionary == null)
			{
				Debug.LogError("Deserialize PlayGuideEvent failed: " + guideConfig);
			}
			else
			{
				LoadPlayGuideEventJson(_eventDictionary, dictionary);
			}
		}
		else
		{
			PrepareVoiceEvents();
		}
	}

	public static void LoadPlayGuideEventJson(Dictionary<string, GuideEvent> events, Dictionary<string, GuideEventJson> guideDict)
	{
		NPCType prevNPCType = NPCType.TheFirm;
		foreach (KeyValuePair<string, GuideEventJson> item in guideDict)
		{
			string key = item.Key;
			if (!string.IsNullOrEmpty(key) && !events.ContainsKey(key))
			{
				GuideEventJson guideEventJson = item.Value;
				if (Platform.Instance.UsePCUI && item.Value.override_pc != null)
				{
					guideEventJson = guideEventJson.override_pc;
				}
				GuideEvent guideEvent = GuideEvent.Create(key, guideEventJson, prevNPCType);
				prevNPCType = guideEvent.NPCType;
				events.Add(guideEvent.Name, guideEvent);
			}
		}
	}

	private void PrepareVoiceEvents()
	{
		foreach (GuideEvent value in _eventDictionary.Values)
		{
			int size = KUtility.GetSize(value.Messages);
			for (int i = 0; i < size; i++)
			{
				string messageVoiceEventName = GetMessageVoiceEventName(value, i);
				if (SoundManager.HasEvent(messageVoiceEventName))
				{
					SoundManager.PrepareEvent(messageVoiceEventName);
					return;
				}
			}
		}
	}

	public static string GetMessageVoiceEventName(GuideEvent guideEvent, int line)
	{
		return $"playguide_{guideEvent.Name}_{line}";
	}

	public static string GetQuizAnswerVoiceEventName(GuideEvent guideEvent, int answer, int line)
	{
		return $"playguide_{guideEvent.Name}_quiz_{answer}_{line}";
	}

	private void ResetGuideProgressSaved()
	{
		Preferences.SetString(GetGuideConfig(GuideConfig.StorageKey), string.Empty);
		if (IsCommonGuideEnabled())
		{
			Preferences.SetString(GetCommonGuideConfig(GuideConfig.StorageKey), string.Empty);
		}
	}

	private void InitializeFlow(string flowName, [CanBeNull] List<string> progress, bool skipLoad, bool canRestart, FlowRegion region)
	{
		FlowStack flowStack = AddFlowStack(flowName);
		if (flowStack != null)
		{
			flowStack.Region = region;
			if (!skipLoad && !LoadGuideProgress(flowName, progress, flowStack) && canRestart)
			{
				_flowStacks.Remove(flowStack);
				InitializeFlow(flowName, null, skipLoad: false, canRestart: false, region);
			}
		}
	}

	private bool LoadGuideProgress(string flowName, [CanBeNull] List<string> progress, [NotNull] FlowStack flowStack)
	{
		GuideRecoder recoder = flowStack.Recoder;
		recoder.Load(progress);
		recoder.IsRecordingEnabled = _currentGuideRole != GuideRole.Tutorial || flowName != "normal_flow";
		string text = recoder.MoveNext();
		MoveToRecordingEnabled(flowStack, text);
		string text2 = null;
		while (text != null)
		{
			if (text2 != null)
			{
				_completedEvents.Add(text2);
			}
			string text3 = flowStack.MoveNext(canRecord: false);
			if (!(text3 == text))
			{
				recoder.RemoveRemains();
				recoder.Record(text3);
				_completedEvents.Add(text3);
				MoveToEnd(flowStack);
				return false;
			}
			text2 = text3;
			text = recoder.MoveNext();
		}
		return true;
	}

	private void MoveToEnd([NotNull] FlowStack flowStack)
	{
		while (true)
		{
			string text = flowStack.MoveNext();
			if (text == "blank")
			{
				break;
			}
			_completedEvents.Add(text);
		}
	}

	private static void MoveToRecordingEnabled([NotNull] FlowStack flowStack, string replay)
	{
		string text = string.Empty;
		while (replay != null && !flowStack.Recoder.IsRecordingEnabled && !(text == "blank"))
		{
			text = flowStack.MoveNext();
		}
	}

	private void SaveGuideProgress()
	{
		DoSaveGuideProgress(common: false);
		if (IsCommonGuideEnabled())
		{
			DoSaveGuideProgress(common: true);
		}
	}

	private void DoSaveGuideProgress(bool common)
	{
		GuideStorageData guideStorageData = CreateStorageData(common);
		string key = ((!common) ? GetGuideConfig(GuideConfig.StorageKey) : GetCommonGuideConfig(GuideConfig.StorageKey));
		if (UseLocalGuideProgress)
		{
			string value = Json.Write(guideStorageData);
			Preferences.SetString(key, value);
		}
		else
		{
			Connections.Frontend.Send(SetStorageItem(key, guideStorageData));
		}
	}

	private GuideStorageData CreateStorageData(bool common)
	{
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		for (int i = 0; i < _flowStacks.Count; i++)
		{
			FlowStack flowStack = _flowStacks[i];
			string text = flowStack.Name;
			if (IsCommonFlow(text) == common && !dictionary.ContainsKey(text))
			{
				dictionary.Add(text, flowStack.Recoder.GetFlows());
			}
		}
		GuideStorageData guideStorageData = new GuideStorageData();
		guideStorageData.CompletedEvents = dictionary;
		GuideStorageData guideStorageData2 = guideStorageData;
		if (!common)
		{
			Command.SaveDogGuideProgress(guideStorageData2);
			guideStorageData2.LastReturnerCount = _lastReturnerCount;
			guideStorageData2.LastNomadCount = _lastNomadCount;
		}
		return guideStorageData2;
	}

	private static SetStorageItem SetStorageItem<TK>(string key, TK value)
	{
		SetStorageItem result = default(SetStorageItem);
		result.Key = key;
		result.Value = Json.WriteToBytes(value);
		return result;
	}

	private GuideStorageData LoadGuideStorageData(string key, [CanBeNull] Dictionary<string, byte[]> storage)
	{
		GuideStorageData guideStorageData = null;
		byte[] value;
		if (UseLocalGuideProgress)
		{
			string @string = Preferences.GetString(key, string.Empty);
			guideStorageData = Json.Read<GuideStorageData>(@string);
		}
		else if (storage != null && storage.TryGetValue(key, out value) && value.Length != 0)
		{
			guideStorageData = Json.Read<GuideStorageData>(value);
		}
		if (guideStorageData == null)
		{
			guideStorageData = DefaultStorageData;
		}
		return guideStorageData;
	}

	public void Initialize(Role type, [CanBeNull] Dictionary<string, byte[]> storage, bool myPersonalRegion)
	{
		ClearAll();
		Initialized = true;
		if (type == Role.Invalid || type == Role.Sandbox)
		{
			return;
		}
		switch (GameManager.ClusterMode)
		{
		case Mode.Editable:
			_currentGuideRole = GuideRole.Editable;
			break;
		case Mode.Offline:
			_currentGuideRole = GuideRole.Offline;
			break;
		default:
			_currentGuideRole = (GuideRole)type;
			break;
		}
		_isMyPersonalRegion = myPersonalRegion;
		LoadPlayGuideFlow();
		LoadPlayGuideEvent();
		GuideStorageData guideStorageData = LoadGuideStorageData(GetGuideConfig(GuideConfig.StorageKey), storage);
		GuideStorageData guideStorageData2 = LoadGuideStorageData(GetCommonGuideConfig(GuideConfig.StorageKey), storage);
		_lastReturnerCount = guideStorageData.LastReturnerCount;
		_lastNomadCount = guideStorageData.LastNomadCount;
		Dictionary<string, List<string>> completedEvents = guideStorageData.CompletedEvents;
		Dictionary<string, List<string>> completedEvents2 = guideStorageData2.CompletedEvents;
		foreach (FlowCondition remainFlowCondition in _remainFlowConditions)
		{
			string text = remainFlowCondition.Name;
			List<string> progress = ((!IsCommonFlow(text)) ? completedEvents.Get(text) : completedEvents2.Get(text));
			InitializeFlow(text, progress, remainFlowCondition.SkipLoad, remainFlowCondition.CanRestart, remainFlowCondition.Region);
		}
		Command.LoadDogGuideProgress(guideStorageData);
	}

	private bool IsCommonFlow(string flowName)
	{
		return FindFlow(flowName)?.Common ?? false;
	}

	public static string GetGuideConfig(GuideConfig config, GuideRole role)
	{
		string arg = string.Empty;
		string arg2 = string.Empty;
		switch (config)
		{
		case GuideConfig.StorageKey:
			arg2 = "guide_progress";
			break;
		case GuideConfig.EventFile:
			arg = "PlayGuide/";
			arg2 = "play_guide_event";
			break;
		case GuideConfig.FlowFile:
			arg = "PlayGuide/";
			arg2 = "play_guide_flow";
			break;
		}
		string arg3 = role.ToString().ToLower();
		return $"{arg}{arg3}_{arg2}";
	}

	private string GetGuideConfig(GuideConfig config)
	{
		return GetGuideConfig(config, _currentGuideRole);
	}

	private static string GetCommonGuideConfig(GuideConfig config)
	{
		return GetGuideConfig(config, GuideRole.Common);
	}

	[CanBeNull]
	private GuideEvent FindEvent(string eventName)
	{
		return (!string.IsNullOrEmpty(eventName)) ? _eventDictionary.Get(eventName) : null;
	}

	private void SetCurrentEvent([NotNull] GuideEvent current)
	{
		GuideEvent currentEvent = _currentEvent;
		_currentEvent = current;
		_isOverlappedEventAllowed = BlankEvent == _currentEvent;
		if (this.EventChanged != null)
		{
			this.EventChanged(currentEvent, _currentEvent);
		}
		ApplySurvivalMemo(_currentEvent);
		ApplyTouchToDo();
		ActivateFaction(_currentEvent.ActivateFaction);
		ApplyHelperTarget(_currentEvent);
		Command.DispatchCustomCmd(_currentEvent.CustomCommand);
		ToDoCollection toDoCollection = _currentEvent.ToDoCollection;
		if (toDoCollection != null)
		{
			GameSystem<ToDoListSystem>.Instance().Add(toDoCollection);
		}
		if (!string.IsNullOrEmpty(_currentEvent.SpawnFlow))
		{
			BeginFlow(_currentEvent.SpawnFlow);
		}
	}

	private void RefreshHelperTargets()
	{
		ApplyHelperTarget(_currentEvent);
		ToDoListSystem toDoListSystem = GameSystem<ToDoListSystem>.Instance();
		for (int i = 0; i < toDoListSystem.CollectionCount; i++)
		{
			GuideEvent guideEvent = toDoListSystem.GetCollection(i).GuideEvent;
			if (guideEvent != null && guideEvent != _currentEvent)
			{
				ApplyHelperTarget(guideEvent);
			}
		}
	}

	public void ApplyHelperTarget([CanBeNull] GuideEvent guideEvent)
	{
		if (guideEvent != null && (guideEvent.ToDoCollection == null || guideEvent.ToDoCollection.IsReady) && this.HelperTargetApplied != null)
		{
			this.HelperTargetApplied(guideEvent);
		}
	}

	private static void ApplySurvivalMemo(GuideEvent guideEvent)
	{
		if (guideEvent.SurvivalMemo != 0)
		{
			MemoSystem.SetMemoAvailable(MemoType.Survival, guideEvent.SurvivalMemo);
		}
	}

	private void ApplyTouchToDo()
	{
		if (!string.IsNullOrEmpty(_currentEvent.TouchToDo))
		{
			GameSystem<ToDoListSystem>.Instance().Touch(_currentEvent.TouchToDo);
		}
	}

	private static void ActivateFaction(FactionType faction)
	{
		if (faction != FactionType.Invalid)
		{
			Connections.Frontend.Send(new ActivateFaction
			{
				Faction = faction
			});
		}
	}

	private void StartEvent(FlowStack flowStack, string eventName)
	{
		GuideEvent guideEvent = FindEvent(eventName);
		if (guideEvent == null)
		{
			ChangeEvent(BlankEvent);
		}
		else if (flowStack.Region == null || flowStack.Region.IsAllowed(GameManager.Region))
		{
			guideEvent.FlowStack = flowStack;
			ChangeEvent(guideEvent);
		}
	}

	private void ChangeEvent([NotNull] GuideEvent newEvent)
	{
		if (!_isOverlappedEventAllowed && _currentEvent.FlowStack != newEvent.FlowStack)
		{
			if (newEvent != BlankEvent && !_delayedEventQueue.Contains(newEvent))
			{
				_delayedEventQueue.Enqueue(newEvent);
			}
			return;
		}
		SetCurrentEvent(newEvent);
		if (_currentEvent == BlankEvent)
		{
			if (!ProcessDelayedEventQueue())
			{
				RestoreCurrentEvent();
			}
		}
		else if (_currentEvent.Duration <= 0f && KUtility.GetSize(_currentEvent.Messages) == 0 && _currentEvent.SpotlightTarget == null)
		{
			OnGuideMsgFinished();
		}
	}

	private void RestoreCurrentEvent()
	{
		for (int i = 0; i < _flowStacks.Count; i++)
		{
			FlowStack flowStack = _flowStacks[i];
			if (!flowStack.Started)
			{
				continue;
			}
			string current = flowStack.GetCurrent();
			if (current != null)
			{
				GuideEvent guideEvent = FindEvent(current);
				if (guideEvent != null && guideEvent != BlankEvent)
				{
					guideEvent.FlowStack = flowStack;
					_currentEvent = guideEvent;
					break;
				}
			}
		}
	}

	private static void CompleteEventToDo([NotNull] GuideEvent guideEvent)
	{
		if (guideEvent.ToDoCollection != null)
		{
			int i = 0;
			for (int size = KUtility.GetSize(guideEvent.ToDoCollection.ToDoList); i < size; i++)
			{
				ToDoBase toDoBase = guideEvent.ToDoCollection.ToDoList[i];
				toDoBase.IsCompleted = true;
			}
			GameSystem<ToDoListSystem>.Instance().Remove(guideEvent.ToDoCollection);
		}
	}

	private static void RemoveEventToDo([NotNull] GuideEvent guideEvent)
	{
		if (guideEvent.ToDoCollection != null)
		{
			GameSystem<ToDoListSystem>.Instance().Remove(guideEvent.ToDoCollection);
		}
	}

	public void RemoveHelperTargets([NotNull] GuideEvent guideEvent)
	{
		if (this.HelperTargetRemoved != null)
		{
			this.HelperTargetRemoved(guideEvent);
		}
	}

	public void CompleteAllEvents()
	{
		foreach (FlowStack flowStack in _flowStacks)
		{
			flowStack.Region = null;
		}
		while (_currentEvent != BlankEvent)
		{
			CompleteCurrentEvent();
		}
		Command.Event_UnLockPlayerMove();
	}

	public void CompleteCurrentEvent()
	{
		CompleteEvent(_currentEvent);
	}

	public void CompleteEvent([NotNull] GuideEvent guideEvent)
	{
		if (guideEvent != BlankEvent)
		{
			_completedEvents.Add(guideEvent.Name);
			CompleteEventToDo(guideEvent);
			RemoveHelperTargets(guideEvent);
			MoveToNextEvent(guideEvent.FlowStack);
		}
	}

	public void RemoveFlowCondition(string conditionName)
	{
		for (int num = _remainFlowConditions.Count - 1; num >= 0; num--)
		{
			FlowCondition flowCondition = _remainFlowConditions[num];
			if (flowCondition.Name == conditionName)
			{
				flowCondition.TryUnregister();
				_remainFlowConditions.RemoveAt(num);
			}
		}
	}

	private void MoveToNextEvent([CanBeNull] FlowStack flowStack)
	{
		if (flowStack != null)
		{
			string eventName = flowStack.MoveNext(canRecord: true, canRaiseEvent: true);
			StartEvent(flowStack, eventName);
			SaveGuideProgress();
		}
	}

	private bool ProcessDelayedEventQueue()
	{
		if (_delayedEventQueue.Count > 0)
		{
			GuideEvent newEvent = _delayedEventQueue.Dequeue();
			ChangeEvent(newEvent);
			return true;
		}
		return false;
	}

	public void OnGuideMsgFinished()
	{
		_isOverlappedEventAllowed = true;
		ToDoCollection toDoCollection = _currentEvent.ToDoCollection;
		if (toDoCollection != null && !GuideEvent.CheckAllToDoCompleted(toDoCollection))
		{
			ProcessDelayedEventQueue();
		}
		else
		{
			CompleteCurrentEvent();
		}
	}

	public void BeginFlow(string flowName, bool canMoveToNext = true)
	{
		FlowStack flowStack = FindFlowStack(flowName);
		if (flowStack != null && !flowStack.Started)
		{
			if (flowStack.Completed)
			{
				ChangeEvent(BlankEvent);
				flowStack.Started = true;
			}
			else
			{
				string current = flowStack.GetCurrent();
				if (current != null)
				{
					flowStack.Started = true;
					StartEvent(flowStack, current);
				}
				else
				{
					if (!canMoveToNext)
					{
						return;
					}
					flowStack.Started = true;
					MoveToNextEvent(flowStack);
				}
			}
		}
		RemoveFlowCondition(flowName);
	}

	public void NotifyEventOccured(string type, string param)
	{
		if (this.ExternalEventOccured != null)
		{
			this.ExternalEventOccured(type, param);
		}
	}

	public void NotifyQuizAnswered(string eventName, int index)
	{
		LastQuizAnswer = index;
		if (eventName == "returner_select_reset" && index == 1)
		{
			RequestReturnerGuideAction(ReturnerGuideAction.AdvisorReset);
		}
		if (eventName == "returner_skill_reset" && index == 0)
		{
			RequestReturnerGuideAction(ReturnerGuideAction.SkillReset);
		}
		if (eventName == "returner_greeting_c" && index == 0)
		{
			RequestReturnerGuideAction(ReturnerGuideAction.ReliefGoodsReceive);
		}
	}

	private static void RequestReturnerGuideAction(ReturnerGuideAction action)
	{
		Connections.Frontend.Send(new RequestReturnerGuideAction
		{
			Action = action
		});
	}

	private void TerrainA6_OnLoadingChunksFinished()
	{
		if (IsGuideBegin)
		{
			RefreshHelperTargets();
			return;
		}
		if (_currentGuideRole == GuideRole.Tutorial)
		{
			Command.RestoreDogState();
		}
		ProcessHideOtherPlayer();
		BeginNormalFlow();
		for (int num = _remainFlowConditions.Count - 1; num >= 0; num--)
		{
			BeginFlow(_remainFlowConditions[num].Name, canMoveToNext: false);
		}
		for (int num2 = _remainFlowConditions.Count - 1; num2 >= 0; num2--)
		{
			FlowCondition flowCondition = _remainFlowConditions[num2];
			flowCondition.TryRegister();
		}
		IsGuideBegin = true;
		if (this.Begun != null)
		{
			this.Begun(_prevGuideRole, _currentGuideRole);
		}
		_prevGuideRole = _currentGuideRole;
		CheckReturnerGuide();
		CheckNomadGuide();
	}

	private void CheckReturnerGuide()
	{
		if (!IsGuideBegin)
		{
			return;
		}
		bool? isReturner = _isReturner;
		if (isReturner.HasValue)
		{
			if ((_isReturner == false || _lastReturnerCount < _returnerCount) && IsFlowProgressed("returner_guide"))
			{
				ReloadFlow("returner_guide");
			}
			if (_isReturner == true)
			{
				_lastReturnerCount = _returnerCount;
				BeginFlow("returner_guide");
			}
		}
	}

	private void CheckNomadGuide()
	{
		if (!IsGuideBegin)
		{
			return;
		}
		bool? isNomad = _isNomad;
		if (isNomad.HasValue)
		{
			if ((_isNomad == false || _lastNomadCount < _nomadCount) && IsFlowProgressed("nomad_guide"))
			{
				ReloadFlow("nomad_guide");
			}
			if (_isNomad == true)
			{
				_lastNomadCount = _nomadCount;
				BeginFlow("nomad_guide");
			}
		}
	}

	private void ProcessHideOtherPlayer()
	{
		if (_currentGuideRole != GuideRole.Tutorial)
		{
			return;
		}
		bool hide = true;
		for (int i = 0; i < _completedEvents.Count; i++)
		{
			GuideEvent guideEvent = FindEvent(_completedEvents[i]);
			if (guideEvent != null && guideEvent.CustomCommand != null && guideEvent.CustomCommand.Contains("Event_ShowOtherPlayer"))
			{
				hide = false;
				break;
			}
		}
		Singleton<PlayerManager>.Instance().HideOtherPlayers(hide);
	}

	private void RemoveFlowRelated(FlowStack flowStack)
	{
		string current = flowStack.GetCurrent();
		if (current != null)
		{
			GuideEvent guideEvent = FindEvent(current);
			if (guideEvent != null)
			{
				RemoveEventToDo(guideEvent);
				RemoveHelperTargets(guideEvent);
			}
		}
	}

	private void BeginNormalFlow()
	{
		if (_currentGuideRole == GuideRole.Personal)
		{
			if (GameSystem<FactionSystem>.Instance().IsFactionInitialized)
			{
				BeginPersonalNormalFlow();
			}
			else
			{
				_checkPersonalNormalFlow = true;
			}
		}
		else
		{
			BeginFlow("normal_flow");
		}
	}

	private void BeginPersonalNormalFlow()
	{
		if (GameSystem<FactionSystem>.Instance().IsFactionEnabled(FactionType.Lama) && !IsFlowProgressed("normal_new_user"))
		{
			BeginFlow("normal_existing_user");
		}
		else if (!IsFlowProgressed("normal_existing_user"))
		{
			BeginFlow("normal_new_user");
		}
	}

	public void ReloadFlow(string flowName, Action finished = null)
	{
		FlowStack flowStack = FindFlowStack(flowName);
		if (flowStack == null)
		{
			return;
		}
		RemoveFlowRelated(flowStack);
		Flow flow = FindFlow(flowName);
		if (flow != null)
		{
			ResetFlowContainers(flow);
		}
		FlowStack flowStack2 = CreateFlowStack(flowName, finished);
		if (flowStack2 == null)
		{
			return;
		}
		for (int i = 0; i < _flowStacks.Count; i++)
		{
			if (_flowStacks[i].Name == flowName)
			{
				_flowStacks[i] = flowStack2;
				break;
			}
		}
		if (_currentEvent.FlowStack == flowStack)
		{
			_currentEvent.FlowStack = null;
			BlankEvent.FlowStack = null;
			ChangeEvent(BlankEvent);
		}
		SaveGuideProgress();
	}

	public bool IsFlowProgressed(string flowName)
	{
		FlowStack flowStack = FindFlowStack(flowName);
		return flowStack != null && (flowStack.Completed || flowStack.Progressed);
	}

	public bool IsFlowRunning(string flowName)
	{
		FlowStack flowStack = FindFlowStack(flowName);
		return flowStack != null && !flowStack.Completed && flowStack.Progressed;
	}

	private void ResetFlowContainers(Flow container)
	{
		List<string> list = container.List;
		foreach (string item in list)
		{
			GuideEvent guideEvent = FindEvent(item);
			if (guideEvent == null || guideEvent.ToDoCollection == null)
			{
				continue;
			}
			List<ToDoBase> toDoList = guideEvent.ToDoCollection.ToDoList;
			foreach (ToDoBase item2 in toDoList)
			{
				item2.IsCompleted = false;
				item2.CurrentProgress = 0;
			}
		}
	}

	public int IndexOfDelayedEvent(string eventName)
	{
		int num = -1;
		foreach (GuideEvent item in _delayedEventQueue)
		{
			num++;
			if (item.Name == eventName)
			{
				return num;
			}
		}
		return -1;
	}
}
