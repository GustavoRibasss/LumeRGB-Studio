// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

public class KeyboardBarSettings {
 public int Mode=-1,ColorValue=0xffffff,Brightness=4,Speed=2;
 public bool Multicolor=false;
 public bool Valid(){return Mode>=-1&&Mode<=5&&ColorValue>=0&&ColorValue<=0xffffff&&Brightness>=0&&Brightness<=4&&Speed>=0&&Speed<=4;}
 public KeyboardBarSettings Copy(){return (KeyboardBarSettings)MemberwiseClone();}
}
static partial class Hid {
 public static readonly object KeyboardIoLock=new object();public static ColorBalance ActiveBarBalance=new ColorBalance();public static KeyboardBarSettings BarSettings=new KeyboardBarSettings();public static Color LastKeyboardColor=Color.White;public static int LastKeyboardBrightness=20;
 // Official AULA edge protocol: payload indices below include HID report ID 9.
 public static byte[] NativeBarPacket(KeyboardBarSettings settings){
  if(settings==null||!settings.Valid()||settings.Mode<0)throw new ArgumentOutOfRangeException("settings");
  byte[] p=new byte[64];p[0]=9;p[1]=4;p[2]=6;p[4]=1;p[6]=(byte)(settings.Multicolor?4:7);p[7]=(byte)settings.Mode;p[8]=(byte)(settings.Multicolor?7:0);
  Color corrected=ActiveBarBalance.Apply(EffectOptions.ColorOf(settings.ColorValue));p[9]=corrected.R;p[10]=corrected.G;p[11]=corrected.B;p[12]=(byte)settings.Brightness;p[13]=(byte)settings.Speed;
  int sum=0;for(int i=0;i<63;i++)sum+=p[i];p[63]=(byte)(255-(sum&255));return p;
 }
 public static byte[] BarPacket(Color color,int brightness){var settings=BarSettings;return settings.Mode<0?FollowBarPacket(color,brightness):NativeBarPacket(settings);}
}
partial class ThebestRGB {
 Action<KeyboardBarSettings,Color,int> barWriter=null;
 Button keyboardBarButton;
 string BarSettingsFile {get{return Path.Combine(Path.GetDirectoryName(ProfileStore.FileName),"keyboard-bar.json");}}
 void InitializeKeyboardBar(){
  try{if(File.Exists(BarSettingsFile)){var value=new JavaScriptSerializer().Deserialize<KeyboardBarSettings>(File.ReadAllText(BarSettingsFile));if(value!=null&&value.Valid())pendingBar=value;}}catch{}
  keyboardBarButton=Button("Barra LED",surface);cards[0].Controls.Add(keyboardBarButton);keyboardBarButton.SetBounds(U(105),U(5),U(78),U(27));
  cards[0].Resize+=delegate{keyboardBarButton.SetBounds(U(105),U(5),U(78),U(27));};
  keyboardBarButton.Click+=delegate{if(busy)return;using(var dialog=new KeyboardBarForm(pendingBar,ApplyKeyboardBar,delegate(KeyboardBarSettings value){pendingBar=value.Copy();TrackSetup();}))dialog.ShowDialog(this);};
 }
 async Task<string> ApplyKeyboardBar(KeyboardBarSettings settings){
  if(busy)return "Aguarde o envio atual terminar.";
  SetBusy(true);var previous=Hid.BarSettings;try{
   await Task.Run(()=>{lock(Hid.KeyboardIoLock){var oldBalance=Hid.ActiveBarBalance;Hid.ActiveBarBalance=Calibration.Values[0].Copy();Hid.BarSettings=settings.Copy();try{if(barWriter!=null)barWriter(settings,Hid.LastKeyboardColor,Hid.LastKeyboardBrightness);else Hid.ApplyBar(Hid.LastKeyboardColor,Hid.LastKeyboardBrightness);}catch{Hid.BarSettings=previous;Hid.ActiveBarBalance=oldBalance;throw;}}});pendingBar=settings.Copy();if(appliedDevices[0]!=null)appliedDevices[0].Bar=settings.Copy();
   try{Directory.CreateDirectory(Path.GetDirectoryName(BarSettingsFile));File.WriteAllText(BarSettingsFile,new JavaScriptSerializer().Serialize(settings));LocalBackup.BackupFiles();}catch(Exception ex){status.Text="Barra aplicada, mas não foi possível salvar: "+ex.Message;return status.Text;}
   SaveLastConfiguration();status.Text="Configuração enviada à barra LED do teclado.";return null;
  }catch(Exception ex){Hid.BarSettings=previous;return "Não foi possível aplicar à barra: "+ex.Message;}finally{SetBusy(false);}
 }
}
class KeyboardBarForm:Form {
 public static readonly int[] ModeIds={-1,0,3,1,4,2,5};
 public static readonly string[] ModeNames={"Acompanhar as teclas","Desligada","Luz constante","Fluxo","Respiração","Neon","Luz correndo"};
 public KeyboardBarForm(KeyboardBarSettings original,Func<KeyboardBarSettings,Task<string>> apply,Action<KeyboardBarSettings> prepare=null){
  var settings=original.Copy();Text="ThebestRGB / Barra LED do teclado";ClientSize=new Size(480,514);MinimumSize=Size;MaximumSize=Size;StartPosition=FormStartPosition.CenterParent;Font=new Font("Segoe UI",10);BackColor=Color.FromArgb(20,21,27);ForeColor=Color.White;AutoScaleMode=AutoScaleMode.Dpi;MaximizeBox=false;MinimizeBox=false;
  Controls.Add(new Label{Text="Barra LED do teclado",Font=new Font("Segoe UI",20,FontStyle.Bold),AutoSize=true,Location=new Point(22,18)});
  Controls.Add(new Label{Text="Modo da barra",AutoSize=true,Location=new Point(24,78)});
  var mode=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,BackColor=Color.FromArgb(35,36,45),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};mode.Items.AddRange(ModeNames);mode.SetBounds(24,104,432,32);mode.SelectedIndex=Array.IndexOf(ModeIds,settings.Mode);Controls.Add(mode);
  mode.Visible=false;var modeButton=ThebestRGB.Button(ModeNames[mode.SelectedIndex]+"   ▾",Color.FromArgb(35,36,45));modeButton.TextAlign=ContentAlignment.MiddleLeft;modeButton.Padding=new Padding(12,0,0,0);modeButton.SetBounds(24,104,432,32);Controls.Add(modeButton);var menu=new ContextMenuStrip{ShowImageMargin=false,BackColor=BackColor,ForeColor=Color.White,Renderer=new StudioMenuRenderer()};for(int i=0;i<ModeNames.Length;i++){int index=i;menu.Items.Add(ModeNames[i],null,delegate{mode.SelectedIndex=index;modeButton.Text=ModeNames[index]+"   ▾";});}modeButton.Click+=delegate{menu.Show(modeButton,new Point(0,modeButton.Height));};FormClosed+=delegate{menu.Dispose();};
  var multi=new StudioCheckBox{Text="Multicolorido",Checked=settings.Multicolor,ForeColor=Color.White};multi.SetBounds(24,151,180,30);Controls.Add(multi);
  var color=ThebestRGB.Button("Escolher cor",Color.FromArgb(settings.ColorValue|unchecked((int)0xff000000)));color.SetBounds(260,149,196,32);color.ForeColor=color.BackColor.GetBrightness()>.6?Color.Black:Color.White;Controls.Add(color);
  var brightness=new StudioSlider{Minimum=0,Maximum=4,Value=settings.Brightness,BackColor=BackColor};brightness.SetBounds(20,222,315,28);Controls.Add(brightness);
  var brightText=new Label{Text="Brilho: "+(settings.Brightness+1)+" / 5",AutoSize=true,Location=new Point(24,198)};Controls.Add(brightText);
  var speed=new StudioSlider{Minimum=0,Maximum=4,Value=settings.Speed,BackColor=BackColor};speed.SetBounds(20,282,315,28);Controls.Add(speed);
  var speedText=new Label{Text="Velocidade: "+(settings.Speed+1)+" / 5",AutoSize=true,Location=new Point(24,258)};Controls.Add(speedText);
  var result=new Label{ForeColor=Color.FromArgb(175,178,193)};result.SetBounds(24,320,432,48);Controls.Add(result);
  var save=ThebestRGB.Button("Aplicar à barra",Color.FromArgb(151,119,246));save.SetBounds(276,377,180,34);Controls.Add(save);
  var cancel=ThebestRGB.Button("Fechar",Color.FromArgb(35,36,45));cancel.SetBounds(24,377,110,34);cancel.DialogResult=DialogResult.Cancel;Controls.Add(cancel);CancelButton=cancel;
  var preview=new BarPreviewControl{Settings=settings,Follow=Hid.LastKeyboardColor};preview.SetBounds(24,420,432,62);Controls.Add(preview);Controls.Add(new Label{Text="Prévia ilustrativa",AutoSize=true,ForeColor=Color.Silver,Location=new Point(24,489)});var timer=new System.Windows.Forms.Timer{Interval=40};timer.Tick+=delegate{preview.Invalidate();};timer.Start();FormClosed+=delegate{timer.Dispose();if(prepare!=null)prepare(settings.Copy());};Action update=delegate{preview.Settings=settings;bool active=settings.Mode>0;multi.Enabled=active;color.Enabled=active&&!multi.Checked;brightness.Enabled=active;speed.Enabled=active&&settings.Mode!=3;};
  mode.SelectedIndexChanged+=delegate{settings.Mode=ModeIds[mode.SelectedIndex];update();};multi.CheckedChanged+=delegate{settings.Multicolor=multi.Checked;update();};
  color.Click+=delegate{using(var picker=new SavedColorDialog{Color=color.BackColor,FullOpen=true})if(picker.ShowDialog(this)==DialogResult.OK){settings.ColorValue=picker.Color.ToArgb()&0xffffff;color.BackColor=picker.Color;color.ForeColor=picker.Color.GetBrightness()>.6?Color.Black:Color.White;}};
  brightness.ValueChanged+=delegate{settings.Brightness=brightness.Value;brightText.Text="Brilho: "+(brightness.Value+1)+" / 5";};speed.ValueChanged+=delegate{settings.Speed=speed.Value;speedText.Text="Velocidade: "+(speed.Value+1)+" / 5";};
  bool sending=false;FormClosing+=delegate(object sender,FormClosingEventArgs e){if(sending)e.Cancel=true;};
  save.Click+=async delegate{sending=true;foreach(Control c in Controls)c.Enabled=false;try{result.Text="Enviando à barra LED…";string error=await apply(settings.Copy());result.Text=error??"Configuração enviada à barra LED.";}finally{sending=false;foreach(Control c in Controls)c.Enabled=true;update();}};update();DarkDialogChrome.Attach(this,Text);
 }
}
