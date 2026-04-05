// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Win32.Foundation;
using Snap.Hutao.Win32.UI.WindowsAndMessaging;

namespace Snap.Hutao.Win32;

internal readonly unsafe partial struct HutaoNativeInputLowLevelKeyboardSourceCallback
{
    [GeneratedUnmanagedFunctionPointer]
    private readonly delegate* unmanaged[Stdcall]<uint, KBDLLHOOKSTRUCT*, BOOL> value;
}