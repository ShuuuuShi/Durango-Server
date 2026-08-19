using System;

namespace Sanford.Multimedia.Midi;

public class KeySignatureBuilder : IMessageBuilder
{
	private Key key = Key.CMajor;

	private MetaMessage result;

	public Key Key
	{
		get
		{
			return key;
		}
		set
		{
			key = value;
		}
	}

	public MetaMessage Result => result;

	public KeySignatureBuilder()
	{
	}

	public KeySignatureBuilder(MetaMessage message)
	{
		Initialize(message);
	}

	public void Initialize(MetaMessage message)
	{
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		if (message.MetaType != MetaType.KeySignature)
		{
			throw new ArgumentException("Wrong meta event type.", "messaege");
		}
		sbyte b = (sbyte)message[0];
		if (message[1] == 0)
		{
			switch (b)
			{
			case -7:
				key = Key.CFlatMajor;
				break;
			case -6:
				key = Key.GFlatMajor;
				break;
			case -5:
				key = Key.DFlatMajor;
				break;
			case -4:
				key = Key.AFlatMajor;
				break;
			case -3:
				key = Key.EFlatMajor;
				break;
			case -2:
				key = Key.BFlatMajor;
				break;
			case -1:
				key = Key.FMajor;
				break;
			case 0:
				key = Key.CMajor;
				break;
			case 1:
				key = Key.GMajor;
				break;
			case 2:
				key = Key.DMajor;
				break;
			case 3:
				key = Key.AMajor;
				break;
			case 4:
				key = Key.EMajor;
				break;
			case 5:
				key = Key.BMajor;
				break;
			case 6:
				key = Key.FSharpMajor;
				break;
			case 7:
				key = Key.CSharpMajor;
				break;
			}
		}
		else
		{
			switch (b)
			{
			case -7:
				key = Key.AFlatMinor;
				break;
			case -6:
				key = Key.EFlatMinor;
				break;
			case -5:
				key = Key.BFlatMinor;
				break;
			case -4:
				key = Key.FMinor;
				break;
			case -3:
				key = Key.CMinor;
				break;
			case -2:
				key = Key.GMinor;
				break;
			case -1:
				key = Key.DMinor;
				break;
			case 0:
				key = Key.AMinor;
				break;
			case 1:
				key = Key.EMinor;
				break;
			case 2:
				key = Key.BMinor;
				break;
			case 3:
				key = Key.FSharpMinor;
				break;
			case 4:
				key = Key.CSharpMinor;
				break;
			case 5:
				key = Key.GSharpMinor;
				break;
			case 6:
				key = Key.DSharpMinor;
				break;
			case 7:
				key = Key.ASharpMinor;
				break;
			}
		}
	}

	public void Build()
	{
		byte[] array = new byte[2];
		switch (Key)
		{
		case Key.CFlatMajor:
			array[0] = 249;
			array[1] = 0;
			break;
		case Key.GFlatMajor:
			array[0] = 250;
			array[1] = 0;
			break;
		case Key.DFlatMajor:
			array[0] = 251;
			array[1] = 0;
			break;
		case Key.AFlatMajor:
			array[0] = 252;
			array[1] = 0;
			break;
		case Key.EFlatMajor:
			array[0] = 253;
			array[1] = 0;
			break;
		case Key.BFlatMajor:
			array[0] = 254;
			array[1] = 0;
			break;
		case Key.FMajor:
			array[0] = byte.MaxValue;
			array[1] = 0;
			break;
		case Key.CMajor:
			array[0] = 0;
			array[1] = 0;
			break;
		case Key.GMajor:
			array[0] = 1;
			array[1] = 0;
			break;
		case Key.DMajor:
			array[0] = 2;
			array[1] = 0;
			break;
		case Key.AMajor:
			array[0] = 3;
			array[1] = 0;
			break;
		case Key.EMajor:
			array[0] = 4;
			array[1] = 0;
			break;
		case Key.BMajor:
			array[0] = 5;
			array[1] = 0;
			break;
		case Key.FSharpMajor:
			array[0] = 6;
			array[1] = 0;
			break;
		case Key.CSharpMajor:
			array[0] = 7;
			array[1] = 0;
			break;
		case Key.AFlatMinor:
			array[0] = 249;
			array[1] = 1;
			break;
		case Key.EFlatMinor:
			array[0] = 250;
			array[1] = 1;
			break;
		case Key.BFlatMinor:
			array[0] = 251;
			array[1] = 1;
			break;
		case Key.FMinor:
			array[0] = 252;
			array[1] = 1;
			break;
		case Key.CMinor:
			array[0] = 253;
			array[1] = 1;
			break;
		case Key.GMinor:
			array[0] = 254;
			array[1] = 1;
			break;
		case Key.DMinor:
			array[0] = byte.MaxValue;
			array[1] = 1;
			break;
		case Key.AMinor:
			array[0] = 1;
			array[1] = 0;
			break;
		case Key.EMinor:
			array[0] = 1;
			array[1] = 1;
			break;
		case Key.BMinor:
			array[0] = 2;
			array[1] = 1;
			break;
		case Key.FSharpMinor:
			array[0] = 3;
			array[1] = 1;
			break;
		case Key.CSharpMinor:
			array[0] = 4;
			array[1] = 1;
			break;
		case Key.GSharpMinor:
			array[0] = 5;
			array[1] = 1;
			break;
		case Key.DSharpMinor:
			array[0] = 6;
			array[1] = 1;
			break;
		case Key.ASharpMinor:
			array[0] = 7;
			array[1] = 1;
			break;
		}
		result = new MetaMessage(MetaType.KeySignature, array);
	}
}
