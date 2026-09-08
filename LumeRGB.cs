// SPDX-License-Identifier: GPL-2.0-or-later
// AULA packet protocol adapted from dunn1o's OpenRGB contribution (2026).
// https://gitlab.com/CalcProgrammer1/OpenRGB/-/merge_requests/3422
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;

static partial class Hid {
 [StructLayout(LayoutKind.Sequential)] struct InterfaceData { public int size; public Guid guid; public int flags; public IntPtr reserved; }
 [StructLayout(LayoutKind.Sequential)] struct Caps { public ushort usage, page, input, output, feature; [MarshalAs(UnmanagedType.ByValArray, SizeConst=17)] public ushort[] reserved; public ushort links, ib, iv, idi, ob, ov, odi, fb, fv, fdi; }
 [DllImport("hid.dll")] static extern void HidD_GetHidGuid(out Guid guid);
 [DllImport("setupapi.dll", CharSet=CharSet.Unicode, SetLastError=true)] static extern IntPtr SetupDiGetClassDevs(ref Guid guid, IntPtr e, IntPtr parent, uint flags);
 [DllImport("setupapi.dll", SetLastError=true)] static extern bool SetupDiEnumDeviceInterfaces(IntPtr set, IntPtr dev, ref Guid guid, uint index, ref InterfaceData data);
 [DllImport("setupapi.dll", CharSet=CharSet.Unicode, SetLastError=true)] static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr set, ref InterfaceData data, IntPtr detail, uint size, out uint required, IntPtr dev);
 [DllImport("setupapi.dll")] static extern bool SetupDiDestroyDeviceInfoList(IntPtr set);
 [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)] static extern SafeFileHandle CreateFile(string path, uint access, uint share, IntPtr sec, uint mode, uint flags, IntPtr template);
 [DllImport("hid.dll")] static extern bool HidD_GetPreparsedData(SafeFileHandle handle, out IntPtr data);
 [DllImport("hid.dll")] static extern bool HidD_FreePreparsedData(IntPtr data);
 [DllImport("hid.dll")] static extern int HidP_GetCaps(IntPtr data, out Caps caps);
 [StructLayout(LayoutKind.Sequential)] struct Overlapped { public IntPtr internalLow, internalHigh; public uint offset, offsetHigh; public IntPtr ev; }
 [DllImport("kernel32.dll", SetLastError=true)] static extern bool WriteFile(SafeFileHandle h, IntPtr data, uint size, out uint written, IntPtr overlapped);
 [DllImport("kernel32.dll", SetLastError=true)] static extern bool GetOverlappedResult(SafeFileHandle h, IntPtr overlapped, out uint written, bool wait);
 [DllImport("kernel32.dll", SetLastError=true)] static extern bool CancelIoEx(SafeFileHandle h, IntPtr overlapped);
 static void Send(SafeFileHandle handle, byte[] packet) {
  using(var ev = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset)) {
   IntPtr data = Marshal.AllocHGlobal(packet.Length);
   IntPtr ov = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Overlapped)));
   bool pending = false;
   try {
    Marshal.Copy(packet,0,data,packet.Length);
    Marshal.StructureToPtr(new Overlapped {ev=ev.SafeWaitHandle.DangerousGetHandle()},ov,false);
    uint written;
    if(!WriteFile(handle,data,(uint)packet.Length,out written,ov)) {
     int error=Marshal.GetLastWin32Error();
     if(error!=997) throw new IOException("Falha no envio USB (WriteFile): " + error);
     pending=true;
     if(!ev.WaitOne(2000)) throw new IOException("O envio USB excedeu 2 segundos. Reconecte o teclado e tente novamente.");
     bool ok=GetOverlappedResult(handle,ov,out written,false);int resultError=Marshal.GetLastWin32Error();pending=false;
     if(!ok) throw new IOException("Falha ao concluir o envio USB: " + resultError);
    }
    if(written!=packet.Length) throw new IOException("Envio USB incompleto: " + written + "/" + packet.Length);
   } finally {
    if(pending) {CancelIoEx(handle,ov);uint ignored;GetOverlappedResult(handle,ov,out ignored,true);}
    Marshal.FreeHGlobal(ov);Marshal.FreeHGlobal(data);
   }
  }
 }
 public class Device { public string path; public int length; public override string ToString() { return path + " | output=" + length; } }
 public static List<Device> Find(bool msi=false) {
  var devices = new List<Device>(); Guid guid; HidD_GetHidGuid(out guid);
  IntPtr set = SetupDiGetClassDevs(ref guid, IntPtr.Zero, IntPtr.Zero, 18);
  if(set == new IntPtr(-1)) throw new IOException("Não foi possível consultar os dispositivos USB.");
  try {
   for(uint i=0;;i++) {
    InterfaceData data = new InterfaceData(); data.size=Marshal.SizeOf(data);
    if(!SetupDiEnumDeviceInterfaces(set, IntPtr.Zero, ref guid, i, ref data)) { if(Marshal.GetLastWin32Error()!=259) throw new IOException("Falha ao enumerar USB."); break; }
    uint size; SetupDiGetDeviceInterfaceDetail(set, ref data, IntPtr.Zero, 0, out size, IntPtr.Zero);
    IntPtr detail = Marshal.AllocHGlobal((int)size);
    try {
     Marshal.WriteInt32(detail, IntPtr.Size==8 ? 8 : 6);
     if(!SetupDiGetDeviceInterfaceDetail(set, ref data, detail, size, out size, IntPtr.Zero)) continue;
     string path=Marshal.PtrToStringUni(IntPtr.Add(detail,4));
     string lower=path.ToLowerInvariant();
     if(msi ? !lower.Contains("vid_1462&pid_7e09") : (!lower.Contains("vid_372e&pid_103e") || !lower.Contains("mi_02"))) continue;
     using(var handle=CreateFile(path,0,3,IntPtr.Zero,3,0,IntPtr.Zero)) {
      IntPtr pp; if(handle.IsInvalid || !HidD_GetPreparsedData(handle,out pp)) continue;
      try { Caps caps; if(HidP_GetCaps(pp,out caps)==0x110000 && (msi ? (caps.page==1 && caps.usage==0 && caps.feature>=185) : (caps.page>=0xff00 && caps.output==64))) devices.Add(new Device { path=path,length=msi ? caps.feature : caps.output }); }
      finally { HidD_FreePreparsedData(pp); }
     }
    } finally { Marshal.FreeHGlobal(detail); }
   }
  } finally { SetupDiDestroyDeviceInfoList(set); }
  return devices;
 }
 // MSI 185-byte protocol: OpenRGB MSIMotherboardCommon.h and MSIMotherboard185Controller.cpp.
 // T-bond, Adam Honse and OpenRGB contributors. GPL-2.0-or-later.
 [DllImport("hid.dll",SetLastError=true)] static extern bool HidD_GetFeature(SafeFileHandle h, [In,Out] byte[] data, int size);
 [DllImport("hid.dll",SetLastError=true)] static extern bool HidD_SetFeature(SafeFileHandle h, byte[] data, int size);
 static SafeFileHandle OpenMsi() {
  var devices=Find(true);
  if(devices.Count!=1) throw new IOException("Controladora MSI 7E09: encontradas " + devices.Count + " interfaces compatíveis.");
  var h=CreateFile(devices[0].path,0,3,IntPtr.Zero,3,0,IntPtr.Zero);
  if(h.IsInvalid) {int e=Marshal.GetLastWin32Error();h.Dispose();throw new IOException("Não foi possível abrir a MSI: " + e);}
  return h;
 }
 static byte[] ReadMsi(SafeFileHandle h) {
  var p=new byte[185];p[0]=0x52;
  if(!HidD_GetFeature(h,p,p.Length)) throw new IOException("Não foi possível ler o estado RGB da MSI: " + Marshal.GetLastWin32Error());
  if(p[0]!=0x52) throw new IOException("Resposta MSI incompatível. Nenhuma cor foi enviada.");
  return p;
 }
 public static string ProbeMsi() {
  using(var h=OpenMsi()) {var p=ReadMsi(h);return "MSI 7E09: estado RGB lido (185 bytes). ARGB: " + BitConverter.ToString(p,31,33);}
 }
 public static byte[] MsiPacket(byte[] current,Color c,int brightness) {return MsiPacket(current,new[]{c,c,c},brightness);}
 public static byte[] MsiPacket(byte[] current,Color[] colors,int brightness) {
  if(current==null || current.Length!=185 || current[0]!=0x52) throw new ArgumentException("Estado MSI inválido.");
  if(colors==null||colors.Length!=3) throw new ArgumentException("São necessárias três cores ARGB MSI.");
  if(brightness<0 || brightness>10) throw new ArgumentOutOfRangeException("brightness");
  byte[] p=(byte[])current.Clone();int[] offsets={31,42,53};
  for(int i=0;i<offsets.Length;i++) {Color c=colors[i];int offset=offsets[i];
   p[offset]=(byte)(brightness==0 ? 0 : 1);
   p[offset+1]=p[offset+5]=c.R;p[offset+2]=p[offset+6]=c.G;p[offset+3]=p[offset+7]=c.B;
   p[offset+4]=(byte)((brightness<<2)|1);
   p[offset+8]=(byte)(p[offset+8]|0x80);
   p[offset+9]=(byte)(offset==53 ? 4 : 0);
  }
  p[78]=(byte)(p[78]&0x7f);p[82]=0x81;p[184]=0;
  return p;
 }
 public static void ApplyMsi(Color c,int brightness) {
  using(var h=OpenMsi()) {
   byte[] old=ReadMsi(h);byte[] next=MsiPacket(old,c,brightness);
   old[184]=0;
   if(!HidD_SetFeature(h,old,old.Length)) throw new IOException("Falha ao preparar o RGB MSI: " + Marshal.GetLastWin32Error());
   if(!HidD_SetFeature(h,next,next.Length)) throw new IOException("Falha ao aplicar a cor MSI: " + Marshal.GetLastWin32Error());
  }
 }
 public static void ApplyMsi(Color[] colors,int brightness) {
  using(var h=OpenMsi()) {
   byte[] old=ReadMsi(h);byte[] next=MsiPacket(old,colors,brightness);
   old[184]=0;
   if(!HidD_SetFeature(h,old,old.Length)) throw new IOException("Falha ao preparar o RGB MSI: " + Marshal.GetLastWin32Error());
   if(!HidD_SetFeature(h,next,next.Length)) throw new IOException("Falha ao aplicar as zonas MSI: " + Marshal.GetLastWin32Error());
  }
 }
 public static bool TestMsi() {
  byte[] original=new byte[185];for(int i=0;i<185;i++) original[i]=(byte)i;original[0]=0x52;
  byte[] expected=(byte[])original.Clone();
  foreach(int offset in new int[]{31,42,53}) {byte[] zone=new byte[]{1,255,255,255,41,255,255,255,(byte)(original[offset+8]|128),(byte)(offset==53?4:0)};Array.Copy(zone,0,expected,offset,10);}
  expected[78]&=127;expected[82]=129;expected[184]=0;
  byte[] actual=MsiPacket(original,Color.White,10);
  for(int i=0;i<185;i++) if(actual[i]!=expected[i] || original[i]!=(i==0?82:(byte)i)) return false;
  byte[] off=MsiPacket(original,Color.Black,0);foreach(int offset in new int[]{31,42,53}) if(off[offset]!=0) return false;
  foreach(int invalid in new int[]{-1,11}) {try {MsiPacket(original,Color.White,invalid);return false;}catch(ArgumentOutOfRangeException) {}}
  return true;
 }
 public static byte[] Packet(Color c, int brightness) {
  if(brightness<0 || brightness>20) throw new ArgumentOutOfRangeException("brightness");
  byte[] p=new byte[64]; p[0]=9;p[1]=4;p[2]=1;p[4]=1;p[6]=7;p[7]=1;
  p[9]=c.R;p[10]=c.G;p[11]=c.B;p[12]=(byte)brightness;p[13]=2;
  int sum=0;for(int i=0;i<63;i++) sum+=p[i];p[63]=(byte)(255-(sum&255));return p;
 }
 // AULA official web driver sync_light_edge: command 4, subcommand 6.
 // https://heb.aulacn.com/app-B6-MiDbc.js (xxt / sync_light_edge)
 public static byte[] FollowBarPacket(Color color,int brightness) {
  if(brightness<0 || brightness>20)throw new ArgumentOutOfRangeException("brightness");
  // Scale the static edge color: this firmware does not visibly honor its brightness field.
  Color dimmed=Color.FromArgb((color.R*brightness+10)/20,(color.G*brightness+10)/20,(color.B*brightness+10)/20);
  byte[] p=Packet(dimmed,brightness==0?0:20);p[2]=6;p[3]=0;p[7]=(byte)(brightness==0?0:1);
  int sum=0;for(int i=0;i<63;i++)sum+=p[i];p[63]=(byte)(255-(sum&255));return p;
 }
 public static void ApplyBar(Color color,int brightness) {lock(KeyboardIoLock){ApplyBarLocked(color,brightness);}} static void ApplyBarLocked(Color color,int brightness) {
  var devices=Find();if(devices.Count!=1)throw new IOException("Interface RGB do Hero 68 indisponível.");
  using(var handle=CreateFile(devices[0].path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)) {
   if(handle.IsInvalid)throw new IOException("Não foi possível abrir a barra RGB do teclado.");
   Send(handle,BarPacket(color,brightness));System.Threading.Thread.Sleep(20);
  }
 }
 public static void Apply(Color color,int brightness) {lock(KeyboardIoLock){ApplyKeyboardLocked(color,brightness);}} static void ApplyKeyboardLocked(Color color,int brightness) {
  var devices=Find();
  if(devices.Count!=1) throw new IOException(devices.Count==0 ? "Não encontrei uma interface RGB compatível. Conecte o Hero 68 por USB e feche o configurador AULA." : "Encontrei mais de uma interface compatível. É necessário verificar a interface antes de enviar cores.");
  using(var handle=CreateFile(devices[0].path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)) {
   if(handle.IsInvalid) throw new IOException("Não foi possível abrir o teclado. Código Windows: " + Marshal.GetLastWin32Error());
   LastKeyboardColor=color;LastKeyboardBrightness=brightness;byte[] p=Packet(color,brightness);
   Send(handle,p); System.Threading.Thread.Sleep(20);
   Send(handle,BarPacket(color,brightness));System.Threading.Thread.Sleep(20);
  }
 }
}
class RgbApp:Form {
 public static Color FullIntensity(Color c) {int max=Math.Max(c.R,Math.Max(c.G,c.B));if(max==0)return Color.Black;return Color.FromArgb((c.R*255+max/2)/max,(c.G*255+max/2)/max,(c.B*255+max/2)/max);}
 Color chosen=Color.FromArgb(117,88,255); Label status; Button colorButton; StudioSlider brightness; Button apply;
 public RgbApp() {
  Text="ThebestRGB v9 • Teclado, gabinete, RTX e RAM"; ClientSize=new Size(700,875);MinimumSize=new Size(716,550);StartPosition=FormStartPosition.CenterScreen;AutoScroll=true;AutoScrollMinSize=new Size(680,875);
  BackColor=Color.FromArgb(19,21,30);ForeColor=Color.White;Font=new Font("Segoe UI",10);AutoScaleMode=AutoScaleMode.Dpi;
  var title=new Label {Text="ThebestRGB",Font=new Font("Segoe UI",25,FontStyle.Bold),AutoSize=true,Location=new Point(28,23)};Controls.Add(title);
  Controls.Add(new Label {Text="Controle local de iluminação",AutoSize=true,ForeColor=Color.Silver,Location=new Point(31,78)});
  var keyboard=new GroupBox {Text="AULA HERO 68 • TECLAS E BARRA LED",ForeColor=Color.White,Location=new Point(28,118),Size=new Size(644,200),Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right};Controls.Add(keyboard);
  colorButton=new Button {Text="Escolher cor",Location=new Point(20,37),Size=new Size(175,42),BackColor=chosen,ForeColor=Color.White,FlatStyle=FlatStyle.Flat};colorButton.Click+=delegate {using(var dialog=new ColorDialog {Color=chosen,FullOpen=true}) if(dialog.ShowDialog()==DialogResult.OK) {chosen=FullIntensity(dialog.Color);colorButton.BackColor=chosen;colorButton.ForeColor=chosen.GetBrightness()>0.6f?Color.Black:Color.White;status.Text="Cor RGB: "+chosen.R+", "+chosen.G+", "+chosen.B+". Ajuste a intensidade na barra de brilho.";} };keyboard.Controls.Add(colorButton);
  var brightnessLabel=new Label {Text="Brilho: 100%",AutoSize=true,Location=new Point(220,35)}; keyboard.Controls.Add(brightnessLabel);
  brightness=new StudioSlider {Minimum=0,Maximum=20,Value=20,Location=new Point(214,58),Size=new Size(260,40),TickStyle=TickStyle.None};keyboard.Controls.Add(brightness); brightness.ValueChanged+=delegate {brightnessLabel.Text="Brilho: " + (brightness.Value*5) + "%";};
  apply=new Button {Text="Aplicar ao teclado",Location=new Point(20,107),Size=new Size(175,39),BackColor=Color.FromArgb(92,70,210),FlatStyle=FlatStyle.Flat};apply.Click+=delegate {SetColor(brightness.Value);};keyboard.Controls.Add(apply);
  var off=new Button {Text="Apagar",Location=new Point(210,107),Size=new Size(105,39),FlatStyle=FlatStyle.Flat};off.Click+=delegate {SetColor(0);};keyboard.Controls.Add(off);
  var detect=new Button {Text="Detectar",Location=new Point(330,107),Size=new Size(105,39),FlatStyle=FlatStyle.Flat};detect.Click+=delegate {Detect();};keyboard.Controls.Add(detect);
  keyboard.Controls.Add(new Label {Text="Cor em intensidade máxima • use Brilho para reduzir",AutoSize=true,ForeColor=Color.Silver,Location=new Point(20,166)});
  var cabinet=new GroupBox {Text="GABINETE · MSI B650M PROJECT ZERO",ForeColor=Color.White,Location=new Point(28,334),Size=new Size(644,150),Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right};Controls.Add(cabinet);
    cabinet.Controls.Add(new Label {Text="Usa a cor e o brilho escolhidos acima • 3 conectores ARGB",AutoSize=true,ForeColor=Color.Silver,Location=new Point(20,30)});
  var caseApply=new Button {Text="Aplicar ao gabinete",Location=new Point(20,63),Size=new Size(175,39),FlatStyle=FlatStyle.Flat};caseApply.Click+=delegate {SetCabinet(brightness.Value);};cabinet.Controls.Add(caseApply);
  var caseOff=new Button {Text="Apagar gabinete",Location=new Point(210,63),Size=new Size(155,39),FlatStyle=FlatStyle.Flat};caseOff.Click+=delegate {SetCabinet(0);};cabinet.Controls.Add(caseOff);
  cabinet.Controls.Add(new Label {Text="Se a cor voltar sozinha, feche o Mystic Light no MSI Center.",AutoSize=true,ForeColor=Color.Silver,Location=new Point(20,118)});
  var gpuBox=new GroupBox {Text="RTX 3080 VISION • LOGO RGB",ForeColor=Color.White,Location=new Point(28,495),Size=new Size(644,100),Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right};Controls.Add(gpuBox);
  var gpuApply=new Button {Text="Aplicar à RTX",Location=new Point(20,32),Size=new Size(175,39),FlatStyle=FlatStyle.Flat};gpuApply.Click+=delegate {SetGpu(brightness.Value);};gpuBox.Controls.Add(gpuApply);
  var gpuOff=new Button {Text="Apagar logo",Location=new Point(210,32),Size=new Size(155,39),FlatStyle=FlatStyle.Flat};gpuOff.Click+=delegate {SetGpu(0);};gpuBox.Controls.Add(gpuOff);
  var ramBox=new GroupBox {Text="VIPER ELITE 5 • DOIS MÓDULOS RGB",ForeColor=Color.White,Location=new Point(28,606),Size=new Size(644,130)};Controls.Add(ramBox);
  var ramStart=new Button {Text="Ativar RAM",Location=new Point(20,32),Size=new Size(155,39),FlatStyle=FlatStyle.Flat};ramBox.Controls.Add(ramStart);
  ramStart.Click+=async delegate {ramStart.Enabled=false;status.Text="Ativando o acesso à RAM...";try {await System.Threading.Tasks.Task.Run(()=>RamBridge.Start());status.Text="Os dois módulos estão disponíveis. Escolha a cor e aplique.";}catch(Exception ex){status.Text=ex.Message;}finally {ramStart.Enabled=true;}};
  var ramApply=new Button {Text="Aplicar à RAM",Location=new Point(190,32),Size=new Size(155,39),FlatStyle=FlatStyle.Flat};ramBox.Controls.Add(ramApply);ramApply.Click+=delegate {SetRam(brightness.Value);};
  var ramOff=new Button {Text="Apagar RAM",Location=new Point(360,32),Size=new Size(135,39),FlatStyle=FlatStyle.Flat};ramBox.Controls.Add(ramOff);ramOff.Click+=delegate {SetRam(0);};
  ramBox.Controls.Add(new Label {Text="Ativar RAM pode pedir autorização de administrador do Windows.",AutoSize=true,ForeColor=Color.Silver,Location=new Point(20,88)});
  var both=new Button {Text="Aplicar a todos",Location=new Point(28,755),Size=new Size(190,39),FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(92,70,210)};
  both.Click+=delegate {var results=new List<string>();try {Hid.Apply(chosen,brightness.Value);results.Add("Teclado: enviado.");}catch(Exception ex) {results.Add("Teclado: "+ex.Message);}try {Hid.ApplyMsi(chosen,(brightness.Value+1)/2);results.Add("Gabinete: enviado.");}catch(Exception ex) {results.Add("Gabinete: "+ex.Message);}try {VisionGpu.Apply(chosen,(brightness.Value*99+10)/20);results.Add("RTX: enviado.");}catch(Exception ex) {results.Add("RTX: "+ex.Message);}status.Text=string.Join(" ",results.ToArray());};Controls.Add(both);
  both.Click+=delegate {string previous=status.Text;try {RamBridge.Apply(chosen,brightness.Value);status.Text=previous+" RAM: confirmada.";}catch(Exception ex){status.Text=previous+" RAM: "+ex.Message;}};
  status=new Label {Text="Pronto para detectar o teclado.",Location=new Point(30,811),Size=new Size(642,60),ForeColor=Color.Silver,Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right};Controls.Add(status);
  Shown+=delegate {Detect();};
 }
 void SetRam(int level) {try {status.Text=RamBridge.Apply(chosen,level);}catch(Exception ex) {status.Text=ex.Message;}}
 void SetCabinet(int level) {try {Hid.ApplyMsi(chosen,(level+1)/2);status.Text="Cor enviada aos três conectores ARGB. Confira as luzes do gabinete.";}catch(Exception ex) {status.Text=ex.Message;}}
 void SetGpu(int level) {try {VisionGpu.Apply(chosen,(level*99+10)/20);status.Text="Cor enviada ao logo da RTX. Confira a iluminação.";}catch(Exception ex) {status.Text=ex.Message;}}
 void Detect() {try {int n=Hid.Find().Count;status.Text=n==1 ? "Hero 68 detectado. Escolha a cor e clique em Aplicar." : "Interfaces RGB compatíveis encontradas: "+n+". Nenhum comando enviado.";} catch(Exception ex) {status.Text=ex.Message;}}
 void SetColor(int level) {apply.Enabled=false;try {Hid.Apply(chosen,level);status.Text="Comando enviado ao teclado. Confira a mudança nos LEDs.";}catch(Exception ex) {status.Text=ex.Message;}finally {apply.Enabled=true;}}
 [STAThread] static int Main(string[] args) {
  if(args.Length>0 && args[0]=="--test-bar-white") {try {Hid.ApplyBar(Color.White,20);Console.WriteLine("Comando da barra LED enviado: branco, brilho 20.");return 0;}catch(Exception ex){Console.Error.WriteLine(ex.Message);return 1;}}
  if(args.Length>0 && args[0]=="--test-vivid-magenta") {int failures=0;Color c=FullIntensity(Color.FromArgb(64,0,64));foreach(var action in new Action[]{()=>Hid.Apply(c,20),()=>Hid.ApplyMsi(c,10),()=>VisionGpu.Apply(c,99),()=>RamBridge.Apply(c,20)}) {try {action();Console.WriteLine("Envio RGB 255,0,255 concluído.");}catch(Exception ex){Console.Error.WriteLine(ex.Message);failures++;}}return failures;}
  if(args.Length>0 && args[0]=="--probe-ram") {try {Console.WriteLine(RamBridge.Probe());return 0;}catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
  if(args.Length>0 && args[0]=="--test-ram-white") {try {Console.WriteLine(RamBridge.Apply(Color.White,20));return 0;}catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
  if(args.Length>0 && args[0]=="--probe-gpu") {try {Console.WriteLine(VisionGpu.Probe());return 0;}catch(Exception ex) {Console.Error.WriteLine(ex.Message);return 1;}}
  if(args.Length>0 && args[0]=="--test-gpu-white") {try {VisionGpu.Apply(Color.White,99);Console.WriteLine("Logo RTX: branco máximo enviado.");return 0;}catch(Exception ex) {Console.Error.WriteLine(ex.Message);return 1;}}
  if(args.Length>0 && args[0]=="--probe-msi") {try {Console.WriteLine(Hid.ProbeMsi());return 0;}catch(Exception ex) {Console.Error.WriteLine(ex.Message);return 1;}}
  if(args.Length>0 && args[0]=="--test-msi-white") {try {Hid.ApplyMsi(Color.White,10);Console.WriteLine("Branco máximo enviado aos três conectores ARGB MSI.");return 0;}catch(Exception ex) {Console.Error.WriteLine(ex.Message);return 1;}}
  if(args.Length>0 && args[0]=="--self-test") {
   if(!Hid.TestMsi()) return 4; if(!VisionGpu.Test()) return 5; if(!RamBridge.Test()) return 6;
   byte[] bar=Hid.BarPacket(Color.White,20);int barSum=0;foreach(byte b in bar)barSum+=b;if(bar.Length!=64||bar[2]!=6||bar[3]!=0||bar[7]!=1||bar[12]!=20||(barSum&255)!=255||Hid.BarPacket(Color.Black,0)[7]!=0)return 8;
   if(FullIntensity(Color.FromArgb(64,0,64)).ToArgb()!=Color.Magenta.ToArgb() || FullIntensity(Color.White).ToArgb()!=Color.White.ToArgb() || FullIntensity(Color.Black).ToArgb()!=Color.Black.ToArgb() || FullIntensity(Color.FromArgb(32,64,0)).ToArgb()!=Color.FromArgb(128,255,0).ToArgb()) return 7;
   foreach(Color c in new Color[]{Color.Black,Color.White,Color.Red,Color.FromArgb(5,120,255)}) for(int b=0;b<=20;b++) {byte[] p=Hid.Packet(c,b);int sum=0;foreach(byte x in p) sum+=x;if(p.Length!=64 || (sum&255)!=255 || p[9]!=c.R || p[10]!=c.G || p[11]!=c.B || p[12]!=b) return 1;}
   return 0;
  }
  if(args.Length>0 && args[0]=="--test-white") {try {Hid.Apply(Color.White,20);Console.WriteLine("Branco RGB 255/255/255, brilho 20: 64 bytes enviados.");return 0;}catch(Exception ex) {Console.Error.WriteLine(ex.Message);return 1;}}
  if(args.Length>0 && args[0]=="--test-red") {try {Hid.Apply(Color.Red,10);Console.WriteLine("WriteFile: envio completo de 64 bytes, vermelho, brilho 10. Confirmação visual pendente.");return 0;}catch(Exception ex) {Console.Error.WriteLine(ex.Message);return 1;}}
  if(args.Length>0 && args[0]=="--detect") {try {foreach(var d in Hid.Find()) Console.WriteLine(d);return 0;}catch(Exception ex) {Console.Error.WriteLine(ex.Message);return 1;}}
  Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Application.Run(new RgbApp());return 0;
 }
}















