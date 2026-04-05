// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using Snap.Hutao.Core.DataTransfer;
using Snap.Hutao.Core.DependencyInjection.Abstraction;
using Snap.Hutao.Core.Logging;
using Snap.Hutao.Core.Setting;
using Snap.Hutao.Service;
using Snap.Hutao.Service.Announcement;
using Snap.Hutao.Service.Notification;
using Snap.Hutao.Service.User;
using Snap.Hutao.UI.Xaml.Control.Card;
using Snap.Hutao.UI.Xaml.View.Card;
using Snap.Hutao.Web.Hoyolab.Bbs.Home;
using Snap.Hutao.Web.Hoyolab.Hk4e.Common.Announcement;
using Snap.Hutao.Web.Hoyolab.Takumi.Event.Miyolive;
using Snap.Hutao.Web.Hoyolab.Takumi.GameRecord;
using Snap.Hutao.Web.Hoyolab.Takumi.GameRecord.ActCalendar;
using Snap.Hutao.Web.Response;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Snap.Hutao.ViewModel.Home;

[BindableCustomPropertyProvider]
[Service(ServiceLifetime.Scoped)]
internal sealed partial class AnnouncementViewModel : Abstraction.ViewModel
{
    private readonly IAnnouncementService announcementService;
    private readonly IServiceProvider serviceProvider;
    private readonly CultureOptions cultureOptions;
    private readonly ITaskContext taskContext;
    private readonly AppOptions appOptions;

    [GeneratedConstructor]
    public partial AnnouncementViewModel(IServiceProvider serviceProvider);

    [ObservableProperty]
    public partial string GreetingText { get; set; } = SH.FormatViewPageHomeGreetingTextCommon2(LocalSetting.Get(SettingKeys.LaunchTimes, 0));

    [ObservableProperty]
    public partial ImmutableArray<CodeWrapper> RedeemCodes { get; set; } = [];

    [ObservableProperty]
    public partial List<CardReference>? Cards { get; set; }

    [ObservableProperty]
    public partial ActCalendar ActivityCalendar { get; set; }

    [ObservableProperty]
    public partial AnnouncementWrapper? Announcement { get; set; }

    [GeneratedRegex("act_id=(.*?)(?:&|$)")]
    private static partial Regex ActIdExtractor { get; }

    protected override ValueTask<bool> LoadOverrideAsync(CancellationToken token)
    {
        InitializeDashboard();
        InitializeMiyoliveCodeAsync(token).SafeForget();
        InitializeInGameAnnouncementAsync(token).SafeForget();
        InitializeActivityCalendarAsync(token).SafeForget();
        return ValueTask.FromResult(true);
    }

    [SuppressMessage("", "SH003")]
    private async Task InitializeInGameAnnouncementAsync(CancellationToken token)
    {
        try
        {
            AnnouncementWrapper? announcementWrapper = await announcementService.GetAnnouncementWrapperAsync(cultureOptions.LanguageCode, appOptions.Region.Value, token).ConfigureAwait(false);
            await taskContext.SwitchToMainThreadAsync();
            Announcement = announcementWrapper;
            DeferContentLoader?.Load(nameof(UI.Xaml.View.Page.AnnouncementPage.GameAnnouncementPivot));
        }
        catch (OperationCanceledException)
        {
        }
    }

    [SuppressMessage("", "SH003")]
    private async Task InitializeMiyoliveCodeAsync(CancellationToken token)
    {
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            IUserService userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            if (await userService.GetCurrentUserAndUidAsync().ConfigureAwait(false) is not { IsOversea: false } userAndUid)
            {
                // The oversea user can direct use their code on the official website.
                return;
            }

            IHomeClient homeClient = scope.ServiceProvider
                .GetRequiredService<IOverseaSupportFactory<IHomeClient>>()
                .CreateFor(userAndUid);

            Response<NewHomeNewInfo> newHomeInfoResponse = await homeClient.GetNewHomeInfoAsync(2, token).ConfigureAwait(false);

            if (!ResponseValidator.TryValidateWithoutUINotification(newHomeInfoResponse, out NewHomeNewInfo? newHomeInfo))
            {
                return;
            }

            Uri url;
            if (newHomeInfo.Lives is [{ Data.LiveUrl: { } url1 }, ..])
            {
                url = url1;
            }
            else if (newHomeInfo.Navigator.SingleOrDefault(nav => nav.Name.EqualsAny(["直播兑换码", "前瞻直播"], StringComparison.OrdinalIgnoreCase)) is { AppPath: { } url2 })
            {
                url = url2;
            }
            else
            {
                return;
            }

            if (ActIdExtractor.Match(url.OriginalString) is not { Success: true, Groups: [_, { Value: { } actId }, ..] })
            {
                return;
            }

            IMiyoliveClient miyoliveClient = scope.ServiceProvider
                .GetRequiredService<IOverseaSupportFactory<IMiyoliveClient>>()
                .CreateFor(userAndUid);

            Response<CodeListWrapper> codeListResponse = await miyoliveClient.RefreshCodeAsync(actId, token).ConfigureAwait(false);
            if (!ResponseValidator.TryValidateWithoutUINotification(codeListResponse, out CodeListWrapper? wrapper))
            {
                return;
            }

            ImmutableArray<CodeWrapper> wrappers = wrapper.CodeList.SelectAsArray(static wrapper => wrapper.WithTitle(wrapper.Title.DecodeHtml()));
            wrappers = [.. wrappers.Where(static wrapper => !string.IsNullOrEmpty(wrapper.Code))];
            if (wrappers.IsEmpty)
            {
                return;
            }

            await taskContext.SwitchToMainThreadAsync();
            RedeemCodes = wrappers;
        }
    }

    private async Task InitializeActivityCalendarAsync(CancellationToken token)
    {
        try
        {
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                IUserService userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                if (await userService.GetCurrentUserAndUidAsync().ConfigureAwait(false) is not { IsOversea: false } userAndUid)
                {
                    return;
                }

                IGameRecordClient gameRecordClient = scope.ServiceProvider
                    .GetRequiredService<IOverseaSupportFactory<IGameRecordClient>>()
                    .CreateFor(userAndUid);

                Response<ActCalendar> response = await gameRecordClient.GetActCalendarAsync(userAndUid, token).ConfigureAwait(false);

                if (ResponseValidator.TryValidateWithoutUINotification(response, out ActCalendar? calendar))
                {
                    await taskContext.SwitchToMainThreadAsync();
                    ActivityCalendar = calendar;
                    DeferContentLoader?.Load(nameof(UI.Xaml.View.Page.AnnouncementPage.ActivityCalendarGrid));
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    [Command("CopyCodeCommand")]
    private async Task CopyCodeToClipboardAsync(string? code)
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory.CreateUI("Copy redeem code to ClipBoard", "AnnouncementPage.Command"));

        if (string.IsNullOrEmpty(code))
        {
            return;
        }

        await serviceProvider.GetRequiredService<IClipboardProvider>().SetTextAsync(code).ConfigureAwait(false);
        serviceProvider.GetRequiredService<IMessenger>().Send(InfoBarMessage.Success(SH.ViewPageAnnouncementRedeemCodeCopySucceed));
    }

    private void InitializeDashboard()
    {
        List<CardReference> result = [];

        if (LocalSetting.Get(SettingKeys.IsHomeCardLaunchGamePresented, true))
        {
            result.Add(CardReference.Create(new LaunchGameCard(serviceProvider), SettingKeys.HomeCardLaunchGameOrder));
        }

        if (LocalSetting.Get(SettingKeys.IsHomeCardGachaStatisticsPresented, true))
        {
            result.Add(CardReference.Create(new GachaStatisticsCard(serviceProvider), SettingKeys.HomeCardGachaStatisticsOrder));
        }

        if (LocalSetting.Get(SettingKeys.IsHomeCardAchievementPresented, true))
        {
            result.Add(CardReference.Create(new AchievementCard(serviceProvider), SettingKeys.HomeCardAchievementOrder));
        }

        if (LocalSetting.Get(SettingKeys.IsHomeCardDailyNotePresented, true))
        {
            result.Add(CardReference.Create(new DailyNoteCard(serviceProvider), SettingKeys.HomeCardDailyNoteOrder));
        }

        if (LocalSetting.Get(SettingKeys.IsHomeCardCalendarPresented, true))
        {
            result.Add(CardReference.Create(new CalendarCard(serviceProvider), SettingKeys.HomeCardCalendarOrder));
        }

        if (LocalSetting.Get(SettingKeys.IsHomeCardSignInPresented, true))
        {
            result.Add(CardReference.Create(new SignInCard(serviceProvider), SettingKeys.HomeCardSignInOrder));
        }

        Cards = result.SortBy(r => r.Order);
    }
}