using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
static class ScreenColors {
 static int current=Color.Black.ToArgb();
 public static Color Current {get{return Color.FromArgb(Volatile.Read(ref current));}}
 public static Color Dominant(Bitmap image){
  var count=new int[99];var red=new long[99];var green=new long[99];var blue=new long[99];
  for(int y=0;y<image.Height;y++)for(int x=0;x<image.Width;x++){
   Color c=image.GetPixel(x,y);int max=Math.Max(c.R,Math.Max(c.G,c.B)),min=Math.Min(c.R,Math.Min(c.G,c.B));
   int saturation=max==0?0:Math.Min(3,(max-min)*4/max);
   int bucket=max<12?96:(max-min<max*.18?(max<128?97:98):(int)((c.GetHue()+7.5f)%360/15)*4+saturation);
   count[bucket]++;red[bucket]+=c.R;green[bucket]+=c.G;blue[bucket]+=c.B;
  }
  int winner=0;for(int i=1;i<count.Length;i++)if(count[i]>count[winner])winner=i;
  if(count[winner]==0)return Color.Black;
  return Color.FromArgb((int)(red[winner]/count[winner]),(int)(green[winner]/count[winner]),(int)(blue[winner]/count[winner]));
 }
 public static IDisposable Start(int speed,bool followBrightness){return new Capture(speed,followBrightness);}
 public static Color Normalize(Color c){int peak=Math.Max(c.R,Math.Max(c.G,c.B));return peak==0?Color.White:Color.FromArgb(c.R*255/peak,c.G*255/peak,c.B*255/peak);}
 public static Color FullBrightness(Color color){int peak=Math.Max(color.R,Math.Max(color.G,color.B));if(peak==0)return Color.Black;return Color.FromArgb(VividChannel(color.R,peak),VividChannel(color.G,peak),VividChannel(color.B,peak));}
 static int VividChannel(int channel,int peak){return (int)Math.Round(255*Math.Pow((double)channel/peak,2.0));}
 sealed class Capture:IDisposable {
  readonly CancellationTokenSource stop=new CancellationTokenSource();readonly Task worker;
public Capture(int speed,bool followBrightness){worker=Task.Run(async ()=>{Color smooth=Color.Black;double red=0,greenSmooth=0,blue=0;var clock=System.Diagnostics.Stopwatch.StartNew();double previous=0;try{while(!stop.IsCancellationRequested){try{var screen=Screen.PrimaryScreen;if(screen==null)break;var bounds=screen.Bounds;using(var frame=new Bitmap(bounds.Width,bounds.Height))using(var g=Graphics.FromImage(frame))using(var small=new Bitmap(32,18))using(var scaled=Graphics.FromImage(small)){g.CopyFromScreen(bounds.Location,Point.Empty,bounds.Size);scaled.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;scaled.DrawImage(frame,new Rectangle(0,0,32,18));var dominant=Dominant(small);var target=followBrightness?dominant:(dominant.ToArgb()==Color.Black.ToArgb()?Normalize(smooth):Normalize(dominant));double now=clock.Elapsed.TotalSeconds;double elapsed=Math.Min(.2,now-previous);previous=now;double duration=2.8-.30*Math.Max(1,Math.Min(5,speed));double blend=1-Math.Exp(-elapsed/duration);red+=(target.R-red)*blend;greenSmooth+=(target.G-greenSmooth)*blend;blue+=(target.B-blue)*blend;smooth=Color.FromArgb((int)Math.Round(red),(int)Math.Round(greenSmooth),(int)Math.Round(blue));Volatile.Write(ref current,(followBrightness?smooth:Normalize(smooth)).ToArgb());}}catch{Volatile.Write(ref current,Color.Black.ToArgb());}await Task.Delay(33,stop.Token);}}catch(OperationCanceledException){}});}
  public void Dispose(){stop.Cancel();worker.ContinueWith(t=>stop.Dispose());}
 }
}
