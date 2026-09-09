using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
class ScreenBrightnessSwitch:CheckBox {
 public ScreenBrightnessSwitch(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);UseVisualStyleBackColor=false;Cursor=Cursors.Hand;AccessibleName="Acompanhar luminosidade da tela";}
 protected override void OnCheckedChanged(EventArgs e){base.OnCheckedChanged(e);Invalidate();}
 protected override void OnPaint(PaintEventArgs e){
  var g=e.Graphics;g.Clear(Parent==null?BackColor:Parent.BackColor);g.SmoothingMode=SmoothingMode.AntiAlias;int h=Math.Min(24,Height-12),w=h*2,x=Width-w-2,y=(Height-h)/2;
  using(var path=new GraphicsPath()){path.AddArc(x,y,h,h,90,180);path.AddArc(x+w-h,y,h,h,270,180);path.CloseFigure();using(var b=new SolidBrush(Checked?Color.FromArgb(151,119,246):Color.FromArgb(61,64,77)))g.FillPath(b,path);}
  using(var b=new SolidBrush(Color.White))g.FillEllipse(b,Checked?x+w-h+3:x+3,y+3,h-6,h-6);
  TextRenderer.DrawText(g,"Luminosidade da tela",Font,new Rectangle(0,0,x-8,Height),ForeColor,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
  if(Focused)ControlPaint.DrawFocusRectangle(g,ClientRectangle);
 }
}
partial class ThebestRGB {
 ScreenBrightnessSwitch screenBrightness;bool refreshingScreenBrightness;
 void LayoutScreenControls(){if(screenBrightness==null)return;screenBrightness.Visible=effectMode.SelectedIndex==18;if(!screenBrightness.Visible)return;screenBrightness.SetBounds(all.Left,effectSpeed.Bottom+12,all.Width,38);all.Top=screenBrightness.Bottom+16;effectStop.Top=all.Bottom+9;}
 void InitializeScreenControls(){
  screenBrightness=new ScreenBrightnessSwitch{ForeColor=Color.White,BackColor=hero.BackColor,AutoSize=false};hero.Controls.Add(screenBrightness);help.SetToolTip(screenBrightness,"Ligado: acompanha o brilho da imagem. Desligado: altera apenas as cores.");
  Action refresh=delegate{refreshingScreenBrightness=true;try{screenBrightness.Visible=effectMode.SelectedIndex==18;screenBrightness.Checked=effectOptions[18].FollowScreenBrightness;}finally{refreshingScreenBrightness=false;}};
  screenBrightness.CheckedChanged+=delegate{if(refreshingScreenBrightness)return;effectOptions[18].FollowScreenBrightness=screenBrightness.Checked;TrackSetup();status.Text="Luminosidade ajustada. Clique em Aplicar iluminação.";};
  effectMode.SelectedIndexChanged+=delegate{refresh();LayoutScreenControls();};Activated+=delegate{refresh();LayoutScreenControls();};refresh();LayoutScreenControls();
 }
}
