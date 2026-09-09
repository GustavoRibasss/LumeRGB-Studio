using System;using System.Drawing;
class DominantColorTest {
 static void Main(){using(var b=new Bitmap(10,10)){using(var g=Graphics.FromImage(b)){g.Clear(Color.White);g.FillRectangle(new SolidBrush(Color.FromArgb(90,5,8)),0,0,7,10);}var c=ScreenColors.Dominant(b);if(c.R!=90||c.G!=5||c.B!=8)throw new Exception("Dark red diluted");using(var g=Graphics.FromImage(b))g.Clear(Color.Black);if(ScreenColors.Dominant(b).ToArgb()!=Color.Black.ToArgb())throw new Exception("Black changed");}Console.WriteLine("PASS: dominant dark red preserved; black preserved");}
}
