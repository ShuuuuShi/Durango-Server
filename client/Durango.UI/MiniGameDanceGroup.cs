using System;
using System.Collections;
using System.Collections.Generic;
using Durango.Network;
using Durango.Render.Camera;
using Durango.Render.Particle;
using Durango.Terrain;
using Durango.UI.Control;
using Durango.Utils;
using Durango.Utils.Extensions;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class MiniGameDanceGroup : UIBase
{
	[Serializable]
	public class ShakableUI
	{
		[Serializable]
		public class Shakable
		{
			[Flags]
			public enum Mode
			{
				Scale = 1,
				Translate = 2
			}

			public float Frequency = (float)Math.PI * 2f;

			public float Intensity = 3f;

			public float DampingRatio = 0.2f;

			public Transform Target;

			public Transform Destination;

			public Mode ShakeMode = Mode.Translate;

			[NonSerialized]
			public Vector2 Veclocty;

			public void UpdateSpring()
			{
				if (Target.position != Destination.position && (ShakeMode & Mode.Translate) != 0)
				{
					Target.position = Maths.CalculateSpring(Target.position, Destination.position, ref Veclocty, DampingRatio, Frequency, MiniGameDanceHelper.DeltaTime);
				}
				if (Target.localScale != Destination.localScale && (ShakeMode & Mode.Scale) != 0)
				{
					Target.localScale = Maths.CalculateSpring(Target.localScale, Destination.localScale, ref Veclocty, DampingRatio, Frequency, MiniGameDanceHelper.DeltaTime);
				}
			}
		}

		[SerializeField]
		private float _normalShakeForce = 1f;

		[SerializeField]
		private float _goodShakeForce = 1.2f;

		[SerializeField]
		private float _greatShakeForce = 1.5f;

		[SerializeField]
		private Shakable[] _controllers;

		[SerializeField]
		private Shakable[] _items;

		public void ShakeControllers(Vector2 dir, Shakable.Mode mode)
		{
			Shake(_controllers, dir, mode);
		}

		public void ShakeItems(Shakable.Mode mode)
		{
			Shake(_items, Vector3.zero, mode);
		}

		private static void Shake(IEnumerable<Shakable> items, Vector3 dir, Shakable.Mode mode)
		{
			foreach (Shakable item in items)
			{
				if (item != null)
				{
					if (dir == Vector3.zero)
					{
						dir = UnityEngine.Random.insideUnitCircle;
					}
					dir = Vector3.Normalize(dir);
					if ((item.ShakeMode & mode & Shakable.Mode.Scale) != 0)
					{
						item.Target.localScale = item.Destination.localScale + Vector3.one * item.Intensity;
					}
					if ((item.ShakeMode & mode & Shakable.Mode.Translate) != 0)
					{
						item.Target.position = item.Destination.position + dir * item.Intensity;
					}
				}
			}
		}

		public void Updated()
		{
			Shakable[] controllers = _controllers;
			for (int i = 0; i < controllers.Length; i++)
			{
				controllers[i]?.UpdateSpring();
			}
			Shakable[] items = _items;
			for (int j = 0; j < items.Length; j++)
			{
				items[j]?.UpdateSpring();
			}
		}
	}

	private const string BlurKey = "MiniGameDance";

	private string[] _danceKeys = new string[4] { "Emotion_Dance_Rhythm_A", "Emotion_Dance_Rhythm_B", "Emotion_Dance_Rhythm_C", "Emotion_Dance_Rhythm_D" };

	[SerializeField]
	private SelectableWidget _closeBtn;

	[SerializeField]
	private UIWidget _endPanel;

	[SerializeField]
	private SelectableButton _exitButton;

	[SerializeField]
	private SelectableButton _restartButton;

	[SerializeField]
	private UILabel _finalScoreTitleLabel;

	[SerializeField]
	private UILabel _finalScoreWidget;

	[SerializeField]
	private MiniGameDanceResultEffectPlayer _resultEffectPlayer;

	[SerializeField]
	private UIEventListener _gamePanel;

	[SerializeField]
	private UIWidget _countdownLabel;

	[SerializeField]
	private TweenerPlayer _countdownTweener;

	[SerializeField]
	private MiniGameDanceNote _miniGameDanceNotePrefab;

	[SerializeField]
	private UIWidget _noteTargetWidget;

	[SerializeField]
	private UIWidget _noteTargetSprite;

	[SerializeField]
	private TweenerPlayer _noteTargetWidgetTweenPlayer;

	[SerializeField]
	private ParticleType _normalParticle;

	[SerializeField]
	private ParticleType _goodParticle;

	[SerializeField]
	private ParticleType _greatParticle;

	[SerializeField]
	private TweenerPlayer _tweenPlayer;

	[SerializeField]
	private UILabel _resultText;

	[SerializeField]
	private StylizedNumberWidget _accumulatedNote;

	[SerializeField]
	private UILabel _scoreTitleLabel;

	[SerializeField]
	private UILabel _scoreText;

	[SerializeField]
	private TweenFloat _scoreTweener;

	[SerializeField]
	private UIWidget _timeWidget;

	[SerializeField]
	private UISprite _timeSprite;

	[SerializeField]
	private UILabel _timeText;

	[SerializeField]
	private float _swipeThreshold = 0.2f;

	[SerializeField]
	private ShakableUI _shakables;

	private readonly Stack<MiniGameDanceNote> _notePool = new Stack<MiniGameDanceNote>();

	private readonly MiniGameStatus _status = new MiniGameStatus();

	private Stack<MiniGameDanceAsset.DanceNoteData> _notes = new Stack<MiniGameDanceAsset.DanceNoteData>();

	private readonly Dictionary<float, MiniGameDanceNote> _noteObjs = new Dictionary<float, MiniGameDanceNote>();

	private ICoroutineBinder _countdownSequence;

	private ICoroutineBinder _gameSequence;

	private ICoroutineBinder _noteSequence;

	public static bool IsShow { get; private set; }

	private void Start()
	{
		_finalScoreTitleLabel.text = T._("SCORE");
		_scoreTitleLabel.text = T._("Score");
		base.OnOpenSucceed += delegate
		{
			IsShow = true;
			OpenWindow(MiniGameStatus.Mode.Game);
			RegisterKeyboardController();
		};
		base.OnCloseSucceed += delegate
		{
			IsShow = false;
			BlurController.BlurOff("MiniGameDance");
			KillGame();
			UnregisterKeyboardController();
		};
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.MiniGameDance, delegate
		{
			Open();
		});
		_exitButton.Clicked = delegate
		{
			Close();
		};
		_exitButton.Text = T._("나가기");
		_restartButton.Clicked = delegate
		{
			OpenWindow(MiniGameStatus.Mode.Game);
		};
		_restartButton.Text = T._("다시 도전!");
		_closeBtn.Clicked = delegate
		{
			SoundManager.PlayEvent("ui_minigame_end");
			Close();
		};
		_scoreTweener.SetCallback(delegate(float value, bool isFinished)
		{
			_scoreText.text = string.Format(T.Culture, "{0:N0}", value);
		});
		_gamePanel.onDragStart = delegate
		{
			_status.IsPressed = true;
		};
		_gamePanel.onDrag = NoteSwiped;
		_gamePanel.onDragEnd = delegate
		{
			_status.IsPressed = (_status.IsSwipeTried = (_status.IsSuccessfullySwiped = false));
		};
		_gamePanel.onPress = delegate(GameObject obj, bool isPressed)
		{
			if (isPressed)
			{
				NoteClicked();
			}
		};
		SetChildrenActive(activated: false);
	}

	private void OpenWindow(MiniGameStatus.Mode mode)
	{
		switch (mode)
		{
		case MiniGameStatus.Mode.End:
			SoundManager.PlayEvent("ui_minigame_result");
			BlurController.BlurOn("MiniGameDance", BlurController.Mask.Game);
			_finalScoreWidget.text = string.Format(T.Culture, "{0:N0}", _status.TotalScore);
			_endPanel.gameObject.SetActive(value: true);
			_gamePanel.gameObject.SetActive(value: false);
			_resultEffectPlayer.Play();
			break;
		case MiniGameStatus.Mode.Game:
			SoundManager.PlayEvent("ui_minigame_start");
			BlurController.BlurOff("MiniGameDance");
			_status.Init();
			_notes.Clear();
			ResourceSingleton<MiniGameDanceAsset>.Instance().FillData(_status.PlayingMusicName, _notes);
			_accumulatedNote.gameObject.SetActive(value: false);
			SpawnText(0f, MiniGameStatus.AccuracyType.None);
			BlurController.BlurOff("MiniGameDance");
			_endPanel.gameObject.SetActive(value: false);
			_gamePanel.gameObject.SetActive(value: true);
			_resultEffectPlayer.Stop();
			PlayerController.MotionUpdater.Motion(_danceKeys.Random());
			StartMusic(_status.PlayingMusicName, _status.MusicInstanceId, delegate
			{
				StartGame();
				this.StartCoroutine(ref _countdownSequence, CountdownSequence(_status));
				this.StartCoroutine(ref _gameSequence, RythmnGameSequence(_status));
				this.StartCoroutine(ref _noteSequence, NoteSpawnSequence(_status));
			});
			break;
		default:
			throw new ArgumentOutOfRangeException("mode", mode, null);
		}
	}

	private void StartGame()
	{
		_status.StartTime = MiniGameDanceHelper.ElapsedTime;
		_status.IsPlaying = true;
		Singleton<CameraController>.Instance().Zoom(1.54f, 2f);
		Singleton<CameraController>.Instance().Angle(Vector3.up * 1000f, 180f);
		Singleton<TerrainBase>.Instance().HideWorldSpritePool();
		Connections.Frontend.Send(default(MiniGameDanceStarted));
	}

	private void KillGame()
	{
		_status.IsPlaying = false;
		StopAllCoroutines();
		foreach (KeyValuePair<float, MiniGameDanceNote> noteObj in _noteObjs)
		{
			noteObj.Value.gameObject.SetActive(value: false);
			_notePool.Push(noteObj.Value);
		}
		_resultEffectPlayer.Stop();
		StopMusic();
		_noteObjs.Clear();
		Singleton<CameraController>.Instance().Zoom(0.42f, 2f);
		Singleton<CameraController>.Instance().ClearAngle();
		Singleton<TerrainBase>.Instance().RestoreWorldSpritePoolVisibility();
	}

	private void Update()
	{
		_shakables.Updated();
		if (_status.IsPlaying && _timeWidget.isActiveAndEnabled)
		{
			float duration = _status.GetDuration();
			float num = Mathf.Clamp(MiniGameDanceHelper.ElapsedTime - _status.StartTime, 0f, duration);
			_timeText.text = TimedeltaFormatter.Format(num);
			_timeSprite.fillAmount = num / duration;
		}
	}

	private void NoteClicked()
	{
		if (_status.IsSwipeTried)
		{
			return;
		}
		_shakables.ShakeControllers(Vector2.one, ShakableUI.Shakable.Mode.Scale);
		if (_noteTargetWidgetTweenPlayer != null)
		{
			_noteTargetWidgetTweenPlayer.Play();
		}
		Pair<MiniGameDanceAsset.DanceNoteData, MiniGameStatus.AccuracyType> pressbieNote = _status.GetPressbieNote();
		MiniGameDanceAsset.DanceNoteData item = pressbieNote.Item1;
		if (item != null && item.Pattern == MiniGameDanceAsset.DanceNoteData.Type.Dot)
		{
			_status.AddToScore(new Pair<float, MiniGameStatus.AccuracyType>(item.TimeKey, pressbieNote.Item2));
			if (pressbieNote.Item2 == MiniGameStatus.AccuracyType.Perfect)
			{
				SoundManager.PlayEvent("ui_minigame_note");
			}
			SpawnEffect(MiniGameDanceAsset.DanceNoteData.Type.Dot, item.TimeKey, pressbieNote.Item2);
			_status.IsSwipeTried = (_status.IsSuccessfullySwiped = false);
			_status.IsPressed = false;
		}
	}

	private void NoteSwiped(GameObject go, Vector2 delta)
	{
		if (!_status.IsPressed || _status.IsSuccessfullySwiped || delta.sqrMagnitude < _swipeThreshold * _swipeThreshold)
		{
			return;
		}
		float num = _swipeThreshold * 3f;
		if (!_status.IsSwipeTried && delta.sqrMagnitude > num * num)
		{
			_shakables.ShakeControllers(delta, ShakableUI.Shakable.Mode.Translate);
			_status.IsSwipeTried = true;
		}
		Pair<MiniGameDanceAsset.DanceNoteData, MiniGameStatus.AccuracyType> pressbieNote = _status.GetPressbieNote();
		MiniGameDanceAsset.DanceNoteData item = pressbieNote.Item1;
		if (item == null || item.Pattern == MiniGameDanceAsset.DanceNoteData.Type.None)
		{
			return;
		}
		MiniGameDanceAsset.DanceNoteData.Type type = MiniGameDanceHelper.AnalyzeSwipeDirection(delta);
		if (type == item.Pattern)
		{
			if (pressbieNote.Item2 == MiniGameStatus.AccuracyType.Perfect)
			{
				SoundManager.PlayEvent("ui_minigame_note");
			}
			_status.IsSuccessfullySwiped = true;
			_status.AddToScore(new Pair<float, MiniGameStatus.AccuracyType>(item.TimeKey, pressbieNote.Item2));
			SpawnEffect(type, item.TimeKey, pressbieNote.Item2);
		}
	}

	private void SpawnEffect(MiniGameDanceAsset.DanceNoteData.Type direction, float timeKey, MiniGameStatus.AccuracyType accuracy)
	{
		SpawnText(_status.TotalScore, accuracy);
		_shakables.ShakeItems(ShakableUI.Shakable.Mode.Translate);
		MiniGameDanceNote miniGameDanceNote = _noteObjs.Get(timeKey);
		if (!(miniGameDanceNote == null))
		{
			Vector3 position = miniGameDanceNote.transform.position;
			miniGameDanceNote.HitAndKillObject(timeKey, MoveToPoolCalled);
			ParticleType particleType = ConvertAccuracyToParticleObject(accuracy);
			if (!string.IsNullOrEmpty(particleType.Path))
			{
				ParticleManager.Emit(particleType, position, Quaternion.AngleAxis(MiniGameDanceHelper.GetRotation(direction), Vector3.forward), comeForwardToCamera: false, groundDecal: false, base.transform.lossyScale);
			}
		}
	}

	private IEnumerator CountdownSequence(MiniGameStatus status)
	{
		_countdownLabel.gameObject.SetActive(value: true);
		_countdownTweener.Play();
		yield return this.WaitForNode(status, 1.5f);
	}

	private IEnumerator RythmnGameSequence(MiniGameStatus status)
	{
		Stack<MiniGameDanceAsset.DanceNoteData> notes = new Stack<MiniGameDanceAsset.DanceNoteData>(new Stack<MiniGameDanceAsset.DanceNoteData>(_notes));
		float cur = MiniGameDanceHelper.ElapsedTime;
		while (cur < _status.StartTime + _status.GetDuration() && !(MiniGameDanceHelper.ElapsedTime - _status.StartTime > _status.GetDuration()))
		{
			while (true)
			{
				MiniGameDanceAsset.DanceNoteData danceNoteData = ((notes.Count <= 0) ? null : notes.Peek());
				if (MiniGameDanceHelper.IsInTimeRange(status, danceNoteData, MiniGameDanceHelper.AccuracyToTimeRange(MiniGameStatus.AccuracyType.Normal)))
				{
					status.PressibleNotes.Add(new Pair<MiniGameDanceAsset.DanceNoteData, MiniGameStatus.AccuracyType>(danceNoteData, MiniGameStatus.AccuracyType.None));
					if (notes.Count > 0)
					{
						notes.Pop();
					}
					continue;
				}
				if (MiniGameDanceHelper.IsOverTimeRange(status, danceNoteData, MiniGameDanceHelper.AccuracyToTimeRange(MiniGameStatus.AccuracyType.Normal)))
				{
					if (notes.Count > 0)
					{
						notes.Pop();
					}
					continue;
				}
				break;
			}
			_status.UpdatePressibleNotes(status);
			yield return null;
			cur = MiniGameDanceHelper.ElapsedTime;
		}
		yield return this.WaitForNode(status, _status.GetDuration());
		Connections.Frontend.Send(new MiniGameDanceScore
		{
			Score = _status.TotalScore
		});
		KillGame();
		OpenWindow(MiniGameStatus.Mode.End);
	}

	private IEnumerator NoteSpawnSequence(MiniGameStatus status)
	{
		foreach (MiniGameDanceAsset.DanceNoteData note in _notes)
		{
			yield return this.WaitForNode(status, note.TimeKey - note.TransitionTime);
			if (note.Pattern != 0 && !_noteObjs.ContainsKey(note.TimeKey))
			{
				MiniGameDanceNote noteObject = GetNoteObject();
				_noteObjs.Add(note.TimeKey, noteObject);
				noteObject.Set(_status.StartTime, note, _noteTargetWidget, MoveToPoolCalled);
			}
		}
	}

	private void RegisterKeyboardController()
	{
		GameSystem<InputSystem>.Instance().On(InputCommand.Up, UpCommand);
		GameSystem<InputSystem>.Instance().On(InputCommand.Left, LeftCommand);
		GameSystem<InputSystem>.Instance().On(InputCommand.Right, RightCommand);
		GameSystem<InputSystem>.Instance().On(InputCommand.Down, DownCommand);
		GameSystem<InputSystem>.Instance().On(InputCommand.MiniGameSpace, ClickCommand);
	}

	private void UnregisterKeyboardController()
	{
		GameSystem<InputSystem>.Instance().Off(InputCommand.Up, UpCommand);
		GameSystem<InputSystem>.Instance().Off(InputCommand.Left, LeftCommand);
		GameSystem<InputSystem>.Instance().Off(InputCommand.Right, RightCommand);
		GameSystem<InputSystem>.Instance().Off(InputCommand.Down, DownCommand);
		GameSystem<InputSystem>.Instance().Off(InputCommand.MiniGameSpace, ClickCommand);
	}

	private void ClickCommand(InputCommandMessage msg)
	{
		NoteClicked();
		if (!Application.isEditor)
		{
		}
	}

	private void DownCommand(InputCommandMessage msg)
	{
		_status.IsPressed = true;
		NoteSwiped(base.gameObject, Vector2.down);
		_status.IsPressed = (_status.IsSwipeTried = (_status.IsSuccessfullySwiped = false));
		if (!Application.isEditor)
		{
		}
	}

	private void RightCommand(InputCommandMessage msg)
	{
		_status.IsPressed = true;
		NoteSwiped(base.gameObject, Vector2.right);
		_status.IsPressed = (_status.IsSwipeTried = (_status.IsSuccessfullySwiped = false));
		if (!Application.isEditor)
		{
		}
	}

	private void LeftCommand(InputCommandMessage msg)
	{
		_status.IsPressed = true;
		NoteSwiped(base.gameObject, Vector2.left);
		_status.IsPressed = (_status.IsSwipeTried = (_status.IsSuccessfullySwiped = false));
		if (!Application.isEditor)
		{
		}
	}

	private void UpCommand(InputCommandMessage msg)
	{
		_status.IsPressed = true;
		NoteSwiped(base.gameObject, Vector2.up);
		_status.IsPressed = (_status.IsSwipeTried = (_status.IsSuccessfullySwiped = false));
		if (!Application.isEditor)
		{
		}
	}

	private void MoveToPoolCalled(float timeKey, MiniGameDanceNote obj, bool isFadeOut)
	{
		if (obj != null)
		{
			obj.gameObject.SetActive(value: false);
			_notePool.Push(obj);
			_noteObjs.Remove(timeKey);
		}
		if (isFadeOut)
		{
			_status.AccuracyAccumulatedCount = 0;
		}
	}

	private MiniGameDanceNote GetNoteObject()
	{
		return (_notePool.Count <= 0) ? UnityEngine.Object.Instantiate(_miniGameDanceNotePrefab, _gamePanel.transform) : _notePool.Pop();
	}

	private ParticleType ConvertAccuracyToParticleObject(MiniGameStatus.AccuracyType accuracy)
	{
		return accuracy switch
		{
			MiniGameStatus.AccuracyType.Normal => _normalParticle, 
			MiniGameStatus.AccuracyType.Great => _goodParticle, 
			MiniGameStatus.AccuracyType.Perfect => _greatParticle, 
			_ => default(ParticleType), 
		};
	}

	private void SpawnText(float totalScore, MiniGameStatus.AccuracyType accuracy)
	{
		string text = MiniGameDanceHelper.AccuracyToText(accuracy);
		Color color = MiniGameDanceHelper.AccuracyToColor(accuracy);
		_resultText.text = text;
		_resultText.color = color;
		int accuracyAccumulatedCount = _status.AccuracyAccumulatedCount;
		if (accuracyAccumulatedCount > 0)
		{
			_countdownLabel.gameObject.SetActive(value: false);
			_countdownTweener.Stop();
			_accumulatedNote.gameObject.SetActive(value: true);
		}
		_accumulatedNote.Set(accuracyAccumulatedCount);
		_tweenPlayer.Play();
		_scoreTweener.Begin(totalScore);
	}

	private void StartMusic(string currentMusic, uint musicInstanceId, [NotNull] Action<uint> startSequence)
	{
		StopMusic();
		Action action = delegate
		{
			_status.MusicInstanceId = SoundManager.PlayEvent(currentMusic, SoundPosition.Empty, exclusive: true);
			startSequence(musicInstanceId);
		};
		if (SoundManager.IsPrepared(currentMusic))
		{
			action();
		}
		else
		{
			SoundManager.PrepareEvent(currentMusic, action);
		}
	}

	private void StopMusic()
	{
		if (_status.MusicInstanceId != 0)
		{
			SoundManager.StopEvent(_status.MusicInstanceId);
			_status.MusicInstanceId = 0u;
		}
	}
}
