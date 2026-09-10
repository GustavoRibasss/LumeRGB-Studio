using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
static class ScreenColors {
 static int current=Color.Black.ToArgb();
 public static Color Current {get{return Color.FromArgb(Volatile.Read(ref current));}}
 public static bool IsRedDominant(Color color){if(color.R<20||color.R<=color.G||color.R<=color.B)return false;float hue=color.GetHue();return hue<=24||hue>=336;}
 // Scene colours are sent as clear, stable RGB hues instead of muddy shades
 // created by shadows. The brightness option can still dim these colours.
 public static bool HasSceneColor(Color color){int max=Math.Max(color.R,Math.Max(color.G,color.B)),min=Math.Min(color.R,Math.Min(color.G,color.B));return max>=20&&(max-min)/(double)max>=.20;}
 public static Color MatchSceneColor(Color color){
  if(!HasSceneColor(color))return Color.Black;
  float hue=color.GetHue();
  if(hue<24||hue>=336)return Color.Red;
  if(hue<48)return Color.FromArgb(255,128,0);
  if(hue<72)return Color.Yellow;
  if(hue<166)return Color.Lime;
  // Bright blue scenes such as ocean, sky, ice and snow need a light-blue
  // output. Keep the vivid cyan only for dark, strongly teal/neon scenes.
  if(hue<211)return color.R>=45||color.GetBrightness()>=.42f?Color.FromArgb(102,217,255):Color.FromArgb(0,220,255);
  if(hue<271)return Color.FromArgb(0,96,255);
  return Color.FromArgb(176,96,255);
 }
 static Color AtScreenBrightness(Color colour,Color source){int level=Math.Max(source.R,Math.Max(source.G,source.B));return Color.FromArgb(colour.R*level/255,colour.G*level/255,colour.B*level/255);}
 public static Color Dominant(Bitmap image){
  // Ignore shadows and neutral pixels. The LEDs should follow the strongest
  // visible colour in the scene: red moon, green grass, blue ice, and so on.
  var weight=new double[24];var red=new double[24];var green=new double[24];var blue=new double[24];
  for(int y=0;y<image.Height;y++)for(int x=0;x<image.Width;x++){
   Color c=image.GetPixel(x,y);int max=Math.Max(c.R,Math.Max(c.G,c.B)),min=Math.Min(c.R,Math.Min(c.G,c.B));
   if(max<18)continue;
   double saturation=(max-min)/(double)max;
   if(saturation<.20)continue;
   int bucket=(int)((c.GetHue()+7.5f)%360/15);
   // Saturated, brighter areas speak louder than dark or washed-out areas,
   // while still allowing a large coloured region to win naturally.
   double influence=(.15+.85*saturation)*(.25+.75*max/255.0);
   weight[bucket]+=influence;red[bucket]+=c.R*influence;green[bucket]+=c.G*influence;blue[bucket]+=c.B*influence;
  }
  int winner=0;for(int i=1;i<weight.Length;i++)if(weight[i]>weight[winner])winner=i;
  if(weight[winner]<=0)return Color.Black;
  return Color.FromArgb((int)(red[winner]/weight[winner]),(int)(green[winner]/weight[winner]),(int)(blue[winner]/weight[winner]));
 }
 public static IDisposable Start(int speed,bool followBrightness){return new Capture(speed,followBrightness);}
 public static Color Normalize(Color c){int peak=Math.Max(c.R,Math.Max(c.G,c.B));return peak==0?Color.White:Color.FromArgb(c.R*255/peak,c.G*255/peak,c.B*255/peak);}
 public static Color FullBrightness(Color color){int peak=Math.Max(color.R,Math.Max(color.G,color.B));if(peak==0)return Color.Black;return Color.FromArgb(VividChannel(color.R,peak),VividChannel(color.G,peak),VividChannel(color.B,peak));}
 static int VividChannel(int channel,int peak){return (int)Math.Round(255*Math.Pow((double)channel/peak,2.0));}
 sealed class Capture:IDisposable {
  readonly CancellationTokenSource stop=new CancellationTokenSource();readonly Task worker;
public Capture(int speed,bool followBrightness){worker=Task.Run(async ()=>{Color smooth=Color.Black;double red=0,greenSmooth=0,blue=0;var clock=System.Diagnostics.Stopwatch.StartNew();double previous=0;try{while(!stop.IsCancellationRequested){try{var screen=Screen.PrimaryScreen;if(screen==null)break;var bounds=screen.Bounds;using(var frame=new Bitmap(bounds.Width,bounds.Height))using(var g=Graphics.FromImage(frame))using(var small=new Bitmap(32,18))using(var scaled=Graphics.FromImage(small)){g.CopyFromScreen(bounds.Location,Point.Empty,bounds.Size);scaled.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;scaled.DrawImage(frame,new Rectangle(0,0,32,18));var source=Dominant(small);var colour=MatchSceneColor(source);var target=followBrightness?AtScreenBrightness(colour,source):colour;if(colour.ToArgb()==Color.Black.ToArgb())target=followBrightness?Color.Black:Normalize(smooth);double now=clock.Elapsed.TotalSeconds;double elapsed=Math.Min(.2,now-previous);previous=now;double duration=2.8-.30*Math.Max(1,Math.Min(5,speed));double blend=1-Math.Exp(-elapsed/duration);red+=(target.R-red)*blend;greenSmooth+=(target.G-greenSmooth)*blend;blue+=(target.B-blue)*blend;smooth=Color.FromArgb((int)Math.Round(red),(int)Math.Round(greenSmooth),(int)Math.Round(blue));Volatile.Write(ref current,(followBrightness?smooth:Normalize(smooth)).ToArgb());}}catch{Volatile.Write(ref current,Color.Black.ToArgb());}await Task.Delay(33,stop.Token);}}catch(OperationCanceledException){}});}
  public void Dispose(){stop.Cancel();worker.ContinueWith(t=>stop.Dispose());}
 }
}
