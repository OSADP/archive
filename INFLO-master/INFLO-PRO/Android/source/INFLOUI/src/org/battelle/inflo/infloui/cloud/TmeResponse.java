/**
 * @file         infloui/cloud/TmeResponse.java
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
 * {@code Parcelable} response used when {@code TmeCloudService} broadcasts it's
 * completion of a request. The {@code TmeResponse} contains the required
 * information about the results of the response.
 * 
 * @author branch
 * 
 */
public class TmeResponse implements Parcelable {

	/**
	 * Required for creating {@code TmeResponse} from {@code Parcelable}.
	 */
	public static Parcelable.Creator<TmeResponse> CREATOR = new Creator<TmeResponse>() {

		@Override
		public TmeResponse[] newArray(int size) {
			return new TmeResponse[size];
		}

		@Override
		public TmeResponse createFromParcel(Parcel source) {

			Gson builder = new Gson();

			try {
				return builder.fromJson(source.readString(), TmeResponse.class);
			} catch (JsonSyntaxException e) {
				e.printStackTrace();
			}
			return null;
		}
	};

	private final boolean successful;
	private final String strBody;
	private final String strErrorMessage;

	/**
	 * Constructor containing the three required pieces of information for the
	 * response.
	 * 
	 * @param successful
	 * @param body
	 * @param errorMessage
	 */
	public TmeResponse(boolean successful, String body, String errorMessage) {
		this.successful = successful;
		this.strBody = body;
		this.strErrorMessage = errorMessage;
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
	 * @return the successful
	 */
	public boolean isSuccessful() {
		return successful;
	}

	/**
	 * @return the body
	 */
	public String getBody() {
		return strBody;
	}

	/**
	 * @return the errorMessage
	 */
	public String getErrorMessage() {
		return strErrorMessage;
	}
}
