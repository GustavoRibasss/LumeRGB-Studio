// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
class EffectColorControl:Panel {
 TextBox hex;Button pick;Label error;bool updating;
 public Action<int> Changed;
 public bool ValidHex {get{int n;return hex.Text.Length==7&&hex.Text[0]=='#'&&int.TryParse(hex.Text.Substring(1),System.Globalization.NumberStyles.HexNumber,null,out n);}}
 public EffectColorControl(string title,string description,int color){
  Size=new Size(424,100);BackColor=Color.FromArgb(30,32,41);
  Controls.Add(new Label{Text=title,ForeColor=Color.White,Location=new Point(14,10),Size=new Size(390,21),Font=new Font("Segoe UI",10,FontStyle.Bold)});
  Controls.Add(new Label{Text=description,ForeColor=ThebestRGB.Muted,Location=new Point(14,35),Size=new Size(394,20)});
  pick=ThebestRGB.Button("Escolher cor",Color.Black);pick.SetBounds(14,58,145,30);Controls.Add(pick);
  hex=new TextBox{Location=new Point(178,64),Width=92,MaxLength=7,BackColor=BackColor,ForeColor=Color.White,BorderStyle=BorderStyle.None};Controls.Add(hex);
  error=new Label{Text="Use #RRGGBB",ForeColor=Color.Salmon,Location=new Point(282,64),Size=new Size(128,22),Visible=false};Controls.Add(error);
  pick.Click+=delegate{int n=0;if(ValidHex)n=int.Parse(hex.Text.Substring(1),System.Globalization.NumberStyles.HexNumber);using(var d=new SavedColorDialog{Color=EffectOptions.ColorOf(n),FullOpen=true})if(d.ShowDialog()==DialogResult.OK)Set(d.Color.ToArgb()&0xffffff,true);};
  hex.TextChanged+=delegate{if(updating)return;error.Visible=!ValidHex;if(ValidHex)Set(int.Parse(hex.Text.Substring(1),System.Globalization.NumberStyles.HexNumber),true);};Set(color,false);
 }
 void Set(int color,bool notify){updating=true;var c=EffectOptions.ColorOf(color);hex.Text="#"+color.ToString("X6");pick.BackColor=c;pick.ForeColor=c.GetBrightness()>.55?Color.Black:Color.White;error.Visible=false;updating=false;if(notify&&Changed!=null)Changed(color);}
}
partial class EffectEditor:Form {
 public EffectOptions Result;public int Speed;
 readonly int mode;readonly LightState[] states;
 readonly List<EffectColorControl> colors=new List<EffectColorControl>();
 readonly DeviceArt[] art=new DeviceArt[4];readonly Label[] rgb=new Label[4];
 readonly Label[] deviceNames=new Label[4];
 Timer timer=new Timer{Interval=33};Stopwatch clock=Stopwatch.StartNew();
 FlowLayoutPanel[] pages=new FlowLayoutPanel[3];Button[] tabs=new Button[3];bool[] availableTabs=new bool[3];Panel right,left,bottom;Label note,description;CheckBox customPaletteToggle;int currentTab;
 public EffectEditor(int mode,EffectOptions initial,int speed,LightState[] states){
  this.mode=mode;this.states=states;Result=initial.Copy();Speed=speed;
  Text="ThebestRGB / Configurar "+EffectLibrary.Names[mode];ClientSize=new Size(1080,Math.Min(720,Screen.PrimaryScreen.WorkingArea.Height-55));MinimumSize=new Size(1040,690);StartPosition=FormStartPosition.CenterParent;BackColor=Color.FromArgb(10,11,15);ForeColor=Color.White;Font=new Font("Segoe UI",9);AutoScaleDimensions=new SizeF(96,96);AutoScaleMode=AutoScaleMode.Dpi;
  bottom=new Panel{Dock=DockStyle.Bottom,Height=78,BackColor=Color.FromArgb(17,18,23)};Controls.Add(bottom);
  var cancel=ThebestRGB.Button("Cancelar",Color.FromArgb(35,36,45));cancel.SetBounds(20,23,100,34);cancel.DialogResult=DialogResult.Cancel;bottom.Controls.Add(cancel);CancelButton=cancel;
  var reset=ThebestRGB.Button("Restaurar este efeito",Color.FromArgb(35,36,45));reset.SetBounds(132,23,167,34);bottom.Controls.Add(reset);reset.Click+=delegate{Result=new EffectOptions();Speed=3;TrackEditor();BuildOptions();note.Text="Padrão restaurado na prévia. Confirme para manter.";};
  var confirm=ThebestRGB.Button("Guardar ajustes",Color.FromArgb(151,119,246));confirm.SetBounds(ClientSize.Width-334,23,124,34);confirm.Anchor=AnchorStyles.Right|AnchorStyles.Top;bottom.Controls.Add(confirm);confirm.Click+=delegate{if(colors.Any(c=>!c.ValidHex)){SelectTab(0);note.Text="Corrija a cor indicada: use # e seis caracteres de 0 a F.";return;}DialogResult=DialogResult.OK;Close();};
  var hint=new Label{Text="Depois, clique em Aplicar iluminação\nna tela principal para enviar aos LEDs.",ForeColor=ThebestRGB.Muted,Location=new Point(ClientSize.Width-489,21),Size=new Size(275,42),Anchor=AnchorStyles.Right|AnchorStyles.Top};hint.Visible=false;bottom.Controls.Add(hint);
  right=new Panel{Dock=DockStyle.Right,Width=480,BackColor=Color.FromArgb(16,17,22)};Controls.Add(right);
  var heading=new Label{Text="Personalize o efeito",Font=new Font("Segoe UI",16,FontStyle.Bold),Location=new Point(20,20),Size=new Size(436,34)};right.Controls.Add(heading);
  for(int i=0;i<3;i++){int tab=i;tabs[i]=ThebestRGB.Button(new[]{"Cores","Movimento","Intensidade"}[i],Color.FromArgb(30,31,40));tabs[i].SetBounds(20+i*147,68,139,34);right.Controls.Add(tabs[i]);tabs[i].Click+=delegate{SelectTab(tab);};pages[i]=new FlowLayoutPanel{Location=new Point(20,120),Size=new Size(440,450),FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=false,Anchor=AnchorStyles.Top|AnchorStyles.Bottom|AnchorStyles.Left};right.Controls.Add(pages[i]);}
  left=new Panel{Dock=DockStyle.Fill};Controls.Add(left);left.BringToFront();
  left.Controls.Add(new Label{Text=EffectLibrary.Names[mode],Font=new Font("Segoe UI",26,FontStyle.Bold),Location=new Point(24,22),AutoSize=true});
  description=new Label{Text="PRÉVIA AO VIVO\nOs ajustes aparecem aqui enquanto você edita.",ForeColor=ThebestRGB.Muted,Location=new Point(26,79),Size=new Size(520,44)};left.Controls.Add(description);
  for(int i=0;i<4;i++){deviceNames[i]=new Label{Text=states[i].Name+(states[i].Included?"":" · fora da seleção"),Size=new Size(250,23)};left.Controls.Add(deviceNames[i]);art[i]=new DeviceArt(i){Size=new Size(250,96)};left.Controls.Add(art[i]);rgb[i]=new Label{Size=new Size(250,22),ForeColor=ThebestRGB.Muted,Font=new Font("Segoe UI",8)};left.Controls.Add(rgb[i]);}
  note=new Label{Text="A prévia mostra a iluminação de cada dispositivo.\nO brilho de cada dispositivo é ajustado na tela principal.",ForeColor=ThebestRGB.Muted,Size=new Size(510,54)};left.Controls.Add(note);
  bottom.SendToBack();Resize+=delegate{Arrange();};BuildOptions();InitializeEditorFeatures();Arrange();timer.Tick+=delegate{UpdatePreview();};timer.Start();UpdatePreview();FormClosed+=delegate{timer.Stop();timer.Dispose();};DarkDialogChrome.Attach(this,Text);
 }
 void Arrange(){if(left==null)return;int w=left.ClientSize.Width,columns=w<E(490)?1:2,cell=(w-E(48)-E(12)*(columns-1))/columns;left.AutoScroll=columns==1;for(int i=0;i<4;i++){int x=E(24)+i%columns*(cell+E(12)),y=E(171)+i/columns*E(155);deviceNames[i].SetBounds(x,y,cell,E(24));art[i].SetBounds(x,y+E(26),cell,E(85));rgb[i].SetBounds(x,y+E(113),cell,E(22));}note.SetBounds(E(26),columns==1?E(800):Math.Min(E(510),left.Height-E(70)),w-E(50),E(64));foreach(var p in pages){p.SetBounds(E(20),E(120),right.ClientSize.Width-E(40),Math.Max(E(160),right.ClientSize.Height-E(132)));p.AutoScroll=p.Height<E(420);foreach(Control item in p.Controls)item.Width=p.ClientSize.Width-E(16);}}
 void SelectTab(int index){if(index<0||index>=tabs.Length||!availableTabs[index])index=Enumerable.Range(0,tabs.Length).FirstOrDefault(i=>availableTabs[i]);currentTab=index;for(int i=0;i<3;i++){tabs[i].Visible=availableTabs[i];pages[i].Visible=availableTabs[i]&&i==index;tabs[i].BackColor=i==index?Color.FromArgb(70,54,107):Color.FromArgb(30,31,40);}}
 void RefreshTabs(){int count=availableTabs.Count(x=>x);if(count==0){availableTabs[2]=true;count=1;}int gap=8,width=(right.ClientSize.Width-40-gap*(count-1))/count,position=0;for(int i=0;i<tabs.Length;i++)if(availableTabs[i])tabs[i].SetBounds(20+position++*(width+gap),68,width,34);if(!availableTabs[Math.Max(0,Math.Min(currentTab,tabs.Length-1))])currentTab=Enumerable.Range(0,tabs.Length).FirstOrDefault(i=>availableTabs[i]);} void BuildOptions(){
  foreach(var p in pages){foreach(Control c in p.Controls.Cast<Control>().ToArray())c.Dispose();p.Controls.Clear();}colors.Clear();
  bool screen=mode==18,electric=mode==16,audio=mode==17;
  bool colorDriven=mode==2||mode==5||mode==12||mode==14||mode==15||audio;
  tabs[0].Text=screen?"Tela":"Cores";tabs[1].Text=screen?"Suavidade":"Movimento";tabs[2].Text="Ajuste";
  if(screen){
   TextBlock(0,"Cores da tela","O Ambilight usa a cor mais forte da cena; não usa paleta manual.");
   var follow=new StudioCheckBox{Text="Acompanhar luminosidade da tela",Checked=Result.FollowScreenBrightness,ForeColor=Color.White,Width=424,Height=32};pages[0].Controls.Add(follow);follow.CheckedChanged+=delegate{Result.FollowScreenBrightness=follow.Checked;TrackEditor();};
   AddSlider(0,"Saturação da imagem","Ajusta apenas a vivacidade das cores capturadas.",0,100,Result.Saturation,v=>Result.Saturation=v,v=>v+"%");
   TextBlock(1,"Transição entre cenas","Controla quão rápido a iluminação acompanha a imagem.");
   AddSlider(1,"Suavidade","À esquerda reage mais devagar; à direita acompanha mais rápido.",1,5,Speed,v=>Speed=v,v=>new[]{"Muito suave","Suave","Equilibrada","Rápida","Imediata"}[v-1]);
  }else{
   if(electric){
    TextBlock(0,"Escolha a paleta","Pulsos são os flashes. A base ilumina entre eles.");
    AddColor("Pulsos","Cor dos flashes que percorrem o setup.",Result.Foreground,v=>Result.Foreground=v);
    AddColor("Base","Tom suave que aparece entre os flashes.",Result.Grid,v=>Result.Grid=v);
    AddColor("Fundo","Escolha preto para apagar o fundo.",Result.Background,v=>Result.Background=v);
   }else if(colorDriven){
    TextBlock(0,"Paleta do efeito","Estas cores definem a aparência do efeito; editar uma ativa a paleta personalizada.");
    var custom=new StudioCheckBox{Text="Usar minha paleta de cores",Checked=Result.CustomPalette,ForeColor=Color.White,Width=424,Height=30};customPaletteToggle=custom;pages[0].Controls.Add(custom);custom.CheckedChanged+=delegate{Result.CustomPalette=custom.Checked;TrackEditor();};
    AddColor("Cor principal","A cor que mais aparece no efeito.",Result.Foreground,v=>Result.Foreground=v);
    AddColor("Cor secundária","Uma segunda cor para combinar com a principal.",Result.Grid,v=>Result.Grid=v);
    AddColor("Fundo","Escolha preto para apagar o fundo.",Result.Background,v=>Result.Background=v);
   }
   if(!audio){
    TextBlock(1,"Defina o ritmo",electric?"Comece pela velocidade. Depois ajuste o formato dos pulsos.":"A velocidade controla a duração do ciclo de animação.");
    AddSlider(1,"Velocidade","Mais lenta à esquerda, mais rápida à direita.",1,5,Speed,v=>Speed=v,v=>new[]{"Muito lenta","Lenta","Média","Rápida","Muito rápida"}[v-1]);
    if(electric){AddSlider(1,"Quantidade de pulsos","Número de flashes distribuídos em cada ciclo.",1,5,Result.Density,v=>Result.Density=v,v=>v==1?"1 pulso":v+" pulsos");AddSlider(1,"Largura do pulso","De flashes estreitos a faixas mais largas.",10,100,Result.Width,v=>Result.Width=v,v=>v+" / 100");AddSlider(1,"Rastro do pulso","Quanto do brilho permanece atrás de cada flash.",0,100,Result.Dissipation,v=>Result.Dissipation=v,v=>v+"%");}
   }
   TextBlock(2,audio?"Resposta ao áudio":"Cor e acabamento",audio?"A luz acompanha o áudio de saída do Windows.":"Ajuste a vivacidade sem alterar o brilho dos dispositivos.");
   AddSlider(2,"Saturação","À esquerda, tons neutros. À direita, cores vivas.",0,100,Result.Saturation,v=>Result.Saturation=v,v=>v+"%");
   if(electric){AddSlider(2,"Ritmo dos pulsos","Ajuste relativo à velocidade da aba Movimento.",1,100,Result.SparkSpeed,v=>Result.SparkSpeed=v,v=>(v/50.0).ToString("0.00")+"×");AddSlider(2,"Ritmo da base","Velocidade da luz suave entre os flashes.",1,100,Result.GridSpeed,v=>Result.GridSpeed=v,v=>(v/50.0).ToString("0.00")+"×");AddSlider(2,"Intervalo entre dispositivos","Em 0%, os pulsos aparecem juntos. Aumente para alternar.",0,100,Result.Spread,v=>Result.Spread=v,v=>v+"%");var reverse=new StudioCheckBox{Text="Inverter o sentido dos pulsos",Checked=Result.Reverse,ForeColor=Color.White,Width=424,Height=32};pages[2].Controls.Add(reverse);reverse.CheckedChanged+=delegate{Result.Reverse=reverse.Checked;TrackEditor();};}
  }
  if(IsHandleCreated){float factor=E(96)/96f;if(factor!=1)foreach(var page in pages)foreach(Control item in page.Controls)item.Scale(new SizeF(factor,factor));}availableTabs[0]=screen||electric||colorDriven;availableTabs[1]=!audio;availableTabs[2]=!screen;RefreshTabs();SelectTab(currentTab);Arrange();
 } void TextBlock(int tab,string title,string description){var p=new Panel{Width=424,Height=50,Margin=new Padding(0,0,0,4)};p.Controls.Add(new Label{Text=title,Font=new Font("Segoe UI",11,FontStyle.Bold),Location=new Point(0,0),Size=new Size(424,24)});p.Controls.Add(new Label{Text=description,ForeColor=ThebestRGB.Muted,Location=new Point(0,26),Size=new Size(424,24)});pages[tab].Controls.Add(p);}
 void AddColor(string name,string description,int value,Action<int> change){var c=new EffectColorControl(name,description,value){Margin=new Padding(0,0,0,8)};c.Changed=delegate(int edited){change(edited);if(mode!=16&&customPaletteToggle!=null)customPaletteToggle.Checked=true;TrackEditor();};colors.Add(c);pages[0].Controls.Add(c);}
 void AddSlider(int tab,string name,string hint,int min,int max,int value,Action<int> change,Func<int,string> format){var p=new Panel{Width=424,Height=64,BackColor=Color.FromArgb(30,32,41),Margin=new Padding(0,0,0,4)};p.Controls.Add(new Label{Text=name,Location=new Point(12,4),Size=new Size(218,21),Font=new Font("Segoe UI",9,FontStyle.Bold)});var number=new Label{Text=format(value),Location=new Point(230,4),Size=new Size(100,21),TextAlign=ContentAlignment.TopRight};p.Controls.Add(number);p.Controls.Add(new Label{Text=hint,Location=new Point(12,25),Size=new Size(402,20),ForeColor=ThebestRGB.Muted,Font=new Font("Segoe UI",8)});var slider=new StudioSlider{Minimum=min,Maximum=max,Value=value,BackColor=p.BackColor,AccessibleName=name};slider.SetBounds(7,44,410,18);p.Controls.Add(slider);var input=new NumericUpDown{Minimum=min,Maximum=max,Value=value,BackColor=Color.FromArgb(40,44,57),ForeColor=Color.White,BorderStyle=BorderStyle.FixedSingle,AccessibleName=name};input.SetBounds(340,2,72,24);p.Controls.Add(input);input.ValueChanged+=delegate{slider.Value=(int)input.Value;};slider.ValueChanged+=delegate{input.Value=slider.Value;number.Text=format(slider.Value);change(slider.Value);TrackEditor();};pages[tab].Controls.Add(p);}
 void UpdatePreview(){if(!Visible||WindowState==FormWindowState.Minimized)return;for(int i=0;i<4;i++){Color c=EffectOptions.Sample(mode,states[i].Color,clock.Elapsed.TotalSeconds,Speed,i,Result);if(art[i].Light!=c||art[i].Level!=states[i].Brightness){art[i].Light=c;art[i].Level=states[i].Brightness;art[i].Invalidate();}rgb[i].Text="Brilho do dispositivo: "+states[i].Brightness+"%";}}
}
partial class ThebestRGB {
 EffectOptions[] effectOptions=EffectOptions.Defaults();
 Button configure;
 void InitializeEditor(){
  configure=Button("Configurar efeito",Color.FromArgb(44,51,69));configure.SetBounds(550,82,150,29);hero.Controls.Add(configure);
  configure.Click+=delegate{
   if(busy)return;int mode=effectMode.SelectedIndex;
   if(mode==0){status.Text="Para usar uma cor fixa, escolha a cor e o brilho na tela principal.";return;}
   using(var editor=new EffectEditor(mode,effectOptions[mode],effectSpeed.Value,cards.Select(c=>c.State.Copy()).ToArray())){
    var originalOptions=effectOptions[mode].Copy();int originalSpeed=effectSpeed.Value;editor.ApplyDirect=ApplyFromEditor;editorOpen=true;DialogResult choice;try{choice=editor.ShowDialog(this);}finally{editorOpen=false;}if(choice==DialogResult.OK){effectOptions[mode]=editor.Result.Copy();effectSpeed.Value=editor.Speed;TrackSetup();if(!editor.AppliedDirect)status.Text="Ajustes prontos. Aplique a iluminação ou salve em um perfil.";}else{effectOptions[mode]=originalOptions;effectSpeed.Value=originalSpeed;TrackSetup();}
   }
  };
 }
}



















