using System;

namespace PitchDetector;

internal class PitchDsp
{
	private const int kCourseOctaveSteps = 96;

	private const int kScanHiSize = 31;

	private const float kScanHiFreqStep = 1.005f;

	private const int kMinMidiNote = 21;

	private const int kMaxMidiNote = 108;

	public static readonly double InverseLog2 = 1.0 / Math.Log10(2.0);

	private float[] m_scanHiOffset = new float[31];

	private float[] m_peakBuf = new float[31];

	private float m_minPitch;

	private float m_maxPitch;

	private int m_minNote;

	private int m_maxNote;

	private int m_blockLen14;

	private int m_blockLen24;

	private int m_blockLen34;

	private int m_blockLen44;

	private double m_sampleRate;

	private float m_detectLevelThreshold;

	private int m_numCourseSteps;

	private float[] m_pCourseFreqOffset;

	private float[] m_pCourseFreq;

	private int m_prevPitchIdx;

	private float[] m_detectCurve;

	public float MaxPitch => m_maxPitch;

	public float MinPitch => m_minPitch;

	public int MaxNote => m_maxNote;

	public int MinNote => m_minNote;

	public PitchDsp(double sampleRate, float minPitch, float maxPitch, float detectLevelThreshold)
	{
		m_sampleRate = sampleRate;
		m_minPitch = minPitch;
		m_maxPitch = maxPitch;
		m_detectLevelThreshold = detectLevelThreshold;
		m_minNote = (int)((double)PitchToMidiNote(m_minPitch) + 0.5) + 2;
		m_maxNote = (int)((double)PitchToMidiNote(m_maxPitch) + 0.5) - 2;
		m_blockLen44 = (int)(m_sampleRate / (double)m_minPitch + 0.5);
		m_blockLen34 = m_blockLen44 * 3 / 4;
		m_blockLen24 = m_blockLen44 / 2;
		m_blockLen14 = m_blockLen44 / 4;
		m_numCourseSteps = (int)(Math.Log((double)m_maxPitch / (double)m_minPitch) / Math.Log(2.0) * 96.0 + 0.5) + 3;
		m_pCourseFreqOffset = new float[m_numCourseSteps + 10000];
		m_pCourseFreq = new float[m_numCourseSteps + 10000];
		m_detectCurve = new float[m_numCourseSteps];
		double num = 1.0 / Math.Pow(2.0, 1.0 / 96.0);
		double num2 = (double)m_maxPitch / num;
		for (int i = 0; i < m_numCourseSteps; i++)
		{
			m_pCourseFreq[i] = (float)num2;
			m_pCourseFreqOffset[i] = (float)(m_sampleRate / num2);
			num2 *= num;
		}
		for (int j = 0; j < 31; j++)
		{
			m_scanHiOffset[j] = (float)Math.Pow(1.00499999523163, 15 - j);
		}
	}

	public float DetectPitch(float[] samplesLo, float[] samplesHi, int numSamples)
	{
		if (!LevelIsAbove(samplesLo, numSamples, m_detectLevelThreshold) && !LevelIsAbove(samplesHi, numSamples, m_detectLevelThreshold))
		{
			return 0f;
		}
		return DetectPitchLo(samplesLo, samplesHi);
	}

	private float DetectPitchLo(float[] samplesLo, float[] samplesHi)
	{
		m_detectCurve.Clear();
		float num = 200f;
		float num2 = 600f;
		bool flag = false;
		for (int i = 0; i < m_numCourseSteps; i += 8)
		{
			int num3 = Math.Min(m_blockLen44, (int)m_pCourseFreqOffset[i] * 2);
			float[] samples;
			if (i >= 258)
			{
				if (!flag)
				{
					m_detectCurve.Clear(247, 269);
					flag = true;
				}
				samples = samplesLo;
			}
			else
			{
				samples = samplesHi;
			}
			int stepSize = num3 / 10;
			int stepSize2 = Math.Max(1, Math.Min(5, i * 5 / m_numCourseSteps));
			if (!((double)RatioAbsDiffLinear(samples, i, num3, stepSize, hiRes: false) > (double)num))
			{
				continue;
			}
			int num4 = -1;
			float num5 = 0f;
			float num6 = 0f;
			int num7 = 4;
			int j = i;
			int num8 = Math.Max(i - 11, 0);
			for (int num9 = Math.Min(i + 11, m_numCourseSteps - 1); j >= num8 && j < num9; j += num7)
			{
				float num10 = RatioAbsDiffLinear(samples, j, num3, stepSize2, hiRes: true);
				if ((double)num5 < (double)num10)
				{
					num5 = num10;
					num4 = j;
				}
				if ((double)num6 > (double)num10)
				{
					num7 = -num7 >> 1;
					if (num7 == 0)
					{
						if ((double)num5 > (double)num2 && num4 >= 6 && num4 <= m_numCourseSteps - 7)
						{
							float num11 = RatioAbsDiffLinear(samples, num4 - 5, num3, stepSize2, hiRes: true);
							float num12 = RatioAbsDiffLinear(samples, num4 + 5, num3, stepSize2, hiRes: true);
							if ((double)num5 / ((double)num11 + (double)num12) * 2.0 > ((m_prevPitchIdx > 0 && Math.Abs(m_prevPitchIdx - num4) < 10) ? 1.20000004768372 : 1.5))
							{
								float num13 = DetectPitchHi(samples, num4);
								if ((double)num13 > 1.0)
								{
									m_prevPitchIdx = num4;
									return num13;
								}
								break;
							}
							break;
						}
						break;
					}
				}
				num6 = num10;
			}
		}
		m_prevPitchIdx = 0;
		return 0f;
	}

	private float DetectPitchHi(float[] samples, int lowFreqIdx)
	{
		int num = -1;
		float num2 = 0f;
		int num3 = 4;
		int i = 15;
		m_peakBuf.Clear();
		float num4 = m_pCourseFreqOffset[lowFreqIdx];
		for (; i >= 0 && i < 31; i += num3)
		{
			if ((double)m_peakBuf[i] == 0.0)
			{
				m_peakBuf[i] = SumAbsDiffHermite(samples, num4 * m_scanHiOffset[i], m_blockLen44, 1);
			}
			if (num < 0 || (double)m_peakBuf[num] < (double)m_peakBuf[i])
			{
				num = i;
			}
			if ((double)num2 > (double)m_peakBuf[i])
			{
				num3 = -num3 >> 1;
				if (num3 == 0)
				{
					float num5 = Math.Min(m_peakBuf[num - 1], m_peakBuf[num + 1]);
					float num6 = num5 - num5 * (1f / 32f);
					float num7 = (float)Math.Log10((double)m_peakBuf[num - 1] - (double)num6);
					float num8 = (float)Math.Log10((double)m_peakBuf[num] - (double)num6);
					float num9 = (float)Math.Log10((double)m_peakBuf[num + 1] - (double)num6);
					return (float)Math.Pow(1.00499999523163, (double)((float)num + (float)(((double)num9 - (double)num7) / (2.0 * (2.0 * (double)num8 - (double)num7 - (double)num9)))) - 15.0) * m_pCourseFreq[lowFreqIdx];
				}
			}
			num2 = m_peakBuf[i];
		}
		return 0f;
	}

	public static double CreateSineWave(float[] buffer, int numSamples, float sampleRate, float freq, float amplitude, double startAngle)
	{
		double num = (double)freq / (double)sampleRate * Math.PI * 2.0;
		double num2 = startAngle;
		for (int i = 0; i < numSamples; i++)
		{
			buffer[i] = (float)Math.Sin(num2) * amplitude;
			for (num2 += num; num2 > Math.PI; num2 -= Math.PI * 2.0)
			{
			}
		}
		return num2;
	}

	public bool LevelIsAbove(float[] buffer, int len, float level)
	{
		if (buffer == null || buffer.Length == 0)
		{
			return false;
		}
		int num = Math.Min(buffer.Length, len);
		for (int i = 0; i < num; i++)
		{
			if ((double)Math.Abs(buffer[i]) >= (double)level)
			{
				return true;
			}
		}
		return false;
	}

	public static void CopyBuffer<T>(T[] source, int srcStart, T[] destination, int dstStart, int length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (source == null || source.Length < srcStart + length)
		{
			throw new Exception("Source buffer is null or not large enough");
		}
		if (destination == null || destination.Length < dstStart + length)
		{
			throw new Exception("Destination buffer is null or not large enough");
		}
		int num = srcStart;
		int num2 = dstStart;
		for (int i = 0; i < length; i++)
		{
			destination[num2++] = source[num++];
		}
	}

	private float InterpolateHermite(float fY0, float fY1, float fY2, float fY3, float frac)
	{
		float num = (float)(0.5 * ((double)fY2 - (double)fY0));
		float num2 = (float)(1.5 * ((double)fY1 - (double)fY2) + 0.5 * ((double)fY3 - (double)fY0));
		float num3 = fY0 - fY1 + num - num2;
		return ((num2 * frac + num3) * frac + num) * frac + fY1;
	}

	private float InterpolateLinear(float y0, float y1, float frac)
	{
		return (float)((double)y0 * (1.0 - (double)frac) + (double)y1 * (double)frac);
	}

	private float RatioAbsDiffLinear(float[] samples, int freqIdx, int blockLen, int stepSize, bool hiRes)
	{
		if (hiRes && (double)m_detectCurve[freqIdx] > 0.0)
		{
			return m_detectCurve[freqIdx];
		}
		int num = (int)m_pCourseFreqOffset[freqIdx];
		float frac = m_pCourseFreqOffset[freqIdx] - (float)num;
		float num2 = 0f;
		float num3 = 0.01f;
		int num4 = 0;
		int num5 = 0;
		while (num5 < blockLen)
		{
			float num6 = samples[num5];
			float num7 = InterpolateLinear(samples[num + num5], samples[num + num5 + 1], frac);
			num3 += Math.Abs(num6 - num7);
			num2 += Math.Abs(num6) + Math.Abs(num7);
			num5 += stepSize;
			num4++;
		}
		float num8 = (float)((double)num2 / (double)num3 * 100.0);
		if (hiRes)
		{
			m_detectCurve[freqIdx] = num8;
		}
		return num8;
	}

	private float SumAbsDiffHermite(float[] samples, float fOffset, int blockLen, int stepSize)
	{
		int num = (int)fOffset;
		float frac = fOffset - (float)num;
		float num2 = 0.001f;
		int num3 = 0;
		int num4 = 0;
		while (num4 < blockLen)
		{
			int num5 = num + num4;
			num2 += Math.Abs(samples[num4] - InterpolateHermite(samples[num5 - 1], samples[num5], samples[num5 + 1], samples[num5 + 2], frac));
			num4 += stepSize;
			num3++;
		}
		return (float)num3 / num2;
	}

	public static bool PitchToMidiNote(float pitch, out int note, out int cents)
	{
		if ((double)pitch < 20.0)
		{
			note = 0;
			cents = 0;
			return false;
		}
		float num = (float)(12.0 * Math.Log10((double)pitch / 55.0) * InverseLog2) + 33f;
		note = (int)((double)num + 0.5);
		cents = (int)(((double)note - (double)num) * 100.0);
		return true;
	}

	public static float PitchToMidiNote(float pitch)
	{
		if ((double)pitch < 20.0)
		{
			return 0f;
		}
		return (float)(12.0 * Math.Log10((double)pitch / 55.0) * InverseLog2) + 33f;
	}

	public float MidiNoteToPitch(float note)
	{
		if ((double)note < 33.0)
		{
			return 0f;
		}
		float num = (float)Math.Pow(10.0, ((double)note - 33.0) / InverseLog2 / 12.0) * 55f;
		if ((double)num <= (double)m_maxPitch)
		{
			return num;
		}
		return 0f;
	}

	public static string GetNoteName(int note, bool sharps, bool showOctave)
	{
		if (note < 21 || note > 108)
		{
			return null;
		}
		note -= 21;
		int num = (note + 9) / 12;
		note %= 12;
		string text = null;
		switch (note)
		{
		case 0:
			text = "A";
			break;
		case 1:
			text = (sharps ? "A#" : "Bb");
			break;
		case 2:
			text = "B";
			break;
		case 3:
			text = "C";
			break;
		case 4:
			text = (sharps ? "C#" : "Db");
			break;
		case 5:
			text = "D";
			break;
		case 6:
			text = (sharps ? "D#" : "Eb");
			break;
		case 7:
			text = "E";
			break;
		case 8:
			text = "F";
			break;
		case 9:
			text = (sharps ? "F#" : "Gb");
			break;
		case 10:
			text = "G";
			break;
		case 11:
			text = (sharps ? "G#" : "Ab");
			break;
		}
		if (showOctave)
		{
			text = text + " " + num;
		}
		return text;
	}
}
