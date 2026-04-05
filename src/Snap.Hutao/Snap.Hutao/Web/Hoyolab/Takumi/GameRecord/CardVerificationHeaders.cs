// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Web.Endpoint.Hoyolab;

namespace Snap.Hutao.Web.Hoyolab.Takumi.GameRecord;

internal sealed class CardVerificationHeaders
{
    public int ChallengeGame { get; private set; }

    public string ChallengePath { get; private set; } = string.Empty;

    public string Page { get; private set; } = string.Empty;

    public static CardVerificationHeaders CreateForIndex(IApiEndpoints apiEndpoints)
    {
        return Create(apiEndpoints.GameRecordIndexPath());
    }

    public static CardVerificationHeaders CreateForDailyNote(IApiEndpoints apiEndpoints)
    {
        return Create(apiEndpoints.GameRecordDailyNotePath(), $"{HoyolabOptions.ToolVersion}_#/ys/daily");
    }

    public static CardVerificationHeaders CreateForSpiralAbyss(IApiEndpoints apiEndpoints)
    {
        return Create(apiEndpoints.GameRecordSpiralAbyssPath(), $"{HoyolabOptions.ToolVersion}_#/ys/deep");
    }

    public static CardVerificationHeaders CreateForCharacterAll(IApiEndpoints apiEndpoints)
    {
        return Create(apiEndpoints.GameRecordCharacterList(), $"{HoyolabOptions.ToolVersion}_#/ys/role/all");
    }

    public static CardVerificationHeaders CreateForCharacterDetail(IApiEndpoints apiEndpoints)
    {
        return Create(apiEndpoints.GameRecordCharacterList(), $"{HoyolabOptions.ToolVersion}_#/ys/role/detail");
    }

    public static CardVerificationHeaders CreateForRoleCombat(IApiEndpoints apiEndpoints)
    {
        return Create(apiEndpoints.GameRecordRoleCombatPath(), $"{HoyolabOptions.ToolVersion}_#/ys/role-combat");
    }

    public static CardVerificationHeaders CreateForHardChallenge(IApiEndpoints apiEndpoints)
    {
        return Create(apiEndpoints.GameRecordHardChallengePath(), $"{HoyolabOptions.ToolVersion}_#/ys/challenge");
    }

    public static CardVerificationHeaders CreateForActCalendar(IApiEndpoints apiEndpoints)
    {
        return Create(apiEndpoints.GameRecordHardChallengePath(), $"{HoyolabOptions.ToolVersion}_#/ys/calendar");
    }

    private static CardVerificationHeaders Create(string path, string page = $"{HoyolabOptions.ToolVersion}_#/ys")
    {
        return new()
        {
            ChallengeGame = 2,
            ChallengePath = path,
            Page = page,
        };
    }
}