using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.MediaFoundation;

namespace Captain.Application.Native {
  internal unsafe class Evr {
    /// <summary>
    /// <p> Creates a media sample that manages a Direct3D surface. </p>
    /// </summary>
    /// <param name="unkSurfaceRef"><dd> <p> A reference to the <strong><see cref="T:SharpDX.ComObject" /></strong> interface of the Direct3D surface. This parameter can be <strong><c>null</c></strong>. </p> </dd></param>
    /// <param name="sampleOut"><dd> <p> Receives a reference to the sample's <strong><see cref="T:SharpDX.MediaFoundation.Sample" /></strong> interface. The caller must release the interface.</p> </dd></param>
    /// <returns><p>If this function succeeds, it returns <strong><see cref="F:SharpDX.Result.Ok" /></strong>. Otherwise, it returns an <strong><see cref="T:SharpDX.Result" /></strong> error code.</p></returns>
    /// <remarks>
    /// <p>The media sample created by this function exposes the following interfaces in addition to <strong><see cref="T:SharpDX.MediaFoundation.Sample" /></strong>:</p><ul> <li> <strong><see cref="T:SharpDX.MediaFoundation.DesiredSample" /></strong> </li> <li> <strong><see cref="T:SharpDX.MediaFoundation.TrackedSample" /></strong> </li> </ul><p>If <em>pUnkSurface</em> is non-<strong><c>null</c></strong>, the sample contains a single media buffer, which holds a reference to the Direct3D surface. To get the Direct3D surface from the media buffer, call <strong><see cref="M:SharpDX.MediaFoundation.ServiceProvider.GetService(System.Guid,System.Guid)" /></strong> on the buffer, using the service identifier <see cref="F:SharpDX.MediaFoundation.MediaServiceKeys.Buffer" />. The media buffer does not implement <strong><see cref="T:SharpDX.MediaFoundation.Buffer2D" /></strong>, nor does it implement the <strong><see cref="M:SharpDX.MediaFoundation.MediaBuffer.Lock(System.Int32@,System.Int32@)" /></strong> and <strong>Unlock</strong> methods.</p><p>Alternatively, you can set <em>pUnkSurface</em> to <strong><c>null</c></strong>, and later add a DirectX surface buffer to the sample by calling <strong><see cref="M:SharpDX.MediaFoundation.Sample.AddBuffer(SharpDX.MediaFoundation.MediaBuffer)" /></strong>. To create a DirectX surface buffer, call <strong><see cref="M:SharpDX.MediaFoundation.MediaFactory.CreateDXSurfaceBuffer(System.Guid,SharpDX.ComObject,SharpDX.Mathematics.Interop.RawBool,SharpDX.MediaFoundation.MediaBuffer@)" /></strong>.</p>
    /// </remarks>
    /// <msdn-id>ms703859</msdn-id>
    /// <unmanaged>HRESULT MFCreateVideoSampleFromSurface([In] IUnknown* pUnkSurface,[Out] IMFSample** ppSample)</unmanaged>
    /// <unmanaged-short>MFCreateVideoSampleFromSurface</unmanaged-short>
    public static void CreateVideoSampleFromSurface(ComObject unkSurfaceRef, out Sample sampleOut) {
      IntPtr zero = IntPtr.Zero;
      Result sampleFromSurface =
        MFCreateVideoSampleFromSurface_((void*) (unkSurfaceRef?.NativePointer ?? IntPtr.Zero), &zero);
      sampleOut = zero == IntPtr.Zero ? null : new Sample(zero);
      sampleFromSurface.CheckError();
    }

    [DllImport("Evr.dll", EntryPoint = "MFCreateVideoSampleFromSurface", CallingConvention = CallingConvention.StdCall)]
    private static extern int MFCreateVideoSampleFromSurface_(void* arg0, void* arg1);
  }
}