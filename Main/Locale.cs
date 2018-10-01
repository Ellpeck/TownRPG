using System;
using System.Collections.Generic;

namespace TownRPG.Main {
    public class Locale {

        private const string CurrentLanguage = "En";
        private static readonly Dictionary<string, Dictionary<string, string>> Locales = new Dictionary<string, Dictionary<string, string>>();

        public static string GetInterface(string key) {
            return Get("Interface", key);
        }

        public static string Get(string category, string key) {
            Dictionary<string, string> locale;
            if (!Locales.TryGetValue(category, out locale)) {
                locale = GameImpl.Instance.Content.Load<Dictionary<string, string>>("Locale/" + CurrentLanguage + '/' + category);
                Locales.Add(category, locale);
            }

            string value;
            if (locale.TryGetValue(key, out value)) {
                return value;
            } else {
                return category + '.' + key;
            }
        }

    }
}