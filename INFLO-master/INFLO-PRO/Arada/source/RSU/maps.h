/**
 * Definitions for storing Map Data for the MnDOT Project
 */
#ifndef	_MAPS_H_
#define	_MAPS_H_




/*
 * Structure Definition for storing map data in a more usable state.
 */
typedef struct LatLong {
	double Latitude;
	double Longitude;
} latLong_t;


typedef struct Map_Lane {
	int	 	id;
	int	 	width;
	int 	heading;
	struct Map_Nodes {

		A_SEQUENCE_OF(struct LatLong) list;

		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} nodes;
	/*
	 * This type is extensible,
	 * possible extensions are below.
	 */

	/* Context for parsing across buffer boundaries */
	asn_struct_ctx_t _asn_ctx;
} Map_Lane_t;


/* MAPLANES */
typedef struct MAPLANES {
	struct Map_Lanes {
		A_SEQUENCE_OF(struct Map_Lane) list;

		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} mapLanes;

} MAPLANES_t;

MAPLANES_t				*mapLanes;

#endif
