using System;
using System.Collections.Generic;

namespace Sanford.Multimedia.Midi;

public sealed class Track
{
	private int count = 1;

	private int endOfTrackOffset;

	private MidiEvent head;

	private MidiEvent tail;

	private MidiEvent endOfTrackMidiEvent;

	public int Count => count;

	public int Length
	{
		get
		{
			int num = EndOfTrackOffset;
			if (tail != null)
			{
				num += tail.AbsoluteTicks;
			}
			return num + 1;
		}
	}

	public int EndOfTrackOffset
	{
		get
		{
			return endOfTrackOffset;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("EndOfTrackOffset", value, "End of track offset out of range.");
			}
			endOfTrackOffset = value;
			endOfTrackMidiEvent.SetAbsoluteTicks(Length);
		}
	}

	public object SyncRoot => this;

	public Track()
	{
		endOfTrackMidiEvent = new MidiEvent(this, Length, MetaMessage.EndOfTrackMessage);
	}

	public IEnumerable<MidiEvent> Iterator()
	{
		for (MidiEvent current = head; current != null; current = current.Next)
		{
			yield return current;
		}
		yield return endOfTrackMidiEvent;
	}

	public IEnumerable<int> DispatcherIterator(MessageDispatcher dispatcher)
	{
		IEnumerator<MidiEvent> enumerator = Iterator().GetEnumerator();
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current.AbsoluteTicks;
			dispatcher.Dispatch(enumerator.Current.MidiMessage);
		}
	}

	public IEnumerable<int> TickIterator(int startPosition, ChannelChaser chaser, MessageDispatcher dispatcher)
	{
		if (startPosition < 0)
		{
			throw new ArgumentOutOfRangeException("startPosition", startPosition, "Start position out of range.");
		}
		IEnumerator<MidiEvent> enumerator = Iterator().GetEnumerator();
		bool notFinished;
		for (notFinished = enumerator.MoveNext(); notFinished && enumerator.Current.AbsoluteTicks < startPosition; notFinished = enumerator.MoveNext())
		{
			IMidiMessage message = enumerator.Current.MidiMessage;
			if (message.MessageType == MessageType.Channel)
			{
				chaser.Process((ChannelMessage)message);
			}
			else if (message.MessageType == MessageType.Meta)
			{
				dispatcher.Dispatch(message);
			}
		}
		chaser.Chase();
		int ticks = startPosition;
		while (notFinished)
		{
			for (; ticks < enumerator.Current.AbsoluteTicks; ticks++)
			{
				yield return ticks;
			}
			yield return ticks;
			while (notFinished && enumerator.Current.AbsoluteTicks == ticks)
			{
				dispatcher.Dispatch(enumerator.Current.MidiMessage);
				notFinished = enumerator.MoveNext();
			}
			ticks++;
		}
	}

	public void Insert(int position, IMidiMessage message)
	{
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException("position", position, "IMidiMessage position out of range.");
		}
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		MidiEvent midiEvent = new MidiEvent(this, position, message);
		if (head == null)
		{
			head = midiEvent;
			tail = midiEvent;
		}
		else if (position >= tail.AbsoluteTicks)
		{
			midiEvent.Previous = tail;
			tail.Next = midiEvent;
			tail = midiEvent;
			endOfTrackMidiEvent.SetAbsoluteTicks(Length);
			endOfTrackMidiEvent.Previous = tail;
		}
		else
		{
			MidiEvent next = head;
			while (next.AbsoluteTicks < position)
			{
				next = next.Next;
			}
			midiEvent.Next = next;
			midiEvent.Previous = next.Previous;
			if (next.Previous != null)
			{
				next.Previous.Next = midiEvent;
			}
			else
			{
				head = midiEvent;
			}
			next.Previous = midiEvent;
		}
		count++;
	}

	public void Clear()
	{
		head = (tail = null);
		count = 1;
	}

	public void Merge(Track trk)
	{
		if (trk == null)
		{
			throw new ArgumentNullException("trk");
		}
		if (trk == this || trk.Count == 1)
		{
			return;
		}
		count += trk.Count - 1;
		MidiEvent next = head;
		MidiEvent next2 = trk.head;
		MidiEvent midiEvent = null;
		if (next != null && next.AbsoluteTicks <= next2.AbsoluteTicks)
		{
			midiEvent = new MidiEvent(this, next.AbsoluteTicks, next.MidiMessage);
			next = next.Next;
		}
		else
		{
			midiEvent = new MidiEvent(this, next2.AbsoluteTicks, next2.MidiMessage);
			next2 = next2.Next;
		}
		head = midiEvent;
		while (next != null && next2 != null)
		{
			while (next != null && next.AbsoluteTicks <= next2.AbsoluteTicks)
			{
				midiEvent.Next = new MidiEvent(this, next.AbsoluteTicks, next.MidiMessage);
				midiEvent.Next.Previous = midiEvent;
				midiEvent = midiEvent.Next;
				next = next.Next;
			}
			if (next != null)
			{
				while (next2 != null && next2.AbsoluteTicks <= next.AbsoluteTicks)
				{
					midiEvent.Next = new MidiEvent(this, next2.AbsoluteTicks, next2.MidiMessage);
					midiEvent.Next.Previous = midiEvent;
					midiEvent = midiEvent.Next;
					next2 = next2.Next;
				}
			}
		}
		while (next != null)
		{
			midiEvent.Next = new MidiEvent(this, next.AbsoluteTicks, next.MidiMessage);
			midiEvent.Next.Previous = midiEvent;
			midiEvent = midiEvent.Next;
			next = next.Next;
		}
		while (next2 != null)
		{
			midiEvent.Next = new MidiEvent(this, next2.AbsoluteTicks, next2.MidiMessage);
			midiEvent.Next.Previous = midiEvent;
			midiEvent = midiEvent.Next;
			next2 = next2.Next;
		}
		tail = midiEvent;
		endOfTrackMidiEvent.SetAbsoluteTicks(Length);
		endOfTrackMidiEvent.Previous = tail;
	}

	public void RemoveAt(int index)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", index, "Track index out of range.");
		}
		if (index == Count - 1)
		{
			throw new ArgumentException("Cannot remove the end of track event.", "index");
		}
		MidiEvent midiEvent = GetMidiEvent(index);
		if (midiEvent.Previous != null)
		{
			midiEvent.Previous.Next = midiEvent.Next;
		}
		else
		{
			head = head.Next;
		}
		if (midiEvent.Next != null)
		{
			midiEvent.Next.Previous = midiEvent.Previous;
		}
		else
		{
			tail = tail.Previous;
			endOfTrackMidiEvent.SetAbsoluteTicks(Length);
			endOfTrackMidiEvent.Previous = tail;
		}
		MidiEvent next = (midiEvent.Previous = null);
		midiEvent.Next = next;
		count--;
	}

	public MidiEvent GetMidiEvent(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index", index, "Track index out of range.");
		}
		MidiEvent midiEvent;
		if (index == Count - 1)
		{
			midiEvent = endOfTrackMidiEvent;
		}
		else if (index < Count / 2)
		{
			midiEvent = head;
			for (int i = 0; i < index; i++)
			{
				midiEvent = midiEvent.Next;
			}
		}
		else
		{
			midiEvent = tail;
			for (int num = Count - 2; num > index; num--)
			{
				midiEvent = midiEvent.Previous;
			}
		}
		return midiEvent;
	}

	public void Move(MidiEvent e, int newPosition)
	{
		if (e.Owner != this)
		{
			throw new ArgumentException("MidiEvent does not belong to this Track.");
		}
		if (newPosition < 0)
		{
			throw new ArgumentOutOfRangeException("newPosition");
		}
		if (e == endOfTrackMidiEvent)
		{
			throw new InvalidOperationException("Cannot move end of track message. Use the EndOfTrackOffset property instead.");
		}
		MidiEvent midiEvent = e.Previous;
		MidiEvent midiEvent2 = e.Next;
		if (e.Previous != null && e.Previous.AbsoluteTicks > newPosition)
		{
			e.Previous.Next = e.Next;
			if (e.Next != null)
			{
				e.Next.Previous = e.Previous;
			}
			while (midiEvent != null && midiEvent.AbsoluteTicks > newPosition)
			{
				midiEvent2 = midiEvent;
				midiEvent = midiEvent.Previous;
			}
		}
		else if (e.Next != null && e.Next.AbsoluteTicks < newPosition)
		{
			e.Next.Previous = e.Previous;
			if (e.Previous != null)
			{
				e.Previous.Next = e.Next;
			}
			while (midiEvent2 != null && midiEvent2.AbsoluteTicks < newPosition)
			{
				midiEvent = midiEvent2;
				midiEvent2 = midiEvent2.Next;
			}
		}
		if (midiEvent != null)
		{
			midiEvent.Next = e;
		}
		if (midiEvent2 != null)
		{
			midiEvent2.Previous = e;
		}
		e.Previous = midiEvent;
		e.Next = midiEvent2;
		e.SetAbsoluteTicks(newPosition);
		if (newPosition < head.AbsoluteTicks)
		{
			head = e;
		}
		if (newPosition > tail.AbsoluteTicks)
		{
			tail = e;
		}
		endOfTrackMidiEvent.SetAbsoluteTicks(Length);
		endOfTrackMidiEvent.Previous = tail;
	}
}
