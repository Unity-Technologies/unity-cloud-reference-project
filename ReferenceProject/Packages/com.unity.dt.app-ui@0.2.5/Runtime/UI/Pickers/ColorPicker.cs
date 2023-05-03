using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine.Dt.App.Core;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// ColorPicker UI Element.
    /// </summary>
    public class ColorPicker : VisualElement, INotifyValueChanged<Color>
    {
        /// <summary>
        /// The type of channels sliders to display in the ColorPicker.
        /// </summary>
        public enum SliderMode
        {
            /// <summary>
            /// Display the RGB sliders as integers.
            /// </summary>
            RGB255,
            /// <summary>
            /// Display the RGB sliders as floats.
            /// </summary>
            RGB01,
            /// <summary>
            /// Display the HSV sliders as floats.
            /// </summary>
            HSV
        }

        /// <summary>
        /// The ColorPicker main styling class.
        /// </summary>
        public const string ussClassName = "appui-colorpicker";

        /// <summary>
        /// The ColorPicker toolbar styling class.
        /// </summary>
        public const string toolbarUssClassName = ussClassName + "__colortoolbar";

        /// <summary>
        /// The WheelContainer styling class.
        /// </summary>
        public const string wheelContainerUssClassName = ussClassName + "__wheelcontainer";

        /// <summary>
        /// The ColorWheel styling class.
        /// </summary>
        public const string wheelUssClassName = ussClassName + "__wheel";

        /// <summary>
        /// The SV square styling class.
        /// </summary>
        public const string svSquareUssClassName = ussClassName + "__svsquare";

        /// <summary>
        /// The Alpha slider styling class.
        /// </summary>
        public const string alphaSliderUssClassName = ussClassName + "__alphaslider";

        /// <summary>
        /// The Channels container styling class.
        /// </summary>
        public const string channelsContainerUssClassName = ussClassName + "__channels-container";

        /// <summary>
        /// The channels dropdown styling class.
        /// </summary>
        public const string channelsDropdownUssClassName = ussClassName + "__channels-dropdown";

        /// <summary>
        /// The RGB red slider styling class.
        /// </summary>
        public const string redChannelSliderUssClassName = ussClassName + "__red-channel-slider";

        /// <summary>
        /// The RGB green slider styling class.
        /// </summary>
        public const string greenChannelSliderUssClassName = ussClassName + "__green-channel-slider";

        /// <summary>
        /// The RGB blue slider styling class.
        /// </summary>
        public const string blueChannelSliderUssClassName = ussClassName + "__blue-channel-slider";

        /// <summary>
        /// The hue slider styling class.
        /// </summary>
        public const string hueSliderUssClassName = ussClassName + "__hue-slider";

        /// <summary>
        /// The saturation slider styling class.
        /// </summary>
        public const string saturationSliderUssClassName = ussClassName + "__saturation-slider";

        /// <summary>
        /// The brightness slider styling class.
        /// </summary>
        public const string brightnessSliderUssClassName = ussClassName + "__brightness-slider";

        /// <summary>
        /// The hex field styling class.
        /// </summary>
        public const string hexFieldUssClassName = ussClassName + "__hex-field";

        readonly ColorSlider m_AlphaSlider;

        readonly ColorToolbar m_Toolbar;

        readonly VisualElement m_WheelContainer;

        readonly ColorWheel m_Wheel;

        readonly SVSquare m_SvSquare;

        readonly VisualElement m_ChannelsContainer;

        readonly VisualElement m_RGBContainer;

        readonly VisualElement m_RGBFloatContainer;

        readonly VisualElement m_HSVContainer;

        readonly Dropdown m_ChannelsDropdown;

        readonly SliderInt m_RedChannelSlider;

        readonly SliderInt m_GreenChannelSlider;

        readonly SliderInt m_BlueChannelSlider;

        readonly SliderFloat m_RedFChannelSlider;

        readonly SliderFloat m_GreenFChannelSlider;

        readonly SliderFloat m_BlueFChannelSlider;

        readonly SliderFloat m_HueSlider;

        readonly SliderFloat m_SaturationSlider;

        readonly SliderFloat m_BrightnessSlider;

        readonly TextField m_HexField;

        Color m_Value;

        /// <summary>
        /// The content container of the ColorPicker. This is null as the ColorPicker does not have a content container.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The current color value.
        /// </summary>
        public Color value
        {
            get => m_Value;

            set
            {
                var changed = m_Value != value;

                using var evt = ChangeEvent<Color>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);

                if (changed)
                    SendEvent(evt);
            }
        }

        /// <summary>
        /// The previous color value. This color will be displayed in the toolbar.
        /// </summary>
        public Color previousValue
        {
            get => m_Toolbar.previousColor;
            set => m_Toolbar.previousColor = value;
        }

        /// <summary>
        /// Determines if the ColorPicker should display the alpha slider.
        /// </summary>
        public bool showAlpha
        {
            get => !m_AlphaSlider.ClassListContains(Styles.hiddenUssClassName);
            set => m_AlphaSlider.EnableInClassList(Styles.hiddenUssClassName, !value);
        }

        /// <summary>
        /// Determines if the ColorPicker should display the toolbar.
        /// </summary>
        public bool showToolbar
        {
            get => !m_Toolbar.ClassListContains(Styles.hiddenUssClassName);
            set => m_Toolbar.EnableInClassList(Styles.hiddenUssClassName, !value);
        }

        /// <summary>
        /// Determines if the ColorPicker should display the hex field.
        /// </summary>
        public bool showHex
        {
            get => !m_HexField.ClassListContains(Styles.hiddenUssClassName);
            set => m_HexField.EnableInClassList(Styles.hiddenUssClassName, !value);
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ColorPicker()
        {
            AddToClassList(ussClassName);

            focusable = false;
            pickingMode = PickingMode.Ignore;

            m_Toolbar = new ColorToolbar { name = toolbarUssClassName, focusable = false, pickingMode = PickingMode.Ignore };
            m_Toolbar.AddToClassList(toolbarUssClassName);
            hierarchy.Add(m_Toolbar);

            m_WheelContainer = new VisualElement { name = wheelContainerUssClassName, focusable = false, pickingMode = PickingMode.Ignore };
            m_WheelContainer.AddToClassList(wheelContainerUssClassName);
            hierarchy.Add(m_WheelContainer);

            m_Wheel = new ColorWheel { name = wheelUssClassName, focusable = true, pickingMode = PickingMode.Position };
            m_Wheel.AddToClassList(wheelUssClassName);
            m_WheelContainer.hierarchy.Add(m_Wheel);

            m_SvSquare = new SVSquare { name = svSquareUssClassName, focusable = true, pickingMode = PickingMode.Position };
            m_SvSquare.AddToClassList(svSquareUssClassName);
            m_WheelContainer.hierarchy.Add(m_SvSquare);

            m_AlphaSlider = new ColorSlider { name = alphaSliderUssClassName, focusable = true, pickingMode = PickingMode.Position };
            m_AlphaSlider.AddToClassList(alphaSliderUssClassName);
            hierarchy.Add(m_AlphaSlider);

            m_ChannelsContainer = new VisualElement { name = channelsContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_ChannelsContainer.AddToClassList(channelsContainerUssClassName);
            hierarchy.Add(m_ChannelsContainer);

            m_ChannelsDropdown = new Dropdown { name = channelsDropdownUssClassName, pickingMode = PickingMode.Position, focusable = true };
            m_ChannelsDropdown.AddToClassList(channelsDropdownUssClassName);
            m_ChannelsContainer.hierarchy.Add(m_ChannelsDropdown);

            m_RGBContainer = new VisualElement();
            m_ChannelsContainer.hierarchy.Add(m_RGBContainer);

            m_ChannelsDropdown.sourceItems = new List<SliderMode>
            {
                SliderMode.RGB255,
                SliderMode.RGB01,
                SliderMode.HSV
            };

            m_ChannelsDropdown.bindItem = BindDropdownItem;

            m_RedChannelSlider = new SliderInt
            {
                name = redChannelSliderUssClassName,
                lowValue = 0,
                highValue = 255,
                label = "R",
                filled = true,
            };
            m_RedChannelSlider.AddToClassList(redChannelSliderUssClassName);
            m_RGBContainer.hierarchy.Add(m_RedChannelSlider);

            m_GreenChannelSlider = new SliderInt
            {
                name = greenChannelSliderUssClassName,
                lowValue = 0,
                highValue = 255,
                label = "G",
                filled = true,
            };
            m_GreenChannelSlider.AddToClassList(greenChannelSliderUssClassName);
            m_RGBContainer.hierarchy.Add(m_GreenChannelSlider);

            m_BlueChannelSlider = new SliderInt
            {
                name = blueChannelSliderUssClassName,
                lowValue = 0,
                highValue = 255,
                label = "B",
                filled = true,
            };
            m_BlueChannelSlider.AddToClassList(blueChannelSliderUssClassName);
            m_RGBContainer.hierarchy.Add(m_BlueChannelSlider);

            m_RGBFloatContainer = new VisualElement();
            m_ChannelsContainer.hierarchy.Add(m_RGBFloatContainer);

            m_RedFChannelSlider = new SliderFloat
            {
                name = redChannelSliderUssClassName,
                lowValue = 0,
                highValue = 1,
                label = "R",
                filled = true,
            };
            m_RedFChannelSlider.AddToClassList(redChannelSliderUssClassName);
            m_RGBFloatContainer.hierarchy.Add(m_RedFChannelSlider);

            m_GreenFChannelSlider = new SliderFloat
            {
                name = greenChannelSliderUssClassName,
                lowValue = 0,
                highValue = 1,
                label = "G",
                filled = true,
            };
            m_GreenFChannelSlider.AddToClassList(greenChannelSliderUssClassName);
            m_RGBFloatContainer.hierarchy.Add(m_GreenFChannelSlider);

            m_BlueFChannelSlider = new SliderFloat
            {
                name = blueChannelSliderUssClassName,
                lowValue = 0,
                highValue = 1,
                label = "B",
                filled = true,
            };
            m_BlueFChannelSlider.AddToClassList(blueChannelSliderUssClassName);
            m_RGBFloatContainer.hierarchy.Add(m_BlueFChannelSlider);

            m_HSVContainer = new VisualElement();
            m_ChannelsContainer.hierarchy.Add(m_HSVContainer);

            m_HueSlider = new SliderFloat
            {
                name = hueSliderUssClassName,
                lowValue = 0,
                highValue = 1,
                label = "H",
                filled = true,
            };
            m_HueSlider.AddToClassList(hueSliderUssClassName);
            m_HSVContainer.hierarchy.Add(m_HueSlider);

            m_SaturationSlider = new SliderFloat
            {
                name = saturationSliderUssClassName,
                lowValue = 0,
                highValue = 1,
                label = "S",
                filled = true,
            };
            m_SaturationSlider.AddToClassList(saturationSliderUssClassName);
            m_HSVContainer.hierarchy.Add(m_SaturationSlider);

            m_BrightnessSlider = new SliderFloat
            {
                name = brightnessSliderUssClassName,
                lowValue = 0,
                highValue = 1,
                label = "V",
                filled = true,
            };
            m_BrightnessSlider.AddToClassList(brightnessSliderUssClassName);
            m_HSVContainer.hierarchy.Add(m_BrightnessSlider);

            m_HexField = new TextField { name = hexFieldUssClassName };
            m_HexField.AddToClassList(hexFieldUssClassName);
            hierarchy.Add(m_HexField);

            showAlpha = false;
            showToolbar = false;

            m_Wheel.RegisterValueChangingCallback(OnWheelChanging);
            m_SvSquare.RegisterValueChangingCallback(OnSquareValueChanging);
            m_ChannelsDropdown.RegisterValueChangedCallback(OnChannelModeChanged);

            m_RedChannelSlider.RegisterValueChangingCallback(OnRedChannelChanging);
            m_GreenChannelSlider.RegisterValueChangingCallback(OnGreenChannelChanging);
            m_BlueChannelSlider.RegisterValueChangingCallback(OnBlueChannelChanging);

            m_RedFChannelSlider.RegisterValueChangingCallback(OnRedFChannelChanging);
            m_GreenFChannelSlider.RegisterValueChangingCallback(OnGreenFChannelChanging);
            m_BlueFChannelSlider.RegisterValueChangingCallback(OnBlueFChannelChanging);

            m_HueSlider.RegisterValueChangingCallback(OnHueSliderChanging);
            m_SaturationSlider.RegisterValueChangingCallback(OnSaturationSliderChanging);
            m_BrightnessSlider.RegisterValueChangingCallback(OnBrightnessSliderChanging);

            m_AlphaSlider.RegisterValueChangingCallback(OnAlphaChannelValueChanging);

            m_Toolbar.previousColorSwatchClicked += OnPreviousSwatchClicked;

            m_HexField.RegisterValueChangedCallback(OnHexValueChanged);

            m_ChannelsDropdown.SetValueWithoutNotify(0);
            OnChannelModeChanged(null);
            SetValueWithoutNotify(Color.clear);
        }

        void OnHexValueChanged(ChangeEvent<string> evt)
        {
            var hexStrValue = evt.newValue ?? "#000000";
            
            var strBuilder = new StringBuilder();
            foreach (var hexChar in hexStrValue)
            {
                if (Uri.IsHexDigit(hexChar))
                    strBuilder.Append(hexChar);
            }
            var str = strBuilder.ToString();

            if (!ColorExtensions.IsValidHex(str))
                return;

            var argbHex = ColorExtensions.RgbaToArgbHex(str);
            var argb = int.Parse(argbHex, NumberStyles.HexNumber);
            var winColor = System.Drawing.Color.FromArgb(argb);
            var c = new Color(winColor.R / 255f, winColor.G / 255f, winColor.B / 255f, winColor.A / 255f);

            Color.RGBToHSV(c, out var h, out var s, out var v);
            m_Wheel.SetValueWithoutNotify(h);
            m_SvSquare.referenceHue = h;
            m_SvSquare.SetValueWithoutNotify(new Vector2(s, v));
            m_AlphaSlider.colorRange = new List<ColorEntry>
            {
                new ColorEntry(new Color(c.r, c.g, c.b, 0), 0),
                new ColorEntry(new Color(c.r, c.g, c.b, 1), 1)
            };
            m_AlphaSlider.SetValueWithoutNotify(c.a);

            m_RedChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.r * 255));
            m_GreenChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.g * 255));
            m_BlueChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.b * 255));

            m_RedFChannelSlider.SetValueWithoutNotify(c.r);
            m_GreenFChannelSlider.SetValueWithoutNotify(c.g);
            m_BlueFChannelSlider.SetValueWithoutNotify(c.b);

            m_HueSlider.SetValueWithoutNotify(h);
            m_SaturationSlider.SetValueWithoutNotify(s);
            m_BrightnessSlider.SetValueWithoutNotify(v);

            TryNotifyValueChanged(c);
        }

        /// <summary>
        /// Sets the value without notifying the value changed callback.
        /// </summary>
        /// <param name="newValue"> The new color value. </param>
        public void SetValueWithoutNotify(Color newValue)
        {
            m_Value = newValue;
            Color.RGBToHSV(m_Value, out var h, out var s, out var v);
            m_Wheel.SetValueWithoutNotify(h);
            m_SvSquare.referenceHue = h;
            m_SvSquare.SetValueWithoutNotify(new Vector2(s, v));
            m_AlphaSlider.colorRange = new List<ColorEntry>
            {
                new ColorEntry(new Color(m_Value.r, m_Value.g, m_Value.b, 0), 0),
                new ColorEntry(new Color(m_Value.r, m_Value.g, m_Value.b, 1), 1)
            };
            m_AlphaSlider.SetValueWithoutNotify(m_Value.a);

            m_RedChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(m_Value.r * 255));
            m_GreenChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(m_Value.g * 255));
            m_BlueChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(m_Value.b * 255));

            m_RedFChannelSlider.SetValueWithoutNotify(m_Value.r);
            m_GreenFChannelSlider.SetValueWithoutNotify(m_Value.g);
            m_BlueFChannelSlider.SetValueWithoutNotify(m_Value.b);

            m_HueSlider.SetValueWithoutNotify(h);
            m_SaturationSlider.SetValueWithoutNotify(s);
            m_BrightnessSlider.SetValueWithoutNotify(v);

            m_HexField.SetValueWithoutNotify(ColorExtensions.ArgbToRgbaHex(
                System.Drawing.Color.FromArgb(
                Mathf.RoundToInt(m_AlphaSlider.value * 255),
                m_RedChannelSlider.value,
                m_GreenChannelSlider.value,
                m_BlueChannelSlider.value).ToArgb().ToString("X8")));

            m_Toolbar.currentColor = m_Value;
        }

        void OnPreviousSwatchClicked()
        {
            value = m_Toolbar.previousColor;
        }

        void BindDropdownItem(MenuItem item, int index)
        {
            var mode = (SliderMode)m_ChannelsDropdown.sourceItems[index];
            switch (mode)
            {
                case SliderMode.RGB255:
                    item.label = "RGB (0-255)";
                    break;
                case SliderMode.RGB01:
                    item.label = "RGB (0-1)";
                    break;
                case SliderMode.HSV:
                    item.label = "HSV";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void OnChannelModeChanged(ChangeEvent<int> evt)
        {
            var channelMode = (SliderMode)m_ChannelsDropdown.value;
            m_RGBContainer.EnableInClassList(Styles.hiddenUssClassName, channelMode != SliderMode.RGB255);
            m_RGBFloatContainer.EnableInClassList(Styles.hiddenUssClassName, channelMode != SliderMode.RGB01);
            m_HSVContainer.EnableInClassList(Styles.hiddenUssClassName, channelMode != SliderMode.HSV);
        }

        void OnWheelChanging(ChangingEvent<float> evt)
        {
            m_SvSquare.referenceHue = evt.newValue;
            var c = m_SvSquare.selectedColor;
            c.a = m_AlphaSlider.value;

            m_AlphaSlider.colorRange = new List<ColorEntry>
            {
                new ColorEntry(new Color(c.r, c.g, c.b, 0), 0),
                new ColorEntry(new Color(c.r, c.g, c.b, 1), 1)
            };
            m_RedChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.r * 255));
            m_GreenChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.g * 255));
            m_BlueChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.b * 255));

            m_RedFChannelSlider.SetValueWithoutNotify(c.r);
            m_GreenFChannelSlider.SetValueWithoutNotify(c.g);
            m_BlueFChannelSlider.SetValueWithoutNotify(c.b);

            m_HueSlider.SetValueWithoutNotify(evt.newValue);

            m_HexField.SetValueWithoutNotify(ColorExtensions.ArgbToRgbaHex(
                System.Drawing.Color.FromArgb(
                Mathf.RoundToInt(m_AlphaSlider.value * 255),
                m_RedChannelSlider.value,
                m_GreenChannelSlider.value,
                m_BlueChannelSlider.value).ToArgb().ToString("X8")));

            TryNotifyValueChanged(c);
        }

        void OnSquareValueChanging(ChangingEvent<Vector2> evt)
        {
            var c = m_SvSquare.selectedColor;
            c.a = m_AlphaSlider.value;

            m_AlphaSlider.colorRange = new List<ColorEntry>
            {
                new ColorEntry(new Color(c.r, c.g, c.b, 0), 0),
                new ColorEntry(new Color(c.r, c.g, c.b, 1), 1)
            };
            m_RedChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.r * 255));
            m_GreenChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.g * 255));
            m_BlueChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.b * 255));

            m_RedFChannelSlider.SetValueWithoutNotify(c.r);
            m_GreenFChannelSlider.SetValueWithoutNotify(c.g);
            m_BlueFChannelSlider.SetValueWithoutNotify(c.b);

            m_SaturationSlider.SetValueWithoutNotify(evt.newValue.x);
            m_BrightnessSlider.SetValueWithoutNotify(evt.newValue.y);

            m_HexField.SetValueWithoutNotify(ColorExtensions.ArgbToRgbaHex(
                System.Drawing.Color.FromArgb(
                Mathf.RoundToInt(m_AlphaSlider.value * 255),
                m_RedChannelSlider.value,
                m_GreenChannelSlider.value,
                m_BlueChannelSlider.value).ToArgb().ToString("X8")));

            TryNotifyValueChanged(c);
        }

        void OnRedFChannelChanging(ChangingEvent<float> evt)
        {
            var c = new Color(evt.newValue, m_GreenFChannelSlider.value, m_BlueFChannelSlider.value);
            Color.RGBToHSV(c, out var h, out var s, out var v);
            c.a = m_AlphaSlider.value;
            m_Wheel.SetValueWithoutNotify(h);
            m_SvSquare.referenceHue = h;
            m_SvSquare.SetValueWithoutNotify(new Vector2(s, v));
            m_AlphaSlider.colorRange = new List<ColorEntry>
            {
                new ColorEntry(new Color(c.r, c.g, c.b, 0), 0),
                new ColorEntry(new Color(c.r, c.g, c.b, 1), 1)
            };
            m_RedChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.r * 255));
            m_HueSlider.SetValueWithoutNotify(h);
            m_SaturationSlider.SetValueWithoutNotify(s);
            m_BrightnessSlider.SetValueWithoutNotify(v);
            m_HexField.SetValueWithoutNotify(ColorExtensions.ArgbToRgbaHex(
                System.Drawing.Color.FromArgb(
                Mathf.RoundToInt(m_AlphaSlider.value * 255),
                m_RedChannelSlider.value,
                m_GreenChannelSlider.value,
                m_BlueChannelSlider.value).ToArgb().ToString("X8")));
            TryNotifyValueChanged(c);
        }

        void OnRedChannelChanging(ChangingEvent<int> evt)
        {
            var c = new Color(evt.newValue / 255f, m_GreenChannelSlider.value / 255f, m_BlueChannelSlider.value / 255f);
            Color.RGBToHSV(c, out var h, out var s, out var v);
            c.a = m_AlphaSlider.value;
            m_Wheel.SetValueWithoutNotify(h);
            m_SvSquare.referenceHue = h;
            m_SvSquare.SetValueWithoutNotify(new Vector2(s, v));
            m_AlphaSlider.colorRange = new List<ColorEntry>
            {
                new ColorEntry(new Color(c.r, c.g, c.b, 0), 0),
                new ColorEntry(new Color(c.r, c.g, c.b, 1), 1)
            };
            m_RedFChannelSlider.SetValueWithoutNotify(c.r);
            m_HueSlider.SetValueWithoutNotify(h);
            m_SaturationSlider.SetValueWithoutNotify(s);
            m_BrightnessSlider.SetValueWithoutNotify(v);
            m_HexField.SetValueWithoutNotify(ColorExtensions.ArgbToRgbaHex(
                System.Drawing.Color.FromArgb(
                Mathf.RoundToInt(m_AlphaSlider.value * 255),
                m_RedChannelSlider.value,
                m_GreenChannelSlider.value,
                m_BlueChannelSlider.value).ToArgb().ToString("X8")));
            TryNotifyValueChanged(c);
        }

        void OnGreenFChannelChanging(ChangingEvent<float> evt)
        {
            var c = new Color(m_RedFChannelSlider.value, evt.newValue, m_BlueFChannelSlider.value);
            Color.RGBToHSV(c, out var h, out var s, out var v);
            c.a = m_AlphaSlider.value;
            m_Wheel.SetValueWithoutNotify(h);
            m_SvSquare.referenceHue = h;
            m_SvSquare.SetValueWithoutNotify(new Vector2(s, v));
            m_AlphaSlider.colorRange = new List<ColorEntry>
            {
                new ColorEntry(new Color(c.r, c.g, c.b, 0), 0),
                new ColorEntry(new Color(c.r, c.g, c.b, 1), 1)
            };
            m_GreenChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.g * 255));
            m_HueSlider.SetValueWithoutNotify(h);
            m_SaturationSlider.SetValueWithoutNotify(s);
            m_BrightnessSlider.SetValueWithoutNotify(v);
            m_HexField.SetValueWithoutNotify(ColorExtensions.ArgbToRgbaHex(
                System.Drawing.Color.FromArgb(
                Mathf.RoundToInt(m_AlphaSlider.value * 255),
                m_RedChannelSlider.value,
                m_GreenChannelSlider.value,
                m_BlueChannelSlider.value).ToArgb().ToString("X8")));
            TryNotifyValueChanged(c);
        }

        void OnGreenChannelChanging(ChangingEvent<int> evt)
        {
            var c = new Color(m_RedChannelSlider.value / 255f, evt.newValue / 255f, m_BlueChannelSlider.value / 255f);
            Color.RGBToHSV(c, out var h, out var s, out var v);
            c.a = m_AlphaSlider.value;
            m_Wheel.SetValueWithoutNotify(h);
            m_SvSquare.referenceHue = h;
            m_SvSquare.SetValueWithoutNotify(new Vector2(s, v));
            m_AlphaSlider.colorRange = new List<ColorEntry>
            {
                new ColorEntry(new Color(c.r, c.g, c.b, 0), 0),
                new ColorEntry(new Color(c.r, c.g, c.b, 1), 1)
            };
            m_GreenFChannelSlider.SetValueWithoutNotify(c.g);
            m_HueSlider.SetValueWithoutNotify(h);
            m_SaturationSlider.SetValueWithoutNotify(s);
            m_BrightnessSlider.SetValueWithoutNotify(v);
            m_HexField.SetValueWithoutNotify(ColorExtensions.ArgbToRgbaHex(
                System.Drawing.Color.FromArgb(
                Mathf.RoundToInt(m_AlphaSlider.value * 255),
                m_RedChannelSlider.value,
                m_GreenChannelSlider.value,
                m_BlueChannelSlider.value).ToArgb().ToString("X8")));
            TryNotifyValueChanged(c);
        }

        void OnBlueFChannelChanging(ChangingEvent<float> evt)
        {
            var c = new Color(m_RedFChannelSlider.value, m_GreenFChannelSlider.value, evt.newValue);
            Color.RGBToHSV(c, out var h, out var s, out var v);
            c.a = m_AlphaSlider.value;
            m_Wheel.SetValueWithoutNotify(h);
            m_SvSquare.referenceHue = h;
            m_SvSquare.SetValueWithoutNotify(new Vector2(s, v));
            m_AlphaSlider.colorRange = new List<ColorEntry>
            {
                new ColorEntry(new Color(c.r, c.g, c.b, 0), 0),
                new ColorEntry(new Color(c.r, c.g, c.b, 1), 1)
            };
            m_BlueChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.b * 255));
            m_HueSlider.SetValueWithoutNotify(h);
            m_SaturationSlider.SetValueWithoutNotify(s);
            m_BrightnessSlider.SetValueWithoutNotify(v);
            m_HexField.SetValueWithoutNotify(ColorExtensions.ArgbToRgbaHex(
                System.Drawing.Color.FromArgb(
                Mathf.RoundToInt(m_AlphaSlider.value * 255),
                m_RedChannelSlider.value,
                m_GreenChannelSlider.value,
                m_BlueChannelSlider.value).ToArgb().ToString("X8")));
            TryNotifyValueChanged(c);
        }

        void OnBlueChannelChanging(ChangingEvent<int> evt)
        {
            var c = new Color(m_RedChannelSlider.value / 255f, m_GreenChannelSlider.value / 255f, evt.newValue / 255f);
            Color.RGBToHSV(c, out var h, out var s, out var v);
            c.a = m_AlphaSlider.value;
            m_Wheel.SetValueWithoutNotify(h);
            m_SvSquare.referenceHue = h;
            m_SvSquare.SetValueWithoutNotify(new Vector2(s, v));
            m_AlphaSlider.colorRange = new List<ColorEntry>
            {
                new ColorEntry(new Color(c.r, c.g, c.b, 0), 0),
                new ColorEntry(new Color(c.r, c.g, c.b, 1), 1)
            };
            m_BlueFChannelSlider.SetValueWithoutNotify(c.b);
            m_HueSlider.SetValueWithoutNotify(h);
            m_SaturationSlider.SetValueWithoutNotify(s);
            m_BrightnessSlider.SetValueWithoutNotify(v);
            m_HexField.SetValueWithoutNotify(ColorExtensions.ArgbToRgbaHex(
                System.Drawing.Color.FromArgb(
                Mathf.RoundToInt(m_AlphaSlider.value * 255),
                m_RedChannelSlider.value,
                m_GreenChannelSlider.value,
                m_BlueChannelSlider.value).ToArgb().ToString("X8")));
            TryNotifyValueChanged(c);
        }

        void OnHueSliderChanging(ChangingEvent<float> evt)
        {
            var c = Color.HSVToRGB(evt.newValue, m_SaturationSlider.value, m_BrightnessSlider.value);
            c.a = m_AlphaSlider.value;
            m_Wheel.SetValueWithoutNotify(evt.newValue);
            m_SvSquare.referenceHue = evt.newValue;
            m_AlphaSlider.colorRange = new List<ColorEntry>
            {
                new ColorEntry(new Color(c.r, c.g, c.b, 0), 0),
                new ColorEntry(new Color(c.r, c.g, c.b, 1), 1)
            };
            m_RedChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.r * 255));
            m_GreenChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.g * 255));
            m_BlueChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.b * 255));
            m_RedFChannelSlider.SetValueWithoutNotify(c.r);
            m_GreenFChannelSlider.SetValueWithoutNotify(c.g);
            m_BlueFChannelSlider.SetValueWithoutNotify(c.b);
            m_HexField.SetValueWithoutNotify(ColorExtensions.ArgbToRgbaHex(
                System.Drawing.Color.FromArgb(
                Mathf.RoundToInt(m_AlphaSlider.value * 255),
                m_RedChannelSlider.value,
                m_GreenChannelSlider.value,
                m_BlueChannelSlider.value).ToArgb().ToString("X8")));
            TryNotifyValueChanged(c);
        }

        void OnSaturationSliderChanging(ChangingEvent<float> evt)
        {
            var c = Color.HSVToRGB(m_HueSlider.value, evt.newValue, m_BrightnessSlider.value);
            c.a = m_AlphaSlider.value;
            m_SvSquare.SetValueWithoutNotify(new Vector2(evt.newValue, m_BrightnessSlider.value));
            m_AlphaSlider.colorRange = new List<ColorEntry>
            {
                new ColorEntry(new Color(c.r, c.g, c.b, 0), 0),
                new ColorEntry(new Color(c.r, c.g, c.b, 1), 1)
            };
            m_RedChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.r * 255));
            m_GreenChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.g * 255));
            m_BlueChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.b * 255));
            m_RedFChannelSlider.SetValueWithoutNotify(c.r);
            m_GreenFChannelSlider.SetValueWithoutNotify(c.g);
            m_BlueFChannelSlider.SetValueWithoutNotify(c.b);
            m_HexField.SetValueWithoutNotify(ColorExtensions.ArgbToRgbaHex(
                System.Drawing.Color.FromArgb(
                Mathf.RoundToInt(m_AlphaSlider.value * 255),
                m_RedChannelSlider.value,
                m_GreenChannelSlider.value,
                m_BlueChannelSlider.value).ToArgb().ToString("X8")));
            TryNotifyValueChanged(c);
        }

        void OnBrightnessSliderChanging(ChangingEvent<float> evt)
        {
            var c = Color.HSVToRGB(m_HueSlider.value, m_SaturationSlider.value, evt.newValue);
            c.a = m_AlphaSlider.value;
            m_SvSquare.SetValueWithoutNotify(new Vector2(m_SaturationSlider.value, evt.newValue));
            m_AlphaSlider.colorRange = new List<ColorEntry>
            {
                new ColorEntry(new Color(c.r, c.g, c.b, 0), 0),
                new ColorEntry(new Color(c.r, c.g, c.b, 1), 1)
            };
            m_RedChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.r * 255));
            m_GreenChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.g * 255));
            m_BlueChannelSlider.SetValueWithoutNotify(Mathf.RoundToInt(c.b * 255));
            m_RedFChannelSlider.SetValueWithoutNotify(c.r);
            m_GreenFChannelSlider.SetValueWithoutNotify(c.g);
            m_BlueFChannelSlider.SetValueWithoutNotify(c.b);
            m_HexField.SetValueWithoutNotify(ColorExtensions.ArgbToRgbaHex(
                System.Drawing.Color.FromArgb(
                Mathf.RoundToInt(m_AlphaSlider.value * 255),
                m_RedChannelSlider.value,
                m_GreenChannelSlider.value,
                m_BlueChannelSlider.value).ToArgb().ToString("X8")));
            TryNotifyValueChanged(c);
        }

        void OnAlphaChannelValueChanging(ChangingEvent<float> evt)
        {
            var c = new Color(m_RedChannelSlider.value / 255f, m_GreenChannelSlider.value / 255f, m_BlueChannelSlider.value / 255f, evt.newValue);
            m_HexField.SetValueWithoutNotify(ColorExtensions.ArgbToRgbaHex(
                System.Drawing.Color.FromArgb(
                Mathf.RoundToInt(m_AlphaSlider.value * 255),
                m_RedChannelSlider.value,
                m_GreenChannelSlider.value,
                m_BlueChannelSlider.value).ToArgb().ToString("X8")));
            TryNotifyValueChanged(c);
        }

        void TryNotifyValueChanged(Color current)
        {
            if (current != m_Value)
            {
                var prev = m_Value;
                m_Value = current;
                m_Toolbar.currentColor = m_Value;
                using var changeEvent = ChangeEvent<Color>.GetPooled(prev, m_Value);
                changeEvent.target = this;
                SendEvent(changeEvent);
            }
        }

        /// <summary>
        /// Class used to create a <see cref="ColorPicker"/> using UXML.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<ColorPicker, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="ColorPicker"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_ShowAlpha = new UxmlBoolAttributeDescription
            {
                name = "show-alpha",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_ShowToolbar = new UxmlBoolAttributeDescription
            {
                name = "show-toolbar",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_ShowHex = new UxmlBoolAttributeDescription
            {
                name = "show-hex",
                defaultValue = false
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                m_PickingMode.defaultValue = PickingMode.Ignore;
                base.Init(ve, bag, cc);

                var el = (ColorPicker)ve;
                el.showAlpha = m_ShowAlpha.GetValueFromBag(bag, cc);
                el.showToolbar = m_ShowToolbar.GetValueFromBag(bag, cc);
                el.showHex = m_ShowHex.GetValueFromBag(bag, cc);
            }
        }
    }
}
