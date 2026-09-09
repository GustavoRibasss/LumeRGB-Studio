// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public static class EffectColors {
 public static Color Sample(int mode,Color basis,double seconds,int speed){
  double period=40-6*Math.Max(1,Math.Min(5,speed));
  double phase=(seconds%period)/period;
  if(mode==2){
   double gain=(1+Math.Cos(phase*2*Math.PI))/2;
   return Color.FromArgb((int)Math.Round(basis.R*gain),(int)Math.Round(basis.G*gain),(int)Math.Round(basis.B*gain));
  }
  if(mode==3){
   Color[] palette={Color.Red,Color.FromArgb(255,120,0),Color.Yellow,Color.Lime,Color.Cyan,Color.Blue,Color.Magenta};
   double position=phase*7;int i=Math.Min(6,(int)position);double t=position-i;t=t*t*(3-2*t);
   Color a=palette[i],b=palette[(i+1)%7];
   return Color.FromArgb((int)Math.Round(a.R+(b.R-a.R)*t),(int)Math.Round(a.G+(b.G-a.G)*t),(int)Math.Round(a.B+(b.B-a.B)*t));
  }
  if(mode!=1)return basis;
  double h=phase*6;int sector=(int)h;int up=(int)Math.Round((h-sector)*255),down=255-up;
  switch(sector){case 0:return Color.FromArgb(255,up,0);case 1:return Color.FromArgb(down,255,0);case 2:return Color.FromArgb(0,255,up);case 3:return Color.FromArgb(0,down,255);case 4:return Color.FromArgb(up,0,255);default:return Color.FromArgb(255,0,down);}
 }
}
partial class ThebestRGB {
 ComboBox effectMode;StudioSlider effectSpeed;Button effectStop;Label speedText;
 CancellationTokenSource effectCancel;
 Task effectTask=Task.FromResult(0);
 Action<int,Color,int> effectWriter=null;
 bool closingAfterStop;
 void InitializeEffects(){
  hero.Controls.Add(Label("MODO",8,Muted,18,119));
  effectMode=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,BackColor=Color.FromArgb(34,40,55),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};
  effectMode.Items.AddRange(EffectLibrary.Names);effectMode.MaxDropDownItems=16;effectMode.IntegralHeight=false;effectMode.DropDownHeight=330;
  effectMode.SelectedIndex=0;effectMode.SetBounds(18,140,219,26);hero.Controls.Add(effectMode);
  effectMode.SelectedIndexChanged+=delegate{if(effectMode.SelectedIndex>=0)status.Text=EffectLibrary.Descriptions[effectMode.SelectedIndex]+" Clique em Aplicar para usar.";};
  hero.Controls.Add(Label("VELOCIDADE",8,Muted,266,119));
  effectSpeed=new StudioSlider{Minimum=1,Maximum=5,Value=3,TickStyle=TickStyle.None,BackColor=hero.BackColor};
  effectSpeed.SetBounds(256,138,182,30);hero.Controls.Add(effectSpeed);
  speedText=Label("3 / 5",9,Color.White,445,145);hero.Controls.Add(speedText);
  effectSpeed.ValueChanged+=delegate{speedText.Text=effectSpeed.Value+" / 5";};
  effectStop=Button("Parar efeito",Color.FromArgb(44,51,69));effectStop.SetBounds(550,135,150,32);hero.Controls.Add(effectStop);effectStop.Enabled=false;
  effectStop.Click+=async delegate{if(busy)return;SetBusy(true);try{await StopEffect();status.Text="Efeito parado. Os LEDs ficam na última cor enviada.";}finally{SetBusy(false);}};
  FormClosing+=async delegate(object sender,FormClosingEventArgs e){
   if(closingAfterStop)return;if(!exitRequested&&closeToTray&&e.CloseReason==CloseReason.UserClosing&&!editorOpen){e.Cancel=true;Hide();return;}
   if(busy){e.Cancel=true;status.Text="Aguarde o envio terminar para fechar.";return;}
   if(!effectTask.IsCompleted){e.Cancel=true;SetBusy(true);await StopEffect();closingAfterStop=true;Close();}
  };
  FormClosed+=delegate{if(effectCancel!=null){effectCancel.Cancel();effectCancel.Dispose();}};
 }
 async Task StopEffect(){
  if(effectCancel!=null)effectCancel.Cancel();
  await effectTask;FreezeAppliedEffects();
  if(effectCancel!=null){effectCancel.Dispose();effectCancel=null;}
  if(IsDisposed)return;
  effectStop.Enabled=false;activate.Enabled=!busy;
  foreach(var card in cards){card.RefreshState(false);if(card.Status.Text=="Animando")card.Status.Text="Parado";}
 }
 async Task ApplyCards(List<LightCard> targets){
  if(busy)return;
  if(targets.Count==0){status.Text="Selecione pelo menos um dispositivo.";return;}
  SaveLastConfiguration();SetBusy(true);await StopEffect();applySucceeded=false;applyError="";SetBusy(false);
  if(blackoutActive){blackoutActive=false;lightsOffRestore=null;}if(targets.Contains(cards[0])){Hid.BarSettings=pendingBar.Copy();Hid.ActiveBarBalance=Calibration.Values[0].Copy();}if(effectMode.SelectedIndex==0){await ApplyStaticCards(targets);return;}
  int mode=effectMode.SelectedIndex,speed=effectSpeed.Value;
  var states=targets.Select(c=>c.State.Copy()).ToArray();
  var indices=targets.Select(c=>cards.IndexOf(c)).ToArray();
  effectStarted=new TaskCompletionSource<bool>();effectCancel=new CancellationTokenSource();effectStop.Enabled=true;activate.Enabled=false;
  status.Text=effectMode.Text+" ativo. Mantenha o app aberto. Alterações ficam prontas para o próximo Aplicar.";
  effectTask=RunEffect(targets,states,indices,mode,speed,effectCancel.Token);await effectStarted.Task;if(applySucceeded)for(int i=0;i<states.Length;i++)RecordApplied(indices[i],states[i],mode,speed,effectOptions[mode],pendingBar);
 }
 static void SendEffectFrame(int index,Color c,int brightness){
  int level=(brightness+2)/5;
  switch(index){case 0:Hid.Apply(c,level);break;case 1:Hid.ApplyMsi(c,(brightness+5)/10);break;case 2:VisionGpu.Apply(c,(brightness*99+50)/100);break;case 3:RamBridge.Apply(c,level);break;}
 }
 sealed class TestColorStream:IColorStream {
  Action<int,Color,int> writer;int index;
  public TestColorStream(Action<int,Color,int> writer,int index){this.writer=writer;this.index=index;}
  public void SendFrame(Color c,int b){writer(index,c,b);}
  public void Dispose(){}
 }
 async Task RunEffect(List<LightCard> targets,LightState[] states,int[] indices,int mode,int speed,CancellationToken token,EffectOptions overrideOptions=null,ColorBalance[] overrideBalances=null){
  var started=effectStarted;var options=(overrideOptions??effectOptions[mode]).Copy();var bar=Hid.BarSettings.Copy();var balances=(overrideBalances??Calibration.Values).Select(v=>v.Copy()).ToArray();var clock=System.Diagnostics.Stopwatch.StartNew();
  var counts=new int[states.Length];activeEffectIndices=indices.ToArray();foreach(int index in indices)effectSentCounts[index]=0;
  using(var timing=new FrameTiming())
  using(var screenCapture=mode==18?ScreenColors.Start(speed,options.FollowScreenBrightness):null)
  using(var linked=CancellationTokenSource.CreateLinkedTokenSource(token))
  using(var preview=new System.Windows.Forms.Timer{Interval=33}){
   preview.Tick+=delegate{
    if(linked.IsCancellationRequested||!Visible||WindowState==FormWindowState.Minimized)return;
    for(int i=0;i<targets.Count;i++){
     if(Volatile.Read(ref counts[i])==0){targets[i].Status.Text="Preparando";continue;}
     targets[i].Art.Light=EffectOptions.Sample(mode,states[i].Color,clock.Elapsed.TotalSeconds,speed,indices[i],options);
     targets[i].Art.Level=states[i].Brightness;targets[i].Art.Invalidate();targets[i].Status.Text="Animando";
    }
   };
   preview.Start();
   try{
    var workers=Enumerable.Range(0,states.Length).Select(i=>Task.Factory.StartNew(async ()=>{
     try{
      linked.Token.ThrowIfCancellationRequested();
      using(IColorStream stream=effectWriter!=null?new TestColorStream(effectWriter,indices[i]):ColorStreams.Open(indices[i],states[i],indices[i]==1?msiZones:null,balances[indices[i]])){
       // Independent devices never wait for the RAM's SMBus transfers.
       var pacer=new FramePacer(30);
       while(!linked.IsCancellationRequested){
        double now=clock.Elapsed.TotalSeconds;
        Color sampled=EffectOptions.Sample(mode,states[i].Color,now,speed,indices[i],options);Color frameColor=indices[i]==1?sampled:balances[indices[i]].Apply(sampled);stream.SendFrame(frameColor,states[i].Brightness);lastEffectRgb[indices[i]]=sampled.ToArgb();Interlocked.Increment(ref effectSentCounts[indices[i]]);
        Interlocked.Increment(ref counts[i]);if(counts.All(n=>n>0)){applySucceeded=true;started.TrySetResult(true);}
        await pacer.Wait(linked.Token).ConfigureAwait(false);
       }
      }
     }catch(OperationCanceledException){}
     catch(Exception ex){linked.Cancel();throw new InvalidOperationException(states[i].Name+": "+ex.Message,ex);}
    },CancellationToken.None,TaskCreationOptions.LongRunning,TaskScheduler.Default).Unwrap()).ToArray();
    await Task.WhenAll(workers);
   }catch(Exception ex){applySucceeded=false;applyError=ex.Message;started.TrySetResult(false);status.Text="Efeito interrompido: "+ex.Message;foreach(var card in targets)card.Status.Text="Interrompido";}
   finally{started.TrySetResult(false);preview.Stop();if(!IsDisposed){effectStop.Enabled=false;activate.Enabled=!busy;}}
  }
 }
}
















