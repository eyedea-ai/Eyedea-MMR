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

#ifndef EYEDEA_ER_IMAGE_H
#define EYEDEA_ER_IMAGE_H

#include "er_explink.h"

/* ***************************************************************************
 * IMAGE COLOR MODELS                                                        *
 * Color models/schemes used by ERImage.                                     *
 * Color models defines channels. Each pixel consists of channels.           *
 * Eg. Pixel of color image (ER_IMAGE_COLORMODEL_BGR) has 3 channels         *
 * - B (blue), G (green) and R (red) channel so image data buffer            *
 * is organized BGRBGRBGR                                                    *
 * ***************************************************************************/
typedef enum {
    ER_IMAGE_COLORMODEL_UNK       = 0,   /* Unknown color model */
    ER_IMAGE_COLORMODEL_GRAY      = 1,   /* Gray scale image */
    ER_IMAGE_COLORMODEL_BGR       = 2,   /* Color image BGR channels */
    ER_IMAGE_COLORMODEL_YCBCR420  = 3,   /* YCbCr 4:2:0 image */
    ER_IMAGE_COLORMODEL_BGRA      = 4,   /* Color image BGRA channels */
    ER_IMAGE_COLORMODEL_YCBCRNV12 = 5    /* YCbCr with NV12 layout image */
} ERImageColorModel;

/* ***************************************************************************
 * IMAGE DATA TYPES                                                          *
 * Image data type per channel.                                              *
 * Eg. ER_IMAGE_COLORMODEL_BGR image with ER_IMAGE_DATATYPE_UCHAR takes      *
 * 3 bytes per pixel.                                                        *
 * NOTE: ER_IMAGE_COLORMODEL_YCBCR420 and ER_IMAGE_COLORMODEL_YCBCRNV12      *
 * color models are supported as an crop image input only.                   *
 * ***************************************************************************/
typedef enum {
    ER_IMAGE_DATATYPE_UNK   = 0,        /* Unknown data type */
    ER_IMAGE_DATATYPE_UCHAR = 1,        /* byte representation [0-255] */
    ER_IMAGE_DATATYPE_FLOAT = 2         /* float representation */
} ERImageDataType;


/* ***************************************************************************
 * EYEDEA RECOGNITION IMAGE STRUCTURE                                        *
 * ERImage is a structure representing image used by Eyedea Recognition s.r.o*
 * It contains channels and data type specification.                         *
 * ***************************************************************************/
typedef struct
{
    ERImageColorModel color_model;      /*!< Color model of the image */
    ERImageDataType   data_type;        /*!< Data type of the image */
    unsigned int      width;            /*!< Width of the image in pixels */
    unsigned int      height;           /*!< Height of the image in pixels, for YCbCr color models it is 2/3 of actual row_data size, in the last third the Cb and Cr are stored. */
    unsigned int      num_channels;     /*!< Number of channels, zero for YCbCr color models. */
    unsigned int      depth;            /*!< Bytes per pixel, depth = num_channels * sizeof(type), where type is "unsigned char" or "float" based on data_type value, zero for YCbCr color models. */
    unsigned int      step;             /*!< Row byte step, width * depth + possible alignment bytes */
    unsigned int      size;             /*!< Byte size of the image data (step * height) */
    unsigned int      data_size;        /*!< Byte size of the allocated data */
    unsigned char*    data;             /*!< Pointer to the image data, interleaved if num_channels > 1 */
    unsigned char**   row_data;         /*!< pointer to pointers to the image row data */
    unsigned char     data_allocated;   /*!< flag if structure use self allocated data */
} ERImage;


/* ***************************************************************************
 * HELPER FUNCTIONS FOR ERImage                                              *
 * ***************************************************************************/

/** Allocate all dynamic fields and fill the ERImage structure */
ER_FUNCTION_PREFIX int          erImageAllocate(ERImage* image, unsigned int width, unsigned int height, ERImageColorModel color_model, ERImageDataType data_type);

/** Allocate dynamic fields but data buffer and fill the ERImage structure */
ER_FUNCTION_PREFIX int          erImageAllocateBlank(ERImage* image, unsigned int width, unsigned int height, ERImageColorModel color_model, ERImageDataType data_type);

/** Allocate dynamic fields but data buffer and fill the ERImage structure, the input data buffer is used for image data  */
ER_FUNCTION_PREFIX int          erImageAllocateAndWrap(ERImage* image, unsigned int width, unsigned int height, ERImageColorModel color_model, ERImageDataType data_type, unsigned char* data, unsigned int step);

/** Get byte size of given data type */
ER_FUNCTION_PREFIX unsigned int erImageGetDataTypeSize(ERImageDataType data_type);

/** Get Number of channels for given color model */
ER_FUNCTION_PREFIX unsigned int erImageGetColorModelNumChannels(ERImageColorModel color_model);

/** Get ERImage pixel depth */
ER_FUNCTION_PREFIX unsigned int erImageGetPixelDepth(ERImageColorModel color_model, ERImageDataType data_type);

/** Deep copy of image */
ER_FUNCTION_PREFIX int          erImageCopy(const ERImage* image, ERImage* image_copy);

/** Read image from file */
ER_FUNCTION_PREFIX int          erImageRead(ERImage* image, const char *filename);

/** Write image to file */
ER_FUNCTION_PREFIX int          erImageWrite(const ERImage* image, const char* filename);

/** Free dynamic fields of ERImage */
ER_FUNCTION_PREFIX void         erImageFree(ERImage *image);

/** Get string with version of ERImage */
ER_FUNCTION_PREFIX const char*  erVersion(void);

/** Get string with error log messages and trace*/
ER_FUNCTION_PREFIX const char*  erGetErrorLog(void);

/** Reset error log, invalidates previous const char* obtained from erGetErrorLog() */
ER_FUNCTION_PREFIX void erResetErrorLog(void);

/* function pointers types for explicit linking */
typedef unsigned int (*fcn_erImageGetDataTypeSize)          (ERImageDataType);
typedef unsigned int (*fcn_erImageGetColorModelNumChannels) (ERImageColorModel);
typedef unsigned int (*fcn_erImageGetPixelDepth)            (ERImageColorModel, ERImageDataType);
typedef int          (*fcn_erImageAllocateBlank)            (ERImage*, unsigned int, unsigned int, ERImageColorModel, ERImageDataType);
typedef int          (*fcn_erImageAllocate)                 (ERImage*, unsigned int, unsigned int, ERImageColorModel, ERImageDataType);
typedef int          (*fcn_erImageAllocateAndWrap)          (ERImage*, unsigned int, unsigned int, ERImageColorModel, ERImageDataType, unsigned char*, unsigned int);
typedef int          (*fcn_erImageCopy)                     (const ERImage*, ERImage*);
typedef int          (*fcn_erImageRead)                     (ERImage*, const char*);
typedef int          (*fcn_erImageWrite)                    (const ERImage*, const char*);
typedef void         (*fcn_erImageFree)                     (ERImage*);
typedef const char*  (*fcn_erVersion)                       (void);
typedef const char*  (*fcn_erGetErrorLog)                   (void);
typedef void         (*fcn_erResetErrorLog)                 (void);

#endif
