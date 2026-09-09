using System.Collections.Generic;
static class EffectPresets {
 public static IEnumerable<StudioPreset> ForMode(int mode){
  foreach(var preset in StudioPreset.All())if(preset.Mode==mode)yield return preset;
  if(mode<1||mode>15)yield break;
  yield return new StudioPreset{Name="Padrão do efeito",Mode=mode,Speed=3,Options=new EffectOptions()};
  yield return new StudioPreset{Name="Suave e lento",Mode=mode,Speed=1,Options=new EffectOptions{Intensity=80,Saturation=80}};
  yield return new StudioPreset{Name="Vivo e rápido",Mode=mode,Speed=4,Options=new EffectOptions{Intensity=100,Saturation=100}};
  yield return new StudioPreset{Name="Branco suave",Mode=mode,Speed=2,Options=new EffectOptions{CustomPalette=true,Foreground=0xFFFFFF,Grid=0xFFFFFF,Background=0,Intensity=100}};
  yield return new StudioPreset{Name="Azul e violeta",Mode=mode,Speed=2,Options=new EffectOptions{CustomPalette=true,Foreground=0x00CFFF,Grid=0xAF50FF,Background=0x000010}};
  yield return new StudioPreset{Name="Âmbar",Mode=mode,Speed=1,Options=new EffectOptions{CustomPalette=true,Foreground=0xFFAA30,Grid=0xFF5020,Background=0x100200}};
 }
}
