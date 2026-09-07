// Read-only discovery of devices exposed by a running OpenRGB SDK server.
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

class OpenRgbDevicesForm:Form {
 ListBox devices;Label status;Button refresh,close;
 public OpenRgbDevicesForm(){
  Text="Lume / Dispositivos ARGB compatíveis";ClientSize=new Size(660,410);MinimumSize=ClientSize;StartPosition=FormStartPosition.CenterParent;BackColor=Color.FromArgb(11,12,17);ForeColor=Color.White;Font=new Font("Segoe UI",9);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;ShowInTaskbar=false;
  Controls.Add(new Label{Text="Dispositivos encontrados pelo OpenRGB",Font=new Font("Segoe UI",14,FontStyle.Bold),Location=new Point(24,28),AutoSize=true});
  Controls.Add(new Label{Text="A busca consulta apenas nomes, canais e LEDs. Nenhuma cor é enviada durante a detecção.",ForeColor=LumeStudio.Muted,Location=new Point(24,60),AutoSize=true});
  devices=new ListBox{Location=new Point(24,112),Size=new Size(612,200),BackColor=Color.FromArgb(25,27,35),ForeColor=Color.White,BorderStyle=BorderStyle.FixedSingle,IntegralHeight=false};Controls.Add(devices);
  status=new Label{Text="Pronto para verificar.",ForeColor=LumeStudio.Muted,Location=new Point(24,330),Size=new Size(612,22)};Controls.Add(status);
  refresh=LumeStudio.Button("Verificar novamente",Color.FromArgb(42,48,64));refresh.SetBounds(24,358,160,34);refresh.Click+=delegate{Scan();};Controls.Add(refresh);
  close=LumeStudio.Button("Fechar",Color.FromArgb(151,119,246));close.ForeColor=Color.Black;close.SetBounds(496,358,140,34);close.DialogResult=DialogResult.Cancel;Controls.Add(close);CancelButton=close;
  DarkDialogChrome.Attach(this,Text);Shown+=delegate{Scan();};
 }
 async void Scan(){if(!refresh.Enabled)return;refresh.Enabled=false;devices.Items.Clear();status.Text="Consultando o servidor OpenRGB…";try{var found=await Task.Run(()=>RamBridge.StartGeneric());if(found.Count==0){devices.Items.Add("Nenhum dispositivo encontrado.");status.Text="Nenhum dispositivo ARGB foi exposto pelo OpenRGB.";}else{foreach(var d in found)devices.Items.Add(d.name+"  •  "+(string.IsNullOrEmpty(d.location)?"local não informado":d.location)+"  •  "+d.leds+" LEDs"+(d.direct?"  •  Direct":""));status.Text=found.Count+" dispositivo(s) encontrado(s). Detecção concluída sem alterar cores.";}}catch(Exception ex){devices.Items.Add("Nenhum dispositivo ARGB encontrado.");status.Text="Não foi possível iniciar a detecção: "+ex.Message;}finally{refresh.Enabled=true;}}
}
