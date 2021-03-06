using System;
using System.Collections.Generic;
using Mono.Debugging.Backend;

namespace Mono.Debugging.Client
{
	[Serializable]
	public class Backtrace
	{
		IBacktrace serverBacktrace;
		int count;
		
		[NonSerialized]
		DebuggerSession session;

		List<StackFrame> frames;
		
		public Backtrace (IBacktrace serverBacktrace)
		{
			this.serverBacktrace = serverBacktrace;
			
			count = serverBacktrace.FrameCount;

			// Get first frame, which is most used(for thread location)
			if (count > 0)
				GetFrame (0, 1);
		}
		
		internal void Attach (DebuggerSession session)
		{
			this.session = session;
			serverBacktrace = session.WrapDebuggerObject (serverBacktrace);
			if (frames != null) {
				foreach (StackFrame f in frames) {
					f.Attach (session);
					f.SourceBacktrace = serverBacktrace;
				}
			}
		}

		public int FrameCount
		{
			get { return count; }
		}

		public StackFrame GetFrame (int n)
		{
			return GetFrame (n, 20);
		}

		private StackFrame GetFrame(int index, int fetchMultipleCount)
		{
			if (frames == null)
				frames = new List<StackFrame>();

			if (index >= frames.Count) {
				StackFrame[] newSet = serverBacktrace.GetStackFrames(frames.Count, index + fetchMultipleCount);
				foreach (StackFrame sf in newSet) {
					sf.SourceBacktrace = serverBacktrace;
					sf.Index = frames.Count;
					frames.Add (sf);
					sf.Attach (session);
				}
			}
			
			if (frames.Count > 0)
				return frames[System.Math.Min (System.Math.Max (0, index), frames.Count - 1)];

			return null;
		}
	}
}
