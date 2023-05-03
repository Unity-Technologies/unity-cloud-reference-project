package com.unity3d.player.appui;

import android.os.Build;
import android.os.VibrationEffect;

public class HapticFeedback {
    public static final int UNDEFINED = 0;
    public static final int LIGHT = 1;
    public static final int MEDIUM = 2;
    public static final int HEAVY = 3;
    public static final int SUCCESS = 4;
    public static final int ERROR = 5;
    public static final int WARNING = 6;
    public static final int SELECTION = 7;

    static final int lightAmplitude = 1;
    static final int mediumAmplitude;

    static {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            mediumAmplitude = VibrationEffect.DEFAULT_AMPLITUDE;
        }
        else {
            mediumAmplitude = 100;
        }
    }

    static final int heavyAmplitude = 255;

    static final long lightTiming = 8;
    static final long mediumTiming = 25;
    static final long heavyTiming = 50;
    static final long selectionTiming = 2;
    static final long[] errorTimings = new long[] { 0, 6, 120, 12, 120, 12 };
    static final long[] successTimings = new long[] { 0, 12, 120, 16 };
    static final long[] warningTimings = new long[] { 0, 80, 100, 40 };
}
