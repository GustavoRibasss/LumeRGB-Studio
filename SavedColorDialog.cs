using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Web.Script.Serialization;
class SavedColorDialog:Form {
 public Color Color{get;set;}public bool FullOpen{get;set;}
 readonly Panel swatch;readonly TextBox code;readonly Label info;
 static string PaletteFile{get{return Path.Combine(Path.GetDirectoryName(ProfileStore.FileName),"custom-colors.json");}}
 public SavedColorDialog(){Text="Escolher cor";ClientSize=new Size(420,240);MinimumSize=MaximumSize=ClientSize;StartPosition=FormStartPosition.CenterParent;FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;BackColor=System.Drawing.Color.FromArgb(20,21,27);ForeColor=System.Drawing.Color.White;Font=new Font("Segoe UI",10);
  Controls.Add(new Label{Text="COR ATUAL",Bounds=new Rectangle(20,16,100,18),ForeColor=ThebestRGB.Muted,Font=new Font("Segoe UI",8,FontStyle.Bold)});
  swatch=new Panel{Bounds=new Rectangle(20,39,96,68),BackColor=Color.White};swatch.Paint+=delegate(object sender,PaintEventArgs e){using(var p=new Pen(Color.FromArgb(220,211,255),2))e.Graphics.DrawRectangle(p,1,1,swatch.Width-3,swatch.Height-3);};Controls.Add(swatch);
  var codeFrame=new Panel{Bounds=new Rectangle(134,39,266,68),BackColor=System.Drawing.Color.FromArgb(31,33,43)};codeFrame.Paint+=delegate(object sender,PaintEventArgs e){using(var p=new Pen(Color.FromArgb(72,68,91)))e.Graphics.DrawRectangle(p,0,0,codeFrame.Width-1,codeFrame.Height-1);};Controls.Add(codeFrame);
  codeFrame.Controls.Add(new Label{Text="CÓDIGO HEX",Bounds=new Rectangle(14,8,120,16),ForeColor=ThebestRGB.Muted,Font=new Font("Segoe UI",7,FontStyle.Bold)});
  code=new TextBox{Bounds=new Rectangle(11,28,244,26),BackColor=codeFrame.BackColor,ForeColor=System.Drawing.Color.White,BorderStyle=BorderStyle.None,Font=new Font("Consolas",12,FontStyle.Bold),MaxLength=7};codeFrame.Controls.Add(code);
  info=new Label{Bounds=new Rectangle(134,112,266,22),ForeColor=ThebestRGB.Muted,Font=new Font("Segoe UI",9)};Controls.Add(info);
  var divider=new Panel{Bounds=new Rectangle(20,143,380,1),BackColor=System.Drawing.Color.FromArgb(57,58,70)};Controls.Add(divider);
  var edit=ThebestRGB.Button("Editar cores salvas",System.Drawing.Color.FromArgb(44,48,63));edit.SetBounds(20,158,183,34);Controls.Add(edit);edit.Click+=delegate{using(var d=new ColorDialog{Color=Color,FullOpen=true}){try{if(File.Exists(PaletteFile))d.CustomColors=new JavaScriptSerializer().Deserialize<int[]>(File.ReadAllText(PaletteFile));}catch{}if(d.ShowDialog(this)==DialogResult.OK){Color=d.Color;RefreshColor();}try{Directory.CreateDirectory(Path.GetDirectoryName(PaletteFile));File.WriteAllText(PaletteFile,new JavaScriptSerializer().Serialize(d.CustomColors));}catch(Exception ex){MessageBox.Show(this,"Não foi possível salvar as cores: "+ex.Message);}}};
  var sample=ThebestRGB.Button("Conta-gotas",System.Drawing.Color.FromArgb(44,48,63));sample.SetBounds(217,158,183,34);Controls.Add(sample);sample.Click+=delegate{Hide();try{using(var capture=new PixelPicker()){if(capture.ShowDialog()==DialogResult.OK)Color=capture.Selected;}}catch(Exception ex){MessageBox.Show("Não foi possível capturar a tela: "+ex.Message);}finally{Show();Activate();RefreshColor();}};
  var cancel=ThebestRGB.Button("Cancelar",System.Drawing.Color.FromArgb(47,35,43));cancel.SetBounds(20,202,140,34);cancel.DialogResult=DialogResult.Cancel;Controls.Add(cancel);CancelButton=cancel;
  var ok=ThebestRGB.Button("Usar cor",System.Drawing.Color.FromArgb(151,119,246));ok.ForeColor=System.Drawing.Color.FromArgb(18,13,30);ok.Font=new Font("Segoe UI",9,FontStyle.Bold);ok.SetBounds(260,202,140,34);Controls.Add(ok);ok.Click+=delegate{int n;string value=code.Text.Trim().TrimStart('#');if(value.Length!=6||!int.TryParse(value,System.Globalization.NumberStyles.HexNumber,null,out n)){info.Text="Informe uma cor como #A060FF";return;}Color=System.Drawing.Color.FromArgb((n>>16)&255,(n>>8)&255,n&255);DialogResult=DialogResult.OK;};AcceptButton=ok;
  Shown+=delegate{RefreshColor();};DarkDialogChrome.Attach(this,Text);
 }
 void RefreshColor(){swatch.BackColor=Color;code.Text="#"+(Color.ToArgb()&0xffffff).ToString("X6");info.Text="RGB "+Color.R+", "+Color.G+", "+Color.B;}
 sealed class PixelPicker:Form {
  Bitmap snapshot;public Color Selected;
  public PixelPicker(){Bounds=SystemInformation.VirtualScreen;FormBorderStyle=FormBorderStyle.None;StartPosition=FormStartPosition.Manual;TopMost=true;ShowInTaskbar=false;Cursor=Cursors.Cross;KeyPreview=true;}
  protected override void OnLoad(EventArgs e){snapshot=new Bitmap(Width,Height);using(var g=Graphics.FromImage(snapshot))g.CopyFromScreen(Location,Point.Empty,Size);BackgroundImage=snapshot;base.OnLoad(e);}
  protected override void OnMouseDown(MouseEventArgs e){if(e.Button==MouseButtons.Left){Selected=snapshot.GetPixel(e.X,e.Y);DialogResult=DialogResult.OK;}else DialogResult=DialogResult.Cancel;}
  protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Escape)DialogResult=DialogResult.Cancel;base.OnKeyDown(e);}
  protected override void Dispose(bool disposing){if(disposing&&snapshot!=null){BackgroundImage=null;snapshot.Dispose();}base.Dispose(disposing);}
 }
}
