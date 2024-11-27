////////////////////////////////////////////////////////////////////////////////////////
//                                                                                    //
//    C# interface to Eyedentify SDK                                                  //
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
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Eyedea.er;
using System.IO;

/// <summary>
/// Namespace containing all the classes, functions, structures and enumerators of the Eyedentify SDK.
/// </summary>
namespace Eyedea.Eyedentify
{
    /// <summary>
    /// Descriptor is a buffer with condensated description of the input image.
    /// It is used to compare two images using <seealso cref="EdfCsSDK.edfCompareDescs(EdfDescriptor, EdfDescriptor)"/>
    /// or to classify an image using <seealso cref="EdfCsSDK.edfClassify(EdfDescriptor)"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct EdfDescriptor
    {
        /// <summary>
        /// Version of the model used to create the feature.</summary>
        private UInt32 version;
        /// <summary>
        /// Number of bytes in the data array.</summary>
        private UInt32 size;
        /// <summary>
        /// Pointer to array containing descriptor data.</summary>
        private IntPtr data;

        /// <summary>
        /// Gets the version of the model used to create the feature.
        /// </summary>
        /// <returns>Signed integer representing the version of the model.</returns>
        public uint getVersion() {
            return (uint)version;
        }

        /// <summary>
        /// Gets the number of bytes in the data array.
        /// </summary>
        /// <returns>Signed integer representing the number of bytes.</returns>
        public uint getSize() {
            return (uint)size;
        }

        /// <summary>
        /// Gets the array containing the descriptor data.
        /// </summary>
        /// <returns>Byte array containing the descriptor data.</returns>
        public byte[] getData() {
            unsafe {
                byte* data = (byte*)this.data;
                byte[] descData = new byte[size];
                for (uint i = 0; i < size; i++) {
                    descData[i] = data[i];
                }
                return descData;
            }
        }

        /// <summary>
        /// Sets the data of the descriptor. Data is copied to the unmanaged context.
        /// </summary>
        /// <param name="data">Data to copy to the descriptor.</param>
        public void setData(byte[] data) {
            if ((uint)data.Length > this.size) {
                throw new EdfException("Destination array is too small.");
            }
            Marshal.Copy(data, 0, this.data, data.Length);
        }
    };

    /// <summary>
    /// <seealso cref="EdfPoints"/> is a structure containing 2D points.
    /// It is used to specify the input 2D points in the <seealso cref="EdfCropParams"/> structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct EdfPoints
    {
        /// <summary>
        /// Number of valid points in the set.
        /// </summary>
        public Int32 length;
        /// <summary>
        /// Row coordinates of points.
        /// </summary>
        public double[] rows;
        /// <summary>
        /// Column coordinates of points.
        /// </summary>
        public double[] cols;

        /// <summary>
        /// Creates instance of the <seealso cref="EdfPoints"/> structure and allocates all underlying fields accoring to the input parameter.
        /// </summary>
        /// <param name="length">Length of the points array to allocate.</param>
        /// <returns>Instance of the <seealso cref="EdfPoints"/> with allocated all underlying fields.</returns>
        public static EdfPoints allocateEdfPoints(uint length) {
            EdfPoints edfPoints = new EdfPoints();
            edfPoints.length = (Int32)length;
            edfPoints.rows   = new double[length];
            edfPoints.cols   = new double[length];
            return edfPoints;
        }

        /// <summary>
        /// Creates instance of the <seealso cref="EdfPoints"/> structure and wraps it around arrays spacified as the input parameters.<para />
        /// If input arrays have non-equal length the structure is created but the internal field <seealso cref="EdfPoints.length"/> is set to the length of the smaller one of the input arrays.
        /// </summary>
        /// <param name="rows">Double array containing the y-coordinates of the input points.</param>
        /// <param name="cols">Double array containing the x-coordinates of the input points.</param>
        /// <returns>Instance of the <seealso cref="EdfPoints"/> wrapped around the input arrays.</returns>
        public static EdfPoints wrapEdfPoints(double[] rows, double[] cols) {
            EdfPoints edfPoints = new EdfPoints();
            edfPoints.length = 0;
            edfPoints.rows = new double[0];
            edfPoints.cols = new double[0];
            if (rows != null && cols != null) {
                edfPoints.length = Math.Min(rows.Length, cols.Length);
                edfPoints.rows = rows;
                edfPoints.cols = cols;
            }
            return edfPoints;
        }

        /// <summary>
        /// Creates instance of the <seealso cref="EdfPoints"/> structure and copies the input <seealso cref="PointF"/> array to the internal arrays.
        /// </summary>
        /// <param name="points">Array of PointF to be copied to the internal arrays.</param>
        /// <returns>Instance of the <seealso cref="EdfPoints"/> wrapped around the input <seealso cref="PointF"/> array.</returns>
        public static EdfPoints wrapEdfPoints(PointF[] points) {
            EdfPoints edfPoints = allocateEdfPoints((uint)points.Length);
            for (uint i = 0; i < edfPoints.length; i++) {
                edfPoints.rows[i] = points[i].Y;
                edfPoints.cols[i] = points[i].X;
            }
            return edfPoints;
        }
    };

    /// <summary>
    /// <seealso cref="EdfValues"/> is a structure containing real number values.
    /// It is used to specify the input parameters in the <seealso cref="EdfCropParams"/> structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct EdfValues
    {
        /// <summary>
        /// Number of valid values in the set.</summary>
        public Int32 length;
        /// <summary>
        /// Contained values.</summary>
        public double[] values;

        /// <summary>
        /// Creates instance of the <seealso cref="EdfValues"/> structure and allocates all underlying fields accoring to the input parameter.
        /// </summary>
        /// <param name="length">Length of the values array to allocate.</param>
        /// <returns>Instance of the <seealso cref="EdfValues"/> with allocated all underlying fields.</returns>
        public static EdfValues allocateEdfValues(uint length) {
            EdfValues edfValues = new EdfValues();
            edfValues.length = (Int32)length;
            edfValues.values = new double[length];
            return edfValues;
        }

        /// <summary>
        /// Creates instance of the <seealso cref="EdfValues"/> structure and wraps it around arrays spacified as the input parameters.
        /// </summary>
        /// <param name="values">Double array containing the input values.</param>
        /// <returns>Instance of the <seealso cref="EdfValues"/> wrapped around the input array.</returns>
        public static EdfValues wrapEdfValues(double[] values) {
            EdfValues edfValues = new EdfValues();
            edfValues.length = 0;
            edfValues.values = new double[0];
            if (values != null) {
                edfValues.length = values.Length;
                edfValues.values = values;
            }
            return edfValues;
        }
    };

    /// <summary>
    /// <seealso cref="EdfCropParams"/> is a structure containing <seealso cref="EdfPoints"/> and <seealso cref="EdfValues"/> structures.
    /// It is used to specify the input parameters for the <seealso cref="EdfCsSDK.edfCropImage(ERImage, EdfCropParams, EdfCropImageConfig*)"/> function.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct EdfCropParams
    {
        /// <summary>
        /// Contained <seealso cref="EdfPoints"/>.</summary>
        public EdfPoints points;
        /// <summary>
        /// Contained <seealso cref="EdfValues"/>.</summary>
        public EdfValues values;

        /// <summary>
        /// Creates instance of the <seealso cref="EdfCropParams"/> structure and allocates all underlying fields accoring to the input parameters.
        /// </summary>
        /// <param name="lengthPoints">Length of the points array to allocate.</param>
        /// <param name="lengthValues">Length of the values array to allocate.</param>
        /// <returns>Instance of the <seealso cref="EdfCropParams"/> with allocated all underlying fields.</returns>
        public static EdfCropParams allocateEdfCropParams(uint lengthPoints, uint lengthValues) {
            EdfCropParams cropParams = new EdfCropParams();
            cropParams.points = EdfPoints.allocateEdfPoints(lengthPoints);
            cropParams.values = EdfValues.allocateEdfValues(lengthValues);
            return cropParams;
        }

        /// <summary>
        /// Creates instance of the <seealso cref="EdfCropParams"/> structure and wraps it around arrays spacified as the input parameters.
        /// </summary>
        /// <param name="rows">Double array containing the y-coordinates of the input points.</param>
        /// <param name="cols">Double array containing the x-coordinates of the input points.</param>
        /// <param name="values">Double array containing the input values.</param>
        /// <returns>Instance of the <seealso cref="EdfCropParams"/> wrapped around the input arrays.</returns>
        public static EdfCropParams wrapEdfCropParams(double[] rows, double[] cols, double[] values) {
            EdfCropParams cropParams = new EdfCropParams();
            cropParams.points = EdfPoints.wrapEdfPoints(rows, cols);
            cropParams.values = EdfValues.wrapEdfValues(values);
            return cropParams;
        }

        /// <summary>
        /// Creates instance of the <seealso cref="EdfCropParams"/> structure and wraps it around arrays spacified as the input parameters.
        /// </summary>
        /// <param name="points">Array of PointF to be copied to the internal arrays.</param>
        /// <param name="values">Double array containing the input values.</param>
        /// <returns>Instance of the <seealso cref="EdfCropParams"/> wrapped around the input arrays.</returns>
        public static EdfCropParams wrapEdfCropParams(PointF[] points, double[] values) {
            EdfCropParams cropParams = new EdfCropParams();
            cropParams.points = EdfPoints.wrapEdfPoints(points);
            cropParams.values = EdfValues.wrapEdfValues(values);
            return cropParams;
        }
    };

    /// <summary>
    /// <seealso cref="EdfClassifyResultValue"/> represents one entry of the classification result. It contains the name of the value and value itself both represented by string.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct EdfClassifyResultValue
    {
        /// <summary>Name of the value entry.</summary>
        private string task_name;
        /// <summary>Length of the name.</summary>
        private UInt32 task_name_length;
        /// <summary>String value.</summary>
        private string class_name;
        /// <summary>Length of the string value.</summary>
        private UInt32 class_name_length;
        /// <summary>Class identificator.</summary>
        private Int32 class_id;
        /// <summary>Classification score.</summary>
        private float score;

        /// <summary>
        /// Gets the name of the classification task as a <seealso cref="string"/>.
        /// </summary>
        /// <returns>The <seealso cref="string"/> containing the name of the task.</returns>
        public string TaskName {
            get {
                return task_name.Substring(0, (int)task_name_length);
            }
        }

        /// <summary>
        /// Gets the name of the classification class as a <seealso cref="string"/>.
        /// </summary>
        /// <returns>The <seealso cref="string"/> containing the name of the class.</returns>
        public string ClassName {
            get {
                return class_name.Substring(0, (int)class_name_length);
            }
        }

        /// <summary>
        /// Gets the ID of the classification class as a <seealso cref="int"/>.
        /// </summary>
        /// <returns>The <seealso cref="int"/> representing the ID of the class.</returns>
        public int ClassID {
            get {
                return (int)class_id;
            }
        }

        /// <summary>
        /// Gets the classification score.
        /// </summary>
        /// <returns>The <seealso cref="float"/> containing the score of the classification.</returns>
        public float Score {
            get {
                return score;
            }
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("    ").Append(TaskName)
              .Append(": ").Append(ClassName)
              .Append(" (").Append(ClassID)
              .Append(", score: ").Append(score)
              .Append(")").Append("\n");
            return sb.ToString();
        }
    }

    /// <summary>
    /// <seealso cref="EdfClassifyResult"/> represents the classification result. It contains the number of entries and the entries itself represented by the <seealso cref="EdfClassifyResultValue"/> structure.
    /// </summary>
    public struct EdfClassifyResult
    {
        /// <summary>Number of values.</summary>
        private uint num_values;
        /// <summary>Array of result values.</summary>
        private EdfClassifyResultValue[] values;

        /// <summary>
        /// Creates the classify result instance.
        /// </summary>
        /// <param name="numValues">Number of contained values.</param>
        /// <param name="values">Array containing <see cref="EdfClassifyResultValue"/> values.</param>
        public EdfClassifyResult(uint numValues, EdfClassifyResultValue[] values) {
            this.num_values = numValues;
            this.values     = values;
        }

        /// <summary>
        /// Gets the number of contained classification result values.
        /// </summary>
        /// <returns>Number of contained classification result values.</returns>
        public uint NumValues {
            get {
                return num_values;
            }
        }

        /// <summary>
        /// Gets the classification result value on the specified index.
        /// </summary>
        /// <param name="index">Index of the value to get.</param>
        /// <returns>Classification result value on the specified index.</returns>
        public EdfClassifyResultValue getValue(int index) {
            if (index < 0 || index >= num_values) {
                throw new IndexOutOfRangeException();
            }

            return values[index];
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < num_values; i++) {
                EdfClassifyResultValue result = getValue(i);
                sb.Append(result.ToString());
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// <seealso cref="EdfInitConfig"/> represents the configuration parameters set used during SDK module initialization.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct EdfInitConfig
    {
        /// <summary><seealso cref="string"/> with path to the module.</summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string module_path;
        /// <summary><seealso cref="string"/> with model filename.</summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string model_file;
        /// <summary>Selected computation mode.</summary>
        public ERComputationMode computation_mode;
        /// <summary>GPU device identifier (used only when <seealso cref="EdfInitConfig.computation_mode"/> == <seealso cref="ERComputationMode.ER_COMPUTATION_MODE_GPU"/>).</summary>
        public Int32 gpu_device_id;
        /// <summary>Number of threads to run the ER_COMPUTATION_MODE_CPU computation, value <=0 sets std::thread::hardware_concurency.</summary>
        public Int32 num_threads;
    }

    /// <summary>
    /// <seealso cref="EdfComputeDescConfig"/> represents the configuration parameters used during descriptor computation.<para/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct EdfComputeDescConfig
    {
        /// <summary>Size of the input data batch. 
        /// Set 0 to disable batch processing. 
        /// Set 1-N to set the size of the batch (value 1 has the same effect as 0).</summary>
        public UInt32 batch_size;

        public EdfComputeDescConfig(uint batchSize) {
            this.batch_size = batchSize;
        }

        public static EdfComputeDescConfig getDefault() {
            return new EdfComputeDescConfig(0);
        }
    }

    /// <summary>
    /// <seealso cref="EdfCropImageConfig"/> represents the configuration parameters used during input image cropping.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct EdfCropImageConfig
    {
        public Int32  full_crop;
        public Int32  color_normalization;
        public Int32  use_antialiasing;
        public UInt32 antialiasing_kernel_size;
        public float  antialiasing_sigma;

        public EdfCropImageConfig(bool fullCrop, bool colorNormalization) {
            this.full_crop                = fullCrop           ? 1 : -1;
            this.color_normalization      = colorNormalization ? 1 : -1;
            this.use_antialiasing         =    0; // Default value (0)
            this.antialiasing_kernel_size =    0;
            this.antialiasing_sigma       = 0.0f;
        }

        public EdfCropImageConfig(bool useAntialiasing, uint antialiasingKernelSize = 0, float antialiasingSigma = 0.0f) {
            this.full_crop                =    0; // Default value (0)
            this.color_normalization      =    0; // Default value (0)
            this.use_antialiasing         = useAntialiasing    ? 1 : -1;
            this.antialiasing_kernel_size = antialiasingKernelSize;
            this.antialiasing_sigma       = antialiasingSigma;
        }

        public EdfCropImageConfig(bool fullCrop, bool colorNormalization, 
                                  bool useAntialiasing, uint antialiasingKernelSize = 0, float antialiasingSigma = 0.0f) {
            this.full_crop                = fullCrop           ? 1 : -1;
            this.color_normalization      = colorNormalization ? 1 : -1;
            this.use_antialiasing         = useAntialiasing    ? 1 : -1;
            this.antialiasing_kernel_size = antialiasingKernelSize;
            this.antialiasing_sigma       = antialiasingSigma;
        }

        public static EdfCropImageConfig getDefault() {
            return new EdfCropImageConfig(false, true);
        }
    }

    /// <summary>
    /// <seealso cref="EdfClassifyConfig"/> represents the configuration parameters used during descriptor classification.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct EdfClassifyConfig
    {
        /// <summary>
        /// Set to  0 to get classification results      with                                  dependency rules applied. DEFAULT<para></para>
        /// Set to  1 to get classification results both with and without (_NODEP name suffix) dependency rules applied.<para></para>
        /// Set to -1 to get classification results               without (_NODEP name suffix) dependency rules applied.
        /// </summary>
        public Int32 use_dependency_rules;

        /// <summary>
        /// Creates instance of <see cref="EdfClassifyConfig"/> using Int32 value.
        /// </summary>
        /// <param name="useDependencyRules">
        /// Set to  0 to get classification results      with                                  dependency rules applied. DEFAULT<para></para>
        /// Set to  1 to get classification results both with and without (_NODEP name suffix) dependency rules applied.<para></para>
        /// Set to -1 to get classification results               without (_NODEP name suffix) dependency rules applied.
        /// </param>
        public EdfClassifyConfig(int useDependencyRules = 0) {
            this.use_dependency_rules = useDependencyRules;
        }

        /// <summary>
        /// Creates instance of <see cref="EdfClassifyConfig"/> using bool value.
        /// </summary>
        /// <param name="useDependencyRules">
        /// Set to true  to get classification results with                         dependency rules applied. DEFAULT<para></para>
        /// Set to false to get classification results without (_NODEP name suffix) dependency rules applied.
        /// </param>
        public EdfClassifyConfig(bool useDependencyRules = true) {
            this.use_dependency_rules = 0;
            if (!useDependencyRules) {
                this.use_dependency_rules = -1;
            }
        }

        public static EdfClassifyConfig getDefault() {
            return new EdfClassifyConfig(0);
        }
    }

    /// <summary>
    /// General Eyedentify SDK exception.
    /// </summary>
    [Serializable]
    public class EdfException : ApplicationException
    {
        private static string strEXHeader = "Eyedentify Exception: ";

        public EdfException() { }
        public EdfException(string message)
            : base(strEXHeader + message) { }
        public EdfException(string message, System.Exception inner)
            : base(strEXHeader + message, inner) { }

        protected EdfException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    };

    /// <summary>
    /// Exception used when a Eyedentify SDK function called without proper initialization.
    /// </summary>
    [Serializable]
    public class EdfUninitializedModule : EdfException
    {
        private static string strEXHeader = "Eyedentify is not initialized.";

        public EdfUninitializedModule()
            : base(strEXHeader) { }
        public EdfUninitializedModule(string message)
            : base(strEXHeader + "\n" + message) { }
        public EdfUninitializedModule(string message, System.Exception inner)
            : base(strEXHeader + "\n" + message, inner) { }

        protected EdfUninitializedModule(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    };

    /// <summary>
    /// Class representing the Eyedentify SDK module instance and containing all needed methods.
    /// </summary>
    public class EdfCsSDK {
        /// <summary>
        /// Native methods for explicit linking.
        /// </summary>
        static class NativeMethods {
            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string dllToLoad);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

            [DllImport("kernel32.dll")]
            public static extern bool FreeLibrary(IntPtr hModule);
        }

        /// <summary>
        /// Structure used for passing the EdfInitConfig to the unmanaged code.
        /// </summary>
        unsafe private struct EdfInitConfigUnmanaged
        {
            /// <summary><seealso cref="char"/> array with path to the module.</summary>
            public char* module_path;
            /// <summary><seealso cref="char"/> array with model filename.</summary>
            public char* model_file;
            /// <summary>Selected computation mode.</summary>
            public ERComputationMode computation_mode;
            /// <summary>GPU device identifier (used only when <seealso cref="EdfInitConfig.computation_mode"/> == <seealso cref="ERComputationMode.ER_COMPUTATION_MODE_GPU"/>).</summary>
            public Int32 gpu_device_id;
            /// <summary>Number of threads to run the ER_COMPUTATION_MODE_CPU computation, value <=0 sets std::thread::hardware_concurency.</summary>
            public Int32 num_threads;
        }

        /// <summary>
        /// Structure used for passing the EdfCropParams to the unmanaged code.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        unsafe private struct EdfCropParamsUnmanaged
        {
            /// <summary>Number of valid points in the set.</summary>
            public Int32 lengthPoints;
            /// <summary>Row coordinates of points.</summary>
            public double* rows;
            /// <summary>Column coordinates of points.</summary>
            public double* cols;
            /// <summary>Number of valid values in the set.</summary>
            public Int32 lengthValues;
            /// <summary>Contained values.</summary>
            public double* values;
        }

        /// <summary>
        /// <seealso cref="EdfClassifyResultUM"/> represents the unmanaged classification result structure <seealso cref="EdfClassifyResult"/>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct EdfClassifyResultUM
        {
            /// <summary>Number of values.</summary>
            public UInt32 num_values;
            /// <summary>Array of result values.</summary>
            private IntPtr values;

            /// <summary>
            /// Returns the array containing EdfClassifyResultValue elements copied from the C/C++ environment.
            /// </summary>
            /// <returns>Array containing EdfClassifyResultValue elements copied from the C/C++ environment.</returns>
            public EdfClassifyResultValue[] getValues() {
                EdfClassifyResultValue[] array = new EdfClassifyResultValue[num_values];
                for (int i = 0; i < num_values; i++) {
                    IntPtr ptr = values + i * Marshal.SizeOf(typeof(EdfClassifyResultValue));
                    array[i] = (EdfClassifyResultValue)Marshal.PtrToStructure(ptr, typeof(EdfClassifyResultValue));
                }

                return array;
            }

            /// <summary>
            /// Returns <see cref="EdfClassifyResult"/> with copied results.
            /// </summary>
            /// <returns><see cref="EdfClassifyResult"/> with copied results.</returns>
            public EdfClassifyResult toEdfClassifyResult() {
                return new EdfClassifyResult(num_values, getValues());
            }
        }

        ///////
        // Declare function types
        ///////
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate int fcn_edfInitEyedentify(EdfInitConfigUnmanaged *init_config, void** module_state);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate void fcn_edfFreeEyedentify(void** module_state);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate int fcn_edfComputeDesc(ERImage* img, void* module_state, EdfDescriptor* descriptor, EdfComputeDescConfig* config);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate int fcn_edfCompareDescs(EdfDescriptor* desc_A, EdfDescriptor* desc_B, void* module_state, float* score);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate int fcn_edfCropImage(ERImage* image_in, EdfCropParamsUnmanaged* cropParams, void* module_state, ERImage* cropped_image, EdfCropImageConfig* config);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate void fcn_edfFreeCropImage(void* module_state, ERImage* cropped_image);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate UInt32 fcn_edfModelVersion(void* module_state);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate int fcn_edfClassify(EdfDescriptor* desc, void* module_state, EdfClassifyResultUM** classify_result, EdfClassifyConfig* config);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate void fcn_edfAllocDesc(EdfDescriptor* desc, UInt32 size, UInt32 version);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate void fcn_edfFreeDesc(EdfDescriptor* desc);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe private delegate int fcn_edfFreeClassifyResult(EdfClassifyResultUM** classify_result, void* module_state);

        ////////
        // Define dll and function pointers
        ////////
        IntPtr pDll = IntPtr.Zero;

        IntPtr pEdfInitEyedentify           = IntPtr.Zero;
        IntPtr pEdfFreeEyedentify           = IntPtr.Zero;
        IntPtr pEdfComputeDesc              = IntPtr.Zero;
        IntPtr pEdfCompareDescs             = IntPtr.Zero;
        IntPtr pEdfCropImage                = IntPtr.Zero;
        IntPtr pEdfFreeCropImage            = IntPtr.Zero;
        IntPtr pEdfModelVersion             = IntPtr.Zero;
        IntPtr pEdfClassify                 = IntPtr.Zero;
        IntPtr pEdfAllocDesc                = IntPtr.Zero;
        IntPtr pEdfFreeDesc                 = IntPtr.Zero;
        IntPtr pEdfFreeClassifyResult       = IntPtr.Zero;

        ///////////////
        // Define delegates of functions
        ///////////////
        fcn_edfInitEyedentify     fcnEdfInitEyedentify;
        fcn_edfFreeEyedentify     fcnEdfFreeEyedentify;
        fcn_edfComputeDesc        fcnEdfComputeDesc;
        fcn_edfCompareDescs       fcnEdfCompareDescs;
        fcn_edfCropImage          fcnEdfCropImage;
        fcn_edfFreeCropImage      fcnEdfFreeCropImage;
        fcn_edfModelVersion       fcnEdfModelVersion;
        fcn_edfClassify           fcnEdfClassify;
        fcn_edfAllocDesc          fcnEdfAllocDesc;
        fcn_edfFreeDesc           fcnEdfFreeDesc;
        fcn_edfFreeClassifyResult fcnEdfFreeClassifyResult;

        /// <summary>
        /// Pointer to the module instance.
        /// </summary>
        unsafe private void* pvModuleState = null;

        /// <summary>
        /// Allows image manipulation in the <see cref="ERImage"/> structure.
        /// </summary>
        ERImageUtils erImageUtils = null;

        /// <summary>
        /// Eyedentify SDK dll initialization.
        /// </summary>
        /// <param name="dllDirPath">Path to the Eyedentify DLL library directory.</param>
        public EdfCsSDK(string dllDirPath) {
            string dllFilename = "eyedentify-Win32.dll";
            if (Environment.Is64BitProcess) {
                dllFilename = "eyedentify-x64.dll";
            }
            dllDirPath.Replace('\\', '/');

            // Add Eyedentify SDK lib path to the PATH variable
            // to met dependencies loading during explicit linking.
            string pathVariable = Environment.GetEnvironmentVariable("PATH");
            if (!pathVariable.Contains(dllDirPath)) {
                Environment.SetEnvironmentVariable("PATH", dllDirPath + ";" + pathVariable);
            }

            string dllPath = Path.Combine(dllDirPath, dllFilename);
            // open dll
            pDll = NativeMethods.LoadLibrary(@dllPath);
            if (pDll == IntPtr.Zero) {
                throw new EdfException("Loading library " + dllPath + " failed!");
            }

            //////////////////////////
            // load functions from dll
            //////////////////////////
            pEdfInitEyedentify = NativeMethods.GetProcAddress(pDll, "edfInitEyedentify");
            if (pEdfInitEyedentify == IntPtr.Zero) {
                throw new EdfException("edfInitEyedentify NULL");
            }

            pEdfFreeEyedentify = NativeMethods.GetProcAddress(pDll, "edfFreeEyedentify");
            if (pEdfFreeEyedentify == IntPtr.Zero) {
                throw new EdfException("edfFreeEyedentify NULL");
            }

            pEdfComputeDesc = NativeMethods.GetProcAddress(pDll, "edfComputeDesc");
            if (pEdfComputeDesc == IntPtr.Zero) {
                throw new EdfException("edfComputeDesc NULL");
            }

            pEdfCompareDescs = NativeMethods.GetProcAddress(pDll, "edfCompareDescs");
            if (pEdfCompareDescs == IntPtr.Zero) {
                throw new EdfException("edfCompareDescs NULL");
            }

            pEdfCropImage = NativeMethods.GetProcAddress(pDll, "edfCropImage");
            if (pEdfCropImage == IntPtr.Zero) {
                throw new EdfException("edfCropImage NULL");
            }

            pEdfFreeCropImage = NativeMethods.GetProcAddress(pDll, "edfFreeCropImage");
            if (pEdfFreeCropImage == IntPtr.Zero) {
                throw new EdfException("edfFreeCropImage NULL");
            }

            pEdfModelVersion = NativeMethods.GetProcAddress(pDll, "edfModelVersion");
            if (pEdfModelVersion == IntPtr.Zero) {
                throw new EdfException("edfModelVersion NULL");
            }

            pEdfClassify = NativeMethods.GetProcAddress(pDll, "edfClassify");
            if (pEdfClassify == IntPtr.Zero) {
                throw new EdfException("edfClassify NULL");
            }

            pEdfAllocDesc = NativeMethods.GetProcAddress(pDll, "edfAllocDesc");
            if (pEdfAllocDesc == IntPtr.Zero) {
                throw new EdfException("edfAllocDesc NULL");
            }

            pEdfFreeDesc = NativeMethods.GetProcAddress(pDll, "edfFreeDesc");
            if (pEdfFreeDesc == IntPtr.Zero) {
                throw new EdfException("edfFreeDesc NULL");
            }

            pEdfFreeClassifyResult = NativeMethods.GetProcAddress(pDll, "edfFreeClassifyResult");
            if (pEdfFreeClassifyResult == IntPtr.Zero) {
                throw new EdfException("edfFreeClassifyResult NULL");
            }

            ///////////////////////
            // Setup delegates
            ///////////////////////
            fcnEdfInitEyedentify     = (fcn_edfInitEyedentify)Marshal.GetDelegateForFunctionPointer(pEdfInitEyedentify, typeof(fcn_edfInitEyedentify));
            fcnEdfFreeEyedentify     = (fcn_edfFreeEyedentify)Marshal.GetDelegateForFunctionPointer(pEdfFreeEyedentify, typeof(fcn_edfFreeEyedentify));
            fcnEdfComputeDesc        = (fcn_edfComputeDesc)Marshal.GetDelegateForFunctionPointer(pEdfComputeDesc, typeof(fcn_edfComputeDesc));
            fcnEdfCompareDescs       = (fcn_edfCompareDescs)Marshal.GetDelegateForFunctionPointer(pEdfCompareDescs, typeof(fcn_edfCompareDescs));
            fcnEdfCropImage          = (fcn_edfCropImage)Marshal.GetDelegateForFunctionPointer(pEdfCropImage, typeof(fcn_edfCropImage));
            fcnEdfFreeCropImage      = (fcn_edfFreeCropImage)Marshal.GetDelegateForFunctionPointer(pEdfFreeCropImage, typeof(fcn_edfFreeCropImage));
            fcnEdfModelVersion       = (fcn_edfModelVersion)Marshal.GetDelegateForFunctionPointer(pEdfModelVersion, typeof(fcn_edfModelVersion));
            fcnEdfClassify           = (fcn_edfClassify)Marshal.GetDelegateForFunctionPointer(pEdfClassify, typeof(fcn_edfClassify));
            fcnEdfAllocDesc          = (fcn_edfAllocDesc)Marshal.GetDelegateForFunctionPointer(pEdfAllocDesc, typeof(fcn_edfAllocDesc));
            fcnEdfFreeDesc           = (fcn_edfFreeDesc)Marshal.GetDelegateForFunctionPointer(pEdfFreeDesc, typeof(fcn_edfFreeDesc));
            fcnEdfFreeClassifyResult = (fcn_edfFreeClassifyResult)Marshal.GetDelegateForFunctionPointer(pEdfFreeClassifyResult, typeof(fcn_edfFreeClassifyResult));

            erImageUtils = new ERImageUtils(pDll);
        }

        ~EdfCsSDK() {
            try {
                unsafe {
                    if (pDll != IntPtr.Zero) {
                        if (pvModuleState != null) {
                            fixed (void** ppvModuleState = &pvModuleState) {
                                fcnEdfFreeEyedentify(ppvModuleState);
                            }
                        }
                        NativeMethods.FreeLibrary(pDll);
                    }
                }
            } catch {}
        }

        /// <summary>
        /// Checks whether the module is loaded.
        /// </summary>
        /// <returns>Loading state of the module.</returns>
        public bool IsLoaded() {
            unsafe {
                return pvModuleState == null ? false : true;
            }
        }

        private void checkModuleInitialized(bool checkSDKInit = true) {
            unsafe {
                if ((pvModuleState == null && checkSDKInit) ||
                    pDll == IntPtr.Zero ||
                    erImageUtils == null) {
                    throw new EdfUninitializedModule();
                }
            }
        }

        /// <summary>
        /// Creates the instance of the <seealso cref="ERImage"/> and fills it with the image data from <seealso cref="Bitmap"/>.
        /// </summary>
        /// <param name="bitmap">Input bitmap to convert.</param>
        /// <returns>Created <seealso cref="ERImage"/> with input image data.</returns>
        public ERImage csBitmapToERImage(Bitmap bitmap) {
            checkModuleInitialized(false);
            return erImageUtils.csBitmapToERImage(bitmap);
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
            checkModuleInitialized(false);
            return erImageUtils.erImageToCsBitmap(image);
        }

        /// <summary>
        /// Reads the image <seealso cref="ERImage"/> from the the file.
        /// </summary>
        /// <param name="filename">Path to the file to read the image from.</param>
        /// <returns>Image <seealso cref="ERImage"/> read from the file.</returns>
        public ERImage erImageRead(string filename) {
            checkModuleInitialized(false);
            return erImageUtils.erImageRead(filename);
        }

        /// <summary>
        /// Writes the input <seealso cref="ERImage"/> to the file.
        /// </summary>
        /// <param name="image">Input image to write.</param>
        /// <param name="filename">Path to the file to write the image.</param>
        public void erImageWrite(ERImage image, string filename) {
            checkModuleInitialized(false);
            erImageUtils.erImageWrite(image, filename);
        }

        /// <summary>
        /// Frees the input <seealso cref="ERImage"/>.
        /// </summary>
        /// <param name="image">Input image to free.</param>
        public void erImageFree(ref ERImage image) {
            checkModuleInitialized(false);
            erImageUtils.erImageFree(ref image);
        }

        /// <summary>
        /// Initializes the Eyedentify SDK recognition module.
        /// </summary>
        /// <param name="initConfig">Structure <seealso cref="EdfInitConfig"/> containing initialization parameters.</param>
        /// <returns>True if initialized correctly, false otherwise.</returns>
        public bool edfInitEyedentify(EdfInitConfig initConfig) {
            unsafe {
                if (pDll == IntPtr.Zero) {
                    return false;
                }

                if (pvModuleState != null) {
                    fixed (void** ppvModuleState = &pvModuleState) {
                        fcnEdfFreeEyedentify(ppvModuleState);
                    }
                }

                // Add Eyedentify SDK module lib path to the PATH variable
                // to met dependencies loading during explicit linking.
                string moduleLibPath = Path.Combine(initConfig.module_path, "lib/");
                string pathVariable = Environment.GetEnvironmentVariable("PATH");
                if (!pathVariable.Contains(moduleLibPath)) {
                    Environment.SetEnvironmentVariable("PATH", moduleLibPath + ";" + pathVariable);
                }

                EdfInitConfigUnmanaged initConfigUnmanaged = new EdfInitConfigUnmanaged();
                IntPtr pModule_path = Marshal.StringToHGlobalAnsi(initConfig.module_path);
                IntPtr pModel_file = Marshal.StringToHGlobalAnsi(initConfig.model_file);
                initConfigUnmanaged.module_path      = (char*)pModule_path;
                initConfigUnmanaged.model_file       = (char*)pModel_file;
                initConfigUnmanaged.computation_mode = initConfig.computation_mode;
                initConfigUnmanaged.gpu_device_id    = initConfig.gpu_device_id;


                fixed (void** ppvModuleState = &pvModuleState) {
                    if (fcnEdfInitEyedentify(&initConfigUnmanaged, ppvModuleState) != 0) {
                        Marshal.FreeHGlobal(pModule_path);
                        Marshal.FreeHGlobal(pModel_file);
                        throw new EdfException("Initialization failed.");
                    }
                }
                Marshal.FreeHGlobal(pModule_path);
                Marshal.FreeHGlobal(pModel_file);
            }
            return true;
        }

        /// <summary>
        /// Frees the Eyedentify SDK recognition module.
        /// </summary>
        public void edfFreeEyedentify() {
            unsafe {
                checkModuleInitialized();
                try {
                    fixed (void** ppvModuleState = &pvModuleState) {
                        fcnEdfFreeEyedentify(ppvModuleState);
                    }
                } catch (Exception e) {
                    throw new EdfException("fcnEdfFreeEyedentify exception. " + e.Message);
                }
            }
        }

        /// <summary>
        /// Computed the descriptor from the input image crop.
        /// </summary>
        /// <param name="img">Input image crop created using <seealso cref="edfCropImage(ERImage, EdfCropParams, EdfCropImageConfig*)"/> function.</param>
        /// <param name="config">Descriptor computation configuration.</param>
        /// <returns>Descriptor of the input image.</returns>
        public EdfDescriptor edfComputeDesc(ERImage img, EdfComputeDescConfig config) {
            unsafe {
                checkModuleInitialized();
                EdfDescriptor descriptor = new EdfDescriptor();
                int descriptorStatus = fcnEdfComputeDesc(&img, pvModuleState, &descriptor, &config);
                if (descriptorStatus != 0) {
                    throw new EdfException("Error during descriptor computation.");
                }
                return descriptor;
            }
        }

        /// <summary>
        /// Computed the descriptor from the input image crop.
        /// </summary>
        /// <param name="img">Input image crop created using <seealso cref="edfCropImage(ERImage, EdfCropParams, EdfCropImageConfig*)"/> function.</param>
        /// <returns>Descriptor of the input image.</returns>
        public EdfDescriptor edfComputeDesc(ERImage img) {
            return edfComputeDesc(img, EdfComputeDescConfig.getDefault());
        }

        private IntPtr AllocateUnmanagedArray(Type type, int length) {
            IntPtr arrayPtr = Marshal.AllocHGlobal(Marshal.SizeOf(type)*length);
            return arrayPtr;
        }

        private IntPtr GetPtrOfMarshaledArray<T>(T[] array) {
            IntPtr arrayPtr = AllocateUnmanagedArray(typeof(T), array.Length);
            IntPtr arrayPtrPos = arrayPtr;
            for (int i = 0; i < array.Length; i++) {
                T element = array[i];
                Marshal.StructureToPtr(element, arrayPtrPos, false);
                arrayPtrPos = IntPtr.Add(arrayPtrPos, Marshal.SizeOf(element));
            }
            return arrayPtr;
        }

        private void GetManagedArrayFromUnmanaged<T>(IntPtr arrayPtr, int length, T[] array) {
            IntPtr arrayPtrPos = arrayPtr;
            for (int i = 0; i < length; i++) {
                array[i] = (T)Marshal.PtrToStructure(arrayPtrPos, typeof(T));
                arrayPtrPos = IntPtr.Add(arrayPtrPos, Marshal.SizeOf(array[i]));
            }
        }

        /// <summary>
        /// Computed the descriptors from the input image crops array.
        /// </summary>
        /// <param name="img">Input image crops array. Each crop created using <seealso cref="edfCropImage(ERImage, EdfCropParams, EdfCropImageConfig*)"/> function.</param>
        /// <returns>Array od descriptors of the input images.</returns>
        public EdfDescriptor[] edfComputeDesc(ERImage[] imgs) {
            unsafe {
                checkModuleInitialized();
                EdfComputeDescConfig config = new EdfComputeDescConfig((uint)imgs.Length);
                IntPtr imgsIntPtr = GetPtrOfMarshaledArray(imgs);
                IntPtr descriptorsIntPtr = AllocateUnmanagedArray(typeof(EdfDescriptor), imgs.Length);
                ERImage* imgsPtr = (ERImage*)imgsIntPtr.ToPointer();
                EdfDescriptor* descriptorsPtr = (EdfDescriptor*)descriptorsIntPtr.ToPointer();
                int descriptorStatus = fcnEdfComputeDesc(imgsPtr, pvModuleState, descriptorsPtr, &config);
                Marshal.FreeHGlobal(imgsIntPtr);
                if (descriptorStatus != 0) {
                    Marshal.FreeHGlobal(descriptorsIntPtr);
                    throw new EdfException("Error during descriptor computation.");
                }
                EdfDescriptor[] descriptors = new EdfDescriptor[imgs.Length];
                for (int i = 0; i < imgs.Length; i++) {
                    descriptors[i] = new EdfDescriptor();
                }
                GetManagedArrayFromUnmanaged<EdfDescriptor>(descriptorsIntPtr, imgs.Length, descriptors);
                Marshal.FreeHGlobal(descriptorsIntPtr);
                return descriptors;
            }
        }

        /// <summary>
        /// Compares two input descriptors and returns similarity score.
        /// </summary>
        /// <param name="descA">First descriptor to compare.</param>
        /// <param name="descB">Second descriptor to compare.</param>
        /// <returns>Similarity score.</returns>
        public float edfCompareDescs(EdfDescriptor descA, EdfDescriptor descB) {
            unsafe {
                checkModuleInitialized();
                float score = float.MinValue;
                int compareStatus = fcnEdfCompareDescs(&descA, &descB, pvModuleState, &score);
                if (compareStatus != 0) {
                    throw new EdfException("Error during descriptor matching.");
                }
                return score;
            }
        }

        /// <summary>
        /// Allocates <seealso cref="EdfDescriptor"/> and all underlying fields with respect to the input parameters.
        /// </summary>
        /// <param name="size">Size of the descriptor data in bytes to allocate.</param>
        /// <param name="version">Version of the descriptor.</param>
        /// <returns></returns>
        public EdfDescriptor edfAllocDesc(UInt32 size, UInt32 version) {
            unsafe {
                checkModuleInitialized(false);
                EdfDescriptor descriptor = new EdfDescriptor();
                fcnEdfAllocDesc(&descriptor, size, version);
                return descriptor;
            }
        }

        /// <summary>
        /// Initializes the <seealso cref="EdfDescriptor"/> instance from the byte array containing the descriptor data.
        /// </summary>
        /// <param name="data">Descriptor data to wrap.</param>
        /// <param name="version">Version of the descriptor.</param>
        /// <returns></returns>
        public EdfDescriptor edfWrapDesc(byte[] data, UInt32 version) {
            unsafe {
                checkModuleInitialized(false);
                EdfDescriptor descriptor = this.edfAllocDesc((uint)data.Length, version);
                descriptor.setData(data);
                return descriptor;
            }
        }

        /// <summary>
        /// Frees the descriptor previously allocated using function <seealso cref="edfAllocDesc(uint, uint)"/> or <seealso cref="edfComputeDesc(ERImage*, EdfComputeDescConfig*)"/>
        /// </summary>
        /// <param name="descriptor">The descriptor fo free.</param>
        public void edfFreeDesc(ref EdfDescriptor descriptor) {
            unsafe {
                checkModuleInitialized(false);
                fixed (EdfDescriptor* descriptorPtr = &descriptor) {
                    fcnEdfFreeDesc(descriptorPtr);
                }
            }
        }

        /// <summary>
        /// Crops the input image with respect to the input cropping parameters specified in <seealso cref="EdfCropParams"/>.
        /// </summary>
        /// <param name="imageIn">Input image to crop.</param>
        /// <param name="cropParams">Input cropping parameters.</param>
        /// <param name="config">Optional mage cropping configuration.</param>
        /// <returns>Pointer to the cropped input image.</returns>
        public ERImage edfCropImage(ERImage imageIn, EdfCropParams cropParams, EdfCropImageConfig config) {
            unsafe {
                checkModuleInitialized();

                ERImage cropped_image = new ERImage();
                // Create instance of the unmanaged structure
                // which will be passed to the native function.
                EdfCropParamsUnmanaged cropParamsUM = new EdfCropParamsUnmanaged();
                cropParamsUM.lengthPoints = cropParams.points.length;
                cropParamsUM.lengthValues = cropParams.values.length;
                cropParamsUM.rows         = null;
                cropParamsUM.cols         = null;
                cropParamsUM.values       = null;

                // Fix the pointers to the three arrays which will be
                // passed in the structure to the native function.
                List<GCHandle> handlesList = new List<GCHandle>();
                if (cropParams.points.rows.Length  > 0 &&
                    cropParams.points.cols.Length  > 0) {
                    GCHandle rowsHandle = GCHandle.Alloc(cropParams.points.rows, GCHandleType.Pinned);
                    handlesList.Add(rowsHandle);
                    cropParamsUM.rows = (double*)rowsHandle.AddrOfPinnedObject();
                    GCHandle colsHandle = GCHandle.Alloc(cropParams.points.cols, GCHandleType.Pinned);
                    handlesList.Add(colsHandle);
                    cropParamsUM.cols = (double*)colsHandle.AddrOfPinnedObject();
                }
                if (cropParams.values.values.Length > 0) {
                    GCHandle valsHandle = GCHandle.Alloc(cropParams.values.values, GCHandleType.Pinned);
                    handlesList.Add(valsHandle);
                    cropParamsUM.values = (double*)valsHandle.AddrOfPinnedObject();
                }
                int cropStatus = fcnEdfCropImage(&imageIn, &cropParamsUM, pvModuleState, &cropped_image, &config);
                for (int i = 0; i < handlesList.Count; i++) {
                    handlesList[i].Free();
                }
                if (cropStatus != 0) {
                    throw new EdfException("Error during image cropping.");
                }
            
                return cropped_image;
            }
        }

        /// <summary>
        /// Crops the input image with respect to the input cropping parameters specified in <seealso cref="EdfCropParams"/>.
        /// </summary>
        /// <param name="imageIn">Input image to crop.</param>
        /// <param name="cropParams">Input cropping parameters.</param>
        /// <returns>Pointer to the cropped input image.</returns>
        public ERImage edfCropImage(ERImage imageIn, EdfCropParams cropParams) {
            return edfCropImage(imageIn, cropParams, EdfCropImageConfig.getDefault());
        }

        /// <summary>
        /// Free image cropped using <see cref="edfCropImage(ERImage, EdfCropParams, EdfCropImageConfig)"/>.
        /// </summary>
        /// <param name="croppedImage">EdfImage structure instance created by <see cref="edfCropImage(ERImage, EdfCropParams, EdfCropImageConfig)"/></param>
        public void edfFreeCropImage(ref ERImage croppedImage) {
            unsafe {
                checkModuleInitialized(false);
                fixed (ERImage* croppedImagePtr = &croppedImage) {
                    fcnEdfFreeCropImage(pvModuleState, croppedImagePtr);
                }
            }
        }

        /// <summary>
        /// Returns the version of the loaded binary model.
        /// </summary>
        /// <returns>Version of the loaded binary model.</returns>
        public UInt32 edfModelVersion() {
            unsafe {
                checkModuleInitialized();
                return fcnEdfModelVersion(pvModuleState);
            }
        }

        /// <summary>
        /// Classifies the <seealso cref="EdfDescriptor"/> with <seealso cref="EdfClassifyConfig"/> configuration structure 
        /// and outputs the result in the <seealso cref="EdfClassifyResult"/>.
        /// </summary>
        /// <param name="desc">Input <seealso cref="EdfDescriptor"/> to classify.</param>
        /// <param name="config"><seealso cref="EdfClassifyConfig"/> structure which configures the descriptor classification process.</param>
        /// <returns>Classification result values.</returns>
        public EdfClassifyResult edfClassify(EdfDescriptor desc, EdfClassifyConfig config) {
            unsafe {
                checkModuleInitialized();
                EdfClassifyResultUM* classifyResultUM = null;
                int classifyStatus = fcnEdfClassify(&desc, pvModuleState, &classifyResultUM, &config);
                if (classifyStatus != 0) {
                    throw new EdfException("Error during descriptor classification.");
                }

                EdfClassifyResult classifyResult = classifyResultUM->toEdfClassifyResult();
                
                int freeStatus = fcnEdfFreeClassifyResult(&classifyResultUM, pvModuleState);
                if (freeStatus != 0) {
                    throw new EdfException("Error during descriptor classification clean up.");
                }

                return classifyResult;
            }
        }

        /// <summary>
        /// Classifies the <seealso cref="EdfDescriptor"/> and outputs the result in the <seealso cref="EdfClassifyResult"/>.
        /// </summary>
        /// <param name="desc">Input <seealso cref="EdfDescriptor"/> to classify.</param>
        /// <returns>Classification result values.</returns>
        public EdfClassifyResult edfClassify(EdfDescriptor desc) {
            return edfClassify(desc, EdfClassifyConfig.getDefault());
        }
    }
}
