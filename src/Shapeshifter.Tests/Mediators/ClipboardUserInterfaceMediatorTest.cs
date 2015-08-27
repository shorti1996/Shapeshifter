﻿using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Shapeshifter.Core.Data;
using Shapeshifter.UserInterface.WindowsDesktop.Controls.Clipboard.Interfaces;
using Shapeshifter.UserInterface.WindowsDesktop.Factories.Interfaces;
using Shapeshifter.UserInterface.WindowsDesktop.Mediators.Interfaces;
using Shapeshifter.UserInterface.WindowsDesktop.Services.Events;
using Shapeshifter.UserInterface.WindowsDesktop.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shapeshifter.Tests.Mediators
{
    [TestClass]
    public class ClipboardUserInterfaceMediatorTest : TestBase
    {
        [TestMethod]
        public void IsConnectedIsFalseIfClipboardHookIsNotConnected()
        {
            var container = CreateContainer(c =>
            {
                c.RegisterFake<IPasteCombinationDurationMediator>()
                    .IsConnected
                    .Returns(true);
            });

            var mediator = container.Resolve<IClipboardUserInterfaceMediator>();
            Assert.IsFalse(mediator.IsConnected);
        }

        [TestMethod]
        public void IsConnectedIsFalseIfKeyboardHookIsNotConnected()
        {
            var container = CreateContainer(c =>
            {
                c.RegisterFake<IPasteCombinationDurationMediator>()
                    .IsConnected
                    .Returns(false);
            });

            var mediator = container.Resolve<IClipboardUserInterfaceMediator>();
            Assert.IsFalse(mediator.IsConnected);
        }

        [TestMethod]
        public void IsConnectedIsTrueIfAllHooksAreConnected()
        {
            var container = CreateContainer(c =>
            {
                c.RegisterFake<IPasteCombinationDurationMediator>()
                    .IsConnected
                    .Returns(true);
            });

            var mediator = container.Resolve<IClipboardUserInterfaceMediator>();
            Assert.IsTrue(mediator.IsConnected);
        }

        [TestMethod]
        public void ConnectConnectsHotkeyHook()
        {
            var container = CreateContainer(c =>
            {
                c.RegisterFake<IClipboardCopyInterceptor>();
                c.RegisterFake<IPasteCombinationDurationMediator>();
            });

            var mediator = container.Resolve<IClipboardUserInterfaceMediator>();
            mediator.Connect();

            var fakeHotkeyHookService = container.Resolve<IPasteCombinationDurationMediator>();
            fakeHotkeyHookService.Received().Connect();
        }

        [TestMethod]
        public void DisconnectDisconnectsKeyboardHook()
        {
            var container = CreateContainer(c =>
            {
                c.RegisterFake<IPasteCombinationDurationMediator>()
                    .IsConnected
                    .Returns(true);
            });

            var mediator = container.Resolve<IClipboardUserInterfaceMediator>();
            mediator.Disconnect();

            var fakeKeyboardHookService = container.Resolve<IPasteCombinationDurationMediator>();
            fakeKeyboardHookService.Received().Disconnect();
        }

        [TestMethod]
        public void DataCopiedCausesMediatorToCreatePackage()
        {
            var container = CreateContainer(c =>
            {
                c.RegisterFake<IClipboardCopyInterceptor>();
                c.RegisterFake<IPasteCombinationDurationMediator>();

                var fakeFactory = Substitute.For<IClipboardDataControlFactory>();
                c.RegisterInstance<IEnumerable<IClipboardDataControlFactory>>(new[] { fakeFactory });
            });

            var mediator = container.Resolve<IClipboardUserInterfaceMediator>();
            mediator.Connect();

            var fakeClipboardHookService = container.Resolve<IClipboardCopyInterceptor>();
            fakeClipboardHookService.DataCopied += Raise.Event<EventHandler<DataCopiedEventArgument>>(fakeClipboardHookService, new DataCopiedEventArgument());

            Assert.AreEqual(1, mediator.ClipboardElements.Count());
        }

        [TestMethod]
        public void DataCopiedCausesMediatorToDecoratePackageWithData()
        {
            var fakeData = Substitute.For<IClipboardData>();

            var container = CreateContainer(c =>
            {
                c.RegisterFake<IClipboardCopyInterceptor>();
                c.RegisterFake<IPasteCombinationDurationMediator>();

                var fakeFactory = Substitute.For<IClipboardDataControlFactory>();
                fakeFactory
                    .CanBuildData(Arg.Any<uint>())
                    .Returns(true);

                fakeFactory
                    .BuildData(Arg.Any<uint>(), Arg.Any<byte[]>())
                    .Returns(fakeData);
                c.RegisterInstance<IEnumerable<IClipboardDataControlFactory>>(new[] { fakeFactory });
            });

            var mediator = container.Resolve<IClipboardUserInterfaceMediator>();
            mediator.Connect();

            var fakeClipboardHookService = container.Resolve<IClipboardCopyInterceptor>();
            fakeClipboardHookService.DataCopied += Raise.Event<EventHandler<DataCopiedEventArgument>>(fakeClipboardHookService, new DataCopiedEventArgument());

            var addedPackage = mediator.ClipboardElements.Single();
            var content = addedPackage.Contents.Single();
            Assert.AreSame(fakeData, content);
        }

        [TestMethod]
        public void DataCopiedCausesMediatorToDecoratePackageWithControl()
        {
            var fakeControl = Substitute.For<IClipboardControl>();

            var container = CreateContainer(c =>
            {
                c.RegisterFake<IClipboardCopyInterceptor>();
                c.RegisterFake<IPasteCombinationDurationMediator>();

                var fakeFactory = Substitute.For<IClipboardDataControlFactory>();
                fakeFactory
                    .CanBuildData(Arg.Any<uint>())
                    .Returns(true);

                fakeFactory
                    .CanBuildControl(Arg.Any<IClipboardData>())
                    .Returns(true);

                fakeFactory
                    .BuildControl(Arg.Any<IClipboardData>())
                    .Returns(fakeControl);
                c.RegisterInstance<IEnumerable<IClipboardDataControlFactory>>(new[] { fakeFactory });
            });

            var mediator = container.Resolve<IClipboardUserInterfaceMediator>();
            mediator.Connect();

            var fakeClipboardHookService = container.Resolve<IClipboardCopyInterceptor>();
            fakeClipboardHookService.DataCopied += Raise.Event<EventHandler<DataCopiedEventArgument>>(fakeClipboardHookService, new DataCopiedEventArgument());

            var addedPackage = mediator.ClipboardElements.Single();
            Assert.AreSame(fakeControl, addedPackage.Control);
        }

        [TestMethod]
        public void DataCopiedTriggersEvent()
        {
            var container = CreateContainer(c =>
            {
                c.RegisterFake<IClipboardCopyInterceptor>();
                c.RegisterFake<IPasteCombinationDurationMediator>();
            });

            var mediator = container.Resolve<IClipboardUserInterfaceMediator>();
            mediator.Connect();

            object eventSender = null;
            ControlEventArgument eventArgument = null;
            mediator.ControlAdded += (sender, e) =>
            {
                eventSender = sender;
                eventArgument = e;
            };
            
            var fakeClipboardHookService = container.Resolve<IClipboardCopyInterceptor>();
            fakeClipboardHookService.DataCopied += Raise.Event<EventHandler<DataCopiedEventArgument>>(fakeClipboardHookService, new DataCopiedEventArgument());

            var addedPackage = mediator.ClipboardElements.Single();
            Assert.IsNotNull(addedPackage);

            Assert.AreSame(mediator, eventSender);
            Assert.AreSame(addedPackage, eventArgument.Package);
        }
    }
}