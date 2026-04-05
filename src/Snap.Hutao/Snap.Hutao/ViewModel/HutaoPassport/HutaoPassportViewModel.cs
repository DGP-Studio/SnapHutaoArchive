// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Core.Logging;
using Snap.Hutao.Service.Navigation;
using Snap.Hutao.UI.Xaml.View.Page;

namespace Snap.Hutao.ViewModel.HutaoPassport;

[Service(ServiceLifetime.Scoped)]
internal sealed partial class HutaoPassportViewModel : Abstraction.ViewModel
{
    private readonly INavigationService navigationService;

    [GeneratedConstructor]
    public partial HutaoPassportViewModel(IServiceProvider serviceProvider);

    [Command("OpenTestPageCommand")]
    private async Task OpenTestPageAsync()
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory.CreateUI("Navigate to TestPage", "HutaoPassportViewModel.Command"));
        await navigationService.NavigateAsync<TestPage>(NavigationExtraData.Default).ConfigureAwait(false);
    }
}