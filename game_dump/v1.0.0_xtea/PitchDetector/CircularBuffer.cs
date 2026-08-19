using System;

namespace PitchDetector;

internal class CircularBuffer<T> : IDisposable
{
	private int m_bufSize;

	private int m_begBufOffset;

	private int m_availBuf;

	private long m_startPos;

	private T[] m_buffer;

	public long StartPosition
	{
		get
		{
			return m_startPos;
		}
		set
		{
			m_startPos = value;
		}
	}

	public long EndPosition => m_startPos + m_availBuf;

	public int Available
	{
		get
		{
			return m_availBuf;
		}
		set
		{
			m_availBuf = Math.Min(value, m_bufSize);
		}
	}

	public CircularBuffer()
	{
	}

	public CircularBuffer(int bufCount)
	{
		SetSize(bufCount);
	}

	public void Dispose()
	{
		SetSize(0);
	}

	public void Reset()
	{
		m_begBufOffset = 0;
		m_availBuf = 0;
		m_startPos = 0L;
	}

	public void SetSize(int newSize)
	{
		Reset();
		if (m_bufSize != newSize)
		{
			if (m_buffer != null)
			{
				m_buffer = null;
			}
			m_bufSize = newSize;
			if (m_bufSize > 0)
			{
				m_buffer = new T[m_bufSize];
			}
		}
	}

	public void Clear()
	{
		Array.Clear(m_buffer, 0, m_buffer.Length);
	}

	public int WriteBuffer(T[] m_pInBuffer, int count)
	{
		count = Math.Min(count, m_bufSize);
		int num = ((m_availBuf != m_bufSize) ? m_availBuf : m_begBufOffset);
		int num2 = Math.Min(count, m_bufSize - num);
		int num3 = count - num2;
		PitchDsp.CopyBuffer(m_pInBuffer, 0, m_buffer, num, num2);
		if (num3 > 0)
		{
			PitchDsp.CopyBuffer(m_pInBuffer, num2, m_buffer, 0, num3);
		}
		if (num3 == 0)
		{
			if (m_availBuf != m_bufSize)
			{
				m_availBuf += count;
			}
			else
			{
				m_begBufOffset += count;
				m_startPos += count;
			}
		}
		else
		{
			if (m_availBuf != m_bufSize)
			{
				m_startPos += num3;
			}
			else
			{
				m_startPos += count;
			}
			m_begBufOffset = num3;
			m_availBuf = m_bufSize;
		}
		return count;
	}

	public bool ReadBuffer(T[] outBuffer, long startRead, int readCount)
	{
		int num = (int)(startRead + readCount);
		int num2 = (int)(m_startPos + m_availBuf);
		if (startRead < m_startPos || num > num2)
		{
			return false;
		}
		int num3 = (int)((startRead - m_startPos + m_begBufOffset) % m_bufSize);
		int num4 = Math.Min(readCount, m_bufSize - num3);
		int num5 = readCount - num4;
		PitchDsp.CopyBuffer(m_buffer, num3, outBuffer, 0, num4);
		if (num5 > 0)
		{
			PitchDsp.CopyBuffer(m_buffer, 0, outBuffer, num4, num5);
		}
		return true;
	}
}
