using System;using System.Drawing;using System.Windows.Forms;using System.Threading.Tasks;
class BarLayoutCheck {
 [STAThread] static void Main(){Application.EnableVisualStyles();using(var f=new KeyboardBarForm(new KeyboardBarSettings(),s=>Task.FromResult<string>(null))){f.Show();Application.DoEvents();Label label=null;Button selector=null;foreach(Control c in f.Controls){if(c.Text=="Modo da barra")label=c as Label;if(c is Button&&c.Text.StartsWith("Acompanhar"))selector=(Button)c;}if(label==null||selector==null||label.Bottom>selector.Top)throw new Exception("Mode label overlaps selector");using(var b=new Bitmap(f.Width,f.Height)){f.DrawToBitmap(b,new Rectangle(0,0,f.Width,f.Height));b.Save("tests/bar-layout.png");}Console.WriteLine("PASS: label separated from selector");}}
}
