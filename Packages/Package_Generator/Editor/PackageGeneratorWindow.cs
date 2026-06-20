using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Coolishbee.PackageGenerator.PackageDoc;
using Coolishbee.PackageGenerator.PackageSample;
using Coolishbee.PackageGenerator.Utils;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Coolishbee.PackageGenerator
{
    public sealed class PackageGeneratorWindow : EditorWindow
    {
        private enum GeneratorTab
        {
            PackageGeneration,
            Documentation,
            Sample
        }

        [MenuItem("Tools/UPM Package Generator")]
        public static void Open()
        {
            GetWindow<PackageGeneratorWindow>(true, "UPM Package Generator", true);
        }

        public static void ForceClose()
        {
            GetWindow<PackageGeneratorWindow>().Close();
        }

        [SerializeField] private PackageGenerationOption _option = new PackageGenerationOption();
        [SerializeField] private GeneratorTab _tab = GeneratorTab.PackageGeneration;
        [SerializeField] private bool _includeChangelog = true;
        [SerializeField] private string _sampleDisplayName = string.Empty;
        [SerializeField] private string _sampleDescription = string.Empty;

        [NonSerialized] private ListRequest _listRequest;
        [NonSerialized] private string[] _localPackages = Array.Empty<string>();
        [NonSerialized] private string _selectedPackageName;
        [NonSerialized] private PackageInfo _selectedPackageInfo;

        [NonSerialized] private Button _generationTabButton;
        [NonSerialized] private Button _documentationTabButton;
        [NonSerialized] private Button _sampleTabButton;
        [NonSerialized] private VisualElement _generationPage;
        [NonSerialized] private VisualElement _documentationPage;
        [NonSerialized] private VisualElement _samplePage;
        [NonSerialized] private VisualElement _documentationPackageContainer;
        [NonSerialized] private VisualElement _samplePackageContainer;
        [NonSerialized] private Label _documentationPackageInfo;
        [NonSerialized] private Label _samplePackageInfo;
        [NonSerialized] private TextField _locationField;
        [NonSerialized] private TextField _packageNameField;
        [NonSerialized] private Toggle _includeRuntimeTestsToggle;
        [NonSerialized] private Toggle _includeEditorTestsToggle;
        [NonSerialized] private VisualElement _testOptionsContainer;

        private void OnEnable()
        {
            minSize = new Vector2(780f, 520f);
            RefreshPackageList();
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();
            LoadStyleSheet();

            var root = new VisualElement { name = "package-generator-root" };
            root.AddToClassList("root");
            rootVisualElement.Add(root);

            root.Add(BuildHeader());

            var body = new VisualElement { name = "package-generator-body" };
            body.AddToClassList("body");
            root.Add(body);

            body.Add(BuildSideMenu());

            var contentScrollView = new ScrollView(ScrollViewMode.Vertical);
            contentScrollView.AddToClassList("content-scroll-view");
            body.Add(contentScrollView);

            _generationPage = BuildPackageGenerationPage();
            _documentationPage = BuildDocumentationPage();
            _samplePage = BuildSamplePage();

            contentScrollView.Add(_generationPage);
            contentScrollView.Add(_documentationPage);
            contentScrollView.Add(_samplePage);

            RefreshPackageSelectionViews();
            SelectTab(_tab);
        }

        private void LoadStyleSheet()
        {
            var stylePath = $"Packages/{PathUtil.CurrentPackageName}/Editor/UI/PackageGeneratorWindow.uss";
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylePath);
            if (styleSheet != null)
            {
                rootVisualElement.styleSheets.Add(styleSheet);
            }
        }

        private static VisualElement BuildHeader()
        {
            var header = new VisualElement();
            header.AddToClassList("header");
            header.Add(new Label("UPM Package Generator") { name = "window-title" });
            header.Add(new HelpBox("Create UPM packages, generate documentation projects, and register UPM samples.", HelpBoxMessageType.Info));
            return header;
        }

        private VisualElement BuildSideMenu()
        {
            var sideMenu = new VisualElement();
            sideMenu.AddToClassList("side-menu");

            _generationTabButton = BuildTabButton("Package Generation", GeneratorTab.PackageGeneration);
            _documentationTabButton = BuildTabButton("Documentation", GeneratorTab.Documentation);
            _sampleTabButton = BuildTabButton("Sample", GeneratorTab.Sample);

            sideMenu.Add(_generationTabButton);
            sideMenu.Add(_documentationTabButton);
            sideMenu.Add(_sampleTabButton);
            return sideMenu;
        }

        private Button BuildTabButton(string label, GeneratorTab tab)
        {
            var button = new Button(() => SelectTab(tab)) { text = label };
            button.AddToClassList("tab-button");
            return button;
        }

        private VisualElement BuildPackageGenerationPage()
        {
            var page = BuildPage("Package Generation", "Create a basic Unity UPM package structure.");
            page.Add(BuildLocationSection());
            page.Add(BuildPackageInfoSection());
            page.Add(BuildPackageOptionsSection());

            var generateButton = new Button(GeneratePackage) { text = "Generate Package" };
            generateButton.AddToClassList("primary-button");
            page.Add(generateButton);
            return page;
        }

        private VisualElement BuildDocumentationPage()
        {
            var page = BuildPage("Documentation", "Create a Documentation/docfx project for the selected local package.");
            page.Add(BuildSection("Target Package", section =>
            {
                _documentationPackageContainer = new VisualElement();
                section.Add(_documentationPackageContainer);
                _documentationPackageInfo = new Label();
                _documentationPackageInfo.AddToClassList("package-info");
                section.Add(_documentationPackageInfo);
            }));

            page.Add(BuildSection("Documentation Options", section =>
            {
                var includeChangelogToggle = new Toggle("Include Changelog") { value = _includeChangelog };
                includeChangelogToggle.RegisterValueChangedCallback(evt => _includeChangelog = evt.newValue);
                section.Add(includeChangelogToggle);
            }));

            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-row");
            buttonRow.Add(new Button(() =>
            {
                if (ValidateSelectedPackage())
                {
                    GenerateDocumentation();
                }
            }) { text = "Generate Documents" });
            page.Add(buttonRow);
            return page;
        }

        private VisualElement BuildSamplePage()
        {
            var page = BuildPage("Sample", "Create a UPM sample under the selected package's Samples~ folder and register it in package.json.");
            page.Add(BuildSection("Target Package", section =>
            {
                _samplePackageContainer = new VisualElement();
                section.Add(_samplePackageContainer);
                _samplePackageInfo = new Label();
                _samplePackageInfo.AddToClassList("package-info");
                section.Add(_samplePackageInfo);
            }));

            page.Add(BuildSection("Sample Information", section =>
            {
                var displayNameField = new TextField("Display Name") { value = _sampleDisplayName };
                displayNameField.RegisterValueChangedCallback(evt => _sampleDisplayName = evt.newValue.Trim());
                section.Add(displayNameField);

                var descriptionField = new TextField("Description") { value = _sampleDescription, multiline = true };
                descriptionField.AddToClassList("multiline-field");
                descriptionField.RegisterValueChangedCallback(evt => _sampleDescription = evt.newValue.Trim());
                section.Add(descriptionField);
            }));

            var generateButton = new Button(() =>
            {
                if (ValidateSampleInputs())
                {
                    GeneratePackageSample();
                }
            }) { text = "Generate Package Sample" };
            generateButton.AddToClassList("primary-button");
            page.Add(generateButton);
            return page;
        }

        private VisualElement BuildLocationSection()
        {
            return BuildSection("Location", section =>
            {
                var row = new VisualElement();
                row.AddToClassList("inline-row");

                _locationField = new TextField("Location") { value = _option.Location };
                _locationField.AddToClassList("grow-field");
                _locationField.RegisterValueChangedCallback(evt => SetString(ref _option.Location, evt.newValue.Trim()));
                row.Add(_locationField);

                row.Add(new Button(() => SetLocation(PackageGenerationOption.DefaultLocation)) { text = "Reset" });
                row.Add(new Button(() => SetLocation(PackageGenerationOption.LocalLocation)) { text = "Local" });
                row.Add(new Button(BrowseLocation) { text = "Browse" });
                section.Add(row);
            });
        }

        private VisualElement BuildPackageInfoSection()
        {
            return BuildSection("Package Information", section =>
            {
                var categoryField = new EnumField("Category", _option.Category);
                categoryField.RegisterValueChangedCallback(evt =>
                {
                    RecordModification();
                    _option.Category = (PackageCategory)evt.newValue;
                    RefreshPackageName();
                });
                section.Add(categoryField);

                var displayNameField = new TextField("Display Name") { value = _option.DisplayName };
                displayNameField.RegisterValueChangedCallback(evt =>
                {
                    SetString(ref _option.DisplayName, evt.newValue.Trim());
                    RefreshPackageName();
                });
                section.Add(displayNameField);

                _packageNameField = new TextField("Package Name") { value = _option.PackageName };
                _packageNameField.RegisterValueChangedCallback(evt => SetString(ref _option.PackageName, evt.newValue.Trim()));
                section.Add(_packageNameField);

                var descriptionField = new TextField("Description") { value = _option.Description };
                descriptionField.RegisterValueChangedCallback(evt => SetString(ref _option.Description, evt.newValue.Trim()));
                section.Add(descriptionField);

                var authorField = new TextField("Author") { value = _option.Author };
                authorField.RegisterValueChangedCallback(evt => SetString(ref _option.Author, evt.newValue.Trim()));
                section.Add(authorField);

                var unityVersionField = new TextField("Unity Version") { value = _option.UnityVersion };
                unityVersionField.RegisterValueChangedCallback(evt => SetString(ref _option.UnityVersion, evt.newValue.Trim()));
                section.Add(unityVersionField);
            });
        }

        private VisualElement BuildPackageOptionsSection()
        {
            return BuildSection("Generation Options", section =>
            {
                section.Add(BuildToggle("Include Runtime Assembly", _option.IncludeRuntime, value => _option.IncludeRuntime = value));
                section.Add(BuildToggle("Include Editor Assembly", _option.IncludeEditor, value => _option.IncludeEditor = value));
                section.Add(BuildToggle("Include Tests", _option.IncludeTests, value =>
                {
                    _option.IncludeTests = value;
                    if (value is false)
                    {
                        _option.IncludeRuntimeTests = false;
                        _option.IncludeEditorTests = false;
                        _includeRuntimeTestsToggle?.SetValueWithoutNotify(false);
                        _includeEditorTestsToggle?.SetValueWithoutNotify(false);
                    }
                    RefreshTestOptionsVisibility();
                }));

                _testOptionsContainer = new VisualElement();
                _testOptionsContainer.AddToClassList("indented-section");
                _includeRuntimeTestsToggle = BuildToggle("Include Runtime Tests", _option.IncludeRuntimeTests, value => _option.IncludeRuntimeTests = value);
                _includeEditorTestsToggle = BuildToggle("Include Editor Tests", _option.IncludeEditorTests, value => _option.IncludeEditorTests = value);
                _testOptionsContainer.Add(_includeRuntimeTestsToggle);
                _testOptionsContainer.Add(_includeEditorTestsToggle);
                section.Add(_testOptionsContainer);
                RefreshTestOptionsVisibility();
            });
        }

        private static VisualElement BuildPage(string title, string description)
        {
            var page = new VisualElement();
            page.AddToClassList("page");
            page.Add(new Label(title) { name = "page-title" });
            page.Add(new Label(description) { name = "page-description" });
            return page;
        }

        private static VisualElement BuildSection(string title, Action<VisualElement> buildContent)
        {
            var section = new VisualElement();
            section.AddToClassList("section");
            section.Add(new Label(title) { name = "section-title" });
            buildContent(section);
            return section;
        }

        private Toggle BuildToggle(string label, bool value, Action<bool> onChanged)
        {
            var toggle = new Toggle(label) { value = value };
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == evt.previousValue)
                {
                    return;
                }
                RecordModification();
                onChanged(evt.newValue);
            });
            return toggle;
        }

        private void SelectTab(GeneratorTab tab)
        {
            _tab = tab;
            SetPageVisibility(_generationPage, tab == GeneratorTab.PackageGeneration);
            SetPageVisibility(_documentationPage, tab == GeneratorTab.Documentation);
            SetPageVisibility(_samplePage, tab == GeneratorTab.Sample);
            SetTabSelected(_generationTabButton, tab == GeneratorTab.PackageGeneration);
            SetTabSelected(_documentationTabButton, tab == GeneratorTab.Documentation);
            SetTabSelected(_sampleTabButton, tab == GeneratorTab.Sample);
        }

        private static void SetPageVisibility(VisualElement page, bool visible)
        {
            if (page != null)
            {
                page.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private static void SetTabSelected(Button button, bool selected)
        {
            if (button == null)
            {
                return;
            }
            button.EnableInClassList("selected", selected);
        }

        private void SetLocation(string location)
        {
            RecordModification();
            _option.Location = location;
            _locationField?.SetValueWithoutNotify(location);
        }

        private void BrowseLocation()
        {
            var folder = EditorUtility.OpenFolderPanel("Select package generation location", _option.Location, string.Empty);
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }
            SetLocation(Path.GetRelativePath(Directory.GetCurrentDirectory(), folder).Replace('\\', '/').TrimEnd('/'));
        }

        private void GeneratePackage()
        {
            if (PackageGenerationOptionValidator.Validate(_option, out var error) is false)
            {
                EditorUtility.DisplayDialog("Warning", error, "OK");
                return;
            }

            if (EditorUtility.DisplayDialog("Confirm", $"A package will be created at the following path.\n\n{_option.PackageRoot}\n\nDo you want to continue?", "OK", "Cancel") is false)
            {
                return;
            }

            PackageGeneratorCore.Generate(_option);
            EditorUtility.OpenWithDefaultApp(_option.PackageRoot);
        }

        private void RefreshPackageList()
        {
            _listRequest = Client.List(false, false);
            EditorApplication.delayCall += RefreshPackageListProgress;
        }

        private void RefreshPackageListProgress()
        {
            if (_listRequest.IsCompleted is false)
            {
                EditorApplication.delayCall += RefreshPackageListProgress;
                return;
            }

            if (_listRequest.Status is not StatusCode.Success)
            {
                Debug.LogError($"Failed to get package list: {_listRequest.Error.message}");
                return;
            }

            _localPackages = _listRequest.Result
                .Where(package => package.source is PackageSource.Embedded or PackageSource.Local)
                .Select(package => package.name)
                .OrderBy(name => name)
                .ToArray();

            if (_localPackages.Length > 0 && string.IsNullOrEmpty(_selectedPackageName))
            {
                SetSelectedPackage(_localPackages[0]);
            }

            RefreshPackageSelectionViews();
        }

        private void RefreshPackageSelectionViews()
        {
            BuildPackageSelection(_documentationPackageContainer);
            BuildPackageSelection(_samplePackageContainer);
            RefreshSelectedPackageInfo();
        }

        private void BuildPackageSelection(VisualElement container)
        {
            if (container == null)
            {
                return;
            }

            container.Clear();
            if (_localPackages.Length == 0)
            {
                container.Add(new HelpBox("No local or embedded packages are available.", HelpBoxMessageType.Info));
                return;
            }

            var packageNames = _localPackages.ToList();
            var selectedIndex = Math.Max(0, packageNames.IndexOf(_selectedPackageName));
            var popup = new PopupField<string>("Package", packageNames, selectedIndex);
            popup.RegisterValueChangedCallback(evt => SetSelectedPackage(evt.newValue));
            container.Add(popup);
        }

        private void SetSelectedPackage(string packageName)
        {
            _selectedPackageName = packageName;
            _selectedPackageInfo = _listRequest?.Result?.FirstOrDefault(package => package.name == packageName);
            RefreshSelectedPackageInfo();
        }

        private void RefreshSelectedPackageInfo()
        {
            var text = _selectedPackageInfo == null
                ? string.Empty
                : $"Display Name: {_selectedPackageInfo.displayName}\nDescription: {_selectedPackageInfo.description}";
            if (_documentationPackageInfo != null)
            {
                _documentationPackageInfo.text = text;
            }
            if (_samplePackageInfo != null)
            {
                _samplePackageInfo.text = text;
            }
        }

        private bool ValidateSelectedPackage()
        {
            if (_selectedPackageInfo != null)
            {
                return true;
            }
            EditorUtility.DisplayDialog("Error", "Select a package.", "OK");
            return false;
        }

        private void GenerateDocumentation()
        {
            try
            {
                var projectRoot = PathUtil.GetProjectRootPath(_selectedPackageName);
                new DocGenerator().GenerateDocs(new GenerateDocsInputDto
                {
                    DestDirPath = projectRoot,
                    DisplayName = _selectedPackageInfo.displayName,
                    Description = _selectedPackageInfo.description,
                    IncludeChangelog = _includeChangelog
                });
                EditorUtility.DisplayDialog("Success", "Documentation generated successfully.", "OK");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to generate documentation: {e.Message}", "OK");
            }
        }

        private bool ValidateSampleInputs()
        {
            if (string.IsNullOrWhiteSpace(_selectedPackageName) ||
                string.IsNullOrWhiteSpace(_sampleDisplayName) ||
                string.IsNullOrWhiteSpace(_sampleDescription))
            {
                EditorUtility.DisplayDialog("Error", "Fill out all fields.", "OK");
                return false;
            }
            return true;
        }

        private void GeneratePackageSample()
        {
            try
            {
                new PackageSampleGenerator().GeneratePackageSample(new GeneratePackageSampleInputDto
                {
                    PackageName = _selectedPackageName,
                    DisplayName = _sampleDisplayName,
                    Description = _sampleDescription
                });
                EditorUtility.DisplayDialog("Success", "Package sample generated successfully.", "OK");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to generate package sample: {e.Message}", "OK");
            }
        }

        private void RefreshPackageName()
        {
            _option.PackageName = PackageGenerationOption.BuildPackageName(_option.Category, _option.DisplayName);
            _packageNameField?.SetValueWithoutNotify(_option.PackageName);
        }

        private void RefreshTestOptionsVisibility()
        {
            if (_testOptionsContainer != null)
            {
                _testOptionsContainer.style.display = _option.IncludeTests ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void SetString(ref string target, string value)
        {
            if (target == value)
            {
                return;
            }
            RecordModification();
            target = value;
        }

        private void RecordModification()
        {
            Undo.RecordObject(this, "Editor Modification");
            EditorUtility.SetDirty(this);
        }
    }
}
