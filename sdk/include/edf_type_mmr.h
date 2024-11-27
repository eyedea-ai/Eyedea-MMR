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

///////////////////////////////////////////////////////////
//                   EYEDENTIFY SDK                      //
//                 recognition library                   //
///////////////////////////////////////////////////////////

#ifndef EDF_TYPE_MMR
#define EDF_TYPE_MMR

// MMR based on LP detection
// Number of points used for the input image cropping
#define EDF_MMR_CROP_POINTS           1
// Number of values used for the input image cropping
#define EDF_MMR_CROP_VALUES           2
// Macro used for easy filling the X point coordinate in the cropping parameters structure
#define EDF_LP_CENTER_X(params)       params.points.cols[0]
// Macro used for easy filling the Y point coordinate in the cropping parameters structure
#define EDF_LP_CENTER_Y(params)       params.points.rows[0]
// Macro used for easy filling the px/m scale in the cropping parameters structure
#define EDF_LP_SCALE_PX_PER_M(params) params.values.values[0]
// Macro used for easy filling the rotation angle in the cropping parameters structure
#define EDF_LP_ROTATION(params)       params.values.values[1]

// MMR based on CARBOX detection
// Number of points used for the input image cropping
#define EDF_MMRBOX_CROP_POINTS           2
// Number of values used for the input image cropping
#define EDF_MMRBOX_CROP_VALUES           0
// Macro used for easy filling the top left corner X point coordinate in the cropping parameters structure
#define EDF_MMRBOX_TOP_LEFT_X(params)       params.points.cols[0]
// Macro used for easy filling the top left corner Y point coordinate in the cropping parameters structure
#define EDF_MMRBOX_TOP_LEFT_Y(params)       params.points.rows[0]
// Macro used for easy filling the bottom right corner X point coordinate in the cropping parameters structure
#define EDF_MMRBOX_BOTTOM_RIGHT_X(params)   params.points.cols[1]
// Macro used for easy filling the bottom right corner Y point coordinate in the cropping parameters structure
#define EDF_MMRBOX_BOTTOM_RIGHT_Y(params)   params.points.rows[1]

#endif // EDF_TYPE_MMR
