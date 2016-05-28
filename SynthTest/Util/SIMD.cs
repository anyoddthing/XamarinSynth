using System;
using System.Runtime.InteropServices;

#if !__UNIFIED__
    using nfloat = global::System.Single;
    using nint = global::System.Int32;
    using nuint = global::System.UInt32;
#endif

namespace SynthTest
{
    // Ok this is obviously not plattform independent code ...
    public static class SIMD
    {
        private const string VecLibFramework = "/System/Library/Frameworks/Accelerate.framework/Frameworks/vecLib.framework/vecLib";

        public static unsafe void Add(float *input, float scalar, float *output, int elements)
        {
            vDSP_vsadd(input, 1, &scalar, output, 1, elements);
        }

        [DllImport (VecLibFramework, EntryPoint = "vDSP_vsadd")]
        unsafe static extern void vDSP_vsadd(
            float *input, nint inputStride, float *scalar, float *output, nint ouputStride, nint elements);

        public static unsafe void Mul(float *input1, float* input2, float *output, int elements)
        {
            vDSP_vmul(input1, 1, input2, 1, output, 1, elements);
        }

        [DllImport (VecLibFramework, EntryPoint = "vDSP_vmul")]
        unsafe static extern void vDSP_vmul(
            float *input1, nint input1Stride, float *input2, nint input2Stride, float *output, nint ouputStride, nint elements);

        public static unsafe void Mul(float *input, float scalar, float *output, int elements)
        {
            vDSP_vsmul(input, 1, &scalar, output, 1, elements);
        }

        [DllImport (VecLibFramework, EntryPoint = "vDSP_vsmul")]
        unsafe static extern void vDSP_vsmul(
            float *input, nint inputStride, float *scalar, float *output, nint ouputStride, nint elements);
    }
}

