package com.unity3d.player.appui;

import android.content.Context;
import android.content.res.Configuration;
import android.content.res.Resources;
import android.os.Build;
import android.os.Bundle;
import android.os.VibrationEffect;
import android.os.Vibrator;
import android.os.VibratorManager;
import android.util.DisplayMetrics;
import android.util.Log;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

public class AppUIActivity extends UnityPlayerActivity {

    int m_CurrentUiMode = Configuration.UI_MODE_NIGHT_UNDEFINED;
    float m_CurrentFontScale = 1;
    float m_CurrentScaledDensity = 1;
    float m_CurrentDensity = 1;
    int m_CurrentDensityDpi = DisplayMetrics.DENSITY_DEFAULT;
    Vibrator m_Vibrator;

    public boolean IsNightModeDefined() {
        return m_CurrentUiMode != Configuration.UI_MODE_NIGHT_UNDEFINED;
    }

    public boolean IsNightModeEnabled() {
        return m_CurrentUiMode == Configuration.UI_MODE_NIGHT_YES;
    }

    public float FontScale() {
        return m_CurrentFontScale;
    }

    public float ScaledDensity() {
        return m_CurrentScaledDensity;
    }

    public float Density() {
        return m_CurrentDensity;
    }

    public int DensityDpi() {
        return m_CurrentDensityDpi;
    }

    void Vibrate(long[] timings, int repeat) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            m_Vibrator.vibrate(VibrationEffect.createWaveform(timings, repeat));
        } else {
            m_Vibrator.vibrate(timings, repeat);
        }
    }

    void Vibrate(long[] timings) {
        Vibrate(timings, -1);
    }

    void Vibrate(long timing, int amplitude) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            m_Vibrator.vibrate(VibrationEffect.createOneShot(timing, amplitude));
        }
        else {
            m_Vibrator.vibrate(timing);
        }
    }

    void Vibrate(long timing) {
        Vibrate(timing, -1);
    }

    public void RunHapticFeedback(int hapticFeedbackType) {

        switch (hapticFeedbackType) {
            case HapticFeedback.LIGHT: {
                Vibrate(HapticFeedback.lightTiming, HapticFeedback.lightAmplitude);
                break;
            }
            case HapticFeedback.MEDIUM: {
                Vibrate(HapticFeedback.mediumTiming, HapticFeedback.mediumAmplitude);
                break;
            }
            case HapticFeedback.HEAVY: {
                Vibrate(HapticFeedback.heavyTiming, HapticFeedback.heavyAmplitude);
                break;
            }
            case HapticFeedback.SUCCESS: {
                Vibrate(HapticFeedback.successTimings);
                break;
            }
            case HapticFeedback.ERROR: {
                Vibrate(HapticFeedback.errorTimings);
                break;
            }
            case HapticFeedback.WARNING: {
                Vibrate(HapticFeedback.warningTimings);
                break;
            }
            case HapticFeedback.SELECTION: {
                Vibrate(HapticFeedback.selectionTiming, HapticFeedback.lightAmplitude);
                break;
            }
            default:
                break;
        }
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        Resources resources = getResources();

        Configuration configuration = resources.getConfiguration();

        m_CurrentUiMode = configuration.uiMode & Configuration.UI_MODE_NIGHT_MASK;
        m_CurrentFontScale = configuration.fontScale;

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            VibratorManager manager = (VibratorManager) getSystemService(Context.VIBRATOR_MANAGER_SERVICE);
            m_Vibrator = manager.getDefaultVibrator();
        }
        else {
            m_Vibrator = (Vibrator) getSystemService(Context.VIBRATOR_SERVICE);
        }

        DisplayMetrics displayMetrics = resources.getDisplayMetrics();
        m_CurrentScaledDensity = displayMetrics.scaledDensity;
        m_CurrentDensity = displayMetrics.density;
        m_CurrentDensityDpi = displayMetrics.densityDpi;


        Log.d("APP UI", "Initial Night Mode: " + m_CurrentUiMode);
        Log.d("APP UI", "Initial Font Scale: " + m_CurrentFontScale);
        Log.d("APP UI", "Initial Density: " + displayMetrics.density);
        Log.d("APP UI", "Initial Scaled Density: " + displayMetrics.scaledDensity);
        Log.d("APP UI", "Initial Density DPI: " + displayMetrics.densityDpi);
    }

    @Override
    public void onConfigurationChanged(Configuration newConfig) {
        super.onConfigurationChanged(newConfig);

        DisplayMetrics displayMetrics = getResources().getDisplayMetrics();
        boolean configurationChanged = false;

        int currentNightMode = newConfig.uiMode & Configuration.UI_MODE_NIGHT_MASK;
        if (m_CurrentUiMode != currentNightMode) {
            m_CurrentUiMode = currentNightMode;
            configurationChanged = true;

            switch (m_CurrentUiMode) {
                case Configuration.UI_MODE_NIGHT_NO:
                    Log.d("APP UI", "Light Mode enabled");
                    break;
                case Configuration.UI_MODE_NIGHT_YES:
                    Log.d("APP UI", "Dark Mode enabled");
                    break;
                case Configuration.UI_MODE_NIGHT_UNDEFINED:
                    Log.d("APP UI", "Dark Mode is Undefined");
                    break;
            }
        }

        float currentFontScale = newConfig.fontScale;
        if (m_CurrentFontScale != currentFontScale) {
            m_CurrentFontScale = currentFontScale;
            configurationChanged = true;
            Log.d("APP UI", "Changed Font Scale : " + m_CurrentFontScale);
        }

        float currentScaledDensity = displayMetrics.scaledDensity;
        if (m_CurrentScaledDensity != currentScaledDensity) {
            m_CurrentScaledDensity = currentScaledDensity;
            configurationChanged = true;
            Log.d("APP UI", "Changed Scaled Density : " + m_CurrentScaledDensity);
        }

        float currentDensity = displayMetrics.density;
        if (m_CurrentDensity != currentDensity) {
            m_CurrentDensity = currentDensity;
            configurationChanged = true;
            Log.d("APP UI", "Changed Density : " + m_CurrentDensity);
        }

        int currentDensityDpi = newConfig.densityDpi;
        if (m_CurrentDensityDpi != currentDensityDpi) {
            m_CurrentDensityDpi = currentDensityDpi;
            configurationChanged = true;
            Log.d("APP UI", "Changed Density DPI : " + m_CurrentDensityDpi);
        }

        if (configurationChanged) {
            UnityPlayer.UnitySendMessage("AppUIUpdater", "OnAndroidNativeMessageReceived", "configurationChanged");
        }
    }
}
