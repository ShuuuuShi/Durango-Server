using UnityEngine;

public class MediaPlayer2UITexture : MonoBehaviour
{
	[SerializeField]
	private MediaPlayerCtrl _mediaPlayer;

	[SerializeField]
	private UITexture _texture;

	private void Start()
	{
		_mediaPlayer.OnVideoTextureUpdated = MediaPlayer_VideoTextureUpdated;
		if (Application.isEditor || Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.WindowsPlayer)
		{
			_texture.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
		}
	}

	private void OnEnable()
	{
		_texture.mainTexture = null;
	}

	private void MediaPlayer_VideoTextureUpdated(Texture videoTexture)
	{
		_texture.mainTexture = videoTexture;
	}
}
