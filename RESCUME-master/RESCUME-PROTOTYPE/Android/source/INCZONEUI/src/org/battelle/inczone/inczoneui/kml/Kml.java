package org.battelle.inczone.inczoneui.kml;

import java.util.ArrayList;
import java.util.List;

public class Kml {

	private final String name;
	private final List<LineString> lineStrings = new ArrayList<LineString>();

	public Kml(String name) {
		this.name = name + ".kml";
	}

	public void addLineString(LineString ls) {
		lineStrings.add(ls);
	}

	public String getXml() {
		StringBuilder sb = new StringBuilder();

		sb.append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
		sb.append("<kml xmlns=\"http://www.opengis.net/kml/2.2\" xmlns:gx=\"http://www.google.com/kml/ext/2.2\" xmlns:kml=\"http://www.opengis.net/kml/2.2\" xmlns:atom=\"http://www.w3.org/2005/Atom\">\n");
		sb.append("<Document>\n");

		sb.append("\t<name>");
		sb.append(name);
		sb.append("</name>\n");

		for (LineString ls : lineStrings) {
			sb.append(ls.getXml(1));
		}

		sb.append("</Document>\n");
		sb.append("</kml>\n");

		return sb.toString();
	}

	public String getName() {
		return name;
	}
}
