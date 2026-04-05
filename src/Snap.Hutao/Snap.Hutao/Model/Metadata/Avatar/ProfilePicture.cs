// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Model.Intrinsic;
using Snap.Hutao.Model.Primitive;

namespace Snap.Hutao.Model.Metadata.Avatar;

internal sealed class ProfilePicture
{
    public required ProfilePictureId Id { get; init; }

    public required string Icon { get; init; }

    public required string Name { get; init; }

    public required ProfilePictureUnlockBy UnlockType { get; init; }

    /// <summary>
    /// <see cref="ProfilePictureUnlockBy.PROFILE_PICTURE_UNLOCK_BY_ITEM"/> -> <see cref="MaterialId"/>
    /// <br/>
    /// <see cref="ProfilePictureUnlockBy.PROFILE_PICTURE_UNLOCK_BY_AVATAR"/> -> <see cref="AvatarId"/>
    /// <br/>
    /// <see cref="ProfilePictureUnlockBy.PROFILE_PICTURE_UNLOCK_BY_COSTUME"/> -> <see cref="CostumeId"/>
    /// <br/>
    /// <see cref="ProfilePictureUnlockBy.PROFILE_PICTURE_UNLOCK_BY_PARENT_QUEST"/> -> <see cref="QuestId"/>
    /// </summary>
    public uint UnlockParameter { get; init; }
}
