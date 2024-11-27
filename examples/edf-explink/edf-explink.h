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
#pragma once

///////////////////////////////////////////////////////////
//                   EYEDENTIFY SDK                      //
//                 recognition library                   //
///////////////////////////////////////////////////////////
#include <edf.h>

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    linkEyedentify                                                                                                //
//      Explicitly links the Eyedentify shared library from the path sdk_path and loads all needed functions        //
//      to the EdfAPI structure instance.                                                                           //
//                                                                                                                  //
//      input:          sdk_path      - path to the Eyedentify SDK folder ([EYEDENTIFY_PACKAGE]/sdk/)               //
//      output:         edf_api_state - pointer to the Eyedentify shared library state                              //
//                                      (contains shared library handler and function pointers)                     //
//                                                                                                                  //
//      return value:   0 on success, error code on failure                                                         //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
int linkEyedentify(const char* sdk_path, EdfAPI* edf_api_state);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    unlinkEyedentify                                                                                              //
//      Unlinks the Eyedyntify library previously linked with the function linkEyedentify().                        //
//                                                                                                                  //
//      input:          edf_api_state - pointer to the Eyedentify shared library state                              //
//                                                                                                                  //
//      return value:   0 on success, error code on failure                                                         //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
int unlinkEyedentify(EdfAPI* edf_api_state);
