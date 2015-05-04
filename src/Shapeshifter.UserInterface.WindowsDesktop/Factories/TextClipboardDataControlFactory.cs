﻿using System;
using System.Windows;
using Shapeshifter.Core.Data;
using Shapeshifter.Core.Factories.Interfaces;
using Shapeshifter.UserInterface.WindowsDesktop.Controls.Clipboard;
using Shapeshifter.UserInterface.WindowsDesktop.Controls.Clipboard.ViewModels;
using Shapeshifter.UserInterface.WindowsDesktop.Factories.Interfaces;

namespace Shapeshifter.UserInterface.WindowsDesktop.Factories
{
    class TextClipboardDataControlFactory : IClipboardDataControlFactory
    {
        private readonly IDataSourceService dataSourceService;

        public TextClipboardDataControlFactory(IDataSourceService dataSourceService)
        {
            this.dataSourceService = dataSourceService;
        }

        public UIElement BuildControl(IClipboardData clipboardData)
        {
            var textData = (ClipboardTextData)clipboardData;

            return new ClipboardTextDataControl()
            {
                DataContext = new ClipboardTextDataViewModel(textData)
            };
        }

        public IClipboardData BuildData(string format, object data)
        {
            throw new NotImplementedException();
        }

        public bool CanBuildControl(IClipboardData data)
        {
            return data is ClipboardTextData;
        }

        public bool CanBuildData(string format)
        {
            return format == "CF_OEMTEXT" || format == "CF_TEXT" || format == "CF_UNICODETEXT";
        }
    }
}