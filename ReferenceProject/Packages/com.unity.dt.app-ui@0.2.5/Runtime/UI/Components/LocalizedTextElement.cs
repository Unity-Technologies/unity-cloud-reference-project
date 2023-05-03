using System;
using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
#if UNITY_LOCALIZATION_PRESENT
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A localized text element.
    /// </summary>
    public class LocalizedTextElement : TextElement
    {
        /// <summary>
        /// The main USS class name of this element.
        /// </summary>
        public new static readonly string ussClassName = "appui-localized-text";

        string m_ReferenceText;

        IList<object> m_Variables;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LocalizedTextElement()
            : this(null) { }

        /// <summary>
        /// Constructor with a reference text.
        /// </summary>
        /// <param name="text"> The reference text to use when formatting the localized string. You can also use plain text for no translation. </param>
        public LocalizedTextElement(string text)
        {
            AddToClassList(ussClassName);

            this.text = text;

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        /// <summary>
        /// The reference text to use when formatting the localized string. You can also use plain text for no translation.
        /// </summary>
        public new string text
        {
            get => m_ReferenceText;
            set
            {
                m_ReferenceText = value;
                UpdateTextWithCurrentLocale();
            }
        }

        /// <summary>
        /// The variables to use when formatting the localized string.
        /// </summary>
        public IList<object> variables
        {
            get => m_Variables;
            set
            {
                m_Variables = value;
                UpdateTextWithCurrentLocale();
            }
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            if (evt.destinationPanel != null)
            {
                UpdateTextWithCurrentLocale();
# if UNITY_LOCALIZATION_PRESENT
                var settings = LocalizationSettings.GetInstanceDontCreateDefault();
                if (settings)
                {
                    settings.OnSelectedLocaleChanged += UpdateTextWithLocale;
                    var op = settings.GetInitializationOperation();
                    op.Completed += OnLocalizationConfigCompleted;
                }
# endif
            }
        }

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
#if UNITY_LOCALIZATION_PRESENT
            var settings = LocalizationSettings.GetInstanceDontCreateDefault();
            if (settings)
            {
                settings.OnSelectedLocaleChanged -= UpdateTextWithLocale;
                var op = settings.GetInitializationOperation();
                op.Completed -= OnLocalizationConfigCompleted;
            }
#endif
        }

        void UpdateTextWithCurrentLocale()
        {
#if UNITY_LOCALIZATION_PRESENT
            if (panel == null)
                return;

            var locale = this.GetContext().locale;

            if (!locale)
            {
                base.text = m_ReferenceText;
                return;
            }

            if (TryGetTableAndEntry(m_ReferenceText, out var table, out var entry))
            {
                var settings = LocalizationSettings.GetInstanceDontCreateDefault();
                var db = (settings) ? settings.GetStringDatabase() : null;
                if (db == null)
                {
                    base.text = m_ReferenceText;
                    return;
                }

                var dbEntryOp = db.GetTableEntryAsync(table, entry, locale);
                dbEntryOp.Completed += op =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        var dbEntry = op.Result.Entry;
                        if (dbEntry == null || dbEntry.IsSmart && (m_Variables == null || m_Variables.Count == 0))
                        {
                            base.text = m_ReferenceText;
                        }
                        else
                        {
                            base.text = dbEntry.GetLocalizedString(m_Variables);
                        }
                    }
                    else
                    {
                        base.text = m_ReferenceText;
                    }
                };
            }
            else
            {
                base.text = m_ReferenceText;
            }
#else
            base.text = m_ReferenceText;
#endif
        }
        
#if UNITY_LOCALIZATION_PRESENT
        void UpdateTextWithLocale(Locale obj)
        {
            UpdateTextWithCurrentLocale();
        }
        
        void OnLocalizationConfigCompleted(AsyncOperationHandle<LocalizationSettings> obj)
        {
            UpdateTextWithCurrentLocale();
        }
#endif

        /// <summary>
        /// Try to get the table and string reference from a reference text.
        /// The naming convention is `@table:tableEntry`.
        /// </summary>
        /// <param name="referenceText"> The reference text to parse. </param>
        /// <param name="table"> The table name. </param>
        /// <param name="tableEntry"> The table entry reference. </param>
        /// <returns> True if the reference text is valid. </returns>
        internal static bool TryGetTableAndEntry(string referenceText, out string table, out string tableEntry)
        {
            table = null;
            tableEntry = null;
            if (string.IsNullOrEmpty(referenceText) || !referenceText.Contains(":") || !referenceText.StartsWith("@"))
                return false;

            var split = referenceText[1..].Split(':');
            if (split.Length != 2)
                return false;

            table = split[0];
            tableEntry = split[1];
            return true;
        }

        /// <summary>
        /// Uxml factory for the <see cref="LocalizedTextElement"/>.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<LocalizedTextElement, UxmlTraits> { }

        /// <summary>
        /// Uxml traits for the <see cref="LocalizedTextElement"/>.
        /// </summary>
        public new class UxmlTraits : TextElementExtendedUxmlTraits
        {
            /// <summary>
            /// Initialize the <see cref="LocalizedTextElement"/> using the attribute bag.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize. </param>
            /// <param name="bag"> The attribute bag. </param>
            /// <param name="cc"> The creation context. </param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var element = (LocalizedTextElement)ve;
                element.text = ((TextElement)ve).text;
            }
        }
    }
}
