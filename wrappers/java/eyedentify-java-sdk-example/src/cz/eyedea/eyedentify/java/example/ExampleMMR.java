////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2016-2023 by Eyedea Recognition, s.r.o.                              //
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

package cz.eyedea.eyedentify.java.example;

import java.io.IOException;

import cz.eyedea.eyedentify.java.EdfJavaSDK;
import cz.eyedea.eyedentify.java.structures.EdfClassifyResult;
import cz.eyedea.eyedentify.java.structures.EdfCropParams;
import cz.eyedea.eyedentify.java.structures.EdfDescriptor;
import cz.eyedea.eyedentify.java.structures.EdfException.EdfInitException;
import cz.eyedea.eyedentify.java.structures.EdfException.EdfPointsInitException;
import cz.eyedea.eyedentify.java.structures.EdfException.NoCropParamsException;
import cz.eyedea.eyedentify.java.structures.EdfInitConfig;
import cz.eyedea.er.java.structures.ERException.NoImageException;
import cz.eyedea.er.java.structures.ERException.UnsupportedImageColorModelException;
import cz.eyedea.er.java.structures.ERImage;
import cz.eyedea.er.java.structures.ERComputationMode;

public class ExampleMMR {
	
	private static final String SDK_PACKAGE_ROOT    = "../../../";
	private static final String SDK_IMAGE_DATA_ROOT = SDK_PACKAGE_ROOT + "data/images-mmr/";
	private static final String SDK_MODULE_PATH     = SDK_PACKAGE_ROOT + "sdk/modules/edftf2lite/";
	private static final Boolean USE_CARBOX         = false;
	private static final String MODEL_NAME_LP       = "MMR_VCMMCT_FAST_2024Q2.dat";
	private static final String MODEL_NAME_CARBOX   = "MMRBOX_VCMMCT_FAST_2024Q2.dat";
	
	private static final String IMAGE_FILEPATH      = SDK_IMAGE_DATA_ROOT + "car_cz.jpg";

	private static final double[] COLS_LP   = { 475.0 };
	private static final double[] ROWS_LP   = { 573.0 };
	private static final double[] VALUES_LP = { 257.7, 2.0 };

	private static final double[] COLS_CARBOX   = { 282.0, 754.0 };
	private static final double[] ROWS_CARBOX   = { 142.0, 640.0 };
	private static final double[] VALUES_CARBOX = {  };


	public static void main(String[] args) {
		// Input image loading
		ERImage img = null;
		try {
			img = new ERImage(IMAGE_FILEPATH);
		} catch (NullPointerException | IllegalArgumentException | NoImageException
				| UnsupportedImageColorModelException | IOException e1) {
			System.err.println("Error loading image: " + IMAGE_FILEPATH);
			return;
		}

		// Eyedentify wrapper for MMR initialization
		EdfInitConfig initConfig = new EdfInitConfig(SDK_MODULE_PATH, USE_CARBOX ? MODEL_NAME_CARBOX : MODEL_NAME_LP,
		    ERComputationMode.ER_COMPUTATION_MODE_CPU);
		EdfJavaSDK edfJavaSDK = null;
		try {
			edfJavaSDK = new EdfJavaSDK(initConfig);
		} catch (EdfInitException | UnsatisfiedLinkError e) {
			String message = e.getMessage();
			System.err.println("EdfJavaSDK MMR error: " + message);
			return;
		}

		// Initialize the input image cropping parameters
		EdfCropParams cropParams = null;
		try {
			cropParams = new EdfCropParams(USE_CARBOX ? COLS_CARBOX : COLS_LP,
			                               USE_CARBOX ? ROWS_CARBOX : ROWS_LP,
			                               USE_CARBOX ? VALUES_CARBOX : VALUES_LP);
		} catch (EdfPointsInitException e) {
			System.err.println("Error creating cropping parameters.");
			return;
		}
		
		// Descriptor computation part
		EdfDescriptor descriptor = null;
		
		// Descriptor computation using ERImage and cropping params
		long startTime = System.currentTimeMillis();
		try {
			ERImage croppedImage = edfJavaSDK.edfCropImage(img, cropParams);
			descriptor = edfJavaSDK.edfComputeDesc(croppedImage);
		} catch (NoImageException | NoCropParamsException e) {
			System.err.println("Invalid input parameters for image cropping and descriptor computation.");
			return;
		}
		long elapsedTime = System.currentTimeMillis() - startTime;
		System.out.println("Descriptor computation (BufferedImage) finished in " + (elapsedTime/1000.0) + "s");
		System.out.println("Performance: " + (1/(elapsedTime/1000.0)) + " descriptors/s\n");
		
// 		// Batch descriptor computation - currently not enabled in edftf2lite module
// 		final int BATCH_SIZE = 16;
// 		try {
// 			ERImage croppedImage = edfJavaSDK.edfCropImage(img, cropParams);
// 			ERImage[] croppedImages = new ERImage[BATCH_SIZE];
// 			for (int i = 0; i < BATCH_SIZE; i++) {
// 				croppedImages[i] = croppedImage;
// 			}
// 			startTime = System.currentTimeMillis();
// 			@SuppressWarnings("unused")
// 			EdfDescriptor[] descriptors = edfJavaSDK.edfComputeDesc(croppedImages);
// 		} catch (NoImageException | NoCropParamsException e) {
// 			System.err.println("Invalid input parameters for image cropping and batch descriptor computation.");
// 			return;
// 		}
// 		elapsedTime = System.currentTimeMillis() - startTime;
// 		System.out.println("Batch descriptor computation (BufferedImage) finished in " + (elapsedTime/1000.0) + "s");
// 		System.out.println("Computation time of one descriptor in the batch " + (elapsedTime/1000.0/BATCH_SIZE) + "s");
// 		System.out.println("Performance: " + (1/(elapsedTime/1000.0/BATCH_SIZE)) + " descriptors/s\n");
// 		
		// Classify the MMR descriptor
		EdfClassifyResult classifyResult = edfJavaSDK.edfClassify(descriptor);
		System.out.println("\nClassification result:");
		System.out.print(classifyResult.toString());
		// Do not forget to call dispose() to free all underlying C/C++ structures
		edfJavaSDK.dispose();
	}

}
