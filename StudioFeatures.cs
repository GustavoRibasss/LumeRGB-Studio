// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web.Script.Serialization;
class StudioPreset {
 public string Name;public int Mode,Speed;public EffectOptions Options;
public static StudioPreset[] All(){return new[]{new StudioPreset{Name="Elétrica azul",Mode=16,Speed=2,Options=new EffectOptions{Foreground=57343,Grid=18528,Background=1032,Density=2,Width=65,Dissipation=75}},new StudioPreset{Name="Elétrica branca",Mode=16,Speed=2,Options=new EffectOptions{Foreground=16777215,Grid=3160128,Background=0,Density=2,Width=70,Dissipation=80}},new StudioPreset{Name="Elétrica violeta",Mode=16,Speed=2,Options=new EffectOptions{Foreground=12615935,Grid=3412064,Background=327696,Density=2,Width=65,Dissipation=80}},new StudioPreset{Name="Elétrica verde",Mode=16,Speed=3,Options=new EffectOptions{Foreground=5308304,Grid=409632,Background=1536,Density=3,Width=50,Dissipation=65}},new StudioPreset{Name="Elétrica dourada",Mode=16,Speed=2,Options=new EffectOptions{Foreground=16765040,Grid=6303744,Background=525056,Density=2,Width=70,Dissipation=80}},new StudioPreset{Name="Elétrica suave",Mode=16,Speed=1,Options=new EffectOptions{Foreground=8448255,Grid=1192000,Background=133136,Density=1,Width=90,Dissipation=95}},new StudioPreset{Name="Elétrica vermelha",Mode=16,Speed=3,Options=new EffectOptions{Foreground=0xFC3636,Grid=0x800202,Background=0}},new StudioPreset{Name="Aurora suave",Mode=6,Speed=1,Options=new EffectOptions{Intensity=75}},new StudioPreset{Name="Vela quente",Mode=13,Speed=2,Options=new EffectOptions{Intensity=85}},new StudioPreset{Name="Cyberpunk",Mode=15,Speed=3,Options=new EffectOptions{Foreground=0xFF3EA5,Grid=0x4D56FF,Background=0x080014,CustomPalette=true}},new StudioPreset{Name="Gelo",Mode=7,Speed=2,Options=new EffectOptions{Foreground=0x9EEBFF,Grid=0x2B6CFF,Background=0x00101C,Intensity=80,CustomPalette=true}},new StudioPreset{Name="Oceano",Mode=8,Speed=2,Options=new EffectOptions{Foreground=0x00D9FF,Grid=0x0050A4,Background=0x000814,Intensity=85,CustomPalette=true}},new StudioPreset{Name="Neon",Mode=10,Speed=4,Options=new EffectOptions{Foreground=0xFF32C8,Grid=0x32F0FF,Background=0x080014,CustomPalette=true}},new StudioPreset{Name="Matrix",Mode=5,Speed=3,Options=new EffectOptions{Foreground=0x43FF6B,Grid=0x087A31,Background=0x000C04,Intensity=80,CustomPalette=true}}};}
}
class SetupSnapshot {
 public KeyboardBarSettings Bar;public LightState[] Devices;public EffectOptions[] Options;public MsiZoneSettings MsiZones;public int Mode,Speed;public int Global,Master;public string Profile;
 public string Key(){return new JavaScriptSerializer().Serialize(this);}
}
partial class ThebestRGB {
 Button verifyDevices,presetButton,undoSetup;Label[] connectionLabels=new Label[4];
 NotifyIcon tray;ContextMenuStrip trayMenu;ToolStripMenuItem closeToTrayItem,profileMenu;bool exitRequested,closeToTray,editorOpen;
 Stack<SetupSnapshot> undoStates=new Stack<SetupSnapshot>();SetupSnapshot lastSetup;bool restoringSetup;DateTime lastEdit=DateTime.MinValue;
 Func<int,string> probeOverride=null;bool applySucceeded;string applyError="";TaskCompletionSource<bool> effectStarted;
 string PreferencesFile {get{return Path.Combine(Path.GetDirectoryName(ProfileStore.FileName),"studio-settings.txt");}}
 internal float LayoutScaleOverride=0;int U(int n){return (int)Math.Round(n*(LayoutScaleOverride>0?LayoutScaleOverride:CurrentAutoScaleDimensions.Width/96f));}
 void InitializeFeatures(){
  verifyDevices=Button("Verificar dispositivos",surface);verifyDevices.Parent=main;verifyDevices.Click+=async delegate{await VerifyDevices();};
  presetButton=Button("Predefinições",surface);presetButton.Parent=main;presetButton.Click+=delegate{if(busy)return;var menu=new ContextMenuStrip{BackColor=surface,ForeColor=Color.White,ShowImageMargin=false,Renderer=new StudioMenuRenderer()};for(int mode=1;mode<EffectLibrary.Names.Length;mode++){if(!EffectLibrary.IsMainEffect(mode))continue;var presets=EffectPresets.ForMode(mode).ToArray();if(presets.Length==0)continue;var group=new ToolStripMenuItem(EffectLibrary.Names[mode]);group.DropDown.BackColor=surface;group.DropDown.ForeColor=Color.White;group.DropDown.Renderer=new StudioMenuRenderer();foreach(var p in presets){var chosen=p;group.DropDownItems.Add(p.Name,null,delegate{UsePreset(chosen);});}menu.Items.Add(group);}menu.Closed+=delegate{BeginInvoke(new Action(()=>menu.Dispose()));};menu.Show(presetButton,new Point(0,presetButton.Height));};
  undoSetup=Button("Desfazer",surface);undoSetup.Parent=main;undoSetup.Click+=delegate{UndoSetup();};
  for(int i=0;i<4;i++){var c=cards[i];connectionLabels[i]=new Label{Text="Não verificado",ForeColor=Muted,AutoSize=false,AutoEllipsis=false,Font=new Font("Segoe UI",8),TextAlign=ContentAlignment.MiddleLeft};c.Controls.Add(connectionLabels[i]);c.Slider.ValueChanged+=delegate{TrackSetup();};c.Included.CheckedChanged+=delegate{TrackSetup();};c.Pick.Click+=delegate{TrackSetup();};c.Test.Click+=async delegate{await TestDevice(c);};}
  master.ValueChanged+=delegate{TrackSetup();};effectMode.SelectedIndexChanged+=delegate{TrackSetup();};effectSpeed.ValueChanged+=delegate{TrackSetup();};
  try{closeToTray=File.Exists(PreferencesFile)&&File.ReadAllText(PreferencesFile)=="1";}catch{closeToTray=false;}
  trayMenu=new ContextMenuStrip{Renderer=new StudioMenuRenderer()};trayMenu.Items.Add("Abrir ThebestRGB",null,delegate{RestoreWindow();});trayMenu.Items.Add("Parar efeito",null,async delegate{if(!busy){SetBusy(true);try{await StopEffect();status.Text="Efeito parado.";}finally{SetBusy(false);}}});
  profileMenu=new ToolStripMenuItem("Aplicar perfil");trayMenu.Items.Add(profileMenu);trayMenu.Opening+=delegate{profileMenu.DropDownItems.Clear();for(int i=0;i<profiles.Count;i++){int index=i;profileMenu.DropDownItems.Add(profiles[i].Name,null,async delegate{if(busy||editorOpen)return;profileList.SelectedIndex=index;await LoadProfileAsync();await ApplyCards(cards.Where(c=>c.State.Included).ToList());});}profileMenu.Enabled=profiles.Count>0&&!busy&&!editorOpen;};
  closeToTrayItem=new ToolStripMenuItem("Botão X minimiza para a bandeja"){Checked=closeToTray,CheckOnClick=true};trayMenu.Items.Add(closeToTrayItem);closeToTrayItem.CheckedChanged+=delegate{closeToTray=closeToTrayItem.Checked;try{Directory.CreateDirectory(Path.GetDirectoryName(PreferencesFile));File.WriteAllText(PreferencesFile,closeToTray?"1":"0");LocalBackup.BackupFiles();}catch(Exception ex){status.Text="Não foi possível salvar a preferência: "+ex.Message;}};
  trayMenu.Items.Add("Sair",null,delegate{if(editorOpen){RestoreWindow();return;}exitRequested=true;Close();if(!IsDisposed)exitRequested=false;});
  tray=new NotifyIcon{Icon=LoadAppIcon(),Text="ThebestRGB",ContextMenuStrip=trayMenu,Visible=true};tray.DoubleClick+=delegate{RestoreWindow();};Resize+=delegate{if(WindowState==FormWindowState.Minimized&&!editorOpen)Hide();};FormClosed+=delegate{tray.Visible=false;tray.Dispose();trayMenu.Dispose();};
  Shown+=delegate{var area=Screen.FromControl(this).WorkingArea;MinimumSize=new Size(Math.Min(MinimumSize.Width,area.Width),Math.Min(MinimumSize.Height,area.Height));if(Width>area.Width||Height>area.Height)Bounds=area;};lastSetup=CaptureSetup();undoSetup.Enabled=false;
 }
 void RestoreWindow(){Show();WindowState=FormWindowState.Normal;Activate();}
 SetupSnapshot CaptureSetup(){return new SetupSnapshot{Bar=pendingBar.Copy(),Devices=cards.Select(c=>c.State.Copy()).ToArray(),Options=effectOptions.Select(o=>o.Copy()).ToArray(),MsiZones=msiZones==null?new MsiZoneSettings():msiZones.Copy(),Mode=effectMode.SelectedIndex,Speed=effectSpeed.Value,Global=global.ToArgb(),Master=master.Value,Profile=profileName.Text};}
void TrackSetup(){if(restoringSetup||undoSetup==null)return;if(colorRamp!=null&&!choosingTone&&colorRamp.Tone.ToArgb()!=global.ToArgb())colorRamp.ResetTone(global);var next=CaptureSetup();if(lastSetup!=null&&next.Key()!=lastSetup.Key()){if((DateTime.UtcNow-lastEdit).TotalMilliseconds>450)undoStates.Push(lastSetup);lastEdit=DateTime.UtcNow;}lastSetup=next;undoSetup.Enabled=undoStates.Count>0&&!busy;LocalBackup.BackupSetup(next);}
 void UndoSetup(){if(busy||undoStates.Count==0)return;var s=undoStates.Pop();pendingBar=s.Bar==null?new KeyboardBarSettings():s.Bar.Copy();msiZones=s.MsiZones==null?new MsiZoneSettings():s.MsiZones.Copy();restoringSetup=true;try{master.Value=s.Master;global=Color.FromArgb(s.Global);pick.BackColor=global;hex.Text="#"+(s.Global&0xffffff).ToString("X6");effectOptions=s.Options.Select(o=>o.Copy()).ToArray();effectMode.SelectedIndex=s.Mode;effectSpeed.Value=s.Speed;profileName.Text=s.Profile;for(int i=0;i<4;i++){cards[i].State=s.Devices[i].Copy();cards[i].State.Name=HardwareNames[i];cards[i].RefreshState(true);cards[i].Status.Text="Não aplicado";}}finally{restoringSetup=false;lastSetup=CaptureSetup();lastEdit=DateTime.MinValue;undoSetup.Enabled=undoStates.Count>0;}status.Text="Configuração anterior recuperada na prévia. Aplique para enviar aos LEDs.";}
 void UsePreset(StudioPreset p){if(busy)return;restoringSetup=true;try{effectOptions[p.Mode]=p.Options.Copy();effectMode.SelectedIndex=p.Mode;effectSpeed.Value=p.Speed;}finally{restoringSetup=false;}lastEdit=DateTime.MinValue;TrackSetup();status.Text=p.Name+" preparada. Configure para visualizar ou aplique aos selecionados.";QueueAutoApply();}
 async Task VerifyDevices(){if(busy)return;if(!effectTask.IsCompleted){status.Text="Pare o efeito antes de verificar os dispositivos.";return;}SetBusy(true);try{for(int i=0;i<4;i++){int index=i;connectionLabels[i].Text="Verificando…";try{string info=await Task.Run(()=>probeOverride!=null?probeOverride(index):ProbeDevice(index));connectionLabels[i].Text="Conectado";connectionLabels[i].ForeColor=Color.LightGreen;help.SetToolTip(connectionLabels[i],info);}catch(Exception ex){connectionLabels[i].Text=index==3?"Verificar acesso":"Indisponível";connectionLabels[i].ForeColor=Color.Salmon;help.SetToolTip(connectionLabels[i],ex.Message);}}status.Text="Verificação concluída sem alterar cores. Passe o mouse sobre o estado para detalhes.";}finally{SetBusy(false);}}
 static string ProbeDevice(int index){switch(index){case 0:using(var stream=new Hid.KeyboardStream())return "Interface do teclado aberta, sem envio de cores.";case 1:return Hid.ProbeMsi();case 2:return VisionGpu.Probe();case 3:return RamBridge.Probe();default:throw new ArgumentOutOfRangeException();}}
 async Task<string> ApplyFromEditor(EffectOptions options,int speed){var targets=cards.Where(c=>c.State.Included).ToList();if(targets.Count==0)return "Selecione pelo menos um dispositivo na tela principal.";effectOptions[effectMode.SelectedIndex]=options.Copy();effectSpeed.Value=speed;TrackSetup();await ApplyCards(targets);if(effectMode.SelectedIndex!=0&&effectStarted!=null)await effectStarted.Task;return applySucceeded?null:(string.IsNullOrEmpty(applyError)?"O envio não foi confirmado. Confira os dispositivos e tente novamente.":applyError);}
 async Task TestDevice(LightCard card){if(busy)return;status.Text="Testando "+card.State.Name+"…";await ApplyCards(new List<LightCard>{card});}
 void LayoutFeatures(){if(verifyDevices==null)return;verifyDevices.SetBounds(U(24),U(91),U(169),U(30));presetButton.SetBounds(U(203),U(91),U(140),U(30));undoSetup.SetBounds(U(353),U(91),U(104),U(30));for(int i=0;i<4;i++)connectionLabels[i].SetBounds(U(185),U(8),cards[i].Width-U(198),U(20));}
}















