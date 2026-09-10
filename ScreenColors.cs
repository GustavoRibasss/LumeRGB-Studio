using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

// Ambilight-inspired scene detection. The whole setup follows the strongest colour in the scene.
static class ScreenColors {
 static readonly int[] values={Color.Black.ToArgb(),Color.Black.ToArgb(),Color.Black.ToArgb(),Color.Black.ToArgb()};
 public static Color ForDevice(int device){return Color.FromArgb(Volatile.Read(ref values[0]));}
 public static Color Current {get{return ForDevice(0);}}
 public static IDisposable Start(int speed,bool followBrightness){return new Capture(speed,followBrightness);}
 public static Color Dominant(Bitmap image){return SampleArea(image,new Rectangle(0,0,image.Width,image.Height));}
 static Color SampleArea(Bitmap image,Rectangle area){
  double weight=0,red=0,green=0,blue=0;int left=Math.Max(0,area.Left),top=Math.Max(0,area.Top),right=Math.Min(image.Width,area.Right),bottom=Math.Min(image.Height,area.Bottom);
  for(int y=top;y<bottom;y++)for(int x=left;x<right;x++){Color c=image.GetPixel(x,y);int peak=Math.Max(c.R,Math.Max(c.G,c.B)),floor=Math.Min(c.R,Math.Min(c.G,c.B));if(peak<14)continue;double saturation=(peak-floor)/(double)peak;double influence=(.10+.90*saturation)*(.20+.80*peak/255.0);weight+=influence;red+=c.R*influence;green+=c.G*influence;blue+=c.B*influence;}
  return weight<=0?Color.Black:Color.FromArgb((int)Math.Round(red/weight),(int)Math.Round(green/weight),(int)Math.Round(blue/weight));
 }
 static Color FullColor(Color c){int peak=Math.Max(c.R,Math.Max(c.G,c.B));return peak==0?Color.Black:Color.FromArgb(c.R*255/peak,c.G*255/peak,c.B*255/peak);}
 static Color WithBrightness(Color c,Color source){int level=Math.Max(source.R,Math.Max(source.G,source.B));return Color.FromArgb(c.R*level/255,c.G*level/255,c.B*level/255);}
 static Color StrongestSceneColor(Bitmap image){
  var weight=new double[24];var red=new double[24];var green=new double[24];var blue=new double[24];double neutralLight=0;
  for(int y=0;y<image.Height;y++)for(int x=0;x<image.Width;x++){Color c=image.GetPixel(x,y);int peak=Math.Max(c.R,Math.Max(c.G,c.B)),floor=Math.Min(c.R,Math.Min(c.G,c.B));if(peak<18)continue;double saturation=(peak-floor)/(double)peak;if(saturation<.22){neutralLight+=peak/255.0;continue;}int bin=(int)((c.GetHue()+7.5f)%360/15);double influence=Math.Pow(saturation,1.7)*(.25+.75*peak/255.0);weight[bin]+=influence;red[bin]+=c.R*influence;green[bin]+=c.G*influence;blue[bin]+=c.B*influence;}
  int winner=0;for(int i=1;i<weight.Length;i++)if(weight[i]>weight[winner])winner=i;if(weight[winner]<=0)return neutralLight>=45?Color.White:Color.Black;Color source=Color.FromArgb((int)(red[winner]/weight[winner]),(int)(green[winner]/weight[winner]),(int)(blue[winner]/weight[winner]));float hue=source.GetHue();if(hue<24||hue>=336)return Color.Red;if(hue<48)return Color.FromArgb(255,128,0);if(hue<72)return Color.Yellow;if(hue<166)return Color.Lime;if(hue<211)return source.GetBrightness()>=.42f?Color.FromArgb(102,217,255):Color.FromArgb(0,220,255);if(hue<271)return Color.FromArgb(0,96,255);return Color.FromArgb(176,96,255);
 }
 sealed class Capture:IDisposable {
  readonly CancellationTokenSource stop=new CancellationTokenSource();readonly Task worker;
  public Capture(int speed,bool followBrightness){worker=Task.Run(async ()=>{double red=0,green=0,blue=0;var clock=System.Diagnostics.Stopwatch.StartNew();double previous=0;try{while(!stop.IsCancellationRequested){try{var screen=Screen.PrimaryScreen;if(screen==null)break;var bounds=screen.Bounds;using(var frame=new Bitmap(bounds.Width,bounds.Height))using(var graphics=Graphics.FromImage(frame))using(var small=new Bitmap(96,54))using(var scaled=Graphics.FromImage(small)){graphics.CopyFromScreen(bounds.Location,Point.Empty,bounds.Size);scaled.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;scaled.DrawImage(frame,new Rectangle(0,0,small.Width,small.Height));Color source=StrongestSceneColor(small);Color target=followBrightness?WithBrightness(source,source):source;if(source.ToArgb()==Color.Black.ToArgb())target=followBrightness?Color.Black:Color.FromArgb((int)red,(int)green,(int)blue);double now=clock.Elapsed.TotalSeconds,elapsed=Math.Min(.2,now-previous);previous=now;double duration=2.35-.27*Math.Max(1,Math.Min(5,speed)),blend=1-Math.Exp(-elapsed/duration);red+=(target.R-red)*blend;green+=(target.G-green)*blend;blue+=(target.B-blue)*blend;int rgb=Color.FromArgb((int)Math.Round(red),(int)Math.Round(green),(int)Math.Round(blue)).ToArgb();for(int i=0;i<4;i++)Volatile.Write(ref values[i],rgb);}}catch{for(int i=0;i<4;i++)Volatile.Write(ref values[i],Color.Black.ToArgb());}await Task.Delay(33,stop.Token);}}catch(OperationCanceledException){}});}
  public void Dispose(){stop.Cancel();worker.ContinueWith(t=>stop.Dispose());}
 }
}
