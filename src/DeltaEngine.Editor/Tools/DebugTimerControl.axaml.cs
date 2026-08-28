using Avalonia.Controls;
using System.Diagnostics;
using System.Globalization;

namespace Delta.Engine.Editor;

public partial class DebugTimerControl : UserControl
{
    private Stopwatch? sw;
    private int prevTime;
    public DebugTimerControl() => InitializeComponent();

    public void StartDebug()
    {
        (sw ??= new()).Restart();
    }

    public void StopDebug()
    {
        const string usS = "us";
        const string msS = "ms";
        sw?.Stop();
        int time = (int)(sw?.Elapsed.TotalMicroseconds ?? 0);
        prevTime = SmoothInt(prevTime, time, 50);
        string format = prevTime > 1000 ? msS : usS;
        string t = prevTime > 1000
            ? ((float)prevTime / 1000).ToString("0.00", CultureInfo.InvariantCulture)
            : prevTime.ToString(CultureInfo.InvariantCulture);
        DebugTimer.Content = $"{t}{format}";
    }

    private static int SmoothInt(int value1, int value2, int smoothing)
    {
        return ((value1 * smoothing) + value2) / (smoothing + 1);
    }
}
