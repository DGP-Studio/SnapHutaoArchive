// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Core.Database;
using BindingUser = Snap.Hutao.ViewModel.User.User;
using EntityUser = Snap.Hutao.Model.Entity.User;

namespace Snap.Hutao.Service.User;

internal interface IUserService
{
    ITaskContext TaskContext { get; }

    ValueTask<AdvancedDbCollectionView<BindingUser, EntityUser>> GetUsersAsync();

    ValueTask<ValueResult<UserOptionResultKind, string?>> ProcessInputCookieAsync(InputCookie inputCookie);

    ValueTask<bool> RefreshCookieTokenAsync(EntityUser user);

    ValueTask RemoveUserAsync(BindingUser user);
}