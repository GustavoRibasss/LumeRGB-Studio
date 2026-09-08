using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

partial class LumeStudio {
 internal const string ReleaseVersion="22.4.1";
 bool designReady,designLayout;
 Button newProfile,editProfile,aboutStudio;
 void InitializeDesign(){
  Text="ThebestRGB · "+ReleaseVersion;windowTitle.Text=Text;Muted=Color.FromArgb(168,174,192);
  MinimumSize=new Size(880,600);
  foreach(var c in new Control[]{profileFormTitle,profileNameLabel,profileNameFrame,profileDescriptionLabel,profileDescriptionFrame,profileNote})c.Visible=false;
  foreach(Control c in side.Controls)if(c is Label&&c.Text.StartsWith("S T U D I O"))c.Text="STUDIO / "+ReleaseVersion;
  newProfile=Button("+  Novo perfil",surface);side.Controls.Add(newProfile);newProfile.Click+=delegate{EditProfileDetails(true);};
  editProfile=Button("Editar perfil",surface);side.Controls.Add(editProfile);editProfile.Click+=delegate{EditProfileDetails(false);};
  aboutStudio=Button("Sobre o ThebestRGB",surface);side.Controls.Add(aboutStudio);aboutStudio.Click+=delegate{using(var f=new Form{Text="Sobre o ThebestRGB",ClientSize=new Size(440,270),BackColor=surface,ForeColor=Color.White,StartPosition=FormStartPosition.CenterParent,Font=new Font("Segoe UI",10),MaximizeBox=false,MinimizeBox=false}){f.Controls.Add(new Label{Text="lume",Font=new Font("Segoe UI",28,FontStyle.Bold),AutoSize=true,Location=new Point(24,22)});f.Controls.Add(new Label{Text="Studio "+ReleaseVersion+"\nIluminação para o seu espaço.\n\nOpenRGB integrado · GPL-2.0\nPerfis e preferências salvos neste computador.",AutoSize=false,Size=new Size(390,135),Location=new Point(26,88)});var close=Button("Fechar",surface);close.SetBounds(300,224,112,32);close.DialogResult=DialogResult.Cancel;f.Controls.Add(close);f.CancelButton=close;DarkDialogChrome.Attach(f,f.Text);f.ShowDialog(this);}};
  profileEmpty.Text="Seu setup, do seu jeito.\n\nCrie seu primeiro perfil.";
  foreach(var card in cards){card.BackColor=Color.FromArgb(23,26,35);card.Slider.BackColor=card.BackColor;card.Status.BackColor=Color.FromArgb(32,36,47);card.Status.TextAlign=ContentAlignment.MiddleCenter;card.Status.ForeColor=Muted;card.Status.TextChanged+=delegate{card.Status.ForeColor=card.Status.Text=="Aplicado"?Color.FromArgb(116,226,179):card.Status.Text=="Falhou"?Color.Salmon:Muted;};}
  designReady=true;LayoutDesign();
 }
 void EditProfileDetails(bool fresh){if(busy)return;int index=profileList.SelectedIndex;using(var f=new Form{Text=fresh?"Novo perfil":"Editar perfil",ClientSize=new Size(420,300),BackColor=surface,ForeColor=Color.White,Font=new Font("Segoe UI",10),StartPosition=FormStartPosition.CenterParent,MaximizeBox=false,MinimizeBox=false}){
  var name=new TextBox{Text=fresh?"":index>=0?profiles[index].Name:profileName.Text,MaxLength=40,BackColor=Color.FromArgb(32,36,47),ForeColor=Color.White};name.SetBounds(24,66,372,30);
  var description=new TextBox{Text=fresh?"":index>=0?profiles[index].Description:profileDescription.Text,MaxLength=80,Multiline=true,BackColor=name.BackColor,ForeColor=Color.White};description.SetBounds(24,134,372,76);
  f.Controls.Add(new Label{Text="Nome do perfil",AutoSize=true,Location=new Point(24,38)});f.Controls.Add(name);f.Controls.Add(new Label{Text="Descrição opcional",AutoSize=true,Location=new Point(24,107)});f.Controls.Add(description);
  var ok=Button("Salvar setup atual",Color.FromArgb(151,119,246));ok.ForeColor=Color.FromArgb(18,13,30);ok.SetBounds(214,238,182,36);f.Controls.Add(ok);var cancel=Button("Cancelar",surface);cancel.SetBounds(24,238,114,36);cancel.DialogResult=DialogResult.Cancel;f.Controls.Add(cancel);f.CancelButton=cancel;
  ok.Click+=delegate{if(string.IsNullOrWhiteSpace(name.Text)){name.Focus();return;}profileName.Text=name.Text.Trim();profileDescription.Text=description.Text;SaveProfile();f.Close();};DarkDialogChrome.Attach(f,f.Text);f.ShowDialog(this);
 }}
 void LayoutDesign(){if(!designReady||designLayout)return;designLayout=true;main.SuspendLayout();side.SuspendLayout();try{
  main.AutoScrollPosition=Point.Empty;int pad=U(24),gap=U(16),width=main.ClientSize.Width-pad*2;bool narrow=width<U(940);int left=narrow?width:width-U(296);int cols=left<U(736)?1:2;int stageH=main.ClientSize.Height<U(700)?U(174):U(230);int y=U(128);LayoutFeatures();resetSetup.SetBounds(U(467),U(91),U(124),U(30));testSelected.SetBounds(U(599),U(91),U(151),U(30));if(narrow){resetSetup.SetBounds(U(24),U(130),U(124),U(30));testSelected.SetBounds(U(160),U(130),U(151),U(30));y=U(176);}
  stage.SetBounds(pad,y,left,stageH);quickProfiles.SetBounds(U(16),stageH-U(30),left-U(32),U(28));quickProfiles.Visible=profiles.Any(p=>p.Favorite);
  deviceHeading.SetBounds(pad,y+stageH+U(21),U(120),U(22));selectionSummary.SetBounds(pad+U(118),y+stageH+U(21),U(120),U(22));
  selectAll.SetBounds(pad+left-U(146),y+stageH+U(12),U(146),U(32));compareButton.SetBounds(pad+left-U(258),y+stageH+U(12),U(102),U(32));
  int cardY=y+stageH+U(56),cw=(left-gap*(cols-1))/cols,ch=U(142);
  for(int i=0;i<4;i++){var card=cards[i];card.SetBounds(pad+i%cols*(cw+gap),cardY+i/cols*(ch+gap),cw,ch);LayoutDesignCard(card,i);int aw=left/2;card.Art.SetBounds(i%2*aw+U(8),U(28)+i/2*((stageH-U(quickProfiles.Visible?60:32))/2),aw-U(16),(stageH-U(quickProfiles.Visible?60:32))/2);}
  int bottom=cardY+(4/cols)*(ch+gap);hero.SetBounds(narrow?pad:pad+left+gap,narrow?bottom:U(128),U(280),U(534));
  int contentBottom=narrow?bottom+hero.Height+pad:Math.Max(bottom-gap,hero.Bottom)+U(4);main.AutoScroll=true;main.AutoScrollMinSize=new Size(0,contentBottom);shortcutHint.Visible=false;
  profileListFrame.SetBounds(U(16),U(224),U(148),Math.Max(U(112),side.ClientSize.Height-U(434)));profileList.SetBounds(1,1,profileListFrame.Width-2,profileListFrame.Height-2);profileEmpty.SetBounds(U(10),U(20),U(128),U(90));int sy=profileListFrame.Bottom+U(12);
  newProfile.SetBounds(U(16),sy,U(148),U(34));editProfile.SetBounds(U(16),sy+U(42),U(148),U(32));save.SetBounds(U(16),sy+U(82),U(148),U(34));save.Text="Salvar setup";load.SetBounds(U(16),sy+U(124),U(72),U(32));remove.SetBounds(U(94),sy+U(124),U(70),U(32));diagnostic.Visible=false;aboutStudio.SetBounds(U(16),sy+U(164),U(148),U(30));
  LayoutWindowChrome();
 }finally{side.ResumeLayout();main.ResumeLayout();designLayout=false;}}
 void LayoutDesignCard(LightCard c,int i){int w=c.Width;c.Included.SetBounds(U(16),U(12),w-U(124),U(28));foreach(Control item in c.Controls)if(item is Label&&item!=c.Status&&item!=c.Percent&&!connectionLabels.Contains(item))item.Visible=false;
  connectionLabels[i].SetBounds(U(16),U(43),w-U(32),U(22));connectionLabels[i].BackColor=c.BackColor;
  c.Slider.SetBounds(U(12),U(72),w-U(84),U(26));c.Percent.SetBounds(w-U(64),U(76),U(54),U(22));
  c.Pick.SetBounds(U(16),U(101),U(64),U(30));c.Apply.SetBounds(U(88),U(101),U(76),U(30));c.Test.SetBounds(U(172),U(101),U(66),U(30));c.Status.SetBounds(w-U(108),U(103),U(94),U(26));
  if(i==0)keyboardBarButton.SetBounds(w-U(94),U(12),U(80),U(28));
 }
}
