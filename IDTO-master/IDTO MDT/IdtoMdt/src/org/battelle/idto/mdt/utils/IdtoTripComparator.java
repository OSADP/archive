package org.battelle.idto.mdt.utils;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Comparator;
import java.util.Date;
import java.util.Locale;
import java.util.TimeZone;

import org.battelle.idto.ws.otp.data.IdtoTrip;
import org.battelle.idto.ws.otp.data.Step;

public class IdtoTripComparator implements Comparator<IdtoTrip>{

	@Override
	public int compare(IdtoTrip lhs, IdtoTrip rhs) {
		SimpleDateFormat inputFormatter = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.ENGLISH);
		inputFormatter.setTimeZone(TimeZone.getTimeZone("UTC"));

		try {
			Step lhslastStep = lhs.getSteps().get(lhs.getSteps().size()-1);


			Date lhsdate = inputFormatter.parse(lhslastStep.getStartDate());


			Step rhslastStep = rhs.getSteps().get(rhs.getSteps().size()-1);
			Date rhsdate = inputFormatter.parse(rhslastStep.getStartDate());
		
			return lhsdate.compareTo(rhsdate);
		} catch (ParseException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		return 0;
	}
}
