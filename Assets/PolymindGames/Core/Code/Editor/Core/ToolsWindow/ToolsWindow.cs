using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using PolymindGames;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace PolymindGamesEditor.ToolPages
{
    using UnityObject = UnityEngine.Object;

    public sealed class ToolsWindow : EditorWindow
    {
        [SerializeField]
        private TreeViewState _treeViewState;

        private PagesTreeView _pagesTreeView;
        private IEditorToolPage _selectedPage;
        private SearchField _searchField;


        public static void OpenPage(UnityObject unityObject)
        {
            var window = GetToolsWindow();
            window._pagesTreeView.FindAndSelectPageWithObject(unityObject);
        }
        
        [MenuItem("Tools/Polymind Games/Tools", priority = 1000)]
        private static void Init() => GetToolsWindow();
        
        private static ToolsWindow GetToolsWindow()
        {
            bool initPosition = !HasOpenInstances<ToolsWindow>();
            var window = GetWindow<ToolsWindow>();

            if (initPosition)
            {
                const float WINDOW_WIDTH = 1500f;
                const float WINDOW_HEIGHT = 800f;
                float x = (Screen.currentResolution.width - WINDOW_WIDTH) / 2f;
                float y = (Screen.currentResolution.height - WINDOW_HEIGHT) / 2f;
                window.position = new Rect(x, y, WINDOW_WIDTH, WINDOW_HEIGHT);

                const float WINDOW_MIN_WIDTH = 1200f;
                const float WINDOW_MIN_HEIGHT = 500f;
                window.minSize = new Vector2(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT);
            }
            
            window.titleContent = new GUIContent("Polymind Tools", Resources.Load<Texture2D>("Icons/Editor_PolymindLogoSmall"));

            return window;
        }

        private void OnEnable()
        {
            _treeViewState ??= new TreeViewState();
            _pagesTreeView = new PagesTreeView(_treeViewState);
            _searchField = new SearchField();
        }

        private void OnGUI()
        {
            using (new GUILayout.HorizontalScope())
            {
                float treeViewWidth = Mathf.Clamp(position.width * 0.15f, 0f, 250f);
                
                using (new GUILayout.VerticalScope(GuiStyles.Box))
                {
                    Rect rect = GUILayoutUtility.GetRect(29f, 200f, 18f, 18f, EditorStyles.toolbarSearchField, GUILayout.Width(treeViewWidth - 6f));
                    rect.x += 4f;
                    _pagesTreeView.searchString = _searchField.OnToolbarGUI(rect, _pagesTreeView.searchString);
                    
                    rect = GUILayoutUtility.GetRect(treeViewWidth, position.height - 16f, GUILayout.Width(treeViewWidth), GUILayout.Height(position.height - 34f));
                    _pagesTreeView.OnGUI(rect);
                }

                _pagesTreeView.Draw(position, treeViewWidth);
            }
        }

        #region Internal
        private sealed class PageTreeViewItem : TreeViewItem
        {
            public readonly IEditorToolPage Page;

            public PageTreeViewItem(IEditorToolPage page, int id, int depth, string displayName) : base(id, depth, displayName)
            {
                Page = page;
            }
        }

        private sealed class PagesTreeView : TreeView
        {
            private IEditorToolPage _selectedPage;
            
            public PagesTreeView(TreeViewState treeViewState) : base(treeViewState)
            {
                Reload();

                if (treeViewState.selectedIDs.Count > 0)
                {
                    SelectionChanged(treeViewState.selectedIDs);
                }
                else
                {
                    SetSelection(new[]
                    {
                        0
                    });
                    
                    var item = FindItem(0, rootItem);
                    _selectedPage = ((PageTreeViewItem)item).Page;
                }

                SetFocusAndEnsureSelectedItem();
                ExpandAll();
            }

            public void FindAndSelectPageWithObject(UnityObject unityObject)
            {
                if (unityObject == null)
                {
                    SetSelection(new[]
                    {
                        0
                    });
                }

                foreach (var item in GetRows())
                {
                    if (item is PageTreeViewItem pageItem)
                    {
                        if (pageItem.Page.IsCompatibleWithObject(unityObject))
                        {
                            _selectedPage = pageItem.Page;
                            SetSelection(new[]
                            {
                                item.id
                            });
                            return;
                        }
                    }
                }
            }

            public void Draw(Rect rect, float width)
            {
                if (_selectedPage == null)
                    return;
                
                bool disable = _selectedPage.DisableInPlaymode && Application.isPlaying;
                using (new EditorGUI.DisabledScope(disable))
                {
                    float pageWidth = rect.width - width;
                    Rect pageRect = new Rect(rect.x - pageWidth, rect.y, pageWidth, rect.height);
                    _selectedPage.DrawPage(pageRect);
                }
            }

            protected override void SelectionChanged(IList<int> selectedIds)
            {
                var item = FindItem(selectedIds[0], rootItem);
                _selectedPage = ((PageTreeViewItem)item).Page;
            }

            protected override TreeViewItem BuildRoot()
            {
                var root = CreateRootItem();
                var pages = CreatePages();
                SetupParentsAndChildrenFromDepths(root, pages);
                return root;
            }

            private static TreeViewItem CreateRootItem() => new()
            {
                id = -1,
                depth = -1,
                displayName = "Root"
            };

            private static IList<TreeViewItem> CreatePages()
            {
                var rootPages = CreateRootPages();
                var pagesList = new List<PageTreeViewItem>(rootPages.Length * 4);

                for (int i = 0; i < rootPages.Length; i++)
                    pagesList.AddRange(GetSubPages(rootPages[i], pagesList.Count, 0));

                // Convert List<PageTreeViewItem> to IList<TreeViewItem>
                IList<TreeViewItem> treeViewItems = pagesList.ConvertAll(item => (TreeViewItem)item);
                return treeViewItems;
            }

            private static IEditorToolPage[] CreateRootPages()
            {
                var pageTypes = typeof(RootPage).Assembly.GetTypes()
                    .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(RootPage))).ToArray();

                var pages = new IEditorToolPage[pageTypes.Length];
                for (int i = 0; i < pages.Length; i++)
                    pages[i] = (IEditorToolPage)Activator.CreateInstance(pageTypes[i]);
                
                pages.OrderArray();
                return pages;
            }
            
            private static IEnumerable<PageTreeViewItem> GetSubPages(IEditorToolPage toolPage, int id, int depth)
            {
                yield return new PageTreeViewItem(toolPage, id, depth, toolPage.DisplayName);
                foreach (var subPage in toolPage.GetSubPages().OrderBy(page => page.Order))
                {
                    // Recursively get sub-pages
                    foreach (var page in GetSubPages(subPage, ++id, depth + 1))
                    {
                        yield return page;
                    }
                }
            }
        }
        #endregion
    }
}