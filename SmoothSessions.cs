// SPDX-License-Identifier: GPL-2.0-or-later
// Persistent color streams, using the same validated device addresses and packets.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using Microsoft.Win32.SafeHandles;

interface IColorStream:IDisposable {void SendFrame(Color color,int brightness);}
static class ColorStreams {
 public static IColorStream Open(int index,LightState state,MsiZoneSettings zones=null,ColorBalance balance=null){
  switch(index){case 0:return new Hid.KeyboardStream();case 1:return new Hid.CaseStream(zones,balance);case 2:return new VisionGpu.ColorStream(state);case 3:return new RamBridge.ColorStream(state);default:throw new ArgumentOutOfRangeException("index");}
 }
}
static partial class Hid {
 public sealed class KeyboardStream:IColorStream {
  SafeFileHandle handle;byte[] lastBar;
  public KeyboardStream(){
   var devices=Find();if(devices.Count!=1)throw new IOException("Interface RGB do Hero 68 indisponível.");
   handle=CreateFile(devices[0].path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero);
   if(handle.IsInvalid){handle.Dispose();throw new IOException("Não foi possível abrir o teclado.");}
  }
  public void SendFrame(Color color,int brightness){
   int level=(brightness+2)/5;lock(KeyboardIoLock){LastKeyboardColor=color;LastKeyboardBrightness=level;
   Send(handle,Packet(color,level));Thread.Sleep(5);
   byte[] bar=BarPacket(color,level);if(lastBar==null||!System.Linq.Enumerable.SequenceEqual(lastBar,bar)){Send(handle,bar);lastBar=bar;}}
  }
  public void Dispose(){handle.Dispose();}
 }
 public sealed class CaseStream:IColorStream {
 SafeFileHandle handle;byte[] baseline;
 MsiZoneSettings zones;ColorBalance balance;
 public CaseStream(MsiZoneSettings settings=null,ColorBalance calibration=null){
  zones=settings==null?new MsiZoneSettings():settings.Copy();balance=calibration==null?new ColorBalance():calibration.Copy();
   handle=OpenMsi();
   try{baseline=ReadMsi(handle);baseline[184]=0;
    if(!HidD_SetFeature(handle,baseline,baseline.Length))throw new IOException("Falha ao preparar o RGB MSI.");
   }catch{handle.Dispose();throw;}
 }
 public void SendFrame(Color color,int brightness){
   Color[] colors=zones.Resolve(color);for(int i=0;i<colors.Length;i++)colors[i]=balance.Apply(colors[i]);
   byte[] next=MsiPacket(baseline,colors,(brightness+5)/10);
   if(!HidD_SetFeature(handle,next,next.Length))throw new IOException("Falha no envio RGB MSI.");
  }
  public void Dispose(){handle.Dispose();}
 }
}
static partial class VisionGpu {
 public sealed class ColorStream:IColorStream {
  IntPtr gpu;int lastBrightness;
  public ColorStream(LightState state){
   gpu=Find();
   foreach(byte b in new byte[]{0xAB,0,0,0})Transfer(gpu,b,false);
   if(Transfer(gpu,0,true)!=0xAB)throw new IOException("Controlador RGB da RTX não confirmou sua identificação.");
   lastBrightness=(state.Brightness*99+50)/100;
   foreach(byte b in Commands(state.Color,lastBrightness))Transfer(gpu,b,false);
  }
  public void SendFrame(Color color,int brightness){
   foreach(byte b in new byte[]{0x40,color.R,color.G,color.B})Transfer(gpu,b,false);
   int level=(brightness*99+50)/100;
   if(level!=lastBrightness){foreach(byte b in new byte[]{0x88,1,5,(byte)level})Transfer(gpu,b,false);lastBrightness=level;}
  }
  public void Dispose(){}
 }
}
static partial class RamBridge {
 public sealed class ColorStream:IColorStream {
  Connection connection;List<Device> devices;
  public ColorStream(LightState state){
   // Establish the proven Static mode once, never Direct mode on this RAM.
   Apply(state.Color,(state.Brightness+2)/5);
   connection=new Connection();
   try{devices=connection.List();if(devices.Count!=2)throw new IOException("Esperava dois módulos ENE.");}
   catch{connection.Dispose();throw;}
  }
  public void SendFrame(Color color,int brightness){
   int level=(brightness+2)/5;
   foreach(var d in devices){
    connection.Send(d.id,1050,Payload(d.leds,color,level));
    // Wait for the server to process the color before queuing another frame.
    Device after=connection.Get(d.id);
    if(after.name!=d.name||after.location!=d.location||after.active!=d.staticIndex)throw new IOException("O controle da RAM mudou. Pare outros aplicativos RGB e aplique novamente.");
    uint expected=(uint)(color.R*level/20)|((uint)(color.G*level/20)<<8)|((uint)(color.B*level/20)<<16);
    foreach(uint actual in after.colors)if((actual&0xffffff)!=expected)throw new IOException("A RAM não confirmou o recebimento da cor.");
   }
  }
  public void Dispose(){connection.Dispose();}
 }
}












