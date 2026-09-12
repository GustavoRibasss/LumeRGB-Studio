// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
sealed class FrameTiming:IDisposable {
 [DllImport("winmm.dll")] static extern uint timeBeginPeriod(uint period);
 [DllImport("winmm.dll")] static extern uint timeEndPeriod(uint period);
 bool enabled;
 public FrameTiming(){enabled=timeBeginPeriod(1)==0;}
 public void Dispose(){if(enabled){timeEndPeriod(1);enabled=false;}}
}
sealed class FramePacer {
 readonly Stopwatch clock=Stopwatch.StartNew();
 readonly double interval;
 double next;
 public FramePacer(int fps){if(fps<1||fps>60)throw new ArgumentOutOfRangeException("fps");interval=1000.0/fps;next=interval;}
 public Task Wait(CancellationToken token){
  double now=clock.Elapsed.TotalMilliseconds;
  // Late frames are dropped. Never queue catch-up bursts.
  if(next<now)next=now+interval;
  int delay=Math.Max(1,(int)Math.Ceiling(next-now));
  // A cancellable kernel wait avoids the coarse .NET Framework Task.Delay timer queue.
  if(token.WaitHandle.WaitOne(delay))token.ThrowIfCancellationRequested();
  next+=interval;
  return Task.FromResult(0);
 }
}










