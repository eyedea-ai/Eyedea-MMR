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

#ifndef EYEDEA_EYEDENTIFY_SDK_H
#define EYEDEA_EYEDENTIFY_SDK_H

#include "edf_type.h"

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    edfLinkAPI                                                                                                    //
//      Funtion to explicitly link all api functions.                                                               //
//      This function is the only one to be linked by user.                                                         //
//                                                                                                                  //
//      input:          handle - pointer loaded library.                                                            //
//                             If eyedentify library is linked implicitly (during compilation), EdfAPI can still be //
//                             used, only call the function with nullptr handle.                                    //
//      output:         api  - pointer to structure with pointers to api functions to be linked                     //
//                                                                                                                  //
//      return value:   0 on success, error code on failure                                                         //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/** Explicit linking of all EyeScan SDK API functions into EsAPI structure. */
ER_FUNCTION_PREFIX int edfLinkAPI(shlib_hnd handle, EdfAPI *api);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    edfInitEyedentify                                                                                             //
//      Initializes the EyeDentify SDK module. The SDK is not thread-safe. All calls to the SDK                     //
//      are done using the module_state pointer returned by this function.                                          //
//                                                                                                                  //
//      input:          init_config   - pointer to the initialization structure                                     //
//      output:         module_state  - pointer to the module state                                                 //
//                                                                                                                  //
//      return value:   0 on success, error code on failure                                                         //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
ER_FUNCTION_PREFIX int edfInitEyedentify(const EdfInitConfig* init_config, void** module_state);

ER_FUNCTION_PREFIX int edfInitEyedentify_ExternalInference(const EdfInitConfig* init_config,
                                                           fcn_edfInferenceCallback inference_callback,
                                                           unsigned int output_buffer_size,
                                                           void** module_state);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    edfFreeEyedentify                                                                                             //
//      Frees the state previously initialized by edfInitEyedentify.                                                //
//                                                                                                                  //
//      input:          module_state - pointer to the module state                                                  //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
ER_FUNCTION_PREFIX void edfFreeEyedentify(void** module_state);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    edfComputeDesc                                                                                                //
//      Computes the descriptor given the aligned image and the module state. The image must                        //
//      be prealigned with model dependent registration technique. See examples.                                    //
//                                                                                                                  //
//      input:          img          - registered and aligned image using edfCropImage                              //
//                      module_state - pointer to the module state                                                  //
//                      config       - descriptor computation configuration (can be NULL)                           //
//      output:         descriptor   - pointer to a user defined EdfDescriptor structure to fill                    //
//                                                                                                                  //
//      return value:   0 on success, error code on failure                                                         //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
ER_FUNCTION_PREFIX int edfComputeDesc(const ERImage* img, const void* module_state,
                                       EdfDescriptor* descriptor, EdfComputeDescConfig* config);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    edfCompareDescs                                                                                               //
//      Compares two descriptors and returns a score. The higher the score, the better match between images.        // 
//                                                                                                                  //
//      input:          desc_A, desc_B - descriptors for verification, outputted by edfComputeDesc                  //
//                      module_state - pointer to the module state                                                  //
//      output:         score - score of the verification to be filled                                              //
//                                                                                                                  //
//      return value:   0 on success, error code on failure                                                         //
//                                                                                                                  //
//      ////////////////////////////////////////////////////////////////////////////////////////////////////        //
//      // How to speedup edfCompareDescs function calls?                                                 //        //
//      ////////////////////////////////////////////////////////////////////////////////////////////////////        //
//      // The Eyedentify SDK uses SSE2 instruction set to achieve speedup on x86 processors. To benefit  //        //
//      // from this feature, you have to keep the "data" members of edfdescriptor structure aligned      //        //
//      // to 16 bytes. That means size_t(edfdescriptor.data) % 16 == 0. By default, the data field in    //        //
//      // descriptors returned from the library have the data field aligned as necessary. If you store   //        //
//      // the structure and later load it for matching, the data member will be probably misaligned and  //        //
//      // the speed of matching will degrade by a factor of 4. The alignment can be achived using        //        //
//      // _mm_malloc/_mm_free routines on Unix/Windows or using edfAllocDesc or by an own allocator.     //        //
//      ////////////////////////////////////////////////////////////////////////////////////////////////////        //
//                                                                                                                  //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
ER_FUNCTION_PREFIX int edfCompareDescs(const EdfDescriptor* desc_A, const EdfDescriptor* desc_B,
                                        void const* module_state, float* score);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    edfAllocDesc                                                                                                  //
//      Alloc descriptors storage.                     .                                                            //
//                                                                                                                  //
//      input:          descriptor  - pointer to a EdfDescriptor structure                                          //
//                      size        - size in bytes of the descriptor data to be allocated                          //
//                      version     - version of the descriptor                                                     //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
ER_FUNCTION_PREFIX void edfAllocDesc(EdfDescriptor* desc, unsigned int size, unsigned int version);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    edfFreeDesc                                                                                                   //
//      Free descriptors computed using edfComputeDesc.                                                             //
//                                                                                                                  //
//      input:          descriptor  - pointer to a EdfDescriptor structure                                          //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
ER_FUNCTION_PREFIX void edfFreeDesc(EdfDescriptor* desc);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    edfCropParamsAllocate                                                                                         //
//      Allocates the points and values in the EdfCropParams structure instance. The structure EdfCropParams        //
//      must be initialized on the user's side, this function only allocates the underlying data.                   //
//                                                                                                                  //
//      input:          size_points - number of points to allocate                                                  //
//                      size_values - number of values to allocate                                                  //
//      output:         params      - pointer to the EdfCropParams instance to allocate the data in                 //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
ER_FUNCTION_PREFIX void edfCropParamsAllocate(int size_points, int size_values, EdfCropParams* params);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    edfCropParamsWrap                                                                                             //
//      Wraps the EdfCropParams structure instance over the arrays containing points and values data. The structure //
//      EdfCropParams must be initialized on the user's side, this function only allocates the underlying data.     //
//      WARNING: Since the wrapped data are allocated by user, do not use the freeEdfCropParams() to deallocate.    //
//                                                                                                                  //
//      input:          size_points - number of points to wrap                                                      //
//                      rows        - pointer to the array containing the Y points coordinates                      //
//                      cols        - pointer to the array containing the X points coordinates                      //
//                      size_values - number of values to wrap                                                      //
//                      values_data - pointer to the array containing values data                                   //
//      output:         params      - pointer to the EdfCropParams instance to wrap over the data                   //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
ER_FUNCTION_PREFIX void edfCropParamsWrap(int size_points, double* rows, double* cols,
                                           int size_values, double* values_data, EdfCropParams* params);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    edfCropParamsFree                                                                                             //
//      Deletes the points and values data in the EdfCropParams structure instance. The structure EdfCropParams     //
//      must be freed on the user's side, this function only deallocates the underlying data.                       //
//                                                                                                                  //
//      input:          params - pointer to the EdfCropParams instance to deallocate the data in                    //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
ER_FUNCTION_PREFIX void edfCropParamsFree(EdfCropParams* params);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    edfCropImage                                                                                                  //
//      Crop and align input image for edfComputeDesc                                                               //
//                                                                                                                  //
//      input:          image_in      - pointer to a input image                                                    //
//                      params        - parameters for the input image alignment                                    //
//                                      content is module dependent (see the manual)                                //
//                      module_state  - pointer to the module state                                                 //
//                      config        - image cropping configuration (can be NULL)                                  //
//      output:         cropped_image - registered and aligned image for edfComputeDesc                             //
//                                                                                                                  //
//      return value:   0 on success, error code on failure                                                         //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
ER_FUNCTION_PREFIX int edfCropImage(const ERImage* image_in, EdfCropParams* params, void* module_state,
                                     ERImage* cropped_image, EdfCropImageConfig* config);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    edfFreeCropImage                                                                                              //
//      Free image cropped using edfCropImage.                                                                      //
//                                                                                                                  //
//      input:         module_state  - pointer to the module state                                                  //
//                     cropped_image  - pointer to a EdfImage structure instance created by edfCropImage            //
//                                                                                                                  //
//      return value:   0 on success, error code on failure                                                         //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
ER_FUNCTION_PREFIX int edfFreeCropImage(void* module_state, ERImage* cropped_image);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    edfModelVersion                                                                                               //
//      Return version of the model initialized. Used to verify descriptor version to model version.                //
//                                                                                                                  //
//      input:          module_state - pointer to the module state                                                  //
//                                                                                                                  //
//      return value:   0 on failure, model version on success                                                      //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
ER_FUNCTION_PREFIX unsigned int edfModelVersion(const void* module_state);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    edfClassify                                                                                                   //
//      Classifies the descriptor using the classifiers loaded from the model and returns                           //
//      the classification result.                                                                                  //
//                                                                                                                  //
//      input:          descriptor      - pointer to a EdfDescriptor structure                                      //
//                      module_state    - pointer to the module state                                               //
//                      config          - classification configuration (can be NULL)                                //
//      output:         classify_result - double pointer to the EdfClassifyResult structure                         //
//                                        containing the result of the classification                               //
//                                                                                                                  //
//      return value:   0 on success, error code on failure                                                         //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
ER_FUNCTION_PREFIX int edfClassify(const EdfDescriptor* desc, void* module_state,
                                    EdfClassifyResult** classify_result, EdfClassifyConfig* config);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    edfFreeClassifyResult                                                                                         //
//      Free classification result created by edfClassify.                                                          //
//                                                                                                                  //
//      input:          classify_result - double pointer to the EdfClassifyResult structure created by edfClassify  //
//                      module_state    - pointer to the module state                                               //
//                                                                                                                  //
//      return value:   0 on success, error code on failure                                                         //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
ER_FUNCTION_PREFIX int edfFreeClassifyResult(EdfClassifyResult** classify_result, void* module_state);

#endif  // EYEDEA_EDF_H
