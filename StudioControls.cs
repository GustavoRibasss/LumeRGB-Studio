// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
class StudioSlider:Control {
 int minimum=0,maximum=100,current=0;
 public int Minimum{get{return minimum;}set{minimum=value;Value=current;}}
 public int Maximum{get{return maximum;}set{maximum=value;Value=current;}}
 public int Value{get{return current;}set{int n=Math.Max(minimum,Math.Min(maximum,value));if(n==current)return;current=n;Invalidate();if(ValueChanged!=null)ValueChanged(this,EventArgs.Empty);}}
 public TickStyle TickStyle{get;set;}
 public event EventHandler ValueChanged;
 public StudioSlider(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);Height=28;TabStop=true;Cursor=Cursors.Hand;AccessibleRole=AccessibleRole.Slider;}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;float y=Height/2f,x=9+(Width-18)*(current-minimum)/(float)Math.Max(1,maximum-minimum);using(var bg=new Pen(Color.FromArgb(47,49,59),4))using(var fg=new Pen(Enabled?Color.FromArgb(167,145,255):Color.FromArgb(76,77,87),4)){bg.StartCap=bg.EndCap=fg.StartCap=fg.EndCap=LineCap.Round;g.DrawLine(bg,9,y,Width-9,y);g.DrawLine(fg,9,y,x,y);}using(var b=new SolidBrush(Enabled?Color.FromArgb(225,217,255):Color.Gray))g.FillEllipse(b,x-5,y-5,10,10);if(Focused)ControlPaint.DrawFocusRectangle(g,ClientRectangle);}
 void SetMouse(int x){Value=minimum+(int)Math.Round(Math.Max(0,Math.Min(1,(x-9)/(double)Math.Max(1,Width-18)))*(maximum-minimum));}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button==MouseButtons.Left){Focus();Capture=true;SetMouse(e.X);}}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(Capture&&e.Button==MouseButtons.Left)SetMouse(e.X);}
 protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);Capture=false;}
 protected override bool IsInputKey(Keys key){return key==Keys.Left||key==Keys.Right||key==Keys.Up||key==Keys.Down||base.IsInputKey(key);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Left||e.KeyCode==Keys.Down){Value--;e.Handled=true;}if(e.KeyCode==Keys.Right||e.KeyCode==Keys.Up){Value++;e.Handled=true;}if(e.KeyCode==Keys.Home)Value=Minimum;if(e.KeyCode==Keys.End)Value=Maximum;}
}
class StudioButton:Button {
 bool hover;
 float hoverBlend;Timer hoverTimer; public StudioButton(){DoubleBuffered=true;} void AnimateHover(){if(hoverTimer==null){hoverTimer=new Timer{Interval=16};hoverTimer.Tick+=delegate{float target=hover?1:0;hoverBlend+=Math.Sign(target-hoverBlend)*Math.Min(.16f,Math.Abs(target-hoverBlend));Invalidate();if(Math.Abs(target-hoverBlend)<.001f)hoverTimer.Stop();};}hoverTimer.Start();} protected override void Dispose(bool disposing){if(disposing&&hoverTimer!=null)hoverTimer.Dispose();base.Dispose(disposing);}
 protected override void OnMouseEnter(EventArgs e){base.OnMouseEnter(e);hover=true;AnimateHover();}
 protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);hover=false;AnimateHover();}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(Parent==null?Color.FromArgb(15,16,20):Parent.BackColor);var rect=new Rectangle(0,0,Width-1,Height-1);using(var path=new GraphicsPath()){int r=12;path.AddArc(rect.Left,rect.Top,r,r,180,90);path.AddArc(rect.Right-r,rect.Top,r,r,270,90);path.AddArc(rect.Right-r,rect.Bottom-r,r,r,0,90);path.AddArc(rect.Left,rect.Bottom-r,r,r,90,90);path.CloseFigure();Color c=Enabled?BackColor:Color.FromArgb(28,29,35);if(Enabled&&hoverBlend>0)c=ControlPaint.Light(c,.12f*hoverBlend);using(var b=new SolidBrush(c))g.FillPath(b,path);using(var p=new Pen(Color.FromArgb(Enabled?50:34,52,63)))g.DrawPath(p,path);}TextRenderer.DrawText(g,Text,Font,rect,Enabled?ForeColor:Color.FromArgb(105,108,122),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(4,4,Width-9,Height-9));}
}
class SetupStage:Panel {
 public SetupStage(){SetStyle(ControlStyles.ResizeRedraw,true);DoubleBuffered=true;BackColor=Color.FromArgb(15,16,21);}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;using(var pen=new Pen(Color.FromArgb(34,36,46)))g.DrawRectangle(pen,0,0,Width-1,Height-1);using(var font=new Font("Segoe UI",8))using(var b=new SolidBrush(Color.FromArgb(137,139,155)))g.DrawString("PRÉVIA DO SETUP",font,b,18,15);using(var font=new Font("Segoe UI",8))using(var b=new SolidBrush(Color.FromArgb(100,104,122)))g.DrawString("Prévia da iluminação • confira nos LEDs",font,b,18,Height-26);}
}

// Keeps the native CheckBox interaction and accessibility, with Studio rendering.
class StudioCheckBox:CheckBox {
 bool hover;
 public StudioCheckBox(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);Cursor=Cursors.Hand;UseVisualStyleBackColor=false;TabStop=true;}
 float ScaleFactor {get{using(var g=CreateGraphics())return g.DpiX/96f;}}
 public override Size GetPreferredSize(Size proposedSize){float s=ScaleFactor;var text=TextRenderer.MeasureText(Text,Font);return new Size(text.Width+(int)Math.Ceiling(30*s)+Padding.Horizontal,Math.Max(text.Height,(int)Math.Ceiling(18*s))+(int)Math.Ceiling(6*s)+Padding.Vertical);}
 protected override void OnMouseEnter(EventArgs e){base.OnMouseEnter(e);hover=true;Invalidate();}
 protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);hover=false;Invalidate();}
 protected override void OnCheckedChanged(EventArgs e){base.OnCheckedChanged(e);Invalidate();}
 protected override void OnEnabledChanged(EventArgs e){base.OnEnabledChanged(e);Invalidate();}
 protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();}
 protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);Invalidate();}
 protected override void OnPaint(PaintEventArgs e){
  var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(BackColor);
  float s=g.DpiX/96f,edge=17*s,x=Padding.Left+2*s,y=(Height-edge)/2f;
  var box=new RectangleF(x,y,edge,edge);float diameter=7*s;
  using(var path=new GraphicsPath()){
   path.AddArc(box.Left,box.Top,diameter,diameter,180,90);path.AddArc(box.Right-diameter,box.Top,diameter,diameter,270,90);path.AddArc(box.Right-diameter,box.Bottom-diameter,diameter,diameter,0,90);path.AddArc(box.Left,box.Bottom-diameter,diameter,diameter,90,90);path.CloseFigure();
   Color fill=Checked?Color.FromArgb(157,126,248):Color.FromArgb(30,32,42);Color border=Checked?fill:Color.FromArgb(111,114,135);
   if(hover&&Enabled){fill=Checked?Color.FromArgb(179,153,255):Color.FromArgb(43,39,59);border=Color.FromArgb(187,162,255);}
   if(!Enabled){fill=Color.FromArgb(49,48,61);border=Color.FromArgb(97,96,113);}
   using(var b=new SolidBrush(fill))g.FillPath(b,path);using(var p=new Pen(border,1.2f*s))g.DrawPath(p,path);
   using(var p=new Pen(Enabled?Color.FromArgb(20,14,36):Color.FromArgb(179,177,193),2*s)){p.StartCap=p.EndCap=LineCap.Round;p.LineJoin=LineJoin.Round;if(CheckState==CheckState.Indeterminate)g.DrawLine(p,x+4*s,y+8.5f*s,x+13*s,y+8.5f*s);else if(Checked)g.DrawLines(p,new[]{new PointF(x+4*s,y+8.5f*s),new PointF(x+7*s,y+11.5f*s),new PointF(x+13*s,y+5.5f*s)});}
  }
  int textLeft=(int)Math.Ceiling(x+edge+9*s);var rect=new Rectangle(textLeft,Padding.Top,Math.Max(0,Width-textLeft-Padding.Right),Height-Padding.Vertical);
  TextRenderer.DrawText(g,Text,Font,rect,Enabled?ForeColor:Color.FromArgb(164,163,177),TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
  if(Focused&&ShowFocusCues){using(var p=new Pen(Color.FromArgb(196,177,255),s)){p.DashStyle=DashStyle.Dot;g.DrawRectangle(p,0,0,Width-1,Height-1);}}
 }
}

class StudioMenuRenderer:ToolStripProfessionalRenderer {
 public StudioMenuRenderer():base(new StudioMenuColors()){RoundedEdges=false;}
 protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e){using(var b=new SolidBrush(Color.FromArgb(19,20,27)))e.Graphics.FillRectangle(b,e.AffectedBounds);}
 protected override void OnRenderImageMargin(ToolStripRenderEventArgs e){using(var b=new SolidBrush(Color.FromArgb(19,20,27)))e.Graphics.FillRectangle(b,e.AffectedBounds);}
 protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e){e.TextColor=Color.White;base.OnRenderItemText(e);}
}
class StudioMenuColors:ProfessionalColorTable {
 public override Color MenuBorder{get{return Color.FromArgb(78,65,112);}}
 public override Color MenuItemBorder{get{return Color.FromArgb(109,86,166);}}
 public override Color MenuItemSelected{get{return Color.FromArgb(67,52,101);}}
 public override Color MenuItemSelectedGradientBegin{get{return Color.FromArgb(67,52,101);}}
 public override Color MenuItemSelectedGradientEnd{get{return Color.FromArgb(67,52,101);}}
 public override Color ToolStripDropDownBackground{get{return Color.FromArgb(19,20,27);}}
 public override Color ImageMarginGradientBegin{get{return Color.FromArgb(19,20,27);}}
 public override Color ImageMarginGradientMiddle{get{return Color.FromArgb(19,20,27);}}
 public override Color ImageMarginGradientEnd{get{return Color.FromArgb(19,20,27);}}
}









