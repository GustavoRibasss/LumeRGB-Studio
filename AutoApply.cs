using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
partial class ThebestRGB {
 bool autoApply,autoApplySending;Timer autoApplyTimer;
 string AutoApplyFile{get{return Path.Combine(Path.GetDirectoryName(ProfileStore.FileName),"auto-apply.txt");}}
 void InitializeAutoApply(){
  try{autoApply=File.Exists(AutoApplyFile)&&File.ReadAllText(AutoApplyFile)=="1";}catch{}
  Action refresh=delegate{testSelected.Text="Aplicação automática";((AutoApplyButton)testSelected).IsOn=autoApply;testSelected.BackColor=surface;testSelected.AccessibleDescription=autoApply?"Ativada":"Desativada";testSelected.Invalidate();};
  help.SetToolTip(testSelected,"Quando ligado, escolher uma cor, efeito ou predefinição aplica aos dispositivos selecionados.");
  testSelected.Click+=delegate{autoApply=!autoApply;refresh();if(!autoApply)autoApplyTimer.Stop();try{Directory.CreateDirectory(Path.GetDirectoryName(AutoApplyFile));File.WriteAllText(AutoApplyFile,autoApply?"1":"0");}catch(Exception ex){status.Text="Não foi possível salvar a preferência: "+ex.Message;}};
  autoApplyTimer=new Timer{Interval=70};autoApplyTimer.Tick+=async delegate{if(busy||autoApplySending||editorOpen)return;autoApplyTimer.Stop();if(!autoApply||restoringSetup||startupApplying)return;autoApplySending=true;try{await ApplyCards(cards.Where(c=>c.State.Included).ToList());}catch(Exception ex){status.Text="Falha na aplicação automática: "+ex.Message;}finally{autoApplySending=false;}};
  effectMode.SelectedIndexChanged+=delegate{if(!editorOpen)QueueAutoApply();};
  FormClosed+=delegate{autoApplyTimer.Dispose();};refresh();
 }
 void QueueAutoApply(){if(!autoApply||autoApplyTimer==null||restoringSetup||startupApplying||editorOpen)return;autoApplyTimer.Stop();autoApplyTimer.Start();}
}
