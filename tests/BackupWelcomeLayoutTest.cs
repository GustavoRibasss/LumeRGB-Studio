using System;
using System.Drawing;
using System.Windows.Forms;
class BackupWelcomeLayoutTest {
 [STAThread] static void Main(){Application.EnableVisualStyles();using(var form=new BackupWelcomeForm()){form.Show();Application.DoEvents();if(form.FormBorderStyle!=FormBorderStyle.None)throw new Exception("Welcome form should use dark chrome.");foreach(Control control in form.Controls)if(control.Bottom>form.ClientSize.Height||control.Right>form.ClientSize.Width)throw new Exception("Welcome control was clipped.");using(var image=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(image,new Rectangle(Point.Empty,image.Size));image.Save("../backup-welcome.png");}form.Close();}Console.WriteLine("PASS: welcome backup layout rendered.");}
}
