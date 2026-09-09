// SPDX-License-Identifier: GPL-2.0-or-later
using System;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

partial class ThebestRGB {
 // One instance per Windows user/session, regardless of the executable's filename or folder.
 static int RunSingleApp(string[] args){
  string scope=BrandMigration.InstanceScope+WindowsIdentity.GetCurrent().User.Value;
  using(var gate=new Mutex(false,scope+".Instance"))
  using(var wake=new EventWaitHandle(false,EventResetMode.AutoReset,scope+".Show")){
   bool owns=false;
   try{
    try{owns=gate.WaitOne(0);}catch(AbandonedMutexException){owns=true;}
    if(!owns){
     if(!args.Any(a=>string.Equals(a,"--tray",StringComparison.OrdinalIgnoreCase)))wake.Set();
     return 0;
    }
    using(var app=new ThebestRGB()){
     app.Shown+=async delegate{await app.ApplyLastConfigurationOnStartup();};
     bool trayStart=args.Any(a=>string.Equals(a,"--tray",StringComparison.OrdinalIgnoreCase));
     if(trayStart)app.Shown+=delegate{app.Hide();};
     IntPtr handle=app.Handle;
     RegisteredWaitHandle listener=ThreadPool.RegisterWaitForSingleObject(wake,delegate{
      try{if(!app.IsDisposed&&app.IsHandleCreated)app.BeginInvoke(new Action(delegate{
       if(app.IsDisposed)return;
       app.RestoreWindow();
       var dialog=app.OwnedForms.LastOrDefault(f=>f.Visible);
       if(dialog!=null)dialog.Activate();
      }));}catch(InvalidOperationException){}
     },null,Timeout.Infinite,false);
     try{Application.Run(app);}finally{listener.Unregister(null);}
    }
    return 0;
   }finally{if(owns)gate.ReleaseMutex();}
  }
 }
}
