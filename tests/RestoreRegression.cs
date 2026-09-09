using System;using System.IO;using System.Reflection;using System.Collections.Generic;using System.Windows.Forms;
class RestoreRegression{
 [STAThread]static void Main(){var flags=BindingFlags.Instance|BindingFlags.NonPublic;ProfileStore.FileName=Path.Combine(Path.GetTempPath(),"restore-test-"+Guid.NewGuid(),"profiles.json");using(var f=new ThebestRGB()){
 var cards=(List<LightCard>)typeof(ThebestRGB).GetField("cards",flags).GetValue(f);
 cards[0].State.R=21;cards[0].State.G=45;cards[0].State.B=180;cards[0].State.Brightness=37;
 typeof(ThebestRGB).GetMethod("SaveRestorePoint",flags).Invoke(f,null);cards[0].State.R=255;
 typeof(ThebestRGB).GetMethod("RestoreSavedSetup",flags).Invoke(f,null);
 if(cards[0].State.R!=21||cards[0].State.Brightness!=37)throw new Exception("Saved state was not restored");
 File.WriteAllText(Path.Combine(Path.GetDirectoryName(ProfileStore.FileName),"restore-point.json"),"{}");
 typeof(ThebestRGB).GetMethod("RestoreSavedSetup",flags).Invoke(f,null);
 if(cards[0].State.R!=21||cards[0].State.Brightness!=37)throw new Exception("Invalid state mutated setup");
 }Console.WriteLine("PASS: restore persistence and malformed data rejected without changes; no hardware commands.");}
}
