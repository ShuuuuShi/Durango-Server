using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.PlayGuide;
using Durango.UI.Control;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Shared.Faction;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class DialogueGroupBase : UIBase
{
	protected enum Type
	{
		System,
		Dialogue,
		Quiz
	}

	protected class Context
	{
		public Action Finished;

		public float Duration;

		public string Sound;

		public bool Remote;

		public bool Blur;

		public Material Portrait;

		public string Image;

		public ColoredText Name;

		public ColoredText Message;

		public string VoiceEvent;

		public string ChapterTitle;

		public Type Type;

		public QuizData Quiz;

		public GuideEvent Guide;
	}

	protected struct ColoredText
	{
		private readonly string _text;

		private readonly Color _color;

		public bool IsBlank => _text == null || _text.Trim().Length == 0;

		public string Text => _text;

		public Color Color => _color;

		private ColoredText(string value)
		{
			_text = value;
			_color = Color.white;
		}

		public ColoredText(string text, Color color)
		{
			_text = text;
			_color = color;
		}

		public static implicit operator ColoredText(string value)
		{
			return new ColoredText(value);
		}

		public static implicit operator string(ColoredText value)
		{
			return value._text;
		}
	}

	[SerializeField]
	protected UIWidget MainWidget;

	[SerializeField]
	protected UILabel SystemLabel;

	[SerializeField]
	private GameObject _dialogueParent;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private UITexture _backgroundTexture;

	[SerializeField]
	protected ListObjectPool ChoicePool;

	[SerializeField]
	protected GameObject DialogueContext;

	[SerializeField]
	protected UILabel DialogueLabel;

	[SerializeField]
	private GameObject _chapter;

	[SerializeField]
	private UILabel _chapterTitle;

	[SerializeField]
	[EnumList(typeof(ShowPortrait), false, 0, -1)]
	private Material[] _guidePortaits;

	[SerializeField]
	[EnumList(typeof(Shared.Faction.Messenger), true, 1, 24)]
	private Material[] _messengerPortraits;

	[SerializeField]
	private Material _vhsMaterial;

	[SerializeField]
	private SoundEventType _radioStart;

	[SerializeField]
	private SoundEventType _radioClosed;

	[SerializeField]
	private SoundEventType _radioMessage;

	private readonly List<Context> _contexts = new List<Context>();

	protected Context Current;

	private GuideEvent _guide;

	private int _quizIndex;

	private readonly List<string> _quizSelected = new List<string>();

	private float _nextTimer;

	private TypeWriterEffect _typewriterDialouge;

	private TypeWriterEffect _typewriterSystem;

	private bool _readyToNext;

	private uint _voiceInstanceId;

	protected virtual void Start()
	{
		if (Application.isEditor)
		{
			_vhsMaterial = new Material(_vhsMaterial);
		}
		base.VisibleController.Changed += OnVisibleChanged;
		_typewriterDialouge = DialogueLabel.gameObject.AddComponent<TypeWriterEffect>();
		_typewriterDialouge.enabled = false;
		_typewriterDialouge.Finished += TypeWriterDialouge_Finished;
		_typewriterSystem = SystemLabel.gameObject.AddComponent<TypeWriterEffect>();
		_typewriterSystem.enabled = false;
		_typewriterSystem.Finished += TypeWriteSystem_Finished;
		SoundManager.PrepareEvent(_radioStart);
		SoundManager.PrepareEvent(_radioClosed);
		SoundManager.PrepareEvent(_radioMessage);
		GameSystem<PlayGuideSystem>.Instance().Command.Ready += PlayGuideSystem_Ready;
		GameSystem<PlayGuideSystem>.Instance().EventChanged += PlayGuideSystem_EventChanged;
		GameSystem<ArchipelagoMissionSystem>.Instance().MissionStarted += ArchipelagoMissionSystem_MissionStarted;
		GameSystem<ArchipelagoMissionSystem>.Instance().MissionEnded += ArchipelagoMissionSystem_MissionEnded;
		GameSystem<QuestSystem>.Instance().QuestStarted += QuestSystem_QuestStarted;
		GameSystem<QuestSystem>.Instance().QuestFinished += QuestSystem_QuestFinished;
		SetChoiceCount(0);
		SetChildrenActive(activated: false);
		SetVisible(visible: false, "Loading");
		UIManager.OnLoadingCurtainHidden(OnLoadingCurtainHidden);
	}

	protected virtual void Update()
	{
		if (!base.IsOpened || !base.Visible || GameSystem<PlayGuideSystem>.Instance().PauseUpdate)
		{
			return;
		}
		if (Current == null)
		{
			Next();
		}
		else if (Current.Duration > 0f)
		{
			_nextTimer -= Time.deltaTime;
			if (_nextTimer < 0f)
			{
				Next();
			}
		}
	}

	protected override bool TryOpen()
	{
		if (base.TryOpen())
		{
			Next();
			return true;
		}
		return false;
	}

	protected override bool TryClose()
	{
		BlurOff();
		return base.TryClose();
	}

	private void OnVisibleChanged(bool visible)
	{
		if (base.IsOpened)
		{
			if (!visible)
			{
				BlurOff();
				StopDialogueVoice();
			}
			else
			{
				Refresh(resume: true);
			}
		}
	}

	private void Add(Context context)
	{
		_contexts.Add(context);
		if (!base.IsOpened)
		{
			Open();
		}
	}

	private void Next()
	{
		if (Current != null && Current.Finished != null)
		{
			Current.Finished();
		}
		if (_contexts.Count > 0)
		{
			Current = _contexts[0];
			_contexts.RemoveAt(0);
			Refresh();
		}
		else
		{
			Current = null;
			Close();
		}
		OnContextChanged();
	}

	private void OnLoadingCurtainHidden()
	{
		SetVisible(visible: true, "Loading");
	}

	private void OnContextChanged()
	{
		bool pause = Current != null && Current.Type != 0 && (Current.Portrait != null || !string.IsNullOrEmpty(Current.Image));
		UIManager.FindScript<AlarmGroup>().PauseRewardAlarm("Dialogue", pause);
		UIManager.FindScript<PlayerHudGroupBase>().PauseSpecialDealPopup(pause);
	}

	private void Refresh(bool resume = false)
	{
		if (!base.Visible || GameSystem<CombatSystem>.Instance().CombatMode)
		{
			return;
		}
		if (Current == null)
		{
			Next();
			return;
		}
		SoundManager.PlayEvent(Current.Sound);
		if (Current.Blur)
		{
			BlurOn();
		}
		else
		{
			BlurOff();
		}
		if (Current.Portrait == null)
		{
			_portraitTexture.gameObject.SetActive(value: false);
		}
		else
		{
			_portraitTexture.gameObject.SetActive(value: true);
			_portraitTexture.material = ((!Current.Remote) ? Current.Portrait : GetVhsMaterial(Current.Portrait));
			_portraitTexture.RemoveFromPanel();
		}
		if (string.IsNullOrEmpty(Current.Image))
		{
			_backgroundTexture.gameObject.SetActive(value: false);
		}
		else
		{
			_backgroundTexture.gameObject.SetActive(value: true);
			SetTexture(_backgroundTexture, Current.Image);
		}
		SystemLabel.gameObject.SetActive(Current.Type == Type.System || Current.Type == Type.Quiz);
		_dialogueParent.SetActive(Current.Type == Type.Dialogue || Current.Type == Type.Quiz);
		DialogueContext.SetActive(Current.Type == Type.Dialogue);
		if (_chapter != null)
		{
			_chapter.SetActive(!string.IsNullOrEmpty(Current.ChapterTitle));
			_chapterTitle.text = Current.ChapterTitle;
		}
		if (Current.Type != Type.Quiz)
		{
			SetChoiceCount(0);
		}
		switch (Current.Type)
		{
		case Type.System:
			SetSystemLabel(Current.Message, typing: false);
			break;
		case Type.Dialogue:
			SetDialogue(Current);
			break;
		case Type.Quiz:
			SetQuiz(Current);
			break;
		}
		if (!resume)
		{
			AddToChat(Current);
		}
		if (!resume || Current.Type != 0)
		{
			_nextTimer = Current.Duration;
		}
		PlayDialogueVoice(Current.VoiceEvent);
		OnRefresh();
	}

	protected virtual void OnRefresh()
	{
	}

	private static void AddToChat(Context ctx)
	{
		string speakerName = ((ctx.Type == Type.System) ? string.Empty : ((!string.IsNullOrEmpty(ctx.Name)) ? ((string)ctx.Name) : T._("[ffbf00]K[-]")));
		GameSystem<SocialSystem>.Instance().AddSystemChat(ctx.Message, speakerName);
	}

	protected virtual void SetDialogue([NotNull] Context ctx)
	{
		DialogueLabel.color = ctx.Message.Color;
		DialogueLabel.UpdateAnchors();
		_readyToNext = false;
		_typewriterDialouge.Reset();
		_typewriterDialouge.enabled = true;
	}

	private void SetQuiz([NotNull] Context ctx)
	{
		QuizData quiz = ctx.Quiz;
		if (quiz == null)
		{
			Next();
			return;
		}
		SetSystemLabel(ctx.Message, typing: true);
		int choiceCount = Math.Min(3, KUtility.GetSize(quiz.Choices));
		SetChoiceCount(choiceCount);
		for (int i = 0; i < choiceCount; i++)
		{
			string choice = quiz.Choices[i];
			ChoiceButton choiceButton = ChoicePool.Get<ChoiceButton>(i);
			choiceButton.Set(choice, i);
			choiceButton.Disabled = _quizSelected.Contains(choice);
			int choiceIndex = i;
			choiceButton.Clicked = delegate
			{
				GameSystem<PlayGuideSystem>.Instance().NotifyQuizAnswered(ctx.Guide.Name, choiceIndex);
				GameSystem<SocialSystem>.Instance().AddSystemChat(T._("<em>{0}</em>{0:-을} 선택했습니다.", T._(choice)), string.Empty);
				_quizSelected.Add(choice);
				int index;
				string[] messages = quiz.GetMessages(_quizSelected, out index);
				int size = KUtility.GetSize(messages);
				bool flag = KUtility.GetSize(quiz.Solutions) > 0 && quiz.Solutions.Contains(choice);
				if (size == 0)
				{
					NextQuiz();
					Context context = CreateContext(_guide, onStart: false, onFinish: true, null);
					context.Duration = 0.01f;
					context.Type = Type.System;
					Add(context);
				}
				else if (flag || _quizSelected.Count >= choiceCount)
				{
					NextQuiz();
					for (int j = 0; j < size; j++)
					{
						Context item = CreateContext(_guide, onStart: false, j == size - 1, messages[j], PlayGuideSystem.GetQuizAnswerVoiceEventName(_guide, index, j));
						_contexts.Add(item);
					}
				}
				else
				{
					for (int k = 0; k < size; k++)
					{
						string message = messages[k];
						if (k == size - 1)
						{
							Context item2 = CreateQuiz(ctx, message);
							_contexts.Add(item2);
						}
						else
						{
							Context item3 = CreateContext(_guide, onStart: false, onFinish: false, message);
							_contexts.Add(item3);
						}
					}
				}
				Next();
			};
		}
	}

	private void NextQuiz()
	{
		_quizSelected.Clear();
		_quizIndex++;
	}

	private void ResetQuiz()
	{
		_quizSelected.Clear();
		_quizIndex = 0;
	}

	private void SetSystemLabel(ColoredText text, bool typing)
	{
		if (string.IsNullOrEmpty(text))
		{
			SystemLabel.gameObject.SetActive(value: false);
			return;
		}
		SystemLabel.text = text;
		SystemLabel.color = text.Color;
		UIUtility.UpdateAnchors(SystemLabel.transform);
		_typewriterSystem.Reset();
		_typewriterSystem.enabled = typing;
	}

	protected virtual void SetChoiceCount(int count)
	{
		ChoicePool.Set(count);
	}

	protected virtual void BlurOn()
	{
	}

	protected virtual void BlurOff()
	{
	}

	protected void OnPressDialogue(bool pressed)
	{
		if (Current == null || Current.Type == Type.Quiz)
		{
			return;
		}
		if (pressed)
		{
			_readyToNext = !_typewriterDialouge.enabled;
			_typewriterDialouge.SetFastFoward(fastFoward: true);
			return;
		}
		if (_readyToNext)
		{
			StopDialogueVoice();
			Next();
		}
		_typewriterDialouge.SetFastFoward(fastFoward: false);
	}

	protected virtual void TypeWriterDialouge_Finished()
	{
	}

	protected virtual void TypeWriteSystem_Finished()
	{
	}

	private void PlayGuideSystem_Ready()
	{
		Next();
	}

	private void PlayGuideSystem_EventChanged(GuideEvent prev, GuideEvent cur)
	{
		RemovePrevGuide(prev);
		AddCurentGuide(cur);
	}

	private void ArchipelagoMissionSystem_MissionStarted([NotNull] ArchipelagoMission mission)
	{
		AddDialogue(mission.Intro, null);
	}

	private void ArchipelagoMissionSystem_MissionEnded(ArchipelagoToDoCollection toDoCollection)
	{
		Action finished = null;
		if (GameSystem<ArchipelagoMissionSystem>.Instance().GetNextRegion() == null)
		{
			GameSystem<ToDoListSystem>.Instance().Remove(toDoCollection);
			finished = delegate
			{
				ChapterGroup chapterGroup = UIManager.FindScript<ChapterGroup>();
				if (!(chapterGroup == null))
				{
					chapterGroup.Show(T._("개척 임무 완료"), string.Empty, 1);
				}
			};
		}
		AddDialogue(toDoCollection.Outro, finished);
	}

	private void QuestSystem_QuestStarted(string questId)
	{
		QuestYml questYml = SingletonDict<string, QuestYml>.Instance.Get(questId);
		if (questYml != null && KUtility.GetSize(questYml.QuestStartMessages) > 0)
		{
			AddQuestMessages(questId, questYml.QuestStartMessages);
		}
	}

	private void QuestSystem_QuestFinished(string questId)
	{
		QuestYml questYml = SingletonDict<string, QuestYml>.Instance.Get(questId);
		if (questYml != null && KUtility.GetSize(questYml.QuestEndMessages) > 0)
		{
			AddQuestMessages(questId, questYml.QuestEndMessages);
		}
	}

	public void AddQuestMessages(string questId, QuestMessages[] messages, bool addFront = false)
	{
		List<Context> list = null;
		if (addFront && (_contexts.Count > 0 || Current != null))
		{
			list = new List<Context>();
			if (Current != null)
			{
				list.Add(Current);
			}
			list.AddRange(_contexts);
			Current = null;
			_contexts.Clear();
		}
		string chapterTitle = GameSystem<QuestSystem>.Instance().GetChatper(questId)?.Title ?? ((Gettext)null);
		for (int i = 0; i < KUtility.GetSize(messages); i++)
		{
			QuestMessages questMessages = messages[i];
			AddDialogue(questMessages.Message, questMessages.Messenger, questMessages.Remote, blur: true, questMessages.Image, questMessages.HidePortrait, chapterTitle);
		}
		if (list != null)
		{
			_contexts.AddRange(list);
		}
	}

	private void RemovePrevGuide(GuideEvent prev)
	{
		if (prev == null)
		{
			return;
		}
		if (Current != null && Current.Guide == prev)
		{
			Current = null;
		}
		for (int num = _contexts.Count - 1; num >= 0; num--)
		{
			if (_contexts[num].Guide == prev)
			{
				_contexts.RemoveAt(num);
			}
		}
	}

	private void AddCurentGuide(GuideEvent guide)
	{
		if (guide == null)
		{
			return;
		}
		string[] messages = guide.Messages;
		int size = KUtility.GetSize(messages);
		if (guide.Duration <= 0f && size == 0)
		{
			return;
		}
		_guide = guide;
		ResetQuiz();
		if (size == 0)
		{
			Add(CreateContext(_guide, onStart: true, onFinish: true, null));
			return;
		}
		for (int i = 0; i < size; i++)
		{
			Context context = CreateContext(_guide, i == 0, i == size - 1, messages[i], PlayGuideSystem.GetMessageVoiceEventName(_guide, i));
			Add(context);
		}
	}

	private static Shared.Faction.Messenger ToMessenger(NPCType npc)
	{
		return npc switch
		{
			NPCType.Optimistic => Shared.Faction.Messenger.Charlie, 
			NPCType.TheFirm => Shared.Faction.Messenger.K, 
			NPCType.ChlorophylForum => Shared.Faction.Messenger.Liu, 
			NPCType.ChamberOfPioneer => Shared.Faction.Messenger.Nowak, 
			NPCType.TheCommittee => Shared.Faction.Messenger.X, 
			NPCType.Lama => Shared.Faction.Messenger.Lama, 
			NPCType.Concierge => Shared.Faction.Messenger.Concierge, 
			NPCType.RescueTf => Shared.Faction.Messenger.K, 
			_ => Shared.Faction.Messenger.K, 
		};
	}

	private Context CreateContext(GuideEvent guide, bool onStart, bool onFinish, string message, string voiceEventName = null)
	{
		Context dialogue = new Context();
		dialogue.Guide = guide;
		dialogue.Type = ((!guide.IsSystem) ? Type.Dialogue : Type.System);
		dialogue.Blur = guide.IsBlur;
		dialogue.Duration = guide.Duration;
		if (!guide.HidePortrait)
		{
			if (guide.ShowPortrait != ShowPortrait.None)
			{
				dialogue.Portrait = _guidePortaits[(int)guide.ShowPortrait];
			}
			else
			{
				Shared.Faction.Messenger messenger = ToMessenger(guide.NPCType);
				dialogue.Portrait = _messengerPortraits[(int)messenger];
			}
		}
		dialogue.Image = guide.Image;
		dialogue.Name = ((!guide.HidePortrait) ? guide.NameTag : " ");
		dialogue.Remote = guide.Remote;
		if (guide.Remote)
		{
			dialogue.Sound = ((!onStart) ? _radioMessage : _radioStart);
		}
		if (onFinish)
		{
			dialogue.Quiz = guide.GetQuiz(_quizIndex);
			dialogue.Finished = delegate
			{
				if (dialogue.Quiz != null)
				{
					Context item = CreateQuiz(dialogue, dialogue.Quiz.Message);
					_contexts.Add(item);
				}
				else
				{
					if (!guide.IsSystem && guide.Remote)
					{
						SoundManager.PlayEvent(_radioClosed);
					}
					GameSystem<PlayGuideSystem>.Instance().OnGuideMsgFinished();
				}
			};
		}
		dialogue.VoiceEvent = voiceEventName;
		dialogue.Message = T._(message);
		return dialogue;
	}

	private static Context CreateQuiz(Context prev, string message)
	{
		Context context = new Context();
		context.Guide = prev.Guide;
		context.Blur = prev.Blur;
		context.Remote = prev.Remote;
		context.Portrait = prev.Portrait;
		context.Image = prev.Image;
		context.Quiz = prev.Quiz;
		context.Sound = prev.Sound;
		context.Name = prev.Name;
		context.Type = Type.Quiz;
		context.Message = T._(message);
		return context;
	}

	private Material GetVhsMaterial(Material baseMat)
	{
		_vhsMaterial.mainTexture = baseMat.mainTexture;
		_vhsMaterial.SetTexture("_AlphaTex", baseMat.GetTexture("_AlphaTex"));
		return _vhsMaterial;
	}

	public void AddFactionTalks(Talks talks)
	{
		if (talks != null)
		{
			int i = 0;
			for (int size = KUtility.GetSize(talks.List); i < size; i++)
			{
				Talk talk = talks.List[i];
				Context context = new Context();
				context.Blur = true;
				context.Sound = _radioMessage;
				context.Remote = true;
				int messenger = (int)talk.Messenger;
				context.Portrait = ((messenger < 0) ? null : _messengerPortraits[messenger]);
				context.Name = SingletonDict<Shared.Faction.Messenger, Yaml.Messenger>.Get(talk.Messenger).Name.ToString();
				context.Message = new ColoredText(talk.Message, new Color32(137, 210, byte.MaxValue, byte.MaxValue));
				context.Type = Type.Dialogue;
				Add(context);
			}
		}
	}

	private void AddDialogue([CanBeNull] Dialogue dialogue, [CanBeNull] Action finished)
	{
		if (dialogue != null && KUtility.GetSize(dialogue.Talks) != 0)
		{
			bool blur = dialogue.Blur;
			bool remote = dialogue.Remote;
			for (int i = 0; i < dialogue.Talks.Count; i++)
			{
				MissionTalk missionTalk = dialogue.Talks[i];
				Action action = ((i != dialogue.Talks.Count - 1) ? null : finished);
				string message = missionTalk.Message;
				Shared.Faction.Messenger messenger = missionTalk.Messenger;
				bool remote2 = remote;
				bool blur2 = blur;
				string image = missionTalk.Image;
				bool hidePortrait = missionTalk.HidePortrait;
				Action onFinished = action;
				AddDialogue(message, messenger, remote2, blur2, image, hidePortrait, null, onFinished);
			}
		}
	}

	[ExposedInEditor(null)]
	private void AddDialogue(string message, Shared.Faction.Messenger messenger, bool remote, bool blur, string image = null, bool hidePortrait = false, string chapterTitle = null, Action onFinished = null)
	{
		Yaml.Messenger messenger2 = SingletonDict<Shared.Faction.Messenger, Yaml.Messenger>.Get(messenger);
		Context context = new Context();
		context.Portrait = ((hidePortrait || messenger < Shared.Faction.Messenger.Player) ? null : _messengerPortraits[(int)messenger]);
		context.Image = image;
		context.Message = message;
		context.Name = ((!hidePortrait) ? messenger2.Name.ToString() : " ");
		context.Type = Type.Dialogue;
		context.Finished = onFinished;
		context.Remote = remote;
		if (remote)
		{
			context.Sound = _radioMessage;
		}
		context.Blur = blur;
		context.ChapterTitle = chapterTitle;
		Add(context);
	}

	private void PlayDialogueVoice(string eventName)
	{
		StopDialogueVoice();
		if (SoundManager.HasEvent(eventName))
		{
			_voiceInstanceId = SoundManager.PlayEvent(eventName, SoundPosition.Empty, exclusive: true);
		}
	}

	private void StopDialogueVoice()
	{
		SoundManager.StopEvent(_voiceInstanceId);
		_voiceInstanceId = 0u;
	}

	private static void SetTexture(UITexture texture, string imageName)
	{
		if (texture == null)
		{
			return;
		}
		string path = $"UI/DialogueImage/{imageName}.mat";
		Durango.Utils.Singleton<AssetBundleManager>.Instance().RequestAsset(path, typeof(Material), delegate(UnityEngine.Object asset)
		{
			if (!(asset == null))
			{
				texture.material = asset as Material;
			}
		});
	}
}
