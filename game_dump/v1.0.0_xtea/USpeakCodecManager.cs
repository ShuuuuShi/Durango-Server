using System;
using MoPhoGames.USpeak.Codec;
using UnityEngine;

public class USpeakCodecManager : ScriptableObject
{
	private static USpeakCodecManager instance;

	public ICodec[] Codecs;

	public string[] CodecNames = new string[0];

	public string[] FriendlyNames = new string[0];

	public static USpeakCodecManager Instance
	{
		get
		{
			if ((Object)(object)instance == (Object)null)
			{
				instance = (USpeakCodecManager)(object)Resources.Load("CodecManager");
				if (Application.isPlaying)
				{
					instance.Codecs = new ICodec[instance.CodecNames.Length];
					for (int i = 0; i < instance.Codecs.Length; i++)
					{
						instance.Codecs[i] = (ICodec)Activator.CreateInstance(Type.GetType(instance.CodecNames[i]));
					}
				}
			}
			return instance;
		}
	}
}
