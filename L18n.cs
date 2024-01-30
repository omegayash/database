using System;
using System.Collections.Generic;
using System.Linq;

using Plugin.Settings;

namespace namespace.MultiLanguage
{
    public class L18n
    {
        public L18n()
        {
        }
        public static Dictionary<string, string> cache { get; set; }
        public static List<ResourceBundle> GetlangList { get; set; }
        /// <remarks>
        /// Maybe we can cache this info rather than querying every time
        /// </remarks>
        public static List<ResourceBundle> GetLang()
        {
            var MultiLang = new DatabaseHandler().getItems<ResourceBundle>();
            return MultiLang;
        }
        public static string Localize(string key, string LangID)
        {
            //    "pm.screen.key"
            // Platform-specific
            //  if(GetlangList==null)
            // {
            GetlangList = GetLang();
            // }
            if (GetlangList.Any(s => s.languageKey == key && s.language_id == LangID.ToString()))
            {
                var result = GetlangList.SingleOrDefault(s => s.languageKey == key && s.language_id == LangID.ToString()).text;
                return result;
            }
            CrossSettings.Current.AddOrUpdateValue("timespan", Convert.ToString(UtilityFunction.ConvertToUnixTimestamp(DateTime.Now)));
            return key + "???";
        }

    }
}
