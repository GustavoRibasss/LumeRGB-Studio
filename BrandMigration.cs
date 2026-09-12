using System;
using System.IO;
using Microsoft.Win32;
static class BrandMigration { internal const string InstanceScope=@"Local\LumeRGB.Studio.";
 // Legacy names are used only to preserve existing user settings during migration.
 internal static string DataDirectory(){return MigrateData(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));}
 internal static string MigrateData(string root){
  string current=Path.Combine(root,"ThebestRGB"),previous=Path.Combine(root,"LumeRGB");
  string marker=Path.Combine(current,".migration-complete");
  if(File.Exists(marker))return current;
  Directory.CreateDirectory(current);
  if(Directory.Exists(previous))foreach(string source in Directory.GetFiles(previous)){
   string target=Path.Combine(current,Path.GetFileName(source));
   if(!File.Exists(target))File.Copy(source,target,false);
  }
  File.WriteAllText(marker,"1");return current;
 }
 internal static void Startup(){
  try{using(var key=Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run",true)){
   if(key==null)return;var old=key.GetValue("LumeStudio") as string;
   if(old==null)return;
   if(key.GetValue("ThebestRGB")==null)key.SetValue("ThebestRGB",old);
   key.DeleteValue("LumeStudio",false);
  }}catch{ /* Retain the old entry if migration is unavailable. */ }
 }
}
