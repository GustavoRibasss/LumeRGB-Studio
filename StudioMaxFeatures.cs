// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

partial class LumeStudio {
 Button resetSetup,testSelected,diagnostic;ToolStripMenuItem themeItem,startupItem;
 Dictionary<int,Color> lastSentColors=new Dictionary<int,Color>();
 Keys saveShortcut=Keys.Control|Keys.S,undoShortcut=Keys.Control|Keys.Z,verifyShortcut=Keys.F5,stopShortcut=Keys.Escape;
 Label shortcutHint;
 void InitializeMaxFeatures(){
  resetSetup=Button("Restaurar setup",surface);resetSetup.Parent=main;resetSetup.Click+=delegate{ResetSetup();};
  testSelected=Button("Testar selecionados",surface);testSelected.Parent=main;testSelected.Click+=async delegate{if(cards.All(c=>!c.State.Included)){status.Text="Selecione ao menos um dispositivo para testar.";return;}await ApplyCards(cards.Where(c=>c.State.Included).ToList());};
  diagnostic=Button("Diagnóstico",surface);diagnostic.Parent=side;diagnostic.Click+=delegate{ExportDiagnostics();};
  shortcutHint=new Label{Text="Ctrl+S salvar  ·  Ctrl+Z desfazer  ·  F5 verificar  ·  Esc parar",ForeColor=Muted,Font=new Font("Segoe UI",8),AutoEllipsis=true};main.Controls.Add(shortcutHint);
  themeItem=new ToolStripMenuItem("Tema claro"){CheckOnClick=true};themeItem.CheckedChanged+=delegate{ApplyTheme(themeItem.Checked);};trayMenu.Items.Add(themeItem);
  startupItem=new ToolStripMenuItem("Iniciar com o Windows"){CheckOnClick=true,Checked=IsStartupEnabled()};startupItem.CheckedChanged+=delegate{SetStartup(startupItem.Checked);};trayMenu.Items.Add(startupItem);
  trayMenu.Items.Add("Configurar atalhos",null,delegate{ConfigureShortcuts();});
  KeyPreview=true;UpdateMaxState();Resize+=delegate{LayoutMaxFeatures();};Shown+=delegate{LayoutMaxFeatures();};
 }
 void LayoutMaxFeatures(){if(designReady){LayoutDesign();return;}if(resetSetup==null)return;int baseX=U(467);resetSetup.SetBounds(baseX,U(91),U(124),U(30));testSelected.SetBounds(baseX+U(132),U(91),U(151),U(30));diagnostic.SetBounds(U(16),U(main.ClientSize.Height<720?598:614),U(148),U(30));shortcutHint.SetBounds(U(24),U(654),Math.Max(U(300),main.ClientSize.Width-U(48)),U(20));shortcutHint.Visible=main.ClientSize.Height>=720;}
 void UpdateMaxState(){if(testSelected==null)return;testSelected.Enabled=!busy&&cards.Any(c=>c.State.Included);resetSetup.Enabled=!busy;diagnostic.Enabled=!busy;}
 void ResetSetup(){if(busy)return;if(lastSetup!=null)undoStates.Push(lastSetup);restoringSetup=true;try{pendingBar=new KeyboardBarSettings();msiZones=new MsiZoneSettings();effectOptions=EffectOptions.Defaults();effectMode.SelectedIndex=0;effectSpeed.Value=3;master.Value=100;global=Color.White;pick.BackColor=global;pick.ForeColor=Color.Black;hex.Text="#FFFFFF";for(int i=0;i<cards.Count;i++){cards[i].State.R=255;cards[i].State.G=255;cards[i].State.B=255;cards[i].State.Brightness=100;cards[i].State.Included=true;cards[i].RefreshState(true);cards[i].Status.Text="Não aplicado";connectionLabels[i].Text="Não verificado";connectionLabels[i].ForeColor=Muted;}}finally{restoringSetup=false;}lastSetup=CaptureSetup();lastEdit=DateTime.MinValue;UpdateSelection();UpdateMaxState();status.Text="Setup restaurado na prévia. Clique em Aplicar iluminação para enviar.";}
 void ExportDiagnostics(){if(busy)return;using(var dialog=new SaveFileDialog{Title="Exportar diagnóstico",Filter="Arquivo de texto (*.txt)|*.txt",FileName="lume-diagnostico.txt",AddExtension=true})if(dialog.ShowDialog(this)==DialogResult.OK){try{var text=new StringBuilder();text.AppendLine("ThebestRGB 22");text.AppendLine("Gerado: "+DateTime.Now.ToString("s"));text.AppendLine("Sistema: "+Environment.OSVersion);text.AppendLine("DPI: "+CurrentAutoScaleDimensions.Width+" x "+CurrentAutoScaleDimensions.Height);text.AppendLine("Modo: "+effectMode.Text+" / velocidade "+effectSpeed.Value);text.AppendLine("Perfis: "+profiles.Count);text.AppendLine("Runtime OpenRGB: "+File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"runtime","OpenRGB.exe")));for(int i=0;i<cards.Count;i++)text.AppendLine(cards[i].State.Name+" | incluído="+cards[i].State.Included+" | conexão="+connectionLabels[i].Text+" | status="+cards[i].Status.Text);File.WriteAllText(dialog.FileName,text.ToString(),Encoding.UTF8);status.Text="Diagnóstico exportado: "+Path.GetFileName(dialog.FileName)+".";}catch(Exception ex){status.Text="Não foi possível exportar o diagnóstico: "+ex.Message;}}}
 protected override bool ProcessCmdKey(ref Message msg,Keys keyData){if(keyData==saveShortcut){SaveProfile();return true;}if(keyData==undoShortcut){UndoSetup();return true;}if(keyData==verifyShortcut){VerifyDevices();return true;}if(keyData==stopShortcut&&!busy){StopEffect();status.Text="Efeito parado.";return true;}return base.ProcessCmdKey(ref msg,keyData);}
 void ConfigureShortcuts(){using(var dialog=new ShortcutSettingsForm(saveShortcut,undoShortcut,verifyShortcut,stopShortcut)){if(dialog.ShowDialog(this)!=DialogResult.OK)return;saveShortcut=dialog.SaveKey;undoShortcut=dialog.UndoKey;verifyShortcut=dialog.VerifyKey;stopShortcut=dialog.StopKey;shortcutHint.Text=ShortcutText();}}
 string ShortcutText(){return FormatKey(saveShortcut)+" salvar  ·  "+FormatKey(undoShortcut)+" desfazer  ·  "+FormatKey(verifyShortcut)+" verificar  ·  "+FormatKey(stopShortcut)+" parar";}
 static string FormatKey(Keys k){return k==Keys.F5?"F5":k==Keys.F6?"F6":k==Keys.Escape?"Esc":k==Keys.F12?"F12":k==(Keys.Control|Keys.S)?"Ctrl+S":k==(Keys.Control|Keys.Shift|Keys.S)?"Ctrl+Shift+S":k==(Keys.Control|Keys.Z)?"Ctrl+Z":k==(Keys.Control|Keys.Y)?"Ctrl+Y":k.ToString();}
 bool IsStartupEnabled(){try{using(var key=Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run",false))return key!=null&&key.GetValue("LumeStudio")!=null;}catch{return false;}}
 void SetStartup(bool enabled){try{using(var key=Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run",true)){if(enabled)key.SetValue("LumeStudio","\""+Application.ExecutablePath+"\" --tray");else key.DeleteValue("LumeStudio",false);}status.Text=enabled?"Inicialização com o Windows ativada.":"Inicialização com o Windows desativada.";}catch(Exception ex){status.Text="Não foi possível alterar a inicialização: "+ex.Message;startupItem.Checked=!enabled;}}
 void ApplyTheme(bool light){Color root=light?Color.FromArgb(242,243,248):Color.FromArgb(10,11,15);Color panel=light?Color.White:Color.FromArgb(20,21,27);Color sideColor=light?Color.FromArgb(232,234,241):Color.FromArgb(14,15,19);Color text=light?Color.FromArgb(35,36,45):Color.White;BackColor=root;main.BackColor=root;side.BackColor=sideColor;hero.BackColor=panel;stage.BackColor=light?Color.FromArgb(235,237,244):Color.FromArgb(15,16,21);footer.BackColor=sideColor;foreach(var card in cards){card.BackColor=panel;card.Slider.BackColor=panel;card.Included.ForeColor=text;card.Percent.ForeColor=text;card.Status.ForeColor=light?Color.FromArgb(82,84,98):Muted;}foreach(Control c in main.Controls)if(c is Label)c.ForeColor=text;foreach(Control c in side.Controls)if(c is Label)c.ForeColor=light?Color.FromArgb(82,84,98):Muted;themeItem.Text=light?"Tema escuro":"Tema claro";status.Text=light?"Tema claro aplicado.":"Tema escuro aplicado.";}
}

class ShortcutSettingsForm:Form {
 public Keys SaveKey,UndoKey,VerifyKey,StopKey;
 ComboBox save,undo,verify,stop;
 public ShortcutSettingsForm(Keys s,Keys u,Keys v,Keys p){SaveKey=s;UndoKey=u;VerifyKey=v;StopKey=p;Text="Configurar atalhos";ClientSize=new Size(380,245);StartPosition=FormStartPosition.CenterParent;BackColor=Color.FromArgb(20,21,27);ForeColor=Color.White;Font=new Font("Segoe UI",9);string[] names={"Salvar perfil","Desfazer","Verificar dispositivos","Parar efeito"};ComboBox[] boxes={save=new ComboBox(),undo=new ComboBox(),verify=new ComboBox(),stop=new ComboBox()};Keys[][] values={new[]{Keys.Control|Keys.S,Keys.Control|Keys.Shift|Keys.S},new[]{Keys.Control|Keys.Z,Keys.Control|Keys.Y},new[]{Keys.F5,Keys.F6},new[]{Keys.Escape,Keys.F12}};Keys[] current={s,u,v,p};for(int i=0;i<4;i++){Controls.Add(new Label{Text=names[i],Location=new Point(18,18+i*40),Width=145});boxes[i].DropDownStyle=ComboBoxStyle.DropDownList;boxes[i].SetBounds(175,14+i*40,170,28);foreach(var key in values[i])boxes[i].Items.Add(LumeStudioKeyName(key));boxes[i].SelectedIndex=Math.Max(0,Array.IndexOf(values[i],current[i]));Controls.Add(boxes[i]);}var ok=LumeStudio.Button("Salvar",Color.FromArgb(151,119,246));ok.SetBounds(247,190,98,34);Controls.Add(ok);var cancel=LumeStudio.Button("Cancelar",Color.FromArgb(35,36,45));cancel.SetBounds(135,190,100,34);cancel.DialogResult=DialogResult.Cancel;Controls.Add(cancel);CancelButton=cancel;ok.Click+=delegate{SaveKey=values[0][save.SelectedIndex];UndoKey=values[1][undo.SelectedIndex];VerifyKey=values[2][verify.SelectedIndex];StopKey=values[3][stop.SelectedIndex];DialogResult=DialogResult.OK;Close();};DarkDialogChrome.Attach(this,Text);}
 static string LumeStudioKeyName(Keys k){return k==Keys.F5?"F5":k==Keys.F6?"F6":k==Keys.Escape?"Esc":k==Keys.F12?"F12":k==(Keys.Control|Keys.Shift|Keys.S)?"Ctrl+Shift+S":k==(Keys.Control|Keys.Y)?"Ctrl+Y":k==(Keys.Control|Keys.Z)?"Ctrl+Z":"Ctrl+S";}
}


