using System;

namespace Sanford.Multimedia.Midi;

public class TempoChangeBuilder : IMessageBuilder
{
	private const int Shift = 8;

	private int tempo = 500000;

	private MetaMessage result;

	private bool changed = true;

	public int Tempo
	{
		get
		{
			return tempo;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("Tempo", value, "Tempo is out of range.");
			}
			tempo = value;
			changed = true;
		}
	}

	public MetaMessage Result => result;

	public TempoChangeBuilder()
	{
	}

	public TempoChangeBuilder(MetaMessage e)
	{
		Initialize(e);
	}

	public void Initialize(MetaMessage e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (e.MetaType != MetaType.Tempo)
		{
			throw new ArgumentException("Wrong meta message type.", "e");
		}
		int num = 0;
		if (BitConverter.IsLittleEndian)
		{
			int num2 = e.Length - 1;
			for (int i = 0; i < e.Length; i++)
			{
				num |= e[num2] << 8 * i;
				num2--;
			}
		}
		else
		{
			for (int j = 0; j < e.Length; j++)
			{
				num |= e[j] << 8 * j;
			}
		}
		tempo = num;
	}

	public void Build()
	{
		if (!changed)
		{
			return;
		}
		byte[] array = new byte[3];
		if (BitConverter.IsLittleEndian)
		{
			int num = array.Length - 1;
			for (int i = 0; i < array.Length; i++)
			{
				array[num] = (byte)(tempo >> 8 * i);
				num--;
			}
		}
		else
		{
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = (byte)(tempo >> 8 * j);
			}
		}
		changed = false;
		result = new MetaMessage(MetaType.Tempo, array);
	}
}
