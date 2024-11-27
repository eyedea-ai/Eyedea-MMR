///////////////////////////////////////////////////////////
//                                                       //
// Copyright (c) 2023 by Eyedea Recognition, s.r.o.      //
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
//                   EYEDEA MMR SDK                      //
//             recognition library example               //
///////////////////////////////////////////////////////////

// Eyedea MMR include - include path to sdk/include must be added
#include <edf.h>
#include <edf_type_mmr.h>

#include <chrono>  // time measure
#include <fstream> // raw image reading
#include <cstring>


////////////////////////////////////////////////////////////////////////////////
// CONSTANTS - SDK PATH, MODULE NAME                                          //
////////////////////////////////////////////////////////////////////////////////
const char *EDF_SDK_PATH    = "../../sdk/";
const char *EDF_MODULE_NAME = "edftf2lite"; // module name depends on the type and version, do not change


////////////////////////////////////////////////////////////////////////////////
// CONSTANTS - MODELS AND OPTION'S DEFAULT VALUES                             //
////////////////////////////////////////////////////////////////////////////////
enum MMRTask { VCMMGVCT=0, VCMMCT=1, VCMCT=2, VCCT=3 }; //< helper enum for indexing models
enum MMRType { LP = 0, CARBOX = 1 };  // << helper enum for indexing crop type (license plate-based or carbox-based)
const char *MMR_FAST_MODELS[] = {"MMR_VCMMGVCT_FAST_2024Q2.dat",    /* LP: category - make - model - generation - variation */
                                 "MMRBOX_VCMMGVCT_FAST_2024Q2.dat", /* CARBOX: category - make - model - generation - variation */
                                 "MMR_VCMMCT_FAST_2024Q2.dat",      /* LP: category - make - model */
                                 "MMRBOX_VCMMCT_FAST_2024Q2.dat",   /* CARBOX: category - make - model */
                                 "MMR_VCMCT_FAST_2024Q2.dat",       /* LP: category - make */
                                 "MMRBOX_VCMCT_FAST_2024Q2.dat",    /* CARBOX: category - make */
                                 "MMR_VCCT_FAST_2024Q2.dat",        /* LP: category */
                                 "MMRBOX_VCCT_FAST_2024Q2.dat"};    /* CARBOX: category */

const char *MMR_PREC_MODELS[] = {"MMR_VCMMGVCT_PREC_2024Q2.dat",
                                 "MMRBOX_VCMMGVCT_PREC_2024Q2.dat",
                                 "MMR_VCMMCT_PREC_2024Q2.dat",
                                 "MMRBOX_VCMMCT_PREC_2024Q2.dat",
                                 "MMR_VCMCT_PREC_2024Q2.dat",
                                 "MMRBOX_VCMCT_PREC_2024Q2.dat",
                                 "MMR_VCCT_PREC_2024Q2.dat",
                                 "MMRBOX_VCCT_PREC_2024Q2.dat" };

const bool               DEFAULT_FAST_VERSION = true;       // run fast model
const MMRType            DEFAULT_MMR_TYPE = MMRType::LP;               // default type LP
const MMRTask            DEFAULT_MMR_TASK = MMRTask::VCMMCT;           // default task VCMMCT
const ERComputationMode  DEFAULT_COMPUTATION_MODE = ER_COMPUTATION_MODE_CPU; // default CPU computation mode
const int                DEFAULT_GPU_ID = 0;                // first gpu device
const int                DEFAULT_NUM_THREADS = 1;           // use 1 thread for computation
const char*              DEFAULT_ONNX_PROVIDER = "cpu";     // cpu as default provider, set "cuda","tensorrt","rocm" for gpu

////////////////////////////////////////////////////////////////////////////////
// INPUT IMAGES AND ANNOTATIONS                                               //
////////////////////////////////////////////////////////////////////////////////

/** Input image structure
 */
struct InputImage {
    std::string imageFilename;//!<  Absolute or relative path from example to the image file
    bool isImageRawYCbCr420;  //!< Flag if stored image is raw YCbCr420 coded
    int rawYCbCrWidth;        //!< Width of the raw YCbCr420 coded image.
    int rawYCbCrHeight;       //!< Height of the raw YCbCr420 coded image.
    InputImage() : imageFilename(""), isImageRawYCbCr420(false), rawYCbCrWidth(0), rawYCbCrHeight(0) {}
    InputImage(std::string filename, bool isRaw, int rawW=0, int rawH=0) : imageFilename(filename), isImageRawYCbCr420(isRaw), rawYCbCrWidth(rawW), rawYCbCrHeight(rawH) {}
};
/** Input position of LP for MMR recognition, the licence plate center, rotation and pixel per meter resolution of the LP
 */
struct InputPositionLP {
    float center_x;       //!< x (column) coordinate of the LP's center in pixels; [0,0] is the top left corner of the image
    float center_y;       //!< y (row) coordinate of the LP's center in pixels; [0,0] is the top left corner of the image
    float resolution_ppm; //!< resolution of the licence plate (width of the LP in image [pxl] / real width of the LP [m] )
    float rotation_dgr;   //!< rotation of the LP counter clockwise
    InputPositionLP() : center_x(0.f), center_y(0.f), resolution_ppm(0.f), rotation_dgr(0.f) {}
    InputPositionLP(float cx, float cy, float res, float rot) : center_x(cx), center_y(cy), resolution_ppm(res), rotation_dgr(rot) {}
};

/** Input position of CARBOX for MMR recognition, the top left and bottom right corners of the carbox
 */
struct InputPositionCARBOX {
    float top_left_x;       //!< x (column) coordinate of the CARBOX's top left corner in pixels; [0,0] is the top left corner of the image
    float top_left_y;       //!< y (row) coordinate of the CARBOX's top left corner in pixels; [0,0] is the top left corner of the image
    float bottom_right_x;       //!< x (column) coordinate of the CARBOX's bottom right corner in pixels; [0,0] is the top left corner of the image
    float bottom_right_y;       //!< y (row) coordinate of the CARBOX's bottom right corner in pixels; [0,0] is the top left corner of the image
    InputPositionCARBOX() : top_left_x(0.f), top_left_y(0.f), bottom_right_x(0.f), bottom_right_y(0.f) {}
    InputPositionCARBOX(float top_left_x, float top_left_y, float bottom_right_x, float bottom_right_y) : top_left_x(top_left_x), 
        top_left_y(top_left_y), bottom_right_x(bottom_right_x), bottom_right_y(bottom_right_y) {}
};

/** Input data structure - image and licence plate position definition
 */
struct InputData {
    InputImage image;
    InputPositionLP lp;
    InputPositionCARBOX carbox;
    InputData(InputImage im, InputPositionLP lp, InputPositionCARBOX carbox): image(im), lp(lp), carbox(carbox){}
};

/** example input data */
/* NOTE: car_cz.jpg - resolution_ppm: License plate has 134 pixels in the image and Czech LP is 0.52 m wide:  134/0.52 = 257.7 */
/*                    {imageFilename, isImageRawYCbCr420, rawYCbCrWidth, rawYCbCrHeight}          {center_x, center_y, resolution_ppm, rotation_dgr} */
InputData inputs[] = {
                      {
                        InputImage{"../../data/images-mmr/car_cz.jpg",       false },
                        InputPositionLP{475.0f, 573.0f, 257.7f, 1.0f},
                        InputPositionCARBOX{282.0f, 142.0f, 754.0f, 640.0f}
                      },
                      { 
                        InputImage{"../../data/images-mmr/car_cz2.raw",      true, 1400, 1050 },
                        InputPositionLP{714.0f, 775.0f, 265.7f, 1.0f},
                        InputPositionCARBOX{478.0f, 218.0f, 943.0f, 850.0f},
                      },
                      { 
                        InputImage{"../../data/images-mmr/car_cz3.png",      false },
                        InputPositionLP{390.0f, 668.0f, 259.6f,-8.0f},
                        InputPositionCARBOX{167.0f, 131.0f, 746.0f, 758.0f},
                      },
                      { 
                        InputImage{"../../data/images-mmr/car_cz4.jpg",      false },
                        InputPositionLP{728.0f, 835.0f, 384.6f, 0.0f},
                        InputPositionCARBOX{399.0f, 119.0f, 1057.0f, 917.0f},
                      },
                      { 
                        InputImage{"../../data/images-mmr/car_it.jpg",       false },
                        InputPositionLP{515.0f, 810.0f, 344.4f, 1.0f},
                        InputPositionCARBOX{223.0f, 47.0f, 1161.0f, 931.0f},
                      },
                      { 
                        InputImage{"../../data/images-mmr/car_cz_rear.jpg",  false },
                        InputPositionLP{616.0f, 488.0f, 286.5f, 1.0f},
                        InputPositionCARBOX{245.0f, 141.0f, 868.0f, 630.0f}
                      },
                      { 
                        InputImage{"../../data/images-mmr/car_cz2_rear.jpg", false },
                        InputPositionLP{286.0f, 520.0f, 160.7f, 0.0f},
                        InputPositionCARBOX{196.0f, 221.0f, 370.0f, 627.0f}
                      },
                    };
size_t num_inputs = sizeof(inputs)/sizeof(InputData);

////////////////////////////////////////////////////////////////////////////////
// HELPER FUNCTIONS DECLARATIONS, DEFINITIONS ARE AT THE END OF THE FILE      //
////////////////////////////////////////////////////////////////////////////////
int parse_arguments(int argc, char * argv[], bool &help, bool &fast_version, MMRType& mmr_type, MMRTask &mmr_task, ERComputationMode &computation_mode, int &gpu_id, int &num_threads, std::string &onnx_provider);
ERImage loadImage(EdfAPI &api, const InputImage &input_image); //< load image from file to ERImage structure
EdfInitConfig setEdfInitConfig(const char *path, const char *name, ERComputationMode mode, int gpu_device_id,
                               int num_threads, const std::string & onnx_provider); //< Set up init configuration structure
EdfCropParams setEdfCropParamsByLP(EdfAPI &api, const InputPositionLP &pos); //< Set up crop parameters using LP
EdfCropParams setEdfCropParamsByCARBOX(EdfAPI& api, const InputPositionCARBOX& pos); //< Set up crop parameters using CARBOX
void printResults(const EdfClassifyResult &result_mmr); //< read results from EdfClassifyResult structure and print on the screen

// macros for time measure
std::chrono::steady_clock::time_point start,stop;
#define TIC start = std::chrono::steady_clock::now();
#define TOC(msg) stop  = std::chrono::steady_clock::now(); \
    std::cout << msg << (double)std::chrono::duration_cast<std::chrono::microseconds>(stop - start).count()/1000. << " ms" << std::endl;

//macro with cleanup routines on error
#define EARLY_EXIT edfAPI.erImageFree(&image); \
                   edfAPI.edfFreeEyedentify(&mmr_state); \
                   std::cout << "Press a key to exit..." << std::endl; \
                   std::cin.get(); \
                   return -1;

///////////////////////////////////////////////////////////////////////////////////////
// Eyedea MMR module example                                                         //
///////////////////////////////////////////////////////////////////////////////////////
//   This example demonstrates the basic usage of the Eyedea MMR module.             //
//       1) It starts with Eyedentify library loading,                               //
//       2) Eyedentify module initialization and then goes through                   //
//       3) input image file loading,                                                //
//       4) cropping the input image,                                                //
//       5) descriptor computation,                                                  //
//       6) classification                                                           //
//       7) cleaning up at the end.                                                  //
///////////////////////////////////////////////////////////////////////////////////////
int main(int argc, char * argv[]) {
    bool option_fast_version = DEFAULT_FAST_VERSION;
    MMRType option_mmr_type = DEFAULT_MMR_TYPE;
    MMRTask option_mmr_task = DEFAULT_MMR_TASK;
    ERComputationMode option_computation_mode = DEFAULT_COMPUTATION_MODE;
    int option_gpu_id = DEFAULT_GPU_ID;
    int option_num_threads = DEFAULT_NUM_THREADS;
    std::string option_onnx_provider{DEFAULT_ONNX_PROVIDER};
    bool help = false;
    ////////////////////////////////////////////////////////////////
    //  Reading input parameters                                  //
    ////////////////////////////////////////////////////////////////
    if ( parse_arguments(argc, argv, help, option_fast_version, option_mmr_type, option_mmr_task, option_computation_mode, 
            option_gpu_id, option_num_threads, option_onnx_provider) !=0 ){
        std::cerr << "Argument parsing failed!\n";
        return -1;
    }
    if (help) return 0;

    //////////////////////////////////////////////////////////////
    // Eyedentify library loading
    //////////////////////////////////////////////////////////////
    // Create the instance of the EdfAPI structure 
    // which allows to call explicitly linked Eyedentify library
    EdfAPI edfAPI;
#ifdef EXPLICIT_LINKING
    // explicitly link library
    std::string edf_lib_path = std::string(EDF_SDK_PATH) + "lib/" + std::string(EDF_SHLIB_NAME);
    shlib_hnd hdll = nullptr;
    ER_OPEN_SHLIB(hdll, edf_lib_path.c_str());
    if (hdll==nullptr) {
        std::cerr << "Library '" << edf_lib_path << "' not loaded!\n" << ER_SHLIB_LASTERROR << "\n";
        return -1;
    }
    fcn_edfLinkAPI pfLinkAPI=nullptr;     /* The function which will link all other api functions */
    ER_LOAD_SHFCN(pfLinkAPI, fcn_edfLinkAPI, hdll, "edfLinkAPI");
    if (pfLinkAPI==nullptr) {
        std::cerr << "Loading function 'esLinkAPI' from " << edf_lib_path << " failed!\n";
        return -1;
    }
    if ( pfLinkAPI(hdll, &edfAPI) != 0 ){
        std::cerr << "Function edfLinkAPI() returned with error!\n";
        return -1;
    }
#else
    // libeyedentify is linked implicitly during build process all functions 
    // like edfInitEyedentify are visible and directly accessible. 
    // The edfLinkAPI(nullptr,...) call just maps all functions to EdfAPI's functions pointers
    edfLinkAPI(nullptr, &edfAPI);
#endif

    // Modules pointers
    void *mmr_state   = nullptr;
    ERImage image;  // structure for input image from file
    std::memset(&image,0,sizeof(ERImage));

    //////////////////////////////////////////////////////////////
    // Eyedentify modules initialization
    //////////////////////////////////////////////////////////////
    // Build path to the modules.
    std::string edfModulePath = std::string(EDF_SDK_PATH) + "modules/" + std::string(EDF_MODULE_NAME) + "/";

    // select fast or precise model
    const char *mmr_model = nullptr;
    if (option_fast_version){
        mmr_model  = MMR_FAST_MODELS[2*option_mmr_task+option_mmr_type];
    } else {
        mmr_model  = MMR_PREC_MODELS[2*option_mmr_task+option_mmr_type];
    }

    // Initialize the module itself using the prepared configuration.
    std::cout << "Eyedentify MMR module initialization..." << std::endl;
    TIC
    EdfInitConfig config = setEdfInitConfig(edfModulePath.c_str(), mmr_model, option_computation_mode, option_gpu_id, option_num_threads,option_onnx_provider);
    if ( edfAPI.edfInitEyedentify(&config, &mmr_state) != 0 ){
        std::cerr << "Error during Eyedentify module initialization!\n";
        EARLY_EXIT
    }
    TOC("\tdone in ")
    // Print the used model version.
    std::cout << "MMR module model version: " << edfAPI.edfModelVersion(mmr_state) << std::endl;
    
    //////////////////////////////////////////////////////////////
    // Iterate over all input data
    //////////////////////////////////////////////////////////////
    for (size_t i = 0; i < num_inputs; i++) {
        InputData input = inputs[i];
        //////////////////////////////////////////////////////////////
        // Input image file loading
        //////////////////////////////////////////////////////////////
        image = loadImage(edfAPI, input.image);

        // Check whether the image was loaded
        if (image.size == 0) {
            std::cerr << "Error during " << input.image.imageFilename << " image reading!\n";
            EARLY_EXIT
        }

        //////////////////////////////////////////////////////////////
        // Cropping the input image
        //////////////////////////////////////////////////////////////
        // Create the image crop with respect to the license plate.
        ERImage cropImageMMR;
        EdfCropParams params{};
        
        if (option_mmr_type == MMRType::LP)
            params = setEdfCropParamsByLP(edfAPI, input.lp);
        else if (option_mmr_type == MMRType::CARBOX)
            params = setEdfCropParamsByCARBOX(edfAPI, input.carbox);
        else
        {
            std::cerr << "Option mmr_type not allowed!\n";
            EARLY_EXIT
        }

        TIC
        // Create the image crop with respect to the license plate.
        if (edfAPI.edfCropImage(&image, &params, mmr_state, &cropImageMMR, nullptr) != 0){
            std::cerr << "Error during image cropping for MMR recognition!\n";
            edfAPI.edfCropParamsFree(&params);
            EARLY_EXIT
        }
        TOC("Image for MMR recognition cropped in:   ")

        // If the input image is raw YCbCr420 data was user allocated (in loadImage()).
        // Delete the data here because erImageFree() does not delete user allocated data.
        if (input.image.isImageRawYCbCr420) {
            delete[] image.data;
            image.data = NULL;
        }
        // Free the image structure fields. The image is not needed anymore.
        edfAPI.erImageFree(&image);
        // Free vehicle crop parameters
        edfAPI.edfCropParamsFree(&params);

        //////////////////////////////////////////////////////////////
        // Descriptor computation
        //////////////////////////////////////////////////////////////
        // Create the EdfDescriptor structure.
        EdfDescriptor descriptor_mmr;
        // Compute the descriptor. Crop image is used as an input,
        // the output is copied to the EdfDescriptor structure.
        TIC
        int retval_mmr = edfAPI.edfComputeDesc(&cropImageMMR, mmr_state, &descriptor_mmr, NULL);
        TOC("MMR descriptor calculated in:       ");

        // Free the crop image data. The crop is not needed anymore.
        edfAPI.edfFreeCropImage(mmr_state , &cropImageMMR);

        // Check the descriptor computation result.
        if (retval_mmr != 0) {
            std::cerr << "Error during descriptor computation!\n";
            EARLY_EXIT
        }

        //////////////////////////////////////////////////////////////
        // Classification
        //////////////////////////////////////////////////////////////
        // Initialize the EdfClassifyResult structure pointer.
        EdfClassifyResult *classify_result_mmr = nullptr;

        // Run the classification. The result will be copied
        // to the newly created EdfClassifyResult structure.
        TIC
        if (edfAPI.edfClassify(&descriptor_mmr, mmr_state, &classify_result_mmr, NULL) != 0){
            std::cout << "Error during MMR classification!\n";
            EARLY_EXIT
        }
        TOC("MMR classification calculated in:   ");
       
        // Free the descriptor fields.
        edfAPI.edfFreeDesc(&descriptor_mmr);

        // print results
        printResults(*classify_result_mmr);

        // Free the classification result.
        edfAPI.edfFreeClassifyResult(&classify_result_mmr, mmr_state);

    }

    //////////////////////////////////////////////////////////////
    // Cleaning up
    //////////////////////////////////////////////////////////////

    // Free the module. All module internal structures
    // will be deleted and program can be finished.
    edfAPI.edfFreeEyedentify(&mmr_state);

    // Wait for user input.
    std::cout << std::endl << "--------------------------------------------------" << std::endl;
    std::cout << "Press a key to exit..." << std::endl;
    std::cin.get();

#ifdef EXPLICIT_LINKING
    ER_FREE_LIB(edfAPI.shlib_handle);
#endif
    return 0;
}

ERImage loadImage(EdfAPI &api, const InputImage &input){
    std::cout << std::endl << "File: " << input.imageFilename << std::endl;
    std::cout << "--------------------------------------------------" << std::endl;
    ERImage image;
    image.size = 0;
    // A) Load image from raw YCbCr420 data file.
    if (input.isImageRawYCbCr420) {
        // Set input raw file.
        std::ifstream file(input.imageFilename.c_str(), std::ios::binary | std::ios::ate);
        // Check whether file was opened.
        if (file.is_open()) {
            // Get file size.
            unsigned int size = (unsigned int)file.tellg();
            file.seekg(0, std::ios::beg);
            // Allocate array for file data.
            unsigned char *image_data = new unsigned char[size];
            // Read all data from file.
            if (!file.read((char*)image_data, size)) {
                // Delete array if data reading was not successful.
                delete[] image_data;
            } else {
                // Allocate EdfImage structure fields and set the data pointer to the YCbCr420 data.
                api.erImageAllocateAndWrap(&image, input.rawYCbCrWidth, input.rawYCbCrHeight,
                                                ER_IMAGE_COLORMODEL_YCBCR420, ER_IMAGE_DATATYPE_UCHAR,
                                                image_data, input.rawYCbCrWidth);
            }
        }
    // B) Load and decode image from image file.
    } else {
        // Read the input image
        int image_read_code = api.erImageRead(&image, input.imageFilename.c_str());
        // Check whether the image was loaded.
        if (image_read_code != 0) {
            // Handle errors
        }
    }
    return image;
}

EdfInitConfig setEdfInitConfig(const char *path, const char *name, ERComputationMode mode, int gpu_device_id, int num_threads, const std::string &onnx_provider)
{
    EdfInitConfig config{};
    config.module_path      = path; // e.g. ../../sdk/modules/edftf2lite-mmr/
    config.model_file       = name; // e.g. MMR_VCMMGVCT_FAST_2024Q2.dat
    config.computation_mode = mode; // e.g. ER_COMPUTATION_MODE_CPU
    config.gpu_device_id    = gpu_device_id;
    config.num_threads      = num_threads;
    config.onnx_provider    = onnx_provider.c_str();
    return config;
}
EdfCropParams setEdfCropParamsByLP(EdfAPI &api, const InputPositionLP &position)
{
    EdfCropParams params;
    api.edfCropParamsAllocate(EDF_MMR_CROP_POINTS, EDF_MMR_CROP_VALUES, &params);

    // Set license plate center in the input image.
    EDF_LP_CENTER_X(params)             = position.center_x;
    EDF_LP_CENTER_Y(params)             = position.center_y;
    // Set license plate resolution in pixels per meter.
    // Example car_cz.jpg: License plate has 134 pixels in the image and
    //    Czech LP is 0.52 m wide:  134/0.52 = 257.7
	EDF_LP_SCALE_PX_PER_M(params)       = position.resolution_ppm;
    // Set license plate rotation compensation in degrees.
    EDF_LP_ROTATION(params)             = position.rotation_dgr;
    return params;
}

EdfCropParams setEdfCropParamsByCARBOX(EdfAPI& api, const InputPositionCARBOX& position)
{
    EdfCropParams params;
    api.edfCropParamsAllocate(EDF_MMRBOX_CROP_POINTS, EDF_MMRBOX_CROP_VALUES, &params);

    // Set carbox coordinates to EDF input structure
    EDF_MMRBOX_TOP_LEFT_X(params) = position.top_left_x;
    EDF_MMRBOX_TOP_LEFT_Y(params) = position.top_left_y;
    EDF_MMRBOX_BOTTOM_RIGHT_X(params) = position.bottom_right_x;
    EDF_MMRBOX_BOTTOM_RIGHT_Y(params) = position.bottom_right_y;
    return params;
}

void printResults(const EdfClassifyResult &result_mmr){
    // Print the classification result to the console.
    std::cout << std::endl << "Vehicle classification:" << std::endl;
    std::cout << "Result:" << std::endl;
    // Iterate over all result entries.
    for (unsigned int i = 0; i < result_mmr.num_values; i++) {
        // Get the classification result value name.
        std::string name(result_mmr.values[i].task_name, result_mmr.values[i].task_name_length);
        // Get the classification result value.
        std::string value(result_mmr.values[i].class_name, result_mmr.values[i].class_name_length);
        std::cout << "\t" << name << ": " << value << " (" << result_mmr.values[i].class_id << ")" << std::endl;
    }
    // Iterate over all scores.
    std::cout << "Scores: [ ";
    for (unsigned int i = 0; i < result_mmr.num_values; i++) {
        // Get the classification score value.
        std::cout << result_mmr.values[i].score;
        if (i != (result_mmr.num_values-1)) {
            std::cout << ", ";
        }
    }
    std::cout << " ]" << std::endl;
}

bool check_arg(const char *arg, std::string option, std::string &retv)
{
    if (option.back()!='=' && strlen(arg) != option.length())
        return false;

    if (option.compare(0,option.length(),arg,option.length()) == 0)
    {
        retv.assign(arg+option.length());
        return true;
    }
    return false;
}

int parse_arguments(int argc, char * argv[], bool &help, bool &fast_version, MMRType& mmr_type, MMRTask &mmr_task, ERComputationMode &computation_mode, int &gpu_id, int &num_threads, std::string &onnx_provider)
{
    help = false;
    ////////////////////////////////////////////////////////////////
    //  Reading input parameters                                  //
    ////////////////////////////////////////////////////////////////
    for( int i = 1; i < argc; i++ )
    {
        std::string retv;
        if (check_arg(argv[i], "-vcmmgvct", retv) || check_arg(argv[i], "-VCMMGVCT", retv))
            mmr_task = VCMMGVCT;
        else if (check_arg(argv[i], "-vcmmct", retv) || check_arg(argv[i], "-VCMMCT", retv))
            mmr_task = VCMMCT;
        else if (check_arg(argv[i], "-vcmct", retv) || check_arg(argv[i], "-VCMCT", retv))
            mmr_task = VCMCT;
        else if (check_arg(argv[i], "-vcct", retv) || check_arg(argv[i], "-VCCT", retv))
            mmr_task = VCCT;
        else if (check_arg(argv[i], "-lp", retv))
            mmr_type = MMRType::LP;
        else if (check_arg(argv[i], "-carbox", retv))
            mmr_type = MMRType::CARBOX;
        else if (check_arg(argv[i], "-fast", retv))
            fast_version = true;
        else if (check_arg(argv[i], "-precise", retv))
            fast_version = false;
        else if (check_arg(argv[i], "-cpu", retv))
            computation_mode = ER_COMPUTATION_MODE_CPU;
        else if (check_arg(argv[i], "-gpu-id=", retv))
            gpu_id = atoi(retv.c_str());
        else if (check_arg(argv[i], "-gpu", retv))
            computation_mode = ER_COMPUTATION_MODE_GPU;
        else if (check_arg(argv[i], "-threads=", retv))
            num_threads = atoi(retv.c_str());
        else if (check_arg(argv[i], "-onnx-provider=", retv))
            onnx_provider = retv.c_str();
        else if (check_arg(argv[i], "-h", retv) || check_arg(argv[i], "--help", retv)){
            printf("NAME\n"
            "        example-mmr-API - Example of MMR SDK for vehicle type, color and tags recognition.\n\n"
            "SYNOPSIS\n"
            "        Unix   : ./example-mmr-API [options]\n"
            "        Windows: example-mmr-API.exe [options]\n"
            "\n"
            "DESCRIPTION\n"
            "        The example processes prepared set of images with license plate annotation (center, resolution and rotaion)\n"
            "        and makes recogntion of the vehicle based on selected MMR task:\n"
            "          VCMMGVCT - category-make-model-generation-variation\n"
            "          VCMMCT   - category-make-model\n"
            "          VCMCT    - category-make\n"
            "          VCCT     - category\n"
            "        Defaults is VCMMCT task.\n\n"
            "OPTIONS\n"
            "        -h, --help this help\n"
            "        -vcmmgvct, -VCMMGVCT \n"
            "                   classify the vehicle using category-make-model-generation-variation model\n"
            "        -vcmmct, -VCMMCT \n"
            "                   classify the vehicle using category-make-model model [default]\n"
            "        -vcmct, -VCMCT \n"
            "                   classify the vehicle using category-make model\n"
            "        -vcct, -VCCT \n"
            "                   classify the vehicle using category model\n"
            "        -lp        use license plate for image alignment in edfCropImage [default]\n"
            "        -carbox    use carbox for image alignment in edfCropImage\n"
            "        -fast      use fast models for recognition [default]\n"
            "        -precise   use precise models for recognition (suitable for server solutions with GPU)\n"
            "        -cpu       run recognition on cpu device [default]\n"
            "        -gpu       run recognition on cpu device\n"
            "        -gpu-id=GPU_ID \n"
            "                   set GPU_ID gpu device for computation\n"
            "        -threads=NUM_THREADS \n"
            "                   set number of threads for cpu computation [default 1]. \n"
            "                   Number of threads is bounded by <1,hardware_concurrency> range.\n"
            "                   Set -1 value for hardware_concurrency.\n"
            "        -onnx-provider=PROVIDER \n"
            "                   Set ONNX provider - one of {cpu,cuda,tensorrt,rocm,openvino} [default cpu]\n"
            "                   This setting is superior to -cpu if 'cuda','tensorrt' or 'rocm' is used.\n"
            "\n"
            "EXAMPLE\n"
            "        example-mmr-API -vcmmgvct -precise  \n"
            "                    run example with VCMMGVCT task only using precise models \n"
            "\n"
            "(C) 2023, Eyedea Recognition s.r.o., http://www.eyedea.cz\n"
            "\n");
            help = true;
            return 0;
        }
        else{
            printf("WARNING: Unknown option %s\nSee `%s --help' for more information.\n",argv[i], argv[0]);
            return -1;
        }
    }
    return 0;
}
