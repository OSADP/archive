package org.battelle.inczone.inczoneui.kml;

public class LatLngPoint {

	private final double lat;
	private final double lng;

	public LatLngPoint(double lat, double lng) {
		this.lat = lat;
		this.lng = lng;
	}

	public double getLat() {
		return lat;
	}

	public double getLng() {
		return lng;
	}
}
