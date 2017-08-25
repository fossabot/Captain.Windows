#pragma once

using namespace System;
using namespace System::Runtime::CompilerServices;

namespace Captain {
  namespace Application {
    namespace NativeHelpers {
      [ExtensionAttribute]
      public ref class UriExtensions abstract sealed {
      public:
        [ExtensionAttribute]
        static void Open(Uri ^uri);
      };
    }
  }
}