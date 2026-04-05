// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Web.Hoyolab.Takumi.GameRecord.ActCalendar;

internal class ActOther : Act
{
    [JsonPropertyName("other_act_detail")]
    public ActOtherDetail? OtherActDetail { get; init; }
}