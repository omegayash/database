using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace namespace
{
    [ContentProperty("Text")]
    public class TranslateExtension : IMarkupExtension
    {
        public TranslateExtension()
        {
        }
        public string Text { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text == null)
                return null;

            // Do your translation lookup here, using whatever method you require
            var translated = L18n.Localize(Text, App.LangID);

            return translated;
        }
    }
}
