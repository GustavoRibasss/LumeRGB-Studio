// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Runtime.InteropServices;

static class AudioMeter {
 [ComImport,Guid("BCDE0395-E52F-467C-8E3D-C4579291692E"),ClassInterface(ClassInterfaceType.None)] sealed class MMDeviceEnumerator{}
 [ComImport,Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IDeviceEnumerator {int EnumAudioEndpoints(int flow,int state,out IntPtr devices);int GetDefaultAudioEndpoint(int flow,int role,out IMMDevice device);}
 [ComImport,Guid("D666063F-1587-4E43-81F1-B948E807363F"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IMMDevice {int Activate(ref Guid iid,int ctx,IntPtr p,out IAudioMeterInformation obj);}
 [ComImport,Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IAudioMeterInformation {int GetPeakValue(out float peak);}
 const int ERender=0,Console=0;
 static DateTime last=DateTime.MinValue;static float cached;
 public static float GetPeak(){if((DateTime.UtcNow-last).TotalMilliseconds<35)return cached;last=DateTime.UtcNow;try{var e=(IDeviceEnumerator)(object)new MMDeviceEnumerator();IMMDevice d;int hr=e.GetDefaultAudioEndpoint(ERender,Console,out d);if(hr<0)Marshal.ThrowExceptionForHR(hr);Guid iid=typeof(IAudioMeterInformation).GUID;IAudioMeterInformation m;hr=d.Activate(ref iid,23,IntPtr.Zero,out m);if(hr<0)Marshal.ThrowExceptionForHR(hr);float v;hr=m.GetPeakValue(out v);if(hr<0)Marshal.ThrowExceptionForHR(hr);cached=Math.Max(0,Math.Min(1,v));Marshal.ReleaseComObject(m);Marshal.ReleaseComObject(d);Marshal.ReleaseComObject(e);}catch{cached=0;}return cached;}
}
