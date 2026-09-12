// Starts the bundled OpenRGB SDK server without requiring a separate installation.
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

static class OpenRgbRuntime {
 static readonly string[] Files={"OpenRGB.exe","hidapi.dll","libEGL.dll","libGLESv2.dll","libusb-1.0.dll","LpcIO.bin","PawnIOLib.dll","Qt5Core.dll","Qt5Gui.dll","Qt5Widgets.dll","SmbusI801.bin","SmbusIntelSkylakeIMC.bin","SmbusNCT6793.bin","SmbusPIIX4.bin","imageformats/qgif.dll","imageformats/qico.dll","imageformats/qjpeg.dll","platforms/qwindows.dll","styles/qwindowsvistastyle.dll"};
 static Process process;
 static string tempPath;
 public static Process StartServer(){if(process!=null&&!process.HasExited)return process;tempPath=Path.Combine(Path.GetTempPath(),"ThebestRGB", "openrgb-"+Process.GetCurrentProcess().Id+"-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(tempPath);var asm=typeof(OpenRgbRuntime).Assembly;foreach(string file in Files){string resource="OpenRGB."+file.Replace('/','.');using(var input=asm.GetManifestResourceStream(resource)){if(input==null)throw new IOException("Recurso OpenRGB ausente: "+file);string target=Path.Combine(tempPath,file.Replace('/',Path.DirectorySeparatorChar));Directory.CreateDirectory(Path.GetDirectoryName(target));using(var output=File.Create(target)){input.CopyTo(output);}}}string exe=Path.Combine(tempPath,"OpenRGB.exe");process=Process.Start(new ProcessStartInfo(exe,"--server --noautoconnect"){UseShellExecute=true,Verb="runas",WindowStyle=ProcessWindowStyle.Hidden,WorkingDirectory=tempPath});return process;}
 public static void StopServer(){try{if(process!=null&&!process.HasExited)process.CloseMainWindow();}catch{}process=null;}
}
