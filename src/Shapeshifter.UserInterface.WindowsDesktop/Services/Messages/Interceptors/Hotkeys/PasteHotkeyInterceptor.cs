﻿using Shapeshifter.UserInterface.WindowsDesktop.Infrastructure.Logging.Interfaces;
using Shapeshifter.UserInterface.WindowsDesktop.Services.Api;
using Shapeshifter.UserInterface.WindowsDesktop.Services.Events;
using Shapeshifter.UserInterface.WindowsDesktop.Services.Keyboard.Interfaces;
using Shapeshifter.UserInterface.WindowsDesktop.Services.Messages.Factories.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;
using Shapeshifter.UserInterface.WindowsDesktop.Services.Messages.Interfaces;

namespace Shapeshifter.UserInterface.WindowsDesktop.Services.Keyboard
{
    [ExcludeFromCodeCoverage]
    class PasteHotkeyInterceptor : IPasteHotkeyInterceptor
    {
        readonly ILogger logger;
        readonly IHotkeyInterception hotkeyInterception;

        IntPtr windowHandle;

        public event EventHandler<HotkeyFiredArgument> HotkeyFired;

        public PasteHotkeyInterceptor(
            ILogger logger,
            IHotkeyInterceptionFactory hotkeyInterceptionFactory)
        {
            this.logger = logger;

            hotkeyInterception = hotkeyInterceptionFactory.CreateInterception(
                KeyboardApi.VK_KEY_V, true, true);
        }

        public void Install(IntPtr windowHandle)
        {
            //TODO: proper exceptions for install for ALL installs (don't install when already installed etc).
            this.windowHandle = windowHandle;
            hotkeyInterception.Start(windowHandle);
        }

        public void Uninstall()
        {
            //TODO: proper exceptions for uninstall for ALL uninstalls (don't uninstall when already uninstalled etc).
            hotkeyInterception.Stop(windowHandle);
        }

        public void ReceiveMessageEvent(WindowMessageReceivedArgument e)
        {
            if (e.Message == KeyboardApi.WM_HOTKEY && (int)e.WordParameter == hotkeyInterception.InterceptionId)
            {
                HandleHotkeyMessage();
            }
        }

        void HandleHotkeyMessage()
        {
            logger.Information("Paste hotkey message received.", 1);

            if (HotkeyFired != null)
            {
                HotkeyFired(this, new HotkeyFiredArgument(
                    hotkeyInterception.KeyCode, hotkeyInterception.ControlNeeded));
            }
        }
    }
}
