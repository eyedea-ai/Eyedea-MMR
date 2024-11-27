"""
////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////
///                                                              ///
///    ERImage wrapper for Python 3                              ///
///   --------------------------------------------------------   ///
///                                                              ///
///    Eyedea Recognition, Ltd. (C) 2017-2018                    ///
///                                                              ///
///    Contact:                                                  ///
///               web: http://www.eyedea.cz                      ///
///             email: info@eyedea.cz                            ///
///                                                              ///
////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////
"""

from cffi import FFI
from PIL import Image
import numpy as np
import inspect

ER_IMAGE_COLORMODEL_UNK = 0
ER_IMAGE_COLORMODEL_GRAY = 1
ER_IMAGE_COLORMODEL_BGR = 2
ER_IMAGE_COLORMODEL_YCBCR420 = 3
ER_IMAGE_COLORMODEL_BGRA = 4
ER_IMAGE_COLORMODEL_YCBCRNV12 = 5

ER_IMAGE_ALL_COLORMODELS = [ER_IMAGE_COLORMODEL_UNK,
                            ER_IMAGE_COLORMODEL_GRAY,
                            ER_IMAGE_COLORMODEL_BGR,
                            ER_IMAGE_COLORMODEL_YCBCR420,
                            ER_IMAGE_COLORMODEL_BGRA,
                            ER_IMAGE_COLORMODEL_YCBCRNV12]

ER_IMAGE_DATATYPE_UNK = 0
ER_IMAGE_DATATYPE_UCHAR = 1
ER_IMAGE_DATATYPE_FLOAT = 2

ER_IMAGE_ALL_DATATYPES = [ER_IMAGE_DATATYPE_UNK,
                          ER_IMAGE_DATATYPE_UCHAR,
                          ER_IMAGE_DATATYPE_FLOAT]

ER_COMPUTATION_MODE_CPU = 0
ER_COMPUTATION_MODE_GPU = 1
ER_COMPUTATION_MODE_TPU = 2

ER_ALL_COMPUTATION_MODES = [ER_COMPUTATION_MODE_CPU,
                            ER_COMPUTATION_MODE_GPU,
                            ER_COMPUTATION_MODE_TPU]

class ErError(Exception):
    """Eyedea ER Error class."""

    def __init__(self, internal_function_name, internal_error_code):
        self.internal_function_name = internal_function_name
        self.internal_error_code = internal_error_code


class ERRoI:
    """Region of Interest class."""

    def __init__(self):
        self.x = 0.0
        self.y = 0.0
        self.width = 0.0
        self.height = 0.0

    def c_init(self, ffi: FFI, c_structure):
        """
        Fills this mirror structure with given C structure data.
        :param ffi: Instance of the FFI class.
        :param c_structure: C structure data.
        """
        if c_structure == ffi.NULL:
            return

        self.x = c_structure.x
        self.y = c_structure.y
        self.width = c_structure.width
        self.height = c_structure.height

    def get_c(self, ffi: FFI):
        """
        Converts this Python structure into a C structure.
        :param ffi: The FFI to use for creation of the C structure.
        :return: The resulting C structure
        """
        c_structure = ffi.new("ERRoI *")

        c_structure.x = self.x
        c_structure.y = self.y
        c_structure.width = self.width
        c_structure.height = self.height

        return c_structure


class ERRotatedRect:
    """Rotated rectangle class."""

    def __init__(self):
        self.x = 0.0
        self.y = 0.0
        self.width = 0.0
        self.height = 0.0
        self.angle = 0.0

    def c_init(self, ffi: FFI, c_structure):
        """
        Fills this mirror structure with given C structure data.
        :param ffi: Instance of the FFI class.
        :param c_structure: C structure data.
        """
        if c_structure == ffi.NULL:
            return

        self.x = c_structure.x
        self.y = c_structure.y
        self.width = c_structure.width
        self.height = c_structure.height
        self.angle = c_structure.angle

    def get_c(self, ffi: FFI):
        """
        Converts this Python structure into a C structure.
        :param ffi: The FFI to use for creation of the C structure.
        :return: The resulting C structure
        """
        c_structure = ffi.new("ERRotatedRect *")

        c_structure.x = self.x
        c_structure.y = self.y
        c_structure.width = self.width
        c_structure.height = self.height
        c_structure.angle = self.angle

        return c_structure


class ERPoint:
    """Class for 2D points."""

    def __init__(self):
        self.x = 0.0
        self.y = 0.0


class ER:
    """ER library for image handling wrapper."""

    @staticmethod
    def __define_ffi(ffi):
        # type definitions from er_image.h
        ffi.cdef("""
                 typedef enum 
                 {
                     ER_IMAGE_COLORMODEL_UNK      = 0,
                     ER_IMAGE_COLORMODEL_GRAY     = 1,
                     ER_IMAGE_COLORMODEL_BGR      = 2,
                     ER_IMAGE_COLORMODEL_YCBCR420 = 3,
                     ER_IMAGE_COLORMODEL_BGRA     = 4,
                     ER_IMAGE_COLORMODEL_YCBCRNV12 = 5
                 } ERImageColorModel;
                 """)

        ffi.cdef("""
                 typedef enum
                 {
                     ER_IMAGE_DATATYPE_UNK   = 0,
                     ER_IMAGE_DATATYPE_UCHAR = 1,
                     ER_IMAGE_DATATYPE_FLOAT = 2
                 } ERImageDataType;
                 """)

        ffi.cdef("""
                 typedef enum
                 {
                     ER_COMPUTATION_MODE_CPU = 0,
                     ER_COMPUTATION_MODE_GPU = 1,
                     ER_COMPUTATION_MODE_TPU = 2
                 } ERComputationMode;
                 """)

        ffi.cdef("""
                 typedef struct
                 {
                     ERImageColorModel color_model;
                     ERImageDataType   data_type;
                     unsigned int      width;
                     unsigned int      height;
                     unsigned int      num_channels;
                     unsigned int      depth;
                     unsigned int      step;
                     unsigned int      size;
                     unsigned int      data_size;
                     unsigned char*    data;
                     unsigned char**   row_data;
                     unsigned char     data_allocated;
                 } ERImage;
                 """)

        ffi.cdef("""
                 typedef struct
                 {
                     float x;
                     float y;
                 } ERPoint2f;
                 """)

        ffi.cdef("""typedef ERPoint2f ERPoint;""")

        ffi.cdef("""
                 typedef struct
                 {
                     int x;
                     int y;
                     int width;
                     int height;
                 } ERRoI;
                 """)

        ffi.cdef("""
                 typedef struct
                 {
                     float x;
                     float y;
                     float width;
                     float height;
                     float angle;
                 } ERRotatedRect;
                 """)

        ffi.cdef("""
                 typedef struct
                 {
                     ERPoint2f *points;
                     unsigned int num_points;
                 } ERPoints;
                 """)

        # function definition from er_image.h
        ffi.cdef("""
                 int          erImageAllocate(ERImage* image, unsigned int width, unsigned int height, 
                                              ERImageColorModel color_model, ERImageDataType data_type);
                 """)

        ffi.cdef("""
                 void         erImageFree(ERImage *image);
                 """)

    def __init__(self, ffi, lib_with_er_path, support_libs=None):
        if type(lib_with_er_path) is not str:
            raise TypeError("Variable lib_with_er_path must be a string.")

        # add function definitions to ffi
        self.__define_ffi(ffi)
        self.ffi = ffi

        # load top library and dependency
        if type(support_libs) is list:
            for lib in support_libs:
                ffi.dlopen(lib)

        self.__er = ffi.dlopen(lib_with_er_path)

        if self.__er == ffi.NULL:
            raise ErError("""Failed to open shared library. 
                             ER must be loaded from a library which includes ER (EyeScan, EyeFace, Eyedentify...).""",
                          -1)

    def convert_pil_image_to_erimage(self, pil_image):
        # check that one of base classes is PIL.Image.Image
        if Image.Image not in inspect.getmro(pil_image.__class__):
            raise TypeError("Input pil_image must be a PIL.Image.Image object.")

        if FFI not in inspect.getmro(self.ffi.__class__):
            raise TypeError("Input must be a FFI class from cffi.")

        width = self.ffi.cast("unsigned int", pil_image.width)
        height = self.ffi.cast("unsigned int", pil_image.height)

        # expect uchar data type
        data_type = ER_IMAGE_DATATYPE_UCHAR

        if pil_image.mode == "L":
            color_model = ER_IMAGE_COLORMODEL_GRAY
        elif pil_image.mode == "RGB":
            color_model = ER_IMAGE_COLORMODEL_BGR
            # convert to BGR
            b, g, r = pil_image.split()
            pil_image = Image.merge("RGB", (r, g, b))
        elif pil_image.mode == "RGBA":
            color_model = ER_IMAGE_COLORMODEL_BGRA
            # convert to BGRA
            b, g, r, a = pil_image.split()
            pil_image = Image.merge("RGBA", (r, g, b, a))
        else:
            print("Color model conversion not implemented")
            raise TypeError("Input pil_image object must be of mode L or RGB.")

        er_image = self.ffi.new("ERImage*")

        ret_val = self.__er.erImageAllocate(er_image, width, height, color_model, data_type)

        if ret_val != 0:
            raise TypeError("Failed to allocate ERImage.")

        # copy data to er image
        byte_buffer = pil_image.tobytes()
        self.ffi.memmove(er_image.data, byte_buffer, er_image.step * er_image.height)

        er_image_gc = self.ffi.gc(er_image, self.__er.erImageFree)

        return er_image_gc

    @staticmethod
    def convert_erimage_to_nparray(er_image):
        # alloc new numpy array
        if er_image.data_type == ER_IMAGE_DATATYPE_UCHAR:
            np_dtype = "uint8"
        elif er_image.data_type == ER_IMAGE_DATATYPE_FLOAT:
            np_dtype = "float32"
        else:
            raise TypeError("Expected er_image.data_type either uchar or float.")

        if er_image.color_model == ER_IMAGE_COLORMODEL_GRAY:
            mode = "L"
        elif er_image.color_model == ER_IMAGE_COLORMODEL_BGR:
            mode = "BGR"
        else:
            raise TypeError("Expected er_image.color_model either GRAY or BGR.")

        # check there is no step
        if er_image.depth * er_image.width != er_image.step:
            raise ValueError("Input image data not continuous - not implemented.")

        b = bytearray(er_image.data_size)
        FFI().memmove(b, er_image.data, er_image.data_size)
        np_image = np.frombuffer(b, np_dtype, er_image.height * er_image.width * er_image.num_channels)
        np_image = np.reshape(np_image, [er_image.height, er_image.width, er_image.num_channels])

        return np_image, mode

    def convert_nparray_to_erimage(self, np_image, np_image_mode):

        if np_image.dtype.type == np.uint8:
            data_type = ER_IMAGE_DATATYPE_UCHAR
        elif np_image.dtype.type == np.float32:
            data_type = ER_IMAGE_DATATYPE_FLOAT
        else:
            raise ValueError("np_image data type expected to be uint8 or float32.")

        if np_image_mode == "L":
            color_model = ER_IMAGE_COLORMODEL_GRAY
        elif np_image_mode == "BGR":
            color_model = ER_IMAGE_COLORMODEL_BGR
        elif np_image_mode == "BGRA":
            color_model = ER_IMAGE_COLORMODEL_BGRA
        else:
            raise ValueError("np_image_mode expected to be 'L', 'BGR' or 'BGRA'")

        er_image = self.ffi.new("ERImage*")

        width = np_image.shape[1]
        height = np_image.shape[0]

        ret_val = self.__er.erImageAllocate(er_image, width, height, color_model, data_type)

        if ret_val != 0:
            raise TypeError("Failed to allocate ERImage.")

        # copy data to er image
        self.ffi.memmove(er_image.data, np_image.tobytes(), er_image.step * er_image.height)

        er_image_gc = self.ffi.gc(er_image, self.__er.erImageFree)

        return er_image_gc

    def convert_bytes_to_erimage(self, width, height, color_model, data_type, data, step):

        if color_model not in ER_IMAGE_ALL_COLORMODELS or data_type not in ER_IMAGE_ALL_DATATYPES:
            raise ValueError("Color model or data type not valid.")

        size_of_element = (1 if data_type == ER_IMAGE_DATATYPE_UCHAR else 4
                           if data_type == ER_IMAGE_DATATYPE_FLOAT else 0)

        if step != width * size_of_element:
            raise NotImplementedError("Step size must be same as width*sizeof(element) now.")

        er_image = self.ffi.new("ERImage*")
        ret_val = self.__er.erImageAllocate(er_image, width, height, color_model, data_type)

        if ret_val != 0:
            raise TypeError("Failed to allocate ERImage.")

        if color_model in [ER_IMAGE_COLORMODEL_GRAY, ER_IMAGE_COLORMODEL_BGR, ER_IMAGE_COLORMODEL_BGRA]:
            self.ffi.memmove(er_image.data, data, er_image.step * er_image.height)
        elif color_model in [ER_IMAGE_COLORMODEL_YCBCR420, ER_IMAGE_COLORMODEL_YCBCRNV12]:
            # Y plane in full res, then Cb and Cr as 1 per 2x2 pixels
            self.ffi.memmove(er_image.data, data, er_image.step * (er_image.height + er_image.height // 2))

        er_image_gc = self.ffi.gc(er_image, self.__er.erImageFree)

        return er_image_gc
