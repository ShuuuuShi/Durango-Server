using System;
using System.Collections;
using Durango.System;
using Durango.Terrain;
using Durango.Utils;
using L10N;
using UnityEngine;

namespace Durango.UI;

public abstract class LoadingCurtainBase : MonoBehaviour
{
	public enum LoadingState
	{
		Open,
		Closing,
		Closed
	}

	protected static bool IsChunkLoadFailed;

	private UIWidget _widget;

	protected float Duration = 0.5f;

	public Action<LoadingState> StateChanged { get; set; }

	protected LoadingState State { get; private set; }

	public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());

	protected void SetState(LoadingState state)
	{
		State = state;
		if (StateChanged != null)
		{
			StateChanged(state);
		}
	}

	protected IEnumerator WaitForChunkLoading()
	{
		IsChunkLoadFailed = false;
		float beginTime = Time.realtimeSinceStartup;
		// [แก้เอง] 1 ก.ย. 2026 — เดิมเรียก Instance() ตรง ๆ ถ้าซีน Main ยังไม่มี Terrain_Mobile
		// (เข้าโลกช้ากว่าที่ม่านโหลดเริ่มรอ) Singleton จะพยายาม AddComponent TerrainBase ซึ่งเป็น
		// abstract ⇒ "can't be abstract" + NullReferenceException ⇒ **โครูทีนตาย**
		// ⇒ ม่านโหลดไม่เปิด และ timeout 60 วิ ข้างล่างไม่มีวันทำงาน = ค้างหน้าโหลดถาวร
		// เช็ค HasInstance ก่อน เพื่อให้รอเฉย ๆ แล้วปล่อยให้ timeout เด้งกลับหน้าไตเติ้ลพร้อมข้อความ
		while (!Singleton<TerrainBase>.HasInstance() || !Singleton<TerrainBase>.Instance().IsReady)
		{
			yield return null;
			float timeOut = 60f;
			if (Time.realtimeSinceStartup - beginTime > timeOut)
			{
				IsChunkLoadFailed = true;
				string text = ((!TerrainBase.IsPlayerInitialized) ? T._("플레이어 정보를 불러오는데 실패하였습니다.") : T._("지형 정보를 불러오는데 실패하였습니다."));
				string arg = T._("화면을 터치 후 다시 시도해 주세요.");
				string lastEvictedMsg = ((!Platform.Instance.UsePCUI) ? $"{text}\n{arg}" : text);
				GameManager.LastEvictedMsg = lastEvictedMsg;
				Singleton<GameManager>.Instance().MoveToTitle();
				break;
			}
		}
	}

	protected IEnumerator Fadein()
	{
		float remainTime = Duration;
		while (remainTime > 0f)
		{
			remainTime -= Time.deltaTime;
			Widget.alpha = Mathf.Clamp01(1f - remainTime / Duration);
			yield return null;
		}
	}

	protected IEnumerator Fadeout()
	{
		float remainTime = Duration;
		while (remainTime > 0f)
		{
			remainTime -= Time.deltaTime;
			Widget.alpha = Mathf.Clamp01(remainTime / Duration);
			yield return null;
		}
	}
}
