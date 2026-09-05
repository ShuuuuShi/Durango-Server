using System;
using Durango.Network;
using UnityEngine;

namespace Durango.UI;

public class NetworkStatusWidget : MonoBehaviour
{
	[SerializeField]
	private GameObject _networkContainer;

	[SerializeField]
	private UISprite _pingSprite;

	[SerializeField]
	private UISprite _pingUpper;

	[SerializeField]
	private UISprite[] _wifiSprites;

	[SerializeField]
	private UILabel _networkTypeLabel;

	[SerializeField]
	private GameObject _batteryContainer;

	[SerializeField]
	private UISprite _batteryBorder;

	[SerializeField]
	private UISprite _batteryUpper;

	[SerializeField]
	private UISprite _chargeSprite;

	[SerializeField]
	private GameObject _timeContainer;

	[SerializeField]
	private UILabel _timeLabel;

	private float _nextRefreshAt;

	private NetworkReachability _networkReachability;

	private void Update()
	{
		float time = Time.time;
		if (!(time < _nextRefreshAt))
		{
			_nextRefreshAt = time + 2f;
			// [3 ก.ย. 2026] อัปเดตจำนวนคนออนไลน์ระหว่างอยู่ในเกมด้วย (เดิมรีเฟรชแค่หน้าไตเติ้ล)
			//   RefreshServerStatus มีตัวกันยิงถี่ในตัว (10 วิ/ครั้ง) ยิง /knock อ่าน online_players
			Durango.Offline.Server.RefreshServerStatus();
			if (_networkContainer.activeSelf)
			{
				RefreshPing();
			}
			if (_batteryContainer.activeSelf)
			{
				RefreshBattery();
			}
			if (_timeContainer.activeSelf)
			{
				RefreshTime();
			}
		}
	}

	private void RefreshPing()
	{
		float ping = Connections.Frontend.Ping;
		int num;
		Color color;
		if (ping < 0.2f)
		{
			num = 3;
			color = new Color32(37, 124, 37, byte.MaxValue);
		}
		else if (ping < 0.5f)
		{
			num = 2;
			color = new Color32(159, 103, 36, byte.MaxValue);
		}
		else
		{
			num = 1;
			color = new Color32(159, 36, 36, byte.MaxValue);
		}
		NetworkReachability networkReachability = _networkReachability;
		_networkReachability = Application.internetReachability;
		if (_networkReachability == networkReachability)
		{
			return;
		}
		switch (_networkReachability)
		{
		case NetworkReachability.ReachableViaCarrierDataNetwork:
			_networkTypeLabel.text = "3G[c][555555]/[-][/c]LTE";
			_pingUpper.fillAmount = (float)num / 3f;
			_pingUpper.color = color;
			_pingSprite.alpha = 1f;
			_wifiSprites[0].alpha = 0f;
			break;
		case NetworkReachability.ReachableViaLocalAreaNetwork:
		{
			_networkTypeLabel.text = "WIFI";
			for (int i = 0; i < _wifiSprites.Length; i++)
			{
				if (i < num)
				{
					_wifiSprites[i].color = color;
				}
				else
				{
					_wifiSprites[i].alpha = 0f;
				}
				_pingSprite.alpha = 0f;
			}
			break;
		}
		}
	}

	private void RefreshTime()
	{
		// เวลา + จำนวนคนออนไลน์ (ไอคอนคน + เลข) บนแถบสถานะบนสุด — ใช้ label เดิม ไม่แตะ prefab
		string people = (Durango.Offline.Server.OnlinePlayers >= 0)
			? ("  [icon=icon_person:0.8] " + Durango.Offline.Server.OnlinePlayers)
			: string.Empty;
		_timeLabel.text = DateTime.Now.ToString("HH:mm") + people;
	}

	private void RefreshBattery()
	{
		float batteryLevel = SystemInfo.batteryLevel;
		BatteryStatus batteryStatus = SystemInfo.batteryStatus;
		_batteryUpper.fillAmount = batteryLevel;
		Color color = ((!(batteryLevel > 0.2f)) ? new Color32(159, 36, 36, byte.MaxValue) : new Color32(153, 153, 153, byte.MaxValue));
		_batteryBorder.color = color;
		_batteryUpper.color = color;
		if (batteryStatus == BatteryStatus.Charging)
		{
			_chargeSprite.color = color;
		}
		else
		{
			_chargeSprite.alpha = 0f;
		}
	}
}
