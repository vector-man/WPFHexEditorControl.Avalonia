using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Xml.Linq;
using WpfHexEditor.Sample.MVVM.Contracts.App;
using WpfHexEditor.Sample.MVVM.Contracts.Common;

namespace WpfHexEditor.Sample.MVVM.App
{
    [Export(typeof(ILanguageService))]
    public class LanguageServiceImpl : ILanguageService {
        private LanguageProvider _currentProvider;
        public LanguageProvider CurrentProvider {
            get => _currentProvider;
            set {
                if (value == _currentProvider) {
                    return;
                }

                _currentProvider = value;
                InitializeLanguageDict();
            }
        }

        private List<LanguageProvider> _allProviders = new List<LanguageProvider>();
        public IEnumerable<LanguageProvider> AllProviders => _allProviders.Select(p => p);

        private ILanguageDict _languageDict;
        public string FindResourceString(string keyName) {
            if (string.IsNullOrEmpty(keyName)) {
                throw new ArgumentNullException(nameof(keyName));
            }

            if (_languageDict != null) {
                return (_languageDict[keyName] as string) ?? keyName;
            }

            return keyName;
        }

        public void Initialize() {
            InitializeDocument();
            InitilizeProviders();
            InitilizeCurrentProvider();
        }

        private string ConfigFileName => $"{Environment.CurrentDirectory}/{Constants.LanguageConfigName}";

        /// <summary>
        /// Initialize read configuration;
        /// </summary>
        private void InitializeDocument() {
            if (File.Exists(ConfigFileName)) {
                try {
                    _xDoc = XDocument.Load(ConfigFileName);
                }
                catch (Exception ex) {
                    LoggerService.WriteCallerLine(ex.Message);
                }
            }
        }

        private XDocument _xDoc;

        /// <summary>
        /// 初始化所有语言提供者;
        /// </summary>
        private void InitilizeProviders() {
            if (_xDoc == null) {
                return;
            }
            _allProviders.Clear();

            if (_xDoc == null) {
                return;
            }

            var elem = _xDoc.Root.Element(Constants.LanguageProviders);
            if (elem == null) {
                return;
            }

            var providerElems = elem.Elements(Constants.Provider);

            foreach (var pElem in providerElems) {
                var lanName = pElem.GetXElemValue(Constants.ProviderName);
                var lanType = pElem.GetXElemValue(Constants.ProviderType);
                var provider = new LanguageProvider(lanName, lanType);
                _allProviders.Add(provider);
            }
        }

        /// <summary>
        /// 初始化当前语言;
        /// </summary>
        private void InitilizeCurrentProvider() {
            if (_xDoc == null) {
                return;
            }

            var curLan = _xDoc.Root.GetXElemValue(Constants.CurrentLanguage);
            if (string.IsNullOrEmpty(curLan)) {
                return;
            }

            var provider = AllProviders.FirstOrDefault(p => p.Type == curLan);
            if (provider == null) {
                return;
            }

            CurrentProvider = provider;
        }

        /// <summary>
        /// 初始化语言,操作字典等;
        /// </summary>
        private void InitializeLanguageDict() {
            //try {
            //    var languageDict = new DirectoryInfo($"{Environment.CurrentDirectory}/Languages");
            //    foreach (var direct in languageDict.GetDirectories()) {
            //        //_ direct.Name
            //    } 
            //}
            //catch(Exception ex) {
            //    Contracts.App.LoggerService.WriteCallerLine(ex.Message);

            //}

            if (CurrentProvider == null) {
                return;
            }


            _languageDict = ServiceProvider.Current.GetInstance<ILanguageDict>();

            if (_languageDict == null) {
                return;
            }

            _languageDict.ClearMergedDictionaries();
            var dicts = new List<ResourceDictionary>();
            var providerDirect = $"{Environment.CurrentDirectory}/{Constants.LanguageDirect}/{CurrentProvider.Type}";
            if (!Directory.Exists(providerDirect)) {
                return;
            }

            var di = new DirectoryInfo(providerDirect);
            //遍历添加语言文件;
            foreach (var file in di.GetFiles()) {
                try {
                    _languageDict.AddMergedDictionaryFromPath($"{providerDirect}/{file.Name}");
                }
                catch (Exception ex) {
                    LoggerService.WriteCallerLine(ex.Message);
                }
            }
        }
    }


    [Export(typeof(ILanguageDict))]
    public class AppLanguageDictProvider : ILanguageDict {
        public string this[string keyName] => LanguageDict[keyName] as string;

        private ResourceDictionaryEx _languageDict;
        private ResourceDictionaryEx LanguageDict {
            get {
                if (_languageDict == null) {
                    _languageDict = Application.Current.Resources.MergedDictionaries.FirstOrDefault(p => {
                        if (p is ResourceDictionaryEx rsEx) {
                            return rsEx.Name == Constants.LanguageDict;
                        }
                        return false;
                    }) as ResourceDictionaryEx;
                }
                return _languageDict;
            }
        }

        public void AddMergedDictionaryFromPath(string path) {
            using (var rs = File.OpenRead(path)) {
                var res = XamlReader.Load(rs) as ResourceDictionary;
                LanguageDict.MergedDictionaries.Add(res);
            }
        }

        public void ClearMergedDictionaries() {
            LanguageDict.MergedDictionaries.Clear();
        }
    }

    public class ResourceDictionaryEx : ResourceDictionary {

        public string Name { get; set; }
    }
}
