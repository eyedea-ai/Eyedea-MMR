///////////////////////////////////////////////////////////
//                                                       //
// Copyright (c) 2014-2020 by Eyedea Recognition, s.r.o. //
//                  ALL RIGHTS RESERVED.                 //
//                                                       //
// Author: Eyedea Recognition, s.r.o.                    //
//                                                       //
// Contact:                                              //
//           web: http://www.eyedea.cz                   //
//           email: info@eyedea.cz                       //
//                                                       //
// Consult your license regarding permissions and        //
// restrictions.                                         //
//                                                       //
///////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////
//                   EYEDENTIFY SDK                      //
//                 recognition library                   //
///////////////////////////////////////////////////////////

#ifndef EDF_TYPE
#define EDF_TYPE

#include "er_image.h"
#include "er_explink.h"
#include "er_type.h"

// Memory alignment used for EdfDescriptor data allocation
#define EDF_MEMORY_ALIGNMENT 32

#define EDF_ERROR_GPU_MODE_NOT_SUPPORTED -67856865
#define EDF_ERROR_GPU_DEVICE_ID_FAILED   -67856866
#define EDF_ERROR_GPU_DEVICE_BUSY        -67856867
#define EDF_ERROR_GPU_DEVICE_COUNT       -67856868
#define EDF_ERROR_NO_GPU_DEVICE          -67856869
#define EDF_ERROR_GPU_DEVICE_ID_INVALID  -67856870

//////////////////////////////////////////////////////////////
//      EdfDescriptor                                       //
//////////////////////////////////////////////////////////////
// Descriptor is a buffer with condensated description      //
// of the input image. It is used to compare two images     //
// using edfCompareDescs or to classify an image using      //
// edfClassify.                                             //
//////////////////////////////////////////////////////////////
typedef struct {
    unsigned int        version;     // version of the model used to create the feature
    unsigned int        size;        // number of bytes in the data array
    unsigned char*      data;        // pointer to descriptor data
} EdfDescriptor;

//////////////////////////////////////////////////////////////
//      EdfPoints                                           //
//////////////////////////////////////////////////////////////
// EdfPoints is a structure containing 2D points.           //
// It is used to specify the input 2D points in the         //
// EdfCropParams structure.                                 //
//////////////////////////////////////////////////////////////
typedef struct {
    int     length;              // Number of valid points in the set.
    double* rows;                // Row coordinates of points.
    double* cols;                // Column coordinates of points.
} EdfPoints;

//////////////////////////////////////////////////////////////
//      EdfValues                                           //
//////////////////////////////////////////////////////////////
// EdfValues is a structure containing real number values.  //
// It is used to specify the input parameters in the        //
// EdfCropParams structure.                                 //
//////////////////////////////////////////////////////////////
typedef struct {
    int     length;              // Number of valid values in the set.
    double* values;              // Contained values.
} EdfValues;

//////////////////////////////////////////////////////////////
//      EdfCropParams                                       //
//////////////////////////////////////////////////////////////
// EdfCropParams is a structure containing EdfPoints and    //
// EdfValues structures. It is used to specify the input    //
// parameters for the edfCropImage() function.              //
//////////////////////////////////////////////////////////////
typedef struct {
    EdfPoints points;
    EdfValues values;
} EdfCropParams;

//////////////////////////////////////////////////////////////
//      EdfClassifyResultValue                              //
//////////////////////////////////////////////////////////////
// EdfClassifyResultValue represents one entry of           //
// the classification result. It contains the name of       //
// the value and value itself both represented by string.   //
//////////////////////////////////////////////////////////////
typedef struct {
    char* task_name;                      // Name of the value entry
    unsigned int task_name_length;        // Length of the name

    char* class_name;                     // String value
    unsigned int class_name_length;       // Length of the string value
    int class_id;                         // Class identificator
    float score;                          // Classification score
} EdfClassifyResultValue;

//////////////////////////////////////////////////////////////
//      EdfClassifyResult                                   //
//////////////////////////////////////////////////////////////
// EdfClassifyResult represents the classification result.  //
// It contains the number of entries and the entries itself //
// represented by the EdfClassifyResultValue structure.     //
//////////////////////////////////////////////////////////////
typedef struct {
    unsigned int num_values;        // Number of values
    EdfClassifyResultValue* values; // Array of result values
} EdfClassifyResult;

//////////////////////////////////////////////////////////////
// Config values defines                                    //
//////////////////////////////////////////////////////////////
// Predefined value to enable setting.
#define EDF_CONFIG_VALUE_ENABLED   1
// Predefined value to disable setting.
#define EDF_CONFIG_VALUE_DISABLED -1
// Predefined value to use default setting.
#define EDF_CONFIG_VALUE_DEFAULT   0

//////////////////////////////////////////////////////////////
//      EdfInitConfig                                       //
//////////////////////////////////////////////////////////////
// EdfInitConfig represents the configuration parameters    //
// set used during SDK module initialization.               //
//////////////////////////////////////////////////////////////
typedef struct {
    const char* module_path;             // null terminated string with path to the module
    const char* model_file;              // null terminated string with model filename
    ERComputationMode computation_mode;  // selected computation mode
    int gpu_device_id;                   // GPU device identifier (used only when computation_mode == ER_COMPUTATION_MODE_GPU)
    int num_threads;                     // number of threads to run the ER_COMPUTATION_MODE_CPU computation; special values: 0 for 1 thread, <0 for 0.9*std::thread::hardware_concurency
    const char* onnx_provider;           // specify onnx provider to be used (supported by edfonnx modules only)
} EdfInitConfig;

//////////////////////////////////////////////////////////////
//      EdfComputeDescConfig                                //
//////////////////////////////////////////////////////////////
// EdfComputeDescConfig represents the configuration        //
// parameters used during descriptor computation.           //
//////////////////////////////////////////////////////////////
typedef struct {
    unsigned int batch_size; // Size of the input data batch.
                             // Set 0 to disable batch processing.
                             // Set 1-N to set the size of the batch (value 1 has the same effect as 0).
                             // Using sdks/modules/edftf2lite-* backend requires setting batch_size to 0 or 1. May change in a future release.
} EdfComputeDescConfig;

//////////////////////////////////////////////////////////////
//      EdfCropImageConfig                                  //
//////////////////////////////////////////////////////////////
// EdfCropImageConfig represents the configuration          //
// parameters used during input image cropping.             //
//////////////////////////////////////////////////////////////
typedef struct {
    int          full_crop;                // Set to  1 to create the full crop (with the border).
                                           // Set to  0 to create default  crop.
                                           // Set to -1 to create standard crop.
    int          color_normalization;      // Set to  1 to     use color normalization.
                                           // Set to  0 to     use color normalization default setting.
                                           // Set to -1 to not use color normalization.
    int          use_antialiasing;         // Set to  1 to     use antialiasing during image transformation.
                                           // Set to  0 to     use antialiasing default setting.
                                           // Set to -1 to not use antialiasing during image transformation.
    unsigned int antialiasing_kernel_size; // The size of the convolution kernel used during antialiasing.
                                           // Set to 0 to use the default convolution kernel size (computed from transformation scale). DEFAULT
                                           // Used only in combination with use_antialiasing == 1.
    float        antialiasing_sigma;       // The sigma parameter of the Gaussian distribution in the antialiasing convolution kernel.
                                           // Set to 0.0f to use the default sigma size (computed from kernel size). DEFAULT
                                           // Used only in combination with use_antialiasing == 1.
} EdfCropImageConfig;

//////////////////////////////////////////////////////////////
//      EdfClassifyConfig                                   //
//////////////////////////////////////////////////////////////
// EdfClassifyConfig represents the configuration           //
// parameters used during descriptor classification.        //
//////////////////////////////////////////////////////////////
typedef struct {
    int use_dependency_rules; // Set to  0 to get classification results      with                                  dependency rules applied. DEFAULT
                              // Set to  1 to get classification results both with and without (_NODEP name suffix) dependency rules applied.
                              // Set to -1 to get classification results               without (_NODEP name suffix) dependency rules applied.
    int num_top_scores;       // define number of best results per classification task
                              // Set to 0 or 1 for the top-1 classification (default)
                              // >1 return sorted min(num_top_scores,num_classes) scores
                              // Set -1 to return all classes, not sorted
} EdfClassifyConfig;

///////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////
// Logging macros definitions
///////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////
#ifdef __cplusplus
#include <iostream>
#define EDF_LOG_INFO(msg)    std::cout << "Eyedentify: "           << msg << std::endl;
#define EDF_LOG_ERR(msg)     std::cerr << "ERROR - Eyedentify: "   << msg << std::endl;
#define EDF_LOG_WARNING(msg) std::cerr << "Warning - Eyedentify: " << msg << std::endl;
#else
#include <stdio.h>
#define EDF_LOG_INFO(msg)    printf("Eyedentify: %s\n"          , msg);
#define EDF_LOG_ERR(msg)     printf("ERROR - Eyedentify: %s\n"  , msg);
#define EDF_LOG_WARNING(msg) printf("Warning - Eyedentify: %s\n", msg);
#endif

// Shared library name ////////////////////////////////////////
#define EDF_SHLIB_NAME ER_LIB_PREFIX "eyedentify-" ER_LIB_TARGET ER_LIB_EXT

// For backward compatibility, we keep EDF_ versions of ER_ functions
typedef shlib_hnd edf_shlib_hnd;
#define EDF_OPEN_SHLIB(shlib_hnd, shlib_filename) ER_OPEN_SHLIB(shlib_hnd, shlib_filename)
#define EDF_LOAD_SHFCN(shfcn_ptr, shfcn_type, shlib_hnd, shfcn_name) ER_LOAD_SHFCN(shfcn_ptr, shfcn_type, shlib_hnd, shfcn_name)
#define EDF_FREE_LIB(shlib_hnd) ER_FREE_LIB(shlib_hnd)
#define EDF_FUNCTION_PREFIX ER_FUNCTION_PREFIX

#if defined(WIN32) || defined(_WIN32) || defined(_DLL) || defined(_WINDOWS_) || defined(_WINDLL)
#include <windows.h>
#define EDF_FILE_OPEN(file_id, path) fopen_s(&file_id, path, "rb")
#else
#define EDF_FILE_OPEN(file_id, path) file_id = fopen(path, "rb")
#endif

// Eyedentify API functions
typedef int          (*fcn_edfInferenceCallback)     (const ERImage*, unsigned char*);
typedef int          (*fcn_edfInitEyedentify)     (const EdfInitConfig*, void**);
typedef int          (*fcn_edfInitEyedentify_ExternalInference) (const EdfInitConfig*, fcn_edfInferenceCallback, unsigned int, void**);
typedef void         (*fcn_edfFreeEyedentify)     (void**);
typedef int          (*fcn_edfComputeDesc)        (const ERImage*, const void*, EdfDescriptor*, EdfComputeDescConfig*);
typedef int          (*fcn_edfCompareDescs)       (const EdfDescriptor*, const EdfDescriptor*, void const*, float*);
typedef void         (*fcn_edfAllocDesc)          (EdfDescriptor*, unsigned int, unsigned int);
typedef void         (*fcn_edfFreeDesc)           (EdfDescriptor*);
typedef void         (*fcn_edfCropParamsAllocate) (int, int, EdfCropParams*);
typedef void         (*fcn_edfCropParamsWrap)     (int, double*, double*, int, double*, EdfCropParams*);
typedef void         (*fcn_edfCropParamsFree)     (EdfCropParams*);
typedef int          (*fcn_edfCropImage)          (const ERImage*, EdfCropParams*, void*, ERImage*, EdfCropImageConfig*);
typedef int          (*fcn_edfFreeCropImage)      (void*, ERImage*);
typedef unsigned int (*fcn_edfModelVersion)       (const void*);
typedef int          (*fcn_edfClassify)           (const EdfDescriptor*, void*, EdfClassifyResult**, EdfClassifyConfig*);
typedef int          (*fcn_edfFreeClassifyResult) (EdfClassifyResult**, void*);



struct EdfAPI {
    shlib_hnd                           shlib_handle;
    // Eyedentify API main functions
    fcn_edfInitEyedentify               edfInitEyedentify;
    fcn_edfInitEyedentify_ExternalInference  edfInitEyedentify_ExternalInference;
    fcn_edfFreeEyedentify               edfFreeEyedentify;
    fcn_edfComputeDesc                  edfComputeDesc;
    fcn_edfCompareDescs                 edfCompareDescs;
    fcn_edfAllocDesc                    edfAllocDesc;
    fcn_edfFreeDesc                     edfFreeDesc;
    fcn_edfCropParamsAllocate           edfCropParamsAllocate;
    fcn_edfCropParamsWrap               edfCropParamsWrap;
    fcn_edfCropParamsFree               edfCropParamsFree;
    fcn_edfCropImage                    edfCropImage;
    fcn_edfFreeCropImage                edfFreeCropImage;
    fcn_edfModelVersion                 edfModelVersion;
    fcn_edfClassify                     edfClassify;
    fcn_edfFreeClassifyResult           edfFreeClassifyResult;
    // ERImage API utils functions
    fcn_erImageGetDataTypeSize          erImageGetDataTypeSize;
    fcn_erImageGetColorModelNumChannels erImageGetColorModelNumChannels;
    fcn_erImageGetPixelDepth            erImageGetPixelDepth;
    fcn_erImageAllocateBlank            erImageAllocateBlank;
    fcn_erImageAllocate                 erImageAllocate;
    fcn_erImageAllocateAndWrap          erImageAllocateAndWrap;
    fcn_erImageCopy                     erImageCopy;
    fcn_erImageRead                     erImageRead;
    fcn_erImageWrite                    erImageWrite;
    fcn_erImageFree                     erImageFree;
    fcn_erVersion                       erVersion;
    fcn_erGetErrorLog                   erGetErrorLog;
    fcn_erResetErrorLog                 erResetErrorLog;
};

typedef int          (*fcn_edfLinkAPI)            (shlib_hnd, EdfAPI*);

/////////////////////////////////////////////////////////////////////////

#endif // EDF_TYPE
