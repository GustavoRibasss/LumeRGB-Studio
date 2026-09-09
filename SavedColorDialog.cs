using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Web.Script.Serialization;
class SavedColorDialog:Form {
 public Color Color{get;set;}public bool FullOpen{get;set;}
 readonly Button swatch;readonly TextBox code;readonly Label info;
 static string PaletteFile{get{return Path.Combine(Path.GetDirectoryName(ProfileStore.FileName),"custom-colors.json");}}
 public SavedColorDialog(){Text="Escolher cor";ClientSize=new Size(390,210);StartPosition=FormStartPosition.CenterParent;FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;BackColor=System.Drawing.Color.FromArgb(20,21,27);ForeColor=System.Drawing.Color.White;Font=new Font("Segoe UI",10);
  swatch=new Button{Bounds=new Rectangle(20,20,80,55),FlatStyle=FlatStyle.Flat};Controls.Add(swatch);
  code=new TextBox{Bounds=new Rectangle(120,24,140,28)};Controls.Add(code);
  info=new Label{Bounds=new Rectangle(120,57,250,24)};Controls.Add(info);
  var edit=new Button{Text="Editar / cores salvas",Bounds=new Rectangle(20,95,170,34)};Controls.Add(edit);edit.Click+=delegate{using(var d=new ColorDialog{Color=Color,FullOpen=true}){try{if(File.Exists(PaletteFile))d.CustomColors=new JavaScriptSerializer().Deserialize<int[]>(File.ReadAllText(PaletteFile));}catch{}if(d.ShowDialog(this)==DialogResult.OK){Color=d.Color;RefreshColor();}try{Directory.CreateDirectory(Path.GetDirectoryName(PaletteFile));File.WriteAllText(PaletteFile,new JavaScriptSerializer().Serialize(d.CustomColors));}catch(Exception ex){MessageBox.Show(this,"Não foi possível salvar as cores: "+ex.Message);}}};
  var sample=new Button{Text="Conta-gotas",Bounds=new Rectangle(200,95,170,34)};Controls.Add(sample);sample.Click+=delegate{Hide();try{using(var capture=new PixelPicker()){if(capture.ShowDialog()==DialogResult.OK)Color=capture.Selected;}}catch(Exception ex){MessageBox.Show("Não foi possível capturar a tela: "+ex.Message);}finally{Show();Activate();RefreshColor();}};
  var ok=new Button{Text="Usar cor",Bounds=new Rectangle(200,155,170,34)};Controls.Add(ok);ok.Click+=delegate{int n;string value=code.Text.Trim().TrimStart('#');if(value.Length!=6||!int.TryParse(value,System.Globalization.NumberStyles.HexNumber,null,out n)){info.Text="Use #RRGGBB";return;}Color=System.Drawing.Color.FromArgb((n>>16)&255,(n>>8)&255,n&255);DialogResult=DialogResult.OK;};AcceptButton=ok;
  var cancel=new Button{Text="Cancelar",Bounds=new Rectangle(20,155,170,34),DialogResult=DialogResult.Cancel};Controls.Add(cancel);CancelButton=cancel;Shown+=delegate{RefreshColor();};
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
