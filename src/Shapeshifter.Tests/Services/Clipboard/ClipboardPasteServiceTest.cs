﻿namespace Shapeshifter.WindowsDesktop.Services.Clipboard
{
    using System;
    using System.Threading.Tasks;

    using Autofac;

    using Interfaces;

    using Messages.Interceptors.Hotkeys.Interfaces;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NSubstitute;

    [TestClass]
    public class ClipboardPasteServiceTest: UnitTestFor<IClipboardPasteService>
    {
        [TestMethod]
        public async Task PasteDisablesAndEnablesPasteInterceptor()
        {
            await SystemUnderTest.PasteClipboardContentsAsync();

            var fakeInterceptor = Container.Resolve<IPasteHotkeyInterceptor>();
            fakeInterceptor.Received()
                           .Uninstall();
            fakeInterceptor.Received()
                           .Install(Arg.Any<IntPtr>());
        }
    }
}