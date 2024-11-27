########################################################################################
##                                                                                    ##
##    Python Eyedentify wrapper eyedentify.py                                         ##
## ---------------------------------------------------------------------------------- ##
##                                                                                    ##
## Copyright (c) 2020 by Eyedea Recognition, s.r.o.                                   ##
##                                                                                    ##
## Author: Eyedea Recognition, s.r.o.                                                 ##
##                                                                                    ##
## Contact:                                                                           ##
##           web: http://www.eyedea.cz                                                ##
##           email: info@eyedea.cz                                                    ##
##                                                                                    ##
## BSD License                                                                        ##
## -----------------------------------------------------------------------------------##
## Copyright (c) 2020, Eyedea Recognition, s.r.o.                                     ##
## All rights reserved.                                                               ##
## Redistribution and use in source and binary forms, with or without modification,   ##
## are permitted provided that the following conditions are met :                     ##
##     1. Redistributions of source code must retain the above copyright notice,      ##
##        this list of conditions and the following disclaimer.                       ##
##     2. Redistributions in binary form must reproduce the above copyright notice,   ##
##        this list of conditions and the following disclaimer in the documentation   ##
##        and # or other materials provided with the distribution.                    ##
##     3. Neither the name of the copyright holder nor the names of its contributors  ##
##        may be used to endorse or promote products derived from this software       ##
##        without specific prior written permission.                                  ##
## THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"        ##
## AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED  ##
## WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. ##
## IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,   ##
## INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES(INCLUDING, BUT  ##
## NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR ##
## PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,  ##
## WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT(INCLUDING NEGLIGENCE OR OTHERWISE)  ##
## ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF             ##
## THE POSSIBILITY OF SUCH DAMAGE.                                                    ##
########################################################################################

from cffi import FFI
from PIL import Image
from er import ER
import numpy as np
import inspect
import platform
import os

ER_COMPUTATION_MODE_CPU = 0
ER_COMPUTATION_MODE_GPU = 1


class EdfError(Exception):
    """Eyedentify Error class."""

    def __init__(self, internal_function_name, internal_error_code):
        self.internal_function_name = internal_function_name
        self.internal_error_code = internal_error_code


class EdfInitConfig:
    """Mirror of EfInitConfig structure."""

    def __init__(self):
        self.module_path = ""
        self.model_file = ""
        self.computation_mode = ER_COMPUTATION_MODE_CPU
        self.gpu_device_id = 0
        self.num_threads = 0
        self.onnx_provider = "cpu"


class EdfDescriptor:
    """Mirror of EdfDescriptor structure."""

    def __init__(self):
        self.version = 0
        self.data = bytearray()


class EdfPoints:
    """Mirror of EdfPoints structure."""

    def __init__(self):
        self.rows = []
        self.cols = []


class EdfValues:
    """Mirror of EdfValues structure."""

    def __init__(self):
        self.values = []


class EdfCropParams:
    """Mirror of EdfCropParams structure."""

    def __init__(self):
        self.points = EdfPoints()
        self.values = EdfValues()


class EdfClassifyResultValue:
    """Mirror of EdfClassifyResultValue structure."""

    def __init__(self):
        self.task_name = ""
        self.class_name = ""
        self.class_id = 0
        self.score = 0.0

    def __repr__(self):
        return "{}: {}[{}] ({:0.4f})".format(self.task_name, self.class_name, self.class_id, self.score)


class EdfComputeDescConfig:
    """Mirror of EdfComputeDescConfig structure."""

    def __init__(self):
        self.batch_size = 1


class EdfCropImageConfig:
    """Mirror of EdfCropImageConfig structure."""

    def __init__(self):
        self.full_crop = 0
        self.color_normalization = 0
        self.use_antialiasing = 0
        self.antialiasing_kernel_size = 0
        self.antialiasing_sigma = 0


class EdfClassifyConfig:
    """Mirror of EdfClassifyConfig structure."""

    def __init__(self):
        self.use_dependency_rules = 0


# Class
class Eyedentify:
    __ffi_loaded = set()
    __edf = dict()

    """Python wrapper class for Eyedentify SDK."""

    @staticmethod
    def __define_ffi(ffi):
        # type definitions from edf_type.h
        ffi.cdef("""
                 typedef struct {
                     unsigned int        version;     // version of the model used to create the feature
                     unsigned int        size;        // number of bytes in the data array
                     unsigned char*      data;        // pointer to descriptor data
                 } EdfDescriptor;
                 """)

        ffi.cdef("""
                 typedef struct {
                     int     length;              // Number of valid points in the set.
                     double* rows;                // Row coordinates of points.
                     double* cols;                // Column coordinates of points.
                 } EdfPoints;
                 """)

        ffi.cdef("""
                 typedef struct {
                     int     length;              // Number of valid values in the set.
                     double* values;              // Contained values.
                 } EdfValues;
                 """)

        ffi.cdef("""
                 typedef struct {
                     EdfPoints points;
                     EdfValues values;
                 } EdfCropParams;
                 """)

        ffi.cdef("""
                 typedef struct {
                     char* task_name;                      // Name of the value entry
                     unsigned int task_name_length;        // Length of the name

                     char* class_name;                     // String value
                     unsigned int class_name_length;       // Length of the string value
                     int class_id;
                     float score;
                 } EdfClassifyResultValue;
                 """)

        ffi.cdef("""
                 typedef struct {
                     unsigned int num_values;        // Number of values
                     EdfClassifyResultValue* values; // Array of result values
                 } EdfClassifyResult;
                 """)

        ffi.cdef("""
                 typedef enum { ER_COMPUTATION_MODE_CPU = 0, ER_COMPUTATION_MODE_GPU = 1 } EdfComputationMode;
                 """)

        ffi.cdef("""
                 typedef struct {
                     const char* module_path;             // null terminated string with path to the module
                     const char* model_file;              // null terminated string with model filename
                     EdfComputationMode computation_mode; // selected computation mode
                     int gpu_device_id;                   // GPU device identifier 
                                                          //(used only when computation_mode == EDF_GPU)
                     int num_threads;                     // number of threads to run the ER_COMPUTATION_MODE_CPU
                                                          //computation, value <=0 sets std::thread::hardware_concurency
                     const char* onnx_provider;           // specify onnx provider to be used (supported by edfonnx modules only)

                 } EdfInitConfig;
                 """)

        ffi.cdef("""
                 typedef struct {
                     unsigned int batch_size;
                 } EdfComputeDescConfig;
                 """)

        ffi.cdef("""
                 typedef struct {
                     int full_crop;
                     int color_normalization;
                     int use_antialiasing;
                     unsigned int antialiasing_kernel_size;
                     float antialiasing_sigma;
                 } EdfCropImageConfig;
                 """)

        ffi.cdef("""
                 typedef struct {
                     int use_dependency_rules;
                 } EdfClassifyConfig;
                 """)

        # function definitions from edf.h
        ffi.cdef("""
                 int edfInitEyedentify(const EdfInitConfig* init_config, void** module_state);
                 """)

        ffi.cdef("""
                 void edfFreeEyedentify(void** module_state);
                 """)

        ffi.cdef("""
                 typedef int (*fcn_edfInferenceCallback) (const ERImage*, unsigned char*);
                 """)

        ffi.cdef("""
                 int edfInitEyedentify_ExternalInference(const EdfInitConfig* init_config,
                                                           fcn_edfInferenceCallback inference_callback,
                                                           unsigned int output_buffer_size,
                                                           void** module_state);
                 """)

        ffi.cdef("""
                 int edfComputeDesc(const ERImage* img, const void* module_state,
                                               EdfDescriptor* descriptor, EdfComputeDescConfig* config);
                 """)

        ffi.cdef("""
                 int edfCompareDescs(const EdfDescriptor* desc_A, const EdfDescriptor* desc_B,
                                                void const* module_state, float* score);
                 """)

        ffi.cdef("""
                 void edfAllocDesc(EdfDescriptor* desc, unsigned int size, unsigned int version);
                 """)

        ffi.cdef("""
                 void edfFreeDesc(EdfDescriptor* desc);
                 """)

        ffi.cdef("""
                 int edfCropImage(const ERImage* image_in, EdfCropParams* params, void* module_state,
                                             ERImage* cropped_image, EdfCropImageConfig* config);
                 """)

        ffi.cdef("""
                 void edfFreeCropImage(void* module_state, ERImage* cropped_image);
                 """)

        ffi.cdef("""
                 unsigned int edfModelVersion(const void* module_state);
                 """)

        ffi.cdef("""
                 int edfClassify(const EdfDescriptor* desc, void* module_state, 
                                 EdfClassifyResult** classify_result, EdfClassifyConfig* config);
                 """)

        ffi.cdef("""
                 int edfFreeClassifyResult(EdfClassifyResult** classify_result, void* module_state);
                 """)

    @staticmethod
    def get_eyedentify_path(sdk_dir_path):
        eyedentify_lib_name = "eyedentify-"
        if platform.system() == 'Windows':
            arch = platform.architecture()[0]
            if arch == '32bit':
                eyedentify_lib_name += "Win32"
            elif arch == '64bit':
                eyedentify_lib_name += "x64"
            else:
                raise Exception("Unsupported Windows architecture " + arch)
            eyedentify_lib_name += ".dll"
        elif platform.system() == 'Linux':
            eyedentify_lib_name = "lib" + eyedentify_lib_name + platform.machine() + ".so"
        else:
            raise Exception("Unsupported platform " + platform.system())

        path = os.path.normpath(os.path.join(sdk_dir_path, "lib/" + eyedentify_lib_name))
        return path

    # only loads C DLL one time for the same ffi object
    def __init__(self, ffi, sdk_dir_path, support_libs=None):
        self.ffi = ffi
        self.eyedentify_path = Eyedentify.get_eyedentify_path(sdk_dir_path)
        if type(self.eyedentify_path) is not str:
            raise TypeError("Variable eyedentify_path must be a string.")

        if ffi not in Eyedentify.__ffi_loaded:
            # add function definitions to ffi
            self.__define_ffi(self.ffi)

            # load top library and openblas dependency
            Eyedentify.__edf[self.ffi] = self.ffi.dlopen(self.eyedentify_path)

            if type(support_libs) is list:
                for lib in support_libs:
                    self.ffi.dlopen(lib)

            Eyedentify.__ffi_loaded.add(ffi)

        self.__module_state = self.ffi.new("void**", self.ffi.NULL)

    def init_eyedentify(self, init_config):
        # construct C-counterpart config
        edf_config = self.ffi.new("const EdfInitConfig*")
        module_path_str = self.ffi.new("char[]", init_config.module_path.encode("utf-8"))
        edf_config.module_path = self.ffi.cast("char*", module_path_str)
        model_file_str = self.ffi.new("char[]", init_config.model_file.encode('utf-8'))
        edf_config.model_file = self.ffi.cast("char*", model_file_str)
        edf_config.computation_mode = self.ffi.cast("int", init_config.computation_mode)
        edf_config.gpu_device_id = self.ffi.cast("int", init_config.gpu_device_id)
        edf_config.num_threads = self.ffi.cast("int", init_config.num_threads)
        onnx_provider_str = self.ffi.new("char[]", init_config.onnx_provider.encode("utf-8"))
        edf_config.onnx_provider = self.ffi.cast("char*", onnx_provider_str)

        # allocate internal module_state
        if self.__module_state[0] != self.ffi.NULL:
            print("edfInitEyedentify: Already initialized, skipping...")
            return

        # call edfInitEyedentify from C
        ret_code = Eyedentify.__edf[self.ffi].edfInitEyedentify(edf_config, self.__module_state)

        if ret_code != 0:
            raise EdfError("edfInitEyedentify", ret_code)

        self.__module_state = self.ffi.gc(
            self.__module_state, Eyedentify.__edf[self.ffi].edfFreeEyedentify)

    def init_eyedentify_with_external_inference(self, init_config, inference_callback, output_buffer_size):
        # construct C-counterpart config
        edf_config = self.ffi.new("const EdfInitConfig*")
        module_path_str = self.ffi.new("char[]", init_config.module_path.encode("utf-8"))
        edf_config.module_path = self.ffi.cast("char*", module_path_str)
        model_file_str = self.ffi.new("char[]", init_config.model_file.encode('utf-8'))
        edf_config.model_file = self.ffi.cast("char*", model_file_str)
        edf_config.computation_mode = self.ffi.cast("int", init_config.computation_mode)
        edf_config.gpu_device_id = self.ffi.cast("int", init_config.gpu_device_id)
        edf_config.num_threads = self.ffi.cast("int", init_config.num_threads)
        onnx_provider_str = self.ffi.new("char[]", init_config.onnx_provider.encode("utf-8"))
        edf_config.onnx_provider = self.ffi.cast("char*", onnx_provider_str)

        # allocate internal module_state
        if self.__module_state[0] != self.ffi.NULL:
            print("edfInitEyedentify: Already initialized, skipping...")
            return

        # call edfInitEyedentify from C
        ret_code = Eyedentify.__edf[self.ffi].edfInitEyedentify_ExternalInference(edf_config, inference_callback, output_buffer_size, self.__module_state)

        if ret_code != 0:
            raise EdfError("edfInitEyedentify_ExternalInference", ret_code)

        self.__module_state = self.ffi.gc(
            self.__module_state, Eyedentify.__edf[self.ffi].edfFreeEyedentify)

    # warning: this is not a traditional destructor, it is also called if __init__ raises exception.
    def __del__(self):
        pass

    def compute_desc(self, er_image_crop, config=None):

        batch_size = 1
        if isinstance(er_image_crop, list):
            if len(er_image_crop) > 0:
                batch_size = len(er_image_crop)

        if config is not None:
            if 0 > config.batch_size <= batch_size:
                batch_size = config.batch_size

        c_config = self.ffi.new("EdfComputeDescConfig*")
        c_config.batch_size = self.ffi.cast("unsigned int", batch_size)

        descriptor = self.ffi.new("EdfDescriptor[" + str(batch_size) + "]")

        crops = self.ffi.new("ERImage[" + str(batch_size) + "]")
        if batch_size > 1:
            for c in range(0, batch_size):
                crops[c] = er_image_crop[c][0]
        else:
            crops[0] = er_image_crop[0]

        ret_code = Eyedentify.__edf[self.ffi].edfComputeDesc(crops, self.__module_state[0],
                                                             descriptor, c_config)

        if ret_code != 0:
            raise EdfError("edfComputeDesc", ret_code)

        # convert descriptor to byte array
        py_descs = []
        for c in range(0, batch_size):
            py_desc = EdfDescriptor()
            py_desc.version = descriptor[c].version
            py_desc.data = bytearray(descriptor[c].size)
            self.ffi.memmove(py_desc.data, descriptor[c].data, descriptor[c].size)
            py_descs.append(py_desc)

            Eyedentify.__edf[self.ffi].edfFreeDesc(
                self.ffi.addressof(descriptor[c]))

        if len(py_descs) == 1:
            return py_descs[0]
        else:
            return py_descs

    def compare_descs(self, desc_a, desc_b):

        c_desc_a = self.ffi.new("EdfDescriptor*")
        c_desc_b = self.ffi.new("EdfDescriptor*")

        Eyedentify.__edf[self.ffi].edfAllocDesc(c_desc_a, len(desc_a.data), desc_a.version)
        Eyedentify.__edf[self.ffi].edfAllocDesc(c_desc_b, len(desc_b.data), desc_b.version)

        self.ffi.memmove(c_desc_a.data, desc_a.data, len(desc_a.data))
        self.ffi.memmove(c_desc_b.data, desc_b.data, len(desc_b.data))

        score = self.ffi.new("float*")

        ret_code = Eyedentify.__edf[self.ffi].edfCompareDescs(c_desc_a, c_desc_b,
                                                              self.__module_state[0], score)

        Eyedentify.__edf[self.ffi].edfFreeDesc(c_desc_a)
        Eyedentify.__edf[self.ffi].edfFreeDesc(c_desc_b)

        if ret_code != 0:
            raise EdfError("edfCompareDescs", ret_code)

        return score[0]

    def crop_image(self, er_image, params, config=None):

        crop_params = self.ffi.new("EdfCropParams*")
        crop_params.points.length = self.ffi.cast("int", min(len(params.points.cols), len(params.points.rows)))
        point_cols = self.ffi.new("double[]", params.points.cols)
        point_rows = self.ffi.new("double[]", params.points.rows)
        crop_params.points.cols = self.ffi.cast("double*", point_cols)
        crop_params.points.rows = self.ffi.cast("double*", point_rows)
        crop_params.values.length = self.ffi.cast("int", len(params.values.values))
        values = self.ffi.new("double[]", params.values.values)
        crop_params.values.values = self.ffi.cast("double*", values)

        crop_config = self.ffi.new("EdfCropImageConfig*")

        if config is not None:
            crop_config.full_crop = config.full_crop
            crop_config.color_normalization = config.color_normalization
            crop_config.use_antialiasing = config.use_antialiasing
            crop_config.antialiasing_kernel_size = config.antialiasing_kernel_size
            crop_config.antialiasing_sigma = config.antialiasing_sigma
        else:
            crop_config = self.ffi.NULL

        # convert input image to EdfImage
        c_image_out = self.ffi.new("ERImage*")

        ret_code = Eyedentify.__edf[self.ffi].edfCropImage(er_image, crop_params,
                                                           self.__module_state[0], c_image_out, crop_config)

        if ret_code != 0:
            raise EdfError("edfCropImage", ret_code)

        # convert output crop image from EdfImage to numpy array
        cropped_image, mode = ER.convert_erimage_to_nparray(c_image_out[0])

        Eyedentify.__edf[self.ffi].edfFreeCropImage(self.__module_state[0], c_image_out)

        # return cropped_image
        return cropped_image, mode

    def model_version(self):
        return Eyedentify.__edf[self.ffi].edfModelVersion(self.__module_state[0])

    def classify(self, desc, config=None):
        """classify based on the descriptor"""
        classify_res = self.ffi.new("EdfClassifyResult**")
        c_desc = self.ffi.new("EdfDescriptor*")

        Eyedentify.__edf[self.ffi].edfAllocDesc(c_desc, len(desc.data), desc.version)
        self.ffi.memmove(c_desc.data, desc.data, len(desc.data))

        classify_config = self.ffi.new("EdfClassifyConfig*")

        if config is not None:
            classify_config.use_dependency_rules = config.use_dependency_rules
        else:
            classify_config = self.ffi.NULL

        ret_code = Eyedentify.__edf[self.ffi].edfClassify(c_desc, self.__module_state[0],
                                                          classify_res, classify_config)

        if ret_code != 0:
            raise EdfError("edfClassify", ret_code)

        # copy the classify_result to output
        py_classify_res = []

        for i in range(0, classify_res[0].num_values):
            res_val = EdfClassifyResultValue()

            res_val.task_name = self.ffi.string(
                classify_res[0].values[i].task_name, classify_res[0].values[i].task_name_length).decode("utf-8")
            res_val.class_name = self.ffi.string(
                classify_res[0].values[i].class_name, classify_res[0].values[i].class_name_length).decode("utf-8")
            res_val.class_id = classify_res[0].values[i].class_id
            res_val.score = classify_res[0].values[i].score

            py_classify_res.append(res_val)

        # free underlying memory
        ret_code = Eyedentify.__edf[self.ffi].edfFreeClassifyResult(classify_res, self.__module_state[0])

        if ret_code != 0:
            raise EdfError("edfFreeClassifyResult", ret_code)

        return py_classify_res
