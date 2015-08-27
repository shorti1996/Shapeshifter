﻿using Shapeshifter.UserInterface.WindowsDesktop.Factories.Interfaces;
using Shapeshifter.UserInterface.WindowsDesktop.Data.Interfaces;
using System.Collections.Generic;
using Shapeshifter.UserInterface.WindowsDesktop.Core.Data;
using Shapeshifter.UserInterface.WindowsDesktop.Services.Api;
using System.Linq;

namespace Shapeshifter.UserInterface.WindowsDesktop.Factories
{
    class ClipboardDataControlPackageFactory : IClipboardDataControlPackageFactory
    {
        readonly IClipboardHandleFactory clipboardSessionFactory;
        readonly IEnumerable<IClipboardDataControlFactory> dataFactories;

        public ClipboardDataControlPackageFactory(
            IEnumerable<IClipboardDataControlFactory> dataFactories,
            IClipboardHandleFactory clipboardSessionFactory)
        {
            this.dataFactories = dataFactories;
            this.clipboardSessionFactory = clipboardSessionFactory;
        }

        bool IsAnyFormatSupported(
            IEnumerable<uint> formats)
        {
            return dataFactories.Any(
                x => formats.Any(x.CanBuildData));
        }

        public IClipboardDataControlPackage Create()
        {
            using (clipboardSessionFactory.StartNewSession())
            {
                var formats = ClipboardApi.GetClipboardFormats();
                if (IsAnyFormatSupported(formats))
                {
                    return ConstructPackage(formats);
                }
                else
                {
                    return null;
                }
            }
        }

        IClipboardDataControlPackage ConstructPackage(IEnumerable<uint> formats)
        {
            var package = new ClipboardDataControlPackage();
            DecoratePackageWithClipboardData(formats, package);
            DecoratePackageWithControl(package);

            return package;
        }

        void DecoratePackageWithClipboardData(
            IEnumerable<uint> formats,
            ClipboardDataControlPackage package)
        {
            foreach (var format in formats)
            {
                foreach (var factory in dataFactories)
                {
                    DecoratePackageWithFormatDataUsingFactory(package, factory, format);
                }
            }
        }

        static void DecoratePackageWithFormatDataUsingFactory(ClipboardDataControlPackage package, IClipboardDataControlFactory factory, uint format)
        {
            if (factory.CanBuildData(format))
            {
                var rawData = ClipboardApi.GetClipboardDataBytes(format);

                var clipboardData = factory.BuildData(format, rawData);
                package.AddData(clipboardData);
            }
        }

        void DecoratePackageWithControl(ClipboardDataControlPackage package)
        {
            foreach (var factory in dataFactories)
            {
                foreach (var data in package.Contents)
                {
                    if (factory.CanBuildControl(data))
                    {
                        package.Control = factory.BuildControl(data);
                        break;
                    }
                }
            }
        }
    }
}