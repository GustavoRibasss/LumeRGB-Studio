using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

class DarkScrollPanel:Panel {
 bool hiding;
 [DllImport("user32.dll")]static extern bool ShowScrollBar(IntPtr handle,int bar,bool show);
 public DarkScrollPanel(){DoubleBuffered=true;SetStyle(ControlStyles.ResizeRedraw,true);}
 protected override void WndProc(ref Message m){base.WndProc(ref m);if(!hiding&&IsHandleCreated&&(m.Msg==0x85||m.Msg==0x5||m.Msg==0x47)){hiding=true;try{ShowScrollBar(Handle,3,false);}finally{hiding=false;}}}
}
class SlimScrollBar:Control {
 public int ContentHeight,Viewport,Offset;public Action<int> MoveTo;bool dragging;int grab;
 public SlimScrollBar(){DoubleBuffered=true;Cursor=Cursors.Hand;SetStyle(ControlStyles.ResizeRedraw,true);}
 int ThumbHeight{get{return Math.Min(Height,Math.Max(30,Height*Viewport/Math.Max(1,ContentHeight)));}}
 int ThumbTop{get{return (Height-ThumbHeight)*Offset/Math.Max(1,ContentHeight-Viewport);}}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(BackColor);using(var b=new SolidBrush(dragging?Color.FromArgb(151,119,246):Color.FromArgb(75,77,92)))e.Graphics.FillRectangle(b,3,ThumbTop,Math.Max(3,Width-6),ThumbHeight);}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);dragging=true;Capture=true;grab=e.Y>=ThumbTop&&e.Y<=ThumbTop+ThumbHeight?e.Y-ThumbTop:ThumbHeight/2;ScrollTo(e.Y);}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(dragging)ScrollTo(e.Y);}
 protected override void OnMouseUp(MouseEventArgs e){dragging=false;Capture=false;Invalidate();base.OnMouseUp(e);}
 void ScrollTo(int y){int travel=Math.Max(1,Height-ThumbHeight);int value=Math.Max(0,Math.Min(travel,y-grab));if(MoveTo!=null)MoveTo(value*Math.Max(0,ContentHeight-Viewport)/travel);}
}
partial class ThebestRGB {
 SlimScrollBar slimScroll;
 void InitializeScrollBar(){slimScroll=new SlimScrollBar{BackColor=main.BackColor};Controls.Add(slimScroll);slimScroll.MoveTo=delegate(int offset){main.AutoScrollPosition=new Point(0,offset);UpdateScrollBar();};Shown+=delegate{UpdateScrollBar();};main.SizeChanged+=delegate{if(IsHandleCreated)BeginInvoke(new Action(UpdateScrollBar));};main.Scroll+=delegate{UpdateScrollBar();};main.MouseWheel+=delegate{BeginInvoke(new Action(UpdateScrollBar));};}
 void UpdateScrollBar(){if(slimScroll==null)return;slimScroll.ContentHeight=main.AutoScrollMinSize.Height;slimScroll.Viewport=main.ClientSize.Height;slimScroll.Offset=-main.AutoScrollPosition.Y;slimScroll.SetBounds(main.Right-U(12),main.Top+U(26),U(10),Math.Max(40,main.Height-U(30)));slimScroll.Visible=slimScroll.ContentHeight>slimScroll.Viewport;slimScroll.BringToFront();slimScroll.Invalidate();}
}
