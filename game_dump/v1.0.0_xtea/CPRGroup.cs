using System.Collections.Generic;
using UnityEngine;

public class CPRGroup : UIBase
{
	private class NoteItem
	{
		public GameObject Note;

		private bool _isCleanHit;

		public TweenAlpha AlphaTweener => (!Object.op_Implicit((Object)(object)Note)) ? null : Note.GetComponent<TweenAlpha>();

		public TweenScale ScaleTweener => (!Object.op_Implicit((Object)(object)Note)) ? null : Note.GetComponent<TweenScale>();

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
					if (Object.op_Implicit((Object)(object)AlphaTweener))
					{
						AlphaTweener.PlayForward();
					}
					if (Object.op_Implicit((Object)(object)ScaleTweener))
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
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				return (!Object.op_Implicit((Object)(object)Note)) ? Vector3.zero : Note.transform.localPosition;
			}
			set
			{
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				if (Object.op_Implicit((Object)(object)Note))
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
			if (!IsCleanHit && Object.op_Implicit((Object)(object)AlphaTweener) && AlphaTweener.value < 1f)
			{
				return true;
			}
			return false;
		}

		public void SetMissNote()
		{
			if (Object.op_Implicit((Object)(object)AlphaTweener))
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

	private Gauge _CPRGauge;

	[SerializeField]
	private GameObject _counter;

	private float _nextCountTime;

	[SerializeField]
	private UILabel _noteProgress;

	[SerializeField]
	private GameObject _score;

	[SerializeField]
	private AudioClipType _missAudio;

	[SerializeField]
	private AudioClipType _coolAudio;

	[SerializeField]
	private ParticleType _coolParticle;

	[SerializeField]
	private AudioClipType _greatAudio;

	[SerializeField]
	private ParticleType _greatParticle;

	private List<NoteItem> _pressNotes = new List<NoteItem>();

	private float _startCPRTime;

	private float _lastHitTime;

	private bool _isCPRStarted;

	private int _hitCount;

	private void Awake()
	{
		for (int i = 0; i < _noteCount; i++)
		{
			NoteItem noteItem = new NoteItem();
			noteItem.AddNote(((Component)_pressNoteBase.transform.parent).gameObject.AddChild(_pressNoteBase.gameObject));
			_pressNotes.Add(noteItem);
		}
		_isCPRStarted = false;
		_CPRGauge = new Gauge(100f, 0f, new GaugeNode[1]
		{
			new GaugeNode
			{
				Time = 0.0,
				Value = 0f
			}
		});
		ParticleManager.Cache(_coolParticle);
		ParticleManager.Cache(_greatParticle);
		SoundManager.Cache(_missAudio);
		SoundManager.Cache(_coolAudio);
		SoundManager.Cache(_greatAudio);
	}

	private void Start()
	{
		UIEventListener.Get(_chestBtn).onClick = delegate
		{
			PressChest();
		};
		base.OnOpenSucceed += StartCPR;
	}

	private void OnEnable()
	{
		GameSystem<CPRSystem>.Instance().CPRInterrupted += CPRSystem_CPRInterrupted;
		GameSystem<CPRSystem>.Instance().CPRStarted += CPRSystem_CPRStarted;
	}

	private void OnDisable()
	{
		GameSystem<CPRSystem>.Instance().CPRInterrupted -= CPRSystem_CPRInterrupted;
		GameSystem<CPRSystem>.Instance().CPRStarted -= CPRSystem_CPRStarted;
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
		if (Object.op_Implicit((Object)(object)component))
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
		if (Object.op_Implicit((Object)(object)component2))
		{
			component2.ResetToBeginning();
			component2.PlayForward();
		}
		TweenScale component3 = _counter.GetComponent<TweenScale>();
		if (Object.op_Implicit((Object)(object)component3))
		{
			component3.ResetToBeginning();
			component3.PlayForward();
		}
		_nextCountTime += 1f;
	}

	private bool CheckFinish()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		if (_pressNotes[_noteCount - 1].Pos.y <= (float)(-Screen.height) || (_pressNotes[_noteCount - 1].IsCleanHit && _lastHitTime + 1f < time))
		{
			return true;
		}
		return false;
	}

	private void UpdateNotes()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
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
			float num3 = Vector3.Distance(_pressNotes[i].Pos, _chestBtn.transform.localPosition);
			if (num3 > (float)_coolRange && _pressNotes[i].Pos.y < _chestBtn.transform.localPosition.y && !_pressNotes[i].IsMissNote())
			{
				AddScore(Judgment.Miss);
				if (Object.op_Implicit((Object)(object)_pressNotes[i].AlphaTweener))
				{
					_pressNotes[i].AlphaTweener.PlayForward();
				}
			}
		}
	}

	public void StartCPR()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		_isCPRStarted = true;
		_startCPRTime = (_nextCountTime = Time.time);
		_lastHitTime = 0f;
		_hitCount = 0;
		for (int i = 0; i < _noteCount; i++)
		{
			_pressNotes[i].IsCleanHit = false;
			_pressNotes[i].Pos = _pressNoteBase.transform.localPosition;
			if (Object.op_Implicit((Object)(object)_pressNotes[i].Note))
			{
				_pressNotes[i].Note.SetActive(true);
			}
			if (Object.op_Implicit((Object)(object)_pressNotes[i].AlphaTweener))
			{
				_pressNotes[i].AlphaTweener.ResetToBeginning();
			}
			if (Object.op_Implicit((Object)(object)_pressNotes[i].ScaleTweener))
			{
				_pressNotes[i].ScaleTweener.ResetToBeginning();
			}
		}
		TweenScale component = _chestBtn.GetComponent<TweenScale>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.ResetToBeginning();
		}
		_CPRGauge.Determination[0].Value = 0f;
		_CPRGauge.Determination[0].Time = Gauge.CurrentTime;
		_CPRGaugeViewer.Set(_CPRGauge, smooth: false);
		UILabel component2 = _counter.GetComponent<UILabel>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			component2.text = "3";
		}
		TweenAlpha component3 = _counter.GetComponent<TweenAlpha>();
		if (Object.op_Implicit((Object)(object)component3))
		{
			component3.ResetToBeginning();
		}
		_noteProgress.text = $"0/{_noteCount}";
	}

	private void PressChest()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		TweenScale component = _chestBtn.GetComponent<TweenScale>();
		if (Object.op_Implicit((Object)(object)component))
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
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		float num = 100f / (float)_noteCount;
		float num2 = 0f;
		switch (judgment)
		{
		case Judgment.Miss:
			num2 = -5f;
			SoundManager.Play((string)_missAudio, loop: false, default(SoundManager.PitchRange));
			break;
		case Judgment.Cool:
			_hitCount++;
			num2 = num * 0.5f;
			SoundManager.Play((string)_coolAudio, loop: false, default(SoundManager.PitchRange));
			ParticleManager.Emit(_coolParticle, MainCamera.NGUIPosToWorldPos(_chestBtn.transform.localPosition), Quaternion.identity);
			break;
		case Judgment.Great:
			_hitCount++;
			num2 = num;
			SoundManager.Play((string)_greatAudio, loop: false, default(SoundManager.PitchRange));
			ParticleManager.Emit(_greatParticle, MainCamera.NGUIPosToWorldPos(_chestBtn.transform.localPosition), Quaternion.identity);
			break;
		}
		_CPRGauge.Determination[0].Time = Gauge.CurrentTime;
		_CPRGauge.Determination[0].Value += num2;
		if (_CPRGauge.Determination[0].Value < 0f)
		{
			_CPRGauge.Determination[0].Value = 0f;
		}
		_CPRGaugeViewer.Set(_CPRGauge);
		GameObject val = ((Component)_score.transform.parent).gameObject.AddChild(_score.gameObject);
		if (Object.op_Implicit((Object)(object)val))
		{
			val.SetActive(true);
			UILabel component = val.GetComponent<UILabel>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.text = ((num2 == num) ? "+++" : ((!(num2 > 0f)) ? "-" : "+"));
			}
			TweenAlpha component2 = val.GetComponent<TweenAlpha>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.PlayForward();
			}
			TweenPosition component3 = val.GetComponent<TweenPosition>();
			if (Object.op_Implicit((Object)(object)component3))
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

	private void FinishCPR(bool interrupted = false)
	{
		if (!interrupted)
		{
			GameSystem<CPRSystem>.Instance().CPRResult(_CPRGauge.Get());
		}
		_isCPRStarted = false;
		ForceClose();
	}
}
