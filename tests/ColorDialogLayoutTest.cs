using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
class ColorDialogLayoutTest {
 [STAThread] static void Main(){
  Application.EnableVisualStyles();
  ProfileStore.FileName=Path.Combine(Path.GetTempPath(),"ThebestRGB-color-dialog-"+Guid.NewGuid(),"profiles.json");
  using(var dialog=new SavedColorDialog{Color=Color.FromArgb(224,219,255)}){
   dialog.Show();Application.DoEvents();
   if(dialog.FormBorderStyle!=FormBorderStyle.None)throw new Exception("Color dialog should use the dark custom header.");
   if(dialog.Controls.OfType<TextBox>().Any(input=>input.BackColor.ToArgb()==Color.White.ToArgb()))throw new Exception("Hex input must not use a white background.");
   using(var image=new Bitmap(dialog.Width,dialog.Height)){dialog.DrawToBitmap(image,new Rectangle(Point.Empty,image.Size));image.Save("../color-dialog.png");}
   dialog.Close();
  }
  Console.WriteLine("PASS: dark color dialog rendered without white input or title bar.");
 }
}
