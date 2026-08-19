using System;

namespace PitchDetector;

internal class IIRFilter
{
	public enum FilterType
	{
		None,
		LP,
		HP,
		BP
	}

	public enum ProtoType
	{
		None,
		Butterworth,
		Chebyshev
	}

	private const int kHistMask = 31;

	private const int kHistSize = 32;

	private int m_order;

	private ProtoType m_protoType;

	private FilterType m_filterType;

	private float m_fp1;

	private float m_fp2;

	private float m_fN;

	private float m_ripple;

	private float m_sampleRate;

	private double[] m_real;

	private double[] m_imag;

	private double[] m_z;

	private double[] m_aCoeff;

	private double[] m_bCoeff;

	private double[] m_inHistory;

	private double[] m_outHistory;

	private int m_histIdx;

	private bool m_invertDenormal;

	public bool FilterValid
	{
		get
		{
			if (m_order < 1 || m_order > 16 || m_protoType == ProtoType.None || m_filterType == FilterType.None || (double)m_sampleRate <= 0.0 || (double)m_fN <= 0.0)
			{
				return false;
			}
			switch (m_filterType)
			{
			case FilterType.LP:
				if ((double)m_fp2 <= 0.0)
				{
					return false;
				}
				break;
			case FilterType.HP:
				if ((double)m_fp1 <= 0.0)
				{
					return false;
				}
				break;
			case FilterType.BP:
				if ((double)m_fp1 <= 0.0 || (double)m_fp2 <= 0.0 || (double)m_fp1 >= (double)m_fp2)
				{
					return false;
				}
				break;
			}
			return m_filterType != FilterType.BP || (m_order & 1) == 0;
		}
	}

	public ProtoType Proto
	{
		get
		{
			return m_protoType;
		}
		set
		{
			m_protoType = value;
			Design();
		}
	}

	public FilterType Type
	{
		get
		{
			return m_filterType;
		}
		set
		{
			m_filterType = value;
			Design();
		}
	}

	public int Order
	{
		get
		{
			return m_order;
		}
		set
		{
			m_order = Math.Min(16, Math.Max(1, Math.Abs(value)));
			if (m_filterType == FilterType.BP && Odd(m_order))
			{
				m_order++;
			}
			Design();
		}
	}

	public float SampleRate
	{
		get
		{
			return m_sampleRate;
		}
		set
		{
			m_sampleRate = value;
			m_fN = 0.5f * m_sampleRate;
			Design();
		}
	}

	public float FreqLow
	{
		get
		{
			return m_fp1;
		}
		set
		{
			m_fp1 = value;
			Design();
		}
	}

	public float FreqHigh
	{
		get
		{
			return m_fp2;
		}
		set
		{
			m_fp2 = value;
			Design();
		}
	}

	public float Ripple
	{
		get
		{
			return m_ripple;
		}
		set
		{
			m_ripple = value;
			Design();
		}
	}

	private bool Odd(int n)
	{
		return (n & 1) == 1;
	}

	private float Sqr(float value)
	{
		return value * value;
	}

	private double Sqr(double value)
	{
		return value * value;
	}

	private void LocatePolesAndZeros()
	{
		m_real = new double[m_order + 1];
		m_imag = new double[m_order + 1];
		m_z = new double[m_order + 1];
		double num = Math.Log(10.0);
		int num2 = m_order;
		if (m_filterType == FilterType.BP)
		{
			num2 /= 2;
		}
		int num3 = num2 % 2;
		int num4 = num2 + num3;
		int num5 = (3 * num2 + num3) / 2 - 1;
		double num6 = Math.Tan(Math.PI / 2.0 * m_filterType switch
		{
			FilterType.LP => m_fp2, 
			FilterType.HP => (double)m_fN - (double)m_fp1, 
			FilterType.BP => (double)m_fp2 - (double)m_fp1, 
			_ => 0.0, 
		} / (double)m_fN);
		double num7 = Sqr(num6);
		double num8 = 1.0;
		double num9 = 1.0;
		double value = 1.0;
		for (int i = num4; i <= num5; i++)
		{
			double num10 = 0.5 * (double)(2 * i + 1 - num3) * Math.PI / (double)num2;
			switch (m_protoType)
			{
			case ProtoType.Butterworth:
			{
				double num17 = 1.0 - 2.0 * num6 * Math.Cos(num10) + num7;
				num9 = (1.0 - num7) / num17;
				value = 2.0 * num6 * Math.Sin(num10) / num17;
				break;
			}
			case ProtoType.Chebyshev:
			{
				double num11 = 1.0 / Math.Sqrt(1.0 / Sqr(1.0 - (1.0 - Math.Exp(-0.05 * (double)m_ripple * num))) - 1.0);
				double num12 = Math.Pow(Math.Sqrt(num11 * num11 + 1.0) + num11, 1.0 / (double)num2);
				num8 = 0.5 * (num12 - 1.0 / num12);
				double num13 = 0.5 * (num12 + 1.0 / num12);
				double num14 = num8 * num6 * Math.Cos(num10);
				double num15 = num13 * num6 * Math.Sin(num10);
				double num16 = Sqr(1.0 - num14) + Sqr(num15);
				num9 = 2.0 * (1.0 - num14) / num16 - 1.0;
				value = 2.0 * num15 / num16;
				break;
			}
			}
			int num18 = 2 * (num5 - i) + 1;
			m_real[num18 + num3] = num9;
			m_imag[num18 + num3] = Math.Abs(value);
			m_real[num18 + num3 + 1] = num9;
			m_imag[num18 + num3 + 1] = 0.0 - Math.Abs(value);
		}
		if (Odd(num2))
		{
			if (m_protoType == ProtoType.Butterworth)
			{
				num9 = (1.0 - num7) / (1.0 + 2.0 * num6 + num7);
			}
			if (m_protoType == ProtoType.Chebyshev)
			{
				num9 = 2.0 / (1.0 + num8 * num6) - 1.0;
			}
			m_real[1] = num9;
			m_imag[1] = 0.0;
		}
		switch (m_filterType)
		{
		case FilterType.LP:
		{
			for (int m = 1; m <= num2; m++)
			{
				m_z[m] = -1.0;
			}
			break;
		}
		case FilterType.HP:
		{
			for (int l = 1; l <= num2; l++)
			{
				m_real[l] = 0.0 - m_real[l];
				m_z[l] = 1.0;
			}
			break;
		}
		case FilterType.BP:
		{
			for (int j = 1; j <= num2; j++)
			{
				m_z[j] = 1.0;
				m_z[j + num2] = -1.0;
			}
			double num19 = Math.PI / 2.0 * (double)m_fp1 / (double)m_fN;
			double num20 = Math.PI / 2.0 * (double)m_fp2 / (double)m_fN;
			double num21 = Math.Cos(num19 + num20) / Math.Cos(num20 - num19);
			for (int k = 0; k <= (m_order - 1) / 2; k++)
			{
				int num22 = 1 + 2 * k;
				double num23 = m_real[num22];
				double num24 = m_imag[num22];
				double num27;
				double num28;
				double num29;
				double num30;
				if (Math.Abs(num24) < 0.0001)
				{
					double num25 = 0.5 * num21 * (1.0 + num23);
					double num26 = Sqr(num25) - num23;
					if (num26 > 0.0)
					{
						num27 = num25 + Math.Sqrt(num26);
						num28 = num25 - Math.Sqrt(num26);
						num29 = 0.0;
						num30 = 0.0;
					}
					else
					{
						num27 = num25;
						num28 = num25;
						num29 = Math.Sqrt(Math.Abs(num26));
						num30 = 0.0 - num29;
					}
				}
				else
				{
					double num31 = num21 * 0.5 * (1.0 + num23);
					double num32 = num21 * 0.5 * num24;
					double num33 = Sqr(num31) - Sqr(num32) - num23;
					double num34 = 2.0 * num31 * num32 - num24;
					double num35 = Math.Sqrt(0.5 * Math.Abs(num33 + Math.Sqrt(Sqr(num33) + Sqr(num34))));
					double num36 = num34 / (2.0 * num35);
					num27 = num31 + num35;
					num29 = num32 + num36;
					num28 = num31 - num35;
					num30 = num32 - num36;
				}
				m_real[num22] = num27;
				m_real[num22 + 1] = num28;
				m_imag[num22] = num29;
				m_imag[num22 + 1] = num30;
			}
			if (Odd(num2))
			{
				m_real[2] = m_real[num2 + 1];
				m_imag[2] = m_imag[num2 + 1];
			}
			for (int num37 = num2; num37 >= 1; num37--)
			{
				int num38 = 2 * num37 - 1;
				m_real[num38] = m_real[num37];
				m_real[num38 + 1] = m_real[num37];
				m_imag[num38] = Math.Abs(m_imag[num37]);
				m_imag[num38 + 1] = 0.0 - Math.Abs(m_imag[num37]);
			}
			break;
		}
		}
	}

	public void Design()
	{
		if (!FilterValid)
		{
			return;
		}
		m_aCoeff = new double[m_order + 1];
		m_bCoeff = new double[m_order + 1];
		m_inHistory = new double[32];
		m_outHistory = new double[32];
		double[] array = new double[m_order + 1];
		double[] array2 = new double[m_order + 1];
		LocatePolesAndZeros();
		m_aCoeff[0] = 1.0;
		m_bCoeff[0] = 1.0;
		for (int i = 1; i <= m_order; i++)
		{
			m_aCoeff[i] = 0.0;
			m_bCoeff[i] = 0.0;
		}
		int num = 0;
		int order = m_order;
		int num2 = order / 2;
		if (Odd(m_order))
		{
			m_aCoeff[1] = 0.0 - m_z[1];
			m_bCoeff[1] = 0.0 - m_real[1];
			num = 1;
		}
		for (int j = 1; j <= num2; j++)
		{
			int num3 = 2 * j - 1 + num;
			double num4 = 0.0 - (m_z[num3] + m_z[num3 + 1]);
			double num5 = m_z[num3] * m_z[num3 + 1];
			double num6 = -2.0 * m_real[num3];
			double num7 = Sqr(m_real[num3]) + Sqr(m_imag[num3]);
			array[1] = m_aCoeff[1] + num4 * m_aCoeff[0];
			array2[1] = m_bCoeff[1] + num6 * m_bCoeff[0];
			for (int k = 2; k <= order; k++)
			{
				array[k] = m_aCoeff[k] + num4 * m_aCoeff[k - 1] + num5 * m_aCoeff[k - 2];
				array2[k] = m_bCoeff[k] + num6 * m_bCoeff[k - 1] + num7 * m_bCoeff[k - 2];
			}
			for (int l = 1; l <= order; l++)
			{
				m_aCoeff[l] = array[l];
				m_bCoeff[l] = array2[l];
			}
		}
		FilterGain(1000);
	}

	public void Reset()
	{
		if (m_inHistory != null)
		{
			m_inHistory.Clear();
		}
		if (m_outHistory != null)
		{
			m_outHistory.Clear();
		}
		m_histIdx = 0;
	}

	public void Reset(double startValue)
	{
		m_histIdx = 0;
		if (m_inHistory == null || m_outHistory == null)
		{
			return;
		}
		m_inHistory.Fill(startValue);
		if (m_inHistory != null)
		{
			if (m_filterType == FilterType.LP)
			{
				m_outHistory.Fill(startValue);
			}
			else
			{
				m_outHistory.Clear();
			}
		}
	}

	public void FilterBuffer(float[] srcBuf, long srcPos, float[] dstBuf, long dstPos, long nLen)
	{
		double num = (m_invertDenormal ? (-1E-15) : 1E-15);
		m_invertDenormal = !m_invertDenormal;
		for (int i = 0; i < nLen; i++)
		{
			double num2 = 0.0;
			m_inHistory[m_histIdx] = (double)srcBuf[srcPos + i] + num;
			for (int j = 0; j < m_aCoeff.Length; j++)
			{
				num2 += m_aCoeff[j] * m_inHistory[(m_histIdx - j) & 0x1F];
			}
			for (int k = 1; k < m_bCoeff.Length; k++)
			{
				num2 -= m_bCoeff[k] * m_outHistory[(m_histIdx - k) & 0x1F];
			}
			m_outHistory[m_histIdx] = num2;
			m_histIdx = (m_histIdx + 1) & 0x1F;
			dstBuf[dstPos + i] = (float)num2;
		}
	}

	public float FilterSample(float inVal)
	{
		double num = 0.0;
		m_inHistory[m_histIdx] = inVal;
		for (int i = 0; i < m_aCoeff.Length; i++)
		{
			num += m_aCoeff[i] * m_inHistory[(m_histIdx - i) & 0x1F];
		}
		for (int j = 1; j < m_bCoeff.Length; j++)
		{
			num -= m_bCoeff[j] * m_outHistory[(m_histIdx - j) & 0x1F];
		}
		m_outHistory[m_histIdx] = num;
		m_histIdx = (m_histIdx + 1) & 0x1F;
		return (float)num;
	}

	public float[] FilterGain(int freqPoints)
	{
		float[] array = new float[freqPoints];
		float num = -100f;
		float num2 = 10f / (float)Math.Log(10.0);
		double num3 = Math.PI / (double)(freqPoints - 1);
		for (int i = 0; i < freqPoints; i++)
		{
			double num4 = (double)i * num3;
			if (i == 0)
			{
				num4 = 0.000314159265358979;
			}
			if (i == freqPoints - 1)
			{
				num4 = 3.14127849432443;
			}
			double num5 = 0.0;
			double num6 = 0.0;
			double num7 = 0.0;
			double num8 = 0.0;
			for (int j = 0; j <= m_order; j++)
			{
				double num9 = Math.Cos((double)j * num4);
				double num10 = Math.Sin((double)j * num4);
				num5 += num9 * m_aCoeff[j];
				num6 += num10 * m_aCoeff[j];
				num7 += num9 * m_bCoeff[j];
				num8 += num10 * m_bCoeff[j];
			}
			array[i] = num2 * (float)Math.Log((Sqr(num5) + Sqr(num6)) / (Sqr(num7) + Sqr(num8)));
			num = Math.Max(num, array[i]);
		}
		for (int k = 0; k < freqPoints; k++)
		{
			array[k] -= num;
		}
		float num11 = (float)Math.Pow(10.0, -0.05 * (double)num);
		for (int l = 0; l <= m_order; l++)
		{
			m_aCoeff[l] *= num11;
		}
		return array;
	}
}
