////////////////////////////////////////////////////////////////////////////////////////
//                                                                                    //
//    C# Eyedentify SDK MMR Example                                                   //
// ---------------------------------------------------------------------------------- //
//                                                                                    //
// Copyright (c) 2017-2023 by Eyedea Recognition, s.r.o.                              //
//                                                                                    //
// Author: Eyedea Recognition, s.r.o.                                                 //
//                                                                                    //
// Contact:                                                                           //
//           web: http://www.eyedea.cz                                                //
//           email: info@eyedea.cz                                                    //
//                                                                                    //
// BSD License                                                                        //
// -----------------------------------------------------------------------------------//
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
using Eyedea.Eyedentify;
using System.Diagnostics;
using Eyedea.er;

namespace Eyedentify {
    class ExampleMMR {
        private const string SDK_PATH    = "../../../sdk/";
        private const string LIB_PATH    = SDK_PATH + "lib/";
        private const string MODULE_PATH = SDK_PATH + "modules/edftf2lite/";
        private const bool USE_CARBOX = false;
        private const string MODEL_NAME_LP  = "MMR_VCMMCT_FAST_2024Q2.dat";
        private const string MODEL_NAME_CARBOX  = "MMRBOX_VCMMCT_FAST_2024Q2.dat";
        private const string IMAGE_FILE  = "../../../data/images-mmr/car_cz.jpg";
        // private const string IMAGE_FILE2 = "../../../data/images-mmr/car_cz3.png";
        // private const string IMAGE_FILE3 = "../../../data/images-mmr/car_cz4.jpg";

        public static int Main(string[] args) {
            //////////////////////////////////////////////////////////////
            // Eyedentify module initialization
            //////////////////////////////////////////////////////////////
            // Create the instance of the SDK library
            // wrapper and load the Eyedentify DLL.
            Eyedea.Eyedentify.EdfCsSDK eyedentifyCsSDK = new Eyedea.Eyedentify.EdfCsSDK(LIB_PATH);

            // Initialize the module itself using the prepared configuration.
            Console.WriteLine("Eyedentify MMR module initialization...");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            EdfInitConfig initConfig = new EdfInitConfig();
            initConfig.module_path      = MODULE_PATH;
            initConfig.model_file       = USE_CARBOX ? MODEL_NAME_CARBOX : MODEL_NAME_LP;
            initConfig.computation_mode = ERComputationMode.ER_COMPUTATION_MODE_CPU;
            initConfig.gpu_device_id    = 0;
            bool initSuccess = false;
            try {
                initSuccess = eyedentifyCsSDK.edfInitEyedentify(initConfig);
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine("Press a key to exit...");
                Console.ReadKey();
                return 1;
            }

            sw.Stop();
            Console.WriteLine("\tdone in {0} ms", sw.Elapsed.TotalMilliseconds);
            // Check whether the module was initialized correctly.
            if (!initSuccess) {
                Console.WriteLine("Error during Eyedentify module initialization!");
                Console.WriteLine("Press a key to exit...");
                Console.ReadKey();
                return 1;
            }
            // Print the used model version.
            System.Console.WriteLine("Eyedentify binary model version: " + eyedentifyCsSDK.edfModelVersion().ToString());

            //////////////////////////////////////////////////////////////
            // Input image file loading
            //////////////////////////////////////////////////////////////
            // Load the input image to ERImage
            ERImage erImage = eyedentifyCsSDK.erImageRead(IMAGE_FILE);

            //////////////////////////////////////////////////////////////
            // License plate position specification
            //////////////////////////////////////////////////////////////
            EdfCropParams cropParams = default;

            if (!USE_CARBOX)
            {
                cropParams = EdfCropParams.allocateEdfCropParams(1, 2);
                // Set license plate center in the input image.
                cropParams.points.cols[0] = 475.0;
                cropParams.points.rows[0] = 573.0;
                // Set license plate resolution in pixels per meter.
                // -> License plate has 134 pixels in the image and
                //    Czech LP is 0.52 m wide:    134/0.52 = 257.7
                cropParams.values.values[0] = 257.7;
                // Set license plate rotation compensation in degrees.
                cropParams.values.values[1] = 1.0;
            }
            else
            {
                cropParams = EdfCropParams.allocateEdfCropParams(2, 0);
                cropParams.points.cols[0] = 282.0;
                cropParams.points.rows[0] = 142.0;
                cropParams.points.cols[1] = 754.0;
                cropParams.points.rows[1] = 640.0;
            }

            //////////////////////////////////////////////////////////////
            // Cropping the input image
            //////////////////////////////////////////////////////////////
            // Create the image crop with respect to the license plate.
            ERImage croppedImage = eyedentifyCsSDK.edfCropImage(erImage, cropParams);
            // Free the image structure fields. The image is not needed anymore.
            eyedentifyCsSDK.erImageFree(ref erImage);

            //////////////////////////////////////////////////////////////
            // Descriptor computation
            //////////////////////////////////////////////////////////////
            Console.WriteLine("File: {0}", IMAGE_FILE);
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("Vehicle descriptor:");
            // Start the timer.
            sw.Restart();
            // Compute the descriptor. Crop image is used as an input,
            // the output is copied to the EdfDescriptor structure.
            EdfDescriptor descriptor = eyedentifyCsSDK.edfComputeDesc(croppedImage);
            sw.Stop();
            // Check the descriptor computation result.
            if (descriptor.getSize() != 0) {
                // Get the computation duration.
                Console.WriteLine("Descriptor size: {0}, Computed in: {1} ms", descriptor.getSize(), sw.Elapsed.TotalMilliseconds);
            } else {
                Console.WriteLine("Error during descriptor computation!");
            }
            // Free the crop image data. The crop is not needed anymore.
            eyedentifyCsSDK.edfFreeCropImage(ref croppedImage);

            //////////////////////////////////////////////////////////////
            // Classification
            //////////////////////////////////////////////////////////////
            Console.WriteLine("\nVehicle classification:");
            Console.WriteLine("Result:");
            // Run the classification. The result will be copied
            // to the newly created EdfClassifyResult structure.
            try {
                EdfClassifyResult classifyResult = eyedentifyCsSDK.edfClassify(descriptor);
                // Print the classification result to the console.
                Console.Write(classifyResult.ToString());
            } catch (EdfException) {
                Console.WriteLine("Error during vehicle classification!");
            }
            // Free the descriptor fields.
            eyedentifyCsSDK.edfFreeDesc(ref descriptor);

            //////////////////////////////////////////////////////////////
            // Batch descriptor computation
            // NOTE: This part is used only for demonstration how can be
            //       descriptors computed in a batch (one call of function
            //       edfComputeDescriptor()) and is not the part of
            //       the main example pipeline. The advantage is the speed
            //       of the processing where the batch processing can be
            //       faster than sequential on some system configurations.
            //////////////////////////////////////////////////////////////

// Batch mode disabled since version 2.9.0
//             Console.WriteLine("--------------------------------------------------");
//             Console.WriteLine("--------------------------------------------------");
//             Console.WriteLine("Descriptor batch computation");
//             Console.WriteLine("--------------------------------------------------");
//             erImage = eyedentifyCsSDK.erImageRead(IMAGE_FILE);
//             cropParams.points.cols[0]   = 475.0;
//             cropParams.points.rows[0]   = 573.0;
//             cropParams.values.values[0] = 257.7;
//             cropParams.values.values[1] =   1.0;
//             ERImage croppedImage1 = eyedentifyCsSDK.edfCropImage(erImage, cropParams);
//             eyedentifyCsSDK.erImageFree(ref erImage);
// 
//             erImage = eyedentifyCsSDK.erImageRead(IMAGE_FILE2);
//             cropParams.points.cols[0]   = 390.0;
//             cropParams.points.rows[0]   = 668.0;
//             cropParams.values.values[0] = 259.6;
//             cropParams.values.values[1] =  -8.0;
//             ERImage croppedImage2 = eyedentifyCsSDK.edfCropImage(erImage, cropParams);
//             eyedentifyCsSDK.erImageFree(ref erImage);
// 
//             erImage = eyedentifyCsSDK.erImageRead(IMAGE_FILE3);
//             cropParams.points.cols[0]   = 728.0;
//             cropParams.points.rows[0]   = 835.0;
//             cropParams.values.values[0] = 384.6;
//             cropParams.values.values[1] =   0.0;
//             ERImage croppedImage3 = eyedentifyCsSDK.edfCropImage(erImage, cropParams);
//             eyedentifyCsSDK.erImageFree(ref erImage);
// 
//             ERImage[] imageCrops = { croppedImage1, croppedImage2, croppedImage3 };
//             Console.WriteLine("Batch size: {0}", imageCrops.Length);
// 
//             int numComputations = 5;
//             Console.WriteLine("Number of computations: {0}", numComputations);
//             double elapsedTime = 0.0;
//             for (int n = 0; n < numComputations; n++) {
//                 sw.Restart();
//                 EdfDescriptor[] descriptors = eyedentifyCsSDK.edfComputeDesc(imageCrops);
//                 sw.Stop();
//                 for (int j = 0; j < descriptors.Length; j++) {
//                     eyedentifyCsSDK.edfFreeDesc(ref descriptors[j]);
//                 }
//                 elapsedTime += sw.Elapsed.TotalMilliseconds;
//             }
//             // Print the batch computation time.
//             double avgDescComputationTimeMS = elapsedTime / numComputations / imageCrops.Length;
//             Console.WriteLine("Computation time: {0} ms", elapsedTime);
//             Console.WriteLine("Average descriptor computation time: {0} ms", avgDescComputationTimeMS);
// 
//             eyedentifyCsSDK.edfFreeCropImage(ref croppedImage1);
//             eyedentifyCsSDK.edfFreeCropImage(ref croppedImage2);
//             eyedentifyCsSDK.edfFreeCropImage(ref croppedImage3);

            //////////////////////////////////////////////////////////////
            // Cleaning up
            //////////////////////////////////////////////////////////////
            // Free the module. All module internal structures
            // will be deleted and program can be finished.
            eyedentifyCsSDK.edfFreeEyedentify();

            // Wait for user input.
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("Press a key to exit...");
            Console.ReadKey();

            return 0;
        }
    }
}
