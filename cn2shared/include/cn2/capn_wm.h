#pragma once
#include <Windows.h>

#define _WM_CAPN_BASE (WM_APP + 0x6A6A)

/// Captain-specific window messages
#define WM_CAPN_ATTACHWND   (_WM_CAPN_BASE)
#define WM_CAPN_DETACHWND   (_WM_CAPN_BASE + 1)

/// signature for WM_COPYDATA messages sent to c2rthelper
#define WM_COPYDATA_CAPNSIG 0xDECADE21