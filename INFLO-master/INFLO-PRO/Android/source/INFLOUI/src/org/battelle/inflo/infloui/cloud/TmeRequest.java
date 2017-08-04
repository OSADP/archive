/**
 * @file         infloui/cloud/TmeRequest.java
 * @author       Joshua Branch
 * 
 * @copyright Copyright (c) 2013 Battelle Memorial Institute. All rights reserved.
 * 
 * @par
 * Unauthorized use or duplication may violate state, federal and/or
 * international laws including the Copyright Laws of the United States
 * and of other international jurisdictions.
 * 
 * @par
 * @verbatim
 * Battelle Memorial Institute
 * 505 King Avenue
 * Columbus, Ohio  43201
 * @endverbatim
 * 
 * @brief
 * TBD
 * 
 * @details
 * TBD
 */

package org.battelle.inflo.infloui.cloud;

import android.os.Parcel;
import android.os.Parcelable;

import com.google.gson.Gson;
import com.google.gson.JsonSyntaxException;

/**
 * {@code Parcelable} request used when starting the {@code TmeCloudService}.
 * The {@code TmeRequest} contains the required information to perform the
 * request to the cloud.
 * 
 * @author branch
 * 
 */
public class TmeRequest implements Parcelable {

	/**
	 * Required for creating {@code TmeRequest} from {@code Parcelable}.
	 */
	public static Parcelable.Creator<TmeRequest> CREATOR = new Creator<TmeRequest>() {

		@Override
		public TmeRequest[] newArray(int size) {
			return new TmeRequest[size];
		}

		@Override
		public TmeRequest createFromParcel(Parcel source) {

			Gson builder = new Gson();

			try {
				return builder.fromJson(source.readString(), TmeRequest.class);
			} catch (JsonSyntaxException e) {
				e.printStackTrace();
			}
			return null;
		}
	};

	private final TmeRequestMethod rMethod;
	private final String strUrl;
	private String strBody = "";
	private String strResponseTarget = "";

	/**
	 * Constructor containing the two required pieces of information for the
	 * request.
	 * 
	 * @param method
	 * @param url
	 */
	public TmeRequest(TmeRequestMethod method, String url) {
		this.rMethod = method;
		this.strUrl = url;
	}

	/*
	 * Parcelable methods
	 */
	@Override
	public int describeContents() {
		return 0;
	}

	@Override
	public void writeToParcel(Parcel dest, int flags) {
		Gson builder = new Gson();
		dest.writeString(builder.toJson(this));
	}

	/*
	 * Getters and Setters
	 */
	/**
	 * @return the method
	 */
	public TmeRequestMethod getMethod() {
		return rMethod;
	}

	/**
	 * @return the url
	 */
	public String getUrl() {
		return strUrl;
	}

	/**
	 * @return the body
	 */
	public String getBody() {
		return strBody;
	}

	/**
	 * @param body
	 *            the body to set
	 */
	public void setBody(String body) {
		this.strBody = body;
	}

	/**
	 * @return the responseTarget
	 */
	public String getResponseTarget() {
		return strResponseTarget;
	}

	/**
	 * @param responseTarget
	 *            the responseTarget to set
	 */
	public void setResponseTarget(String responseTarget) {
		this.strResponseTarget = responseTarget;
	}

}
