using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
class AutoApplyButton:StudioButton {
 protected override Rectangle TextBounds {get{return new Rectangle(8,0,Width-62,Height-1);}}
 public bool IsOn;
 public AutoApplyButton(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);Cursor=Cursors.Hand;}
 protected override void OnPaint(PaintEventArgs e){
  base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
  int h=18,w=36,x=Width-w-10,y=(Height-h)/2;
  using(var shape=new GraphicsPath()){shape.AddArc(x,y,h,h,90,180);shape.AddArc(x+w-h,y,h,h,270,180);shape.CloseFigure();using(var brush=new SolidBrush(IsOn?Color.FromArgb(151,119,246):Color.FromArgb(65,67,80)))g.FillPath(brush,shape);}
  using(var brush=new SolidBrush(Color.White))g.FillEllipse(brush,IsOn?x+w-h+3:x+3,y+3,h-6,h-6);
  if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(3,3,Width-6,Height-6));
 }
}
