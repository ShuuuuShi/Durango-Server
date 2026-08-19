using UnityEngine;

public class SoundInstance : MonoBehaviour
{
	[SerializeField]
	private AudioClipType _audioClip;

	[SerializeField]
	private Vector3 _offset;

	[SerializeField]
	private bool _loop;

	private int _playSeq;

	private void OnEnable()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		_playSeq = SoundManager.Play(_audioClip.Path, _offset, ((Component)this).transform, _loop);
	}

	private void OnDisable()
	{
		SoundManager.Stop(_playSeq);
	}
}
