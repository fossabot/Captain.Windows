#include <msclr/gcroot.h>

#include "HotkeyManager.h"
#include "../Utils/MonitorEnumerator.h"

using namespace msclr;
using namespace System::ComponentModel;

namespace Captain {
  namespace Application {
    namespace NativeHelpers {
      // HACK: wrapped HotkeyManager instance pointer.
      //       This may seem odd, but is safe as long as the keyboard hook is called on the
      //       same thread
      static gcroot<HotkeyManager^> *g_pgcHotkeyManager;

      static LRESULT CALLBACK KbdHookProc(int nCode, WPARAM wParam, LPARAM lParam) {
        if (nCode == HC_ACTION) {
          switch (wParam) {
          case WM_KEYDOWN:
          case WM_SYSKEYDOWN:
          case WM_KEYUP:
          case WM_SYSKEYUP: {
            KBDLLHOOKSTRUCT *pKbdHookStruct = reinterpret_cast<KBDLLHOOKSTRUCT*>(lParam);

            if (pKbdHookStruct->flags & LLKHF_UP) { (*g_pgcHotkeyManager)->keys &= ~pKbdHookStruct->vkCode; }
            else { (*g_pgcHotkeyManager)->keys |= pKbdHookStruct->vkCode; }

            if ((*g_pgcHotkeyManager)->actions->ContainsKey((*g_pgcHotkeyManager)->keys)) {
              // hotkey activated! (non-exclusive mode, nil monitor)
              if ((*g_pgcHotkeyManager)->actions[(*g_pgcHotkeyManager)->keys]->Invoke(false, -1)) {
                // "eat" key event
                return true;
              }
            }

            break;
          }
          }
        }

        return CallNextHookEx(nullptr, nCode, wParam, lParam);
      }

      HotkeyManager::HotkeyManager() {
        this->log = Logger::GetDefault();

        if (!(this->hhkKbd = SetWindowsHookEx(WH_KEYBOARD_LL, KbdHookProc, 0, 0))) {
          log->WriteLine(LogLevel::Error, "SetWindowsHookEx() failed; LE=0x{0:x8}", GetLastError());
          throw gcnew Win32Exception(GetLastError());
        }
        else {
          log->WriteLine(LogLevel::Debug, "low-level keyboard hook set");
        }

        g_pgcHotkeyManager = new gcroot<HotkeyManager^>(this);
      }
      HotkeyManager::~HotkeyManager() { UnhookWindowsHookEx(this->hhkKbd); }

      void HotkeyManager::Register(int keys, Func<bool, int, bool>^ action) {
        this->actions[keys] = action;
        log->WriteLine(LogLevel::Debug, "registered hotkey; keys=0x{0:x4}", keys);
      }
    }
  }
}
