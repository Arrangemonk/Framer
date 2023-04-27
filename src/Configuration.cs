namespace framer;

public static class InterpolationValues
{
    public const string Linear = "0 0 1 1";
    public const string EaseIn = "0.5 0 1 1";
    public const string EaseOut = "0 0 0.5 1";
    public const string EaseInEaseOut = "0.5 0 0.5 1";
    public const string InstantOvershoot = "0 1 0 1";
    public const string LateOvershoot = "0 0 1 0";
    public const string SlightAnticipation = "0.8 0 0 0.8";
    public const string Anticipation = "1 0 0 1";
    public const string EaseInOvershoot = "1 0 0.5 1";
    public const string EaseOutOvershoot = "0.5 0 0 1";

    public const string WeirdIn = "1 0 0 0";
    public const string WeirdOut = "0 1 0 1";
    public const string SnapIn = "1 1 0 0";
    public const string SnapOut = "1 1 1 0";

    //example for overshoot with 2 bounces
    //<svg>
    //<rect x = "50" y="50" width="50" height="50">
    //<animate attributeName = "x" dur="1s" 
    //values="50;100;70;85;75" 
    //repeatCount="indefinite"
    //keyTimes="0;0.1;0.3;0.5;1"
    //keySplines="0.9,0,0,0.9;0.9,0,0,0.9;0.9,0,0,0.9;0.9,0,0,0.9" />
    //</rect>
    //</svg>
}
public enum Interpolation{
    Linear,
    EaseIn,
    EaseOut,
    EaseInEaseOut,
    InstantOvershoot,
    LateOvershoot, 
    SlightAnticipation,
    Anticipation,
    EaseInOvershoot,
    EaseOutOvershoot,

    WeirdIn,
    WeirdOut,
    SnapIn,
    SnapOut,
}

public enum OutputFormat{
    AnimatedSvg = 1,
    PngSequence = 2,
    SvgSequence = 4,
    All = AnimatedSvg + PngSequence + SvgSequence
}
public class Configuration
{
    public int? Fps{get;set;}
    public Keyframe[] Frames{get;set;}
    public Interpolation? DefaultInterpolation {get;set;}
    public string? OutputFileName{ get; set; }
    public OutputFormat OutputFormat {get;set;}
}

public class Keyframe
{
    public string Filename{get;set;}
    public int? FrameNumber {get;set;}
    public double? Duration{get;set;}
    public Interpolation? Interpolation {get;set;}

}
