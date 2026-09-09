// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Drawing;
using System.Linq;
public class EffectOptions {
 public bool FollowScreenBrightness{get;set;}
 public int Foreground{get;set;} public int Grid{get;set;} public int Background{get;set;}
 public int Saturation{get;set;} public int Intensity{get;set;} public bool CustomPalette{get;set;}
 public int SparkSpeed{get;set;} public int GridSpeed{get;set;} public int Width{get;set;} public int Dissipation{get;set;} public int Density{get;set;} public int Spread{get;set;} public bool Reverse{get;set;}
 public EffectOptions(){Foreground=0x00E1FF;Grid=0x004E60;Background=0x00182A;Saturation=100;Intensity=100;SparkSpeed=50;GridSpeed=50;Width=50;Dissipation=50;Density=2;Spread=65;}
 public EffectOptions Copy(){return (EffectOptions)MemberwiseClone();}
 public static EffectOptions[] Defaults(){return Enumerable.Range(0,EffectLibrary.Names.Length).Select(i=>new EffectOptions()).ToArray();}
 public static bool Valid(EffectOptions o){return o!=null&&o.Foreground>=0&&o.Foreground<=0xffffff&&o.Grid>=0&&o.Grid<=0xffffff&&o.Background>=0&&o.Background<=0xffffff&&o.Saturation>=0&&o.Saturation<=100&&o.Intensity>=0&&o.Intensity<=100&&o.SparkSpeed>=1&&o.SparkSpeed<=100&&o.GridSpeed>=1&&o.GridSpeed<=100&&o.Width>=10&&o.Width<=100&&o.Dissipation>=0&&o.Dissipation<=100&&o.Density>=1&&o.Density<=5&&o.Spread>=0&&o.Spread<=100;}
 public static bool ValidAll(EffectOptions[] a){return a!=null&&a.Length==EffectLibrary.Names.Length&&a.All(Valid);}
 public static Color ColorOf(int x){return Color.FromArgb((x>>16)&255,(x>>8)&255,x&255);}
 public static Color Mix(Color a,Color b,double t){t=Math.Max(0,Math.Min(1,t));return Color.FromArgb((int)Math.Round(a.R+(b.R-a.R)*t),(int)Math.Round(a.G+(b.G-a.G)*t),(int)Math.Round(a.B+(b.B-a.B)*t));}
 static double Pulse(double p,double center,double width){double d=Math.Abs(p-center);d=Math.Min(d,1-d);return Math.Exp(-d*d/(2*width*width));}
 static double Frac(double x){return x-Math.Floor(x);}
 public static Color Sample(int mode,Color basis,double seconds,int speed,int device,EffectOptions o){
  Color c;
  if(mode==16){
   double t=seconds/(40-6*Math.Max(1,Math.Min(5,speed))),position=device*.25*o.Spread/100.0;
   double phase=Frac(t*2*o.SparkSpeed/50.0+(o.Reverse?position:-position));
   double charge=0,width=.03+o.Width*.0011;
   for(int i=0;i<o.Density;i++){
    double center=Frac(.24+(double)i/o.Density);
    double head=Pulse(phase,center,width);
    double tail=Pulse(phase,Frac(center+.10*o.Dissipation/100.0),width*(1+o.Dissipation/100.0));
    charge+=.85/(1+i*.5)*Math.Max(head,tail*o.Dissipation/100.0);
   }
   charge=Math.Min(1,charge);
   double ambient=(.5+.5*Math.Sin((t*3*o.GridSpeed/50.0+position)*2*Math.PI))*.55;
   c=Mix(Mix(ColorOf(o.Background),ColorOf(o.Grid),ambient),ColorOf(o.Foreground),charge);
  }else{
   c=EffectLibrary.Sample(mode,basis,seconds,speed,device);
   if(o.CustomPalette){
    double l=Math.Max(c.R,Math.Max(c.G,c.B))/255.0;
    double h=c.GetHue()/360.0;
    c=Mix(ColorOf(o.Background),Mix(ColorOf(o.Foreground),ColorOf(o.Grid),h),l);
   }
  }
  double gray=.2126*c.R+.7152*c.G+.0722*c.B;
  c=Mix(Color.FromArgb((int)gray,(int)gray,(int)gray),c,o.Saturation/100.0);
  return Mix(Color.Black,c,o.Intensity/100.0);
 }
}











