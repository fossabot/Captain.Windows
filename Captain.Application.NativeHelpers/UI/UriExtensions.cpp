#include <Windows.h>
#include <ShlObj.h>

#include <msclr/marshal.h>

#include "UriExtensions.h"

using namespace msclr::interop;
using namespace System::ComponentModel;
using namespace System::Diagnostics;
using namespace Captain::Common;

namespace Captain {
  namespace Application {
    namespace NativeHelpers {
      /// opens a URI
      void UriExtensions::Open(Uri ^uri) {
        if (uri->IsFile && (uri->Query->StartsWith("?select") || uri->Query->Contains("&select"))) {
          /* open a file explorer window with this file selected */
          Logger ^log = Logger::GetDefault();

          marshal_context ^ctx = gcnew marshal_context();
          LPCWSTR pszName = nullptr;
          HRESULT hr = NULL;
          PIDLIST_ABSOLUTE ppidl = { 0 };

          try {
            // get a pointer to the file name
            pszName = ctx->marshal_as<LPCWSTR>(uri->ToString());

            if (FAILED(hr = SHParseDisplayName(pszName, nullptr, &ppidl, 0, nullptr))) {
              log->WriteLine(LogLevel::Error, "SHParseDisplayName() failed; hr=0x{0:8x}", hr);
              throw gcnew Win32Exception(hr);
            }

            // get ID list from the file path
            //LPITEMIDLIST lpItemIdList = ILCreateFromPathW(pszName);

            // open the folder in file explorer
            if (FAILED(hr = SHOpenFolderAndSelectItems(ppidl, 0, nullptr, 0))) {
              log->WriteLine(LogLevel::Error, "SHOpenFolderAndSelectItems() failed; hr=0x{0:8x}", hr);
              throw gcnew Win32Exception(hr);
            }
          }
          finally {
            log->WriteLine(LogLevel::Debug, "releasing resources");
            delete ctx;
          }
        }
        else {
          Process::Start(uri->ToString());
        }
      }
    }
  }
}
