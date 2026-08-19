using System.Collections.Generic;
using Durango.Render.Camera;
using Durango.Render.Particle;
using Durango.UI.Control;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class CPRGroup : UIBase
{
	private class NoteItem
	{
		public GameObject Note;

		private bool _isCleanHit;

		public TweenAlpha AlphaTweener
		{
			get
			{
				if ((bool)Note)
				{
					return Note.GetComponent<TweenAlpha>();
				}
				return null;
			}
		}

		public TweenScale ScaleTweener
		{
			get
			{
				if ((bool)Note)
				{
					return Note.GetComponent<TweenScale>();
				}
				return null;
			}
		}

		public bool IsCleanHit
		{
			get
			{
				return _isCleanHit;
			}
			set
			{
				_isCleanHit = value;
				if (_isCleanHit)
				{
					if ((bool)AlphaTweener)
					{
						AlphaTweener.PlayForward();
					}
					if ((bool)ScaleTweener)
					{
						ScaleTweener.PlayForward();
					}
				}
			}
		}

		public Vector3 Pos
		{
			get
			{
				if ((bool)Note)
				{
					return Note.transform.localPosition;
				}
				return Vector3.zero;
			}
			set
			{
				if ((bool)Note)
				{
					Note.transform.localPosition = value;
				}
			}
		}

		public void AddNote(GameObject obj)
		{
			Note = obj;
			IsCleanHit = false;
		}

		public bool IsMissNote()
		{
			if (!IsCleanHit && (bool)AlphaTweener && AlphaTweener.value < 1f)
			{
				return true;
			}
			return false;
		}

		public void SetMissNote()
		{
			if ((bool)AlphaTweener)
			{
				AlphaTweener.PlayForward();
			}
		}
	}

	private enum Judgment
	{
		Miss,
		Cool,
		Great
	}

	[SerializeField]
	private int _noteCount;

	[SerializeField]
	private float _delay;

	[SerializeField]
	private int _noteSpeed;

	[SerializeField]
	private float _pressBPM;

	[SerializeField]
	private int _greatRange;

	[SerializeField]
	private int _coolRange;

	[SerializeField]
	private GameObject _chestBtn;

	[SerializeField]
	private GameObject _pressNoteBase;

	[SerializeField]
	private HyperGaugeViewer _CPRGaugeViewer;

	private Gauge _cprGauge;

	[SerializeField]
	private GameObject _counter;

	private float _nextCountTime;

	[SerializeField]
	private UILabel _noteProgress;

	[SerializeField]
	private GameObject _score;

	[SerializeField]
	private SoundEventType _missAudio;

	[SerializeField]
	private SoundEventType _coolAudio;

	[SerializeField]
	private ParticleType _coolParticle;

	[SerializeField]
	private SoundEventType _greatAudio;

	[SerializeField]
	private ParticleType _greatParticle;

	private List<NoteItem> _pressNotes = new List<NoteItem>();

	private float _startCPRTime;

	private float _lastHitTime;

	private bool _isCPRStarted;

	private int _hitCount;

	private uint _cprSoundInstanceId;

	public static bool IsShow { get; private set; }

	private void Awake()
	{
		for (int i = 0; i < _noteCount; i++)
		{
			NoteItem noteItem = new NoteItem();
			noteItem.AddNote(_pressNoteBase.transform.parent.gameObject.AddChild(_pressNoteBase.gameObject));
			_pressNotes.Add(noteItem);
		}
		_isCPRStarted = false;
		_cprGauge = new Gauge(100f, 0f, new GaugeNode[1]
		{
			new GaugeNode
			{
				Time = 0.0,
				Value = 0f
			}
		});
		ParticleManager.Cache(_coolParticle);
		ParticleManager.Cache(_greatParticle);
		SoundManager.PrepareEvent(_missAudio);
		SoundManager.PrepareEvent(_coolAudio);
		SoundManager.PrepareEvent(_greatAudio);
	}

	private void Start()
	{
		UIEventListener.Get(_chestBtn).onPress = delegate(GameObject go, bool isPressEvent)
		{
			if (isPressEvent)
			{
				PressChest();
			}
		};
		base.OnOpenSucceed += StartCPR;
		GameSystem<InputSystem>.Instance().On(InputCommand.MiniGameSpace, OnReceivedMessage);
		GameSystem<CPRSystem>.Instance().CPRInterrupted += CPRSystem_CPRInterrupted;
		GameSystem<CPRSystem>.Instance().CPRStarted += CPRSystem_CPRStarted;
		SetChildrenActive(activated: false);
	}

	private void Update()
	{
		if (_isCPRStarted)
		{
			TweenCountUI();
			if (CheckFinish())
			{
				FinishCPR();
			}
			else
			{
				UpdateNotes();
			}
		}
	}

	private void TweenCountUI()
	{
		float time = Time.time;
		if (_startCPRTime + 3f <= time || !(_nextCountTime <= time))
		{
			return;
		}
		UILabel component = _counter.GetComponent<UILabel>();
		if ((bool)component)
		{
			if (_startCPRTime + 2f <= _nextCountTime)
			{
				component.text = "1";
			}
			else if (_startCPRTime + 1f <= _nextCountTime)
			{
				component.text = "2";
			}
		}
		TweenAlpha component2 = _counter.GetComponent<TweenAlpha>();
		if ((bool)component2)
		{
			component2.ResetToBeginning();
			component2.PlayForward();
		}
		TweenScale component3 = _counter.GetComponent<TweenScale>();
		if ((bool)component3)
		{
			component3.ResetToBeginning();
			component3.PlayForward();
		}
		_nextCountTime += 1f;
	}

	private bool CheckFinish()
	{
		float time = Time.time;
		if (_pressNotes[_noteCount - 1].Pos.y <= (float)(-Screen.height) || (_pressNotes[_noteCount - 1].IsCleanHit && _lastHitTime + 1f < time))
		{
			return true;
		}
		return false;
	}

	private void UpdateNotes()
	{
		float time = Time.time;
		for (int i = 0; i < _noteCount; i++)
		{
			if (_pressNotes[i].IsCleanHit)
			{
				continue;
			}
			float num = (float)i * _pressBPM + _delay;
			if (!(_startCPRTime + num <= time))
			{
				continue;
			}
			Vector3 localPosition = _pressNoteBase.transform.localPosition;
			float num2 = time - _startCPRTime - num;
			localPosition.y -= (float)_noteSpeed * num2;
			_pressNotes[i].Pos = localPosition;
			if (Vector3.Distance(_pressNotes[i].Pos, _chestBtn.transform.localPosition) > (float)_coolRange && _pressNotes[i].Pos.y < _chestBtn.transform.localPosition.y && !_pressNotes[i].IsMissNote())
			{
				AddScore(Judgment.Miss);
				if ((bool)_pressNotes[i].AlphaTweener)
				{
					_pressNotes[i].AlphaTweener.PlayForward();
				}
			}
		}
	}

	private void StartCPR()
	{
		IsShow = true;
		_isCPRStarted = true;
		_startCPRTime = (_nextCountTime = Time.time);
		_lastHitTime = 0f;
		_hitCount = 0;
		for (int i = 0; i < _noteCount; i++)
		{
			_pressNotes[i].IsCleanHit = false;
			_pressNotes[i].Pos = _pressNoteBase.transform.localPosition;
			if ((bool)_pressNotes[i].Note)
			{
				_pressNotes[i].Note.SetActive(value: true);
			}
			if ((bool)_pressNotes[i].AlphaTweener)
			{
				_pressNotes[i].AlphaTweener.ResetToBeginning();
			}
			if ((bool)_pressNotes[i].ScaleTweener)
			{
				_pressNotes[i].ScaleTweener.ResetToBeginning();
			}
		}
		TweenScale component = _chestBtn.GetComponent<TweenScale>();
		if ((bool)component)
		{
			component.ResetToBeginning();
		}
		_cprGauge.Determination[0].Value = 0f;
		_cprGauge.Determination[0].Time = Gauge.CurrentTime;
		_CPRGaugeViewer.Set(_cprGauge, smooth: false);
		UILabel component2 = _counter.GetComponent<UILabel>();
		if ((bool)component2)
		{
			component2.text = 3.ToString();
		}
		TweenAlpha component3 = _counter.GetComponent<TweenAlpha>();
		if ((bool)component3)
		{
			component3.ResetToBeginning();
		}
		_noteProgress.text = $"0/{_noteCount}";
		PlayCprSound();
	}

	private void PressChest()
	{
		TweenScale component = _chestBtn.GetComponent<TweenScale>();
		if ((bool)component)
		{
			component.ResetToBeginning();
			component.PlayForward();
		}
		for (int i = 0; i < _noteCount; i++)
		{
			if (!_pressNotes[i].IsCleanHit && !_pressNotes[i].IsMissNote())
			{
				float num = Vector3.Distance(_pressNotes[i].Pos, _chestBtn.transform.localPosition);
				if (num < (float)_greatRange)
				{
					_pressNotes[i].IsCleanHit = true;
					AddScore(Judgment.Great);
				}
				else if (num < (float)_coolRange)
				{
					_pressNotes[i].IsCleanHit = true;
					AddScore(Judgment.Cool);
				}
				else
				{
					AddScore(Judgment.Miss);
				}
				_lastHitTime = Time.time;
				break;
			}
		}
	}

	private void AddScore(Judgment judgment)
	{
		float num = 100f / (float)_noteCount;
		float num2 = 0f;
		switch (judgment)
		{
		case Judgment.Miss:
			num2 = -5f;
			SoundManager.PlayEvent(_missAudio);
			break;
		case Judgment.Cool:
			_hitCount++;
			num2 = num * 0.5f;
			SoundManager.PlayEvent(_coolAudio);
			ParticleManager.Emit(_coolParticle, MainCamera.NGUIPosToWorldPos(_chestBtn.transform.localPosition), Quaternion.identity);
			break;
		case Judgment.Great:
			_hitCount++;
			num2 = num;
			SoundManager.PlayEvent(_greatAudio);
			ParticleManager.Emit(_greatParticle, MainCamera.NGUIPosToWorldPos(_chestBtn.transform.localPosition), Quaternion.identity);
			break;
		}
		_cprGauge.Determination[0].Time = Gauge.CurrentTime;
		_cprGauge.Determination[0].Value += num2;
		if (_cprGauge.Determination[0].Value < 0f)
		{
			_cprGauge.Determination[0].Value = 0f;
		}
		_CPRGaugeViewer.Set(_cprGauge);
		GameObject gameObject = _score.transform.parent.gameObject.AddChild(_score.gameObject);
		if ((bool)gameObject)
		{
			gameObject.SetActive(value: true);
			UILabel component = gameObject.GetComponent<UILabel>();
			if ((bool)component)
			{
				component.text = ((num2 == num) ? "+++" : ((!(num2 > 0f)) ? "-" : "+"));
			}
			TweenAlpha component2 = gameObject.GetComponent<TweenAlpha>();
			if ((bool)component2)
			{
				component2.PlayForward();
			}
			TweenPosition component3 = gameObject.GetComponent<TweenPosition>();
			if ((bool)component3)
			{
				component3.PlayForward();
			}
		}
		_noteProgress.text = $"{_hitCount}/{_noteCount}";
	}

	private void CPRSystem_CPRStarted()
	{
		Open();
	}

	private void CPRSystem_CPRInterrupted()
	{
		FinishCPR(interrupted: true);
	}

	public void FinishCPR(bool interrupted = false)
	{
		if (!interrupted)
		{
			GameSystem<CPRSystem>.Instance().CPRResult(_cprGauge.Get());
		}
		_isCPRStarted = false;
		IsShow = false;
		StopCprSound();
		ForceClose();
	}

	private void PlayCprSound()
	{
		StopCprSound();
		_cprSoundInstanceId = SoundManager.PlayEvent(Singleton<BgmManager>.Instance().CprSound, SoundPosition.Empty, exclusive: true);
	}

	private void StopCprSound()
	{
		if (_cprSoundInstanceId != 0)
		{
			SoundManager.StopEvent(_cprSoundInstanceId);
			_cprSoundInstanceId = 0u;
		}
	}

	private void OnReceivedMessage(InputCommandMessage msg)
	{
		PressChest();
	}
}
