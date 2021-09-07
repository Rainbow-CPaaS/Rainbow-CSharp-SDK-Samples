using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MultiPlatformApplication.Helpers
{
    // TOTALLY REBASED - ORIGINAL: https://mallibone.com/post/a-simple-navigation-service-for-xamarinforms
    public class NavigationService
    {
        private readonly object _sync = new object();
        private readonly Dictionary<string, Type> _pagesByKey = new Dictionary<string, Type>();

        public void Configure(string pageKey, Type pageType)
        {
            lock (_sync)
            {
                if (_pagesByKey.ContainsKey(pageKey))
                {
                    _pagesByKey[pageKey] = pageType;
                }
                else
                {
                    _pagesByKey.Add(pageKey, pageType);
                }
            }
        }

        public string CurrentPageKey
        {
            get
            {
                lock (_sync)
                {
                    // TODO - manage modal stack

                    Page page = App.Current.MainPage.Navigation?.NavigationStack?.LastOrDefault();
                    if (page == null)
                        return null;

                    var pageType = page.GetType();

                    return _pagesByKey.ContainsValue(pageType)
                        ? _pagesByKey.First(p => p.Value == pageType).Key
                        : null;
                }
            }
        }

        public Page CurrentPage
        {
            get
            {
                lock (_sync)
                {
                    // TODO - manage modal stack
                    Page page = App.Current.MainPage.Navigation?.NavigationStack?.LastOrDefault();

                    return page;
                    
                }
            }
        }

        public async Task ReplaceCurrentPageAsync(string pageKey, bool animated = true)
        {
            await ReplaceCurrentPageAsync(pageKey, null, animated);
        }

        public async Task ReplaceCurrentPageAsync(string pageKey, object parameter, bool animated = true)
        {
            Page page = GetPage(pageKey);
            Page currentPage = CurrentPage;

            App.Current.MainPage.Navigation.InsertPageBefore(page, currentPage);

            await GoBack(animated);
        }

        public async Task GoBack(bool animated = true)
        {
            // TODO - manage Modal Page
            await App.Current.MainPage.Navigation.PopAsync(animated);
        }

        public async Task NavigateModalAsync(string pageKey, bool animated = true)
        {
            await NavigateModalAsync(pageKey, null, animated);
        }

        public async Task NavigateModalAsync(string pageKey, object parameter, bool animated = true)
        {
            var page = GetPage(pageKey, parameter);
            await App.Current.MainPage.Navigation.PushModalAsync(page);
        }

        public async Task NavigateAsync(string pageKey, bool animated = true)
        {
            await NavigateAsync(pageKey, null, animated);
        }

        public async Task NavigateAsync(string pageKey, object parameter, bool animated = true)
        {
            var page = GetPage(pageKey, parameter);
            await App.Current.MainPage.Navigation.PushAsync(page);
        }

        public Page GetPage(string pageKey, object parameter = null)
        {
            lock (_sync)
            {
                if (!_pagesByKey.ContainsKey(pageKey))
                {
                    throw new ArgumentException(
                        $"No such page: {pageKey}. Did you forget to call NavigationService.Configure?");
                }

                var type = _pagesByKey[pageKey];
                ConstructorInfo constructor;
                object[] parameters;

                if (parameter == null)
                {
                    constructor = type.GetTypeInfo()
                        .DeclaredConstructors
                        .FirstOrDefault(c => !c.GetParameters().Any());

                    parameters = new object[]
                    {
                    };
                }
                else
                {
                    constructor = type.GetTypeInfo()
                        .DeclaredConstructors
                        .FirstOrDefault(
                            c =>
                            {
                                var p = c.GetParameters();
                                return p.Length == 1
                                       && p[0].ParameterType == parameter.GetType();
                            });

                    parameters = new[]
                    {
                        parameter
                    };
                }

                if (constructor == null)
                {
                    throw new InvalidOperationException(
                        "No suitable constructor found for page " + pageKey);
                }

                var page = constructor.Invoke(parameters) as Page;

                NavigationPage.SetHasNavigationBar(page, false);
                NavigationPage.SetHasBackButton(page, false);

                return page;
            }
        }
    }
}
