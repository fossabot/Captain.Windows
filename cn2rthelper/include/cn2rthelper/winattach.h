#pragma once
#include <cn2/winattachinfo.h>

/// <summary>
///   Attaches a window
/// </summary>
/// <param name="pInfo">Window attachment information, usually passed by the injector process</param>
void RtAttachWindow(_In_ const PWINATTACHINFO pInfo);