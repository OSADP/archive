package org.battelle.inflo.infloui.cloud;

import java.util.Map;

import android.os.Parcel;
import android.os.Parcelable;

import com.google.gson.Gson;
import com.google.gson.JsonSyntaxException;

public class TmeCloudEndpointStatistics implements Parcelable {

	/**
	 * Required for creating {@code TmeResponse} from {@code Parcelable}.
	 */
	public static Parcelable.Creator<TmeCloudEndpointStatistics> CREATOR = new Creator<TmeCloudEndpointStatistics>() {

		@Override
		public TmeCloudEndpointStatistics[] newArray(int size) {
			return new TmeCloudEndpointStatistics[size];
		}

		@Override
		public TmeCloudEndpointStatistics createFromParcel(Parcel source) {

			Gson builder = new Gson();

			try {
				return builder.fromJson(source.readString(),
						TmeCloudEndpointStatistics.class);
			} catch (JsonSyntaxException e) {
				e.printStackTrace();
			}
			return null;
		}
	};

	public static String getBaseUrl(String url) {

		return url.indexOf('?') == -1 ? url : url
				.substring(0, url.indexOf('?'));
		/*
		 * URL tempUrl = null; try { tempUrl = new URL(url); } catch
		 * (MalformedURLException e) { } return tempUrl != null ?
		 * tempUrl.getHost() : url;
		 */

	}

	public static TmeCloudEndpointStatistics newStatistics(String url) {
		return new TmeCloudEndpointStatistics(url, TmeCloudState.Unknown, 0, 0,
				0, 0);
	}

	public static TmeCloudEndpointStatistics addSuccess(
			TmeCloudEndpointStatistics original, double latency) {
		return new TmeCloudEndpointStatistics(original.getUrl(),
				TmeCloudState.Available, original.getSuccessCount() + 1,
				original.getErrorCount(), latency, original.getTotalLatency()
						+ latency);
	}

	public static TmeCloudEndpointStatistics addError(
			TmeCloudEndpointStatistics original, double latency) {
		return new TmeCloudEndpointStatistics(original.getUrl(),
				TmeCloudState.Unavailable, original.getSuccessCount(),
				original.getErrorCount() + 1, latency,
				original.getTotalLatency() + latency);
	}

	public static void addSuccess(Map<String, TmeCloudEndpointStatistics> map,
			String url, double latency) {
		TmeCloudEndpointStatistics oldValue = null;

		oldValue = map.get(getBaseUrl(url));
		if (oldValue == null)
			oldValue = TmeCloudEndpointStatistics
					.newStatistics(getBaseUrl(url));

		TmeCloudEndpointStatistics newValue = TmeCloudEndpointStatistics
				.addSuccess(oldValue, latency);

		map.put(oldValue.getUrl(), newValue);
	}

	public static void addError(Map<String, TmeCloudEndpointStatistics> map,
			String url, double latency) {
		TmeCloudEndpointStatistics oldValue = null;

		oldValue = map.get(getBaseUrl(url));
		if (oldValue == null)
			oldValue = TmeCloudEndpointStatistics
					.newStatistics(getBaseUrl(url));

		TmeCloudEndpointStatistics newValue = TmeCloudEndpointStatistics
				.addError(oldValue, latency);

		map.put(oldValue.getUrl(), newValue);
	}

	private final String url;
	private final TmeCloudState state;
	private final int successCount;
	private final int errorCount;
	private final double currentLatency;
	private final double totalLatency;

	private TmeCloudEndpointStatistics(String url, TmeCloudState state,
			int success, int error, double currentLatency, double totalLatency) {

		this.url = url;
		this.state = state;
		this.successCount = success;
		this.errorCount = error;
		this.currentLatency = currentLatency;
		this.totalLatency = totalLatency;
	}

	@Override
	public int describeContents() {
		return 0;
	}

	@Override
	public void writeToParcel(Parcel dest, int flags) {
		Gson builder = new Gson();
		dest.writeString(builder.toJson(this));
	}

	public String getUrl() {
		return url;
	}

	public TmeCloudState getState() {
		return state;
	}

	public int getSuccessCount() {
		return successCount;
	}

	public int getErrorCount() {
		return errorCount;
	}

	public double getCurrentLatency() {
		return currentLatency;
	}

	public double getTotalLatency() {
		return totalLatency;
	}

}
