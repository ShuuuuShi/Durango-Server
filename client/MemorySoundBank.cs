using System;
using System.Runtime.InteropServices;

public class MemorySoundBank
{
	private const long AK_BANK_PLATFORM_DATA_ALIGNMENT = 16L;

	private const long AK_BANK_PLATFORM_DATA_ALIGNMENT_MASK = 15L;

	private GCHandle ms_pinnedArray;

	private IntPtr ms_pInMemoryBankPtr = IntPtr.Zero;

	private uint ms_bankID;

	public bool IsValid { get; private set; }

	public MemorySoundBank(byte[] binaryData)
	{
		uint num = 0u;
		try
		{
			ms_pinnedArray = GCHandle.Alloc(binaryData, GCHandleType.Pinned);
			ms_pInMemoryBankPtr = ms_pinnedArray.AddrOfPinnedObject();
			num = (uint)binaryData.Length;
			if ((ms_pInMemoryBankPtr.ToInt64() & 0xF) != 0)
			{
				byte[] array = new byte[(long)binaryData.Length + 16L];
				GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				IntPtr intPtr = gCHandle.AddrOfPinnedObject();
				int destinationIndex = 0;
				if ((intPtr.ToInt64() & 0xF) != 0)
				{
					long num2 = (intPtr.ToInt64() + 15) & -16;
					destinationIndex = (int)(num2 - intPtr.ToInt64());
					intPtr = new IntPtr(num2);
				}
				Array.Copy(binaryData, 0, array, destinationIndex, binaryData.Length);
				ms_pInMemoryBankPtr = intPtr;
				ms_pinnedArray.Free();
				ms_pinnedArray = gCHandle;
			}
		}
		catch
		{
			IsValid = false;
			return;
		}
		AKRESULT aKRESULT = AkSoundEngine.LoadBank(ms_pInMemoryBankPtr, num, out ms_bankID);
		IsValid = aKRESULT == AKRESULT.AK_Success;
	}

	public void Unload()
	{
		if (IsValid)
		{
			AKRESULT aKRESULT = AkSoundEngine.UnloadBank(ms_bankID, ms_pInMemoryBankPtr);
			if (aKRESULT == AKRESULT.AK_Success)
			{
				ms_pinnedArray.Free();
			}
			IsValid = false;
		}
	}
}
