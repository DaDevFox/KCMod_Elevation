using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using I2.Loc;

namespace Fox.Localization
{
    public class Localization
    {
        // Sample Data
            // "en, de, fr, es " +
            // "\n " +
            // "word, Word, Wort, Moût, Palabra" +
            // "\n" +
            // "bread, Bread, Brot, Pain, [Spanish] Bread"


        /// <summary>
        /// The current game language's id code
        /// </summary>
        public static string CurrentLanguage { get => LocalizationManager.CurrentLanguageCode; }

        private static Dictionary<string, Term> _termIndex = new Dictionary<string, Term>();
        private static string[] _languageIndex;

        private static KCModHelper _helper;

        private void Preload(KCModHelper helper)
        {
            _helper = helper;
        }


        /// <summary>
        /// <para>
        /// Sets up the localization given the comma-seperated data, 
        /// where the first row is the languages in correct order, 
        /// and each row is a term, with the term id being the first value and 
        /// the following being the term's translation in its respective langauges
        /// </para>
        /// <para> use \n as a seperator for rows and , as a seperator for items in a row</para>
        /// <para>Example - </para>
        /// <para>en, de, fr, es</para>
        /// <para>term_word, Word, Wort, Moût, Mosto</para>
        /// </summary>
        /// <param name="data"></param>
        public static void Init(string data)
        {
            string[] rows = data.Split('\n');
            for (int i = 0; i < rows.Length; i++)
            {
                if (i == 0)
                    _languageIndex = rows[i]
                        .Trim()
                        .Split(',');
                else
                {
                    string[] row = rows[i].Split(',');
                    string id = row[0];

                    List<string> list = row.ToList();
                    list.RemoveAt(0);
                    row = list.ToArray();

                    if (row.Length != _languageIndex.Length)
                    {
                        string _list = "";
                        for (int k = 0; k < row.Length; k++)
                        {
                            if (k != 0)
                                _list += ",";
                            _list += row[k];
                        }

                        _helper.Log($"Localization Error: row length not equal to amount of languages [{_list}] for untranslated languages, use placeholder [_untranslated]");
                    }
                    else
                    {
                        Term term = new Term(id.Trim(), row);
                        _termIndex.Add(id.Trim(), term);
                    }
                }
            }

            for (int i = 0; i < _languageIndex.Length; i++)
                _languageIndex[i] = _languageIndex[i].Trim();



        }

        /// <summary>
        /// Gets the internal index of the given language
        /// returns -1 if language not found
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static int GetLanguageIndex(string language)
        {
            for (int i = 0; i < _languageIndex.Length; i++)
                if (_languageIndex[i] == language)
                    return i;
            return -1;
        }


        /// <summary>
        /// Gets the translation of the given term in the current game language;
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public static string Get(string term)
        {
            return Get(term, LocalizationManager.CurrentLanguageCode);
        }

        /// <summary>
        /// Gets the translation of the given term in the given language
        /// </summary>
        /// <param name="term"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static string Get(string term, string language)
        {
            if (!_termIndex.ContainsKey(term))
            {
                _helper.Log($"Localization Error: Term not recognized [{term}]");
                return null;
            }

            int idx = GetLanguageIndex(language);

            if (idx == -1)
            {
                _helper.Log($"Localization Error: Language not recognized: [{language}]");
                return null;
            }

            string translation = _termIndex[term].GetTranslation(idx);



            return translation == "_untranslated" ? $"[{term}]: No Translation Available" : translation;
        }


        public class Term
        {
            public string id { get; private set; }
            private string[] languageIndex;

            public Term(string id, string[] languageIndex)
            {
                this.id = id;
                this.languageIndex = languageIndex;
            }

            public string GetTranslation(int language)
            {
                return language >= 0 && language < languageIndex.Length ? languageIndex[language] : null;
            }
        }
    }


}
