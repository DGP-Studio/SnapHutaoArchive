// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Caching.Memory;
using Snap.Hutao.Core;
using Snap.Hutao.Core.DependencyInjection.Annotation.HttpClient;
using Snap.Hutao.Service.Yae.Achievement;
using System.IO;

namespace Snap.Hutao.Service.Feature;

[Service(ServiceLifetime.Singleton, typeof(IFeatureService))]
[HttpClient(HttpClientConfiguration.Default)]
internal sealed partial class FeatureService : IFeatureService
{
    private readonly JsonSerializerOptions jsonSerializerOptions;
    private readonly IMemoryCache memoryCache;

    [GeneratedConstructor]
    public partial FeatureService(IServiceProvider serviceProvider);

    public ValueTask<AchievementFieldId?> GetAchievementFieldIdAsync(string tag)
    {
        return GetTaggedFeatureAsync<AchievementFieldId>(tag, TimeSpan.FromHours(6));
    }

    private async ValueTask<TFeature?> GetTaggedFeatureAsync<TFeature>(string tag, TimeSpan expiration)
        where TFeature : class
    {
        string featureName = typeof(TFeature).Name;
        return await memoryCache.GetOrCreateAsync($"{nameof(FeatureService)}.{featureName}.{tag}", async entry =>
        {
            entry.SetSlidingExpiration(expiration);

            string path = Path.Combine(HutaoRuntime.GetDataRepositoryDirectory(), "Snap.ClientFeature", $"{featureName}_{tag}.json");
            using (FileStream stream = File.OpenRead(path))
            {
                return await JsonSerializer.DeserializeAsync<TFeature>(stream, jsonSerializerOptions).ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }
}