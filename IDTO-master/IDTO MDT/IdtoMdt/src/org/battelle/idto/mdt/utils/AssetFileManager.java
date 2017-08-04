package org.battelle.idto.mdt.utils;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.FileReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import android.content.Context;
import android.content.res.AssetManager;
import android.os.Environment;

import com.google.gson.*;

public class AssetFileManager {
	
	private static final Logger mLogger = LoggerFactory.getLogger(AssetFileManager.class); 
	
	public static void CopyAssets(Context context) {
		
        AssetManager assetManager = context.getAssets();
        String[] files = null;
        try {
            files = assetManager.list("files");
        } catch (IOException e) {
            mLogger.error(e.getMessage());
        }

        File idtoOutputDirectory = new File(Environment.getExternalStorageDirectory().toString() +"/idto/");
        
        idtoOutputDirectory.mkdirs();
        
        for(String filename : files) {
        	System.out.println("File name => "+filename);
            InputStream in = null;
            OutputStream out = null;
            try {
            	
              in = assetManager.open("files/"+filename);   // if files resides inside the "Files" directory itself
              
              File outputFile = new File(Environment.getExternalStorageDirectory().toString() +"/idto/" + filename);
              
              //if(!outputFile.exists())
              //{
	              out = new FileOutputStream(outputFile);
	              copyFile(in, out);
	              in.close();
	              in = null;
	              out.flush();
	              out.close();
	              out = null;
              //}
            } catch(Exception e) {
                mLogger.error(e.getMessage());
            }
        }
    }
	
	public static<T> T getConfigurationFileContents(String filename, Class<T> classOfT)
	{
		File inputFile = new File(Environment.getExternalStorageDirectory().toString() +"/idto/" + filename);
        
		boolean fileExists = inputFile.exists();
		
        if(fileExists)
        {
        	FileReader fileReader;
			try {
				fileReader = new FileReader(inputFile);
				Gson gson = new Gson();
	    		T obj = gson.fromJson(fileReader, classOfT);
	    		return obj;
	    		
			} catch (FileNotFoundException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
				return null;
			}
        	
        }
        else
        {
        	return null;
        }
		
	}
	
    private static void copyFile(InputStream in, OutputStream out) throws IOException {
        byte[] buffer = new byte[1024];
        int read;
        while((read = in.read(buffer)) != -1){
          out.write(buffer, 0, read);
        }
    }
}
