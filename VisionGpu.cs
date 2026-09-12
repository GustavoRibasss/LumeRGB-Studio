// SPDX-License-Identifier: GPL-2.0-or-later
// Based on OpenRGB GigabyteRGBFusionGPUController and NVFC, Adam Honse and contributors.
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
static partial class VisionGpu {
 [DllImport("nvapi64.dll",CallingConvention=CallingConvention.Cdecl)] static extern IntPtr nvapi_QueryInterface(uint id);
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int Init();
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int EnumGpu([Out] IntPtr[] handles,out int count);
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int Pci(IntPtr handle,out uint device,out uint sub,out uint revision,out uint ext);
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int I2c(IntPtr handle,ref Info info,ref uint unknown);
 [StructLayout(LayoutKind.Sequential)] struct Info {public uint version,display;public byte ddc,address;public IntPtr reg;public uint regSize;public IntPtr data;public uint size,speed,speedKhz;public byte port;public uint portSet;}
 static T Function<T>(uint id) where T:class {IntPtr p=nvapi_QueryInterface(id);if(p==IntPtr.Zero) throw new IOException("Função NVIDIA indisponível: "+id.ToString("X"));return Marshal.GetDelegateForFunctionPointer(p,typeof(T)) as T;}
 static void Check(int result,string operation) {if(result!=0) throw new IOException(operation+": código NVIDIA "+result);}
 static IntPtr Find() {
  Check(Function<Init>(0x0150E828)(),"Inicializar NVIDIA");
  var handles=new IntPtr[64];int count;Check(Function<EnumGpu>(0xE5AC921F)(handles,out count),"Enumerar NVIDIA");
  if(count<0 || count>64) throw new IOException("Número inválido de GPUs.");
  for(int i=0;i<count;i++) {uint device,sub,rev,ext;Check(Function<Pci>(0x2DDFB66E)(handles[i],out device,out sub,out rev,out ext),"Identificar GPU");if(device==0x221610DE && sub==0x404B1458) return handles[i];}
  throw new IOException("RTX 3080 Vision 10DE:2216 / 1458:404B não encontrada.");
 }
 public static string Probe() {Find();return "RTX 3080 Vision rev. 2 identificada via NVIDIA. Nenhuma cor enviada.";}
 static byte Transfer(IntPtr gpu,byte value,bool read) {
  IntPtr buffer=Marshal.AllocHGlobal(1);
  try {Marshal.WriteByte(buffer,value);var info=new Info {version=(uint)Marshal.SizeOf(typeof(Info))|0x30000,address=0xC6,data=buffer,size=1,speed=0xFFFF,port=1,portSet=1};uint unknown=0;
   Check(Function<I2c>(read?0x4D7B0709u:0x283AC65Au)(gpu,ref info,ref unknown),read?"Ler RGB RTX":"Enviar RGB RTX");return Marshal.ReadByte(buffer);
  }finally {Marshal.FreeHGlobal(buffer);}
 }
 public static byte[] Commands(Color c,int brightness) {if(brightness<0 || brightness>99) throw new ArgumentOutOfRangeException("brightness");return new byte[]{0x40,c.R,c.G,c.B,0x88,1,5,(byte)brightness};}
 public static void Apply(Color c,int brightness) {
  byte[] commands=Commands(c,brightness);IntPtr gpu=Find();
  foreach(byte b in new byte[]{0xAB,0,0,0}) Transfer(gpu,b,false);
  if(Transfer(gpu,0,true)!=0xAB) throw new IOException("Controlador RGB da RTX não confirmou sua identificação.");
  foreach(byte b in commands) Transfer(gpu,b,false);
 }
 public static bool Test() {if(Marshal.SizeOf(typeof(Info))!=64) return false;byte[] p=Commands(Color.White,99);byte[] expected={0x40,255,255,255,0x88,1,5,99};for(int i=0;i<p.Length;i++) if(p[i]!=expected[i]) return false;foreach(int b in new[]{-1,100}) {try {Commands(Color.Black,b);return false;}catch(ArgumentOutOfRangeException) {}}return true;}
}










