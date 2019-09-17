﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using GalaSoft.MvvmLight.Ioc;
using MyDotNetCoreWpfApp.MVVMLight.Contracts.Services;
using MyDotNetCoreWpfApp.MVVMLight.Contracts.ViewModels;

namespace MyDotNetCoreWpfApp.MVVMLight.Services
{
    public class NavigationService : INavigationService
    {
        private Frame _frame;
        private object _lastExtraDataUsed;
        private readonly Dictionary<string, Type> _pages = new Dictionary<string, Type>();

        public event EventHandler<string> Navigated;        

        public bool CanGoBack
            => _frame != null && _frame.CanGoBack;

        public string CurrentPageKey
        {
            get
            {
                if (_frame.Content is FrameworkElement element)
                {
                    return element.DataContext.GetType().FullName;
                }

                return string.Empty;
            }
        }

        public void Configure(string viewModelName, Type pageType)
        {
            lock (_pages)
            {
                if (_pages.ContainsKey(viewModelName))
                {
                    throw new ArgumentException($"The key {viewModelName} is already configured in NavigationService");
                }

                if (_pages.Any(p => p.Value == pageType))
                {
                    throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == pageType).Key}");
                }

                _pages.Add(viewModelName, pageType);
            }
        }

        public void Initialize(Frame shellFrame)
        {
            if (_frame == null)
            {
                _frame = shellFrame;
                _frame.Navigated += OnNavigated;
                _frame.NavigationFailed += OnNavigationFailed;
            }
        }

        public void GoBack()
            => _frame.GoBack();

        public void NavigateTo(string pageKey)
            => NavigateTo(pageKey, null);

        public void NavigateTo(string pageKey, object parameter)
        {
            Type pageType;
            lock (_pages)
            {
                if (!_pages.TryGetValue(pageKey, out pageType))
                {
                    throw new ArgumentException($"Page not found: {pageKey}. Did you forget to call NavigationService.Configure?");
                }
            }
            if (_frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastExtraDataUsed)))
            {
                var page = SimpleIoc.Default.GetInstance(pageType);
                if (_frame.Content is FrameworkElement element)
                {
                    if (element.DataContext is INavigationAware navigationAware)
                    {
                        navigationAware.OnNavigatingFrom();
                    }
                }
                var navigated = _frame.Navigate(page, parameter);
                if (navigated)
                {
                    _lastExtraDataUsed = parameter;
                }
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is FrameworkElement element)
            {
                if (element.DataContext is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedTo(e.ExtraData);
                }

                Navigated?.Invoke(sender, element.DataContext.GetType().FullName);
            }
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
        }
    }
}
