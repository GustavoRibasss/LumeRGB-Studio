// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
partial class EffectEditor {
 public Func<EffectOptions,int,Task<string>> ApplyDirect;public bool AppliedDirect;
 Button undoEdit,applyDirect;bool sending,restoringEdit;EffectOptions previousEdit;int previousSpeed;DateTime lastEditorEdit=DateTime.MinValue;
 Stack<Tuple<EffectOptions,int>> editorHistory=new Stack<Tuple<EffectOptions,int>>();
 internal float LayoutScaleOverride=0;int E(int n){return (int)Math.Round(n*(LayoutScaleOverride>0?LayoutScaleOverride:CurrentAutoScaleDimensions.Width/96f));}
 void InitializeEditorFeatures(){
  undoEdit=ThebestRGB.Button("Desfazer ajuste",Color.FromArgb(35,36,45));left.Controls.Add(undoEdit);undoEdit.SetBounds(192,125,150,28);undoEdit.Click+=delegate{if(sending||editorHistory.Count==0)return;var old=editorHistory.Pop();restoringEdit=true;Result=old.Item1.Copy();Speed=old.Item2;BuildOptions();restoringEdit=false;previousEdit=Result.Copy();previousSpeed=Speed;undoEdit.Enabled=editorHistory.Count>0;note.Text="Último ajuste desfeito na prévia.";};
var presets=ThebestRGB.Button("Predefinição",Color.FromArgb(35,36,45));left.Controls.Add(presets);presets.SetBounds(26,125,150,28);presets.Click+=delegate{if(sending)return;var menu=new ContextMenuStrip{Renderer=new StudioMenuRenderer()};foreach(var p in EffectPresets.ForMode(mode)){if(p.Mode!=mode)continue;menu.Items.Add(p.Name,null,delegate{Result=p.Options.Copy();Speed=p.Speed;TrackEditor();BuildOptions();});}if(menu.Items.Count==0)menu.Items.Add("Padrão do efeito",null,delegate{Result=new EffectOptions();Speed=3;TrackEditor();BuildOptions();});menu.Show(presets,new Point(0,presets.Height));};
  applyDirect=ThebestRGB.Button("Aplicar e fechar",Color.FromArgb(151,119,246));bottom.Controls.Add(applyDirect);applyDirect.SetBounds(bottom.Width-198,23,178,34);applyDirect.Anchor=AnchorStyles.Right|AnchorStyles.Top;applyDirect.Click+=async delegate{await ApplyAndClose();};
  FormClosing+=delegate(object sender,FormClosingEventArgs args){if(sending){args.Cancel=true;note.Text="Aguarde a confirmação do envio.";}};
  Shown+=delegate{var area=Screen.FromControl(this).WorkingArea;MinimumSize=new Size(Math.Min(MinimumSize.Width,area.Width),Math.Min(MinimumSize.Height,area.Height));if(Width>area.Width||Height>area.Height)Bounds=area;Arrange();};previousEdit=Result.Copy();previousSpeed=Speed;undoEdit.Enabled=false;
 }
 void TrackEditor(){if(undoEdit==null||restoringEdit)return;if(previousEdit!=null){if((DateTime.UtcNow-lastEditorEdit).TotalMilliseconds>450)editorHistory.Push(Tuple.Create(previousEdit.Copy(),previousSpeed));lastEditorEdit=DateTime.UtcNow;}previousEdit=Result.Copy();previousSpeed=Speed;undoEdit.Enabled=editorHistory.Count>0;}
 async Task ApplyAndClose(){if(sending)return;foreach(var c in colors)if(!c.ValidHex){SelectTab(0);note.Text="Corrija a cor indicada antes de aplicar.";return;}if(ApplyDirect==null){note.Text="Aplicação indisponível nesta prévia.";return;}sending=true;right.Enabled=false;bottom.Enabled=false;undoEdit.Enabled=false;note.Text="Enviando aos dispositivos selecionados…";try{string error=await ApplyDirect(Result.Copy(),Speed);if(error!=null){note.Text="Falha no envio: "+error;return;}sending=false;AppliedDirect=true;DialogResult=DialogResult.OK;Close();}catch(Exception ex){note.Text="Falha no envio: "+ex.Message;}finally{sending=false;if(!IsDisposed){right.Enabled=true;bottom.Enabled=true;undoEdit.Enabled=editorHistory.Count>0;}}}
}













