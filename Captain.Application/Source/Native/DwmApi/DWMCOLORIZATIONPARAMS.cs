// ReSharper disable All
namespace Captain.Application.Native {
  internal static partial class DwmApi {
    internal struct DWMCOLORIZATIONPARAMS {
      internal uint ColorizationColor,
                    ColorizationAfterglow,
                    ColorizationColorBalance,
                    ColorizationAfterglowBalance,
                    ColorizationBlurBalance,
                    ColorizationGlassReflectionIntensity,
                    ColorizationOpaqueBlend;
    }
  }
}
