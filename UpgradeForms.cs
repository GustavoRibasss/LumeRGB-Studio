// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

static class BarPreviewRenderer {
 public static Color Sample(KeyboardBarSettings s,Color follow,double seconds,double position){
  if(s.Mode<0)return follow;if(s.Mode==0)return Color.Black;double t=seconds*(s.Speed+1)*.35;Color c=s.Multicolor?EffectColors.Sample(1,Color.White,(t+position)*7,5):EffectOptions.ColorOf(s.ColorValue);double gain=(s.Brightness+1)/5.0;
  if(s.Mode==4)gain*=.15+.85*(.5+.5*Math.Cos(t*2*Math.PI));
  if(s.Mode==1||s.Mode==5){double center=t-Math.Floor(t),d=Math.Abs(position-center);d=Math.Min(d,1-d);gain*=.08+.92*Math.Exp(-d*d/(s.Mode==5?.008:.045));}
  if(s.Mode==2)c=EffectColors.Sample(1,c,t*7,5);
  return EffectOptions.Mix(Color.Black,c,gain);
 }
 public static void Paint(Graphics g,RectangleF bounds,KeyboardBarSettings s,Color follow,double time){for(int i=0;i<40;i++){Color c=Sample(s,follow,time,i/40.0);using(var b=new SolidBrush(c))g.FillRectangle(b,bounds.X+i*bounds.Width/40f,bounds.Y,bounds.Width/40f+1,bounds.Height);}}
}
class BarPreviewControl:Control {
 public KeyboardBarSettings Settings;public Color Follow=Color.White;readonly System.Diagnostics.Stopwatch clock=System.Diagnostics.Stopwatch.StartNew();
 public BarPreviewControl(){DoubleBuffered=true;BackColor=Color.FromArgb(12,13,18);}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);if(Settings!=null)BarPreviewRenderer.Paint(e.Graphics,new RectangleF(12,Height/2-4,Math.Max(1,Width-24),8),Settings,Follow,clock.Elapsed.TotalSeconds);}
}
class CompareSetupForm:Form {
 public CompareSetupForm(AppliedDevice[] before,AppliedDevice[] after,string[] names){Text="ThebestRGB / Comparar iluminação";ClientSize=new Size(960,620);BackColor=Color.FromArgb(15,16,21);ForeColor=Color.White;Font=new Font("Segoe UI",9);StartPosition=FormStartPosition.CenterParent;AutoScaleMode=AutoScaleMode.Dpi;Controls.Add(new Label{Text="Última configuração enviada",Location=new Point(24,18),AutoSize=true,Font=new Font(Font,FontStyle.Bold)});Controls.Add(new Label{Text="Novos ajustes",Location=new Point(496,18),AutoSize=true,Font=new Font(Font,FontStyle.Bold)});var arts=new DeviceArt[8];for(int column=0;column<2;column++)for(int i=0;i<4;i++){int index=column*4+i;var state=column==0?before[i]:after[i];Controls.Add(new Label{Text=names[i]+(state==null?" • ainda não aplicado":" • "+EffectLibrary.Names[state.Mode]),Location=new Point(24+column*472,54+i*132),AutoSize=true});arts[index]=new DeviceArt(i){Location=new Point(24+column*472,78+i*132),Size=new Size(436,96),Light=state==null?Color.Black:state.Balance.Apply(state.State.Color),Level=state==null?0:state.State.Brightness,BarPreview=state==null?new KeyboardBarSettings{Mode=0}:state.Bar.Copy()};Controls.Add(arts[index]);}Controls.Add(new Label{Text="Comparação ilustrativa das configurações; não é uma leitura dos LEDs físicos.",AutoSize=true,Location=new Point(24,588),ForeColor=Color.Silver});var clock=System.Diagnostics.Stopwatch.StartNew();var timer=new System.Windows.Forms.Timer{Interval=40};timer.Tick+=delegate{for(int column=0;column<2;column++)for(int i=0;i<4;i++){var state=column==0?before[i]:after[i];if(state==null)continue;var art=arts[column*4+i];art.Light=state.Balance.Apply(EffectOptions.Sample(state.Mode,state.State.Color,clock.Elapsed.TotalSeconds,state.Speed,i,state.Options));art.Invalidate();}};timer.Start();FormClosed+=delegate{timer.Dispose();};DarkDialogChrome.Attach(this,Text);}
}
class CalibrationForm:Form {
 public ColorBalance[] Values;
 public CalibrationForm(string[] names,ColorBalance[] values){Values=values.Select(v=>v.Copy()).ToArray();Text="ThebestRGB / Ajustar cores entre dispositivos";ClientSize=new Size(620,440);BackColor=Color.FromArgb(20,21,27);ForeColor=Color.White;Font=new Font("Segoe UI",9);StartPosition=FormStartPosition.CenterParent;AutoScaleMode=AutoScaleMode.Dpi;Controls.Add(new Label{Text="Reduza os canais que estão fortes demais. 100% mantém a cor original.",AutoSize=true,Location=new Point(22,20)});string[] channels={"Vermelho","Verde","Azul"};for(int i=0;i<4;i++){int device=i;Controls.Add(new Label{Text=names[i],Location=new Point(22,62+i*74),Size=new Size(172,40)});for(int j=0;j<3;j++){int channel=j;Controls.Add(new Label{Text=channels[j],Location=new Point(203+j*132,48+i*74),AutoSize=true});var n=new NumericUpDown{Minimum=0,Maximum=100,Value=j==0?Values[i].R:j==1?Values[i].G:Values[i].B,BackColor=Color.FromArgb(35,36,45),ForeColor=Color.White};n.SetBounds(203+j*132,70+i*74,112,28);n.ValueChanged+=delegate{if(channel==0)Values[device].R=(int)n.Value;else if(channel==1)Values[device].G=(int)n.Value;else Values[device].B=(int)n.Value;};Controls.Add(n);}}var save=ThebestRGB.Button("Salvar correção",Color.FromArgb(151,119,246));save.SetBounds(434,383,162,34);save.DialogResult=DialogResult.OK;Controls.Add(save);AcceptButton=save;var cancel=ThebestRGB.Button("Cancelar",Color.FromArgb(35,36,45));cancel.SetBounds(22,383,130,34);cancel.DialogResult=DialogResult.Cancel;Controls.Add(cancel);CancelButton=cancel;DarkDialogChrome.Attach(this,Text);}
}

