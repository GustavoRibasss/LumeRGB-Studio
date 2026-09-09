using System;
using System.Drawing;
static class RedCalibration {
 // Visual reference: screen #2E050C -> LED #880000. Blend only near saturated red.
 public static Color Apply(Color c){
  if(c.R==0||c.R<=Math.Max(c.G,c.B))return c;
  double hue=c.GetHue();double distance=Math.Min(hue,360-hue);
  double dominance=(c.R-Math.Max(c.G,c.B))/(double)c.R;
  double weight=Math.Max(0,Math.Min(1,(35-distance)/15))*Math.Max(0,Math.Min(1,(dominance-.25)/.45));
  weight=weight*weight*(3-2*weight);
  double exponent=Math.Log(136.0/255)/Math.Log(46.0/255);
  int red=(int)Math.Round(255*Math.Pow(c.R/255.0,exponent));
  return Color.FromArgb((int)Math.Round(c.R+(red-c.R)*weight),(int)Math.Round(c.G*(1-weight)),(int)Math.Round(c.B*(1-weight)));
 }
}
