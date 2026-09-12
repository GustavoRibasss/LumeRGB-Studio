// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Drawing;
public static class EffectLibrary {
 public static bool IsMainEffect(int mode){return mode==0||mode==1||mode==2||mode==3||mode==4||mode==16||mode==17||mode==18;}
 public static readonly string[] Names={"Cor fixa","Arco-íris","Respiração","Rainbow","Onda pelo setup","Passagem de luz","Aurora","Oceano","Fogo","Pôr do sol","Neon","Pastel","Batimento","Vela","Respiração alternada","Duas cores","Elétrica","Áudio","Ambilight"};
 public static readonly string[] Descriptions={
 "Mantém a cor escolhida em cada dispositivo.",
 "Percorre o espectro de cores em todo o setup.",
 "Aumenta e diminui suavemente a cor escolhida.",
 "Mostra várias cores ao mesmo tempo, distribuídas pelo setup.",
 "O arco-íris avança entre teclado, gabinete, RTX e RAM.",
 "Uma faixa de luz percorre os dispositivos na cor escolhida.",
 "Verde, turquesa e violeta com intensidade ondulante.",
 "Azul profundo e ciano em ondas defasadas pelo setup.",
 "Tons de brasa e âmbar com variação orgânica de intensidade.",
 "Violeta, coral e dourado em um ciclo lento.",
 "Rosa e ciano com pulsação suave.",
 "Espectro de cores claras, com branco misturado.",
 "Dois pulsos suaves por ciclo na cor escolhida.",
 "Âmbar quente com oscilação suave, como uma vela.",
 "Os dispositivos respiram em momentos diferentes.",
 "Alterna suavemente a cor escolhida e sua complementar.",
 "Base azul-petróleo com pulsos ciano percorrendo os dispositivos; adaptação sem reação às teclas.",
 "A intensidade acompanha o áudio de saída do Windows.",
 "Ambilight unificado: encontra a cor mais forte do monitor principal e aplica em todo o setup. Velocidade ajusta a suavidade."
 };
 static double Wave(double x){return .5+.5*Math.Sin(x*2*Math.PI);}
 static Color Mix(Color a,Color b,double t){t=Math.Max(0,Math.Min(1,t));return Color.FromArgb((int)Math.Round(a.R+(b.R-a.R)*t),(int)Math.Round(a.G+(b.G-a.G)*t),(int)Math.Round(a.B+(b.B-a.B)*t));}
 static Color Dim(Color c,double x){return Mix(Color.Black,c,x);}
 static double Pulse(double p,double center,double width){double d=Math.Abs(p-center);d=Math.Min(d,1-d);return Math.Exp(-d*d/(2*width*width));}
 static Color Palette(double phase,params Color[] colors){double v=(phase-Math.Floor(phase))*colors.Length;int i=(int)v;double t=v-i;t=t*t*(3-2*t);return Mix(colors[i],colors[(i+1)%colors.Length],t);}
 public static Color Sample(int mode,Color basis,double seconds,int speed,int device){
  if(mode<0||mode>=Names.Length)throw new ArgumentOutOfRangeException("mode");
  if(device<0||device>3)throw new ArgumentOutOfRangeException("device");
if(mode<4){if(mode==3)seconds+=(40-6*Math.Max(1,Math.Min(5,speed)))*device/4.0;return EffectColors.Sample(mode,basis,seconds,speed);}
  if(mode==18)return ScreenColors.ForDevice(device);
  if(mode==17){double level=.15+.85*AudioMeter.GetPeak();return Dim(basis,level);}
  double period=40-6*Math.Max(1,Math.Min(5,speed));
  double t=seconds/period,p=t-Math.Floor(t),offset=device*.25;
  switch(mode){
   case 4:return EffectColors.Sample(1,basis,seconds+period*offset,speed);
   case 5:return Dim(basis,.08+.92*Pulse(p,offset,.14));
   case 6:return Dim(Palette(t+offset*.3,Color.FromArgb(0,255,140),Color.FromArgb(0,210,255),Color.FromArgb(145,45,255)),.65+.35*Wave(t*2+offset));
   case 7:return Dim(Mix(Color.FromArgb(0,25,255),Color.FromArgb(0,240,255),Wave(t+offset)),.45+.55*Wave(t*2+offset));
   case 8:{double heat=.5+.25*Math.Sin(t*19+device*1.7)+.15*Math.Sin(t*37+device*2.3)+.1*Math.Sin(t*61+device);return Dim(Mix(Color.FromArgb(255,20,0),Color.FromArgb(255,170,12),heat),.45+.55*heat);}
   case 9:return Palette(t,Color.FromArgb(125,20,255),Color.FromArgb(255,45,100),Color.FromArgb(255,150,20));
   case 10:return Dim(Mix(Color.FromArgb(255,0,180),Color.Cyan,Wave(t+offset*.5)),.65+.35*Wave(t*2));
   case 11:return Mix(EffectColors.Sample(1,basis,seconds,speed),Color.White,.6);
   case 12:return Dim(basis,Math.Min(1,.08+.8*Pulse(p,.22,.07)+.58*Pulse(p,.43,.08)));
   case 13:return Dim(Color.FromArgb(255,145,35),.75+.14*Math.Sin(t*17+device)+.07*Math.Sin(t*43+device*2));
   case 14:return Dim(basis,.5+.5*Math.Cos((t+offset)*2*Math.PI));
   case 15:{Color complement=Color.FromArgb(255-basis.R,255-basis.G,255-basis.B);return Mix(basis,complement,.5-.5*Math.Cos(t*2*Math.PI));}
   case 16:{
    // Broad continuous pulses, offset across devices, over a lit teal background.
    double flow=t*2-offset*.65;double phase=flow-Math.Floor(flow);
    double charge=Math.Min(1,.85*Pulse(phase,.24,.085)+.55*Pulse(phase,.59,.115));
    Color grid=Mix(Color.FromArgb(0,24,42),Color.FromArgb(0,78,96),Wave(t*3+offset)*.55);
    Color arc=Mix(Color.FromArgb(0,225,255),Color.FromArgb(100,255,255),charge*.5);
    return Mix(grid,arc,charge);
   }
   default:return basis;
  }
 }
}










