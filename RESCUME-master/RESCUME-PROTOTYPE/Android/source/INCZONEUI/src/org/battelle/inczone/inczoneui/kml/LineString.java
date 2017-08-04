package org.battelle.inczone.inczoneui.kml;

import java.util.ArrayList;
import java.util.List;

public class LineString {

	private final String name;
	private final int color;
	private final List<LatLngPoint> points = new ArrayList<LatLngPoint>();

	public LineString(String name, int color) {
		this.name = name;
		this.color = color;
	}

	public void addPoint(LatLngPoint pnt) {
		points.add(pnt);
	}

	public String getXml(int indent) {

		StringBuilder sb = new StringBuilder();

		addIndents(sb, indent);
		sb.append("<Placemark>\n");

		addIndents(sb, indent + 1);
		sb.append("<name>");
		sb.append(name);
		sb.append("</name>\n");

		addIndents(sb, indent + 1);
		sb.append("<LineString>\n");

		addIndents(sb, indent + 2);
		sb.append("<coordinates>\n");

		addIndents(sb, indent + 3);

		for (LatLngPoint pnt : points) {
			sb.append(String.format("%f,%f,0 ", pnt.getLng(), pnt.getLat()));
		}
		sb.append('\n');

		addIndents(sb, indent + 2);
		sb.append("</coordinates>\n");

		addIndents(sb, indent + 1);
		sb.append("</LineString>\n");

		addIndents(sb, indent + 1);
		sb.append("<Style>\n");

		addIndents(sb, indent + 2);
		sb.append("<LineStyle>\n");

		addIndents(sb, indent + 3);
		sb.append("<color>");
		sb.append(String.format("#%02X%02X%02X%02X", (this.color >> 24) & 0xFF, (this.color >> 0) & 0xFF,
				(this.color >> 8) & 0xFF, (this.color >> 16) & 0xFF));
		sb.append("</color>\n");

		addIndents(sb, indent + 2);
		sb.append("</LineStyle>\n");

		addIndents(sb, indent + 1);
		sb.append("</Style>\n");

		addIndents(sb, indent);
		sb.append("</Placemark>\n");

		return sb.toString();
	}

	public static void addIndents(StringBuilder sb, int indent) {
		for (int i = 0; i < indent; i++) {
			sb.append('\t');
		}
	}
}
