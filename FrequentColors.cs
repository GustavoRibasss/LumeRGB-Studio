using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Forms;
partial class ThebestRGB {
 bool choosingTone;
 readonly List<Button> frequentButtons=new List<Button>();
 Dictionary<string,int> colorUses=new Dictionary<string,int>();
 static readonly int[] DefaultQuickColors={0xFFFFFF,0x880000,0x50DCF0,0xA060FF,0xFF5084,0xFFAF5A};
 string FrequentColorsFile{get{return Path.Combine(Path.GetDirectoryName(ProfileStore.FileName),"frequent-colors.json");}}
 void InitializeFrequentColors(){
  try{if(File.Exists(FrequentColorsFile)){var saved=new JavaScriptSerializer().Deserialize<Dictionary<string,int>>(File.ReadAllText(FrequentColorsFile));if(saved!=null)colorUses=saved.Where(p=>p.Key.Length==6&&p.Value>0&&p.Key.All(Uri.IsHexDigit)).Take(256).ToDictionary(p=>p.Key,p=>p.Value);}}catch{}
  for(int i=0;i<6;i++){var button=Button("",Color.White);button.SetBounds(18+i*41,251,32,20);hero.Controls.Add(button);frequentButtons.Add(button);button.Click+=delegate{if(!busy)SetGlobal(button.BackColor);};}RefreshFrequentColors();
 }
 void RememberColor(Color color){if(frequentButtons.Count==0)return;string key=(color.ToArgb()&0xffffff).ToString("X6");int count;colorUses.TryGetValue(key,out count);colorUses[key]=Math.Min(1000000,count+1);colorUses=colorUses.OrderByDescending(p=>p.Value).Take(256).ToDictionary(p=>p.Key,p=>p.Value);try{Directory.CreateDirectory(Path.GetDirectoryName(FrequentColorsFile));string temp=FrequentColorsFile+".tmp";File.WriteAllText(temp,new JavaScriptSerializer().Serialize(colorUses));if(File.Exists(FrequentColorsFile))File.Replace(temp,FrequentColorsFile,null);else File.Move(temp,FrequentColorsFile);}catch(Exception ex){status.Text="Cor selecionada, mas o histórico não foi salvo: "+ex.Message;}RefreshFrequentColors();}
 void RefreshFrequentColors(){var palette=colorUses.OrderByDescending(p=>p.Value).ThenBy(p=>p.Key).Select(p=>Convert.ToInt32(p.Key,16)).Concat(DefaultQuickColors).Distinct().Take(6).ToArray();for(int i=0;i<frequentButtons.Count;i++){var b=frequentButtons[i];b.BackColor=Color.FromArgb((palette[i]>>16)&255,(palette[i]>>8)&255,palette[i]&255);help.SetToolTip(b,"#"+palette[i].ToString("X6")+" · suas cores mais usadas");}}
}
