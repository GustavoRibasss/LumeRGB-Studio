// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Drawing;
using System.Windows.Forms;

// Maps the three ARGB outputs exposed by the MSI controller to the physical
// devices connected by the user. No hardware write happens in this dialog.
class MsiZonesForm:Form {
 MsiZoneSettings result;StudioCheckBox split;ComboBox fansChannel,waterChannel;Button fansColor,waterColor,auxColor;Panel windowChrome;Label windowTitle;Button windowClose;
 public MsiZoneSettings Result {get{return result;}}
 readonly Color panelColor=Color.FromArgb(23,25,33),surface=Color.FromArgb(43,48,65),accent=Color.FromArgb(157,126,248);
 public MsiZonesForm(MsiZoneSettings original,Color fallback){
  result=original==null?new MsiZoneSettings():original.Copy();if(!result.Custom)result.SetAll(fallback);
  Text="Lume / Zonas ARGB MSI";ClientSize=new Size(590,430);MinimumSize=ClientSize;StartPosition=FormStartPosition.CenterParent;BackColor=Color.FromArgb(11,12,17);ForeColor=Color.White;Font=new Font("Segoe UI",9);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;ShowInTaskbar=false;
  Controls.Add(new Label{Text="Separar Fans e Water Cooler",Font=new Font("Segoe UI",15,FontStyle.Bold),ForeColor=Color.White,Location=new Point(24,20),AutoSize=true});
  Controls.Add(new Label{Text="A MSI 650M Project Zero expõe três canais ARGB. Escolha qual cabo está em cada canal.",ForeColor=LumeStudio.Muted,Location=new Point(26,50),AutoSize=true});
  split=new StudioCheckBox{Text="Controlar cada canal ARGB separadamente",Checked=result.Split,ForeColor=Color.White,BackColor=BackColor,Font=new Font("Segoe UI",9,FontStyle.Bold)};split.SetBounds(24,78,500,31);Controls.Add(split);
  var note=new Label{Text="A numeração segue a ordem dos três canais informados pela placa. Se os cabos estiverem invertidos, troque os canais aqui.",ForeColor=LumeStudio.Muted,Location=new Point(49,109),Size=new Size(505,30),Font=new Font("Segoe UI",8)};Controls.Add(note);
  Controls.Add(new Label{Text="DISPOSITIVO",ForeColor=LumeStudio.Muted,Location=new Point(49,151),AutoSize=true,Font=new Font("Segoe UI",8,FontStyle.Bold)});
  Controls.Add(new Label{Text="CANAL FÍSICO",ForeColor=LumeStudio.Muted,Location=new Point(238,151),AutoSize=true,Font=new Font("Segoe UI",8,FontStyle.Bold)});
  Controls.Add(new Label{Text="COR DO CANAL",ForeColor=LumeStudio.Muted,Location=new Point(405,151),AutoSize=true,Font=new Font("Segoe UI",8,FontStyle.Bold)});
  AddRow("Fans / ventoinhas",156,"Fans",result.FansChannel,out fansChannel,out fansColor,delegate(Color c){result.Fans=Rgb(c);});
  AddRow("Water Cooler",216,"Water Cooler",result.WaterChannel,out waterChannel,out waterColor,delegate(Color c){result.WaterCooler=Rgb(c);});
  ComboBox unusedChannel;AddRow("Acessório ARGB",276,"Acessório",(result.FansChannel==0||result.WaterChannel==0)?2:0,out unusedChannel,out auxColor,delegate(Color c){result.Auxiliary=Rgb(c);});
  // The third channel is derived from the first two, and is shown for clarity.
  fansChannel.SelectedIndexChanged+=delegate{if(fansChannel.SelectedIndex==waterChannel.SelectedIndex)waterChannel.SelectedIndex=(fansChannel.SelectedIndex+1)%3;result.FansChannel=fansChannel.SelectedIndex;};
  waterChannel.SelectedIndexChanged+=delegate{if(waterChannel.SelectedIndex==fansChannel.SelectedIndex)waterChannel.SelectedIndex=(fansChannel.SelectedIndex+1)%3;result.WaterChannel=waterChannel.SelectedIndex;};
  split.CheckedChanged+=delegate{result.Split=split.Checked;SetEnabled();};SetEnabled();
  var cancel=LumeStudio.Button("Cancelar",surface);cancel.SetBounds(307,375,112,34);Controls.Add(cancel);cancel.DialogResult=DialogResult.Cancel;
  var save=LumeStudio.Button("Salvar zonas",accent);save.ForeColor=Color.Black;save.SetBounds(431,375,135,34);Controls.Add(save);save.Click+=delegate{if(fansChannel.SelectedIndex==waterChannel.SelectedIndex){MessageBox.Show(this,"Escolha canais diferentes para Fans e Water Cooler.","Zonas ARGB",MessageBoxButtons.OK,MessageBoxIcon.Information);return;}result.Split=split.Checked;result.FansChannel=fansChannel.SelectedIndex;result.WaterChannel=waterChannel.SelectedIndex;result.Custom=true;DialogResult=DialogResult.OK;Close();};AcceptButton=save;CancelButton=cancel;AttachDarkChrome();
 }
 void AttachDarkChrome(){Control[] content=new Control[Controls.Count];Controls.CopyTo(content,0);FormBorderStyle=FormBorderStyle.None;ClientSize=new Size(590,454);foreach(Control c in content)c.Top+=24;windowChrome=new Panel{Dock=DockStyle.None,BackColor=Color.FromArgb(17,18,24),Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right};Controls.Add(windowChrome);windowChrome.SetBounds(0,0,Width,24);windowTitle=new Label{Text="Lume / Zonas ARGB MSI",ForeColor=Color.FromArgb(205,207,220),Font=new Font("Segoe UI",8),AutoSize=false,TextAlign=ContentAlignment.MiddleLeft};windowTitle.SetBounds(12,0,Width-58,24);windowChrome.Controls.Add(windowTitle);windowClose=LumeStudio.Button("×",Color.FromArgb(52,29,39));windowClose.Font=new Font("Segoe UI",10);windowClose.SetBounds(Width-46,2,42,20);windowChrome.Controls.Add(windowClose);windowClose.Click+=delegate{DialogResult=DialogResult.Cancel;Close();};windowChrome.MouseDown+=delegate(object s,MouseEventArgs e){DragWindow(e);};windowTitle.MouseDown+=delegate(object s,MouseEventArgs e){DragWindow(e);};windowChrome.BringToFront();}
 [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool ReleaseCapture();
 [System.Runtime.InteropServices.DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr hwnd,int msg,IntPtr wParam,IntPtr lParam);
 void DragWindow(MouseEventArgs e){if(e.Button==MouseButtons.Left){ReleaseCapture();SendMessage(Handle,0xA1,(IntPtr)2,IntPtr.Zero);}}
 void AddRow(string label,int y,string colorName,int channel,out ComboBox combo,out Button colorButton,Action<Color> setColor){
  Controls.Add(new Label{Text=label,ForeColor=Color.White,Location=new Point(49,y+11),AutoSize=true,Font=new Font("Segoe UI",9,FontStyle.Bold)});
  combo=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,BackColor=panelColor,ForeColor=Color.White,FlatStyle=FlatStyle.Flat,DrawMode=DrawMode.OwnerDrawFixed,ItemHeight=24};ComboBox box=combo;combo.Items.AddRange(new object[]{"Canal 1","Canal 2","Canal 3"});combo.DrawItem+=delegate(object sender,DrawItemEventArgs e){if(e.Index<0)return;Color fill=(e.State&DrawItemState.Selected)!=0?Color.FromArgb(67,52,101):panelColor;using(var b=new SolidBrush(fill))e.Graphics.FillRectangle(b,e.Bounds);TextRenderer.DrawText(e.Graphics,box.Items[e.Index].ToString(),box.Font,new Rectangle(e.Bounds.X+6,e.Bounds.Y,e.Bounds.Width-6,e.Bounds.Height),Color.White,TextFormatFlags.VerticalCenter);};combo.SelectedIndex=Math.Max(0,Math.Min(2,channel));combo.SetBounds(238,y+5,142,30);Controls.Add(combo);
  colorButton=LumeStudio.Button("Escolher cor",panelColor);Button button=colorButton;colorButton.SetBounds(405,y+5,142,30);Controls.Add(colorButton);int rgb=label.StartsWith("Fans")?result.Fans:label.StartsWith("Water")?result.WaterCooler:result.Auxiliary;PaintColor(button,FromRgb(rgb));button.Click+=delegate{using(var dialog=new ColorDialog{Color=FromRgb(rgb),FullOpen=true})if(dialog.ShowDialog(this)==DialogResult.OK){Color c=RgbApp.FullIntensity(dialog.Color);rgb=Rgb(c);PaintColor(button,c);setColor(c);}};
 }
 void SetEnabled(){bool enabled=split.Checked;fansChannel.Enabled=waterChannel.Enabled=enabled;fansColor.Enabled=waterColor.Enabled=auxColor.Enabled=enabled;}
 static int Rgb(Color c){return c.R<<16|c.G<<8|c.B;}
 static Color FromRgb(int rgb){return Color.FromArgb((rgb>>16)&255,(rgb>>8)&255,rgb&255);}
 static void PaintColor(Button b,Color c){b.BackColor=c;b.ForeColor=c.GetBrightness()>.62?Color.Black:Color.White;}
}
