########################################################################################
##                                                                                    ##
##    Python wrapper example example_mmr.py                                           ##
## ---------------------------------------------------------------------------------- ##
##                                                                                    ##
## Copyright (c) 2020-2023 by Eyedea Recognition, s.r.o.                              ##
##                                                                                    ##
## Author: Eyedea Recognition, s.r.o.                                                 ##
##                                                                                    ##
## Contact:                                                                           ##
##           web: http://www.eyedea.cz                                                ##
##           email: info@eyedea.cz                                                    ##
##                                                                                    ##
## BSD License                                                                        ##
## -----------------------------------------------------------------------------------##
## Copyright (c) 2020-2021, Eyedea Recognition, s.r.o.                                ##
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

from er import ER
from eyedentify import Eyedentify, EdfInitConfig, ER_COMPUTATION_MODE_CPU, ER_COMPUTATION_MODE_GPU, EdfCropParams, \
    EdfCropImageConfig, EdfComputeDescConfig, EdfClassifyConfig


from PIL import Image
import numpy as np
from cffi import FFI

import time

ffi = FFI()

# EXAMPLE
# setup main variables
SDK_DIR = "../../sdk/"
EYEDENTIFY_LIB = Eyedentify.get_eyedentify_path(SDK_DIR)
SUPPORT_LIBS = []

USE_CARBOX = False  # Set to 'False' to use license plate based recognition, 'True' to use carbox based recognition
MODULE_PATH = "../../sdk/modules/edftf2lite/"
MODEL_FILE_LP = "MMR_VCMMCT_FAST_2024Q2.dat"
MODEL_FILE_CARBOX = "MMRBOX_VCMMCT_FAST_2024Q2.dat"

# IMAGE_FILE = "car_color.jpg"
# IMAGE_CROP_PARAMS_LP = [[475], [573], 257.7, 1]  # list of cols, list of rows, pixels per meter scale, in-plane angle
# IMAGE_CROP_PARAMS_CARBOX = [[282, 140], [748, 630]]

IMAGE_FILE = "car_gray.jpg"
IMAGE_CROP_PARAMS_LP = [[319], [182], 173.0, 0]  # list of cols, list of rows, pixels per meter scale, in-plane angle
IMAGE_CROP_PARAMS_CARBOX = [[183, 495], [-100, 216]]

eyedea_er = ER(ffi, EYEDENTIFY_LIB)
mmr = Eyedentify(ffi, SDK_DIR)
# setup Eyedentify Python bindings

# setup EdfInitConfig
my_config = EdfInitConfig()
my_config.module_path = MODULE_PATH
my_config.model_file = MODEL_FILE_CARBOX if USE_CARBOX else MODEL_FILE_LP
my_config.computation_mode = ER_COMPUTATION_MODE_CPU
my_config.gpu_device_id = 0

# initialize Eyedentify library
mmr.init_eyedentify(my_config)

# get model version of Eyedentify model loaded
model_version = mmr.model_version()

print("Model version is " + str(model_version))

# read image
img = Image.open(IMAGE_FILE)

# MMR expects color image, convert to color if gray
img = img.convert("RGB")
er_image = eyedea_er.convert_pil_image_to_erimage(img)

# crop by license plate detection
crop_params = EdfCropParams()

if not USE_CARBOX:
    crop_params.points.cols = IMAGE_CROP_PARAMS_LP[0]
    crop_params.points.rows = IMAGE_CROP_PARAMS_LP[1]
    crop_params.values.values = [IMAGE_CROP_PARAMS_LP[2], IMAGE_CROP_PARAMS_LP[3]]
else:
    crop_params.points.cols = IMAGE_CROP_PARAMS_CARBOX[0]
    crop_params.points.rows = IMAGE_CROP_PARAMS_CARBOX[1]

crop_config = EdfCropImageConfig()

t = time.process_time()
img_crop, img_crop_mode = mmr.crop_image(er_image, crop_params, crop_config)
er_image_crop = eyedea_er.convert_nparray_to_erimage(img_crop, img_crop_mode)

print("Crop computed in " + str(time.process_time() - t) + "s.")

# optional: convert image crop for saving (convert to uint8 + color, cutoff at 0-255)
# if img_crop_mode == "L":
#     pil_crop = Image.fromarray(np.uint8(img_crop), "L")
#     pil_crop = pil_crop.convert("RGB")
# elif img_crop_mode == "BGR":
#     pil_crop = Image.fromarray(np.uint8(img_crop), "RGB")
#     b, g, r = pil_crop.split()
#     pil_crop = Image.merge("RGB", (r, g, b))

# compute descriptor on the crop
desc_config = EdfComputeDescConfig()

t = time.process_time()
desc = mmr.compute_desc(er_image_crop, desc_config)
print("Descriptor computed in " + str(time.process_time() - t) + "s.")

# batch computation example
# NOTE: not supported by edftf2lite module
#t = time.process_time()
#batch = [er_image_crop, er_image_crop, er_image_crop, er_image_crop, er_image_crop]
#descs = mmr.compute_desc(batch)
#elapsed = time.process_time() - t
#print("\nDescriptor batch (" + str(len(descs)) + ") computed in " + str(elapsed) + "s.")
#print("Average descriptor computation time: " + str(elapsed/len(descs)) + "s")

# classify based on the descriptor
classify_config = EdfClassifyConfig()
classify_config.use_dependency_rules = 0
classify_result = mmr.classify(desc, classify_config)

# print results
print("\nClassification:")
for value in classify_result:
    print("\tTask: " + value.task_name + ", class: " + value.class_name + " [" +
          str(value.class_id) + "] (" + str(value.score) + ")")
