using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
partial class ThebestRGB {
 bool choosingTone;
 readonly List<Button> frequentButtons=new List<Button>();
 static readonly int[] DefaultQuickColors={0xFFFFFF,0xFF0000,0x00FF00,0x0000FF,0xFFFF00,0xA060FF};
 void InitializeFrequentColors(){
  string[] names={"Branco","Vermelho","Verde","Azul","Amarelo","Roxo"};
  for(int i=0;i<DefaultQuickColors.Length;i++){
   int value=DefaultQuickColors[i];var button=Button("",Color.FromArgb((value>>16)&255,(value>>8)&255,value&255));
   button.SetBounds(18+i*41,251,32,20);hero.Controls.Add(button);frequentButtons.Add(button);
   button.Click+=delegate{if(!busy)SetGlobal(button.BackColor);};
   help.SetToolTip(button,names[i]+" · #"+value.ToString("X6"));
  }
 }
 void RememberColor(Color color){}
}
