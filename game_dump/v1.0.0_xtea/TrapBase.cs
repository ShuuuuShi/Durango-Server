using UnityEngine;

public abstract class TrapBase : MonoBehaviour
{
	[SerializeField]
	private ParticleType _particleOnConstruct;

	[SerializeField]
	private AudioClipType _soundOnConstruct;

	[SerializeField]
	private ParticleType _particleOnTrapped;

	[SerializeField]
	private AudioClipType _soundOnTrapped;

	[SerializeField]
	private ParticleType _particleOnBreak;

	[SerializeField]
	private AudioClipType _soundOnBreak;

	private void Awake()
	{
		ParticleManager.Cache(_particleOnConstruct);
		ParticleManager.Cache(_particleOnTrapped);
		ParticleManager.Cache(_particleOnBreak);
		SoundManager.Cache(_soundOnConstruct);
		SoundManager.Cache(_soundOnTrapped);
		SoundManager.Cache(_soundOnBreak);
	}

	public virtual float GetAtivateDelay()
	{
		return 10f;
	}

	public virtual void OnConstruct()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		ParticleManager.Emit(_particleOnConstruct, ((Component)this).gameObject.transform.position, Quaternion.identity, null, useLocalPosition: true, comeForwardToCamera: true);
		SoundManager.Play(_soundOnConstruct.Path, ((Component)this).gameObject.transform.position);
	}

	public virtual void OnTrapped()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		ParticleManager.Emit(_particleOnTrapped, ((Component)this).gameObject.transform.position, Quaternion.identity, null, useLocalPosition: true, comeForwardToCamera: true);
		SoundManager.Play(_soundOnTrapped.Path, ((Component)this).gameObject.transform.position);
	}

	public virtual void OnBreak()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		ParticleManager.Emit(_particleOnBreak, ((Component)this).gameObject.transform.position, Quaternion.identity, null, useLocalPosition: true, comeForwardToCamera: true);
		SoundManager.Play(_soundOnBreak.Path, ((Component)this).gameObject.transform.position);
	}
}
