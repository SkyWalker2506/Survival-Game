using System;
using System.Collections.Generic;
using PolymindGames;
using UnityEditor.IMGUI.Controls;

namespace PolymindGamesEditor
{
    public abstract class SearchableDataDefinitionList<T> : DataDefinitionList<T> where T : DataDefinition<T>
    {
        private readonly SearchField _searchField;
        private string _filter = string.Empty;


        protected SearchableDataDefinitionList(string listName, params DataDefinitionAction[] customActions) : base(
            listName, customActions)
        {
            _searchField = new SearchField();
        }
        
        protected override void SetDefinitions(T[] dataDefs)
        {
            int prevScriptableCount = Count;

            Definitions.Clear();

            if (dataDefs != null)
            {
                if (!string.IsNullOrEmpty(_filter))
                    Search(Definitions, dataDefs, _filter);
                else
                    Definitions.AddRange(dataDefs);
            }

            if (prevScriptableCount != Count)
            {
                int indexToSelect = SelectedIndex;

                if (_filter.Length > 3)
                    indexToSelect = GetMostSimilarIndex();

                SelectIndex(indexToSelect);
            }
        }

        protected void DrawSearchBar()
        {
            string prevSearch = _filter;
            _filter = _searchField.OnToolbarGUI(_filter);

            if (_filter != prevSearch)
                SetDefinitions(GetDefinitions != null ? GetDefinitions() : DataDefinition<T>.Definitions);
        }

        private static void Search(List<T> list, T[] options, string filter)
        {
            var simplifiedFilter = filter.ToLower();
            for (var i = 0; i < options.Length; i++)
            {
                var option = options[i].Name;
                if (string.IsNullOrEmpty(filter) || option.ToLower().Contains(simplifiedFilter))
                {
                    if (string.Equals(options[i].Name, filter, StringComparison.CurrentCultureIgnoreCase))
                        list.Insert(0, options[i]);
                    else
                        list.Add(options[i]);
                }
            }
        }

        private int GetMostSimilarIndex()
        {
            int mostSimilarValue = 1000;
            int mostSimilarIndex = 0;
            for (int i = 0; i < Definitions.Count; i++)
            {
                int value = _filter.DamerauLevenshteinDistanceTo(Definitions[i].Name);
                if (value < mostSimilarValue)
                {
                    mostSimilarIndex = i;
                    mostSimilarValue = value;
                }
            }

            return mostSimilarIndex;
        }
    }
}