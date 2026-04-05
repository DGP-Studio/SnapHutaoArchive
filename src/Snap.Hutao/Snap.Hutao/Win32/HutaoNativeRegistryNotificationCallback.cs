// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Win32;

internal readonly unsafe partial struct HutaoNativeRegistryNotificationCallback
{
    [GeneratedUnmanagedFunctionPointer]
    private readonly delegate* unmanaged[Stdcall]<nint, void> value;
}