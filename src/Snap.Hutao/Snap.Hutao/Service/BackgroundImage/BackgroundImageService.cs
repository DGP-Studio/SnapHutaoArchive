// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Core;
using Snap.Hutao.Core.Graphics.Imaging;
using Snap.Hutao.UI;
using System.Collections.Frozen;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Graphics.Imaging;

namespace Snap.Hutao.Service.BackgroundImage;

[Service(ServiceLifetime.Singleton, typeof(IBackgroundImageService))]
internal sealed partial class BackgroundImageService : IBackgroundImageService
{
    private static readonly FrozenSet<string> AllowedFormats = [".bmp", ".gif", ".ico", ".jpg", ".jpeg", ".png", ".tiff", ".webp"];

    private readonly ITaskContext taskContext;
    private readonly AppOptions appOptions;

    private HashSet<string>? availableBackgroundPathSet;

    [GeneratedConstructor]
    public partial BackgroundImageService(IServiceProvider serviceProvider);

    public async ValueTask<ValueResult</* shouldRefresh */ bool, BackgroundImage?>> GetNextBackgroundImageAsync(BackgroundImage? previous, CancellationToken token = default)
    {
        // backgroundImageOptions.Wallpaper will also be set in this method if web wallpaper type is selected
        HashSet<string> availableBackgroundSet = await SkipOrInitAvailableBackgroundAsync(previous, token).ConfigureAwait(false);

        // The availableBackgroundSet will be empty if BackgroundImageType is None
        if (availableBackgroundSet.Count <= 0)
        {
            return new(previous is not null, default!);
        }

        string path = System.Random.Shared.GetItems([.. availableBackgroundSet], 1)[0];
        availableBackgroundSet.Remove(path);

        if (string.Equals(path, previous?.Path, StringComparison.OrdinalIgnoreCase))
        {
            return new(false, default!);
        }

        if (!File.Exists(path))
        {
            Debugger.Break();
            return new(false, default!);
        }

        FileStream fileStream;
        try
        {
            fileStream = File.OpenRead(path);
        }
        catch (IOException)
        {
            // The process cannot access the file '?' because it is being used by another process.
            return new(false, default!);
        }

        using (fileStream)
        {
            Rgba32 accentColor;
            try
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream.AsRandomAccessStream());
                using (SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Straight))
                {
                    accentColor = softwareBitmap.GetRgba32AccentColor();
                }
            }
            catch (COMException comException)
            {
                if (appOptions.BackgroundImageType.Value is not BackgroundImageType.LocalFolder)
                {
                    // For web wallpaper, skip invalid file, as users can't control the file
                    return new(false, default!);
                }

                // 0xC00D36BE MF_E_INVALID_FILE_FORMAT: Throw to let user know which file is invalid
                // E_FAIL
                if (comException.HResult is not unchecked((int)0x80004005))
                {
                    comException.Data.Add("FilePath", path);
                    comException.Data.Add("HResult", $"0x{comException.HResult:X8}");
                    throw;
                }

                return new(false, default!);
            }

            await taskContext.SwitchToMainThreadAsync();

            BackgroundImage background = new()
            {
                Path = path,
                AccentColor = accentColor,
                Luminance = accentColor.Luminance,
            };

            return new(true, background);
        }
    }

    private async ValueTask<HashSet<string>> SkipOrInitAvailableBackgroundAsync(BackgroundImage? previous, CancellationToken token = default)
    {
        switch (appOptions.BackgroundImageType.Value)
        {
            case BackgroundImageType.LocalFolder:
                {
                    if (availableBackgroundPathSet is not { Count: > 0 })
                    {
                        string backgroundFolder = HutaoRuntime.GetDataBackgroundDirectory();

                        availableBackgroundPathSet =
                        [
                            .. Directory
                                .EnumerateFiles(backgroundFolder, "*", SearchOption.AllDirectories)
                                .Where(path => AllowedFormats.Contains(Path.GetExtension(path)))
                        ];

                        // Why > 1: If only one file in the folder, don't change background
                        if (previous is not null && availableBackgroundPathSet.Count > 1)
                        {
                            availableBackgroundPathSet.Remove(previous.Path);
                        }
                    }

                    break;
                }

            default:
                availableBackgroundPathSet = [];
                break;
        }

        return availableBackgroundPathSet ??= [];
    }
}