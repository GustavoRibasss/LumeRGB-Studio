// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Drawing;
using System.Windows.Forms;

// Compact dark window chrome keeps the studio consistent with its dark UI even
// when the Windows desktop is using a light theme.
partial class LumeStudio {
 Panel windowChrome;Label windowTitle;Button windowMinimize,windowMaximize,windowClose;
 const int WmNcHitTest=0x84,HitLeft=10,HitRight=11,HitTop=12,HitTopLeft=13,HitTopRight=14,HitBottom=15,HitBottomLeft=16,HitBottomRight=17;
 void InitializeWindowChrome(){
  FormBorderStyle=FormBorderStyle.None;
  windowChrome=new Panel{Dock=DockStyle.None,Height=24,BackColor=Color.FromArgb(17,18,24),Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right};main.Controls.Add(windowChrome);windowChrome.BringToFront();
  windowTitle=new Label{Text="ThebestRGB / Studio 22",ForeColor=Color.FromArgb(205,207,220),Font=new Font("Segoe UI",8),AutoSize=false,TextAlign=ContentAlignment.MiddleLeft};windowChrome.Controls.Add(windowTitle);
  windowMinimize=ChromeButton("—",Color.FromArgb(25,27,35));windowMaximize=ChromeButton("□",Color.FromArgb(25,27,35));windowClose=ChromeButton("×",Color.FromArgb(52,29,39));windowChrome.Controls.Add(windowMinimize);windowChrome.Controls.Add(windowMaximize);windowChrome.Controls.Add(windowClose);
  windowMinimize.Click+=delegate{WindowState=FormWindowState.Minimized;};windowMaximize.Click+=delegate{WindowState=WindowState==FormWindowState.Maximized?FormWindowState.Normal:FormWindowState.Maximized;LayoutWindowChrome();};windowClose.Click+=delegate{Close();};
  windowChrome.MouseDown+=delegate(object s,MouseEventArgs e){DragWindow(e);};windowTitle.MouseDown+=delegate(object s,MouseEventArgs e){DragWindow(e);};Resize+=delegate{LayoutWindowChrome();};Shown+=delegate{windowChrome.BringToFront();};LayoutWindowChrome();
  if(ClientSize.Height<760){int target=Math.Min(800,Screen.PrimaryScreen.WorkingArea.Height-38);if(target>ClientSize.Height)ClientSize=new Size(ClientSize.Width,target);}
 }
 Button ChromeButton(string text,Color color){var b=Button(text,color);b.Font=new Font("Segoe UI",10);b.ForeColor=Color.FromArgb(225,226,235);b.TabStop=false;return b;}
 void LayoutWindowChrome(){if(windowChrome==null)return;windowChrome.SetBounds(0,0,main.Width,24);windowChrome.BringToFront();int width=44;windowTitle.SetBounds(12,0,Math.Max(100,windowChrome.Width-width*3-20),windowChrome.Height);windowMinimize.SetBounds(windowChrome.Width-width*3,2,width-2,windowChrome.Height-4);windowMaximize.SetBounds(windowChrome.Width-width*2,2,width-2,windowChrome.Height-4);windowClose.SetBounds(windowChrome.Width-width,2,width-2,windowChrome.Height-4);windowMaximize.Text=WindowState==FormWindowState.Maximized?"❐":"□";}
 void DragWindow(MouseEventArgs e){if(e.Button!=MouseButtons.Left||WindowState==FormWindowState.Maximized)return;ReleaseCapture();SendMessage(Handle,0xA1,(IntPtr)2,IntPtr.Zero);}
 [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool ReleaseCapture();
 [System.Runtime.InteropServices.DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr hwnd,int msg,IntPtr wParam,IntPtr lParam);
 protected override void WndProc(ref Message message){if(message.Msg==WmNcHitTest&&WindowState==FormWindowState.Normal){Point p=PointToClient(Cursor.Position);int grip=6;bool left=p.X<=grip,right=p.X>=Width-grip,top=p.Y<=grip,bottom=p.Y>=Height-grip;if(left&&top){message.Result=(IntPtr)HitTopLeft;return;}if(right&&top){message.Result=(IntPtr)HitTopRight;return;}if(left&&bottom){message.Result=(IntPtr)HitBottomLeft;return;}if(right&&bottom){message.Result=(IntPtr)HitBottomRight;return;}if(left){message.Result=(IntPtr)HitLeft;return;}if(right){message.Result=(IntPtr)HitRight;return;}if(top){message.Result=(IntPtr)HitTop;return;}if(bottom){message.Result=(IntPtr)HitBottom;return;}}base.WndProc(ref message);}
}
