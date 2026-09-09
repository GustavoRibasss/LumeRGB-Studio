using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
static class ScreenColors {
 static int current=Color.Black.ToArgb();
 public static Color Current {get{return Color.FromArgb(Volatile.Read(ref current));}}
 public static IDisposable Start(int speed){return new Capture(speed);}
 public static Color FullBrightness(Color color){int peak=Math.Max(color.R,Math.Max(color.G,color.B));if(peak==0)return Color.Black;return Color.FromArgb((color.R*255+peak/2)/peak,(color.G*255+peak/2)/peak,(color.B*255+peak/2)/peak);}
 sealed class Capture:IDisposable {
  readonly CancellationTokenSource stop=new CancellationTokenSource();readonly Task worker;
public Capture(int speed){worker=Task.Run(async ()=>{Color smooth=Color.Black;try{while(!stop.IsCancellationRequested){try{var screen=Screen.PrimaryScreen;if(screen==null)break;var bounds=screen.Bounds;using(var frame=new Bitmap(bounds.Width,bounds.Height))using(var g=Graphics.FromImage(frame))using(var small=new Bitmap(32,18))using(var scaled=Graphics.FromImage(small)){g.CopyFromScreen(bounds.Location,Point.Empty,bounds.Size);scaled.DrawImage(frame,new Rectangle(0,0,32,18));long r=0,b=0,green=0;for(int y=0;y<18;y++)for(int x=0;x<32;x++){var c=small.GetPixel(x,y);r+=c.R;green+=c.G;b+=c.B;}var target=FullBrightness(Color.FromArgb((int)(r/576),(int)(green/576),(int)(b/576)));smooth=EffectOptions.Mix(smooth,target,.08+.07*Math.Max(1,Math.Min(5,speed)));if(Math.Abs(smooth.R-target.R)<=3&&Math.Abs(smooth.G-target.G)<=3&&Math.Abs(smooth.B-target.B)<=3)smooth=target;Volatile.Write(ref current,smooth.ToArgb());}}catch{Volatile.Write(ref current,Color.Black.ToArgb());}await Task.Delay(66,stop.Token);}}catch(OperationCanceledException){}});}
  public void Dispose(){stop.Cancel();worker.ContinueWith(t=>stop.Dispose());}
 }
}
