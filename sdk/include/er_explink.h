/* **************************************************************** */
/* **************************************************************** */
/* *                                                              * */
/* *    Copyright (c) 2015-2018 by Eyedea Recognition, s.r.o.     * */
/* *                  ALL RIGHTS RESERVED.                        * */
/* *                                                              * */
/* *    Author:  Eyedea Recognition, s.r.o.                       * */
/* *                                                              * */
/* *    Contact:                                                  * */
/* *             web: http://www.eyedea.cz                        * */
/* *             email: info@eyedea.cz                            * */
/* *                                                              * */
/* **************************************************************** */
/* **************************************************************** */

#ifndef EYEDEA_ER_EXPLINK_H
#define EYEDEA_ER_EXPLINK_H

//////////////////////////////////////////////////////////////////////
//                                                                  //
// EXPLICIT LINKING                                                 //
// Explicit linking is a type of linkage, which links the library   //
// to the program. Explicit linking links at runtime, as oppose     //
// to traditional linkage, which links at build time.               //
// This is required by the DRM used (gemalto's Sentinel LDK).       //
// See the Developer's Guide and examples to learn more.            //
// We provide multiplatform defines to unify linkage in Windows,    //
// Linux and macOS.                                                 //
//////////////////////////////////////////////////////////////////////

#if _WIN32 || _WIN64 /* WINDOWS Platform */
/* Standard includes */
#   include <windows.h>
#   include <direct.h>
#   include <io.h>
#   include <stdlib.h>

/* function prefix*/
#ifdef __cplusplus
# define ER_FUNCTION_PREFIX extern "C" __declspec(dllexport)
#else
# define ER_FUNCTION_PREFIX __declspec(dllexport)
#endif

/* last error string of linking procedure */
#ifdef UNICODE
static wchar_t __lastErrorMsg__[256];
static inline const wchar_t* __getLastErrorMsg() {
#else
static char __lastErrorMsg__[256];
static inline const char* __getLastErrorMsg() {
#endif
    FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, NULL, GetLastError(), MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), __lastErrorMsg__, 256, NULL);
    return __lastErrorMsg__;
}
#   define ER_SHLIB_LASTERROR __getLastErrorMsg()
/* explicit linking types and  macros*/
typedef HMODULE shlib_hnd; /* type definition of shared lib. handle */
                           /* Shared lib. open and function load routines. */
#ifdef UNICODE
#   define ER_SHLIB_OPEN_ERROR(shlib_filename) fwprintf(stderr, L"Loading %ws library failed: %ws\n", shlib_filename, ER_SHLIB_LASTERROR)
#   define ER_SHFCN_LOAD_ERROR(shfcn_name)     fwprintf(stderr, L"Loading %hs function failed: %ws\n", shfcn_name, ER_SHLIB_LASTERROR)
#else
#   define ER_SHLIB_OPEN_ERROR(shlib_filename) fprintf(stderr, "Loading %s library failed: %s\n", shlib_filename, ER_SHLIB_LASTERROR)
#   define ER_SHFCN_LOAD_ERROR(shfcn_name)     fprintf(stderr, "Loading %s function failed: %s\n", shfcn_name, ER_SHLIB_LASTERROR)
#endif
#   define ER_OPEN_SHLIB(shlib_hnd, shlib_filename) if ( !(shlib_hnd = LoadLibrary(shlib_filename)) ){ ER_SHLIB_OPEN_ERROR(shlib_filename); }
#   define ER_LOAD_SHFCN(shfcn_ptr, shfcn_type, shlib_hnd, shfcn_name) if ( (shfcn_ptr = (shfcn_type) GetProcAddress(shlib_hnd, shfcn_name)) == 0 ){ ER_SHFCN_LOAD_ERROR(shfcn_name); }
#   define ER_FREE_LIB(shlib_hnd) FreeLibrary(shlib_hnd)
#   define ER_LIB_PREFIX 
#   define ER_LIB_EXT ".dll"
#   if _WIN64
#       define ER_LIB_TARGET "x64"
#   else
#       define ER_LIB_TARGET "Win32"
#   endif

#elif __linux__ || __unix /* LINUX/UNIX Platforms */
/* Standard includes */
#   include <dirent.h>
#   include <dlfcn.h>
#   include <sys/stat.h>

#ifdef __ANDROID__
#include <android/log.h>
#endif

/* function prefix */
#ifdef __cplusplus
# define ER_FUNCTION_PREFIX extern "C" __attribute__ ((visibility ("default"))) 
#else
# define ER_FUNCTION_PREFIX __attribute__ ((visibility ("default"))) 
#endif

/* explicit linking types and macros */
typedef void* shlib_hnd; /* type definition of shared lib. handle */
                         /* Shared lib. open and function load routines. */
#ifdef __ANDROID__
#   define ER_OPEN_SHLIB(shlib_hnd, shlib_filename) if ( (shlib_hnd = dlopen(shlib_filename,RTLD_LAZY))==NULL ){ __android_log_print(ANDROID_LOG_WARN, "ER_OPEN_SHLIB","%s", ER_SHLIB_LASTERROR); }
#   define ER_LOAD_SHFCN(shfcn_ptr, shfcn_type, shlib_hnd, shfcn_name) if ( (shfcn_ptr = (shfcn_type) dlsym(shlib_hnd, shfcn_name))==NULL ){ __android_log_print(ANDROID_LOG_WARN, "ER_LOAD_SHFCN", "%s", ER_SHLIB_LASTERROR); }
#else
#   define ER_OPEN_SHLIB(shlib_hnd, shlib_filename) if ( (shlib_hnd = dlopen(shlib_filename,RTLD_LAZY))==NULL ){ fprintf(stderr,"Loading %s library failed: %s\n",shlib_filename, ER_SHLIB_LASTERROR); }
#   define ER_LOAD_SHFCN(shfcn_ptr, shfcn_type, shlib_hnd, shfcn_name) if ( (shfcn_ptr = (shfcn_type) dlsym(shlib_hnd, shfcn_name))==NULL ){ fprintf(stderr,"Loading %s function failed: %s\n",shfcn_name, ER_SHLIB_LASTERROR); }
#endif
#   define ER_FREE_LIB(shlib_hnd) dlclose(shlib_hnd)
#   define ER_LIB_PREFIX "lib"
#   define ER_LIB_EXT ".so"
#   if defined(__x86_64__) || defined(_M_X64) || defined(__ppc64__)
#       define ER_LIB_TARGET "x86_64"
#   elif defined(__i386__)
#       define ER_LIB_TARGET "i686"
#   elif defined(__arm__) && (__ARM_ARCH == 7 || defined(__ARM_ARCH_7A__))
#       define ER_LIB_TARGET "armv7l"
#   elif defined(__arm__) && (__ARM_ARCH == 8 || defined(__ARM_ARCH_8__))
#       define ER_LIB_TARGET "armv7l"
#   elif defined(__aarch64__)
#       define ER_LIB_TARGET "aarch64"
#   else 
#       error Unkown architecture
#   endif
#   define ER_SHLIB_LASTERROR dlerror()
#else
# error Unkown platform
#endif /* Multiplatform defines */
#endif
