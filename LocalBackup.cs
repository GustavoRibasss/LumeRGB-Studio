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
 public BackupWelcomeForm(){Text="Bem-vindo ao ThebestRGB";ClientSize=new Size(510,252);MinimumSize=MaximumSize=ClientSize;StartPosition=FormStartPosition.CenterParent;BackColor=Color.FromArgb(20,21,27);ForeColor=Color.White;Font=new Font("Segoe UI",10);MaximizeBox=false;MinimizeBox=false;
  Controls.Add(new Label{Text="Bem-vindo ao ThebestRGB",Font=new Font("Segoe UI",18,FontStyle.Bold),AutoSize=true,Location=new Point(24,20)});
  Controls.Add(new Label{Text="Antes de começar, escolha onde guardar seu backup local.",ForeColor=ThebestRGB.Muted,AutoSize=true,Location=new Point(26,57)});
  var note=new Panel{BackColor=Color.FromArgb(31,33,43),Location=new Point(24,88),Size=new Size(462,72)};note.Paint+=delegate(object sender,PaintEventArgs e){using(var p=new Pen(Color.FromArgb(72,68,91)))e.Graphics.DrawRectangle(p,0,0,note.Width-1,note.Height-1);};note.Controls.Add(new Label{Text="Suas configurações ficam somente no seu computador.\nDepois desta escolha, o ThebestRGB cria cópias silenciosas\nsempre que você alterar o setup.",ForeColor=Color.FromArgb(220,222,232),Location=new Point(14,12),Size=new Size(430,52),Font=new Font("Segoe UI",9)});Controls.Add(note);
  var choose=ThebestRGB.Button("Escolher pasta de backup",Color.FromArgb(151,119,246));choose.ForeColor=Color.FromArgb(18,13,30);choose.Font=new Font("Segoe UI",9,FontStyle.Bold);choose.SetBounds(258,186,228,36);Controls.Add(choose);choose.Click+=delegate{using(var picker=new FolderBrowserDialog{Description="Escolha uma pasta para o backup local do ThebestRGB",ShowNewFolderButton=true})if(picker.ShowDialog(this)==DialogResult.OK){Folder=picker.SelectedPath;DialogResult=DialogResult.OK;Close();}};
  var later=ThebestRGB.Button("Agora não",Color.FromArgb(44,48,63));later.SetBounds(24,186,130,36);later.DialogResult=DialogResult.Cancel;Controls.Add(later);CancelButton=later;DarkDialogChrome.Attach(this,Text);
 }
}
