// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
partial class ThebestRGB {
 Label deviceHeading,selectionSummary,colorInfo,profileEmpty,profileCount,profileFormTitle,profileNameLabel,profileDescriptionLabel,profileNote;Button selectAll;Panel footer;ToolTip help;SetupStage stage;Button modePicker;ColorRampPreview colorRamp;ContextMenuStrip modes;Panel profileListFrame,profileNameFrame,profileDescriptionFrame;
 Color surface=Color.FromArgb(20,21,27);
 [DllImport("dwmapi.dll",EntryPoint="DwmSetWindowAttribute")]
 static extern int DwmSetWindowAttribute(IntPtr hwnd,int attribute,ref int value,int size);
 [DllImport("uxtheme.dll",EntryPoint="#135")]
 static extern int SetPreferredAppMode(int mode);
 [DllImport("uxtheme.dll",EntryPoint="#133")]
 static extern bool AllowDarkModeForWindow(IntPtr hwnd,bool allow);
 [DllImport("uxtheme.dll",CharSet=CharSet.Unicode)]
 static extern int SetWindowTheme(IntPtr hwnd,string subAppName,string subIdList);
 internal static void ApplyDarkTitleBar(Form form){try{SetPreferredAppMode(2);AllowDarkModeForWindow(form.Handle,true);SetWindowTheme(form.Handle,"DarkMode_Explorer",null);int enabled=1;DwmSetWindowAttribute(form.Handle,20,ref enabled,4);DwmSetWindowAttribute(form.Handle,19,ref enabled,4);}catch{} }
 void InitializeProfessional(){
  Text="ThebestRGB / Studio 22";BackColor=Color.FromArgb(10,11,15);Muted=Color.FromArgb(143,145,159);MinimumSize=new Size(1180,720);ClientSize=new Size(1280,Math.Min(800,Screen.PrimaryScreen.WorkingArea.Height-70));DoubleBuffered=true;
  side.Width=180;side.BackColor=Color.FromArgb(14,15,19);side.AutoScroll=false;main.BackColor=BackColor;InitializeWindowChrome();
  help=new ToolTip{AutoPopDelay=10000};
  foreach(Control item in side.Controls)if(!(item==profileList||item==profileName||item==save||item==load||item==remove))item.Visible=false;
  side.Controls.Add(Label("ThebestRGB",18,Color.White,22,22));side.Controls.Add(Label("S T U D I O  /  2 2",8,Muted,24,72));
  var nav=Button("Iluminação",Color.FromArgb(37,32,52));nav.SetBounds(16,116,148,38);side.Controls.Add(nav);var navMarker=new Panel{BackColor=Color.FromArgb(166,136,255),Location=new Point(16,116),Size=new Size(3,38)};side.Controls.Add(navMarker);nav.Click+=delegate{main.AutoScrollPosition=Point.Empty;};
  side.Controls.Add(Label("MEUS PERFIS",8,Muted,23,195));profileCount=Label("0 salvos",7,Muted,121,196);side.Controls.Add(profileCount);
  profileListFrame=new Panel{Location=new Point(16,225),Size=new Size(148,140),BackColor=surface};profileListFrame.Paint+=delegate(object sender,PaintEventArgs e){using(var p=new Pen(Color.FromArgb(48,48,61)))e.Graphics.DrawRectangle(p,0,0,profileListFrame.Width-1,profileListFrame.Height-1);using(var p=new Pen(Color.FromArgb(78,61,112),2))e.Graphics.DrawLine(p,1,1,profileListFrame.Width-2,1);};side.Controls.Add(profileListFrame);
  profileList.SetBounds(1,1,146,138);profileList.Parent=profileListFrame;profileList.BackColor=surface;profileList.ItemHeight=32;profileList.DrawMode=DrawMode.OwnerDrawFixed;
  profileEmpty=new Label{Text="Nenhum perfil salvo\n\nSalve seu primeiro setup abaixo.",ForeColor=Muted,BackColor=surface,TextAlign=ContentAlignment.MiddleCenter,Font=new Font("Segoe UI",8)};profileEmpty.SetBounds(8,38,132,64);profileListFrame.Controls.Add(profileEmpty);profileEmpty.BringToFront();
  profileList.DrawItem+=DrawProfilePreview;
  profileList.SelectedIndexChanged+=delegate{if(profileList.SelectedIndex>=0)profileName.Text=profiles[profileList.SelectedIndex].Name;};profileList.DoubleClick+=delegate{if(!busy)LoadProfile();};profileList.MouseUp+=delegate(object sender,MouseEventArgs e){if(e.Button!=MouseButtons.Right)return;int index=profileList.IndexFromPoint(e.Location);if(index<0)return;profileList.SelectedIndex=index;var menu=new ContextMenuStrip{Renderer=new StudioMenuRenderer()};menu.Items.Add("Duplicar perfil",null,delegate{DuplicateProfile(index);});menu.Items.Add(profiles[index].Favorite?"Remover favorito":"Marcar como favorito",null,delegate{ToggleFavorite(index);});menu.Show(profileList,e.Location);};
  profileFormTitle=Label("EDITAR PERFIL",8,Muted,20,382);side.Controls.Add(profileFormTitle);
  profileNameLabel=Label("NOME",7,Muted,20,399);side.Controls.Add(profileNameLabel);
  profileNameFrame=new Panel{Location=new Point(20,410),Size=new Size(140,29),BackColor=Color.FromArgb(26,27,35)};profileNameFrame.Paint+=delegate(object sender,PaintEventArgs e){using(var p=new Pen(profileName.Focused?Color.FromArgb(151,119,246):Color.FromArgb(61,62,76)))e.Graphics.DrawRectangle(p,0,0,profileNameFrame.Width-1,profileNameFrame.Height-1);};side.Controls.Add(profileNameFrame);
  profileName.Parent=profileNameFrame;profileName.SetBounds(8,3,124,23);profileName.BackColor=profileNameFrame.BackColor;profileName.BorderStyle=BorderStyle.None;profileName.Font=new Font("Segoe UI",9);profileName.Enter+=delegate{profileNameFrame.Invalidate();};profileName.Leave+=delegate{profileNameFrame.Invalidate();};
  profileDescriptionLabel=Label("DESCRIÇÃO  ·  OPCIONAL",7,Muted,20,444);side.Controls.Add(profileDescriptionLabel);
  profileDescriptionFrame=new Panel{Location=new Point(20,455),Size=new Size(140,40),BackColor=Color.FromArgb(26,27,35)};profileDescriptionFrame.Paint+=delegate(object sender,PaintEventArgs e){using(var p=new Pen(profileDescription.Focused?Color.FromArgb(151,119,246):Color.FromArgb(61,62,76)))e.Graphics.DrawRectangle(p,0,0,profileDescriptionFrame.Width-1,profileDescriptionFrame.Height-1);};side.Controls.Add(profileDescriptionFrame);
  profileDescription=new TextBox{Multiline=true,ScrollBars=ScrollBars.None,MaxLength=80,BackColor=profileDescriptionFrame.BackColor,ForeColor=Color.White,BorderStyle=BorderStyle.None,Font=new Font("Segoe UI",8)};profileDescription.SetBounds(8,5,124,30);profileDescriptionFrame.Controls.Add(profileDescription);help.SetToolTip(profileDescription,"Descrição opcional para lembrar o uso deste perfil.");
  save.SetBounds(16,503,148,34);save.BackColor=Color.FromArgb(151,119,246);save.ForeColor=Color.FromArgb(18,13,30);save.Font=new Font("Segoe UI",9,FontStyle.Bold);
  load.SetBounds(16,545,72,32);load.BackColor=surface;remove.SetBounds(94,545,70,32);remove.BackColor=Color.FromArgb(45,29,38);remove.ForeColor=Color.FromArgb(255,160,175);
  profileNote=new Label{Text="Perfis guardam cores,\nbrilho e efeitos do setup.",ForeColor=Muted,Location=new Point(20,582),Size=new Size(148,28),Font=new Font("Segoe UI",8)};side.Controls.Add(profileNote);
  foreach(Control item in main.Controls)if(item is Label)item.Visible=false;
  main.Controls.Add(Label("Seu espaço. Sua luz.",24,Color.White,24,16));main.Controls.Add(Label("Controle a atmosfera do seu setup.",9,Muted,26,58));
  stage=new SetupStage();main.Controls.Add(stage);
  deviceHeading=Label("DISPOSITIVOS",8,Muted,24,0);main.Controls.Add(deviceHeading);selectionSummary=Label("",8,Muted,0,0);main.Controls.Add(selectionSummary);
  selectAll=Button("",surface);main.Controls.Add(selectAll);selectAll.Click+=delegate{if(busy)return;bool selected=cards.Any(c=>!c.State.Included);foreach(var c in cards)c.Included.Checked=selected;};
  foreach(var card in cards){card.BackColor=surface;card.Art.Parent=stage;card.Art.BackColor=stage.BackColor;card.Art.Caption=card.State.Name;card.Art.Interactive=true;card.Art.Selected=card.State.Included;help.SetToolTip(card.Art,"Clique para selecionar este dispositivo. Clique com o botão direito para escolher uma cor só para ele.");card.Art.Click+=delegate{if(!busy)card.Included.Checked=!card.Included.Checked;};card.Art.MouseUp+=delegate(object sender,MouseEventArgs e){if(e.Button!=MouseButtons.Right||busy)return;using(var d=new SavedColorDialog{Color=card.State.Color,FullOpen=true})if(d.ShowDialog(this)==DialogResult.OK){Color color=d.Color;card.State.R=color.R;card.State.G=color.G;card.State.B=color.B;card.RefreshState(false);card.Status.Text="Cor preparada";status.Text="Cor individual preparada para "+card.State.Name+". Clique em Aplicar iluminação para enviar.";TrackSetup();}};card.Included.Font=new Font("Segoe UI",10,FontStyle.Bold);card.Included.Location=new Point(14,12);foreach(Control c in card.Controls)if(c is Label&&c!=card.Percent&&c!=card.Status&&!connectionLabels.Contains(c)){var meta=(Label)c;meta.Visible=true;meta.ForeColor=Muted;meta.Font=new Font("Segoe UI",7);meta.AutoSize=false;meta.TextAlign=ContentAlignment.MiddleLeft;meta.SetBounds(14,31,card.Width-28,15);}card.Slider.BackColor=surface;card.Status.AutoSize=false;card.Status.Size=new Size(112,20);card.Status.Text="Não aplicado";card.Included.CheckedChanged+=delegate{UpdateSelection();};card.Resize+=delegate{LayoutCard(card);};card.Paint+=delegate(object sender,PaintEventArgs e){using(var p=new Pen(card.State.Included?Color.FromArgb(57,49,77):Color.FromArgb(30,31,38)))e.Graphics.DrawRectangle(p,0,0,card.Width-1,card.Height-1);using(var b=new SolidBrush(card.State.Included?Color.FromArgb(151,119,246):Color.FromArgb(49,50,61)))e.Graphics.FillRectangle(b,0,0,4,card.Height);};}
  hero.BackColor=surface;foreach(Control c in hero.Controls)c.Visible=false;
  string[] labels={"PERSONALIZAR","Modo de iluminação","Cor do setup","Brilho","Velocidade"};int[] ys={18,59,180,292,365};for(int i=0;i<labels.Length;i++)hero.Controls.Add(Label(labels[i],i==0?8:9,Muted,18,ys[i]));
  foreach(Control c in new Control[]{effectMode,configure,pick,hex,master,masterValue,effectSpeed,speedText,all,effectStop})c.Visible=true;
  effectMode.Visible=false;modePicker=Button(EffectLibrary.Names[effectMode.SelectedIndex]+"   ▾",Color.FromArgb(31,32,41));modePicker.SetBounds(18,86,244,30);hero.Controls.Add(modePicker);modes=new ContextMenuStrip{BackColor=Color.FromArgb(31,32,41),ForeColor=Color.White,ShowImageMargin=false,Renderer=new StudioMenuRenderer()};for(int m=0;m<EffectLibrary.Names.Length;m++){if(!EffectLibrary.IsMainEffect(m))continue;int index=m;var item=modes.Items.Add(EffectLibrary.Names[m]);item.Click+=delegate{effectMode.SelectedIndex=index;};}modePicker.Click+=delegate{if(!busy)modes.Show(modePicker,new Point(0,modePicker.Height));};effectMode.SelectedIndexChanged+=delegate{modePicker.Text=effectMode.Text+"   ▾";};configure.SetBounds(18,124,244,32);pick.SetBounds(18,207,135,34);hex.SetBounds(172,217,90,24);hex.BackColor=surface;
InitializeFrequentColors();colorRamp=new ColorRampPreview{Tone=global,Value=(int)Math.Round(global.GetHue()/360.0*100)};colorRamp.SetBounds(18,275,244,14);hero.Controls.Add(colorRamp);colorRamp.ValueChanged+=delegate{if(busy)return;choosingTone=true;try{SetGlobal(colorRamp.SelectedTone);}finally{choosingTone=false;}};colorRamp.MouseUp+=delegate{RememberColor(global);};help.SetToolTip(colorRamp,"Arraste para escolher qualquer cor. O brilho permanece independente.");
  colorInfo=Label("",8,Muted,18,278);colorInfo.Size=new Size(244,24);hero.Controls.Add(colorInfo);
  master.SetBounds(14,320,200,28);masterValue.Location=new Point(217,325);master.BackColor=surface;effectSpeed.SetBounds(14,392,200,28);effectSpeed.BackColor=surface;speedText.Location=new Point(219,397);
  all.Text="Aplicar iluminação";all.BackColor=Color.FromArgb(151,119,246);all.ForeColor=Color.FromArgb(17,12,29);all.Font=new Font("Segoe UI",10,FontStyle.Bold);all.SetBounds(18,444,244,42);effectStop.SetBounds(18,495,244,32);
  effectMode.BackColor=Color.FromArgb(31,32,41);effectMode.ForeColor=Color.White;effectMode.DrawMode=DrawMode.OwnerDrawFixed;effectMode.ItemHeight=24;effectMode.DrawItem+=delegate(object sender,DrawItemEventArgs e){if(e.Index<0)return;using(var b=new SolidBrush(Color.FromArgb(31,32,41)))e.Graphics.FillRectangle(b,e.Bounds);TextRenderer.DrawText(e.Graphics,EffectLibrary.Names[e.Index],effectMode.Font,e.Bounds,Color.White,TextFormatFlags.VerticalCenter);};effectMode.SelectedIndexChanged+=delegate{UpdateEffectControls();UpdateMainColorVisibility();};
  footer=new Panel{Dock=DockStyle.Bottom,Height=54,BackColor=Color.FromArgb(14,15,19)};Controls.Add(footer);footer.SendToBack();activate.Parent=footer;activate.Text="Conectar RAM";activate.SetBounds(16,10,148,34);status.Parent=footer;status.Visible=true;status.SetBounds(204,17,ClientSize.Width-230,28);status.Anchor=AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Top;status.AutoEllipsis=true;status.TextChanged+=delegate{help.SetToolTip(status,status.Text);};
  help.SetToolTip(profileName,"Salvar com o mesmo nome atualiza o perfil.");help.SetToolTip(hex,"Digite #RRGGBB e pressione Enter.");UpdateEffectControls();UpdateMainColorVisibility();UpdateSelection();Shown+=delegate{ApplyDarkTitleBar(this);};FormClosed+=delegate{help.Dispose();modes.Dispose();};
 }
 void LayoutCard(LightCard c){if(designReady){LayoutDesignCard(c,cards.IndexOf(c));return;}c.Included.Location=new Point(U(14),U(10));foreach(Control item in c.Controls)if(item is Label&&!ReferenceEquals(item,c.Percent)&&!ReferenceEquals(item,c.Status)&&!connectionLabels.Contains(item)){item.SetBounds(U(14),U(31),c.Width-U(28),U(15));}c.Status.Location=new Point(c.Width-U(98),U(92));c.Status.Size=new Size(U(92),U(18));connectionLabels[cards.IndexOf(c)].SetBounds(U(14),U(43),c.Width-U(28),U(16));connectionLabels[cards.IndexOf(c)].TextAlign=ContentAlignment.MiddleLeft;c.Slider.SetBounds(U(9),U(58),c.Width-U(73),U(23));c.Percent.Location=new Point(c.Width-U(54),U(62));c.Pick.SetBounds(U(14),U(87),U(72),U(27));c.Apply.SetBounds(U(94),U(87),U(85),U(27));c.Test.SetBounds(U(185),U(87),U(72),U(27));}
 void LayoutSidebar(bool compact){int shift=compact?-12:0;profileFormTitle.Location=new Point(U(20),U(382+shift));profileNameLabel.Location=new Point(U(20),U(399+shift));profileNameFrame.SetBounds(U(20),U(410+shift),U(140),U(29));profileName.SetBounds(U(8),U(3),U(124),U(23));profileDescriptionLabel.Location=new Point(U(20),U(444+shift));profileDescriptionFrame.SetBounds(U(20),U(455+shift),U(140),U(40));profileDescription.SetBounds(U(8),U(5),U(124),U(30));save.SetBounds(U(16),U(503+shift),U(148),U(34));load.SetBounds(U(16),U(545+shift),U(72),U(32));remove.SetBounds(U(94),U(545+shift),U(70),U(32));profileNote.Location=new Point(U(20),U(compact?568:582));profileNote.Size=new Size(U(148),U(28));}
 void UpdateEffectControls(){if(modePicker!=null)modePicker.Enabled=!busy;bool animated=effectMode.SelectedIndex>0;configure.Enabled=animated&&!busy;effectSpeed.Enabled=animated&&!busy;speedText.ForeColor=animated?Color.White:Muted;}
void UpdateMainColorVisibility(){if(colorInfo==null)return;bool fixedMode=effectMode.SelectedIndex==0;foreach(Control c in hero.Controls){if(c==pick||c==hex||(c is StudioButton&&c.Top==251))c.Visible=fixedMode;if(c is Label&&c.Text=="Cor do setup")c.Visible=fixedMode;if(c is Label&&c.Text=="Brilho")c.Location=new Point(18,fixedMode?292:180);if(c is Label&&c.Text=="Velocidade")c.Location=new Point(18,fixedMode?365:255);}colorRamp.Visible=fixedMode;colorInfo.Visible=false;master.SetBounds(14,fixedMode?320:208,200,28);masterValue.Location=new Point(217,fixedMode?325:213);effectSpeed.SetBounds(14,fixedMode?392:283,200,28);speedText.Location=new Point(219,fixedMode?397:288);all.SetBounds(18,fixedMode?444:335,244,42);effectStop.SetBounds(18,fixedMode?495:386,244,32);LayoutScreenControls();}
 void UpdateSelection(){if(selectionSummary==null)return;int n=cards.Count(c=>c.State.Included);selectionSummary.Text=n+" selecionados";selectAll.Text=n==4?"Desmarcar todos":"Selecionar todos";foreach(var c in cards){c.Art.Selected=c.State.Included;c.Art.Invalidate();c.Invalidate();}UpdateMaxState();}
 void LayoutProfessional(){if(designReady){LayoutDesign();return;}if(footer==null)return;main.SuspendLayout();bool compact=main.ClientSize.Height<720;LayoutSidebar(compact);int width=Math.Max(U(952),main.ClientSize.Width-U(48)),left=width-U(300);int stageY=compact?126:132,stageH=compact?190:208,headingY=compact?322:348,selectY=compact?320:344,cardY=compact?352:370,rowGap=compact?128:140,cardHeight=compact?120:126;stage.SetBounds(U(24),U(stageY),left,U(stageH));hero.SetBounds(U(24)+left+U(20),U(92),U(280),U(534));deviceHeading.Location=new Point(U(24),U(headingY));selectionSummary.Location=new Point(U(140),U(headingY));selectAll.SetBounds(U(24)+left-U(151),U(selectY),U(139),U(26));int cw=(left-U(14))/2;for(int i=0;i<4;i++){cards[i].SetBounds(U(24)+i%2*(cw+U(14)),U(cardY)+i/2*U(rowGap),cw,U(cardHeight));LayoutCard(cards[i]);int artH=compact?78:76,artY=compact?26:30,artGap=compact?78:76;cards[i].Art.SetBounds(U(12)+i%2*(left/2),U(artY)+i/2*U(artGap),left/2-U(24),U(artH));}main.AutoScrollMinSize=Size.Empty;main.VerticalScroll.Visible=false;main.HorizontalScroll.Visible=false;if(shortcutHint!=null)shortcutHint.Visible=!compact;LayoutFeatures();LayoutUpgrade();main.ResumeLayout();}
}


















