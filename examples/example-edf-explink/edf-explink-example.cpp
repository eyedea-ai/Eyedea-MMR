///////////////////////////////////////////////////////////
//                                                       //
// Copyright (c) 2016-2020 by Eyedea Recognition, s.r.o. //
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

#include "edf.h"
#include "edf-explink.h"
#include "edf-info.h"

#include <string>
#include <iostream>

#define PATH_TO_EDF_SDK "../../sdk/"

int main(int argc, char * argv[]) {
	EdfAPI edfAPI;
    // Link Eyedentify shared library and all its functions
	int linkCode = linkEyedentify(PATH_TO_EDF_SDK, &edfAPI);
	if (linkCode != 0) {
		EDF_LOG_ERR("Error during Eyedentify shared library linking!");
        std::cout << "\nPress a key to exit..." << std::endl;
        std::cin.get();
		return 1;
	}

    /////////////////////////////////////////////////////
	// Instead of calling Eyedentify library API       //
    // functions directly, call them  using the EdfAPI //
    // structure instance.                             //
    // Example:                                        //
    //     To call edfInitEyedentify(...) function use //
    //     the EdfAPI instance:                        //
    //          edf_api_state.edfInitEyedentify(...)   //
    /////////////////////////////////////////////////////
	// Initialize the pointer to the module.
	void* module_state = NULL;
	// Initialize the module itself using the prepared configuration.
	std::cout << "Eyedentify module initialization..." << std::endl;
	EdfInitConfig initConfig;
	std::string modulePath      = std::string(PATH_TO_EDF_SDK) + "modules/" + std::string(EDF_MODULE_NAME) + "/";
	initConfig.module_path      = modulePath.c_str();
	initConfig.model_file       = EDF_MODEL_NAME;
	initConfig.computation_mode = ER_COMPUTATION_MODE_CPU;
	initConfig.gpu_device_id    = 0;
	int initCode = edfAPI.edfInitEyedentify(&initConfig, &module_state);
	// Check whether the module was initialized correctly.
	if (initCode != 0) {
		EDF_LOG_ERR("Error during Eyedentify module initialization!");
        std::cout << "\nPress a key to exit..." << std::endl;
        std::cin.get();
		return 2;
	}
	// Print the used model version.
	std::cout << "Loaded binary model version: " << edfAPI.edfModelVersion(module_state) << std::endl;

    // Free the module. All module internal structures
    // will be deleted and program can be finished.
    edfAPI.edfFreeEyedentify(&module_state);
    /////////////////////////////////////////////////////

    // Unlink Eyedentify shared library
	int unlinkCode = unlinkEyedentify(&edfAPI);
	if (unlinkCode != 0) {
		EDF_LOG_ERR("Error during Eyedentify shared library unlinking!");
        std::cout << "\nPress a key to exit..." << std::endl;
        std::cin.get();
		return 3;
	}

    // Wait for user input.
    std::cout << "\nPress a key to exit..." << std::endl;
    std::cin.get();

	return 0;
}
