using System;
using System.Collections.Generic;
using System.Text;
using WpfHexEditor.Sample.MVVM.Contracts.Common;

namespace WpfHexEditor.Sample.MVVM.Contracts.App
{
    /// <summary>
    /// LanguageProvider;
    /// </summary>
    public class LanguageProvider {
        public LanguageProvider(string languageName, string lanType) {
            this.LanguageName = languageName;
            Type = lanType;
        }
        /// <summary>
        /// The type name of  the language (i.e. English United States,Simplified Chinese - 简体中文,);
        /// </summary>
        public string LanguageName { get; }

        /// <summary>
        /// Type Name(i.e. en_US for English United States,zh_CN for Simplified Chinese);
        /// </summary>
        public string Type { get; }
    }

    /// <summary>
    /// Language service contract;
    /// </summary>
    public interface ILanguageService {
        /// <summary>
        /// Find the language string with key name;
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        string FindResourceString(string keyName);

        /// <summary>
        /// The current language provider;
        /// </summary>
        LanguageProvider CurrentProvider { get; set; }

        /// <summary>
        /// All the language providers;
        /// </summary>
        IEnumerable<LanguageProvider> AllProviders { get; }

        /// <summary>
        /// Initialize,this method will be invoked in the begining time of the App;
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// 由于测试项目中无Application.Current对象,故单独抽象出一个接口提供被操作的语言资源字典;
    ///  被操作的语言相关资源字典对象;
    /// </summary>
    public interface ILanguageDict {
        string this[string keyName] { get; }
        /// <summary>
        /// 清除所有合并后字典;
        /// </summary>
        void ClearMergedDictionaries();
        /// <summary>
        /// 从指定的绝对路径读取资源字典,并合并;
        /// </summary>
        /// <param name="path"></param>
        void AddMergedDictionaryFromPath(string path);
    }
    public class LanguageService : GenericServiceStaticInstance<ILanguageService> {
        public static string FindResourceString(string keyName) => Current?.FindResourceString(keyName);
    }
}
