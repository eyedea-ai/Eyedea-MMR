///////////////////////////////////////////////////////////
//                                                       //
// Copyright (c) 2016-2021 by Eyedea Recognition, s.r.o. //
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

#include "edf-explink.h"

#include <string>
#include <string.h>
#include <algorithm>

#define EDF_LOAD_SHLIB_FCN_BODY(fcn_ptr, fcn_type, fcn_name, state, err_code)\
    ER_LOAD_SHFCN(state->fcn_ptr, fcn_type, state->shlib_handle, fcn_name); \
    if (!state->fcn_ptr) {                                                   \
        ER_FREE_LIB(state->shlib_handle);                                   \
        const char* msg_func = fcn_name;                                     \
        printf("ERROR - Eyedentify: function %s() not linked\n", msg_func);  \
        return err_code;                                                     \
    }                                                                        \

int linkEyedentify(const char* sdk_path, EdfAPI* edf_api_state) {
    if (!sdk_path || !edf_api_state) {
        return -1;
    }
    // Construct path to the shared library to link
    std::string sdkPath(sdk_path);
    std::replace(sdkPath.begin(), sdkPath.end(), '\\', '/');
    if (sdkPath.at(sdkPath.length()-1) != '/') {
        sdkPath += "/";
    }
    std::string shlibPath = sdkPath + "lib/" + std::string(EDF_SHLIB_NAME);
#if defined(UNICODE) && (defined(WIN32) || defined(_WIN32) || defined(_WINDOWS_))
    std::wstring shlibPathW = std::wstring(shlibPath.begin(), shlibPath.end());
    const wchar_t* shlib_path = shlibPathW.c_str();
#else
    const char* shlib_path = shlibPath.c_str();
#endif

    // Zero the input EdfAPI structure instance.
    memset(edf_api_state, 0, sizeof(EdfAPI));

    // Test if library file exists
    FILE * file_id;
    EDF_FILE_OPEN(file_id, shlib_path);
    if (!file_id) {
        EDF_LOG_ERR("Library file " + shlibPath + " does not exist...");
        return -2;
    }
    fclose(file_id);

    // Link library
    shlib_hnd shlib_handle = NULL;
    ER_OPEN_SHLIB(shlib_handle, shlib_path);
    if (!shlib_handle) {
        EDF_LOG_ERR("Library file " + shlibPath + " could not be loaded...");
        return -3;
    }

    // Set the shared library handle
    edf_api_state->shlib_handle = shlib_handle;

    // Link library functions
    // Eyedentify API main functions
    EDF_LOAD_SHLIB_FCN_BODY(edfInitEyedentify              , fcn_edfInitEyedentify              , "edfInitEyedentify"              , edf_api_state,  -4);
    EDF_LOAD_SHLIB_FCN_BODY(edfFreeEyedentify              , fcn_edfFreeEyedentify              , "edfFreeEyedentify"              , edf_api_state,  -5);
    EDF_LOAD_SHLIB_FCN_BODY(edfComputeDesc                 , fcn_edfComputeDesc                 , "edfComputeDesc"                 , edf_api_state,  -6);
    EDF_LOAD_SHLIB_FCN_BODY(edfCompareDescs                , fcn_edfCompareDescs                , "edfCompareDescs"                , edf_api_state,  -7);
    EDF_LOAD_SHLIB_FCN_BODY(edfAllocDesc                   , fcn_edfAllocDesc                   , "edfAllocDesc"                   , edf_api_state,  -8);
    EDF_LOAD_SHLIB_FCN_BODY(edfFreeDesc                    , fcn_edfFreeDesc                    , "edfFreeDesc"                    , edf_api_state,  -9);
    EDF_LOAD_SHLIB_FCN_BODY(edfCropParamsAllocate          , fcn_edfCropParamsAllocate          , "edfCropParamsAllocate"          , edf_api_state, -10);
    EDF_LOAD_SHLIB_FCN_BODY(edfCropParamsWrap              , fcn_edfCropParamsWrap              , "edfCropParamsWrap"              , edf_api_state, -11);
    EDF_LOAD_SHLIB_FCN_BODY(edfCropParamsFree              , fcn_edfCropParamsFree              , "edfCropParamsFree"              , edf_api_state, -12);
    EDF_LOAD_SHLIB_FCN_BODY(edfCropImage                   , fcn_edfCropImage                   , "edfCropImage"                   , edf_api_state, -13);
    EDF_LOAD_SHLIB_FCN_BODY(edfFreeCropImage               , fcn_edfFreeCropImage               , "edfFreeCropImage"               , edf_api_state, -14);
    EDF_LOAD_SHLIB_FCN_BODY(edfModelVersion                , fcn_edfModelVersion                , "edfModelVersion"                , edf_api_state, -15);
    EDF_LOAD_SHLIB_FCN_BODY(edfClassify                    , fcn_edfClassify                    , "edfClassify"                    , edf_api_state, -16);
    EDF_LOAD_SHLIB_FCN_BODY(edfFreeClassifyResult          , fcn_edfFreeClassifyResult          , "edfFreeClassifyResult"          , edf_api_state, -17);
    // ERImage API utils functions
    EDF_LOAD_SHLIB_FCN_BODY(erImageGetDataTypeSize         , fcn_erImageGetDataTypeSize         , "erImageGetDataTypeSize"         , edf_api_state, -18);
    EDF_LOAD_SHLIB_FCN_BODY(erImageGetColorModelNumChannels, fcn_erImageGetColorModelNumChannels, "erImageGetColorModelNumChannels", edf_api_state, -19);
    EDF_LOAD_SHLIB_FCN_BODY(erImageGetPixelDepth           , fcn_erImageGetPixelDepth           , "erImageAllocateBlank"           , edf_api_state, -20);
    EDF_LOAD_SHLIB_FCN_BODY(erImageAllocateBlank           , fcn_erImageAllocateBlank           , "erImageAllocateBlank"           , edf_api_state, -21);
    EDF_LOAD_SHLIB_FCN_BODY(erImageAllocate                , fcn_erImageAllocate                , "erImageAllocate"                , edf_api_state, -22);
    EDF_LOAD_SHLIB_FCN_BODY(erImageAllocateAndWrap         , fcn_erImageAllocateAndWrap         , "erImageAllocateAndWrap"         , edf_api_state, -23);
    EDF_LOAD_SHLIB_FCN_BODY(erImageCopy                    , fcn_erImageCopy                    , "erImageCopy"                    , edf_api_state, -24);
    EDF_LOAD_SHLIB_FCN_BODY(erImageRead                    , fcn_erImageRead                    , "erImageRead"                    , edf_api_state, -25);
    EDF_LOAD_SHLIB_FCN_BODY(erImageWrite                   , fcn_erImageWrite                   , "erImageWrite"                   , edf_api_state, -26);
    EDF_LOAD_SHLIB_FCN_BODY(erImageFree                    , fcn_erImageFree                    , "erImageFree"                    , edf_api_state, -27);
    EDF_LOAD_SHLIB_FCN_BODY(erVersion                      , fcn_erVersion                      , "erVersion"                      , edf_api_state, -28);

    return 0;
}

int unlinkEyedentify(EdfAPI* edf_api_state) {
    if (edf_api_state) {
        // Unlink the library
        ER_FREE_LIB(edf_api_state->shlib_handle);
        memset(edf_api_state, 0, sizeof(EdfAPI));

        return 0;
    }

    return -1;
}
