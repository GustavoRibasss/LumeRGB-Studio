using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Forms;

class BackupLocation {public string Folder{get;set;}}
static class LocalBackup {
 static string DataFolder {get{return Path.GetDirectoryName(ProfileStore.FileName);}}
 static string SettingsFile {get{return Path.Combine(DataFolder,"backup-location.json");}}
 static string folder;static bool loaded;
 static bool IsNormalInstallation {get{return string.Equals(ProfileStore.FileName,Path.Combine(BrandMigration.DataDirectory(),"profiles.json"),StringComparison.OrdinalIgnoreCase);}}
 static void Load(){if(loaded)return;loaded=true;try{if(File.Exists(SettingsFile)){var setting=new JavaScriptSerializer().Deserialize<BackupLocation>(File.ReadAllText(SettingsFile));if(setting!=null&&!string.IsNullOrWhiteSpace(setting.Folder))folder=setting.Folder;}}catch{folder=null;}}
 public static void Initialize(Form owner){Load();if(!IsNormalInstallation||!string.IsNullOrEmpty(folder))return;using(var welcome=new BackupWelcomeForm()){if(welcome.ShowDialog(owner)!=DialogResult.OK)return;folder=welcome.Folder;try{Directory.CreateDirectory(DataFolder);File.WriteAllText(SettingsFile,new JavaScriptSerializer().Serialize(new BackupLocation{Folder=folder}));}catch{folder=null;return;}BackupFiles();}}
 public static void BackupFiles(){Load();if(string.IsNullOrEmpty(folder))return;try{string target=Path.Combine(folder,"ThebestRGB Backup");Directory.CreateDirectory(target);foreach(string source in Directory.GetFiles(DataFolder,"*.*").Where(f=>{string x=Path.GetExtension(f);return x==".json"||x==".txt";}).Where(f=>!string.Equals(f,SettingsFile,StringComparison.OrdinalIgnoreCase)))File.Copy(source,Path.Combine(target,Path.GetFileName(source)),true);}catch{}}
 public static void BackupSetup(SetupSnapshot setup){Load();if(string.IsNullOrEmpty(folder)||setup==null)return;try{string target=Path.Combine(folder,"ThebestRGB Backup");Directory.CreateDirectory(target);File.WriteAllText(Path.Combine(target,"configuracao-atual.json"),new JavaScriptSerializer().Serialize(setup));BackupFiles();}catch{}}
}
class BackupWelcomeForm:Form {
 public string Folder{get;private set;}
 public BackupWelcomeForm(){Text="Bem-vindo ao ThebestRGB";ClientSize=new Size(640,414);MinimumSize=MaximumSize=ClientSize;StartPosition=FormStartPosition.CenterParent;BackColor=Color.FromArgb(17,18,25);ForeColor=Color.White;Font=new Font("Segoe UI",10);MaximizeBox=false;MinimizeBox=false;
  var hero=new Panel{BackColor=Color.FromArgb(32,27,52),Location=new Point(0,0),Size=new Size(640,124)};hero.Paint+=delegate(object sender,PaintEventArgs e){using(var glow=new SolidBrush(Color.FromArgb(65,151,119,246)))e.Graphics.FillEllipse(glow,466,-105,245,245);using(var ring=new Pen(Color.FromArgb(190,174,145,255),2))e.Graphics.DrawEllipse(ring,25,29,52,52);using(var shield=new SolidBrush(Color.FromArgb(226,218,255)))e.Graphics.FillPolygon(shield,new[]{new Point(51,39),new Point(65,44),new Point(63,61),new Point(51,72),new Point(39,61),new Point(37,44)});};Controls.Add(hero);
  hero.Controls.Add(new Label{Text="ThebestRGB",Font=new Font("Segoe UI",21,FontStyle.Bold),AutoSize=true,Location=new Point(98,27)});
  hero.Controls.Add(new Label{Text="Seu setup, protegido desde o primeiro ajuste.",ForeColor=Color.FromArgb(213,205,237),AutoSize=true,Location=new Point(100,63),Font=new Font("Segoe UI",10)});
  hero.Controls.Add(new Label{Text="CONFIGURAÇÃO INICIAL",ForeColor=Color.FromArgb(190,174,245),AutoSize=true,Location=new Point(100,88),Font=new Font("Segoe UI",7,FontStyle.Bold)});
  Controls.Add(new Label{Text="Escolha uma pasta para o backup local",Font=new Font("Segoe UI",14,FontStyle.Bold),AutoSize=true,Location=new Point(28,148)});
  Controls.Add(new Label{Text="Nada é enviado para a internet. O backup guarda suas configurações neste computador.",ForeColor=ThebestRGB.Muted,AutoSize=true,Location=new Point(30,176),Font=new Font("Segoe UI",9)});
  var cards=new[]{new[]{"01","Local","Perfis, cores e calibração ficam somente no seu PC."},new[]{"02","Automático","Cada alteração atualiza uma cópia silenciosa."},new[]{"03","Sob seu controle","Você escolhe a pasta do backup."}};
  for(int i=0;i<cards.Length;i++){int index=i;var card=new Panel{BackColor=Color.FromArgb(27,29,39),Location=new Point(28+i*196,210),Size=new Size(180,82)};card.Paint+=delegate(object sender,PaintEventArgs e){using(var p=new Pen(Color.FromArgb(55,57,73)))e.Graphics.DrawRectangle(p,0,0,card.Width-1,card.Height-1);};card.Controls.Add(new Label{Text=cards[index][0],ForeColor=Color.FromArgb(173,143,255),Location=new Point(12,10),AutoSize=true,Font=new Font("Segoe UI",7,FontStyle.Bold)});card.Controls.Add(new Label{Text=cards[index][1],Location=new Point(12,28),AutoSize=true,Font=new Font("Segoe UI",9,FontStyle.Bold)});card.Controls.Add(new Label{Text=cards[index][2],ForeColor=ThebestRGB.Muted,Location=new Point(12,49),Size=new Size(155,28),Font=new Font("Segoe UI",7)});Controls.Add(card);}
  var folderFrame=new Panel{BackColor=Color.FromArgb(28,30,40),Location=new Point(28,310),Size=new Size(390,42)};folderFrame.Paint+=delegate(object sender,PaintEventArgs e){using(var p=new Pen(Color.FromArgb(78,72,103)))e.Graphics.DrawRectangle(p,0,0,folderFrame.Width-1,folderFrame.Height-1);};var folderLabel=new Label{Text="Nenhuma pasta selecionada",ForeColor=ThebestRGB.Muted,AutoEllipsis=true,Location=new Point(12,11),Size=new Size(364,20),Font=new Font("Segoe UI",8)};folderFrame.Controls.Add(folderLabel);Controls.Add(folderFrame);
  var choose=ThebestRGB.Button("Escolher pasta",Color.FromArgb(46,49,64));choose.SetBounds(432,310,180,42);Controls.Add(choose);var activate=ThebestRGB.Button("Ativar backup local",Color.FromArgb(151,119,246));activate.ForeColor=Color.FromArgb(18,13,30);activate.Font=new Font("Segoe UI",9,FontStyle.Bold);activate.Enabled=false;activate.SetBounds(402,366,210,34);Controls.Add(activate);choose.Click+=delegate{using(var picker=new FolderBrowserDialog{Description="Escolha uma pasta para o backup local do ThebestRGB",ShowNewFolderButton=true})if(picker.ShowDialog(this)==DialogResult.OK){Folder=picker.SelectedPath;folderLabel.Text=Folder;folderLabel.ForeColor=Color.FromArgb(220,222,232);activate.Enabled=true;}};activate.Click+=delegate{DialogResult=DialogResult.OK;Close();};
  var later=ThebestRGB.Button("Agora não",Color.FromArgb(32,34,45));later.SetBounds(28,366,110,34);later.DialogResult=DialogResult.Cancel;Controls.Add(later);Controls.Add(new Label{Text="Você poderá escolher depois.",ForeColor=ThebestRGB.Muted,Location=new Point(150,374),Size=new Size(235,20),Font=new Font("Segoe UI",8)});CancelButton=later;DarkDialogChrome.Attach(this,Text);
 }
}
