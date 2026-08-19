public class STTController
{
	private enum State
	{
		None,
		JustStart,
		BeginningOfSpeech,
		EndOfSpeech
	}

	public delegate void OnSTTProcessedDelegate(bool succes, string resultText, string confidence);

	public delegate void OnSTTPartialResultDelegate(string resultText);

	public delegate void OnEndOfSpeechDelegate();

	private State _sttState;

	public OnEndOfSpeechDelegate OnEndOfSpeech;

	private string _latestRecognizedText;

	private int _cancelCount;

	public bool STTStarted => _sttState != State.None;

	public event OnSTTProcessedDelegate OnSTTProcessed;

	public event OnSTTPartialResultDelegate OnSTTPartialResult;

	public void InstallEvent()
	{
		SpeechToText.OnError += onError;
		SpeechToText.OnSuccess += onSuccess;
		SpeechToText.OnPartialResult += onPartialResult;
		SpeechToText.OnEndOfSpeech += onEndOfSpeech;
		SpeechToText.OnBeginningOfSpeech += onBeginningOfSpeech;
	}

	public void UninstallEvent()
	{
		SpeechToText.OnError -= onError;
		SpeechToText.OnSuccess -= onSuccess;
		SpeechToText.OnPartialResult -= onPartialResult;
		SpeechToText.OnEndOfSpeech -= onEndOfSpeech;
		SpeechToText.OnBeginningOfSpeech -= onBeginningOfSpeech;
	}

	public void StartSTT_IfCan()
	{
		if (!STTStarted)
		{
			_sttState = State.JustStart;
			_latestRecognizedText = null;
			SpeechToText.CancelSpeech();
			SpeechToText.StartSpeech();
		}
	}

	public void StopSTT()
	{
		if (STTStarted)
		{
			SpeechToText.StopListening();
			if (_sttState == State.JustStart)
			{
				SpeechToText.CancelSpeech();
				_sttState = State.None;
			}
		}
	}

	private void Cleanup()
	{
		SpeechToText.StopListening();
		SpeechToText.CancelSpeech();
		_sttState = State.None;
		_cancelCount = 0;
	}

	private void onError(int errorCode)
	{
		if (STTStarted && _sttState == State.EndOfSpeech)
		{
			Cleanup();
			if (this.OnSTTProcessed != null)
			{
				this.OnSTTProcessed(succes: false, null, null);
			}
		}
		if (errorCode == 5)
		{
			SpeechToText.ResetRecognizer();
			_sttState = State.None;
		}
	}

	private void onSuccess(string resultText, string confidence)
	{
		_latestRecognizedText = resultText;
		Cleanup();
		if (this.OnSTTProcessed != null)
		{
			this.OnSTTProcessed(succes: true, resultText, confidence);
		}
	}

	private void onPartialResult(string resultText)
	{
		if (this.OnSTTPartialResult != null)
		{
			this.OnSTTPartialResult(resultText);
		}
	}

	private void onEndOfSpeech()
	{
		_sttState = State.EndOfSpeech;
	}

	private void onBeginningOfSpeech()
	{
		_sttState = State.BeginningOfSpeech;
	}

	public bool GetLatestRecognizedText(out string result)
	{
		result = _latestRecognizedText;
		return !string.IsNullOrEmpty(_latestRecognizedText);
	}

	public void ClearResult()
	{
		_latestRecognizedText = null;
	}
}
