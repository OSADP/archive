package com.fratis.emergencyresponse;

import org.w3c.dom.Element;

import android.content.Context;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.RadioGroup;

public class XMLControls 
{
	public class xmlEditText extends EditText
	{
		public Element xmlElement = null;
		
		public xmlEditText(Context context) 
		{
			super(context);
		}
	}
	
	public class xmlButton extends Button
	{
		public Element xmlElement = null;
		
		public xmlButton(Context context) 
		{
			super(context);
		}
	}
	
	public class xmlRadioGroup extends RadioGroup
	{
		public Element xmlElement = null;
		
		public xmlRadioGroup(Context context) 
		{
			super(context);
		}
	}
	
	public class xmlCheckBox extends CheckBox
	{
		public Element xmlElement = null;
		
		public xmlCheckBox(Context context) 
		{
			super(context);
		}
	}
}