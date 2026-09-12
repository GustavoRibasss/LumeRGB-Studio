// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Drawing;
using System.Windows.Forms;

// Gives every configuration dialog the same dark, compact window header.
static class DarkDialogChrome {
 const int HeaderHeight=28;
 public static void Attach(Form form,string title){
  if(form==null||form.FormBorderStyle==FormBorderStyle.None)return;
  Size initial=form.ClientSize;Size min=form.MinimumSize;bool fixedSize=form.MaximumSize.Width>0&&form.MaximumSize.Height>0;
  Control[] content=new Control[form.Controls.Count];form.Controls.CopyTo(content,0);
  Rectangle[] originalBounds=new Rectangle[content.Length];for(int i=0;i<content.Length;i++)originalBounds[i]=content[i].Bounds;
  form.SuspendLayout();
  form.FormBorderStyle=FormBorderStyle.None;form.MaximumSize=Size.Empty;form.MinimumSize=Size.Empty;form.ClientSize=new Size(initial.Width,initial.Height+HeaderHeight);form.Padding=new Padding(form.Padding.Left,form.Padding.Top+HeaderHeight,form.Padding.Right,form.Padding.Bottom);
  for(int i=0;i<content.Length;i++)if(content[i].Dock==DockStyle.None){var bounds=originalBounds[i];bounds.Y+=HeaderHeight;content[i].Bounds=bounds;}
  form.ResumeLayout(false);
  if(min.Width>0||min.Height>0)form.MinimumSize=new Size(Math.Max(0,min.Width),Math.Max(0,min.Height+HeaderHeight));
  if(fixedSize)form.MaximumSize=form.MinimumSize=form.ClientSize;
  var bar=new Panel{BackColor=Color.FromArgb(17,18,24),Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right};form.Controls.Add(bar);
  var caption=new Label{Text=title,ForeColor=Color.FromArgb(205,207,220),Font=new Font("Segoe UI",8),AutoSize=false,TextAlign=ContentAlignment.MiddleLeft};bar.Controls.Add(caption);
  Button minButton=null,maxButton=null;
  if(form.MinimizeBox)minButton=ChromeButton("—",Color.FromArgb(25,27,35),bar);
  if(form.MaximizeBox)maxButton=ChromeButton("□",Color.FromArgb(25,27,35),bar);
  var closeButton=ChromeButton("×",Color.FromArgb(52,29,39),bar);
  closeButton.Click+=delegate{form.DialogResult=DialogResult.Cancel;form.Close();};
  if(minButton!=null)minButton.Click+=delegate{form.WindowState=FormWindowState.Minimized;};
  if(maxButton!=null)maxButton.Click+=delegate{form.WindowState=form.WindowState==FormWindowState.Maximized?FormWindowState.Normal:FormWindowState.Maximized;Layout(form,bar,caption,minButton,maxButton,closeButton,title);};
  bar.MouseDown+=delegate(object s,MouseEventArgs e){Drag(form,e);};caption.MouseDown+=delegate(object s,MouseEventArgs e){Drag(form,e);};
  form.Resize+=delegate{Layout(form,bar,caption,minButton,maxButton,closeButton,title);};form.Shown+=delegate{ThebestRGB.ApplyDarkTitleBar(form);bar.BringToFront();};Layout(form,bar,caption,minButton,maxButton,closeButton,title);
 }
 static Button ChromeButton(string text,Color color,Control parent){var b=ThebestRGB.Button(text,color);b.Font=new Font("Segoe UI",10);b.ForeColor=Color.FromArgb(225,226,235);b.TabStop=false;parent.Controls.Add(b);return b;}
 static void Layout(Form form,Panel bar,Label caption,Button minButton,Button maxButton,Button closeButton,string title){if(bar==null||bar.IsDisposed)return;bar.SetBounds(0,0,form.ClientSize.Width,HeaderHeight);bar.BringToFront();int right=form.ClientSize.Width;int w=44;if(minButton!=null){minButton.SetBounds(right-w*3,2,w-2,HeaderHeight-4);}if(maxButton!=null){maxButton.SetBounds(right-w*2,2,w-2,HeaderHeight-4);}closeButton.SetBounds(right-w,2,w-2,HeaderHeight-4);int used=(minButton!=null?1:0)+(maxButton!=null?1:0)+1;caption.SetBounds(12,0,Math.Max(100,right-w*used-20),HeaderHeight);if(maxButton!=null)maxButton.Text=form.WindowState==FormWindowState.Maximized?"❐":"□";}
 static void Drag(Form form,MouseEventArgs e){if(e.Button!=MouseButtons.Left||form.WindowState==FormWindowState.Maximized)return;ReleaseCapture();SendMessage(form.Handle,0xA1,(IntPtr)2,IntPtr.Zero);}
 [System.Runtime.InteropServices.DllImport("user32.dll")]static extern bool ReleaseCapture();
 [System.Runtime.InteropServices.DllImport("user32.dll")]static extern IntPtr SendMessage(IntPtr hwnd,int msg,IntPtr wParam,IntPtr lParam);
}
