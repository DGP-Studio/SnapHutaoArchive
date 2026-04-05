// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.UI.Windowing;
using Snap.Hutao.Win32.Foundation;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Win32;

internal readonly unsafe partial struct HutaoNativeWindowSubclassCallback
{
    [GeneratedUnmanagedFunctionPointer]
    private readonly delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, GCHandle<XamlWindowSubclass>, LRESULT*, BOOL> value;
}