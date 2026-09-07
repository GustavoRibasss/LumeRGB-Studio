// SPDX-License-Identifier: GPL-2.0-or-later
// OpenRGB SDK v0, based on NetworkProtocol.h and RGBController.cpp.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
static partial class RamBridge {
 public class Device {public uint id,type,active;public string name,location;public int leds,staticIndex=-1;public byte[] staticMode;public bool direct;public uint[] colors;}
 sealed class Connection:IDisposable {
  TcpClient tcp;NetworkStream stream;BinaryReader r;BinaryWriter w;
  public Connection() {tcp=new TcpClient{NoDelay=true};try {var a=tcp.BeginConnect("127.0.0.1",6742,null,null);using(a.AsyncWaitHandle) {if(!a.AsyncWaitHandle.WaitOne(1200)) throw new IOException("Clique em Ativar RAM.");}tcp.EndConnect(a);stream=tcp.GetStream();stream.ReadTimeout=3000;stream.WriteTimeout=3000;r=new BinaryReader(stream);w=new BinaryWriter(stream);}catch {tcp.Close();throw;}}
  public void Send(uint d,uint c,byte[] b) {w.Write(Encoding.ASCII.GetBytes("ORGB"));w.Write(d);w.Write(c);w.Write((uint)b.Length);w.Write(b);w.Flush();}
  byte[] Read(int n) {var b=r.ReadBytes(n);if(b.Length!=n) throw new EndOfStreamException();return b;}
  public byte[] Request(uint d,uint c) {Send(d,c,new byte[0]);for(int i=0;i<32;i++) {if(Encoding.ASCII.GetString(Read(4))!="ORGB") throw new IOException("Resposta RGB inválida.");uint rd=r.ReadUInt32(),rc=r.ReadUInt32(),n=r.ReadUInt32();if(n>1048576) throw new IOException("Resposta RGB excedeu limite.");byte[] b=Read((int)n);if(rd==d&&rc==c)return b;}throw new IOException("Resposta RGB não recebida.");}
  public List<Device> List() {byte[] b=Request(0,0);if(b.Length!=4)throw new IOException("Contagem inválida.");uint n=BitConverter.ToUInt32(b,0);if(n>128)throw new IOException("Contagem excedeu limite.");var ds=new List<Device>();for(uint i=0;i<n;i++){Device d=Parse(Request(i,1));d.id=i;if(d.type==1&&d.name=="ENE DRAM")ds.Add(d);}return ds;}
  public Device Get(uint id){return Parse(Request(id,1));}
  public void Dispose(){tcp.Close();}
 }
 static string Str(BinaryReader r){int n=r.ReadUInt16();byte[] b=r.ReadBytes(n);if(b.Length!=n)throw new EndOfStreamException();return Encoding.UTF8.GetString(b).TrimEnd('\0');}
 static void Skip(BinaryReader r,int n){if(n<0||r.BaseStream.Length-r.BaseStream.Position<n)throw new EndOfStreamException();r.BaseStream.Position+=n;}
 public static Device Parse(byte[] b){using(var r=new BinaryReader(new MemoryStream(b))){
  if(r.ReadUInt32()!=b.Length)throw new IOException("Descrição RGB truncada.");var d=new Device();d.type=r.ReadUInt32();d.name=Str(r);Str(r);Str(r);Str(r);d.location=Str(r);
  int modes=r.ReadUInt16();d.active=r.ReadUInt32();for(int i=0;i<modes;i++){int start=(int)r.BaseStream.Position;string mode=Str(r);if(mode=="Direct")d.direct=true;Skip(r,36);int n=r.ReadUInt16();Skip(r,n*4);if(mode=="Static"){d.staticIndex=i;d.staticMode=new byte[(int)r.BaseStream.Position-start];Array.Copy(b,start,d.staticMode,0,d.staticMode.Length);}}
  int zones=r.ReadUInt16();for(int i=0;i<zones;i++){Str(r);Skip(r,16);int n=r.ReadUInt16();Skip(r,n);}
  d.leds=r.ReadUInt16();for(int i=0;i<d.leds;i++){Str(r);Skip(r,4);}int colors=r.ReadUInt16();if(colors!=d.leds||colors>512)throw new IOException("LEDs incompatíveis.");d.colors=new uint[colors];for(int i=0;i<colors;i++)d.colors[i]=r.ReadUInt32();if(r.BaseStream.Position!=r.BaseStream.Length)throw new IOException("Versão RGB incompatível.");return d;
 }}
 public static string Probe(){using(var c=new Connection()){var ds=c.List();if(ds.Count!=2)throw new IOException("Módulos ENE detectados: "+ds.Count+" (esperados: 2).");var text=new List<string>();foreach(var d in ds)text.Add(d.name+" | "+d.location+" | LEDs="+d.leds+" Direct="+d.direct);return string.Join(Environment.NewLine,text.ToArray());}}
 public static string Start(){try{return Probe();}catch{}string exe=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"runtime","OpenRGB.exe");if(!File.Exists(exe))throw new IOException("Mantenha a pasta runtime junto do aplicativo.");Process.Start(new ProcessStartInfo(exe,"--server --noautoconnect"){UseShellExecute=true,Verb="runas",WindowStyle=ProcessWindowStyle.Hidden,WorkingDirectory=Path.GetDirectoryName(exe)});for(int i=0;i<15;i++){Thread.Sleep(500);try{return Probe();}catch{}}throw new IOException("RAM indisponível. Confira a autorização do Windows.");}
 public static byte[] Payload(int leds,Color c,int level){if(leds<1||leds>512||level<0||level>20)throw new ArgumentOutOfRangeException();using(var m=new MemoryStream()){var w=new BinaryWriter(m);w.Write((uint)(6+leds*4));w.Write((ushort)leds);for(int i=0;i<leds;i++){w.Write((byte)(c.R*level/20));w.Write((byte)(c.G*level/20));w.Write((byte)(c.B*level/20));w.Write((byte)0);}return m.ToArray();}}
 public static string Apply(Color color,int level){using(var c=new Connection()){var ds=c.List();if(ds.Count!=2)throw new IOException("Esperava dois módulos ENE; encontrados: "+ds.Count);foreach(var d in ds)if(d.staticMode==null||d.leds<1)throw new IOException("RAM sem modo Static compatível.");
  foreach(var d in ds){byte[] payload=Payload(d.leds,color,level);using(var m=new MemoryStream()){var w=new BinaryWriter(m);w.Write((uint)(8+d.staticMode.Length));w.Write(d.staticIndex);w.Write(d.staticMode);c.Send(d.id,1101,m.ToArray());}Thread.Sleep(150);c.Send(d.id,1050,payload);Thread.Sleep(250);Device after=c.Get(d.id);if(after.active!=d.staticIndex)throw new IOException("Modo Static não confirmado pelo OpenRGB.");uint expected=(uint)(color.R*level/20)|((uint)(color.G*level/20)<<8)|((uint)(color.B*level/20)<<16);foreach(uint v in after.colors)if((v&0xffffff)!=expected)throw new IOException("A cor da RAM não foi recebida pelo OpenRGB.");}
  return "Modo Static e cor enviados. Confirmação física dos LEDs pendente.";
 }}
 public static bool Test(){byte[] p=Payload(8,Color.White,20);if(p.Length!=38||p[0]!=38||p[4]!=8||p[6]!=255||p[37]!=0)return false;p=Payload(8,Color.White,0);for(int i=6;i<p.Length;i++)if(p[i]!=0)return false;try{Parse(new byte[4]);return false;}catch(IOException){}return true;}
}










