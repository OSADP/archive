using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Windows.Forms;

namespace INCZONE.Common
{
  /// <summary>
  /// Summary description for UserControl1.
  /// </summary>
  [DesignerCategory("Code")]
  public class Led : System.Windows.Forms.Control { 

    private Timer tick;

    public Led():base() {
      SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      SetStyle(ControlStyles.DoubleBuffer, true);
      SetStyle(ControlStyles.SupportsTransparentBackColor, true);
      SetStyle(ControlStyles.UserPaint, true);
      SetStyle(ControlStyles.ResizeRedraw, true);

      BackColor = Color.Transparent;

      Width  = 30;
      Height = 30;

      tick = new Timer();
      tick.Enabled = false;
      tick.Tick += new System.EventHandler(this._Tick);
    }
    
    #region new properties
    private bool _Active = true;
    [Category("Behavior"),
    DefaultValue(true)]
    public bool Active {
      get { return _Active; }
      set { 
        _Active = value; 
        Invalidate();
      }
    }

    private Color _ColorOn = Color.Red;
    [Category("Appearance")]
    public Color ColorOn {
      get { return _ColorOn; }
      set { 
        _ColorOn = value; 
        Invalidate();
      }
    }

    private Color _ColorOff = SystemColors.Control;
    [Category("Appearance")]
    public Color ColorOff {
      get { return _ColorOff; }
      set { 
        _ColorOff = value; 
        Invalidate();
      }
    }

    private bool _Flash = false;
    [Category("Behavior"),
    DefaultValue(false)]
    public bool Flash {
      get { return _Flash; }
      set { 
        _Flash = value && (flashIntervals.Length>0); 
        tickIndex = 0;
        tick.Interval = flashIntervals[tickIndex];
        tick.Enabled = _Flash;
        Active = true;
      }
    }

    private string _FlashIntervals="250";
    public int [] flashIntervals = new int[1] {250};
    [Category("Appearance"),
    DefaultValue("250")]
    public string FlashIntervals {
      get { return _FlashIntervals; }
      set { 
        _FlashIntervals = value; 
        string [] fi = _FlashIntervals.Split(new char[] {',','/','|',' ','\n'});
        flashIntervals = new int[fi.Length];
        for (int i=0; i<fi.Length; i++)
          try {
            flashIntervals[i] = int.Parse(fi[i]);
          } catch {
            flashIntervals[i] = 25;
          }
      }
    }

    private string _FlashColors=string.Empty; 
    public Color [] flashColors;
    [Category("Appearance"),
    DefaultValue("")]
    public string FlashColors {
      get { return _FlashColors; }
      set { 
        _FlashColors = value; 
        if (_FlashColors==string.Empty) {
          flashColors=null;
        } else {
          string [] fc = _FlashColors.Split(new char[] {',','/','|',' ','\n'});
          flashColors = new Color[fc.Length];
          for (int i=0; i<fc.Length; i++)
            try {
              flashColors[i] = (fc[i]!="")?Color.FromName(fc[i]):Color.Empty;
            } catch {
              flashColors[i] = Color.Empty;
            }
        }
      }
    }

    #endregion


    public new event PaintEventHandler Paint;

    protected override void OnPaint(PaintEventArgs e) {
      if (null!=Paint) Paint(this,e);
      else {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        base.OnPaint(e);
//        e.Graphics.Clear(BackColor);
        if (Enabled) {
            Color currentColor;
            if (Active)
            {
                currentColor = ColorOn;
            }
            else
            {
                currentColor = ColorOff;
            }
            Rectangle drawArea1 = new Rectangle(0, 0, Width, Height); 
            LinearGradientBrush linearBrush = new LinearGradientBrush(drawArea1, Color.White, Color.Gray, LinearGradientMode.ForwardDiagonal);
            e.Graphics.FillEllipse(linearBrush, 0, 0, Width-1, Height-1);
            e.Graphics.FillEllipse(new SolidBrush(currentColor), 3, 3, Width - 6, Height - 6);

            // GradientFill
            GraphicsPath path = new GraphicsPath();
            if (Width < 30)
            {
                path.AddEllipse(Width / 5, Height / 5, (Width - 6) / 4 * 3, (Height - 6) / 4 * 3);
            }
            else
            {
                path.AddEllipse(Width / 10, Height / 10, (Width - 6) / 4 * 3, (Height - 6) / 4 * 3);
            }

            PathGradientBrush brush = new PathGradientBrush(path);
            brush.WrapMode = WrapMode.Clamp;
            brush.SurroundColors = new Color[] { currentColor };
            brush.CenterColor = Color.White;
            e.Graphics.FillRectangle(brush, 3, 3, Width - 6, Height - 6);
            e.Graphics.DrawEllipse(new Pen(Color.LightGray, 2), 2, 2, Width - 4, Height - 4);
        }
        else e.Graphics.DrawEllipse(new Pen(System.Drawing.SystemColors.ControlDark,1),1,1,Width-3,Height-3);
      }
    }

    public int tickIndex;
    private void _Tick(object sender, System.EventArgs e) {
      tickIndex=(++tickIndex)%(flashIntervals.Length);
      tick.Interval=flashIntervals[tickIndex];
      try {
        if ((flashColors==null)||(flashColors.Length<tickIndex)||(flashColors[tickIndex]==Color.Empty))
          Active = !Active;
        else {
          ColorOn = flashColors[tickIndex];
          Active=true;
        }
      } catch {
        Active = !Active;
      }
    }

  }
}
