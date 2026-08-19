using System.Collections;
using UnityEngine;

[AddComponentMenu("2D Toolkit/Deprecated/GUI/tk2dButton")]
public class tk2dButton : MonoBehaviour
{
	public delegate void ButtonHandlerDelegate(tk2dButton source);

	public Camera viewCamera;

	public string buttonDownSprite = "button_down";

	public string buttonUpSprite = "button_up";

	public string buttonPressedSprite = "button_up";

	private int buttonDownSpriteId = -1;

	private int buttonUpSpriteId = -1;

	private int buttonPressedSpriteId = -1;

	public AudioClip buttonDownSound;

	public AudioClip buttonUpSound;

	public AudioClip buttonPressedSound;

	public GameObject targetObject;

	public string messageName = string.Empty;

	private tk2dBaseSprite sprite;

	private bool buttonDown;

	public float targetScale = 1.1f;

	public float scaleTime = 0.05f;

	public float pressedWaitTime = 0.3f;

	public event ButtonHandlerDelegate ButtonPressedEvent;

	public event ButtonHandlerDelegate ButtonAutoFireEvent;

	public event ButtonHandlerDelegate ButtonDownEvent;

	public event ButtonHandlerDelegate ButtonUpEvent;

	private void OnEnable()
	{
		buttonDown = false;
	}

	private void Start()
	{
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)viewCamera == (Object)null)
		{
			Transform val = ((Component)this).transform;
			while (Object.op_Implicit((Object)(object)val) && (Object)(object)((Component)val).GetComponent<Camera>() == (Object)null)
			{
				val = val.parent;
			}
			if (Object.op_Implicit((Object)(object)val) && (Object)(object)((Component)val).GetComponent<Camera>() != (Object)null)
			{
				viewCamera = ((Component)val).GetComponent<Camera>();
			}
			if ((Object)(object)viewCamera == (Object)null && Object.op_Implicit((Object)(object)tk2dCamera.Instance))
			{
				viewCamera = ((Component)tk2dCamera.Instance).GetComponent<Camera>();
			}
			if ((Object)(object)viewCamera == (Object)null)
			{
				viewCamera = Camera.main;
			}
		}
		sprite = ((Component)this).GetComponent<tk2dBaseSprite>();
		if (Object.op_Implicit((Object)(object)sprite))
		{
			UpdateSpriteIds();
		}
		if ((Object)(object)((Component)this).GetComponent<Collider>() == (Object)null)
		{
			BoxCollider val2 = ((Component)this).gameObject.AddComponent<BoxCollider>();
			Vector3 size = val2.size;
			size.z = 0.2f;
			val2.size = size;
		}
		if (((Object)(object)buttonDownSound != (Object)null || (Object)(object)buttonPressedSound != (Object)null || (Object)(object)buttonUpSound != (Object)null) && (Object)(object)((Component)this).GetComponent<AudioSource>() == (Object)null)
		{
			AudioSource val3 = ((Component)this).gameObject.AddComponent<AudioSource>();
			val3.playOnAwake = false;
		}
	}

	public void UpdateSpriteIds()
	{
		buttonDownSpriteId = ((buttonDownSprite.Length <= 0) ? (-1) : sprite.GetSpriteIdByName(buttonDownSprite));
		buttonUpSpriteId = ((buttonUpSprite.Length <= 0) ? (-1) : sprite.GetSpriteIdByName(buttonUpSprite));
		buttonPressedSpriteId = ((buttonPressedSprite.Length <= 0) ? (-1) : sprite.GetSpriteIdByName(buttonPressedSprite));
	}

	private void PlaySound(AudioClip source)
	{
		if (Object.op_Implicit((Object)(object)((Component)this).GetComponent<AudioSource>()) && Object.op_Implicit((Object)(object)source))
		{
			((Component)this).GetComponent<AudioSource>().PlayOneShot(source);
		}
	}

	private IEnumerator coScale(Vector3 defaultScale, float startScale, float endScale)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		float t2 = Time.realtimeSinceStartup;
		Vector3 scale2 = defaultScale;
		for (float s = 0f; s < scaleTime; s = Time.realtimeSinceStartup - t2)
		{
			float t = Mathf.Clamp01(s / scaleTime);
			float scl = Mathf.Lerp(startScale, endScale, t);
			scale2 = defaultScale * scl;
			((Component)this).transform.localScale = scale2;
			yield return 0;
		}
		((Component)this).transform.localScale = defaultScale * endScale;
	}

	private IEnumerator LocalWaitForSeconds(float seconds)
	{
		float t0 = Time.realtimeSinceStartup;
		for (float s = 0f; s < seconds; s = Time.realtimeSinceStartup - t0)
		{
			yield return 0;
		}
	}

	private IEnumerator coHandleButtonPress(int fingerId)
	{
		buttonDown = true;
		bool buttonPressed = true;
		Vector3 defaultScale = ((Component)this).transform.localScale;
		if (targetScale != 1f)
		{
			yield return ((MonoBehaviour)this).StartCoroutine(coScale(defaultScale, 1f, targetScale));
		}
		PlaySound(buttonDownSound);
		if (buttonDownSpriteId != -1)
		{
			sprite.spriteId = buttonDownSpriteId;
		}
		if (this.ButtonDownEvent != null)
		{
			this.ButtonDownEvent(this);
		}
		RaycastHit hitInfo = default(RaycastHit);
		while (true)
		{
			Vector3 cursorPosition = Vector3.zero;
			bool cursorActive = true;
			if (fingerId != -1)
			{
				bool found = false;
				for (int i = 0; i < Input.touchCount; i++)
				{
					Touch touch = Input.GetTouch(i);
					if (((Touch)(ref touch)).fingerId == fingerId)
					{
						if ((int)((Touch)(ref touch)).phase == 3 || (int)((Touch)(ref touch)).phase == 4)
						{
							break;
						}
						cursorPosition = Vector2.op_Implicit(((Touch)(ref touch)).position);
						found = true;
					}
				}
				if (!found)
				{
					cursorActive = false;
				}
			}
			else
			{
				if (!Input.GetMouseButton(0))
				{
					cursorActive = false;
				}
				cursorPosition = Input.mousePosition;
			}
			if (!cursorActive)
			{
				break;
			}
			Ray ray = viewCamera.ScreenPointToRay(cursorPosition);
			bool colliderHit = ((Component)this).GetComponent<Collider>().Raycast(ray, ref hitInfo, float.PositiveInfinity);
			if (buttonPressed && !colliderHit)
			{
				if (targetScale != 1f)
				{
					yield return ((MonoBehaviour)this).StartCoroutine(coScale(defaultScale, targetScale, 1f));
				}
				PlaySound(buttonUpSound);
				if (buttonUpSpriteId != -1)
				{
					sprite.spriteId = buttonUpSpriteId;
				}
				if (this.ButtonUpEvent != null)
				{
					this.ButtonUpEvent(this);
				}
				buttonPressed = false;
			}
			else if (!buttonPressed && colliderHit)
			{
				if (targetScale != 1f)
				{
					yield return ((MonoBehaviour)this).StartCoroutine(coScale(defaultScale, 1f, targetScale));
				}
				PlaySound(buttonDownSound);
				if (buttonDownSpriteId != -1)
				{
					sprite.spriteId = buttonDownSpriteId;
				}
				if (this.ButtonDownEvent != null)
				{
					this.ButtonDownEvent(this);
				}
				buttonPressed = true;
			}
			if (buttonPressed && this.ButtonAutoFireEvent != null)
			{
				this.ButtonAutoFireEvent(this);
			}
			yield return 0;
		}
		if (buttonPressed)
		{
			if (targetScale != 1f)
			{
				yield return ((MonoBehaviour)this).StartCoroutine(coScale(defaultScale, targetScale, 1f));
			}
			PlaySound(buttonPressedSound);
			if (buttonPressedSpriteId != -1)
			{
				sprite.spriteId = buttonPressedSpriteId;
			}
			if (Object.op_Implicit((Object)(object)targetObject))
			{
				targetObject.SendMessage(messageName);
			}
			if (this.ButtonUpEvent != null)
			{
				this.ButtonUpEvent(this);
			}
			if (this.ButtonPressedEvent != null)
			{
				this.ButtonPressedEvent(this);
			}
			if (((Component)this).gameObject.activeInHierarchy)
			{
				yield return ((MonoBehaviour)this).StartCoroutine(LocalWaitForSeconds(pressedWaitTime));
			}
			if (buttonUpSpriteId != -1)
			{
				sprite.spriteId = buttonUpSpriteId;
			}
		}
		buttonDown = false;
	}

	private void Update()
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (buttonDown)
		{
			return;
		}
		bool flag = false;
		if (Input.multiTouchEnabled)
		{
			RaycastHit val2 = default(RaycastHit);
			for (int i = 0; i < Input.touchCount; i++)
			{
				Touch touch = Input.GetTouch(i);
				if ((int)((Touch)(ref touch)).phase == 0)
				{
					Ray val = viewCamera.ScreenPointToRay(Vector2.op_Implicit(((Touch)(ref touch)).position));
					if (((Component)this).GetComponent<Collider>().Raycast(val, ref val2, 100000000f) && !Physics.Raycast(val, ((RaycastHit)(ref val2)).distance - 0.01f))
					{
						((MonoBehaviour)this).StartCoroutine(coHandleButtonPress(((Touch)(ref touch)).fingerId));
						flag = true;
						break;
					}
				}
			}
		}
		if (!flag && Input.GetMouseButtonDown(0))
		{
			Ray val3 = viewCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit val4 = default(RaycastHit);
			if (((Component)this).GetComponent<Collider>().Raycast(val3, ref val4, 100000000f) && !Physics.Raycast(val3, ((RaycastHit)(ref val4)).distance - 0.01f))
			{
				((MonoBehaviour)this).StartCoroutine(coHandleButtonPress(-1));
			}
		}
	}
}
