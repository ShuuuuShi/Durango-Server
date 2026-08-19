using System.Collections.Generic;
using UnityEngine;

public class USpeakOwnerInfo : MonoBehaviour
{
	public static Dictionary<USpeakOwnerInfo, USpeaker> USpeakerMap = new Dictionary<USpeakOwnerInfo, USpeaker>();

	public static Dictionary<string, USpeakOwnerInfo> USpeakPlayerMap = new Dictionary<string, USpeakOwnerInfo>();

	private USpeaker m_speaker;

	private USpeakPlayer m_Owner;

	public USpeaker Speaker
	{
		get
		{
			if ((Object)(object)m_speaker == (Object)null)
			{
				m_speaker = USpeaker.Get((Object)(object)this);
			}
			return m_speaker;
		}
	}

	public USpeakPlayer Owner => m_Owner;

	public static USpeakOwnerInfo FindPlayerByID(string PlayerID)
	{
		return USpeakPlayerMap[PlayerID];
	}

	public void Init(USpeakPlayer owner)
	{
		m_Owner = owner;
		USpeakPlayerMap.Add(owner.PlayerID, this);
		USpeakerMap.Add(this, ((Component)this).GetComponent<USpeaker>());
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
	}

	public void DeInit()
	{
		USpeakPlayerMap.Remove(m_Owner.PlayerID);
		USpeakerMap.Remove(this);
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}
}
