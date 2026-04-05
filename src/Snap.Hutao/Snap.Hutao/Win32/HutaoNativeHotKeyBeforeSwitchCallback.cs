// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Win32.Foundation;

namespace Snap.Hutao.Win32;

internal readonly unsafe partial struct HutaoNativeHotKeyBeforeSwitchCallback
{
    [GeneratedUnmanagedFunctionPointer]
    private readonly delegate* unmanaged[Stdcall]<BOOL> value;
}