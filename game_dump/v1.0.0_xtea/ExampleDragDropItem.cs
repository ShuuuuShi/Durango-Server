using UnityEngine;

[AddComponentMenu("NGUI/Examples/Drag and Drop Item (Example)")]
public class ExampleDragDropItem : UIDragDropItem
{
	public GameObject prefab;

	protected override void OnDragDropRelease(GameObject surface)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)surface != (Object)null)
		{
			ExampleDragDropSurface component = surface.GetComponent<ExampleDragDropSurface>();
			if ((Object)(object)component != (Object)null)
			{
				GameObject val = ((Component)component).gameObject.AddChild(prefab);
				val.transform.localScale = ((Component)component).transform.localScale;
				Transform transform = val.transform;
				transform.position = UICamera.lastWorldPosition;
				if (component.rotatePlacedObject)
				{
					transform.rotation = Quaternion.LookRotation(((RaycastHit)(ref UICamera.lastHit)).normal) * Quaternion.Euler(90f, 0f, 0f);
				}
				NGUITools.Destroy((Object)(object)((Component)this).gameObject);
				return;
			}
		}
		base.OnDragDropRelease(surface);
	}
}
