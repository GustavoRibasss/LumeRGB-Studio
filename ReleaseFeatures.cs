using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

partial class ThebestRGB {
 bool checkingUpdate;
 void InitializeReleaseFeatures(){
  profileList.ItemHeight=U(66);
  help.SetToolTip(resetSetup,"Restaura e aplica seu setup salvo. Para definir o estado, use Ferramentas > Salvar setup atual como restauração.");
  Shown+=async delegate{await CheckForUpdates(false);};
 }
 void DrawProfilePreview(object sender,DrawItemEventArgs e){
  if(e.Index<0||e.Index>=profiles.Count)return;var p=profiles[e.Index];
  using(var brush=new SolidBrush((e.State&DrawItemState.Selected)!=0?Color.FromArgb(48,39,67):side.BackColor))e.Graphics.FillRectangle(brush,e.Bounds);
  var rect=new Rectangle(e.Bounds.X+U(8),e.Bounds.Y+U(4),e.Bounds.Width-U(16),U(21));
  TextRenderer.DrawText(e.Graphics,(p.Favorite?"★ ":"")+p.Name,profileList.Font,rect,Color.White,TextFormatFlags.EndEllipsis);
  rect.Y+=U(22);rect.Height=U(18);TextRenderer.DrawText(e.Graphics,EffectLibrary.Names[p.Mode],profileList.Font,rect,Muted,TextFormatFlags.EndEllipsis);
  for(int i=0;i<p.Devices.Count;i++){var c=EffectOptions.Sample(p.Mode,p.Devices[i].Color,0,p.Speed,i,p.Options[p.Mode]);using(var b=new SolidBrush(c))e.Graphics.FillRectangle(b,e.Bounds.X+U(8+i*29),e.Bounds.Y+U(48),U(23),U(6));}
 }
 async Task RestoreAndApply(){
  if(busy)return;
  if(!File.Exists(RestorePointFile)){status.Text="Prepare seu setup e use Ferramentas > Salvar setup atual como restauração.";return;}
  try{ReadRestorePoint();}catch(Exception ex){status.Text="Estado salvo inválido: "+ex.Message;return;}
  RestoreSavedSetup();
  if(!status.Text.StartsWith("Estado programado restaurado"))return;
  await ApplyCards(cards.Where(c=>c.State.Included).ToList());
 }
 SetupSnapshot ReadRestorePoint(){
  if(new FileInfo(RestorePointFile).Length>4000000)throw new InvalidDataException("Arquivo muito grande.");
  var s=new JavaScriptSerializer().Deserialize<SetupSnapshot>(File.ReadAllText(RestorePointFile));
  if(s!=null&&s.Options!=null&&s.Options.Length>=17&&s.Options.Length<EffectLibrary.Names.Length&&s.Options.All(EffectOptions.Valid))s.Options=s.Options.Concat(Enumerable.Range(s.Options.Length,EffectLibrary.Names.Length-s.Options.Length).Select(i=>new EffectOptions())).ToArray();
  if(s==null||s.Master<0||s.Master>100||s.Devices==null||!ProfileStore.Valid(new LightProfile{Name="Restauração",Bar=s.Bar,Devices=s.Devices.ToList(),Options=s.Options,Mode=s.Mode,Speed=s.Speed,MsiZones=new MsiZoneSettings()}))throw new InvalidDataException("Configuração incompleta ou fora dos limites.");
  return s;
 }
 class ReleaseAsset {public string name{get;set;}public string browser_download_url{get;set;}}
 class ReleaseInfo {public string tag_name{get;set;}public ReleaseAsset[] assets{get;set;}}
 async Task CheckForUpdates(bool manual){
  if(checkingUpdate)return;checkingUpdate=true;
  try{
   ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
   string json;using(var client=new WebClient()){client.Headers[HttpRequestHeader.UserAgent]="ThebestRGB/"+ReleaseVersion;json=await client.DownloadStringTaskAsync("https://api.github.com/repos/GustavoRibasss/ThebestRGB-Studio/releases/latest");}
   if(IsDisposed)return;var release=new JavaScriptSerializer().Deserialize<ReleaseInfo>(json);Version version;
   if(release==null||!Version.TryParse((release.tag_name??"").TrimStart('v'),out version))throw new InvalidDataException("Versão não reconhecida.");
   if(version<=new Version(ReleaseVersion)){if(manual)status.Text="ThebestRGB "+ReleaseVersion+": você está na versão mais recente.";return;}
   var asset=(release.assets??new ReleaseAsset[0]).FirstOrDefault(a=>a.name=="ThebestRGB.exe");
   string prefix="https://github.com/GustavoRibasss/ThebestRGB-Studio/releases/download/";
   if(asset==null||!(asset.browser_download_url??"").StartsWith(prefix,StringComparison.Ordinal))throw new InvalidDataException("Download oficial não encontrado.");
   status.Text="Nova versão disponível: "+version+". Abra Ferramentas > Verificar atualizações para baixar.";
   if(!manual&&tray!=null)tray.ShowBalloonTip(8000,"Atualização do ThebestRGB","Versão "+version+" disponível. Abra Ferramentas > Verificar atualizações.",ToolTipIcon.Info);
   if(manual&&MessageBox.Show(this,"A versão "+version+" está disponível. Abrir o download oficial?","Atualizar ThebestRGB",MessageBoxButtons.YesNo,MessageBoxIcon.Information)==DialogResult.Yes)Process.Start(new ProcessStartInfo(asset.browser_download_url){UseShellExecute=true});
  }catch(Exception ex){if(manual&&!IsDisposed)status.Text="Não foi possível verificar atualizações: "+ex.Message;}finally{checkingUpdate=false;}
 }
}
