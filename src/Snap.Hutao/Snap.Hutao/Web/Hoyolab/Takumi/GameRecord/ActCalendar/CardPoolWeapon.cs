// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Model.Primitive;

namespace Snap.Hutao.Web.Hoyolab.Takumi.GameRecord.ActCalendar;

internal sealed class CardPoolWeapon : CardPoolItem
{
    [JsonPropertyName("id")]
    public required WeaponId Id { get; set; }

    [JsonPropertyName("wiki_url")]
    public required Uri WikiUrl { get; set; }
}