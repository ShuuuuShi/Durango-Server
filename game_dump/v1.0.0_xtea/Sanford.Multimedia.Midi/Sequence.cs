using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Sanford.Multimedia.Midi;

public sealed class Sequence : IDisposable, IEnumerable, IComponent, ICollection<Track>, IEnumerable<Track>
{
	private List<Track> tracks = new List<Track>();

	private MidiFileProperties properties = new MidiFileProperties();

	private BackgroundWorker loadWorker = new BackgroundWorker();

	private BackgroundWorker saveWorker = new BackgroundWorker();

	private ISite site;

	private bool disposed;

	public Track this[int index]
	{
		get
		{
			if (disposed)
			{
				throw new ObjectDisposedException("Sequence");
			}
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException("index", index, "Sequence index out of range.");
			}
			return tracks[index];
		}
	}

	public int Division
	{
		get
		{
			if (disposed)
			{
				throw new ObjectDisposedException("Sequence");
			}
			return properties.Division;
		}
	}

	public int Format
	{
		get
		{
			if (disposed)
			{
				throw new ObjectDisposedException("Sequence");
			}
			return properties.Format;
		}
		set
		{
			if (disposed)
			{
				throw new ObjectDisposedException("Sequence");
			}
			if (IsBusy)
			{
				throw new InvalidOperationException();
			}
			properties.Format = value;
		}
	}

	public SequenceType SequenceType
	{
		get
		{
			if (disposed)
			{
				throw new ObjectDisposedException("Sequence");
			}
			return properties.SequenceType;
		}
	}

	public bool IsBusy => loadWorker.IsBusy || saveWorker.IsBusy;

	public int Count
	{
		get
		{
			if (disposed)
			{
				throw new ObjectDisposedException("Sequence");
			}
			return tracks.Count;
		}
	}

	public bool IsReadOnly
	{
		get
		{
			if (disposed)
			{
				throw new ObjectDisposedException("Sequence");
			}
			return false;
		}
	}

	public ISite Site
	{
		get
		{
			return site;
		}
		set
		{
			site = value;
		}
	}

	public event EventHandler<AsyncCompletedEventArgs> LoadCompleted;

	public event ProgressChangedEventHandler LoadProgressChanged;

	public event EventHandler<AsyncCompletedEventArgs> SaveCompleted;

	public event ProgressChangedEventHandler SaveProgressChanged;

	public event EventHandler Disposed;

	public Sequence()
	{
		InitializeBackgroundWorkers();
	}

	public Sequence(int division)
	{
		properties.Division = division;
		properties.Format = 1;
		InitializeBackgroundWorkers();
	}

	public Sequence(string fileName)
	{
		InitializeBackgroundWorkers();
		Load(fileName);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (disposed)
		{
			throw new ObjectDisposedException("Sequence");
		}
		return tracks.GetEnumerator();
	}

	private void InitializeBackgroundWorkers()
	{
		loadWorker.DoWork += LoadDoWork;
		loadWorker.ProgressChanged += OnLoadProgressChanged;
		loadWorker.RunWorkerCompleted += OnLoadCompleted;
		loadWorker.WorkerReportsProgress = true;
		saveWorker.DoWork += SaveDoWork;
		saveWorker.ProgressChanged += OnSaveProgressChanged;
		saveWorker.RunWorkerCompleted += OnSaveCompleted;
		saveWorker.WorkerReportsProgress = true;
	}

	public void Load(string fileName)
	{
		if (disposed)
		{
			throw new ObjectDisposedException("Sequence");
		}
		if (IsBusy)
		{
			throw new InvalidOperationException();
		}
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
		Load(stream);
	}

	public void Load(Stream stream)
	{
		using (stream)
		{
			MidiFileProperties midiFileProperties = new MidiFileProperties();
			TrackReader trackReader = new TrackReader();
			List<Track> list = new List<Track>();
			midiFileProperties.Read(stream);
			for (int i = 0; i < midiFileProperties.TrackCount; i++)
			{
				trackReader.Read(stream);
				list.Add(trackReader.Track);
			}
			properties = midiFileProperties;
			tracks = list;
		}
	}

	public void LoadAsync(Stream stream)
	{
		if (disposed)
		{
			throw new ObjectDisposedException("Sequence");
		}
		if (IsBusy)
		{
			throw new InvalidOperationException();
		}
		loadWorker.RunWorkerAsync(stream);
	}

	public void LoadAsyncCancel()
	{
		if (disposed)
		{
			throw new ObjectDisposedException("Sequence");
		}
		loadWorker.CancelAsync();
	}

	public void Save(string fileName)
	{
		if (disposed)
		{
			throw new ObjectDisposedException("Sequence");
		}
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
		Save(stream);
	}

	public void Save(Stream stream)
	{
		using (stream)
		{
			properties.Write(stream);
			TrackWriter trackWriter = new TrackWriter();
			foreach (Track track in tracks)
			{
				trackWriter.Track = track;
				trackWriter.Write(stream);
			}
		}
	}

	public void SaveAsync(Stream stream)
	{
		if (disposed)
		{
			throw new ObjectDisposedException("Sequence");
		}
		if (IsBusy)
		{
			throw new InvalidOperationException();
		}
		saveWorker.RunWorkerAsync(stream);
	}

	public void SaveAsyncCancel()
	{
		if (disposed)
		{
			throw new ObjectDisposedException("Sequence");
		}
		saveWorker.CancelAsync();
	}

	public int GetLength()
	{
		if (disposed)
		{
			throw new ObjectDisposedException("Sequence");
		}
		int num = 0;
		using IEnumerator<Track> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Track current = enumerator.Current;
			if (current.Length > num)
			{
				num = current.Length;
			}
		}
		return num;
	}

	private void OnLoadCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		this.LoadCompleted?.Invoke(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, null));
	}

	private void OnLoadProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		this.LoadProgressChanged?.Invoke(this, e);
	}

	private void LoadDoWork(object sender, DoWorkEventArgs e)
	{
		Stream stream = (Stream)e.Argument;
		using (stream)
		{
			MidiFileProperties midiFileProperties = new MidiFileProperties();
			TrackReader trackReader = new TrackReader();
			List<Track> list = new List<Track>();
			midiFileProperties.Read(stream);
			for (int i = 0; i < midiFileProperties.TrackCount; i++)
			{
				if (loadWorker.CancellationPending)
				{
					break;
				}
				trackReader.Read(stream);
				list.Add(trackReader.Track);
				float num = ((float)i + 1f) / (float)midiFileProperties.TrackCount;
				loadWorker.ReportProgress((int)(100f * num));
			}
			if (loadWorker.CancellationPending)
			{
				e.Cancel = true;
				return;
			}
			properties = midiFileProperties;
			tracks = list;
		}
	}

	private void OnSaveCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		this.SaveCompleted?.Invoke(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, null));
	}

	private void OnSaveProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		this.SaveProgressChanged?.Invoke(this, e);
	}

	private void SaveDoWork(object sender, DoWorkEventArgs e)
	{
		Stream stream = (Stream)e.Argument;
		using (stream)
		{
			properties.Write(stream);
			TrackWriter trackWriter = new TrackWriter();
			for (int i = 0; i < tracks.Count; i++)
			{
				if (saveWorker.CancellationPending)
				{
					break;
				}
				trackWriter.Track = tracks[i];
				trackWriter.Write(stream);
				float num = ((float)i + 1f) / (float)properties.TrackCount;
				saveWorker.ReportProgress((int)(100f * num));
			}
			if (saveWorker.CancellationPending)
			{
				e.Cancel = true;
			}
		}
	}

	public void Add(Track item)
	{
		if (disposed)
		{
			throw new ObjectDisposedException("Sequence");
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		tracks.Add(item);
		properties.TrackCount = tracks.Count;
	}

	public void Clear()
	{
		if (disposed)
		{
			throw new ObjectDisposedException("Sequence");
		}
		tracks.Clear();
		properties.TrackCount = tracks.Count;
	}

	public bool Contains(Track item)
	{
		if (disposed)
		{
			throw new ObjectDisposedException("Sequence");
		}
		return tracks.Contains(item);
	}

	public void CopyTo(Track[] array, int arrayIndex)
	{
		if (disposed)
		{
			throw new ObjectDisposedException("Sequence");
		}
		tracks.CopyTo(array, arrayIndex);
	}

	public bool Remove(Track item)
	{
		if (disposed)
		{
			throw new ObjectDisposedException("Sequence");
		}
		bool flag = tracks.Remove(item);
		if (flag)
		{
			properties.TrackCount = tracks.Count;
		}
		return flag;
	}

	public IEnumerator<Track> GetEnumerator()
	{
		if (disposed)
		{
			throw new ObjectDisposedException("Sequence");
		}
		return tracks.GetEnumerator();
	}

	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			this.Disposed?.Invoke(this, EventArgs.Empty);
		}
	}
}
