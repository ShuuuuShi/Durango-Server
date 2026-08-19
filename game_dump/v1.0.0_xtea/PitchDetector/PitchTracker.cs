using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace PitchDetector;

internal class PitchTracker
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct PitchRecord
	{
		public int RecordIndex { get; set; }

		public float Pitch { get; set; }

		public int MidiNote { get; set; }

		public int MidiCents { get; set; }
	}

	public delegate void PitchDetectedHandler(PitchTracker sender, PitchRecord pitchRecord);

	private const int kOctaveSteps = 96;

	private const int kStepOverlap = 4;

	private const float kMinFreq = 50f;

	private const float kMaxFreq = 1600f;

	private const int kStartCircular = 40;

	private const float kDetectOverlapSec = 0.005f;

	private const float kMaxOctaveSecRate = 10f;

	private const float kAvgOffset = 0.005f;

	private const int kAvgCount = 1;

	private const float kCircularBufSaveTime = 1f;

	private float m_detectLevelThreshold = 0.01f;

	private int m_pitchRecordsPerSecond = 50;

	private List<PitchRecord> m_pitchRecords = new List<PitchRecord>();

	private PitchRecord m_curPitchRecord = default(PitchRecord);

	private PitchDsp m_dsp;

	private CircularBuffer<float> m_circularBufferLo;

	private CircularBuffer<float> m_circularBufferHi;

	private double m_sampleRate;

	private float[] m_pitchBufLo;

	private float[] m_pitchBufHi;

	private int m_pitchBufSize;

	private int m_samplesPerPitchBlock;

	private int m_curPitchIndex;

	private long m_curPitchSamplePos;

	private int m_detectOverlapSamples;

	private float m_maxOverlapDiff;

	private bool m_recordPitchRecords;

	private int m_pitchRecordHistorySize;

	private IIRFilter m_iirFilterLoLo;

	private IIRFilter m_iirFilterLoHi;

	private IIRFilter m_iirFilterHiLo;

	private IIRFilter m_iirFilterHiHi;

	private PitchDetectedHandler m_pitchDetected;

	public double SampleRate
	{
		set
		{
			if (m_sampleRate != value)
			{
				m_sampleRate = value;
				Setup();
			}
		}
	}

	public float DetectLevelThreshold
	{
		set
		{
			float num = Math.Max(0.0001f, Math.Min(1f, value));
			if ((double)m_detectLevelThreshold != (double)num)
			{
				m_detectLevelThreshold = num;
				Setup();
			}
		}
	}

	public int SamplesPerPitchBlock => m_samplesPerPitchBlock;

	public int PitchRecordsPerSecond
	{
		get
		{
			return m_pitchRecordsPerSecond;
		}
		set
		{
			m_pitchRecordsPerSecond = Math.Max(1, Math.Min(100, value));
			Setup();
		}
	}

	public bool RecordPitchRecords
	{
		get
		{
			return m_recordPitchRecords;
		}
		set
		{
			if (m_recordPitchRecords != value)
			{
				m_recordPitchRecords = value;
				if (!m_recordPitchRecords)
				{
					m_pitchRecords = new List<PitchRecord>();
				}
			}
		}
	}

	public int PitchRecordHistorySize
	{
		get
		{
			return m_pitchRecordHistorySize;
		}
		set
		{
			m_pitchRecordHistorySize = value;
			m_pitchRecords.Capacity = m_pitchRecordHistorySize;
		}
	}

	public IList PitchRecords => m_pitchRecords.AsReadOnly();

	public PitchRecord CurrentPitchRecord => m_curPitchRecord;

	public long CurrentPitchSamplePosition => m_curPitchSamplePos;

	public static float MinDetectFrequency => 50f;

	public static float MaxDetectFrequency => 1600f;

	public static double FrequencyStep => Math.Pow(2.0, 1.0 / 96.0);

	public int DetectSampleOffset => (m_pitchBufSize + m_detectOverlapSamples) / 2;

	public event PitchDetectedHandler PitchDetected
	{
		add
		{
			PitchDetectedHandler pitchDetectedHandler = m_pitchDetected;
			PitchDetectedHandler pitchDetectedHandler2;
			do
			{
				pitchDetectedHandler2 = pitchDetectedHandler;
				pitchDetectedHandler = Interlocked.CompareExchange(ref m_pitchDetected, (PitchDetectedHandler)Delegate.Combine(pitchDetectedHandler2, value), pitchDetectedHandler);
			}
			while (pitchDetectedHandler != pitchDetectedHandler2);
		}
		remove
		{
			PitchDetectedHandler pitchDetectedHandler = m_pitchDetected;
			PitchDetectedHandler pitchDetectedHandler2;
			do
			{
				pitchDetectedHandler2 = pitchDetectedHandler;
				pitchDetectedHandler = Interlocked.CompareExchange(ref m_pitchDetected, (PitchDetectedHandler)Delegate.Remove(pitchDetectedHandler2, value), pitchDetectedHandler);
			}
			while (pitchDetectedHandler != pitchDetectedHandler2);
		}
	}

	public void Reset()
	{
		m_curPitchIndex = 0;
		m_curPitchSamplePos = 0L;
		m_pitchRecords.Clear();
		m_iirFilterLoLo.Reset();
		m_iirFilterLoHi.Reset();
		m_iirFilterHiLo.Reset();
		m_iirFilterHiHi.Reset();
		m_circularBufferLo.Reset();
		m_circularBufferLo.Clear();
		m_circularBufferHi.Reset();
		m_circularBufferHi.Clear();
		m_pitchBufLo.Clear();
		m_pitchBufHi.Clear();
		m_circularBufferLo.StartPosition = -m_detectOverlapSamples;
		m_circularBufferLo.Available = m_detectOverlapSamples;
		m_circularBufferHi.StartPosition = -m_detectOverlapSamples;
		m_circularBufferHi.Available = m_detectOverlapSamples;
	}

	public void ProcessBuffer(float[] inBuffer, int sampleCount = 0)
	{
		if (inBuffer == null)
		{
			throw new ArgumentNullException("inBuffer", "Input buffer cannot be null");
		}
		int i = 0;
		int num2;
		for (int num = ((sampleCount == 0) ? inBuffer.Length : Math.Min(sampleCount, inBuffer.Length)); i < num; i += num2)
		{
			num2 = Math.Min(num - i, m_pitchBufSize + m_detectOverlapSamples);
			m_iirFilterLoLo.FilterBuffer(inBuffer, i, m_pitchBufLo, 0L, num2);
			m_iirFilterLoHi.FilterBuffer(m_pitchBufLo, 0L, m_pitchBufLo, 0L, num2);
			m_iirFilterHiLo.FilterBuffer(inBuffer, i, m_pitchBufHi, 0L, num2);
			m_iirFilterHiHi.FilterBuffer(m_pitchBufHi, 0L, m_pitchBufHi, 0L, num2);
			m_circularBufferLo.WriteBuffer(m_pitchBufLo, num2);
			m_circularBufferHi.WriteBuffer(m_pitchBufHi, num2);
			while (m_circularBufferLo.ReadBuffer(m_pitchBufLo, m_curPitchSamplePos, m_pitchBufSize + m_detectOverlapSamples))
			{
				float pitch = 0f;
				m_circularBufferHi.ReadBuffer(m_pitchBufHi, m_curPitchSamplePos, m_pitchBufSize + m_detectOverlapSamples);
				float num3 = m_dsp.DetectPitch(m_pitchBufLo, m_pitchBufHi, m_pitchBufSize);
				if ((double)num3 > 0.0)
				{
					m_pitchBufLo.Copy(m_pitchBufLo, m_detectOverlapSamples, 0, m_pitchBufSize);
					m_pitchBufHi.Copy(m_pitchBufHi, m_detectOverlapSamples, 0, m_pitchBufSize);
					float num4 = m_dsp.DetectPitch(m_pitchBufLo, m_pitchBufHi, m_pitchBufSize);
					if ((double)num4 > 0.0 && (double)Math.Max(num3, num4) / (double)Math.Min(num3, num4) - 1.0 < (double)m_maxOverlapDiff)
					{
						pitch = (float)(((double)num3 + (double)num4) * 0.5);
					}
				}
				AddPitchRecord(pitch);
				m_curPitchSamplePos += m_samplesPerPitchBlock;
				m_curPitchIndex++;
			}
		}
	}

	private void Setup()
	{
		if (!(m_sampleRate < 1.0))
		{
			m_dsp = new PitchDsp(m_sampleRate, 50f, 1600f, m_detectLevelThreshold);
			m_iirFilterLoLo = new IIRFilter();
			m_iirFilterLoLo.Proto = IIRFilter.ProtoType.Butterworth;
			m_iirFilterLoLo.Type = IIRFilter.FilterType.HP;
			m_iirFilterLoLo.Order = 5;
			m_iirFilterLoLo.FreqLow = 45f;
			m_iirFilterLoLo.SampleRate = (float)m_sampleRate;
			m_iirFilterLoHi = new IIRFilter();
			m_iirFilterLoHi.Proto = IIRFilter.ProtoType.Butterworth;
			m_iirFilterLoHi.Type = IIRFilter.FilterType.LP;
			m_iirFilterLoHi.Order = 5;
			m_iirFilterLoHi.FreqHigh = 280f;
			m_iirFilterLoHi.SampleRate = (float)m_sampleRate;
			m_iirFilterHiLo = new IIRFilter();
			m_iirFilterHiLo.Proto = IIRFilter.ProtoType.Butterworth;
			m_iirFilterHiLo.Type = IIRFilter.FilterType.HP;
			m_iirFilterHiLo.Order = 5;
			m_iirFilterHiLo.FreqLow = 45f;
			m_iirFilterHiLo.SampleRate = (float)m_sampleRate;
			m_iirFilterHiHi = new IIRFilter();
			m_iirFilterHiHi.Proto = IIRFilter.ProtoType.Butterworth;
			m_iirFilterHiHi.Type = IIRFilter.FilterType.LP;
			m_iirFilterHiHi.Order = 5;
			m_iirFilterHiHi.FreqHigh = 1500f;
			m_iirFilterHiHi.SampleRate = (float)m_sampleRate;
			m_detectOverlapSamples = (int)(0.00499999988824129 * m_sampleRate);
			m_maxOverlapDiff = 0.05f;
			m_pitchBufSize = (int)(0.0399999991059303 * m_sampleRate) + 16;
			m_pitchBufLo = new float[m_pitchBufSize + m_detectOverlapSamples];
			m_pitchBufHi = new float[m_pitchBufSize + m_detectOverlapSamples];
			m_samplesPerPitchBlock = (int)Math.Round(m_sampleRate / (double)m_pitchRecordsPerSecond);
			m_circularBufferLo = new CircularBuffer<float>((int)(1.0 * m_sampleRate + 0.5) + 10000);
			m_circularBufferHi = new CircularBuffer<float>((int)(1.0 * m_sampleRate + 0.5) + 10000);
		}
	}

	private void AddPitchRecord(float pitch)
	{
		int note = 0;
		int cents = 0;
		PitchDsp.PitchToMidiNote(pitch, out note, out cents);
		PitchRecord pitchRecord = default(PitchRecord);
		pitchRecord.RecordIndex = m_curPitchIndex;
		pitchRecord.Pitch = pitch;
		pitchRecord.MidiNote = note;
		pitchRecord.MidiCents = cents;
		m_curPitchRecord = pitchRecord;
		if (m_recordPitchRecords)
		{
			if (m_pitchRecordHistorySize > 0 && m_pitchRecords.Count >= m_pitchRecordHistorySize)
			{
				m_pitchRecords.RemoveAt(0);
			}
			m_pitchRecords.Add(pitchRecord);
		}
		if (m_pitchDetected != null)
		{
			m_pitchDetected(this, pitchRecord);
		}
	}
}
