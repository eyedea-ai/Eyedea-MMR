/* **************************************************************** */
/* **************************************************************** */
/* *                                                              * */
/* *    Copyright (c) 2022 by Eyedea Recognition, s.r.o.          * */
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

#ifndef EYEDEA_ER_TYPE_H
#define EYEDEA_ER_TYPE_H

#include "er_explink.h"

/** computation mode enumeration */
enum ERComputationMode {
    ER_COMPUTATION_MODE_CPU = 0, /**< compute using CPU */
    ER_COMPUTATION_MODE_GPU = 1, /**< compute using GPU */
    ER_COMPUTATION_MODE_TPU = 2  /**< compute using TPU */
};

/** c-structure for 2D points specified by its pixel index cordinates x and y */
typedef struct {
    int x; /**< x coordinate of the point */
    int y; /**< y coordinate of the point */
} ERPoint2i;

/** c-structure for 2D points specified by its floating points cordinates x and y */
typedef struct {
    float x; /**< x coordinate of the point */
    float y; /**< y coordinate of the point */
} ERPoint2f;

typedef ERPoint2f ERPoint;

/** Region of Interest structure */
typedef struct {
    int x;      /**<  x-coordinate (col) of the top left corner. */
    int y;      /**<  y-coordinate (row) of the top left corner. */
    int width;  /**<  width of the ROI, negative value stands for full width of the image */
    int height; /**<  height of the ROI, negative value stands for full height of the image */
} ERRoI;

/** c-structure representing rotated (i.e. not up-right) rectangle on a plane */
typedef struct {
    float x;      /**< mass center x coordinate */
    float y;      /**< mass center y coordinate */
    float width;  /**< width of the rectangle */
    float height; /**< height of the rectangle */
    float angle;  /**< clockwise rotation angle (in degrees) of the rectangle. When the angle is 0, 90, 180, 270 etc., the rectangle becomes an up-right rectangle. */
} ERRotatedRect;

/** ERPoint2f array container */
typedef struct {
    ERPoint *points; /**< array of ERPoint2f */
    unsigned int num_points; /**< number of points in array */
} ERPoints;

// union ERValue{
//     int i;
//     unsigned int ui;
//     float f;
//     double d;
//     size_t size;
//     const char* c;
//     void* v;
// };
//
// typedef struct {
//     const char *keys;
//     ERValue     value;
// } ERProperty;
//
// typedef struct {
//     ERProperty  *properties;
//     unsigned int num_properties;
// } ERProperties
//
// typedef struct {
//     ERRotatedRect position;
//     ERPoints      points;
//
//     int   class_id;
//     const char* label;
//     float score;
//     float occlusion;
//     int   truncated;
//
//     ERProperty    *properties;
//     unsigned int  num_properties;
// } ERDetection;

/** Helper function to convert ERRotatedRect structure to ERPoint2f[4] array
 *
 *  Function to converts ERRotatedRect structure to ERPoint2f[4] array.
 *  Returns corner points of the rotated rectangle in clockwise direction starting with top-left corner.
 *
 *  \param[in] rect Pointer to ERRotatedRect
 *  \param[out] points Allocated array of ERPoint2f structure with at least 4 elements.
 *  \return Zero on success, errno otherwise.
 */
ER_FUNCTION_PREFIX int erRotatedRectToPoints( const ERRotatedRect *rect, ERPoint2f (*points)[4] );


#endif
