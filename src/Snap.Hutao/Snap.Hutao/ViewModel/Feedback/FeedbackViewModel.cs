// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;
using Snap.Hutao.Core;
using Snap.Hutao.Core.DataTransfer;
using Snap.Hutao.Core.IO.Http.Loopback;
using Snap.Hutao.Core.IO.Http.Proxy;
using Snap.Hutao.Core.Logging;
using Snap.Hutao.Factory.ContentDialog;
using Snap.Hutao.Service;
using Snap.Hutao.Service.Notification;
using System.Runtime.InteropServices;
using Windows.System;

namespace Snap.Hutao.ViewModel.Feedback;

[BindableCustomPropertyProvider]
[Service(ServiceLifetime.Scoped)]
internal sealed partial class FeedbackViewModel : Abstraction.ViewModel
{
    private readonly IContentDialogFactory contentDialogFactory;
    private readonly IClipboardProvider clipboardProvider;
    private readonly IServiceProvider serviceProvider;
    private readonly CultureOptions cultureOptions;
    private readonly ITaskContext taskContext;
    private readonly IMessenger messenger;

    [GeneratedConstructor]
    public partial FeedbackViewModel(IServiceProvider serviceProvider);

    public static HttpProxyUsingSystemProxy DynamicHttpProxy { get => HttpProxyUsingSystemProxy.Instance; }

    public partial RuntimeOptions RuntimeOptions { get; }

    public partial LoopbackSupport LoopbackSupport { get; }

    [Command("NavigateToUriCommand")]
    private static async Task NavigateToUri(string? uri)
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory2.CreateUI("Navigate to uri", "FeedbackViewModel.Command", [("uri", uri ?? "<null>")]));

        if (string.IsNullOrEmpty(uri))
        {
            return;
        }

        await Launcher.LaunchUriAsync(uri.ToUri());
    }

    [Command("CopyDeviceIdCommand")]
    private async Task CopyDeviceIdAsync()
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory.CreateUI("Copy device id", "FeedbackViewModel.Command"));

        try
        {
            await clipboardProvider.SetTextAsync(HutaoRuntime.DeviceId).ConfigureAwait(false);
            messenger.Send(InfoBarMessage.Success(SH.ViewModelSettingCopyDeviceIdSuccess));
        }
        catch (COMException ex)
        {
            messenger.Send(InfoBarMessage.Error(ex));
        }
    }

    [Command("EnableLoopbackCommand")]
    private async Task EnableLoopbackAsync()
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory.CreateUI("Enable Loopback", "FeedbackViewModel.Command"));

        ContentDialogResult result = await contentDialogFactory
            .CreateForConfirmCancelAsync(SH.ViewDialogFeedbackEnableLoopbackTitle, SH.ViewDialogFeedbackEnableLoopbackContent)
            .ConfigureAwait(false);

        if (result is ContentDialogResult.Primary)
        {
            await taskContext.SwitchToMainThreadAsync();
            LoopbackSupport.EnableLoopback();
        }
    }
}