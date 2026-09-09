using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web.Script.Serialization;
partial class ThebestRGB {
 string startupRestorePath;
 bool startupApplying;
 int applyRevision;
 void StartupLog(string message){try{Directory.CreateDirectory(Path.GetDirectoryName(LastConfigurationFile));File.AppendAllText(Path.Combine(Path.GetDirectoryName(LastConfigurationFile),"startup.log"),DateTime.Now.ToString("s")+" "+message+Environment.NewLine);}catch{}}
 string LastConfigurationFile {get{return Path.Combine(Path.GetDirectoryName(ProfileStore.FileName),"last-configuration.json");}}
 void SaveLastConfiguration(){
  applyRevision++;
  if(startupApplying)return;
  try {
   Directory.CreateDirectory(Path.GetDirectoryName(LastConfigurationFile));
   string temp=LastConfigurationFile+".tmp";
   File.WriteAllText(temp,new JavaScriptSerializer().Serialize(CaptureSetup()));
   if(File.Exists(LastConfigurationFile))File.Replace(temp,LastConfigurationFile,null);else File.Move(temp,LastConfigurationFile);
  } catch(Exception ex){status.Text="Não foi possível salvar a configuração de início: "+ex.Message;}
 }
 async Task ApplyLastConfigurationOnStartup(){
  int revision=applyRevision;
  StartupLog("Inicialização iniciada");
  var ramReady=Task.Run(()=>{try{RamBridge.Start();}catch(Exception ex){StartupLog("Acesso à RAM: "+ex.Message);}});
  for(int wait=0;wait<60&&(busy||editorOpen);wait++)await Task.Delay(1000);
  if(IsDisposed||busy||editorOpen||revision!=applyRevision||!File.Exists(LastConfigurationFile)){StartupLog("Restauração adiada ou sem arquivo salvo");return;}
  try{
   startupRestorePath=LastConfigurationFile;
   ReadRestorePoint();
   RestoreSavedSetup();
   if(!status.Text.StartsWith("Estado programado restaurado"))throw new IOException(status.Text);
  }catch(Exception ex){status.Text="Configuração de início inválida: "+ex.Message;StartupLog(status.Text);return;}
  finally{startupRestorePath=null;}
  for(int attempt=0;attempt<3;attempt++){
   if(IsDisposed||busy||editorOpen)return;
   startupApplying=true;
   try{await ApplyCards(cards.Where(c=>c.State.Included).ToList());}
   catch(Exception ex){status.Text="Falha ao iniciar iluminação: "+ex.Message;}
   finally{startupApplying=false;}
   StartupLog("Tentativa "+(attempt+1)+": "+status.Text);
   if(applySucceeded){if(effectTask.IsCompleted)await VerifyDevices();return;}
   revision=applyRevision;
   if(attempt==0)await ramReady;else await Task.Delay(1500);
   if(IsDisposed||revision!=applyRevision)return;
  }
 }
}
