// Compare live values without serializing or allocating snapshots on every UI tick.
static class UiEquality {
 public static bool Bar(KeyboardBarSettings a,KeyboardBarSettings b){return a!=null&&b!=null&&a.Mode==b.Mode&&a.ColorValue==b.ColorValue&&a.Brightness==b.Brightness&&a.Speed==b.Speed&&a.Multicolor==b.Multicolor;}
 public static bool Options(EffectOptions a,EffectOptions b){return a!=null&&b!=null&&a.Foreground==b.Foreground&&a.Grid==b.Grid&&a.Background==b.Background&&a.Saturation==b.Saturation&&a.Intensity==b.Intensity&&a.CustomPalette==b.CustomPalette&&a.SparkSpeed==b.SparkSpeed&&a.GridSpeed==b.GridSpeed&&a.Width==b.Width&&a.Dissipation==b.Dissipation&&a.Density==b.Density&&a.Spread==b.Spread&&a.Reverse==b.Reverse;}
 public static bool Zones(MsiZoneSettings a,MsiZoneSettings b){return a!=null&&b!=null&&a.Split==b.Split&&a.Custom==b.Custom&&a.Fans==b.Fans&&a.WaterCooler==b.WaterCooler&&a.Auxiliary==b.Auxiliary&&a.FansChannel==b.FansChannel&&a.WaterChannel==b.WaterChannel;}
}
partial class ThebestRGB {
 bool MatchesCurrent(AppliedDevice old,int i){var state=cards[i].State;var balance=Calibration.Values[i];return old.State.R==state.R&&old.State.G==state.G&&old.State.B==state.B&&old.State.Brightness==state.Brightness&&old.Mode==effectMode.SelectedIndex&&old.Speed==effectSpeed.Value&&UiEquality.Options(old.Options,effectOptions[effectMode.SelectedIndex])&&old.Balance.R==balance.R&&old.Balance.G==balance.G&&old.Balance.B==balance.B&&(i!=1||UiEquality.Zones(old.MsiZones,msiZones))&&(i!=0||UiEquality.Bar(old.Bar,pendingBar));}
}
