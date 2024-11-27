////////////////////////////////////////////////////////////////////////////////////////
//                                                                                    //
//    C# interface to Eyedea ERImage                                                  //
// ---------------------------------------------------------------------------------- //
//                                                                                    //
// Copyright (c) 2017-2018 by Eyedea Recognition, s.r.o.                              //
//                                                                                    //
// Author: Eyedea Recognition, s.r.o.                                                 //
//                                                                                    //
// Contact:                                                                           //
//           web: http://www.eyedea.cz                                                //
//           email: info@eyedea.cz                                                    //
//                                                                                    //
// BSD License                                                                        //
// -----------------------------------------------------------------------------------//
// Copyright (c) 2018, Eyedea Recognition, s.r.o.                                     //
// All rights reserved.                                                               //
// Redistribution and use in source and binary forms, with or without modification,   //
// are permitted provided that the following conditions are met :                     //
//     1. Redistributions of source code must retain the above copyright notice,      //
//        this list of conditions and the following disclaimer.                       //
//     2. Redistributions in binary form must reproduce the above copyright notice,   //
//        this list of conditions and the following disclaimer in the documentation   //
//        and / or other materials provided with the distribution.                    //
//     3. Neither the name of the copyright holder nor the names of its contributors  //
//        may be used to endorse or promote products derived from this software       //
//        without specific prior written permission.                                  //
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"        //
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED  //
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. //
// IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,   //
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES(INCLUDING, BUT  //
// NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR //
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,  //
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT(INCLUDING NEGLIGENCE OR OTHERWISE)  //
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF             //
// THE POSSIBILITY OF SUCH DAMAGE.                                                    //
////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Eyedea.er {
    /// <summary>
    /// Specifies the color model of the <seealso cref="ERImage"/>.<para/>
    /// NOTE: ER_IMAGE_COLORMODEL_YCBCR* color model is supported as an crop image input only.
    /// </summary>
    public enum ERImageColorModel
    {
        /// <summary>
        /// Unknown color model.</summary>
        ER_IMAGE_COLORMODEL_UNK = 0,
        /// <summary>
        /// Gray color model (one channel).</summary>
        ER_IMAGE_COLORMODEL_GRAY = 1,
        /// <summary>
        /// BGR color model (three channels).</summary>
        ER_IMAGE_COLORMODEL_BGR = 2,
        /// <summary>
        /// YCbCr 4:2:0 color model (three planes).</summary>
        ER_IMAGE_COLORMODEL_YCBCR420 = 3,
        /// <summary>
        /// BGRA color model (four channels).</summary>
        ER_IMAGE_COLORMODEL_BGRA = 4,
        /// <summary>
        /// YCbCr NV12 color model (three planes).</summary>
        ER_IMAGE_COLORMODEL_YCBCRNV12 = 5
    };

    /// <summary>
    /// Specifies the data type of the <seealso cref="ERImage"/> image data.
    /// </summary>
    public enum ERImageDataType
    {
        /// <summary>
        /// Unknown data type.</summary>
        ER_IMAGE_DATATYPE_UNK = 0,
        /// <summary>
        /// Unsigned byte pixel channel representation.</summary>
        ER_IMAGE_DATATYPE_UCHAR = 1,
        /// <summary>
        /// Float pixel channel representation.</summary>
        ER_IMAGE_DATATYPE_FLOAT = 2
    };

    /// <summary>
    /// Computation mode.
    /// </summary>
    public enum ERComputationMode
    {
        /// <summary>
        /// Copmute using CPU.</summary>
        ER_COMPUTATION_MODE_CPU = 0,
        /// <summary>
        /// Copmute using GPU.</summary>
        ER_COMPUTATION_MODE_GPU = 1,
        /// <summary>
        /// Copmute using TPU.</summary>
        ER_COMPUTATION_MODE_TPU = 2
    };

    /// <summary>
    /// <seealso cref="ERImage"/> is a structure representing image.
    /// It contains channels and data type specification. <seealso cref="ERImage"/> is used as an input image structure in all Eyedentify functions.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct ERImage
    {
        /// <summary>Color model of the image.</summary>
        public ERImageColorModel color_model;
        /// <summary>Data type of the image.</summary>
        public ERImageDataType data_type;
        /// <summary>Width of the image.</summary>
        public UInt32 width;
        /// <summary>Height of the image.</summary>
        public UInt32 height;
        /// <summary>Number of channels.</summary>
        public UInt32 num_channels;
        /// <summary>Pixel depth in bytes.</summary>
        public UInt32 depth;
        /// <summary>Row byte step.</summary>
        public UInt32 step;
        /// <summary>Size of the image data (step * height).</summary>
        public UInt32 size;
        /// <summary>Size of the allocated data.</summary>
        public UInt32 data_size;
        /// <summary>Pointer to the image data.</summary>
        public IntPtr data;
        /// <summary>Double pointer to the image rows.</summary>
        public IntPtr row_data;
        /// <summary>Flag representing whether the data was allocated in the structure. (0 = no, 1 = yes)</summary>
        public byte data_allocated;
    };

    /// <summary>
    /// <seealso cref="ERRoI"/> is a structure representing Region of Interest.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct ERPoint2f
    {
        /// <summary>x.</summary>
        public float x;
        /// <summary>y.</summary>
        public float y;
    }

    /// <summary>
    /// <seealso cref="ERRoI"/> is a structure representing Region of Interest.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct ERRoI
    {
        /// <summary>x.</summary>
        public Int32 x;
        /// <summary>y.</summary>
        public Int32 y;
        /// <summary>width.</summary>
        public Int32 width;
        /// <summary>height.</summary>
        public Int32 height;
    };

    /// <summary>
    /// <seealso cref="ERRoI"/> is a structure representing Region of Interest.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct ERRotatedRect
    {
        /// <summary>center of gravity - x.</summary>
        public float x;
        /// <summary>center of gravity - y.</summary>
        public float y;
        /// <summary>width.</summary>
        public float width;
        /// <summary>height.</summary>
        public float height;
        /// <summary>angle.</summary>
        public float angle;
    };

    [Serializable]
    public class ERException : ApplicationException
    {
        private static string strEXHeader = "ERImage Exception: ";

        public ERException() { }
        public ERException(string message)
            : base(strEXHeader + message) { }
        public ERException(string message, System.Exception inner)
            : base(strEXHeader + message, inner) { }

        protected ERException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    };

    public class ERImageUtils {
        /// <summary>
        /// Native methods for explicit linking.
        /// </summary>
        static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string dllToLoad);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

            [DllImport("kernel32.dll")]
            public static extern bool FreeLibrary(IntPtr hModule);
        }

        ///////
        // ERImage API function types
        ///////
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate Int32  fcn_erImageAllocate(ERImage* image, UInt32 width, UInt32 height, ERImageColorModel color_model, ERImageDataType data_type);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate Int32  fcn_erImageAllocateBlank(ERImage* image, UInt32 width, UInt32 height, ERImageColorModel color_model, ERImageDataType data_type);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate Int32  fcn_erImageAllocateAndWrap(ERImage* image, UInt32 width, UInt32 height, ERImageColorModel color_model, ERImageDataType data_type, byte* data, UInt32 step);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate UInt32 fcn_erImageGetDataTypeSize(ERImageDataType data_type);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate UInt32 fcn_erImageGetColorModelNumChannels(ERImageColorModel color_model);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate UInt32 fcn_erImageGetPixelDepth(ERImageColorModel color_model, ERImageDataType data_type);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate Int32  fcn_erImageCopy(ERImage* image, ERImage* image_copy);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate Int32  fcn_erImageRead(ERImage* image, string filename);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate Int32  fcn_erImageWrite(ERImage* image, string filename);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate void   fcn_erImageFree(ERImage* image);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate string fcn_erVersion();

        ////////
        // Define dll and function pointers
        ////////
        IntPtr pDll = IntPtr.Zero;

        IntPtr pErImageAllocate                  =  IntPtr.Zero;
        //IntPtr pErImageAllocateBlank             =  IntPtr.Zero;
        //IntPtr pErImageAllocateAndWrap           =  IntPtr.Zero;
        //IntPtr pErImageGetDataTypeSize           =  IntPtr.Zero;
        //IntPtr pErImageGetColorModelNumChannels  =  IntPtr.Zero;
        //IntPtr pErImageGetPixelDepth             =  IntPtr.Zero;
        //IntPtr pErImageCopy                      =  IntPtr.Zero;
        IntPtr pErImageRead                      =  IntPtr.Zero;
        IntPtr pErImageWrite                     =  IntPtr.Zero;
        IntPtr pErImageFree                      =  IntPtr.Zero;
        //IntPtr pErVersion                        =  IntPtr.Zero;

        ///////////////
        // Define delegates of functions
        ///////////////
        fcn_erImageAllocate                 fcnErImageAllocate                 = null;
        //fcn_erImageAllocateBlank            fcnErImageAllocateBlank            = null;
        //fcn_erImageAllocateAndWrap          fcnErImageAllocateAndWrap          = null;
        //fcn_erImageGetDataTypeSize          fcnErImageGetDataTypeSize          = null;
        //fcn_erImageGetColorModelNumChannels fcnErImageGetColorModelNumChannels = null;
        //fcn_erImageGetPixelDepth            fcnErImageGetPixelDepth            = null;
        //fcn_erImageCopy                     fcnErImageCopy                     = null;
        fcn_erImageRead                     fcnErImageRead                     = null;
        fcn_erImageWrite                    fcnErImageWrite                    = null;
        fcn_erImageFree                     fcnErImageFree                     = null;
        //fcn_erVersion                       fcnErVersion                       = null;

        private IntPtr loadFunctionFromDLL(IntPtr dllPtr, string functionName) {
            IntPtr functionPtr = NativeMethods.GetProcAddress(dllPtr, functionName);
            if (functionPtr == IntPtr.Zero) {
                throw new ERException(functionName + " NULL");
            }

            return functionPtr;
        }

        private void loadLibraryFunctions(IntPtr pDll) {
            if (pDll == IntPtr.Zero) {
                throw new ERException("Loading library failed!");
            }

            //////////////////////////
            // load functions from dll
            //////////////////////////
            pErImageAllocate                 = loadFunctionFromDLL(pDll, "erImageAllocate");
            /*pErImageAllocateBlank            = loadFunctionFromDLL(pDll, "erImageAllocateBlank");
            pErImageAllocateAndWrap          = loadFunctionFromDLL(pDll, "erImageAllocateAndWrap");
            pErImageGetDataTypeSize          = loadFunctionFromDLL(pDll, "erImageGetDataTypeSize");
            pErImageGetColorModelNumChannels = loadFunctionFromDLL(pDll, "erImageGetColorModelNumChannels");
            pErImageGetPixelDepth            = loadFunctionFromDLL(pDll, "erImageGetPixelDepth");
            pErImageCopy                     = loadFunctionFromDLL(pDll, "erImageCopy");*/
            pErImageRead                     = loadFunctionFromDLL(pDll, "erImageRead");
            pErImageWrite                    = loadFunctionFromDLL(pDll, "erImageWrite");
            pErImageFree                     = loadFunctionFromDLL(pDll, "erImageFree");
            //pErVersion                       = loadFunctionFromDLL(pDll, "erVersion");

            ///////////////////////
            // Setup delegates
            ///////////////////////
            fcnErImageAllocate                 = (fcn_erImageAllocate)                Marshal.GetDelegateForFunctionPointer(pErImageAllocate,                 typeof(fcn_erImageAllocate));
            /*fcnErImageAllocateBlank            = (fcn_erImageAllocateBlank)           Marshal.GetDelegateForFunctionPointer(pErImageAllocateBlank,            typeof(fcn_erImageAllocateBlank));
            fcnErImageAllocateAndWrap          = (fcn_erImageAllocateAndWrap)         Marshal.GetDelegateForFunctionPointer(pErImageAllocateAndWrap,          typeof(fcn_erImageAllocateAndWrap));
            fcnErImageGetDataTypeSize          = (fcn_erImageGetDataTypeSize)         Marshal.GetDelegateForFunctionPointer(pErImageGetDataTypeSize,          typeof(fcn_erImageGetDataTypeSize));
            fcnErImageGetColorModelNumChannels = (fcn_erImageGetColorModelNumChannels)Marshal.GetDelegateForFunctionPointer(pErImageGetColorModelNumChannels, typeof(fcn_erImageGetColorModelNumChannels));
            fcnErImageGetPixelDepth            = (fcn_erImageGetPixelDepth)           Marshal.GetDelegateForFunctionPointer(pErImageGetPixelDepth,            typeof(fcn_erImageGetPixelDepth));
            fcnErImageCopy                     = (fcn_erImageCopy)                    Marshal.GetDelegateForFunctionPointer(pErImageCopy,                     typeof(fcn_erImageCopy));*/
            fcnErImageRead                     = (fcn_erImageRead)                    Marshal.GetDelegateForFunctionPointer(pErImageRead,                     typeof(fcn_erImageRead));
            fcnErImageWrite                    = (fcn_erImageWrite)                   Marshal.GetDelegateForFunctionPointer(pErImageWrite,                    typeof(fcn_erImageWrite));
            fcnErImageFree                     = (fcn_erImageFree)                    Marshal.GetDelegateForFunctionPointer(pErImageFree,                     typeof(fcn_erImageFree));
            //fcnErVersion                       = (fcn_erVersion)                      Marshal.GetDelegateForFunctionPointer(pErVersion,                       typeof(fcn_erVersion));
        }

        /// <summary>
        /// ERImage DLL initialization.
        /// </summary>
        /// <param name="dllFilePath">Path to the ERImage DLL library.</param>
        public ERImageUtils(string dllFilePath) {
            // open dll
            pDll = NativeMethods.LoadLibrary(dllFilePath);
            if (pDll == IntPtr.Zero) {
                throw new ERException("Loading library " + dllFilePath + " failed!");
            }
            loadLibraryFunctions(pDll);
        }

        /// <summary>
        /// ERImage DLL initialization.
        /// </summary>
        /// <param name="dllFilePath">Path to the ERImage DLL library.</param>
        public ERImageUtils(IntPtr pDll) {
            loadLibraryFunctions(pDll);
        }

        ~ERImageUtils() {
            try {
                unsafe {
                    if (pDll != IntPtr.Zero) {
                        NativeMethods.FreeLibrary(pDll);
                    }
                }
            } catch {}
        }

        /// <summary>
        /// Creates the instance of the <seealso cref="ERImage"/> and fills it with the image data from <seealso cref="Bitmap"/>.
        /// </summary>
        /// <param name="bitmap">Input bitmap to convert.</param>
        /// <returns>Created <seealso cref="ERImage"/> with input image data.</returns>
        public ERImage csBitmapToERImage(Bitmap bitmap) {
            ERImage erImage = new ERImage();

            //// Select the correct color model
            ERImageColorModel color_model = ERImageColorModel.ER_IMAGE_COLORMODEL_UNK;
            // Grayscale color model - 1 byte per pixel
            if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed) {
                color_model = ERImageColorModel.ER_IMAGE_COLORMODEL_GRAY;
            // RGB color model - 3 bytes per pixel (1 byter per channel)
            } else if (bitmap.PixelFormat == PixelFormat.Format24bppRgb) {
                color_model = ERImageColorModel.ER_IMAGE_COLORMODEL_BGR;
            // ARGB color model - 4 bytes per pixel (1 byter per channel)
            } else if (bitmap.PixelFormat == PixelFormat.Format32bppArgb) {
                color_model = ERImageColorModel.ER_IMAGE_COLORMODEL_BGRA;
            // Other color models are converted to the RGB color model
            } else {
                Bitmap cloneImage = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                using (Graphics gr = Graphics.FromImage(cloneImage)) {
                    gr.DrawImage(bitmap, new Rectangle(0, 0, cloneImage.Width, cloneImage.Height));
                }
                bitmap = cloneImage;
                color_model = ERImageColorModel.ER_IMAGE_COLORMODEL_BGR;
            }

            //// Convert the C# Bitmap to the ERImage
            //       In case of Bitmap with Format24bppRgb binary image data
            //       are stored in BGR color space (same for Format32bppArgb
            //       and BGRA color space). Therefore in case of all
            //       color spaces of ERImage - GRAY, BGR and BGRA - simple data
            //       copying is enough and no conversion is necessary.
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            // Lock the binary data of the Bitmap.
            System.Drawing.Imaging.BitmapData bmpData =
                bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bitmap.PixelFormat);
            unsafe {
                // Pointer to the binary image data.
                byte* dataPtr = (byte*)bmpData.Scan0;
                // Allocate unmanaged ERImage. The ? also checks whether the fcnErImageAllocate is not null.
                Int32? allocationState = fcnErImageAllocate?.Invoke(&erImage, (UInt32)bitmap.Width, (UInt32)bitmap.Height, color_model, ERImageDataType.ER_IMAGE_DATATYPE_UCHAR);
                if (!allocationState.HasValue || allocationState != 0) {
                    throw new ERException("Image allocation failed.");
                }
                // Copy the image data from the Bitmap to the ERImage instance.
                for (int h = 0; h < bitmap.Height; h++) {
                    byte* dataRowPtr = dataPtr + h*bmpData.Stride;
                    byte* erImageRowData = (byte*)erImage.data + h*erImage.step;
                    for (uint i = 0; i < erImage.step; i++) {
                        erImageRowData[i] = dataRowPtr[i];
                    }
                }
            }
            // Unlock the binary data of the bitmap.
            bitmap.UnlockBits(bmpData);

            return erImage;
        }

        /// <summary>
        /// Converts the <see cref="ERImage"/> to the <see cref="Bitmap"/>. Image data is copied during the conversion.
        /// <para/>
        /// WARNING: Float images are not supported by <see cref="Bitmap"/>. 
        /// All <see cref="ERImage"/> structures with float image data are converted 
        /// to the <see cref="byte"/> data type ((<see cref="byte"/>)(erImage.data[i]*255)).
        /// </summary>
        /// <param name="image">Input image <see cref="ERImage"/> to convert.</param>
        /// <returns>Bitmap containing image data.</returns>
        /// <exception cref="ERException">When unsupported color model used (<see cref="ERImageColorModel.ER_IMAGE_COLORMODEL_YCBCR420"/>).</exception>
        public Bitmap erImageToCsBitmap(ERImage image) {
            Bitmap bitmap;
            int channels = 0;
            if        (image.color_model == ERImageColorModel.ER_IMAGE_COLORMODEL_GRAY) {
                channels = 1;
                bitmap = new Bitmap((int)image.width, (int)image.height, PixelFormat.Format8bppIndexed);
                // Set 8 bit indexed palette to grayscale
                for (int i = 0; i < 256; i++) {
                    bitmap.Palette.Entries[i] = Color.FromArgb((byte)i, (byte)i, (byte)i);
                }
            } else if (image.color_model == ERImageColorModel.ER_IMAGE_COLORMODEL_BGR) {
                channels = 3;
                bitmap = new Bitmap((int)image.width, (int)image.height, PixelFormat.Format24bppRgb);
            } else if (image.color_model == ERImageColorModel.ER_IMAGE_COLORMODEL_BGRA) {
                channels = 4;
                bitmap = new Bitmap((int)image.width, (int)image.height, PixelFormat.Format32bppArgb);
            } else {
                throw new ERException("Unsupported ERImage color model.");
            }

            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bitmap.PixelFormat);
            unsafe {
                // Pointer to the binary image data.
                byte* bitmapDataPtr = (byte*)bmpData.Scan0;
                byte* imageDataPtr  = (byte*)image.data;
                if        (image.data_type == ERImageDataType.ER_IMAGE_DATATYPE_UCHAR) {
                    for (int y = 0; y < image.height; y++) {
                        byte* bitmapLinePtr = bitmapDataPtr+(y*bmpData.Stride);
                        byte* imageLinePtr  = imageDataPtr+(y*image.step);
                        for (int x = 0; x < channels*image.width; x++) {
                            bitmapLinePtr[x] = imageLinePtr[x];
                        }
                    }
                } else if (image.data_type == ERImageDataType.ER_IMAGE_DATATYPE_FLOAT) {
                    for (int y = 0; y < image.height; y++) {
                        byte*  bitmapLinePtr = bitmapDataPtr+(y*bmpData.Stride);
                        float* imageLinePtr  = (float*)(imageDataPtr+(y*image.step));
                        for (int x = 0; x < channels*image.width; x++) {
                            bitmapLinePtr[x] = (byte)(imageLinePtr[x]*255);
                        }
                    }
                } else {
                    bitmap.UnlockBits(bmpData);
                    throw new ERException("Unsupported ERImage data type.");
                }
                bitmap.UnlockBits(bmpData);
            }

            return bitmap;
        }

        /// <summary>
        /// Reads the image <seealso cref="ERImage"/> from the the file.
        /// </summary>
        /// <param name="filename">Path to the file to read the image from.</param>
        /// <returns>Image <seealso cref="ERImage"/> read from the file.</returns>
        public ERImage erImageRead(string filename) {
            ERImage image = new ERImage();
            unsafe {
                // Reads unmanaged ERImage. The ? also checks whether the fcnErImageRead is not null.
                Int32? readState = fcnErImageRead?.Invoke(&image, filename);
                if (!readState.HasValue || readState != 0) {
                    throw new ERException("Image reading failed.");
                }
            }

            return image;
        }

        /// <summary>
        /// Writes the input <seealso cref="ERImage"/> to the file.
        /// </summary>
        /// <param name="image">Input image to write.</param>
        /// <param name="filename">Path to the file to write the image.</param>
        public void erImageWrite(ERImage image, string filename) {
            unsafe {
                // Writes unmanaged ERImage. The ? also checks whether the fcnErImageWrite is not null.
                Int32? writeState = fcnErImageWrite?.Invoke(&image, filename);
                if (!writeState.HasValue || writeState != 0) {
                    throw new ERException("Image writing failed.");
                }
            }
        }

        /// <summary>
        /// Frees the input <seealso cref="ERImage"/>.
        /// </summary>
        /// <param name="image">Input image to free.</param>
        public void erImageFree(ref ERImage image) {
            unsafe {
                fixed (ERImage* imagePtr = &image) {
                    fcnErImageFree?.Invoke(imagePtr);
                }
            }
        }
    }
}
