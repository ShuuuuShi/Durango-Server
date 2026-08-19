using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/tk2dUIDragItem")]
public class tk2dUIDragItem : tk2dUIBaseItemControl
{
	public tk2dUIManager uiManager;

	private Vector3 offset = Vector3.zero;

	private bool isBtnActive;

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)uiItem))
		{
			uiItem.OnDown += ButtonDown;
			uiItem.OnRelease += ButtonRelease;
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)uiItem))
		{
			uiItem.OnDown -= ButtonDown;
			uiItem.OnRelease -= ButtonRelease;
		}
		if (isBtnActive)
		{
			if ((Object)(object)tk2dUIManager.Instance__NoCreate != (Object)null)
			{
				tk2dUIManager.Instance.OnInputUpdate -= UpdateBtnPosition;
			}
			isBtnActive = false;
		}
	}

	private void UpdateBtnPosition()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = CalculateNewPos();
	}

	private Vector3 CalculateNewPos()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		Vector2 position = uiItem.Touch.position;
		Camera uICameraForControl = tk2dUIManager.Instance.GetUICameraForControl(((Component)this).gameObject);
		Vector3 val = uICameraForControl.ScreenToWorldPoint(new Vector3(position.x, position.y, ((Component)this).transform.position.z - ((Component)uICameraForControl).transform.position.z));
		val.z = ((Component)this).transform.position.z;
		return val + offset;
	}

	public void ButtonDown()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (!isBtnActive)
		{
			tk2dUIManager.Instance.OnInputUpdate += UpdateBtnPosition;
		}
		isBtnActive = true;
		offset = Vector3.zero;
		Vector3 val = CalculateNewPos();
		offset = ((Component)this).transform.position - val;
	}

	public void ButtonRelease()
	{
		if (isBtnActive)
		{
			tk2dUIManager.Instance.OnInputUpdate -= UpdateBtnPosition;
		}
		isBtnActive = false;
	}
}
