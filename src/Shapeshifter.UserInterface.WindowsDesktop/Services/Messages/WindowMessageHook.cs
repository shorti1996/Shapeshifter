﻿using Shapeshifter.UserInterface.WindowsDesktop.Services.Interfaces;
using System;
using Shapeshifter.UserInterface.WindowsDesktop.Services.Events;
using System.Windows.Interop;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using Shapeshifter.UserInterface.WindowsDesktop.Services.Messages.Interfaces;
using Shapeshifter.UserInterface.WindowsDesktop.Infrastructure.Logging.Interfaces;
using System.Linq;
using Shapeshifter.UserInterface.WindowsDesktop.Windows.Interfaces;

namespace Shapeshifter.UserInterface.WindowsDesktop.Services
{
    [ExcludeFromCodeCoverage]
    class WindowMessageHook : IWindowMessageHook, IDisposable
    {
        HwndSource hooker;

        IWindow connectedWindow;

        readonly IEnumerable<IWindowMessageInterceptor> windowMessageInterceptors;

        readonly ILogger logger;

        public bool IsConnected
        {
            get; private set;
        }

        private IntPtr MainWindowHandle
            => hooker.Handle;

        public WindowMessageHook(
            IEnumerable<IWindowMessageInterceptor> windowMessageInterceptors,
            ILogger logger)
        {
            this.windowMessageInterceptors = windowMessageInterceptors;
            this.logger = logger;

            logger.Information($"Window message hook was constructed using {windowMessageInterceptors.Count()} interceptors.");
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                UninstallInterceptors();
                UninstallWindowMessageHook();

                IsConnected = false;
            }
        }

        void UninstallWindowMessageHook()
        {
            hooker.RemoveHook(WindowHookCallback);
        }

        void UninstallInterceptors()
        {
            foreach (var interceptor in windowMessageInterceptors)
            {
                interceptor.Uninstall();
            }
        }

        public void Connect(IWindow target)
        {
            if (IsConnected)
            {
                throw new InvalidOperationException("The window message hook has already been connected.");
            }

            connectedWindow = target;

            InstallWindowMessageHook();
            InstallInterceptors();

            IsConnected = true;
        }

        void InstallInterceptors()
        {
            var mainWindowHandle = hooker.Handle;
            foreach (var interceptor in windowMessageInterceptors)
            {
                interceptor.Install(mainWindowHandle);
                logger.Information($"Installed interceptor {interceptor.GetType().Name}.");
            }
        }

        void InstallWindowMessageHook()
        {
            var hooker = connectedWindow.HandleSource;
            hooker.AddHook(WindowHookCallback);

            logger.Information($"Installed message hook.");

            this.hooker = hooker;
        }

        IntPtr WindowHookCallback(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            logger.Information($"Message received: [{hwnd}, {msg}, {wParam}, {lParam}]");

            foreach (var interceptor in windowMessageInterceptors)
            {
                var argument = new WindowMessageReceivedArgument(hwnd, msg, wParam, lParam);
                interceptor.ReceiveMessageEvent(argument);

                handled |= argument.Handled;

                logger.Information($"Message passed to interceptor {interceptor.GetType().Name}.");
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (hooker != null)
            {
                hooker.Dispose();
            }
        }
    }
}
