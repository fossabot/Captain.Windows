#pragma once

#include <Windows.h>

using namespace System;
using namespace System::Collections::Generic;
using namespace Captain::Common;

namespace Captain {
  namespace Application {
    namespace NativeHelpers {
      public ref class HotkeyManager {
      private:
        HHOOK hhkKbd;

      public:
        HotkeyManager();
        ~HotkeyManager();

        Logger^ log;
        Dictionary<int, Func<bool, int, bool>^> ^actions = gcnew Dictionary<int, Func<bool, int, bool>^>;
        int keys = 0;

        void Register(int keys, Func<bool, int, bool>^ action);
      };
    }
  }
}

