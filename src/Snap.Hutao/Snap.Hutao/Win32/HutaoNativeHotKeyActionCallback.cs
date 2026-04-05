// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.UI.Input.HotKey;
using Snap.Hutao.Win32.Foundation;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Win32;

internal readonly unsafe partial struct HutaoNativeHotKeyActionCallback
{
    [GeneratedUnmanagedFunctionPointer]
    private readonly delegate* unmanaged[Stdcall]<BOOL, GCHandle<HotKeyCombination>, void> value;
}