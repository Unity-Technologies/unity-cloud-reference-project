//
//  AppUiNativePlugin.mm
//  App UI Native Plugin
//

#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

#if defined(__cplusplus)
extern "C"
{
#endif

typedef NS_ENUM (int, AppUIHapticFeedbackType)
{
    AppUIHapticFeedbackTypeUndefined,
    AppUIHapticFeedbackTypeLight,
    AppUIHapticFeedbackTypeMedium,
    AppUIHapticFeedbackTypeHeavy,
    AppUIHapticFeedbackTypeSuccess,
    AppUIHapticFeedbackTypeError,
    AppUIHapticFeedbackTypeWarning,
    AppUIHapticFeedbackTypeSelection
};

float _IOSAppUIScaleFactor()
{
    return [UIScreen mainScreen].scale;
}

void _IOSRunHapticFeedback(int feedbackType)
{
    if (@available(iOS 10.0, *))
    {
        if (feedbackType == AppUIHapticFeedbackTypeLight)
        {
            [[[UIImpactFeedbackGenerator alloc]initWithStyle: UIImpactFeedbackStyleLight] impactOccurred];
        }
        else if (feedbackType == AppUIHapticFeedbackTypeMedium)
        {
            [[[UIImpactFeedbackGenerator alloc]initWithStyle: UIImpactFeedbackStyleMedium] impactOccurred];
        }
        else if (feedbackType == AppUIHapticFeedbackTypeHeavy)
        {
            [[[UIImpactFeedbackGenerator alloc]initWithStyle: UIImpactFeedbackStyleHeavy] impactOccurred];
        }
        else if (feedbackType == AppUIHapticFeedbackTypeSuccess)
        {
            [[UINotificationFeedbackGenerator new] notificationOccurred: UINotificationFeedbackTypeSuccess];
        }
        else if (feedbackType == AppUIHapticFeedbackTypeError)
        {
            [[UINotificationFeedbackGenerator new] notificationOccurred: UINotificationFeedbackTypeError];
        }
        else if (feedbackType == AppUIHapticFeedbackTypeWarning)
        {
            [[UINotificationFeedbackGenerator new] notificationOccurred: UINotificationFeedbackTypeWarning];
        }
        else if (feedbackType == AppUIHapticFeedbackTypeSelection)
        {
            [[UISelectionFeedbackGenerator new] selectionChanged];
        }
    }
}

int _IOSCurrentAppearance()
{
    if (@available(iOS 13.0, *))
    {
        return (int)[[UITraitCollection currentTraitCollection] userInterfaceStyle];
    }
    return 0;
}

#if defined(__cplusplus)
}
#endif

NS_ASSUME_NONNULL_END
