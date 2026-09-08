// SPDX-License-Identifier: GPL-2.0-or-later
// ThebestRGB Studio. Hardware adapters: LumeRGB.cs, VisionGpu.cs, RamBridge.cs.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

public class LightState {
 public string Name {get;set;} public int R {get;set;} public int G {get;set;} public int B {get;set;}
 public int Brightness {get;set;} public bool Included {get;set;}
 public Color Color {get{return Color.FromArgb(R,G,B);}}
 public LightState Copy(){return new LightState{Name=Name,R=R,G=G,B=B,Brightness=Brightness,Included=Included};}
}
public class MsiZoneSettings {
 public bool Split,Custom;public int Fans=0xffffff,WaterCooler=0xffffff,Auxiliary=0xffffff,FansChannel,WaterChannel=1;
 public MsiZoneSettings Copy(){return new MsiZoneSettings();}
 public bool Valid(){return Fans>=0&&Fans<=0xffffff&&WaterCooler>=0&&WaterCooler<=0xffffff&&Auxiliary>=0&&Auxiliary<=0xffffff&&FansChannel>=0&&FansChannel<3&&WaterChannel>=0&&WaterChannel<3&&FansChannel!=WaterChannel;}
 public void SetAll(Color c){int rgb=c.R<<16|c.G<<8|c.B;Fans=WaterCooler=Auxiliary=rgb;Custom=true;}
 static Color FromRgb(int rgb){return Color.FromArgb((rgb>>16)&255,(rgb>>8)&255,rgb&255);}
 public Color[] Resolve(Color fallback){return new[]{fallback,fallback,fallback};}
}
public class LightProfile {public string Name {get;set;} public KeyboardBarSettings Bar {get;set;} public string Description {get;set;} public bool Favorite {get;set;} public List<LightState> Devices {get;set;} public int Mode {get;set;} public int Speed {get;set;} public EffectOptions[] Options {get;set;} public MsiZoneSettings MsiZones {get;set;} public LightProfile(){Bar=new KeyboardBarSettings();Description="";Speed=3;Options=EffectOptions.Defaults();MsiZones=new MsiZoneSettings();}}
static class ProfileStore {
 public static string FileName=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"LumeRGB","profiles.json");
 public static bool Valid(LightProfile p){return p!=null&&p.Bar!=null&&p.Bar.Valid()&&p.MsiZones!=null&&p.MsiZones.Valid()&&!string.IsNullOrWhiteSpace(p.Name)&&p.Name.Length<=40&&p.Mode>=0&&p.Mode<EffectLibrary.Names.Length&&p.Speed>=1&&p.Speed<=5&&EffectOptions.ValidAll(p.Options)&&p.Devices!=null&&p.Devices.Count==4&&p.Devices.All(x=>x!=null&&x.R>=0&&x.R<=255&&x.G>=0&&x.G<=255&&x.B>=0&&x.B<=255&&x.Brightness>=0&&x.Brightness<=100);}
 public static List<LightProfile> Read(){if(!File.Exists(FileName))return new List<LightProfile>();var all=new JavaScriptSerializer().Deserialize<List<LightProfile>>(File.ReadAllText(FileName));Upgrade(all);if(all==null||all.Count>100||all.Any(x=>!Valid(x)))throw new IOException("Arquivo de perfis inválido. Os perfis existentes foram preservados.");return all;}
 public static void Upgrade(List<LightProfile> all){if(all==null)return;foreach(var p in all){if(p==null)continue;if(p.Bar==null)p.Bar=new KeyboardBarSettings();if(p.MsiZones==null||!p.MsiZones.Valid())p.MsiZones=new MsiZoneSettings();if(p.Options!=null&&p.Options.Length==17&&p.Options.All(EffectOptions.Valid))p.Options=p.Options.Concat(new[]{new EffectOptions()}).ToArray();}} public static void Write(List<LightProfile> all){if(all.Count>100||all.Any(x=>!Valid(x)))throw new IOException("Perfil inválido.");Directory.CreateDirectory(Path.GetDirectoryName(FileName));string tmp=FileName+".tmp";File.WriteAllText(tmp,new JavaScriptSerializer().Serialize(all));if(File.Exists(FileName))File.Replace(tmp,FileName,null);else File.Move(tmp,FileName);}
}
class DeviceArt:Control {
 public KeyboardBarSettings BarPreview;public Color Light=Color.MediumPurple;public int Level=100;public int Kind;
 public DeviceArt(int kind){Kind=kind;DoubleBuffered=true;BackColor=Color.FromArgb(17,21,31);Size=new Size(340,90);}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;float artScale=Math.Max(.1f,Math.Min(g.DpiX/96f,Height/90f));g.ScaleTransform(artScale,artScale);Color c=Color.FromArgb(Light.R*Level/100,Light.G*Level/100,Light.B*Level/100);float cx=Width/(2f*artScale);using(var pen=new Pen(Color.FromArgb(66,74,93),1.5f))using(var glow=new Pen(Color.FromArgb(45,c),9))using(var lit=new Pen(c,3))using(var fill=new SolidBrush(c)) {
  if(Kind==0){g.DrawRectangle(pen,cx-116,20,232,56);g.DrawLine(glow,cx-110,13,cx+110,13);g.DrawLine(lit,cx-110,13,cx+110,13);if(BarPreview!=null)BarPreviewRenderer.Paint(g,new RectangleF(cx-110,11,220,4),BarPreview,c,(Environment.TickCount&int.MaxValue)/1000.0);for(int y=0;y<4;y++)for(int x=0;x<15;x++)g.FillRectangle(fill,cx-107+x*14,27+y*11,x==14?14:10,6);}
  if(Kind==1){g.DrawRectangle(pen,cx-60,9,120,72);g.DrawLine(pen,cx+37,9,cx+37,81);for(int y=0;y<3;y++){g.DrawEllipse(glow,cx-35,15+y*21,16,16);g.DrawEllipse(lit,cx-35,15+y*21,16,16);}g.DrawRectangle(lit,cx-5,24,30,4);g.DrawEllipse(glow,cx-4,42,25,25);g.DrawEllipse(lit,cx-4,42,25,25);g.DrawLine(pen,cx+21,50,cx+29,42);g.DrawLine(pen,cx+29,42,cx+29,28);g.DrawLine(pen,cx+18,64,cx+33,64);g.DrawLine(pen,cx+33,64,cx+33,28);}
  if(Kind==2){g.DrawRectangle(pen,cx-112,20,224,52);g.DrawLine(lit,cx-107,25,cx+107,25);for(int x=0;x<3;x++){g.DrawEllipse(pen,cx-92+x*62,31,33,33);g.DrawLine(pen,cx-75+x*62,36,cx-75+x*62,59);}using(var f=new Font("Segoe UI",8,FontStyle.Bold))g.DrawString("VISION",f,fill,cx+78,47);}
  if(Kind==3){for(int y=0;y<2;y++){g.DrawRectangle(pen,cx-105,18+y*36,210,24);g.DrawLine(glow,cx-102,18+y*36,cx+102,18+y*36);g.DrawLine(lit,cx-102,18+y*36,cx+102,18+y*36);using(var f=new Font("Segoe UI",8,FontStyle.Bold))g.DrawString("VIPER",f,Brushes.Silver,cx-91,24+y*36);}}
 }}
}
class LightCard:Panel {
 public bool Pending;protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);if(Pending)using(var b=new SolidBrush(Color.FromArgb(255,190,90)))e.Graphics.FillEllipse(b,Width-9,Height-9,5,5);} public LightState State;public CheckBox Included;public StudioSlider Slider;public Label Percent,Status;public Button Pick,Apply,Test;public DeviceArt Art;
 public event Action<LightCard> ApplyRequested;
 public LightCard(int kind,LightState state){DoubleBuffered=true;State=state;BackColor=Color.FromArgb(23,28,40);Padding=new Padding(16);Size=new Size(395,244);
  Included=new StudioCheckBox{Text=state.Name,Checked=state.Included,AutoSize=true,ForeColor=Color.White,Location=new Point(16,13),Font=new Font("Segoe UI",11,FontStyle.Bold)};Controls.Add(Included);
  var subtitle=new Label{Text=new[]{"AULA HERO 68 · teclas + barra LED","MSI PROJECT ZERO · 3 conectores ARGB","GIGABYTE VISION · logo RGB","VIPER ELITE 5 · 2 módulos ENE"}[kind],ForeColor=LumeStudio.Muted,AutoSize=true,Location=new Point(17,42),Font=new Font("Segoe UI",8)};Controls.Add(subtitle);
  Art=new DeviceArt(kind){Location=new Point(16,65)};Controls.Add(Art);
  Slider=new StudioSlider{Minimum=0,Maximum=100,Value=100,TickStyle=TickStyle.None,BackColor=BackColor,Location=new Point(9,160),Size=new Size(250,30)};Controls.Add(Slider);
  Percent=new Label{Text="100%",ForeColor=Color.White,AutoSize=true,Location=new Point(274,163)};Controls.Add(Percent);
  Pick=LumeStudio.Button("Cor",Color.FromArgb(44,51,69));Pick.SetBounds(16,199,75,30);Controls.Add(Pick);
  Apply=LumeStudio.Button("Aplicar",Color.FromArgb(44,51,69));Apply.SetBounds(100,199,92,30);Controls.Add(Apply);
  Test=LumeStudio.Button("Testar",Color.FromArgb(44,51,69));Test.SetBounds(196,199,74,30);Controls.Add(Test);
  Status=new Label{Text="Pronto",ForeColor=LumeStudio.Muted,AutoSize=true,Location=new Point(209,207),Font=new Font("Segoe UI",8)};Controls.Add(Status);
  Included.CheckedChanged+=delegate{State.Included=Included.Checked;};
  Slider.ValueChanged+=delegate{State.Brightness=Slider.Value;RefreshState(false);Status.Text="Não aplicado";};
  Pick.Click+=delegate{using(var d=new ColorDialog{Color=State.Color,FullOpen=true})if(d.ShowDialog()==DialogResult.OK){Color c=RgbApp.FullIntensity(d.Color);State.R=c.R;State.G=c.G;State.B=c.B;RefreshState(false);Status.Text="Não aplicado";}};
  Apply.Click+=delegate{if(ApplyRequested!=null)ApplyRequested(this);};
  Resize+=delegate{Art.Width=Width-32;Slider.Width=Width-90;Percent.Left=Width-64;Status.Left=Width-132;};RefreshState(true);
 }
 public void RefreshState(bool inputs){if(inputs){Included.Checked=State.Included;Slider.Value=State.Brightness;}Percent.Text=State.Brightness+"%";Pick.BackColor=State.Color;Pick.ForeColor=State.Color.GetBrightness()>.6?Color.Black:Color.White;Art.Light=State.Color;Art.Level=State.Brightness;Art.Invalidate();}
}
partial class LumeStudio:Form {
 public static Color Muted=Color.FromArgb(144,155,177);
 Color accent=Color.FromArgb(137,104,255),global=Color.White;
 Panel side,main,hero;Label status,masterValue;Button all,pick,activate,save,load,remove;TextBox hex,profileName,profileDescription;StudioSlider master;ListBox profileList;
 List<LightCard> cards=new List<LightCard>();List<LightProfile> profiles=new List<LightProfile>();bool busy,profilesWritable=true;MsiZoneSettings msiZones=new MsiZoneSettings();
 public static Button Button(string text,Color bg){return new StudioButton{Text=text,BackColor=bg,ForeColor=Color.White,FlatStyle=FlatStyle.Flat,Cursor=Cursors.Hand,Font=new Font("Segoe UI",9),UseVisualStyleBackColor=false,FlatAppearance={BorderSize=0}};}
 static Icon LoadAppIcon(){try{string path=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"LumeStudio.ico");if(File.Exists(path))return new Icon(path);}catch{}return SystemIcons.Application;}
 Label Label(string text,int size,Color color,int x,int y){var l=new Label{Text=text,Font=new Font("Segoe UI",size,size>=18?FontStyle.Bold:FontStyle.Regular),ForeColor=color,AutoSize=true,Location=new Point(x,y)};return l;}
 public LumeStudio(){
  Text="ThebestRGB Studio · 21";Icon=LoadAppIcon();BackColor=Color.FromArgb(12,16,24);ForeColor=Color.White;Font=new Font("Segoe UI",9);ClientSize=new Size(1140,Math.Min(918,Screen.PrimaryScreen.WorkingArea.Height-65));MinimumSize=new Size(1050,720);StartPosition=FormStartPosition.CenterScreen;AutoScaleDimensions=new SizeF(96,96);AutoScaleMode=AutoScaleMode.Dpi;
  side=new Panel{Dock=DockStyle.Left,Width=208,BackColor=Color.FromArgb(17,21,31)};Controls.Add(side);
  main=new Panel{Dock=DockStyle.Fill,AutoScroll=false,Padding=new Padding(26)};Controls.Add(main);main.BringToFront();
  side.Controls.Add(Label("LUME",25,Color.White,23,28));side.Controls.Add(Label("RGB STUDIO",9,Muted,27,72));
  var selected=Button("●   Iluminação",Color.FromArgb(42,34,68));selected.SetBounds(16,122,176,42);side.Controls.Add(selected);selected.Click+=delegate{main.AutoScrollPosition=Point.Empty;};
  side.Controls.Add(Label("PERFIS SALVOS",9,Muted,24,197));
  profileList=new ListBox{Location=new Point(20,226),Size=new Size(168,184),BackColor=side.BackColor,ForeColor=Color.White,BorderStyle=BorderStyle.None,Font=new Font("Segoe UI",10),IntegralHeight=false};side.Controls.Add(profileList);
  profileName=new TextBox{Location=new Point(20,427),Width=168,MaxLength=40,Text="Meu setup",BackColor=Color.FromArgb(29,35,48),ForeColor=Color.White,BorderStyle=BorderStyle.FixedSingle};side.Controls.Add(profileName);
  save=Button("Salvar perfil",Color.FromArgb(42,48,64));save.SetBounds(20,463,168,34);side.Controls.Add(save);save.Click+=delegate{SaveProfile();};
  load=Button("Carregar",Color.FromArgb(42,48,64));load.SetBounds(20,507,80,32);side.Controls.Add(load);load.Click+=delegate{LoadProfile();};
  remove=Button("Excluir",Color.FromArgb(42,48,64));remove.SetBounds(108,507,80,32);side.Controls.Add(remove);remove.Click+=delegate{DeleteProfile();};
  var note=new Label{Text="Carregar um perfil prepara\nas cores. Clique em Aplicar\npara acender seu setup.",ForeColor=Muted,Location=new Point(22,560),Size=new Size(172,62),Font=new Font("Segoe UI",8)};side.Controls.Add(note);
  side.Controls.Add(Label("LOCAL · SEM CONTA",8,Muted,23,688));
  main.Controls.Add(Label("Seu setup, em sintonia.",23,Color.White,27,24));main.Controls.Add(Label("Uma cor para tudo. Ou uma identidade para cada dispositivo.",10,Muted,29,67));
  hero=new Panel{Location=new Point(28,105),Size=new Size(870,122),BackColor=Color.FromArgb(23,28,40)};main.Controls.Add(hero);
  hero.Controls.Add(Label("COR DO SETUP",8,Muted,18,14));
  pick=Button("Escolher cor",global);pick.ForeColor=Color.Black;pick.SetBounds(18,41,123,32);hero.Controls.Add(pick);pick.Click+=delegate{using(var d=new ColorDialog{Color=global,FullOpen=true})if(d.ShowDialog()==DialogResult.OK)SetGlobal(RgbApp.FullIntensity(d.Color));};
  hex=new TextBox{Text="#FFFFFF",Location=new Point(153,47),Width=84,MaxLength=7,BackColor=hero.BackColor,ForeColor=Color.White,BorderStyle=BorderStyle.None};hero.Controls.Add(hex);hex.KeyDown+=delegate(object s,KeyEventArgs e){if(e.KeyCode==Keys.Enter){ApplyHex();e.SuppressKeyPress=true;}};hex.Leave+=delegate{ApplyHex();};
  Color[] palette={Color.White,Color.FromArgb(0,224,255),Color.FromArgb(160,96,255),Color.FromArgb(255,60,136),Color.FromArgb(255,160,32)};
  for(int i=0;i<palette.Length;i++){Color c=palette[i];var b=Button("",c);b.SetBounds(19+i*31,87,22,15);hero.Controls.Add(b);b.Click+=delegate{SetGlobal(c);};new ToolTip().SetToolTip(b,"#"+c.R.ToString("X2")+c.G.ToString("X2")+c.B.ToString("X2"));}
  hero.Controls.Add(Label("BRILHO DOS SELECIONADOS",8,Muted,266,14));master=new StudioSlider{Minimum=0,Maximum=100,Value=100,TickStyle=TickStyle.None,BackColor=hero.BackColor,Location=new Point(256,42),Size=new Size(230,32)};hero.Controls.Add(master);
  masterValue=Label("100%",10,Color.White,489,48);hero.Controls.Add(masterValue);master.ValueChanged+=delegate{masterValue.Text=master.Value+"%";foreach(var c in cards.Where(c=>c.State.Included)){c.State.Brightness=master.Value;c.RefreshState(true);c.Status.Text="Não aplicado";}};
  all=Button("Aplicar selecionados",accent);all.SetBounds(603,38,235,40);hero.Controls.Add(all);all.Click+=async delegate{await ApplyCards(cards.Where(c=>c.State.Included).ToList());};
  hero.Controls.Add(Label("Alterações exigem Aplicar.",8,Muted,266,88));
  string[] names={"Teclado","Gabinete","Placa de vídeo","Memórias"};
  for(int i=0;i<4;i++){var state=new LightState{Name=names[i],R=255,G=255,B=255,Brightness=100,Included=true};var card=new LightCard(i,state);cards.Add(card);main.Controls.Add(card);card.ApplyRequested+=async c=>await ApplyCards(new List<LightCard>{c});}
  activate=Button("Ativar acesso à RAM",Color.FromArgb(42,48,64));main.Controls.Add(activate);activate.Click+=async delegate{if(busy)return;SetBusy(true);status.Text="Aguardando acesso à RAM e autorização do Windows...";try{await Task.Run(()=>RamBridge.Start());cards[3].Status.Text="Disponível";status.Text="Dois módulos disponíveis. Escolha um perfil e aplique.";}catch(Exception ex){status.Text=ex.Message;cards[3].Status.Text="Verificar acesso";}finally{SetBusy(false);}};
  status=new Label{ForeColor=Muted,Font=new Font("Segoe UI",9),Text="Pronto. As cores da prévia ainda não foram aplicadas.",AutoSize=false};main.Controls.Add(status);
  try{profiles=ProfileStore.Read();}catch(Exception ex){profilesWritable=false;status.Text=ex.Message;}
  InitializeEffects();InitializeEditor();InitializeProfessional();InitializeFeatures();InitializeMaxFeatures();InitializeKeyboardBar();InitializeUpgrade();InitializeDesign();RefreshProfiles();Resize+=delegate{LayoutMain();};Shown+=delegate{LayoutMain();};LayoutMain();
 }
 void LayoutMain(){LayoutProfessional();}
 void SetGlobal(Color c){global=RgbApp.FullIntensity(c);pick.BackColor=global;pick.ForeColor=global.GetBrightness()>.6?Color.Black:Color.White;hex.Text="#"+global.R.ToString("X2")+global.G.ToString("X2")+global.B.ToString("X2");foreach(var card in cards.Where(x=>x.State.Included)){card.State.R=global.R;card.State.G=global.G;card.State.B=global.B;card.RefreshState(false);card.Status.Text="Não aplicado";}status.Text="Cor preparada para os dispositivos selecionados.";TrackSetup();}
 void ApplyHex(){string value=hex.Text.Trim();if(!value.StartsWith("#"))value="#"+value;int n;if(value.Length!=7||!int.TryParse(value.Substring(1),System.Globalization.NumberStyles.HexNumber,null,out n)){status.Text="Use uma cor hexadecimal como #A060FF.";return;}Color c=Color.FromArgb((n>>16)&255,(n>>8)&255,n&255);if(c.ToArgb()!=global.ToArgb())SetGlobal(c);}
 void SetBusy(bool value){busy=value;if(configure!=null)configure.Enabled=!value;if(effectMode!=null){effectMode.Enabled=effectSpeed.Enabled=!value;effectStop.Enabled=!value&&!effectTask.IsCompleted;}all.Enabled=activate.Enabled=save.Enabled=load.Enabled=remove.Enabled=pick.Enabled=hex.Enabled=master.Enabled=profileList.Enabled=profileName.Enabled=!value;foreach(var c in cards)c.Enabled=!value;Cursor=value?Cursors.WaitCursor:Cursors.Default;UpdateEffectControls();if(verifyDevices!=null){verifyDevices.Enabled=presetButton.Enabled=!value;undoSetup.Enabled=!value&&undoStates.Count>0;}UpdateMaxState();}
 async Task ApplyStaticCards(List<LightCard> targets){if(busy)return;if(targets.Count==0){status.Text="Selecione ao menos um dispositivo.";return;}SetBusy(true);status.Text="Aplicando iluminação...";var states=targets.Select(c=>c.State.Copy()).ToArray();int successes=0;var errors=new List<string>();
  try{for(int i=0;i<targets.Count;i++){LightState s=states[i];int index=cards.IndexOf(targets[i]);targets[i].Status.Text="Enviando...";status.Text="Aplicando "+(i+1)+" de "+targets.Count+": "+s.Name+"…";try{await Task.Run(()=>{if(effectWriter!=null){effectWriter(index,Calibration.Adjust(index,s.Color),s.Brightness);return;}Color from; if(!lastSentColors.TryGetValue(index,out from))from=Color.Black;for(int step=1;step<=4;step++){SendStaticFrame(index,EffectOptions.Mix(from,s.Color,step/4.0),s.Brightness);Thread.Sleep(38);}lastSentColors[index]=s.Color;});targets[i].Status.Text="Aplicado";RecordApplied(index,s,0,effectSpeed.Value,effectOptions[0],pendingBar);successes++;}catch(Exception ex){targets[i].Status.Text="Falhou";errors.Add(s.Name+": "+ex.Message);}}applySucceeded=errors.Count==0;applyError=string.Join(" | ",errors.ToArray());status.Text=errors.Count==0?"Aplicado com sucesso em "+successes+" dispositivo(s). Confira os LEDs.":successes+" aplicado(s); "+string.Join(" | ",errors.ToArray());}
  finally{SetBusy(false);}
 }
 void SendStaticFrame(int index,Color c,int brightness){SendBalancedStaticFrame(index,c,brightness,Calibration.Values[index]);} void SendBalancedStaticFrame(int index,Color c,int brightness,ColorBalance balance){c=balance.Apply(c);int level=(brightness+2)/5;switch(index){case 0:Hid.Apply(c,level);break;case 1:Hid.ApplyMsi(c,(brightness+5)/10);break;case 2:VisionGpu.Apply(c,(brightness*99+50)/100);break;case 3:RamBridge.Apply(c,level);break;}}
  void RefreshProfiles(){profileList.Items.Clear();foreach(var p in profiles)profileList.Items.Add((p.Favorite?"★ ":"")+p.Name);if(profileEmpty!=null){profileEmpty.Visible=profiles.Count==0;profileList.Visible=profiles.Count>0;}if(profileCount!=null)profileCount.Text=profiles.Count==0?"0 salvos":profiles.Count+" salvos";RefreshQuickProfiles();}
 void ToggleFavorite(int index){if(index<0||index>=profiles.Count||!profilesWritable)return;var next=new List<LightProfile>(profiles);next[index].Favorite=!next[index].Favorite;try{ProfileStore.Write(next);profiles=next;RefreshProfiles();profileList.SelectedIndex=index;status.Text=(profiles[index].Favorite?"Favorito marcado: ":"Favorito removido: ")+profiles[index].Name+".";}catch(Exception ex){status.Text="Não foi possível atualizar o favorito: "+ex.Message;}}
 void DuplicateProfile(int index){if(index<0||index>=profiles.Count||!profilesWritable)return;var original=profiles[index];string baseName=original.Name+" cópia";string name=baseName;int suffix=2;while(profiles.Any(p=>string.Equals(p.Name,name,StringComparison.OrdinalIgnoreCase)))name=baseName+" "+suffix++;var copy=new LightProfile{Bar=original.Bar.Copy(),Name=name,Description=original.Description??"",Favorite=false,Mode=original.Mode,Speed=original.Speed,Options=original.Options.Select(o=>o.Copy()).ToArray(),Devices=original.Devices.Select(d=>d.Copy()).ToList(),MsiZones=original.MsiZones==null?new MsiZoneSettings():original.MsiZones.Copy()};var next=new List<LightProfile>(profiles){copy};try{ProfileStore.Write(next);profiles=next;RefreshProfiles();profileList.SelectedIndex=profiles.IndexOf(copy);profileName.Text=copy.Name;if(profileDescription!=null)profileDescription.Text=copy.Description;status.Text="Perfil duplicado: "+copy.Name+".";}catch(Exception ex){status.Text="Não foi possível duplicar: "+ex.Message;}}
 void SaveProfile(){if(!profilesWritable){status.Text="Corrija o arquivo de perfis antes de salvar; nenhum dado foi sobrescrito.";return;}string name=profileName.Text.Trim();if(name.Length==0){status.Text="Dê um nome ao perfil.";return;}var next=new List<LightProfile>(profiles);int i=next.FindIndex(x=>string.Equals(x.Name,name,StringComparison.OrdinalIgnoreCase));var p=new LightProfile{Bar=pendingBar.Copy(),Name=name,Description=profileDescription==null?"":profileDescription.Text.Trim(),Favorite=i>=0&&next[i].Favorite,Mode=effectMode.SelectedIndex,Speed=effectSpeed.Value,Options=effectOptions.Select(x=>x.Copy()).ToArray(),Devices=cards.Select(c=>c.State.Copy()).ToList(),MsiZones=msiZones==null?new MsiZoneSettings():msiZones.Copy()};if(i>=0)next[i]=p;else next.Add(p);try{ProfileStore.Write(next);profiles=next;RefreshProfiles();profileList.SelectedIndex=profiles.IndexOf(p);status.Text="Perfil salvo: "+name+".";}catch(Exception ex){status.Text="Não foi possível salvar: "+ex.Message;}}
 async void LoadProfile(){await LoadProfileAsync();}
 async Task LoadProfileAsync(){if(busy)return;int i=profileList.SelectedIndex;if(i<0){status.Text="Selecione um perfil salvo.";return;}SetBusy(true);await StopEffect();SetBusy(false);var p=profiles[i];pendingBar=p.Bar.Copy();msiZones=p.MsiZones==null?new MsiZoneSettings():p.MsiZones.Copy();effectOptions=p.Options.Select(x=>x.Copy()).ToArray();effectMode.SelectedIndex=p.Mode;effectSpeed.Value=p.Speed;for(int j=0;j<4;j++){cards[j].State=p.Devices[j].Copy();cards[j].State.Name=HardwareNames[j];cards[j].RefreshState(true);cards[j].Status.Text="Não aplicado";}profileName.Text=p.Name;if(profileDescription!=null)profileDescription.Text=p.Description??"";TrackSetup();status.Text="Perfil "+p.Name+" carregado. Clique em Aplicar selecionados.";}
 void DeleteProfile(){if(!profilesWritable)return;int i=profileList.SelectedIndex;if(i<0)return;if(MessageBox.Show(this,"Excluir o perfil “"+profiles[i].Name+"”?", "Excluir perfil",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;var next=new List<LightProfile>(profiles);string name=next[i].Name;next.RemoveAt(i);try{ProfileStore.Write(next);profiles=next;RefreshProfiles();status.Text="Perfil excluído: "+name+".";}catch(Exception ex){status.Text=ex.Message;}}
 [STAThread] public static int Main(string[] args){if(args.Length>0&&args[0]=="--self-test"){var p=new LightProfile{Name="Teste",Devices=Enumerable.Range(0,4).Select(i=>new LightState{Name="D"+i,R=255,G=0,B=128,Brightness=75,Included=i%2==0}).ToList()};var json=new JavaScriptSerializer().Serialize(p);var copy=new JavaScriptSerializer().Deserialize<LightProfile>(json);if(!ProfileStore.Valid(copy)||copy.Devices[2].Brightness!=75||copy.Devices[1].Included)return 1;copy.Devices[0].Brightness=101;if(ProfileStore.Valid(copy))return 2;return 0;}Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);Application.ThreadException+=delegate(object sender,ThreadExceptionEventArgs e){var form=Application.OpenForms.OfType<LumeStudio>().FirstOrDefault();if(form!=null)form.status.Text="Ocorreu um erro inesperado. Tente novamente; nenhum envio adicional foi iniciado.";};if(args.Length>1&&args[0]=="--preview"){using(var form=new LumeStudio()){form.Show();Application.DoEvents();using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(0,0,form.Width,form.Height));bitmap.Save(args[1]);}form.Close();}return 0;}var app=new LumeStudio();if(args.Any(a=>string.Equals(a,"--tray",StringComparison.OrdinalIgnoreCase)))app.Shown+=delegate{app.Hide();};Application.Run(app);return 0;}
}





















